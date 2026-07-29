# QM-AI 数据库升级流程 — Agent 执行手册

> 本文档供 Agent 直接执行。每一步都有明确的验证命令，执行失败时按异常处理流程操作。

## 升级脚本版本说明

| 脚本 | 状态 | 说明 |
|------|------|------|
| `migration_v3_ef_schema_sync.sql` | **当前推荐** | 基于 EF 模型完整比对生成，覆盖 2091 行，幂等可重复执行 |
| `unified_schema_upgrade.sql` | 已废弃 | 早期增量脚本，部分功能已被 v3 覆盖 |
| `migration_v2_*.sql` / `migration_s*_*.sql` | 已废弃 | 均已合并到 init.sql 或 v3 |

**推荐升级路径**: 增量升级统一使用 `migration_v3_ef_schema_sync.sql`，它从 EF 模型出发，逐一比对 init.sql 中的差异，自动补齐缺失的表、列、索引，并修正类型和命名。

## 前置条件

| 检查项 | 验证命令 | 通过标准 |
|--------|---------|---------|
| MySQL 可达 | `mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 -e "SELECT 1"` | 返回 `1` |
| 数据库存在 | `mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 -e "SHOW DATABASES LIKE 'qmai'"` | 出现 `qmai` |
| 磁盘空间 | `df -h $MYSQL_DATA_DIR` | 剩余 > 1GB |

## 升级策略选择

根据远程数据库的**当前状态**选择对应流程：

| 场景 | 判断条件 | 使用流程 |
|------|---------|---------|
| **全新安装** | 数据库为空或只有默认 schema | → 流程 A |
| **增量升级** | 已有 qmai 数据库，数据需保留 | → 流程 B |
| **回滚/重做** | 升级失败或需要重置 | → 流程 C |

---

## 流程 A：全新安装（数据库不存在或需重建）

### 步骤 A1 — 重建数据库

```bash
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 < database/init.sql
```

**验证**:
```bash
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 -e "USE qmai; SHOW TABLES;" | wc -l
# 期望: 50+ 张表
```

### 步骤 A2 — 执行种子数据

```bash
# 按依赖顺序执行，每个步骤独立验证
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/seed_suppliers.sql
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/seed_supplier_scores.sql
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/seed_iqc_receipts.sql
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/seed_iqc_receipts_enhanced.sql
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/seed_tools.sql
```

**验证**:
```bash
# 检查各表记录数
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT 'suppliers' AS t, COUNT(*) AS c FROM suppliers
  UNION ALL SELECT 'supplier_scores', COUNT(*) FROM supplier_scores
  UNION ALL SELECT 'iqc_receipts', COUNT(*) FROM iqc_receipts
  UNION ALL SELECT 'iqc_inspections', COUNT(*) FROM iqc_inspections
  UNION ALL SELECT 'iqc_anomalies', COUNT(*) FROM iqc_anomalies
  UNION ALL SELECT 'tools', COUNT(*) FROM tools;
"
```

### 步骤 A3 — 启动后端（EF Core 种子数据守卫）

```bash
docker compose up -d backend
# 等待健康检查通过
docker compose ps backend | grep healthy
```

**验证**:
```bash
# 检查 Roles 表（DbInitializer 会自动填充）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "SELECT name FROM roles;"
# 期望: Administrator, Operator, Inspector, Engineer
```

---

## 流程 B：增量升级（保留已有数据）

> **核心原则**: `migration_v3_ef_schema_sync.sql` 是幂等的，可安全重复执行。
> 它基于 EF 模型完整比对 init.sql 生成，覆盖所有表结构的差异对齐。

### 步骤 B1 — 备份（必须）

```bash
mysqldump -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai > backup/qmai_$(date +%Y%m%d_%H%M%S).sql
```

**验证**:
```bash
ls -lh backup/qmai_*.sql | tail -1
# 文件不应为空
```

### 步骤 B2 — 执行 EF 模型对齐迁移脚本

```bash
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/migration_v3_ef_schema_sync.sql
```

**验证**:
```bash
# 检查末尾输出
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/migration_v3_ef_schema_sync.sql 2>&1 | grep "Migration Complete"
```

### 步骤 B3 — 验证表结构对齐

```bash
# 关键检查 1：无 ENUM 列（C# Model 用 VARCHAR）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai'
    AND COLUMN_TYPE LIKE 'enum%'
    AND TABLE_NAME NOT IN ('role_permissions');
"
# 期望: 空结果（无 ENUM 列）
```

```bash
# 关键检查 2：equipment 表存在（非 equipments）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "SHOW TABLES LIKE 'equipment%';"
# 期望: equipment, equipment_param_mappings, equipment_status_history, equipment_quality_correlation
```

```bash
# 关键检查 3：缺失表已创建（complaint_events, packaging_confirmations, ipqc_ai_risk_scores）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT table_name FROM information_schema.tables
  WHERE table_schema = 'qmai' AND table_name IN ('complaint_events', 'packaging_confirmations', 'ipqc_ai_risk_scores');
"
# 期望: 3 张表全部存在
```

```bash
# 关键检查 4：org_id 列已添加到关键表
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT TABLE_NAME, COLUMN_NAME
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai' AND COLUMN_NAME = 'org_id'
  ORDER BY TABLE_NAME;
"
# 期望: 25+ 张表有 org_id 列
```

```bash
# 关键检查 5：重命名列已生效（routing_header_id, patrol_plan_id, defect_no, complaint_no）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT TABLE_NAME, COLUMN_NAME FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai' AND COLUMN_NAME IN ('routing_header_id', 'patrol_plan_id', 'defect_no', 'complaint_no');
"
# 期望: 4 列全部存在
```

```bash
# 关键检查 6：旧列已删除（supply_category, manufacturer, equipment_id from tools）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT TABLE_NAME, COLUMN_NAME FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai' AND COLUMN_NAME IN ('supply_category', 'manufacturer', 'equipment_id');
  -- equipment_id 只应在 equipment_param_mappings 中存在，不应在 tools 中
  AND NOT (TABLE_NAME = 'tools' AND COLUMN_NAME = 'equipment_id');
"
# 期望: 空结果（这些列不应存在）
```

```bash
# 关键检查 7：D8 报告字段已对齐（d0_description, d1_team, ..., d8_thanks, current_discipline）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT COLUMN_NAME FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'd8_reports'
  AND COLUMN_NAME LIKE 'd%_description' OR COLUMN_NAME LIKE 'd%_team' OR COLUMN_NAME LIKE 'd%_content'
  OR COLUMN_NAME LIKE 'd%_cause' OR COLUMN_NAME LIKE 'd%_actions' OR COLUMN_NAME LIKE 'd%_verification'
  OR COLUMN_NAME LIKE 'd%_preventive' OR COLUMN_NAME LIKE 'd%_thanks' OR COLUMN_NAME = 'current_discipline'
  ORDER BY COLUMN_NAME;
"
# 期望: 12+ 个 D 系列字段全部存在
```

```bash
# 关键检查 8：设备新字段已添加（production_line, workshop, workshop_id, line_id, has_mqtt_connection, mqtt_topic_prefix）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT COLUMN_NAME FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment'
  AND COLUMN_NAME IN ('production_line', 'workshop', 'workshop_id', 'line_id', 'has_mqtt_connection', 'mqtt_topic_prefix');
"
# 期望: 6 个字段全部存在
```

### 步骤 B4 — 重启后端验证

```bash
docker compose restart backend
docker compose ps backend | grep healthy
```

---

## 流程 C：回滚/重做

### 步骤 C1 — 停止后端

```bash
docker compose stop backend
```

### 步骤 C2 — 从备份恢复

```bash
# 方式 1: 恢复最近一次备份
LATEST_BACKUP=$(ls -t backup/qmai_*.sql | head -1)
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 < "$LATEST_BACKUP"

# 方式 2: 重新走全新安装流程
# mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 -e "DROP DATABASE qmai;"
# 然后执行 流程 A
```

### 步骤 C3 — 验证回滚成功

```bash
# 对比回滚后的表数量
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 -e "SHOW TABLES;" | wc -l
```

---

## 异常处理

| 错误 | 原因 | 处理 |
|------|------|------|
| `Table 'equipments' doesn't exist` | 旧数据库仍用 `equipments` | 先执行 `ALTER TABLE equipments RENAME TO equipment` |
| `Can't DROP COLUMN; check that it exists` | 列已被删除 | 升级脚本已用 `IF EXISTS` 处理，忽略即可 |
| `Duplicate entry for key 'PRIMARY'` | 种子数据重复插入 | 先清空目标表再重新执行 seed 脚本 |
| `Foreign key constraint fails` | FK 引用的表不存在 | 按依赖顺序执行（先 parent 表再 child 表） |
| `Column 'xxx' already exists` | 幂等执行时的正常现象 | 忽略，升级脚本已处理 |
| `Unknown column 'plan_id' in 'ipqc_patrols'` | 列已被重命名为 `patrol_plan_id` | v3 脚本已处理重命名，若报错说明脚本未执行，重新执行 v3 |
| `Unknown column 'routing_id' in 'routing_steps'` | 列已被重命名为 `routing_header_id` | v3 脚本已处理重命名，若报错说明脚本未执行，重新执行 v3 |
| `Can't DROP FOREIGN KEY 'fk_xxx'` | FK 名称不匹配 | v3 脚本通过 `information_schema.KEY_COLUMN_USAGE` 动态获取 FK 名称，忽略此错误即可 |
| `Duplicate column name 'xxx'` | 列已存在 | v3 脚本使用 `CALL sp_column_exists` 检查，忽略即可 |
| `Table 'xxx' already exists` | 新表已创建 | v3 脚本使用 `DROP TABLE IF EXISTS` 后 `CREATE TABLE`，忽略即可 |

---

## v3 迁移脚本变更摘要

> 执行前必读，了解本次迁移覆盖的表范围和变更类型。

### 新增表（3 张）

| 表名 | 来源模块 | 说明 |
|------|---------|------|
| `complaint_events` | M09 | 客诉时间线事件表 |
| `packaging_confirmations` | M05 | 包装确认表 |
| `ipqc_ai_risk_scores` | M04 | IPQC AI 风险评分历史表 |

### 列重命名（6 处）

| 表 | 旧列名 | 新列名 | 说明 |
|----|--------|--------|------|
| `routing_steps` | `routing_id` | `routing_header_id` | 关联 routing_headers 表 |
| `ipqc_patrols` | `plan_id` | `patrol_plan_id` | 关联 ipqc_patrol_plans 表 |
| `defects` | `defect_code` | `defect_no` | 缺陷编号 |
| `complaints` | `complaint_code` | `complaint_no` | 客诉编号 |
| `equipment_status_history` | `status` | `signal` | 设备信号状态 |
| `equipment_status_history` | `started_at` | `recorded_at` | 记录时间 |

### 类型变更（ENUM → VARCHAR 等）

| 表 | 列 | 旧类型 | 新类型 | 说明 |
|----|----|--------|--------|------|
| `iqc_inspection_items` | `result` | ENUM | VARCHAR(10) | 检验结果 |
| `ipqc_patrols` | `status` | ENUM | VARCHAR(20) | 巡检状态 |
| `ipqc_patrol_items` | `result` | ENUM | VARCHAR(10) | 巡检结果 |
| `ipqc_first_piece_items` | `result` | ENUM | VARCHAR(10) | 首件结果 |
| `fqc_inspection_items` | `result` | ENUM | VARCHAR(10) | FQC 结果 |
| `iqc_anomalies` | `anomaly_type` | ENUM | VARCHAR(20) | 异常类型 |
| `iqc_anomalies` | `handler_dept` | ENUM | VARCHAR(20) | 处理部门 |
| `spc_analysis_results` | `analysis_type` | ENUM | VARCHAR(20) | 分析类型 |
| `ipqc_closure_status` | `status` | ENUM | VARCHAR(10) | 关单状态 |
| `dynamic_params` | `precision` | DECIMAL(10,2) | INT | 精度位数 |

### 删除列（NotMapped 或不再需要）

| 表 | 删除的列 | 原因 |
|----|---------|------|
| `suppliers` | `supply_category` | `[NotMapped]` |
| `equipment` | `manufacturer`, `installation_date` | EF 模型无此字段 |
| `tools` | `status`, `equipment_id` | EF 模型无此字段 |
| `complaints` | `product_id`, `batch_no`, `complaint_date`, `org_id` | EF 模型无此字段 |
| `equipment_quality_correlation` | `conclusion` | EF 模型无此字段 |
| `ipqc_patrols` | `patrol_time`, `remark`, `inspector` | 已重命名/替换 |
| `ipqc_closure_status` | `work_order`, `spc_result` | 已重命名为 `work_order_id` |
| `inspection_standards` | `sampling_method`, `aql`, `inspection_level`, `items` | EF 模型无此字段 |
| `users` | `phone`, `last_login_at` | EF 模型无此字段 |
| `permissions` | `description` | EF 模型无此字段 |

### 新增列（按模块统计）

| 模块 | 新增列数 | 主要新增列 |
|------|---------|-----------|
| M15 (Auth) | ~5 | `users.org_id`, `users.created_by`, `roles.created_at` |
| M02 (基础) | ~15 | `equipment` 的 `production_line/workshop/workshop_id/line_id/has_mqtt_connection/mqtt_topic_prefix` 等 |
| M02.5 (动态参数) | ~5 | `dynamic_params.precision` 类型, `param_groups` 的 `description/sort_order/created_by` |
| M03 (IQC) | ~8 | `iqc_anomalies` 的 `failed_item_ids/org_id`, `iqc_inspections` 的 `sampling_level/org_id` |
| M04 (IPQC) | ~15 | `ipqc_first_pieces` 的 `work_order_id/operator_id/inspector_id/allowed_to_produce` 等 |
| M05 (FQC/OQC) | ~5 | `product_batches` 的 `org_id/updated_at` 等 |
| M07 (CAPA) | ~3 | `capa` 的 `org_id/updated_at` 等 |
| M09 (客诉) | ~4 | `complaints` 的 `customer_id/acknowledged_at/five_w2h_json/updated_at`, `d8_reports` 的 D0-D8 字段 |
| M11 (设备) | ~4 | `equipment_status_history` 的 `signal/signal_data/recorded_at` 等 |
| M12 (文档) | ~4 | `documents` 的 `approved_by_str/approved_at/expires_at/created_at/updated_at` |
| M13 (审核) | ~5 | `audit_findings` 的 `requirement_ref/responsible_user_id_str/verified_by_str/verified_at` 等 |

---

## 文件清单

| 文件 | 用途 | 执行时机 |
|------|------|---------|
| `init.sql` | 全量建表 + 索引（基于 init 版本） | 全新安装时 |
| `migration_v3_ef_schema_sync.sql` | **EF 模型对齐迁移**：增表/增列/改类型/重命名/加索引 | 增量升级首选 |
| `unified_schema_upgrade.sql` | 早期增量补齐脚本（部分已废弃） | 仅在 v3 脚本执行前使用 |
| `migration_v2_enterprise_hierarchy.sql` | 企业层级表 + org_id | 已废弃（已合并到 init.sql） |
| `migration_s3_anomaly_enhance.sql` | IQC 异常表增强 | 已废弃（已合并到 init.sql） |
| `migration_s6_spc.sql` | SPC 统计分析表 | 已废弃（已合并到 init.sql） |
| `migration_s7_defect_capa.sql` | 缺陷/CAPA 表 | 已废弃（已合并到 init.sql） |
| `migration_m02_schema_sync.sql` | M02 基础数据对齐 | 已废弃（已合并到 init.sql） |
| `migration_m05_schema_sync.sql` | M05 FQC/OQC 重构 | 已废弃（已合并到 init.sql） |
| `migration_s3_supplier_scores_enhance.sql` | 供应商评分增强 | 已废弃（已合并到 init.sql） |
| `seed_suppliers.sql` | 20 家供应商 | 全新安装或手动补数据 |
| `seed_supplier_scores.sql` | 20 条评分记录 | 供应商 seed 之后 |
| `seed_iqc_receipts.sql` | 10 条 IQC 记录 | 全新安装或手动补数据 |
| `seed_iqc_receipts_enhanced.sql` | 15 条增强 IQC 记录 | IQC seed 之后 |
| `seed_tools.sql` | 25 条刀具数据 | 全新安装或手动补数据 |

---

## 快速决策树

```
需要升级数据库？
├─ 数据库是空的？
│  └─ YES → 流程 A（init.sql → seed → 启动后端）
├─ 有数据需要保留？
│  └─ YES → 流程 B（备份 → migration_v3_ef_schema_sync.sql → 验证 → 重启）
├─ 升级失败了？
│  └─ YES → 流程 C（停止后端 → 恢复备份 → 重做）
└─ 只是验证状态？
   └─ 执行 B3 中的 8 个验证查询
```

---

## 注意事项

1. **所有脚本字符集**: UTF-8 / utf8mb4，执行前确认 `SET NAMES utf8mb4;`
2. **时区**: MySQL 默认 UTC，C# 使用 `DateTime.UtcNow`，无需转换
3. **幂等性**: `migration_v3_ef_schema_sync.sql` 可安全重复执行，使用 `CALL sp_column_exists` + `IF EXISTS` 保护
4. **FK 约束**: 列重命名（如 `routing_id`→`routing_header_id`）前会自动 DROP FK，重命名后重建 FK
5. **后端重启**: 升级后必须 `docker compose restart backend` 使 EF Core 重新读取 schema
6. **DateOnly 映射**: C# `DateOnly` 类型在 MySQL 中映射为 `DATE`，非 `DATETIME`
7. **JSON → TEXT**: 部分 JSON 列（如 `capa_root_causes.content`）在 EF 中为 `string`，迁移为 `TEXT` 而非 `JSON`
8. **种子数据顺序**: 全新安装时先执行 seed 脚本再启动后端，后端 DbInitializer 会通过 `if (!hasRoles)` 守卫避免重复填充

# QM-AI 数据库升级流程 — Agent 执行手册

> 本文档供 Agent 直接执行。每一步都有明确的验证命令，执行失败时按异常处理流程操作。

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

> **核心原则**: `unified_schema_upgrade.sql` 是幂等的，可安全重复执行。

### 步骤 B1 — 备份（必须）

```bash
mysqldump -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai > backup/qmai_$(date +%Y%m%d_%H%M%S).sql
```

**验证**:
```bash
ls -lh backup/qmai_*.sql | tail -1
# 文件不应为空
```

### 步骤 B2 — 执行统一升级脚本

```bash
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/unified_schema_upgrade.sql
```

**验证**:
```bash
# 检查返回值
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai < database/unified_schema_upgrade.sql 2>&1 | grep "completed successfully"
```

### 步骤 B3 — 验证表结构对齐

```bash
# 关键检查：无 ENUM 列（C# Model 用 VARCHAR）
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
# 关键检查：equipment 表存在（非 equipments）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "SHOW TABLES LIKE 'equipment%';"
# 期望: equipment, equipment_param_mappings, equipment_status_history, equipment_quality_correlation
```

```bash
# 关键检查：product_batches 表存在（非 batches）
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "SHOW TABLES LIKE 'product_batches';"
# 期望: 有记录
```

```bash
# 关键检查：org_id 列已添加到关键表
mysql -u root -p"$MYSQL_ROOT_PASSWORD" -h $DB_HOST -P 3306 qmai -e "
  SELECT TABLE_NAME, COLUMN_NAME
  FROM information_schema.COLUMNS
  WHERE TABLE_SCHEMA = 'qmai' AND COLUMN_NAME = 'org_id'
  ORDER BY TABLE_NAME;
"
# 期望: 20+ 张表有 org_id 列
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

---

## 文件清单

| 文件 | 用途 | 执行时机 |
|------|------|---------|
| `init.sql` | 全量建表 + 索引 | 全新安装时 |
| `unified_schema_upgrade.sql` | 增量补齐（幂等） | 任意阶段升级 |
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
│  └─ YES → 流程 B（备份 → unified_schema_upgrade.sql → 验证 → 重启）
├─ 升级失败了？
│  └─ YES → 流程 C（停止后端 → 恢复备份 → 重做）
└─ 只是验证状态？
   └─ 执行 B3 中的 4 个验证查询
```

---

## 注意事项

1. **所有脚本字符集**: UTF-8 / utf8mb4，执行前确认 `SET NAMES utf8mb4;`
2. **时区**: MySQL 默认 UTC，C# 使用 `DateTime.UtcNow`，无需转换
3. **幂等性**: `unified_schema_upgrade.sql` 可安全重复执行，使用 `IF NOT EXISTS` / `IF EXISTS` 保护
4. **FK 约束**: 升级脚本先 `DROP` 旧 FK 再 `ADD` 新 FK，避免约束冲突
5. **后端重启**: 升级后必须 `docker compose restart backend` 使 EF Core 重新读取 schema

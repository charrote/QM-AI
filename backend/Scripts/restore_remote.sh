#!/bin/bash
# ============================================
# QM-AI 数据库同步脚本 (一键恢复)
# ============================================
# 用途：将远程 MySQL 数据库同步到与当前 .NET 项目一致的表结构 + 数据
# 适用场景：
#   1. 远程数据库表结构与本地不一致时（Migration 变更后）
#   2. 远程数据库需要全量恢复时
# 前置条件：
#   - 宿主机已安装 .NET SDK 9.0+
#   - 远程 MySQL 以 Docker 运行，容器名 qm-ai-mysql
#   - 备份文件 backup_qmai_full.sql 在宿主机可访问
#   - 脚本与 .csproj 同目录执行
#
# 用法：./restore_remote.sh [备份文件路径]
#   示例：./restore_remote.sh /data/backups/backup_20260727.sql
# ============================================

set -euo pipefail

# ---------- 配置区 ----------
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/../.." && pwd)"

MYSQL_USER="${MYSQL_USER:-root}"
MYSQL_PASS="${MYSQL_PASS:-qm-ai-2024}"
MYSQL_DB="${MYSQL_DB:-qmai}"
MYSQL_CONTAINER="${MYSQL_CONTAINER:-qm-ai-mysql}"

BACKUP_FILE="${1:-$SCRIPT_DIR/../../backup_qmai_full.sql}"

echo "============================================"
echo "  QM-AI 数据库同步"
echo "  项目目录: $PROJECT_DIR"
echo "  备份文件: $BACKUP_FILE"
echo "============================================"

# ---------- 检查前置条件 ----------
if ! command -v dotnet &>/dev/null; then
    echo "[ERROR] dotnet CLI 未安装，请安装 .NET SDK 9.0+"
    exit 1
fi

if ! command -v docker &>/dev/null; then
    echo "[ERROR] docker 未安装"
    exit 1
fi

if ! docker ps --format '{{.Names}}' | grep -q "^${MYSQL_CONTAINER}$"; then
    echo "[ERROR] MySQL 容器 '${MYSQL_CONTAINER}' 未运行"
    exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
    echo "[ERROR] 备份文件不存在: $BACKUP_FILE"
    exit 1
fi

echo "[OK] 前置条件检查通过"
echo ""

# ---------- Step 1: 清空数据库 ----------
echo "=== Step 1/5: 清空数据库 ==="
TABLES=$(docker exec $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" -N -e "SHOW TABLES;" "$MYSQL_DB" 2>/dev/null || true)

if [ -n "$TABLES" ]; then
    # 临时关闭外键检查
    docker exec $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" "$MYSQL_DB" \
        -e "SET FOREIGN_KEY_CHECKS = 0;" 2>/dev/null

    for t in $TABLES; do
        docker exec $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" "$MYSQL_DB" \
            -e "DROP TABLE IF EXISTS \`${t}\`;" 2>/dev/null
        echo "  DROP TABLE \`${t}\`"
    done

    docker exec $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" "$MYSQL_DB" \
        -e "SET FOREIGN_KEY_CHECKS = 1;" 2>/dev/null
else
    echo "  数据库为空，无需清空"
fi

# ---------- Step 2: 执行 EF Migration 重建表结构 ----------
echo ""
echo "=== Step 2/5: 执行 EF Migration ==="
cd "$PROJECT_DIR/backend"
dotnet ef database update 2>&1
echo "[OK] Migration 完成"

# ---------- Step 3: 恢复备份数据 ----------
echo ""
echo "=== Step 3/5: 恢复备份数据 ==="
docker exec -i $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" "$MYSQL_DB" \
    --init-command="SET NAMES utf8mb4; SET time_zone = '+08:00';" \
    < "$BACKUP_FILE" 2>/dev/null
echo "[OK] 备份数据恢复完成"

# ---------- Step 4: 修复零日期 ----------
echo ""
echo "=== Step 4/5: 修复零日期 (0000-00-00) ==="

# 内联修复 SQL，无需外部文件
docker exec -i $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" "$MYSQL_DB" <<'FIXSQL' 2>/dev/null
SET SQL_MODE = 'NO_ENGINE_SUBSTITUTION';

-- 所有包含 created_at / updated_at 的表
UPDATE inspection_items SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE inspection_plans SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE inspection_plan_items SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_receipts SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_inspections SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_inspection_items SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_anomalies SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE products SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE organizations SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE users SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE roles SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE permissions SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE equipment SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE tools SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE suppliers SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE customers SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE defect_codes SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE processes SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE closure_rules SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE dynamic_params SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE param_groups SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE routing_headers SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE routing_steps SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE routings SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE sys_dict_items SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE sys_dict_types SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE supplier_scores SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE inspection_standards SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE spc_control_charts SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE spc_alert_rules SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
UPDATE boms SET created_at = NOW(), updated_at = NOW() WHERE created_at = '0000-00-00 00:00:00';
FIXSQL

echo "[OK] 零日期修复完成"

# ---------- Step 5: 验证 ----------
echo ""
echo "=== Step 5/5: 验证 ==="
docker exec $MYSQL_CONTAINER mysql -u"$MYSQL_USER" -p"$MYSQL_PASS" "$MYSQL_DB" -e "
SELECT 'inspection_items'   AS tbl, COUNT(*) AS cnt FROM inspection_items
UNION ALL SELECT 'inspection_plans'    , COUNT(*) FROM inspection_plans
UNION ALL SELECT 'iqc_receipts'        , COUNT(*) FROM iqc_receipts
UNION ALL SELECT 'iqc_inspections'     , COUNT(*) FROM iqc_inspections
UNION ALL SELECT 'products'            , COUNT(*) FROM products
UNION ALL SELECT 'users'               , COUNT(*) FROM users
UNION ALL SELECT 'roles'               , COUNT(*) FROM roles
UNION ALL SELECT 'organizations'       , COUNT(*) FROM organizations
UNION ALL SELECT 'equipment'           , COUNT(*) FROM equipment
UNION ALL SELECT 'suppliers'           , COUNT(*) FROM suppliers
UNION ALL SELECT 'sys_dict_types'      , COUNT(*) FROM sys_dict_types
UNION ALL SELECT 'tools'               , COUNT(*) FROM tools;
" 2>/dev/null

echo ""
echo "============================================"
echo "  数据库同步完成！"
echo "  下一步：重启后端服务 (dotnet run 或 systemctl restart)"
echo "============================================"

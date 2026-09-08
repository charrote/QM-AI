-- ============================================================================
-- QM-AI SPC v2 — 属性图(P/C/U) + Xbar-S 标准差 支持
-- 变更：
--   1. spc_data_points 新增 subgroup_std_dev 列（Xbar-S 图的标准差 S）
--   2. spc_control_charts.chart_type 扩到 VARCHAR(50)（容纳 P/C/U，若 v3 已扩则跳过）
-- 幂等可重复执行。
-- 执行：cat database/migration_spc_v2_pcu.sql | docker exec -i qm-ai-mysql mysql -u root -p'qmai_root_2024' qmai
-- ============================================================================

DROP PROCEDURE IF EXISTS spc_v2_migrate;
DELIMITER $$
CREATE PROCEDURE spc_v2_migrate()
BEGIN
    DECLARE v_exists TINYINT;
    DECLARE v_coltype VARCHAR(64);

    -- 1. spc_data_points.subgroup_std_dev
    SELECT COUNT(*) INTO v_exists FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'spc_data_points' AND COLUMN_NAME = 'subgroup_std_dev';
    IF v_exists = 0 THEN
        ALTER TABLE spc_data_points ADD COLUMN subgroup_std_dev DECIMAL(15,6) COMMENT 'S (Xbar-S 组内标准差)';
    END IF;

    -- 2. spc_control_charts.chart_type 宽度（v3 已扩到 50，此处兜底）
    SELECT COLUMN_TYPE INTO v_coltype FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'spc_control_charts' AND COLUMN_NAME = 'chart_type';
    SET v_coltype = COALESCE(v_coltype, '');
    IF v_coltype = 'varchar(10)' THEN
        ALTER TABLE spc_control_charts MODIFY COLUMN chart_type VARCHAR(50) NOT NULL DEFAULT 'Xbar_R';
    END IF;
END$$
DELIMITER ;

CALL spc_v2_migrate();
DROP PROCEDURE IF EXISTS spc_v2_migrate;

-- 验证
SELECT 'spc_data_points.subgroup_std_dev' AS check_item,
       IFNULL((SELECT COLUMN_TYPE FROM information_schema.COLUMNS
               WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='spc_data_points' AND COLUMN_NAME='subgroup_std_dev'), 'MISSING') AS column_type;
SELECT 'spc_control_charts.chart_type' AS check_item,
       IFNULL((SELECT COLUMN_TYPE FROM information_schema.COLUMNS
               WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='spc_control_charts' AND COLUMN_NAME='chart_type'), 'MISSING') AS column_type;

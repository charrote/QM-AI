-- ============================================================
-- QM-AI Unified Schema Upgrade Script
-- 版本: v1.0
-- 日期: 2026-07-29
-- 用途: 远程数据库升级（增量迁移，幂等执行）
-- 适用: 任意阶段的 qmai 数据库
-- 前置: init.sql 已执行（或通过 Docker 初始化）
-- 执行顺序:
--   1. 本脚本 — 补齐所有缺失字段、对齐 C# EF Model
--   2. 如有新增表（SPC 等），按对应 migration_*.sql 执行
-- ============================================================

USE qmai;

DELIMITER $$

-- Helper: add column only if not exists
CREATE PROCEDURE IF NOT EXISTS sp_add_col(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_col VARCHAR(64),
    IN p_def VARCHAR(255)
)
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = p_db AND table_name = p_table AND column_name = p_col
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` ADD COLUMN `', p_col, '` ', p_def);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

-- Helper: change column type
CREATE PROCEDURE IF NOT EXISTS sp_change_col(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_col VARCHAR(64),
    IN p_def VARCHAR(255)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = p_db AND table_name = p_table AND column_name = p_col
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` CHANGE COLUMN `', p_col, '` `', p_col, '` ', p_def);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

-- Helper: drop column if exists
CREATE PROCEDURE IF NOT EXISTS sp_drop_col(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_col VARCHAR(64)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = p_db AND table_name = p_table AND column_name = p_col
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` DROP COLUMN `', p_col, '`');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

-- Helper: drop index if exists
CREATE PROCEDURE IF NOT EXISTS sp_drop_idx(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_idx VARCHAR(64)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.statistics
        WHERE table_schema = p_db AND table_name = p_table AND index_name = p_idx
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` DROP INDEX `', p_idx, '`');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

-- Helper: rename column if old exists
CREATE PROCEDURE IF NOT EXISTS sp_rename_col(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_old VARCHAR(64),
    IN p_new VARCHAR(64),
    IN p_def VARCHAR(255)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = p_db AND table_name = p_table AND column_name = p_old
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` CHANGE COLUMN `', p_old, '` `', p_new, '` ', p_def);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

-- Helper: drop FK if exists
CREATE PROCEDURE IF NOT EXISTS sp_drop_fk(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_name VARCHAR(64)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.REFERENTIAL_CONSTRAINTS
        WHERE CONSTRAINT_SCHEMA = p_db AND CONSTRAINT_NAME = p_name
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` DROP FOREIGN KEY `', p_name, '`');
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

-- ============================================================================
-- 1. products 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'products', 'description', 'VARCHAR(500) COMMENT ''产品描述''');
CALL sp_add_col(DATABASE(), 'products', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_products_org (org_id)');
CALL sp_change_col(DATABASE(), 'products', 'default_aql', 'DECIMAL(10,4) COMMENT ''默认AQL值''');

-- ============================================================================
-- 2. boms 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'boms', 'remark', 'VARCHAR(500) COMMENT ''备注''');
CALL sp_add_col(DATABASE(), 'boms', 'org_id', 'BIGINT COMMENT ''所属组织''');
-- Drop old `path` column if exists (replaced by remark)
CALL sp_drop_col(DATABASE(), 'boms', 'path');

-- ============================================================================
-- 3. processes 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'processes', 'department', 'VARCHAR(100) COMMENT ''所属部门/车间''');
CALL sp_add_col(DATABASE(), 'processes', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'processes', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix is_active ENUM -> TINYINT(1)
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'processes' AND COLUMN_NAME = 'is_active');
SET @fix_sql = IF(@col_type = 'enum''TRUE'',
    'ALTER TABLE processes MODIFY COLUMN is_active TINYINT(1) DEFAULT 1',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 4. routings 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'routings', 'org_id', 'BIGINT COMMENT ''所属组织''');
-- Drop old `steps` column if exists (JSON, no longer in C# model)
CALL sp_drop_col(DATABASE(), 'routings', 'steps');

-- ============================================================================
-- 5. inspection_standards 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'inspection_standards', 'description', 'TEXT COMMENT ''描述''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'item_name', 'VARCHAR(200) COMMENT ''检验项目名称''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'usl', 'DECIMAL(10,4) COMMENT ''规格上限''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'lsl', 'DECIMAL(10,4) COMMENT ''规格下限''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'target', 'DECIMAL(10,4) COMMENT ''目标值''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'unit', 'VARCHAR(50) COMMENT ''单位''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'inspection_method', 'VARCHAR(200) COMMENT ''检验方法''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'sampling_frequency', 'VARCHAR(100) COMMENT ''抽样频率''');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_standards_org (org_id)');
CALL sp_add_col(DATABASE(), 'inspection_standards', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix inspection_type ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'inspection_type');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE inspection_standards MODIFY COLUMN inspection_type VARCHAR(10) COMMENT ''IQC/IPQC/FQC/OQC''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 6. defect_codes 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'defect_codes', 'is_reworkable', 'TINYINT(1) DEFAULT 0 COMMENT ''是否可返工''');
CALL sp_add_col(DATABASE(), 'defect_codes', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_defect_codes_org (org_id)');
CALL sp_add_col(DATABASE(), 'defect_codes', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix defect_severity ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'defect_codes' AND COLUMN_NAME = 'defect_severity');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE defect_codes MODIFY COLUMN defect_severity VARCHAR(10) COMMENT ''CR/MA/MI''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 7. equipment 表补齐（注意：init.sql 中为 equipments，需重命名）
-- ============================================================================
-- Check if table is named 'equipments' or 'equipment'
SET @tb_exists_equipments = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE() AND table_name = 'equipments');
SET @tb_exists_equipment = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE() AND table_name = 'equipment');

-- Rename 'equipments' -> 'equipment' if needed
SET @rename_sql = IF(@tb_exists_equipments > 0 AND @tb_exists_equipment = 0,
    'ALTER TABLE equipments RENAME TO equipment',
    'SELECT 1');
PREPARE rename_stmt FROM @rename_sql;
EXECUTE rename_stmt;
DEALLOCATE PREPARE rename_stmt;

-- Now add missing columns to 'equipment' table
CALL sp_add_col(DATABASE(), 'equipment', 'production_line', 'VARCHAR(100) COMMENT ''所在产线''');
CALL sp_add_col(DATABASE(), 'equipment', 'workshop', 'VARCHAR(100) COMMENT ''所在车间''');
CALL sp_add_col(DATABASE(), 'equipment', 'has_mqtt_connection', 'TINYINT(1) DEFAULT 0 COMMENT ''是否关联MQTT''');
CALL sp_add_col(DATABASE(), 'equipment', 'mqtt_topic_prefix', 'VARCHAR(500) COMMENT ''MQTT Topic前缀''');
CALL sp_add_col(DATABASE(), 'equipment', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'equipment', 'workshop_id', 'BIGINT COMMENT ''关联车间(组织ID)''');
CALL sp_add_col(DATABASE(), 'equipment', 'line_id', 'BIGINT COMMENT ''关联产线(组织ID)''');
CALL sp_add_col(DATABASE(), 'equipment', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE equipment MODIFY COLUMN status VARCHAR(20) DEFAULT ''idle'' COMMENT ''running/idle/fault/maintenance''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 8. tools 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'tools', 'model', 'VARCHAR(200) COMMENT ''工具型号''');
CALL sp_add_col(DATABASE(), 'tools', 'design_life', 'DECIMAL(10,2) COMMENT ''设计寿命''');
CALL sp_add_col(DATABASE(), 'tools', 'life_unit', 'VARCHAR(20) DEFAULT ''cycles'' COMMENT ''寿命单位''');
CALL sp_add_col(DATABASE(), 'tools', 'supplier', 'VARCHAR(200) COMMENT ''供应商''');
CALL sp_add_col(DATABASE(), 'tools', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'tools', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix life_current type
SET @col_type = (SELECT DATA_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'life_current');
SET @fix_sql = IF(@col_type = 'int',
    'ALTER TABLE tools MODIFY COLUMN life_current DECIMAL(10,2) DEFAULT 0 COMMENT ''当前已用寿命''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 9. suppliers 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'suppliers', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_suppliers_org (org_id)');
CALL sp_add_col(DATABASE(), 'suppliers', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
CALL sp_add_col(DATABASE(), 'suppliers', 'supply_category', 'VARCHAR(50) COMMENT ''供应产品类别''');
-- Fix rating type if needed
SET @col_type = (SELECT DATA_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'suppliers' AND COLUMN_NAME = 'rating');
SET @fix_sql = IF(@col_type = 'decimal',
    'ALTER TABLE suppliers MODIFY COLUMN rating VARCHAR(10) COMMENT ''供应商等级：A/B/C/D''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'suppliers' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE suppliers MODIFY COLUMN status VARCHAR(20) DEFAULT ''active'' COMMENT ''active/inactive/blacklisted''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 10. customers 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'customers', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_customers_org (org_id)');
CALL sp_add_col(DATABASE(), 'customers', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');

-- ============================================================================
-- 11. users 表补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'users', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_users_org (org_id)');

-- ============================================================================
-- 12. iqc_receipts / iqc_inspections / iqc_anomalies 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'iqc_receipts', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_iqc_receipts_org (org_id)');
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'iqc_receipts' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE iqc_receipts MODIFY COLUMN status VARCHAR(20) DEFAULT ''pending'' COMMENT ''pending/inspecting/completed/anomaly''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

CALL sp_add_col(DATABASE(), 'iqc_inspections', 'sampling_level', 'VARCHAR(10) COMMENT ''抽样水平''');
CALL sp_add_col(DATABASE(), 'iqc_inspections', 'aql_value', 'DECIMAL(5,2) COMMENT ''AQL值''');
CALL sp_add_col(DATABASE(), 'iqc_inspections', 'org_id', 'BIGINT COMMENT ''所属组织''');
-- Fix result ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'iqc_inspections' AND COLUMN_NAME = 'result');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE iqc_inspections MODIFY COLUMN result VARCHAR(10) DEFAULT ''pending'' COMMENT ''pending/pass/fail/scrap''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

CALL sp_add_col(DATABASE(), 'iqc_anomalies', 'org_id', 'BIGINT COMMENT ''所属组织''');

-- ============================================================================
-- 13. ipqc_first_pieces 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'ipqc_first_pieces', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'ipqc_first_pieces', 'conclusion', 'VARCHAR(20) DEFAULT ''pending'' COMMENT ''结论''');
CALL sp_add_col(DATABASE(), 'ipqc_first_pieces', 'reason', 'VARCHAR(20) COMMENT ''不合格原因''');
CALL sp_add_col(DATABASE(), 'ipqc_first_pieces', 'shift', 'VARCHAR(20) COMMENT ''班次''');
CALL sp_add_col(DATABASE(), 'ipqc_first_pieces', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix result ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ipqc_first_pieces' AND COLUMN_NAME = 'result');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE ipqc_first_pieces MODIFY COLUMN result VARCHAR(20) DEFAULT ''pending'' COMMENT ''pass/fail/pending''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

CALL sp_add_col(DATABASE(), 'ipqc_patrol_plans', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_change_col(DATABASE(), 'ipqc_patrol_plans', 'status', 'VARCHAR(20) DEFAULT ''active'' COMMENT ''active/paused/completed''');

CALL sp_add_col(DATABASE(), 'ipqc_patrols', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'ipqc_patrols', 'conclusion', 'VARCHAR(20) DEFAULT ''pending'' COMMENT ''结论''');
CALL sp_add_col(DATABASE(), 'ipqc_patrols', 'status', 'VARCHAR(20) DEFAULT ''pending'' COMMENT ''状态''');
-- Fix result ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ipqc_patrols' AND COLUMN_NAME = 'result');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE ipqc_patrols MODIFY COLUMN result VARCHAR(20) DEFAULT ''pending'' COMMENT ''pass/fail/pending''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 14. fqc_inspections 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'batch_id', 'BIGINT COMMENT ''关联批次''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'work_order_id', 'BIGINT COMMENT ''关联工单''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'total_checked', 'INT DEFAULT 0 COMMENT ''已检数量''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'total_pass', 'INT DEFAULT 0 COMMENT ''合格数量''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'total_fail', 'INT DEFAULT 0 COMMENT ''不合格数量''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'ac', 'INT DEFAULT 0 COMMENT ''合格判定数Ac''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 're', 'INT DEFAULT 0 COMMENT ''不合格判定数Re''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'inspector_id', 'BIGINT COMMENT ''检验员ID''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'aql_level', 'DECIMAL(5,2)');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_fqc_org (org_id)');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'conclusion', 'VARCHAR(20) DEFAULT ''pending'' COMMENT ''结论''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'checked_at', 'DATETIME COMMENT ''检验时间''');
CALL sp_add_col(DATABASE(), 'fqc_inspections', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix result ENUM -> VARCHAR (if exists)
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'fqc_inspections' AND COLUMN_NAME = 'result');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE fqc_inspections MODIFY COLUMN result VARCHAR(20) DEFAULT ''pending'' COMMENT ''qualified/unqualified/pending''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Drop old columns replaced by batch_id
CALL sp_drop_col(DATABASE(), 'fqc_inspections', 'product_id');
CALL sp_drop_col(DATABASE(), 'fqc_inspections', 'batch_no');
-- Drop old FK
CALL sp_drop_fk(DATABASE(), 'fqc_inspections', 'fqc_inspections_ibfk_1');

-- ============================================================================
-- 15. product_batches (原 batches)
-- ============================================================================
SET @tb_exists_batches = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE() AND table_name = 'batches');
SET @tb_exists_product_batches = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE() AND table_name = 'product_batches');
SET @rename_sql = IF(@tb_exists_batches > 0 AND @tb_exists_product_batches = 0,
    'ALTER TABLE batches RENAME TO product_batches',
    'SELECT 1');
PREPARE rename_stmt FROM @rename_sql;
EXECUTE rename_stmt;
DEALLOCATE PREPARE rename_stmt;

-- Add org_id to product_batches if it exists
CALL sp_add_col(DATABASE(), 'product_batches', 'org_id', 'BIGINT COMMENT ''所属组织''');

-- ============================================================================
-- 16. oqc_releases 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'oqc_releases', 'batch_id', 'BIGINT COMMENT ''关联批次''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'customer_id', 'BIGINT COMMENT ''关联客户''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'release_number', 'VARCHAR(50) NOT NULL DEFAULT '' COMMENT ''放行单号（唯一）''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'quantity', 'DECIMAL(18,4) NOT NULL DEFAULT 0 COMMENT ''放行数量''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'authorized_by', 'INT COMMENT ''授权人ID''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'e_signature_url', 'VARCHAR(500) COMMENT ''电子签名URL''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'signature_time', 'DATETIME COMMENT ''签名时间''');
CALL sp_add_col(DATABASE(), 'oqc_releases', 'updated_at', 'DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Rename release_no -> release_number
CALL sp_rename_col(DATABASE(), 'oqc_releases', 'release_no', 'release_number', 'VARCHAR(50) NOT NULL DEFAULT '' COMMENT ''放行单号（唯一）''');
-- Rename released_by -> authorized_by
CALL sp_rename_col(DATABASE(), 'oqc_releases', 'released_by', 'authorized_by', 'INT COMMENT ''授权人ID''');
-- Rename signature_url -> e_signature_url
CALL sp_rename_col(DATABASE(), 'oqc_releases', 'signature_url', 'e_signature_url', 'VARCHAR(500) COMMENT ''电子签名URL''');
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'oqc_releases' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE oqc_releases MODIFY COLUMN status VARCHAR(20) NOT NULL DEFAULT ''pending'' COMMENT ''pending/signed/released/cancelled''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Drop old FK and column
CALL sp_drop_fk(DATABASE(), 'oqc_releases', 'oqc_releases_ibfk_1');
CALL sp_drop_col(DATABASE(), 'oqc_releases', 'inspection_id');

-- ============================================================================
-- 17. spc_control_charts 补齐
-- ============================================================================
-- Fix chart_type ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'spc_control_charts' AND COLUMN_NAME = 'chart_type');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE spc_control_charts MODIFY COLUMN chart_type VARCHAR(10) NOT NULL COMMENT ''Xbar_R / Xbar_S / I_MR''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
CALL sp_add_col(DATABASE(), 'spc_control_charts', 'org_id', 'BIGINT COMMENT ''所属组织''');

-- Fix spc_anova_results source ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'spc_anova_results' AND COLUMN_NAME = 'source');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE spc_anova_results MODIFY COLUMN source VARCHAR(20) NOT NULL COMMENT ''operator/machine/material/method/environment''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 18. complaints 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'complaints', 'subject', 'VARCHAR(500) COMMENT ''投诉主题''');
CALL sp_add_col(DATABASE(), 'complaints', 'status', 'VARCHAR(20) DEFAULT ''new'' COMMENT ''new/acknowledged/in_progress/closed''');
CALL sp_add_col(DATABASE(), 'complaints', 'five_w2h_json', 'JSON COMMENT ''5W2H分析''');
CALL sp_add_col(DATABASE(), 'complaints', 'assigned_to', 'BIGINT COMMENT ''指派人''');
CALL sp_add_col(DATABASE(), 'complaints', 'due_date', 'DATE');
CALL sp_add_col(DATABASE(), 'complaints', 'acknowledged_at', 'DATETIME');
CALL sp_add_col(DATABASE(), 'complaints', 'closed_at', 'DATETIME');
CALL sp_add_col(DATABASE(), 'complaints', 'created_by', 'BIGINT COMMENT ''创建人''');
CALL sp_add_col(DATABASE(), 'complaints', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_complaints_org (org_id)');
CALL sp_add_col(DATABASE(), 'complaints', 'updated_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix severity ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'complaints' AND COLUMN_NAME = 'severity');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE complaints MODIFY COLUMN severity VARCHAR(10) COMMENT ''critical/major/minor''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'complaints' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE complaints MODIFY COLUMN status VARCHAR(20) DEFAULT ''new'' COMMENT ''new/acknowledged/in_progress/closed''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 19. defects 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'defects', 'source_type', 'VARCHAR(10) NOT NULL COMMENT ''iqc/ipqc/fqc/oqc/customer''');
CALL sp_add_col(DATABASE(), 'defects', 'quantity', 'DECIMAL(15,2) NOT NULL');
CALL sp_add_col(DATABASE(), 'defects', 'image_urls', 'JSON');
CALL sp_add_col(DATABASE(), 'defects', 'status', 'VARCHAR(20) NOT NULL DEFAULT ''open'' COMMENT ''open/investigating/resolved/closed''');
CALL sp_add_col(DATABASE(), 'defects', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_defects_org (org_id)');
CALL sp_add_col(DATABASE(), 'defects', 'updated_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix severity ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'defects' AND COLUMN_NAME = 'severity');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE defects MODIFY COLUMN severity VARCHAR(10) NOT NULL DEFAULT ''major'' COMMENT ''critical/major/minor''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;

-- ============================================================================
-- 20. equipment_param_mappings 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'equipment_param_mappings', 'system_param_code', 'VARCHAR(50) NOT NULL');
CALL sp_add_col(DATABASE(), 'equipment_param_mappings', 'param_group_id', 'BIGINT COMMENT ''关联参数组ID'', INDEX idx_equip_param_mapping_param (param_group_id)');
CALL sp_add_col(DATABASE(), 'equipment_param_mappings', 'data_type', 'VARCHAR(10) NOT NULL DEFAULT ''numeric''');
CALL sp_add_col(DATABASE(), 'equipment_param_mappings', 'unit', 'VARCHAR(20)');
CALL sp_add_col(DATABASE(), 'equipment_param_mappings', 'updated_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Drop old column
CALL sp_drop_col(DATABASE(), 'equipment_param_mappings', 'param_id');
CALL sp_drop_col(DATABASE(), 'equipment_param_mappings', 'data_path');
CALL sp_drop_col(DATABASE(), 'equipment_param_mappings', 'transform_expression');

-- ============================================================================
-- 21. audits 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'audits', 'description', 'TEXT COMMENT ''描述''');
CALL sp_add_col(DATABASE(), 'audits', 'start_date', 'DATE NOT NULL COMMENT ''开始日期''');
CALL sp_add_col(DATABASE(), 'audits', 'end_date', 'DATE NOT NULL COMMENT ''结束日期''');
CALL sp_add_col(DATABASE(), 'audits', 'auditor_id', 'BIGINT NOT NULL COMMENT ''审核人ID''');
CALL sp_add_col(DATABASE(), 'audits', 'auditor_ids_json', 'JSON COMMENT ''审核人IDs（JSON数组）''');
CALL sp_add_col(DATABASE(), 'audits', 'total_findings', 'INT DEFAULT 0 COMMENT ''总发现数''');
CALL sp_add_col(DATABASE(), 'audits', 'conformities', 'INT DEFAULT 0 COMMENT ''符合项数''');
CALL sp_add_col(DATABASE(), 'audits', 'non_conformities', 'INT DEFAULT 0 COMMENT ''不符合项数''');
CALL sp_add_col(DATABASE(), 'audits', 'opportunities', 'INT DEFAULT 0 COMMENT ''改进机会数''');
CALL sp_add_col(DATABASE(), 'audits', 'scope', 'JSON COMMENT ''审核范围''');
CALL sp_add_col(DATABASE(), 'audits', 'updated_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix audit_type ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'audits' AND COLUMN_NAME = 'audit_type');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE audits MODIFY COLUMN audit_type VARCHAR(10) NOT NULL COMMENT ''internal/process/product''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'audits' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE audits MODIFY COLUMN status VARCHAR(10) NOT NULL DEFAULT ''planned'' COMMENT ''planned/in_progress/completed/archived''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Drop old columns
CALL sp_drop_col(DATABASE(), 'audits', 'audited_dept');
CALL sp_drop_col(DATABASE(), 'audits', 'findings');
CALL sp_drop_col(DATABASE(), 'audits', 'report_url');

-- ============================================================================
-- 22. audit_findings (新表，init.sql 中不存在)
-- ============================================================================
SET @tb_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE() AND table_name = 'audit_findings');
SET @tb_sql = IF(@tb_exists = 0,
    'CREATE TABLE audit_findings (
        id BIGINT AUTO_INCREMENT PRIMARY KEY,
        audit_id BIGINT NOT NULL,
        finding_type VARCHAR(20) NOT NULL,
        severity VARCHAR(10),
        description TEXT NOT NULL,
        evidence TEXT,
        requirement_ref VARCHAR(200),
        status VARCHAR(10) NOT NULL DEFAULT ''open'',
        rectification_plan JSON,
        responsible_user_id BIGINT,
        rectification_due_date DATE,
        verified_by BIGINT,
        verified_at DATETIME,
        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
        updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
        INDEX idx_af_audit (audit_id),
        INDEX idx_af_status (status),
        FOREIGN KEY (audit_id) REFERENCES audits(id) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'SELECT 1');
PREPARE tb_stmt FROM @tb_sql;
EXECUTE tb_stmt;
DEALLOCATE PREPARE tb_stmt;

-- ============================================================================
-- 23. documents 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'documents', 'minio_key', 'VARCHAR(500) NOT NULL COMMENT ''MinIO对象键''');
CALL sp_add_col(DATABASE(), 'documents', 'file_size_bytes', 'BIGINT COMMENT ''文件大小''');
CALL sp_add_col(DATABASE(), 'documents', 'file_hash', 'VARCHAR(64) COMMENT ''SHA-256哈希''');
CALL sp_change_col(DATABASE(), 'documents', 'version', 'INT NOT NULL DEFAULT 1');
CALL sp_change_col(DATABASE(), 'documents', 'status', 'VARCHAR(10) NOT NULL DEFAULT ''draft''');
CALL sp_add_col(DATABASE(), 'documents', 'approved_by', 'BIGINT COMMENT ''审批人ID''');
CALL sp_add_col(DATABASE(), 'documents', 'rejection_reason', 'VARCHAR(500) COMMENT ''驳回理由''');
CALL sp_add_col(DATABASE(), 'documents', 'approved_at', 'DATETIME COMMENT ''审批时间''');
CALL sp_add_col(DATABASE(), 'documents', 'expires_at', 'DATE COMMENT ''有效期''');
CALL sp_change_col(DATABASE(), 'documents', 'created_by', 'BIGINT NOT NULL');
CALL sp_add_col(DATABASE(), 'documents', 'updated_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Fix doc_type ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'documents' AND COLUMN_NAME = 'doc_type');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE documents MODIFY COLUMN doc_type VARCHAR(20) NOT NULL',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Fix status ENUM -> VARCHAR
SET @col_type = (SELECT COLUMN_TYPE FROM information_schema.columns
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'documents' AND COLUMN_NAME = 'status');
SET @fix_sql = IF(@col_type LIKE 'enum%',
    'ALTER TABLE documents MODIFY COLUMN status VARCHAR(10) NOT NULL DEFAULT ''draft''',
    'SELECT 1');
PREPARE fix_stmt FROM @fix_sql;
EXECUTE fix_stmt;
DEALLOCATE PREPARE fix_stmt;
-- Drop old columns
CALL sp_drop_col(DATABASE(), 'documents', 'doc_code');
CALL sp_drop_col(DATABASE(), 'documents', 'file_url');

-- ============================================================================
-- 24. document_versions 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'document_versions', 'minio_key', 'VARCHAR(500) NOT NULL');
CALL sp_change_col(DATABASE(), 'document_versions', 'version', 'INT NOT NULL');
CALL sp_add_col(DATABASE(), 'document_versions', 'change_description', 'TEXT COMMENT ''变更说明''');
CALL sp_change_col(DATABASE(), 'document_versions', 'created_by', 'BIGINT NOT NULL');
CALL sp_add_col(DATABASE(), 'document_versions', 'created_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP');
-- Drop old columns
CALL sp_drop_col(DATABASE(), 'document_versions', 'file_url');
CALL sp_drop_col(DATABASE(), 'document_versions', 'change_notes');
CALL sp_drop_col(DATABASE(), 'document_versions', 'uploaded_by');
CALL sp_drop_col(DATABASE(), 'document_versions', 'uploaded_at');

-- ============================================================================
-- 25. supplier_scores 补齐
-- ============================================================================
CALL sp_change_col(DATABASE(), 'supplier_scores', 'assessment_date', 'DATE COMMENT ''评分日期''');
CALL sp_change_col(DATABASE(), 'supplier_scores', 'grade', 'VARCHAR(1) COMMENT ''评级：A/B/C/D''');
-- Drop old column
CALL sp_drop_col(DATABASE(), 'supplier_scores', 'score_date');

-- ============================================================================
-- 26. spc_data_sources (新增列)
-- ============================================================================
CALL sp_add_col(DATABASE(), 'spc_data_sources', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'spc_data_sources', 'filter_conditions', 'JSON COMMENT ''筛选条件''');
CALL sp_add_col(DATABASE(), 'spc_data_sources', 'field_mapping', 'JSON COMMENT ''字段映射''');
CALL sp_add_col(DATABASE(), 'spc_data_sources', 'enabled', 'TINYINT(1) DEFAULT 1');
CALL sp_add_col(DATABASE(), 'spc_data_sources', 'updated_at', 'DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');
-- Drop old columns
CALL sp_drop_col(DATABASE(), 'spc_data_sources', 'source_category');

-- ============================================================================
-- 27. closure_rules 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'closure_rules', 'org_id', 'BIGINT COMMENT ''所属组织''');

-- ============================================================================
-- 28. inspection_items / inspection_plans 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'inspection_items', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_inspection_items_org (org_id)');
CALL sp_add_col(DATABASE(), 'inspection_plans', 'org_id', 'BIGINT COMMENT ''所属组织'', INDEX idx_plans_org (org_id)');

-- ============================================================================
-- 29. param_groups / dynamic_params 补齐
-- ============================================================================
CALL sp_add_col(DATABASE(), 'param_groups', 'org_id', 'BIGINT COMMENT ''所属组织''');
CALL sp_add_col(DATABASE(), 'dynamic_params', 'org_id', 'BIGINT COMMENT ''所属组织''');

SELECT 'Schema upgrade completed successfully!' AS status;

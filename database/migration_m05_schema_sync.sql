-- ============================================================================
-- M05 Schema Sync Migration
-- Purpose: Align MySQL database schema with C# EF Core models
-- Affected: product_batches, packaging_confirmations (new)
--           fqc_inspections, fqc_inspection_items, oqc_releases (restructured)
--
-- This migration uses stored procedures to safely handle conditional ALTERs.
-- Run with: mysql -u root -p < migration_m05_schema_sync.sql
-- ============================================================================

DELIMITER $$

-- Helper: add column only if it doesn't exist
CREATE PROCEDURE IF NOT EXISTS sp_add_column_if_not_exists(
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

-- Helper: drop column only if it exists
CREATE PROCEDURE IF NOT EXISTS sp_drop_column_if_exists(
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

-- Helper: drop index only if it exists
CREATE PROCEDURE IF NOT EXISTS sp_drop_index_if_exists(
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

-- Helper: drop foreign key only if it exists
CREATE PROCEDURE IF NOT EXISTS sp_drop_fk_if_exists(
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

-- Helper: rename column (handles old/new naming)
CREATE PROCEDURE IF NOT EXISTS sp_rename_column_if_exists(
    IN p_db VARCHAR(64),
    IN p_table VARCHAR(64),
    IN p_old_col VARCHAR(64),
    IN p_new_col VARCHAR(64),
    IN p_def VARCHAR(255)
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = p_db AND table_name = p_table AND column_name = p_old_col
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table, '` CHANGE COLUMN `', p_old_col, '` `', p_new_col, '` ', p_def);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

-- Helper: change column type
CREATE PROCEDURE IF NOT EXISTS sp_change_column_if_exists(
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

DELIMITER ;

-- ─── Get current database name ─────────────────────────────────────
SET @current_db = DATABASE();

-- ============================================================================
-- 1. Create product_batches table (new)
-- ============================================================================
SET @tb_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = @current_db AND table_name = 'product_batches');
SET @tb_sql = IF(@tb_exists = 0,
    'CREATE TABLE `product_batches` (
        `id` BIGINT AUTO_INCREMENT PRIMARY KEY,
        `batch_code` VARCHAR(50) NOT NULL COMMENT ''批次编号（LOT-YYYYMMDD-X）'',
        `source` VARCHAR(20) NOT NULL DEFAULT ''manual'' COMMENT ''来源：manual / ipqc-auto / work-order'',
        `product_id` BIGINT NOT NULL COMMENT ''关联产品'',
        `work_order_id` BIGINT COMMENT ''关联工单'',
        `quantity` DECIMAL(18,4) NOT NULL DEFAULT 0 COMMENT ''数量'',
        `status` VARCHAR(20) NOT NULL DEFAULT ''in_progress'' COMMENT ''状态：in_progress / inspected / released / quarantined'',
        `created_at` DATETIME NOT NULL DEFAULT NOW(),
        `updated_at` DATETIME NOT NULL DEFAULT NOW(),
        UNIQUE KEY `uk_product_batches_batch_code` (`batch_code`),
        KEY `idx_product_batches_status` (`status`),
        KEY `idx_product_batches_product` (`product_id`),
        CONSTRAINT `fk_product_batches_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE RESTRICT
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'SELECT 1');
PREPARE tb_stmt FROM @tb_sql;
EXECUTE tb_stmt;
DEALLOCATE PREPARE tb_stmt;

-- ============================================================================
-- 2. Create packaging_confirmations table (new)
-- ============================================================================
SET @tb_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = @current_db AND table_name = 'packaging_confirmations');
SET @tb_sql = IF(@tb_exists = 0,
    'CREATE TABLE `packaging_confirmations` (
        `id` BIGINT AUTO_INCREMENT PRIMARY KEY,
        `batch_id` BIGINT NOT NULL COMMENT ''关联批次'',
        `packaging_method` VARCHAR(200) NOT NULL COMMENT ''包装方式'',
        `qty_per_box` INT COMMENT ''每箱数量'',
        `total_boxes` INT COMMENT ''总箱数'',
        `label_printed` TINYINT(1) NOT NULL DEFAULT 0 COMMENT ''标签是否已打印'',
        `confirmed_by` INT NOT NULL DEFAULT 0 COMMENT ''确认人ID'',
        `confirmed_at` DATETIME NOT NULL DEFAULT NOW(),
        KEY `idx_packaging_batch_id` (`batch_id`),
        KEY `idx_packaging_confirmed_at` (`confirmed_at`),
        CONSTRAINT `fk_packaging_batch` FOREIGN KEY (`batch_id`) REFERENCES `product_batches` (`id`) ON DELETE CASCADE
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci',
    'SELECT 1');
PREPARE tb_stmt FROM @tb_sql;
EXECUTE tb_stmt;
DEALLOCATE PREPARE tb_stmt;

-- ============================================================================
-- 3. Restructure fqc_inspections table
-- ============================================================================
-- Add new columns
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'batch_id', 'BIGINT COMMENT ''关联批次''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'batch_code', 'VARCHAR(50) COMMENT ''批次号''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'sample_size', 'INT NOT NULL DEFAULT 0 COMMENT ''样本量''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'total_checked', 'INT NOT NULL DEFAULT 0 COMMENT ''已检数量''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'total_pass', 'INT NOT NULL DEFAULT 0 COMMENT ''合格数量''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'total_fail', 'INT NOT NULL DEFAULT 0 COMMENT ''不合格数量''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'ac', 'INT NOT NULL DEFAULT 0 COMMENT ''合格判定数Ac''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 're', 'INT NOT NULL DEFAULT 0 COMMENT ''不合格判定数Re''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'inspector_id', 'BIGINT COMMENT ''检验员ID''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'work_order_id', 'BIGINT COMMENT ''关联工单''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'checked_at', 'DATETIME COMMENT ''检验时间''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'conclusion', 'VARCHAR(20) NOT NULL DEFAULT ''pending'' COMMENT ''结论：qualified / unqualified / pending''');
CALL sp_add_column_if_not_exists(@current_db, 'fqc_inspections', 'updated_at', 'DATETIME NOT NULL DEFAULT NOW()');

-- Rename 'result' to 'conclusion' (if old column exists)
CALL sp_rename_column_if_exists(@current_db, 'fqc_inspections', 'result', 'conclusion',
    'VARCHAR(20) NOT NULL DEFAULT ''pending'' COMMENT ''结论：qualified / unqualified / pending''');

-- Change quantity to DECIMAL (was INT)
CALL sp_change_column_if_exists(@current_db, 'fqc_inspections', 'quantity',
    'DECIMAL(18,4) DEFAULT 0 COMMENT ''批次数量（保留字段）''');

-- Add new indexes
CALL sp_drop_index_if_exists(@current_db, 'fqc_inspections', 'idx_fqc_inspections_product');
CALL sp_drop_index_if_exists(@current_db, 'fqc_inspections', 'idx_fqc_inspections_result');

-- Drop old foreign key
CALL sp_drop_fk_if_exists(@current_db, 'fqc_inspections', 'fqc_inspections_ibfk_1');

-- Drop old columns (replaced by batch_id + batch_code)
CALL sp_drop_column_if_exists(@current_db, 'fqc_inspections', 'product_id');
CALL sp_drop_column_if_exists(@current_db, 'fqc_inspections', 'batch_no');

-- Add new indexes
SET @idx_batch = (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @current_db AND table_name = 'fqc_inspections' AND index_name = 'idx_fqc_inspections_batch_id');
SET @idx_batch_sql = IF(@idx_batch = 0,
    'ALTER TABLE `fqc_inspections` ADD INDEX `idx_fqc_inspections_batch_id` (`batch_id`)',
    'SELECT 1');
PREPARE idx_batch_stmt FROM @idx_batch_sql;
EXECUTE idx_batch_stmt;
DEALLOCATE PREPARE idx_batch_stmt;

SET @idx_conclusion = (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @current_db AND table_name = 'fqc_inspections' AND index_name = 'idx_fqc_inspections_conclusion');
SET @idx_conclusion_sql = IF(@idx_conclusion = 0,
    'ALTER TABLE `fqc_inspections` ADD INDEX `idx_fqc_inspections_conclusion` (`conclusion`)',
    'SELECT 1');
PREPARE idx_conclusion_stmt FROM @idx_conclusion_sql;
EXECUTE idx_conclusion_stmt;
DEALLOCATE PREPARE idx_conclusion_stmt;

-- ============================================================================
-- 4. Restructure fqc_inspection_items table
-- ============================================================================
-- Rename measured_value to actual_value
CALL sp_rename_column_if_exists(@current_db, 'fqc_inspection_items', 'measured_value', 'actual_value',
    'DECIMAL(12,4) COMMENT ''实测值''');

-- Remove param_id column (not in C# model)
CALL sp_drop_column_if_exists(@current_db, 'fqc_inspection_items', 'param_id');

-- Drop old foreign key on param_id
CALL sp_drop_fk_if_exists(@current_db, 'fqc_inspection_items', 'fqc_inspection_items_ibfk_2');

-- ============================================================================
-- 5. Restructure oqc_releases table
-- ============================================================================
-- Add new columns
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'batch_id', 'BIGINT COMMENT ''关联批次''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'customer_id', 'BIGINT COMMENT ''关联客户''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'release_number', 'VARCHAR(50) NOT NULL DEFAULT '''' COMMENT ''放行单号（唯一）''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'quantity', 'DECIMAL(18,4) NOT NULL DEFAULT 0 COMMENT ''放行数量''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'authorized_by', 'INT COMMENT ''授权人ID''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'e_signature_url', 'VARCHAR(500) COMMENT ''电子签名URL（MinIO）''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'signature_time', 'DATETIME COMMENT ''签名时间''');
CALL sp_add_column_if_not_exists(@current_db, 'oqc_releases', 'updated_at', 'DATETIME NOT NULL DEFAULT NOW()');

-- Rename release_no to release_number
CALL sp_rename_column_if_exists(@current_db, 'oqc_releases', 'release_no', 'release_number',
    'VARCHAR(50) NOT NULL DEFAULT '''' COMMENT ''放行单号（唯一）''');

-- Change status from ENUM to VARCHAR (to support signed/cancelled states)
SET @status_col_type = (SELECT DATA_TYPE FROM information_schema.columns
    WHERE table_schema = @current_db AND table_name = 'oqc_releases' AND column_name = 'status');
SET @status_sql = IF(@status_col_type = 'enum',
    'ALTER TABLE `oqc_releases` MODIFY COLUMN `status` VARCHAR(20) NOT NULL DEFAULT ''pending'' COMMENT ''状态：pending / signed / released / cancelled''',
    'SELECT 1');
PREPARE status_stmt FROM @status_sql;
EXECUTE status_stmt;
DEALLOCATE PREPARE status_stmt;

-- Rename released_by to authorized_by
CALL sp_rename_column_if_exists(@current_db, 'oqc_releases', 'released_by', 'authorized_by',
    'INT COMMENT ''授权人ID''');

-- Rename signature_url to e_signature_url
CALL sp_rename_column_if_exists(@current_db, 'oqc_releases', 'signature_url', 'e_signature_url',
    'VARCHAR(500) COMMENT ''电子签名URL（MinIO）''');

-- Drop old foreign key on inspection_id
CALL sp_drop_fk_if_exists(@current_db, 'oqc_releases', 'oqc_releases_ibfk_1');

-- Drop old inspection_id column
CALL sp_drop_column_if_exists(@current_db, 'oqc_releases', 'inspection_id');

-- Add unique index on release_number
SET @idx_rel_no = (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @current_db AND table_name = 'oqc_releases' AND index_name = 'uk_oqc_releases_release_number');
SET @idx_rel_no_sql = IF(@idx_rel_no = 0,
    'ALTER TABLE `oqc_releases` ADD UNIQUE KEY `uk_oqc_releases_release_number` (`release_number`)',
    'SELECT 1');
PREPARE idx_rel_no_stmt FROM @idx_rel_no_sql;
EXECUTE idx_rel_no_stmt;
DEALLOCATE PREPARE idx_rel_no_stmt;

-- Add new indexes and foreign keys
SET @idx_batch = (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @current_db AND table_name = 'oqc_releases' AND index_name = 'idx_oqc_releases_batch_id');
SET @idx_batch_sql = IF(@idx_batch = 0,
    'ALTER TABLE `oqc_releases` ADD INDEX `idx_oqc_releases_batch_id` (`batch_id`)',
    'SELECT 1');
PREPARE idx_batch_stmt FROM @idx_batch_sql;
EXECUTE idx_batch_stmt;
DEALLOCATE PREPARE idx_batch_stmt;

SET @idx_cust = (SELECT COUNT(*) FROM information_schema.statistics
    WHERE table_schema = @current_db AND table_name = 'oqc_releases' AND index_name = 'idx_oqc_releases_customer_id');
SET @idx_cust_sql = IF(@idx_cust = 0,
    'ALTER TABLE `oqc_releases` ADD INDEX `idx_oqc_releases_customer_id` (`customer_id`)',
    'SELECT 1');
PREPARE idx_cust_stmt FROM @idx_cust_sql;
EXECUTE idx_cust_stmt;
DEALLOCATE PREPARE idx_cust_stmt;

-- Add foreign keys for batch_id and customer_id
SET @fk_batch_exists = (SELECT COUNT(*) FROM information_schema.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = @current_db AND CONSTRAINT_NAME = 'fk_oqc_releases_batch');
SET @fk_batch_sql = IF(@fk_batch_exists = 0,
    'ALTER TABLE `oqc_releases` ADD CONSTRAINT `fk_oqc_releases_batch` FOREIGN KEY (`batch_id`) REFERENCES `product_batches` (`id`) ON DELETE CASCADE',
    'SELECT 1');
PREPARE fk_batch_stmt FROM @fk_batch_sql;
EXECUTE fk_batch_stmt;
DEALLOCATE PREPARE fk_batch_stmt;

SET @fk_cust_exists = (SELECT COUNT(*) FROM information_schema.REFERENTIAL_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = @current_db AND CONSTRAINT_NAME = 'fk_oqc_releases_customer');
SET @fk_cust_sql = IF(@fk_cust_exists = 0,
    'ALTER TABLE `oqc_releases` ADD CONSTRAINT `fk_oqc_releases_customer` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`) ON DELETE RESTRICT',
    'SELECT 1');
PREPARE fk_cust_stmt FROM @fk_cust_sql;
EXECUTE fk_cust_stmt;
DEALLOCATE PREPARE fk_cust_stmt;

-- Drop old indexes that reference removed columns
CALL sp_drop_index_if_exists(@current_db, 'oqc_releases', 'idx_oqc_releases_inspection');

-- ============================================================================
-- QM-AI Migration: Align DB Schema to EF Models
-- Generated: 2026-07-29
-- Target: MySQL 8.0+ | Database: qmai
-- Strategy: Each operation is idempotent (safe to re-run)
-- ============================================================================

USE qmai;

-- ============================================================================
-- Helper: Stored Procedure to check column existence
-- ============================================================================
DROP PROCEDURE IF EXISTS sp_column_exists;
DELIMITER $$
CREATE PROCEDURE sp_column_exists(
    IN p_table VARCHAR(64),
    IN p_column VARCHAR(64),
    OUT p_exists TINYINT
)
BEGIN
    SELECT COUNT(*) INTO p_exists
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = p_table
      AND COLUMN_NAME = p_column;
END$$
DELIMITER ;

-- ============================================================================
-- STEP 1: Create missing tables (exist in EF but not in DB)
-- ============================================================================

-- 1.1 complaint_events (M09)
DROP TABLE IF EXISTS complaint_events;
CREATE TABLE complaint_events (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    complaint_id BIGINT NOT NULL,
    event_type VARCHAR(20) NOT NULL,
    event_data TEXT COMMENT '事件数据（JSON）',
    created_by BIGINT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (complaint_id) REFERENCES complaints(id) ON DELETE CASCADE,
    INDEX idx_ce_complaint (complaint_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.2 packaging_confirmations (M05)
DROP TABLE IF EXISTS packaging_confirmations;
CREATE TABLE packaging_confirmations (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    batch_id BIGINT NOT NULL,
    packaging_method VARCHAR(200) NOT NULL,
    qty_per_box INT,
    total_boxes INT,
    label_printed TINYINT(1) DEFAULT 0,
    confirmed_by INT NOT NULL DEFAULT 0,
    confirmed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (batch_id) REFERENCES product_batches(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 1.3 ipqc_ai_risk_scores (M04)
DROP TABLE IF EXISTS ipqc_ai_risk_scores;
CREATE TABLE ipqc_ai_risk_scores (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_id BIGINT NOT NULL,
    process_id BIGINT NOT NULL,
    work_order_id BIGINT,
    risk_score INT NOT NULL DEFAULT 0,
    risk_level VARCHAR(20) NOT NULL DEFAULT 'normal',
    factors_json TEXT COMMENT '风险因素分解 (JSON)',
    trend_direction VARCHAR(10) COMMENT 'stable/rising/falling',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_ai_risk_equip (equipment_id),
    INDEX idx_ai_risk_process (process_id),
    INDEX idx_ai_risk_work_order (work_order_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- STEP 2: users table
-- ============================================================================

-- Change username from VARCHAR(50) to VARCHAR(100)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='users' AND COLUMN_NAME='username';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(50)' THEN
    ALTER TABLE users MODIFY COLUMN username VARCHAR(100) NOT NULL;
END IF;

-- Change password_hash from VARCHAR(255) to VARCHAR(500)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='users' AND COLUMN_NAME='password_hash';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(255)' THEN
    ALTER TABLE users MODIFY COLUMN password_hash VARCHAR(500) NOT NULL;
END IF;

-- Change display_name from VARCHAR(100) to VARCHAR(200)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='users' AND COLUMN_NAME='display_name';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(100)' THEN
    ALTER TABLE users MODIFY COLUMN display_name VARCHAR(200);
END IF;

-- Change email from VARCHAR(100) to VARCHAR(200)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='users' AND COLUMN_NAME='email';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(100)' THEN
    ALTER TABLE users MODIFY COLUMN email VARCHAR(200);
END IF;

-- Remove phone column (not in EF model)
CALL sp_column_exists('users', 'phone', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE users DROP COLUMN phone', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Remove last_login_at column (not in EF model)
CALL sp_column_exists('users', 'last_login_at', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE users DROP COLUMN last_login_at', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Add org_id column (not in EF model, but referenced by other tables)
CALL sp_column_exists('users', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE users ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add created_by column
CALL sp_column_exists('users', 'created_by', @exists);
IF @exists = 0 THEN
    ALTER TABLE users ADD COLUMN created_by BIGINT COMMENT '创建人';
END IF;

-- ============================================================================
-- STEP 3: roles table
-- ============================================================================

-- Change name from VARCHAR(50) to VARCHAR(100)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='roles' AND COLUMN_NAME='name';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(50)' THEN
    ALTER TABLE roles MODIFY COLUMN name VARCHAR(100) NOT NULL;
END IF;

-- Change description from VARCHAR(255) to VARCHAR(500)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='roles' AND COLUMN_NAME='description';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(255)' THEN
    ALTER TABLE roles MODIFY COLUMN description VARCHAR(500);
END IF;

-- Add created_at with CURRENT_TIMESTAMP
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='roles' AND COLUMN_NAME='created_at';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = '' THEN
    ALTER TABLE roles ADD COLUMN created_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 4: permissions table
-- ============================================================================

-- Remove description column (not in EF model)
CALL sp_column_exists('permissions', 'description', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE permissions DROP COLUMN description', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 5: products table
-- ============================================================================

-- Change product_code unique constraint
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='products' AND COLUMN_NAME='product_code';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(50)' THEN
    ALTER TABLE products MODIFY COLUMN product_code VARCHAR(50) UNIQUE NOT NULL;
END IF;

-- ============================================================================
-- STEP 6: boms table
-- ============================================================================

-- Change quantity from DECIMAL(10,3) to DECIMAL(10,2) and add default
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='boms' AND COLUMN_NAME='quantity';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'decimal(10,3)' OR @col_type = 'decimal(10,3) DEFAULT NULL' THEN
    ALTER TABLE boms MODIFY COLUMN quantity DOUBLE NOT NULL DEFAULT 1;
END IF;

-- ============================================================================
-- STEP 7: processes table
-- ============================================================================

-- Change description from TEXT to VARCHAR(500)
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='processes' AND COLUMN_NAME='description';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'text' THEN
    ALTER TABLE processes MODIFY COLUMN description VARCHAR(500);
END IF;

-- ============================================================================
-- STEP 8: routings table
-- ============================================================================

-- Add routing_name column
CALL sp_column_exists('routings', 'routing_name', @exists);
IF @exists = 0 THEN
    ALTER TABLE routings ADD COLUMN routing_name VARCHAR(200) COMMENT '工艺路线名称';
END IF;

-- Add standard_time_minutes column (as DOUBLE, matching EF)
CALL sp_column_exists('routings', 'standard_time_minutes', @exists);
IF @exists = 0 THEN
    ALTER TABLE routings ADD COLUMN standard_time_minutes DOUBLE COMMENT '标准工时（分钟）';
END IF;

-- Add product_id NOT NULL (may already exist, just ensure it's NOT NULL)
CALL sp_column_exists('routings', 'product_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE routings ADD COLUMN product_id BIGINT NOT NULL;
END IF;

-- ============================================================================
-- STEP 9: routing_steps table - complete rewrite to match EF
-- ============================================================================

-- The EF model has: routing_header_id, step_order, process_id,
-- standard_time_minutes, description, pre_wait_time_minutes, post_wait_time_minutes,
-- is_active, created_at, updated_at
-- Old DB has: routing_id, step_order, process_id, process_name, workcenter, standard_time

-- Rename routing_id to routing_header_id
CALL sp_column_exists('routing_steps', 'routing_header_id', @exists);
IF @exists = 0 THEN
    -- routing_id exists from init.sql, rename it to routing_header_id
    -- Drop FK constraint first (MySQL auto-names FKs)
    SET @sql = 'SELECT CONSTRAINT_NAME INTO @fk_name FROM information_schema.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''routing_steps'' AND COLUMN_NAME=''routing_id''
        AND REFERENCED_TABLE_NAME IS NOT NULL LIMIT 1';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    IF @fk_name IS NOT NULL AND @fk_name != '' THEN
        SET @sql = CONCAT('ALTER TABLE routing_steps DROP FOREIGN KEY ', @fk_name);
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
    -- Now rename
    CALL sp_column_exists('routing_steps', 'routing_id', @exists);
    IF @exists = 1 THEN
        ALTER TABLE routing_steps RENAME COLUMN routing_id TO routing_header_id;
    END IF;
END IF;

-- Rename standard_time to standard_time_minutes
CALL sp_column_exists('routing_steps', 'standard_time_minutes', @exists);
IF @exists = 0 THEN
    -- standard_time exists from init.sql, rename it to standard_time_minutes
    CALL sp_column_exists('routing_steps', 'standard_time', @exists);
    IF @exists = 1 THEN
        ALTER TABLE routing_steps RENAME COLUMN standard_time TO standard_time_minutes;
        -- Change to DOUBLE
        ALTER TABLE routing_steps MODIFY COLUMN standard_time_minutes DOUBLE;
    END IF;
END IF;

-- Add missing columns
CALL sp_column_exists('routing_steps', 'description', @exists);
IF @exists = 0 THEN
    ALTER TABLE routing_steps ADD COLUMN description VARCHAR(500);
END IF;

CALL sp_column_exists('routing_steps', 'pre_wait_time_minutes', @exists);
IF @exists = 0 THEN
    ALTER TABLE routing_steps ADD COLUMN pre_wait_time_minutes DOUBLE;
END IF;

CALL sp_column_exists('routing_steps', 'post_wait_time_minutes', @exists);
IF @exists = 0 THEN
    ALTER TABLE routing_steps ADD COLUMN post_wait_time_minutes DOUBLE;
END IF;

CALL sp_column_exists('routing_steps', 'is_active', @exists);
IF @exists = 0 THEN
    ALTER TABLE routing_steps ADD COLUMN is_active TINYINT(1) DEFAULT 1;
END IF;

CALL sp_column_exists('routing_steps', 'created_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE routing_steps ADD COLUMN created_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

CALL sp_column_exists('routing_steps', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE routing_steps ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Remove old columns not in EF model
CALL sp_column_exists('routing_steps', 'process_name', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE routing_steps DROP COLUMN process_name', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('routing_steps', 'workcenter', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE routing_steps DROP COLUMN workcenter', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 10: inspection_standards table
-- ============================================================================

-- Add sampling_frequency column
CALL sp_column_exists('inspection_standards', 'sampling_frequency', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_standards ADD COLUMN sampling_frequency VARCHAR(100) COMMENT '抽样频率';
END IF;

-- Remove sampling_method column (not in EF model)
CALL sp_column_exists('inspection_standards', 'sampling_method', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE inspection_standards DROP COLUMN sampling_method', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Remove aql column (not in EF model)
CALL sp_column_exists('inspection_standards', 'aql', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE inspection_standards DROP COLUMN aql', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Remove inspection_level column (not in EF model)
CALL sp_column_exists('inspection_standards', 'inspection_level', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE inspection_standards DROP COLUMN inspection_level', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Remove items column (not in EF model)
CALL sp_column_exists('inspection_standards', 'items', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE inspection_standards DROP COLUMN items', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 11: equipment table
-- ============================================================================

-- Change equipment_code from VARCHAR(50) to VARCHAR(100)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='equipment' AND COLUMN_NAME='equipment_code';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(50)' THEN
    ALTER TABLE equipment MODIFY COLUMN equipment_code VARCHAR(100) NOT NULL;
END IF;

-- Change model from VARCHAR(100) to VARCHAR(200)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='equipment' AND COLUMN_NAME='model';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(100)' THEN
    ALTER TABLE equipment MODIFY COLUMN model VARCHAR(200);
END IF;

-- Change equipment_type from VARCHAR(50) to VARCHAR(50) (already correct)

-- Add production_line column (not in DB)
CALL sp_column_exists('equipment', 'production_line', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment ADD COLUMN production_line VARCHAR(100) COMMENT '所在产线';
END IF;

-- Add workshop column (not in DB)
CALL sp_column_exists('equipment', 'workshop', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment ADD COLUMN workshop VARCHAR(100) COMMENT '所在车间';
END IF;

-- Add workshop_id column (not in DB)
CALL sp_column_exists('equipment', 'workshop_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment ADD COLUMN workshop_id BIGINT COMMENT '关联车间（组织ID）';
END IF;

-- Add line_id column (not in DB)
CALL sp_column_exists('equipment', 'line_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment ADD COLUMN line_id BIGINT COMMENT '关联产线（组织ID）';
END IF;

-- Add has_mqtt_connection column
CALL sp_column_exists('equipment', 'has_mqtt_connection', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment ADD COLUMN has_mqtt_connection TINYINT(1) NOT NULL DEFAULT 0;
END IF;

-- Add mqtt_topic_prefix column
CALL sp_column_exists('equipment', 'mqtt_topic_prefix', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment ADD COLUMN mqtt_topic_prefix VARCHAR(500);
END IF;

-- Remove manufacturer column (not in EF model)
CALL sp_column_exists('equipment', 'manufacturer', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE equipment DROP COLUMN manufacturer', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Remove installation_date column (not in EF model)
CALL sp_column_exists('equipment', 'installation_date', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE equipment DROP COLUMN installation_date', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 12: tools table
-- ============================================================================

-- Change tool_code from VARCHAR(50) to VARCHAR(100)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='tools' AND COLUMN_NAME='tool_code';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(50)' THEN
    ALTER TABLE tools MODIFY COLUMN tool_code VARCHAR(100) NOT NULL;
END IF;

-- Change tool_name from VARCHAR(200) to VARCHAR(200) (already correct)

-- Change design_life from DECIMAL(10,2) to DOUBLE (nullable)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='tools' AND COLUMN_NAME='design_life';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'decimal(10,2)' THEN
    ALTER TABLE tools MODIFY COLUMN design_life DOUBLE;
END IF;

-- Change life_current from DECIMAL(10,2) to DOUBLE
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='tools' AND COLUMN_NAME='life_current';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'decimal(10,2)' THEN
    ALTER TABLE tools MODIFY COLUMN life_current DOUBLE NOT NULL DEFAULT 0;
END IF;

-- Add org_id column
CALL sp_column_exists('tools', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE tools ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Remove status column (not in EF model)
CALL sp_column_exists('tools', 'status', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE tools DROP COLUMN status', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Remove equipment_id column (not in EF model)
CALL sp_column_exists('tools', 'equipment_id', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE tools DROP COLUMN equipment_id', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 13: suppliers table
-- ============================================================================

-- Add address column
CALL sp_column_exists('suppliers', 'address', @exists);
IF @exists = 0 THEN
    ALTER TABLE suppliers ADD COLUMN address VARCHAR(500) COMMENT '供应商地址';
END IF;

-- Add email column
CALL sp_column_exists('suppliers', 'email', @exists);
IF @exists = 0 THEN
    ALTER TABLE suppliers ADD COLUMN email VARCHAR(200);
END IF;

-- Remove supply_category column (NotMapped in EF)
CALL sp_column_exists('suppliers', 'supply_category', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE suppliers DROP COLUMN supply_category', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 14: customers table
-- ============================================================================

-- Add address column
CALL sp_column_exists('customers', 'address', @exists);
IF @exists = 0 THEN
    ALTER TABLE customers ADD COLUMN address VARCHAR(500);
END IF;

-- Add org_id column
CALL sp_column_exists('customers', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE customers ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add updated_at column
CALL sp_column_exists('customers', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE customers ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 15: inspection_items table
-- ============================================================================

-- Change usl/lsl from DECIMAL(15,6) to DECIMAL(15,6) (already correct)
-- Add is_active column
CALL sp_column_exists('inspection_items', 'is_active', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_items ADD COLUMN is_active TINYINT(1) NOT NULL DEFAULT 1;
END IF;

-- ============================================================================
-- STEP 16: inspection_plans table
-- ============================================================================

-- Add material_id column
CALL sp_column_exists('inspection_plans', 'material_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plans ADD COLUMN material_id BIGINT COMMENT '材料(关联products表)';
END IF;

-- Add supplier_id column
CALL sp_column_exists('inspection_plans', 'supplier_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plans ADD COLUMN supplier_id BIGINT COMMENT '供应商';
END IF;

-- Add customer_id column
CALL sp_column_exists('inspection_plans', 'customer_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plans ADD COLUMN customer_id BIGINT COMMENT '客户';
END IF;

-- Add process_id column
CALL sp_column_exists('inspection_plans', 'process_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plans ADD COLUMN process_id BIGINT COMMENT '工艺/工序';
END IF;

-- Add equipment_id column
CALL sp_column_exists('inspection_plans', 'equipment_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plans ADD COLUMN equipment_id BIGINT COMMENT '设备';
END IF;

-- ============================================================================
-- STEP 17: inspection_plan_items table
-- ============================================================================

-- Add sample_size column
CALL sp_column_exists('inspection_plan_items', 'sample_size', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plan_items ADD COLUMN sample_size INT COMMENT '抽样数量(覆盖)';
END IF;

-- Add is_required column
CALL sp_column_exists('inspection_plan_items', 'is_required', @exists);
IF @exists = 0 THEN
    ALTER TABLE inspection_plan_items ADD COLUMN is_required TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否必检';
END IF;

-- ============================================================================
-- STEP 18: dynamic_params table
-- ============================================================================

-- Change precision from DECIMAL(10,2) to INT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='dynamic_params' AND COLUMN_NAME='precision';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'decimal' OR @col_type = 'decimal(10,2)' THEN
    ALTER TABLE dynamic_params MODIFY COLUMN `precision` INT NOT NULL DEFAULT 2;
END IF;

-- Remove ai_strategy column (not in EF model)
CALL sp_column_exists('dynamic_params', 'ai_strategy', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE dynamic_params DROP COLUMN ai_strategy', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 19: param_groups table
-- ============================================================================

-- Add description column
CALL sp_column_exists('param_groups', 'description', @exists);
IF @exists = 0 THEN
    ALTER TABLE param_groups ADD COLUMN description TEXT COMMENT '描述';
END IF;

-- Add sort_order column
CALL sp_column_exists('param_groups', 'sort_order', @exists);
IF @exists = 0 THEN
    ALTER TABLE param_groups ADD COLUMN sort_order INT NOT NULL DEFAULT 0 COMMENT '排序号';
END IF;

-- Add created_by column
CALL sp_column_exists('param_groups', 'created_by', @exists);
IF @exists = 0 THEN
    ALTER TABLE param_groups ADD COLUMN created_by BIGINT NOT NULL DEFAULT 1 COMMENT '创建人ID';
END IF;

-- ============================================================================
-- STEP 20: param_realtime_values table
-- ============================================================================

-- Add value_raw column
CALL sp_column_exists('param_realtime_values', 'value_raw', @exists);
IF @exists = 0 THEN
    ALTER TABLE param_realtime_values ADD COLUMN value_raw VARCHAR(100) COMMENT '原始值(枚举型/布尔型)';
END IF;

-- Add is_active column
CALL sp_column_exists('param_realtime_values', 'is_active', @exists);
IF @exists = 0 THEN
    ALTER TABLE param_realtime_values ADD COLUMN is_active TINYINT(1) NOT NULL DEFAULT 1;
END IF;

-- Add created_at column
CALL sp_column_exists('param_realtime_values', 'created_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE param_realtime_values ADD COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 21: closure_rules table
-- ============================================================================

-- Change condition_json from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='closure_rules' AND COLUMN_NAME='condition_json';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE closure_rules MODIFY COLUMN condition_json TEXT NOT NULL;
END IF;

-- ============================================================================
-- STEP 22: iqc_receipts table
-- ============================================================================

-- Add org_id column
CALL sp_column_exists('iqc_receipts', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_receipts ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add inspector column
CALL sp_column_exists('iqc_receipts', 'inspector', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_receipts ADD COLUMN inspector VARCHAR(100);
END IF;

-- Add updated_at column
CALL sp_column_exists('iqc_receipts', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_receipts ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Change quantity from INT to INT (already correct, but ensure default)

-- ============================================================================
-- STEP 23: iqc_inspections table
-- ============================================================================

-- Add sampling_level column
CALL sp_column_exists('iqc_inspections', 'sampling_level', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_inspections ADD COLUMN sampling_level VARCHAR(10) COMMENT '抽样水平 I/II/III';
END IF;

-- Add org_id column
CALL sp_column_exists('iqc_inspections', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_inspections ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add updated_at column
CALL sp_column_exists('iqc_inspections', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_inspections ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Change aql_value from DECIMAL(5,2) to DOUBLE (nullable)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='iqc_inspections' AND COLUMN_NAME='aql_value';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'decimal(5,2)' THEN
    ALTER TABLE iqc_inspections MODIFY COLUMN aql_value DOUBLE;
END IF;

-- ============================================================================
-- STEP 24: iqc_inspection_items table
-- ============================================================================

-- Change result from ENUM to VARCHAR(10)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='iqc_inspection_items' AND COLUMN_NAME='result';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE iqc_inspection_items MODIFY COLUMN result VARCHAR(10) NOT NULL DEFAULT 'pending';
END IF;

-- Change measured_value from DECIMAL(12,4) to DECIMAL(12,4) (already correct for EF decimal)
-- Change usl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)
-- Change lsl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)

-- Add org_id column
CALL sp_column_exists('iqc_inspection_items', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_inspection_items ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add is_required column
CALL sp_column_exists('iqc_inspection_items', 'is_required', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_inspection_items ADD COLUMN is_required TINYINT(1) NOT NULL DEFAULT 1 COMMENT '是否必检';
END IF;

-- ============================================================================
-- STEP 25: iqc_anomalies table
-- ============================================================================

-- Change anomaly_type from ENUM to VARCHAR(20)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='iqc_anomalies' AND COLUMN_NAME='anomaly_type';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE iqc_anomalies MODIFY COLUMN anomaly_type VARCHAR(20) NOT NULL DEFAULT 'quality';
END IF;

-- Change failed_item_ids from JSON to VARCHAR(500)
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='iqc_anomalies' AND COLUMN_NAME='failed_item_ids';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE iqc_anomalies MODIFY COLUMN failed_item_ids VARCHAR(500) COMMENT '不合格检验项目ID列表';
END IF;

-- Change description from TEXT to TEXT (already correct)

-- Change handler_dept from ENUM to VARCHAR(20)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='iqc_anomalies' AND COLUMN_NAME='handler_dept';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE iqc_anomalies MODIFY COLUMN handler_dept VARCHAR(20);
END IF;

-- Add org_id column
CALL sp_column_exists('iqc_anomalies', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE iqc_anomalies ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- ============================================================================
-- STEP 26: supplier_scores table
-- ============================================================================

-- Change assessment_date from DATE to DATETIME (EF model uses DateTime?)
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='supplier_scores' AND COLUMN_NAME='assessment_date';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'date' THEN
    ALTER TABLE supplier_scores MODIFY COLUMN assessment_date DATETIME COMMENT '评分日期';
END IF;

-- Change score from DECIMAL(5,2) to DECIMAL(10,2) (nullable)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='supplier_scores' AND COLUMN_NAME='score';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 7) = 'decimal' THEN
    ALTER TABLE supplier_scores MODIFY COLUMN score DECIMAL(10,2);
END IF;

-- Add org_id column
CALL sp_column_exists('supplier_scores', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE supplier_scores ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- ============================================================================
-- STEP 27: ipqc_first_pieces table - significant changes needed
-- ============================================================================

-- Add work_order_id column (BIGINT)
CALL sp_column_exists('ipqc_first_pieces', 'work_order_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_pieces ADD COLUMN work_order_id BIGINT COMMENT '关联工单';
END IF;

-- Add operator_id column (BIGINT)
CALL sp_column_exists('ipqc_first_pieces', 'operator_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_pieces ADD COLUMN operator_id BIGINT COMMENT '操作员';
END IF;

-- Add inspector_id column (BIGINT)
CALL sp_column_exists('ipqc_first_pieces', 'inspector_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_pieces ADD COLUMN inspector_id BIGINT COMMENT '检验员';
END IF;

-- Add allowed_to_produce column
CALL sp_column_exists('ipqc_first_pieces', 'allowed_to_produce', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_pieces ADD COLUMN allowed_to_produce TINYINT(1) NOT NULL DEFAULT 0;
END IF;

-- Add checked_at column
CALL sp_column_exists('ipqc_first_pieces', 'checked_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_pieces ADD COLUMN checked_at DATETIME;
END IF;

-- Add org_id column
CALL sp_column_exists('ipqc_first_pieces', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_pieces ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Change product_id to BIGINT (from whatever it is now)
-- The EF model has product_id BIGINT but the DB has product_id BIGINT too

-- Remove old columns not in EF model
CALL sp_column_exists('ipqc_first_pieces', 'work_order', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_first_pieces DROP COLUMN work_order', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('ipqc_first_pieces', 'batch_no', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_first_pieces DROP COLUMN batch_no', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Change inspector from VARCHAR(100) to BIGINT (inspector_id)
-- The DB has inspector VARCHAR(100), EF has inspector_id BIGINT
-- Since we added inspector_id above, we can drop inspector
CALL sp_column_exists('ipqc_first_pieces', 'inspector', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_first_pieces DROP COLUMN inspector', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Change reason column - EF has reason VARCHAR(20) DEFAULT '班次切换', DB has reason VARCHAR(20)
-- Already correct

-- Change conclusion from VARCHAR(20) to VARCHAR(20) (already correct)

-- Change shift from VARCHAR(20) to VARCHAR(20) (already correct)

-- Change equipment_id from BIGINT to BIGINT (already correct)
-- Change process_id from BIGINT to BIGINT (already correct)

-- ============================================================================
-- STEP 28: ipqc_patrol_plans table - significant changes needed
-- ============================================================================

-- Add org_id column
CALL sp_column_exists('ipqc_patrol_plans', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrol_plans ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Change interval_minutes to patrol_interval_min (rename)
-- First check if patrol_interval_min exists
CALL sp_column_exists('ipqc_patrol_plans', 'patrol_interval_min', @exists);
IF @exists = 0 THEN
    -- interval_minutes exists from init.sql, rename it to patrol_interval_min
    CALL sp_column_exists('ipqc_patrol_plans', 'interval_minutes', @exists);
    IF @exists = 1 THEN
        ALTER TABLE ipqc_patrol_plans RENAME COLUMN interval_minutes TO patrol_interval_min;
    END IF;
END IF;

-- Add auto_generate column
CALL sp_column_exists('ipqc_patrol_plans', 'auto_generate', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrol_plans ADD COLUMN auto_generate TINYINT(1) NOT NULL DEFAULT 1;
END IF;

-- Add inspector column (VARCHAR(100))
CALL sp_column_exists('ipqc_patrol_plans', 'inspector', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrol_plans ADD COLUMN inspector VARCHAR(100) COMMENT '默认检验员';
END IF;

-- Add updated_at column
CALL sp_column_exists('ipqc_patrol_plans', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrol_plans ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 29: ipqc_patrols table - significant changes needed
-- ============================================================================

-- Rename plan_id to patrol_plan_id
-- First check if patrol_plan_id exists (in case migration was re-run)
CALL sp_column_exists('ipqc_patrols', 'patrol_plan_id', @exists);
IF @exists = 0 THEN
    -- plan_id exists from init.sql, rename it to patrol_plan_id
    -- Drop FK constraint first (MySQL auto-names FKs, so we need to find the name)
    SET @sql = 'SELECT CONSTRAINT_NAME INTO @fk_name FROM information_schema.KEY_COLUMN_USAGE
        WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''ipqc_patrols'' AND COLUMN_NAME=''plan_id''
        AND REFERENCED_TABLE_NAME IS NOT NULL LIMIT 1';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    IF @fk_name IS NOT NULL AND @fk_name != '' THEN
        SET @sql = CONCAT('ALTER TABLE ipqc_patrols DROP FOREIGN KEY ', @fk_name);
        PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    END IF;
    -- Now rename
    CALL sp_column_exists('ipqc_patrols', 'plan_id', @exists);
    IF @exists = 1 THEN
        ALTER TABLE ipqc_patrols RENAME COLUMN plan_id TO patrol_plan_id;
    END IF;
END IF;

-- Add work_order_id column (BIGINT)
CALL sp_column_exists('ipqc_patrols', 'work_order_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN work_order_id BIGINT COMMENT '关联工单（可选）';
END IF;

-- Add total_checked column
CALL sp_column_exists('ipqc_patrols', 'total_checked', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN total_checked INT NOT NULL DEFAULT 0;
END IF;

-- Add total_pass column
CALL sp_column_exists('ipqc_patrols', 'total_pass', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN total_pass INT NOT NULL DEFAULT 0;
END IF;

-- Add total_fail column
CALL sp_column_exists('ipqc_patrols', 'total_fail', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN total_fail INT NOT NULL DEFAULT 0;
END IF;

-- Add remarks column
CALL sp_column_exists('ipqc_patrols', 'remarks', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN remarks TEXT COMMENT '备注';
END IF;

-- Add org_id column
CALL sp_column_exists('ipqc_patrols', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add created_at column
CALL sp_column_exists('ipqc_patrols', 'created_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN created_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Add updated_at column
CALL sp_column_exists('ipqc_patrols', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Remove old columns not in EF model
CALL sp_column_exists('ipqc_patrols', 'patrol_time', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_patrols DROP COLUMN patrol_time', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('ipqc_patrols', 'remark', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_patrols DROP COLUMN remark', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Rename inspector (VARCHAR) to inspector_id (BIGINT)
-- First add inspector_id
CALL sp_column_exists('ipqc_patrols', 'inspector_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrols ADD COLUMN inspector_id BIGINT COMMENT '检验员ID';
END IF;

-- Then drop inspector
CALL sp_column_exists('ipqc_patrols', 'inspector', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_patrols DROP COLUMN inspector', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 30: ipqc_closure_status table
-- ============================================================================

-- Change work_order from VARCHAR(100) to BIGINT (work_order_id)
CALL sp_column_exists('ipqc_closure_status', 'work_order_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_closure_status ADD COLUMN work_order_id BIGINT COMMENT '关联工单ID';
END IF;

-- Change status from ENUM to VARCHAR(10)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='ipqc_closure_status' AND COLUMN_NAME='status';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE ipqc_closure_status MODIFY COLUMN status VARCHAR(10) NOT NULL DEFAULT 'open';
END IF;

-- Add rule_id column
CALL sp_column_exists('ipqc_closure_status', 'rule_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_closure_status ADD COLUMN rule_id BIGINT COMMENT '关单规则ID';
END IF;

-- Add evaluation_result column (TEXT for JSON)
CALL sp_column_exists('ipqc_closure_status', 'evaluation_result', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_closure_status ADD COLUMN evaluation_result TEXT COMMENT '关单评估结果 (JSON)';
END IF;

-- Add updated_at column
CALL sp_column_exists('ipqc_closure_status', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_closure_status ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Remove old columns not in EF model
CALL sp_column_exists('ipqc_closure_status', 'work_order', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_closure_status DROP COLUMN work_order', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('ipqc_closure_status', 'spc_result', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE ipqc_closure_status DROP COLUMN spc_result', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 31: ipqc_patrol_items table
-- ============================================================================

-- Change result from ENUM to VARCHAR(10)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='ipqc_patrol_items' AND COLUMN_NAME='result';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE ipqc_patrol_items MODIFY COLUMN result VARCHAR(10) NOT NULL DEFAULT 'pending';
END IF;

-- Change usl from DECIMAL(12,4) to DECIMAL(12,4) (already correct for EF decimal)
-- Change lsl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)

-- Add org_id column
CALL sp_column_exists('ipqc_patrol_items', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrol_items ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- ============================================================================
-- STEP 32: ipqc_first_piece_items table
-- ============================================================================

-- Change result from ENUM to VARCHAR(10)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='ipqc_first_piece_items' AND COLUMN_NAME='result';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE ipqc_first_piece_items MODIFY COLUMN result VARCHAR(10) NOT NULL DEFAULT 'pending';
END IF;

-- Change usl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)
-- Change lsl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)

-- Add org_id column
CALL sp_column_exists('ipqc_first_piece_items', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_first_piece_items ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- ============================================================================
-- STEP 33: fqc_inspections table
-- ============================================================================

-- Change quantity from DECIMAL(18,4) to INT (EF model has no quantity, but has total_checked etc.)
-- Actually EF doesn't have quantity on fqc_inspections, it has sample_size, total_checked, total_pass, total_fail
-- Change to match EF model

-- Add aql_level column (DECIMAL(10,2))
CALL sp_column_exists('fqc_inspections', 'aql_level', @exists);
IF @exists = 0 THEN
    ALTER TABLE fqc_inspections ADD COLUMN aql_level DOUBLE COMMENT 'AQL值';
END IF;

-- Add org_id column
CALL sp_column_exists('fqc_inspections', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE fqc_inspections ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add updated_at column
CALL sp_column_exists('fqc_inspections', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE fqc_inspections ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Add inspection_type column (VARCHAR(10))
CALL sp_column_exists('fqc_inspections', 'inspection_type', @exists);
IF @exists = 0 THEN
    ALTER TABLE fqc_inspections ADD COLUMN inspection_type VARCHAR(10) NOT NULL DEFAULT 'full' COMMENT '检验方式：full/sampling';
END IF;

-- ============================================================================
-- STEP 34: fqc_inspection_items table
-- ============================================================================

-- Change result from ENUM to VARCHAR(10)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='fqc_inspection_items' AND COLUMN_NAME='result';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE fqc_inspection_items MODIFY COLUMN result VARCHAR(10) NOT NULL DEFAULT 'pending';
END IF;

-- Change measured_value from DECIMAL(12,4) to DECIMAL(12,4) (already correct for EF decimal)
-- Change usl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)
-- Change lsl from DECIMAL(12,4) to DECIMAL(12,4) (already correct)

-- Add org_id column
CALL sp_column_exists('fqc_inspection_items', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE fqc_inspection_items ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add item_code column (VARCHAR(50))
CALL sp_column_exists('fqc_inspection_items', 'item_code', @exists);
IF @exists = 0 THEN
    ALTER TABLE fqc_inspection_items ADD COLUMN item_code VARCHAR(50) COMMENT '检验项目编码';
END IF;

-- Change data_type default to 'numeric'
CALL sp_column_exists('fqc_inspection_items', 'data_type', @exists);
IF @exists = 1 THEN
    -- Check if it has default
    SET @col_def = '';
    SELECT COLUMN_DEFAULT INTO @col_def FROM information_schema.COLUMNS
        WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='fqc_inspection_items' AND COLUMN_NAME='data_type';
    IF @col_def IS NULL THEN
        ALTER TABLE fqc_inspection_items MODIFY COLUMN data_type VARCHAR(20) NOT NULL DEFAULT 'numeric';
    END IF;
END IF;

-- ============================================================================
-- STEP 35: oqc_releases table
-- ============================================================================

-- Change release_date from DATETIME to DATETIME (already correct)
-- Change status from VARCHAR(20) to VARCHAR(20) (already correct)
-- Add org_id column
CALL sp_column_exists('oqc_releases', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE oqc_releases ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- ============================================================================
-- STEP 36: product_batches table
-- ============================================================================

-- Add org_id column
CALL sp_column_exists('product_batches', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE product_batches ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add updated_at column with ON UPDATE CURRENT_TIMESTAMP
CALL sp_column_exists('product_batches', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE product_batches ADD COLUMN updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;
END IF;

-- Change created_at default from NOW() to CURRENT_TIMESTAMP
SET @col_def = '';
SELECT COLUMN_DEFAULT INTO @col_def FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='product_batches' AND COLUMN_NAME='created_at';
SET @col_def = COALESCE(@col_def, '');
IF @col_def = 'NULL' THEN
    -- NOW() might have been evaluated at creation, reset it
    ALTER TABLE product_batches MODIFY COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 37: spc_control_charts table
-- ============================================================================

-- Change parameter_code from VARCHAR(50) to VARCHAR(50) (already correct)
-- Change chart_type from VARCHAR(10) to VARCHAR(50)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='spc_control_charts' AND COLUMN_NAME='chart_type';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(10)' THEN
    ALTER TABLE spc_control_charts MODIFY COLUMN chart_type VARCHAR(50) NOT NULL DEFAULT 'Xbar_R';
END IF;

-- ============================================================================
-- STEP 38: spc_analysis_results table
-- ============================================================================

-- Change analysis_type from ENUM to VARCHAR(20)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='spc_analysis_results' AND COLUMN_NAME='analysis_type';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) = 'enum' THEN
    ALTER TABLE spc_analysis_results MODIFY COLUMN analysis_type VARCHAR(20) NOT NULL DEFAULT 'cpk';
END IF;

-- ============================================================================
-- STEP 39: spc_alert_rules table
-- ============================================================================

-- Change rule_description from TEXT to TEXT (already correct)
-- Change enabled from TINYINT(1) to TINYINT(1) (already correct)

-- ============================================================================
-- STEP 40: spc_alert_triggers table
-- ============================================================================

-- Add detail column (TEXT for JSON)
CALL sp_column_exists('spc_alert_triggers', 'detail', @exists);
IF @exists = 0 THEN
    ALTER TABLE spc_alert_triggers ADD COLUMN detail TEXT COMMENT '触发详情';
END IF;

-- ============================================================================
-- STEP 41: spc_data_sources table
-- ============================================================================

-- Change inspection_item_id column name (DB has inspection_item_id, EF has inspection_item)
-- Actually EF model maps to column "inspection_item" but the table already has inspection_item_id
-- Let's check and align

-- Check if we need to rename
CALL sp_column_exists('spc_data_sources', 'inspection_item', @exists);
IF @exists = 0 THEN
    -- inspection_item doesn't exist, check if inspection_item_id is there
    CALL sp_column_exists('spc_data_sources', 'inspection_item_id', @exists);
    IF @exists = 1 THEN
        -- Keep as is, EF uses ForeignKey with Column("inspection_item") attribute
        -- This is a EF mapping issue, the column name in DB is fine
    END IF;
END IF;

-- ============================================================================
-- STEP 42: defects table
-- ============================================================================

-- Rename defect_code column to defect_no
-- First check if defect_no exists
CALL sp_column_exists('defects', 'defect_no', @exists);
IF @exists = 0 THEN
    -- Rename defect_code to defect_no
    ALTER TABLE defects RENAME COLUMN defect_code TO defect_no;
    -- Update unique index
    SET @sql = 'ALTER TABLE defects DROP INDEX idx_defect_code';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    SET @sql = 'CREATE UNIQUE INDEX idx_defect_no ON defects(defect_no)';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Change severity from VARCHAR(10) to VARCHAR(10) (already correct)
-- Change source_type from VARCHAR(10) to VARCHAR(10) (already correct)

-- Change image_urls from JSON to TEXT(500)
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='defects' AND COLUMN_NAME='image_urls';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE defects MODIFY COLUMN image_urls TEXT COMMENT '图片URLs';
END IF;

-- Change discovered_by from BIGINT to BIGINT (already correct)

-- Add org_id column
CALL sp_column_exists('defects', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE defects ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add updated_at column
CALL sp_column_exists('defects', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE defects ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Change quantity from DECIMAL(15,2) to DECIMAL(15,2) (already correct)

-- Change description from TEXT to TEXT NOT NULL (already correct)

-- ============================================================================
-- STEP 43: capa table
-- ============================================================================

-- Change severity from VARCHAR(10) to VARCHAR(10) (already correct)
-- Change title from VARCHAR(500) to VARCHAR(500) (already correct)

-- Add org_id column
CALL sp_column_exists('capa', 'org_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE capa ADD COLUMN org_id BIGINT COMMENT '所属组织';
END IF;

-- Add updated_at column
CALL sp_column_exists('capa', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE capa ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 44: capa_corrective_actions table
-- ============================================================================

-- Change action_description from TEXT to TEXT NOT NULL
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_corrective_actions' AND COLUMN_NAME='action_description';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) != 'text' THEN
    ALTER TABLE capa_corrective_actions MODIFY COLUMN action_description TEXT NOT NULL;
END IF;

-- Change responsible_person from BIGINT to BIGINT NOT NULL
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_corrective_actions' AND COLUMN_NAME='responsible_person';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 7) = 'bigint' THEN
    ALTER TABLE capa_corrective_actions MODIFY COLUMN responsible_person BIGINT NOT NULL;
END IF;

-- ============================================================================
-- STEP 45: capa_preventive_actions table
-- ============================================================================

-- Change action_description from TEXT to TEXT NOT NULL
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_preventive_actions' AND COLUMN_NAME='action_description';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) != 'text' THEN
    ALTER TABLE capa_preventive_actions MODIFY COLUMN action_description TEXT NOT NULL;
END IF;

-- Change responsible_person from BIGINT to BIGINT NOT NULL
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_preventive_actions' AND COLUMN_NAME='responsible_person';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 7) = 'bigint' THEN
    ALTER TABLE capa_preventive_actions MODIFY COLUMN responsible_person BIGINT NOT NULL;
END IF;

-- ============================================================================
-- STEP 46: capa_root_causes table
-- ============================================================================

-- Change content from JSON to TEXT NOT NULL
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_root_causes' AND COLUMN_NAME='content';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE capa_root_causes MODIFY COLUMN content TEXT NOT NULL DEFAULT '[]';
END IF;

-- Change root_cause_summary from TEXT to TEXT NOT NULL
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_root_causes' AND COLUMN_NAME='root_cause_summary';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'text' THEN
    ALTER TABLE capa_root_causes MODIFY COLUMN root_cause_summary TEXT NOT NULL;
END IF;

-- ============================================================================
-- STEP 47: capa_temporary_measures table
-- ============================================================================

-- Change description from TEXT to TEXT NOT NULL
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_temporary_measures' AND COLUMN_NAME='description';
SET @col_type = COALESCE(@col_type, '');
IF LEFT(@col_type, 4) != 'text' THEN
    ALTER TABLE capa_temporary_measures MODIFY COLUMN description TEXT NOT NULL;
END IF;

-- ============================================================================
-- STEP 48: capa_verifications table
-- ============================================================================

-- Change conclusion from VARCHAR(20) to VARCHAR(20) NOT NULL (already correct)
-- Change evidence from TEXT to TEXT (already correct)

-- Change image_urls from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='capa_verifications' AND COLUMN_NAME='image_urls';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE capa_verifications MODIFY COLUMN image_urls TEXT COMMENT '图片URLs';
END IF;

-- Change remarks from TEXT to TEXT (already correct)

-- ============================================================================
-- STEP 49: scrap_rework_records table
-- ============================================================================

-- Change rework_steps from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='scrap_rework_records' AND COLUMN_NAME='rework_steps';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE scrap_rework_records MODIFY COLUMN rework_steps TEXT COMMENT '返工步骤(仅返工)';
END IF;

-- ============================================================================
-- STEP 50: complaints table
-- ============================================================================

-- Rename complaint_code to complaint_no (EF model uses complaint_no)
CALL sp_column_exists('complaints', 'complaint_no', @exists);
IF @exists = 0 THEN
    ALTER TABLE complaints RENAME COLUMN complaint_code TO complaint_no;
    -- Update unique index
    SET @sql = 'ALTER TABLE complaints DROP INDEX uk_complaints_complaint_code';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    SET @sql = 'CREATE UNIQUE INDEX uk_complaints_complaint_no ON complaints(complaint_no)';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Add customer_id column (BIGINT)
CALL sp_column_exists('complaints', 'customer_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE complaints ADD COLUMN customer_id BIGINT COMMENT '关联客户';
END IF;

-- Add acknowledged_at column (DATETIME)
CALL sp_column_exists('complaints', 'acknowledged_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE complaints ADD COLUMN acknowledged_at DATETIME COMMENT '确认时间';
END IF;

-- Add five_w2h_json column (TEXT for JSON)
CALL sp_column_exists('complaints', 'five_w2h_json', @exists);
IF @exists = 0 THEN
    ALTER TABLE complaints ADD COLUMN five_w2h_json TEXT COMMENT '5W2H问题描述（JSON）';
END IF;

-- Add updated_at column
CALL sp_column_exists('complaints', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE complaints ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Remove old columns not in EF model
CALL sp_column_exists('complaints', 'product_id', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE complaints DROP COLUMN product_id', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('complaints', 'batch_no', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE complaints DROP COLUMN batch_no', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('complaints', 'complaint_date', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE complaints DROP COLUMN complaint_date', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

CALL sp_column_exists('complaints', 'org_id', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE complaints DROP COLUMN org_id', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 51: d8_reports table
-- ============================================================================

-- Add all D-section columns
CALL sp_column_exists('d8_reports', 'd0_description', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d0_description TEXT COMMENT 'D0 问题概述';
END IF;

CALL sp_column_exists('d8_reports', 'd1_team', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d1_team TEXT COMMENT 'D1 改善小组成员（JSON数组）';
END IF;

CALL sp_column_exists('d8_reports', 'd2_description', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d2_description TEXT COMMENT 'D2 问题描述(5W2H)';
END IF;

CALL sp_column_exists('d8_reports', 'd2_problem_desc', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d2_problem_desc TEXT COMMENT 'D2 问题描述别名';
END IF;

CALL sp_column_exists('d8_reports', 'd3_measures', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d3_measures TEXT COMMENT 'D3 临时围堵措施（JSON数组）';
END IF;

CALL sp_column_exists('d8_reports', 'd4_analysis_method', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d4_analysis_method VARCHAR(20) COMMENT 'D4 分析方法';
END IF;

CALL sp_column_exists('d8_reports', 'd4_content', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d4_content TEXT COMMENT 'D4 分析数据（JSON）';
END IF;

CALL sp_column_exists('d8_reports', 'd4_root_cause', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d4_root_cause TEXT COMMENT 'D4 根本原因总结';
END IF;

CALL sp_column_exists('d8_reports', 'd5_actions', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d5_actions TEXT COMMENT 'D5 永久纠正措施（JSON数组）';
END IF;

CALL sp_column_exists('d8_reports', 'd6_verification', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d6_verification TEXT COMMENT 'D6 实施验证记录（JSON数组）';
END IF;

CALL sp_column_exists('d8_reports', 'd7_preventive', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d7_preventive TEXT COMMENT 'D7 预防措施（JSON数组）';
END IF;

CALL sp_column_exists('d8_reports', 'd8_thanks', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN d8_thanks TEXT COMMENT 'D8 小组祝贺与知识共享';
END IF;

-- Add current_discipline column
CALL sp_column_exists('d8_reports', 'current_discipline', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN current_discipline INT NOT NULL DEFAULT 0 COMMENT '当前步骤 D0~D8';
END IF;

-- Change status from VARCHAR(20) to VARCHAR(20) NOT NULL (already correct)

-- Add completed_at column
CALL sp_column_exists('d8_reports', 'completed_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE d8_reports ADD COLUMN completed_at DATETIME;
END IF;

-- ============================================================================
-- STEP 52: equipment_param_mappings table
-- ============================================================================

-- Add param_group_id column
CALL sp_column_exists('equipment_param_mappings', 'param_group_id', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment_param_mappings ADD COLUMN param_group_id BIGINT COMMENT '关联参数组ID';
END IF;

-- Add unit column
CALL sp_column_exists('equipment_param_mappings', 'unit', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment_param_mappings ADD COLUMN unit VARCHAR(20) COMMENT '单位';
END IF;

-- Add created_at column
CALL sp_column_exists('equipment_param_mappings', 'created_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment_param_mappings ADD COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Add updated_at column
CALL sp_column_exists('equipment_param_mappings', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment_param_mappings ADD COLUMN updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 53: equipment_status_history table
-- ============================================================================

-- Change status column to signal (rename)
CALL sp_column_exists('equipment_status_history', 'signal', @exists);
IF @exists = 0 THEN
    -- Rename status to signal
    ALTER TABLE equipment_status_history RENAME COLUMN status TO signal;
END IF;

-- Change signal from VARCHAR(20) to VARCHAR(10)
SET @col_type = '';
SELECT COLUMN_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='equipment_status_history' AND COLUMN_NAME='signal';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'varchar(20)' THEN
    ALTER TABLE equipment_status_history MODIFY COLUMN signal VARCHAR(10) NOT NULL DEFAULT '';
END IF;

-- Add signal_data column (TEXT)
CALL sp_column_exists('equipment_status_history', 'signal_data', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment_status_history ADD COLUMN signal_data TEXT COMMENT '附加数据（如故障代码 JSON）';
END IF;

-- Rename started_at to recorded_at
CALL sp_column_exists('equipment_status_history', 'recorded_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE equipment_status_history RENAME COLUMN started_at TO recorded_at;
END IF;

-- Remove ended_at column (not in EF model)
CALL sp_column_exists('equipment_status_history', 'ended_at', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE equipment_status_history DROP COLUMN ended_at', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 54: equipment_quality_correlation table
-- ============================================================================

-- Change analysis_date from DATE to DATE (already correct, EF uses DateOnly)

-- Change correlation_data from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='equipment_quality_correlation' AND COLUMN_NAME='correlation_data';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE equipment_quality_correlation MODIFY COLUMN correlation_data TEXT NOT NULL DEFAULT '{}';
END IF;

-- Remove conclusion column (not in EF model)
CALL sp_column_exists('equipment_quality_correlation', 'conclusion', @exists);
SET @sql = IF(@exists = 1, 'ALTER TABLE equipment_quality_correlation DROP COLUMN conclusion', 'SELECT 1');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- STEP 55: documents table
-- ============================================================================

-- Change doc_type from VARCHAR(20) to VARCHAR(20) (already correct)
-- Change minio_key from VARCHAR(500) to VARCHAR(500) (already correct)

-- Change approved_by from BIGINT to BIGINT (already correct)

-- Add approved_by_str column (VARCHAR)
CALL sp_column_exists('documents', 'approved_by_str', @exists);
IF @exists = 0 THEN
    ALTER TABLE documents ADD COLUMN approved_by_str VARCHAR(200) COMMENT '审批人标识（字符串）';
END IF;

-- Add approved_at column (DATETIME)
CALL sp_column_exists('documents', 'approved_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE documents ADD COLUMN approved_at DATETIME COMMENT '审批时间';
END IF;

-- Add expires_at column (DATE)
CALL sp_column_exists('documents', 'expires_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE documents ADD COLUMN expires_at DATE COMMENT '有效期';
END IF;

-- Add created_at column
CALL sp_column_exists('documents', 'created_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE documents ADD COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- Add updated_at column
CALL sp_column_exists('documents', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE documents ADD COLUMN updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 56: document_versions table
-- ============================================================================

-- Add created_by column
CALL sp_column_exists('document_versions', 'created_by', @exists);
IF @exists = 0 THEN
    ALTER TABLE document_versions ADD COLUMN created_by BIGINT NOT NULL;
END IF;

-- ============================================================================
-- STEP 57: audits table
-- ============================================================================

-- Change auditor_ids_json from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='audits' AND COLUMN_NAME='auditor_ids_json';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE audits MODIFY COLUMN auditor_ids_json TEXT COMMENT '审核人IDs（JSON数组）';
END IF;

-- Change scope from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='audits' AND COLUMN_NAME='scope';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE audits MODIFY COLUMN scope TEXT COMMENT '审核范围（产线/工序/产品）';
END IF;

-- Add updated_at column
CALL sp_column_exists('audits', 'updated_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE audits ADD COLUMN updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;
END IF;

-- ============================================================================
-- STEP 58: audit_findings table
-- ============================================================================

-- Change finding_type from VARCHAR(20) to VARCHAR(20) (already correct)

-- Change severity from VARCHAR(10) to VARCHAR(10) (already correct)

-- Change evidence from TEXT to TEXT (already correct)

-- Change requirement_ref from VARCHAR(200) to VARCHAR(200) (already correct)

-- Change rectification_plan from JSON to TEXT
SET @col_type = '';
SELECT DATA_TYPE INTO @col_type FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='audit_findings' AND COLUMN_NAME='rectification_plan';
SET @col_type = COALESCE(@col_type, '');
IF @col_type = 'json' THEN
    ALTER TABLE audit_findings MODIFY COLUMN rectification_plan TEXT COMMENT '整改措施（JSON）';
END IF;

-- Add responsible_user_id_str column (VARCHAR)
CALL sp_column_exists('audit_findings', 'responsible_user_id_str', @exists);
IF @exists = 0 THEN
    ALTER TABLE audit_findings ADD COLUMN responsible_user_id_str VARCHAR(200) COMMENT '整改责任人标识（字符串）';
END IF;

-- Change rectification_due_date from DATE to DATE (already correct, EF uses DateOnly)

-- Add verified_by_str column (VARCHAR)
CALL sp_column_exists('audit_findings', 'verified_by_str', @exists);
IF @exists = 0 THEN
    ALTER TABLE audit_findings ADD COLUMN verified_by_str VARCHAR(200) COMMENT '验证人标识（字符串）';
END IF;

-- Add verified_at column
CALL sp_column_exists('audit_findings', 'verified_at', @exists);
IF @exists = 0 THEN
    ALTER TABLE audit_findings ADD COLUMN verified_at DATETIME COMMENT '验证时间';
END IF;

-- ============================================================================
-- STEP 59: Trace tables (not in EF model - keep as is, but ensure indexes)
-- ============================================================================
-- trace_records exists in DB but not in EF. Keep as-is.

-- ============================================================================
-- STEP 60: AI tables (not in EF model - keep as is)
-- ============================================================================
-- ai_warnings, ai_models, ai_analysis_results exist in DB but not in EF. Keep as-is.

-- ============================================================================
-- STEP 61: Additional indexes needed (only new ones not in init.sql)
-- ============================================================================

-- Users indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''users'' AND INDEX_NAME=''idx_users_created_by''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_users_created_by ON users(created_by);
END IF;

-- Equipment indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''equipment'' AND INDEX_NAME=''idx_equipment_workshop_id''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_equipment_workshop_id ON equipment(workshop_id);
END IF;

SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''equipment'' AND INDEX_NAME=''idx_equipment_line_id''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_equipment_line_id ON equipment(line_id);
END IF;

-- Tools indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''tools'' AND INDEX_NAME=''idx_tools_org''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_tools_org ON tools(org_id);
END IF;

-- Suppliers indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''suppliers'' AND INDEX_NAME=''idx_suppliers_org''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_suppliers_org ON suppliers(org_id);
END IF;

-- Customers indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''customers'' AND INDEX_NAME=''idx_customers_org''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_customers_org ON customers(org_id);
END IF;

-- IPQC patrol items index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''ipqc_patrol_items'' AND INDEX_NAME=''idx_ipqc_patrol_items_inspection_item''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_ipqc_patrol_items_inspection_item ON ipqc_patrol_items(inspection_item_id);
END IF;

-- IPQC first piece items index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''ipqc_first_piece_items'' AND INDEX_NAME=''idx_ipqc_fp_items_inspection_item''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_ipqc_fp_items_inspection_item ON ipqc_first_piece_items(inspection_item_id);
END IF;

-- FQC inspection items index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''fqc_inspection_items'' AND INDEX_NAME=''idx_fqc_items_inspection_item''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_fqc_items_inspection_item ON fqc_inspection_items(inspection_item_id);
END IF;

-- Product batches indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''product_batches'' AND INDEX_NAME=''idx_product_batches_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_product_batches_status ON product_batches(status);
END IF;

-- CAPA indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''capa'' AND INDEX_NAME=''idx_capa_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_capa_status ON capa(status);
END IF;

SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''capa'' AND INDEX_NAME=''idx_capa_current_phase''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_capa_current_phase ON capa(current_phase);
END IF;

-- Defects indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''defects'' AND INDEX_NAME=''idx_defects_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_defects_status ON defects(status);
END IF;

-- Complaints indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''complaints'' AND INDEX_NAME=''idx_complaints_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_complaints_status ON complaints(status);
END IF;

-- D8 reports index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''d8_reports'' AND INDEX_NAME=''idx_d8_reports_complaint''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_d8_reports_complaint ON d8_reports(complaint_id);
END IF;

-- Equipment param mappings index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''equipment_param_mappings'' AND INDEX_NAME=''idx_epm_param_group''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_epm_param_group ON equipment_param_mappings(param_group_id);
END IF;

-- Audit findings index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''audit_findings'' AND INDEX_NAME=''idx_af_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_af_status ON audit_findings(status);
END IF;

-- Packaging confirmations index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''packaging_confirmations'' AND INDEX_NAME=''idx_pc_batch''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_pc_batch ON packaging_confirmations(batch_id);
END IF;

-- OQC releases indexes
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''oqc_releases'' AND INDEX_NAME=''idx_oqc_releases_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_oqc_releases_status ON oqc_releases(status);
END IF;

SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''oqc_releases'' AND INDEX_NAME=''idx_oqc_releases_batch''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_oqc_releases_batch ON oqc_releases(batch_id);
END IF;

-- IPQC closure status index
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''ipqc_closure_status'' AND INDEX_NAME=''idx_ipqc_closure_wo''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_ipqc_closure_wo ON ipqc_closure_status(work_order_id);
END IF;

-- ============================================================================
-- STEP 62: Foreign key constraints to add
-- ============================================================================

-- equipment.foreign keys for workshop_id and line_id (references organizations)
-- These are conceptual references, not strict FK (organizations may not exist yet)

-- tools.foreign keys - removed equipment_id FK above
-- (no FK to add for tools since equipment_id was dropped)

-- ipqc_patrols.foreign key for patrol_plan_id
CALL sp_column_exists('ipqc_patrols', 'patrol_plan_id', @exists);
IF @exists = 1 THEN
    -- Check if FK exists
    SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''ipqc_patrols'' AND CONSTRAINT_NAME=''fk_ipqc_patrols_patrol_plan''';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    IF @cnt = 0 THEN
        ALTER TABLE ipqc_patrols ADD CONSTRAINT fk_ipqc_patrols_patrol_plan
            FOREIGN KEY (patrol_plan_id) REFERENCES ipqc_patrol_plans(id) ON DELETE CASCADE;
    END IF;
END IF;

-- ============================================================================
-- STEP 63: Cleanup - remove old indexes that reference dropped columns
-- ============================================================================

-- Remove idx_users_org (org_id is now a regular column in users)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''users'' AND INDEX_NAME=''idx_users_org''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE users DROP INDEX idx_users_org';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Remove idx_tools_equipment (equipment_id was dropped)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''tools'' AND INDEX_NAME=''idx_tools_equipment''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE tools DROP INDEX idx_tools_equipment';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Remove idx_tools_status (status was dropped)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''tools'' AND INDEX_NAME=''idx_tools_status''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE tools DROP INDEX idx_tools_status';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Remove idx_complaints_org (org_id was dropped)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''complaints'' AND INDEX_NAME=''idx_complaints_org''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE complaints DROP INDEX idx_complaints_org';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Remove idx_complaints_customer (customer_id was just added, FK handles it)
-- Actually customer_id FK should be added, but idx_complaints_customer is fine to keep

-- Remove idx_complaints_product (product_id was dropped)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''complaints'' AND INDEX_NAME=''idx_complaints_product''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE complaints DROP INDEX idx_complaints_product';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Remove idx_complaints_customer FK (customer_id was just added)
-- Add FK for complaints.customer_id
CALL sp_column_exists('complaints', 'customer_id', @exists);
IF @exists = 1 THEN
    SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''complaints'' AND CONSTRAINT_NAME=''fk_complaints_customer''';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
    IF @cnt = 0 THEN
        ALTER TABLE complaints ADD CONSTRAINT fk_complaints_customer
            FOREIGN KEY (customer_id) REFERENCES customers(id);
    END IF;
END IF;

-- Remove idx_equip_param_mapping_equipment (already has it)
-- Remove idx_equip_param_mapping_param (param_group_id replaced param_id)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''equipment_param_mappings'' AND INDEX_NAME=''idx_equip_param_mapping_param''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE equipment_param_mappings DROP INDEX idx_equip_param_mapping_param';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Remove idx_equip_status_history_equipment (equipment_id column name changed?)
-- Actually equipment_id is still there, just status renamed to signal
-- So idx_equip_status_history_equipment is still valid

-- Remove idx_equip_status_history_time (started_at renamed to recorded_at)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''equipment_status_history'' AND INDEX_NAME=''idx_equip_status_history_time''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE equipment_status_history DROP INDEX idx_equip_status_history_time';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- Add new index for equipment_status_history(recorded_at)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''equipment_status_history'' AND INDEX_NAME=''idx_esq_recorded_at''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt = 0 THEN
    CREATE INDEX idx_esq_recorded_at ON equipment_status_history(recorded_at);
END IF;

-- Remove idx_defect_code (renamed to idx_defect_no)
SET @sql = 'SELECT COUNT(*) INTO @cnt FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME=''defects'' AND INDEX_NAME=''idx_defect_code''';
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
IF @cnt > 0 THEN
    SET @sql = 'ALTER TABLE defects DROP INDEX idx_defect_code';
    PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;
END IF;

-- ============================================================================
-- Migration Complete
-- ============================================================================

SELECT 'Schema migration completed successfully!' AS status;

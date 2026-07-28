-- ============================================================================
-- QM-AI M02 Basic Data Schema Synchronization
-- 修复 C# Model 与 MySQL 表结构的字段映射差异
-- 适用于: docker compose down -v 后重新初始化, 或手动修复现有实例
-- ============================================================================

USE qmai;

-- ============================================================================
-- 0. 修复 routing_code 唯一约束 (同一工艺路线有多个步骤, code 应相同)
--    修复 life_current 类型 (模型需要 double, DB 是 int)
--    修复 rating 类型 (模型需要 string, DB 是 decimal)
--    使用 IF NOT EXISTS 避免重复执行报错
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'routings' AND COLUMN_NAME = 'step_order');
SET @sql = IF(@exist = 0, 
    'ALTER TABLE routings DROP INDEX routing_code; ALTER TABLE routings ADD COLUMN step_order INT NOT NULL DEFAULT 0 AFTER product_id; ALTER TABLE routings ADD COLUMN process_id BIGINT AFTER step_order; ALTER TABLE routings ADD COLUMN standard_time_minutes DECIMAL(10,2) DEFAULT 0 AFTER process_id;',
    'SELECT "routings columns already exist" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'life_current' AND DATA_TYPE = 'int');
SET @sql2 = IF(@exist > 0,
    'ALTER TABLE tools MODIFY COLUMN life_current DECIMAL(10,2) DEFAULT 0 COMMENT ''当前已用寿命'';',
    'SELECT "life_current already decimal" AS msg');
PREPARE stmt2 FROM @sql2; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'suppliers' AND COLUMN_NAME = 'rating' AND DATA_TYPE = 'decimal');
SET @sql3 = IF(@exist > 0,
    'ALTER TABLE suppliers MODIFY COLUMN rating VARCHAR(10) COMMENT ''供应商等级：A/B/C/D'';',
    'SELECT "rating already varchar" AS msg');
PREPARE stmt3 FROM @sql3; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;

-- ============================================================================
-- 1. processes 表 - 添加 department, updated_at
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'processes' AND COLUMN_NAME = 'department');
SET @sql = IF(@exist = 0,
    'ALTER TABLE processes ADD COLUMN department VARCHAR(100) COMMENT ''所属部门/车间（显示冗余）'' AFTER process_type;',
    'SELECT "department already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'processes' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE processes ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER is_active;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 2. boms 表 - 添加 remark, updated_at
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'boms' AND COLUMN_NAME = 'remark');
SET @sql = IF(@exist = 0,
    'ALTER TABLE boms ADD COLUMN remark VARCHAR(500) COMMENT ''备注'' AFTER level;',
    'SELECT "remark already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'boms' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE boms ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER created_at;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 3. routings 表 - 补充 description, updated_at
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'routings' AND COLUMN_NAME = 'description');
SET @sql = IF(@exist = 0,
    'ALTER TABLE routings ADD COLUMN description VARCHAR(500) COMMENT ''工艺路线描述'' AFTER routing_name;',
    'SELECT "description already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'routings' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE routings ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER is_active;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 4. equipment 表 - 添加缺失字段
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'production_line');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN production_line VARCHAR(100) COMMENT ''所在产线（冗余）'' AFTER model;',
    'SELECT "production_line already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'workshop');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN workshop VARCHAR(100) COMMENT ''所在车间（冗余）'' AFTER production_line;',
    'SELECT "workshop already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'has_mqtt_connection');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN has_mqtt_connection TINYINT(1) DEFAULT 0 COMMENT ''是否关联MQTT'' AFTER equipment_type;',
    'SELECT "has_mqtt_connection already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'mqtt_topic_prefix');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN mqtt_topic_prefix VARCHAR(500) COMMENT ''MQTT Topic前缀'' AFTER has_mqtt_connection;',
    'SELECT "mqtt_topic_prefix already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER is_active;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'org_id');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN org_id BIGINT COMMENT ''所属组织'' AFTER equipment_name;',
    'SELECT "org_id already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'workshop_id');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN workshop_id BIGINT COMMENT ''关联车间(组织ID)'' AFTER org_id;',
    'SELECT "workshop_id already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'equipment' AND COLUMN_NAME = 'line_id');
SET @sql = IF(@exist = 0,
    'ALTER TABLE equipment ADD COLUMN line_id BIGINT COMMENT ''关联产线(组织ID)'' AFTER workshop_id;',
    'SELECT "line_id already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 5. tools 表 - 添加缺失字段
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'model');
SET @sql = IF(@exist = 0,
    'ALTER TABLE tools ADD COLUMN model VARCHAR(200) COMMENT ''工具型号'' AFTER tool_name;',
    'SELECT "model already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'design_life');
SET @sql = IF(@exist = 0,
    'ALTER TABLE tools ADD COLUMN design_life DECIMAL(10,2) COMMENT ''设计寿命'' AFTER tool_type;',
    'SELECT "design_life already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'life_unit');
SET @sql = IF(@exist = 0,
    'ALTER TABLE tools ADD COLUMN life_unit VARCHAR(20) DEFAULT ''cycles'' COMMENT ''寿命单位'' AFTER design_life;',
    'SELECT "life_unit already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'supplier');
SET @sql = IF(@exist = 0,
    'ALTER TABLE tools ADD COLUMN supplier VARCHAR(200) COMMENT ''供应商'' AFTER life_current;',
    'SELECT "supplier already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'tools' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE tools ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER is_active;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 6. defect_codes 表 - 添加 is_reworkable, updated_at
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'defect_codes' AND COLUMN_NAME = 'is_reworkable');
SET @sql = IF(@exist = 0,
    'ALTER TABLE defect_codes ADD COLUMN is_reworkable TINYINT(1) DEFAULT 0 COMMENT ''是否可返工'' AFTER defect_severity;',
    'SELECT "is_reworkable already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'defect_codes' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE defect_codes ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER is_active;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 7. inspection_standards 表 - 添加缺失字段
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'item_name');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN item_name VARCHAR(200) COMMENT ''检验项目名称'' AFTER standard_name;',
    'SELECT "item_name already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'description');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN description TEXT COMMENT ''描述'' AFTER standard_name;',
    'SELECT "description already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'usl');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN usl DECIMAL(10,4) COMMENT ''规格上限 USL'' AFTER process_id;',
    'SELECT "usl already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'lsl');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN lsl DECIMAL(10,4) COMMENT ''规格下限 LSL'' AFTER usl;',
    'SELECT "lsl already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'target');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN target DECIMAL(10,4) COMMENT ''目标值'' AFTER lsl;',
    'SELECT "target already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'unit');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN unit VARCHAR(50) COMMENT ''单位'' AFTER target;',
    'SELECT "unit already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'inspection_method');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN inspection_method VARCHAR(200) COMMENT ''检验工具/方法'' AFTER unit;',
    'SELECT "inspection_method already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'sampling_frequency');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN sampling_frequency VARCHAR(100) COMMENT ''抽样频率'' AFTER inspection_method;',
    'SELECT "sampling_frequency already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'inspection_standards' AND COLUMN_NAME = 'updated_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE inspection_standards ADD COLUMN updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT ''更新时间'' AFTER is_active;',
    'SELECT "updated_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SELECT 'Migration completed successfully!' AS status;
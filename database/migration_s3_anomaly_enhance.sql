-- ============================================================
-- IQC 来料异常表增强 - 对标设计文档 Part2 Schema + 业务合规性报告
-- 补全：隔离状态、MRB评审、处置决策、CAPA联动、操作审计
-- 适用于: 现有实例增量迁移
-- ============================================================

USE qmai;

-- ============================================================================
-- 0. 补齐 IQC 表缺失的 created_at（init.sql 定义有但实际数据库缺失的）
-- ============================================================================

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_receipts' AND COLUMN_NAME = 'created_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_receipts ADD COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER receipt_date;',
    'SELECT "iqc_receipts.created_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_inspections' AND COLUMN_NAME = 'created_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_inspections ADD COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER receipt_id;',
    'SELECT "iqc_inspections.created_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_inspection_items' AND COLUMN_NAME = 'created_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_inspection_items ADD COLUMN created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER inspection_id;',
    'SELECT "iqc_inspection_items.created_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 1. 扩展 iqc_anomalies 表 - 新增标杆字段
-- ============================================================================

-- 1.1 隔离库存量（不合格品隔离管理）
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'isolated_inventory');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN isolated_inventory DECIMAL(15,2) DEFAULT 0 COMMENT ''隔离库存量（不合格品隔离数量）'' AFTER severity;',
    'SELECT "isolated_inventory already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.2 处置方式（退货/让步接收/返工挑选/特采）
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'disposition');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN disposition ENUM(''return'',''concession'',''rework'',''special_purchase'',''none'') DEFAULT ''none'' COMMENT ''处置方式：return=退货, concession=让步接收, rework=返工挑选, special_purchase=特采, none=待处置'' AFTER description;',
    'SELECT "disposition already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.3 处置决定人
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'disposition_by');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN disposition_by VARCHAR(100) COMMENT ''处置决定人'' AFTER disposition;',
    'SELECT "disposition_by already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.4 处置决定时间
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'disposition_date');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN disposition_date DATETIME COMMENT ''处置决定时间'' AFTER disposition_by;',
    'SELECT "disposition_date already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.5 MRB是否评审
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'mrb_reviewed');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN mrb_reviewed TINYINT(1) DEFAULT 0 COMMENT ''MRB评审是否完成'' AFTER disposition_date;',
    'SELECT "mrb_reviewed already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.6 MRB评审人
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'mrb_reviewer');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN mrb_reviewer VARCHAR(100) COMMENT ''MRB评审人（多部门会签）'' AFTER mrb_reviewed;',
    'SELECT "mrb_reviewer already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.7 MRB评审时间
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'mrb_reviewed_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN mrb_reviewed_at DATETIME COMMENT ''MRB评审完成时间'' AFTER mrb_reviewer;',
    'SELECT "mrb_reviewed_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.8 关联CAPA单ID
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'capa_id');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN capa_id BIGINT COMMENT ''关联CAPA单ID'' AFTER mrb_reviewed_at;',
    'SELECT "capa_id already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.9 首次响应时间
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'first_response_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN first_response_at DATETIME COMMENT ''首次响应时间'' AFTER capa_id;',
    'SELECT "first_response_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.10 处理人部门（质量/采购/工程/生产）
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'handler_dept');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN handler_dept ENUM(''quality'',''purchasing'',''engineering'',''production'',''other'') DEFAULT NULL COMMENT ''处理人部门'' AFTER handler;',
    'SELECT "handler_dept already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.11 创建人ID（操作审计）
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'created_by');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN created_by BIGINT COMMENT ''创建人ID'' AFTER created_at;',
    'SELECT "created_by already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.12 最后更新人ID
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'updated_by');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN updated_by BIGINT COMMENT ''最后更新人ID'' AFTER updated_at;',
    'SELECT "updated_by already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.13 关联检验项目明细（不合格的具体项目）
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'failed_item_ids');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN failed_item_ids JSON COMMENT ''不合格检验项目ID列表'' AFTER first_response_at;',
    'SELECT "failed_item_ids already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.14 不合格品数量
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'defect_qty');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN defect_qty INT DEFAULT 0 COMMENT ''不合格品数量'' AFTER failed_item_ids;',
    'SELECT "defect_qty already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.15 是否已通知供应商
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'supplier_notified');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN supplier_notified TINYINT(1) DEFAULT 0 COMMENT ''是否已通知供应商'' AFTER defect_qty;',
    'SELECT "supplier_notified already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 1.16 供应商回复时间
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'supplier_response_at');
SET @sql = IF(@exist = 0,
    'ALTER TABLE iqc_anomalies ADD COLUMN supplier_response_at DATETIME COMMENT ''供应商回复时间'' AFTER supplier_notified;',
    'SELECT "supplier_response_at already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 2. 扩展状态枚举 - 增加隔离、评审、处置中间状态
-- 注意：MySQL ENUM 扩展需要重建列
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'status');
SET @current_type = (SELECT COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'status');
SET @new_type = 'enum(''open'',''quarantined'',''investigating'',''mrb_reviewing'',''mrb_approved'',''disposed'',''processing'',''resolved'',''closed'')';
SET @sql = IF(@exist > 0 AND @current_type != @new_type,
    CONCAT('ALTER TABLE iqc_anomalies MODIFY COLUMN status ', @new_type, ' DEFAULT ''open'' COMMENT ''状态：open=待处理, quarantined=待隔离, investigating=调查分析中, mrb_reviewing=MRB评审中, mrb_approved=MRB通过, disposed=已处置, processing=处理中, resolved=已解决, closed=已关闭'''),
    'SELECT "status enum OK" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 3. 扩展异常类型枚举 - 增加更多类型
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'anomaly_type');
SET @current_type2 = (SELECT COLUMN_TYPE FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND COLUMN_NAME = 'anomaly_type');
SET @new_type2 = 'enum(''quality'',''quantity'',''document'',''packaging'',''environment'',''other'')';
SET @sql2 = IF(@exist > 0 AND @current_type2 != @new_type2,
    CONCAT('ALTER TABLE iqc_anomalies MODIFY COLUMN anomaly_type ', @new_type2, ' DEFAULT ''quality'' COMMENT ''异常类型：quality=质量问题, quantity=数量问题, document=单据问题, packaging=包装问题, environment=环境问题, other=其他'''),
    'SELECT "anomaly_type enum OK" AS msg');
PREPARE stmt2 FROM @sql2; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

-- ============================================================================
-- 4. 添加索引
-- ============================================================================
SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND INDEX_NAME = 'idx_anomalies_capa');
SET @sql = IF(@exist = 0,
    'CREATE INDEX idx_anomalies_capa ON iqc_anomalies(capa_id);',
    'SELECT "idx_anomalies_capa already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND INDEX_NAME = 'idx_anomalies_mrb');
SET @sql = IF(@exist = 0,
    'CREATE INDEX idx_anomalies_mrb ON iqc_anomalies(mrb_reviewed);',
    'SELECT "idx_anomalies_mrb already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @exist = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS
    WHERE TABLE_SCHEMA = 'qmai' AND TABLE_NAME = 'iqc_anomalies' AND INDEX_NAME = 'idx_anomalies_disposition');
SET @sql = IF(@exist = 0,
    'CREATE INDEX idx_anomalies_disposition ON iqc_anomalies(disposition);',
    'SELECT "idx_anomalies_disposition already exists" AS msg');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- ============================================================================
-- 5. 更新现有种子数据 - 为现有异常记录补全标杆字段
-- ============================================================================
UPDATE iqc_anomalies SET
    isolated_inventory = 800,
    disposition = 'return',
    disposition_by = '张明',
    disposition_date = '2026-07-05 10:00:00',
    mrb_reviewed = 1,
    mrb_reviewer = '张明/李强/王华',
    mrb_reviewed_at = '2026-07-05 09:00:00',
    first_response_at = '2026-07-04 12:00:00',
    defect_qty = 12,
    supplier_notified = 1,
    supplier_response_at = '2026-07-04 15:00:00'
WHERE anomaly_no = 'ANM-20260704-001';

UPDATE iqc_anomalies SET
    isolated_inventory = 3000,
    disposition = 'concession',
    disposition_by = '李强',
    disposition_date = '2026-07-08 14:00:00',
    mrb_reviewed = 1,
    mrb_reviewer = '李强/王华',
    mrb_reviewed_at = '2026-07-08 13:00:00',
    first_response_at = '2026-07-03 10:00:00',
    defect_qty = 2,
    supplier_notified = 1,
    supplier_response_at = '2026-07-05 09:00:00'
WHERE anomaly_no = 'ANM-20260703-002';

SELECT 'Migration completed successfully!' AS status;

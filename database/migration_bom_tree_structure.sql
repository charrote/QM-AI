-- ============================================================================
-- QM-AI Migration: BOM Tree Structure - Add ParentId & ProductCode
-- Generated: 2026-08-11
-- Target: MySQL 8.0+ | Database: qmai
-- Strategy: Idempotent (safe to re-run)
-- ============================================================================

USE qmai;

-- Step 1: Add parent_id column for BOM tree hierarchy
DROP PROCEDURE IF EXISTS sp_bom_parentid_check;
DELIMITER $$
CREATE PROCEDURE sp_bom_parentid_check()
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'boms' AND COLUMN_NAME = 'parent_id'
  ) THEN
    ALTER TABLE boms
      ADD COLUMN parent_id BIGINT DEFAULT NULL COMMENT '父件BOM ID（层级关系）' AFTER product_id;
  END IF;
END$$
DELIMITER ;
CALL sp_bom_parentid_check();
DROP PROCEDURE IF EXISTS sp_bom_parentid_check;

-- Step 2: Add index on parent_id for tree queries
DROP PROCEDURE IF EXISTS sp_bom_parentidx_check;
DELIMITER $$
CREATE PROCEDURE sp_bom_parentidx_check()
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'boms' AND INDEX_NAME = 'idx_boms_parent'
  ) THEN
    ALTER TABLE boms ADD INDEX idx_boms_parent (parent_id);
  END IF;
END$$
DELIMITER ;
CALL sp_bom_parentidx_check();
DROP PROCEDURE IF EXISTS sp_bom_parentidx_check;

-- Step 3: Add product_code column for display (avoid join on tree queries)
DROP PROCEDURE IF EXISTS sp_bom_productcode_check;
DELIMITER $$
CREATE PROCEDURE sp_bom_productcode_check()
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'boms' AND COLUMN_NAME = 'product_code'
  ) THEN
    ALTER TABLE boms
      ADD COLUMN product_code VARCHAR(50) DEFAULT NULL COMMENT '产品编码（冗余）' AFTER product_id;

    -- Backfill existing records
    UPDATE boms b
    JOIN products p ON b.product_id = p.id
    SET b.product_code = p.product_code
    WHERE b.product_code IS NULL;
  END IF;
END$$
DELIMITER ;
CALL sp_bom_productcode_check();
DROP PROCEDURE IF EXISTS sp_bom_productcode_check;

-- ============================================================================
-- QM-AI Migration: Insert BOM L2 Level Seed Data
-- Generated: 2026-08-11
-- Purpose: Add sub-component levels for tree demonstration
-- ============================================================================

USE qmai;

-- Insert L2 sub-components for P001 (product_id=2)
INSERT INTO boms (product_id, product_code, parent_id, material_code, material_name, quantity, unit, level, remark, created_at, updated_at)
VALUES 
  (2, 'A100', 1, 'MAT-001-01', '圆钢锭 Φ60', 1.5, 'kg', 2, '原材料胚料', NOW(), NOW()),
  (2, 'A100', 1, 'MAT-001-02', '切削液', 0.03, 'L', 2, '机加工冷却润滑', NOW(), NOW()),
  (2, 'A100', 2, 'MAT-002-01', '钢球 Φ6.35', 8, 'pcs', 2, 'GCr15 轴承钢', NOW(), NOW()),
  (2, 'A100', 2, 'MAT-002-02', '保持架', 1, 'pcs', 2, 'H62 黄铜冲压', NOW(), NOW());

-- Insert L2 sub-components for P002 (product_id=3)
INSERT INTO boms (product_id, product_code, parent_id, material_code, material_name, quantity, unit, level, remark, created_at, updated_at)
VALUES 
  (3, 'B200', 7, 'MAT-101-01', '铝锭 A1', 2.3, 'kg', 2, '回炉铝配比', NOW(), NOW()),
  (3, 'B200', 7, 'MAT-101-02', '锌锭 Zn', 0.2, 'kg', 2, '合金添加', NOW(), NOW());

-- Insert L2 sub-components for P003 (product_id=4)
INSERT INTO boms (product_id, product_code, parent_id, material_code, material_name, quantity, unit, level, remark, created_at, updated_at)
VALUES 
  (4, 'C300', 14, 'MAT-201-01', '玻璃纤维布', 0.3, 'm²', 2, 'E-glass 48# 编织布', NOW(), NOW()),
  (4, 'C300', 14, 'MAT-201-02', '环氧树脂 EP-44', 0.15, 'kg', 2, '浸渍树脂', NOW(), NOW());

-- Insert L2 sub-components for P004 (product_id=5)
INSERT INTO boms (product_id, product_code, parent_id, material_code, material_name, quantity, unit, level, remark, created_at, updated_at)
VALUES 
  (5, 'D400', 23, 'MAT-301-01', '丙烯腈 AN', 0.08, 'kg', 2, '主体单体', NOW(), NOW()),
  (5, 'D400', 23, 'MAT-301-02', '丁二烯 BD', 0.07, 'kg', 2, '共聚单体', NOW(), NOW());

-- Insert L2 sub-components for P005 (product_id=6)
INSERT INTO boms (product_id, product_code, parent_id, material_code, material_name, quantity, unit, level, remark, created_at, updated_at)
VALUES 
  (6, 'E500', 27, 'MAT-401-01', '铜丝 TU2 Φ0.75', 0.015, 'kg', 2, '无氧铜导体', NOW(), NOW()),
  (6, 'E500', 27, 'MAT-401-02', 'PVC 护套料', 0.008, 'kg', 2, '绝缘包覆', NOW(), NOW());

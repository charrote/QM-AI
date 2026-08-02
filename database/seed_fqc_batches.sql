-- ============================================================================
-- FQC/Batches 成品批次管理 DEMO 数据插入脚本
-- 数据库：qmai
-- 执行方式：cat database/seed_fqc_batches.sql | docker exec -i qm-ai-mysql mysql -u root -p'qmai_root_2024' qmai
-- 前置依赖：
--   1. cat backend/insert_customers_demo.sql | docker exec -i qm-ai-mysql mysql -u root -p'qmai_root_2024' qmai
--       （插入 14 家客户，ID 为 1~14）
--
-- 说明：此脚本插入演示数据（10 个批次 + 完整工作流），非初始化种子
--       插入顺序：product_batches → fqc_inspections → fqc_inspection_items
--                → oqc_releases → packaging_confirmations
--
-- 覆盖场景：
--   - 完整合格流程（批次2、4、6、7）：批次创建 → FQC 全检合格 → OQC 放行 → 包装确认
--   - 不合格隔离流程（批次3、8）：批次创建 → FQC 抽检不合格 → 隔离
--   - 待检验流程（批次1、5、9、10）：批次创建 → 待检验
-- ============================================================================

-- 产品 ID 映射（products 表）：1=精密转轴 A100, 2=壳体 B200, 3=PCB 主板 C300, 4=密封圈 D400, 5=连接线束 E500
-- 用户 ID 映射（users 表）：1=admin, 2=operator, 3=inspector
-- 客户 ID 映射（customers 表）：1=华芯微电子，2=精密电子，3=智联通讯，4=佛山新能源，5=博创半导体，6=亿纬锂能
--                               7=广州汽车零部件，8=中山传动，9=佛山冲压，10=东莞汽车电子
--                               11=美的配件，12=格力配件，13=华帝品质部，14=迈瑞医疗

-- ─── 1. product_batches（10 个批次）──────────────────────────────────────

INSERT INTO product_batches (id, batch_code, source, product_id, quantity, status, created_at, updated_at) VALUES
(1, 'LOT-20260715-A', 'manual', 1, 5000.0000, 'in_progress', '2026-07-15 08:00:00', '2026-07-15 08:00:00'),
(2, 'LOT-20260718-A', 'manual', 2, 3000.0000, 'released', '2026-07-18 08:00:00', '2026-07-20 10:00:00'),
(3, 'LOT-20260720-A', 'manual', 3, 2000.0000, 'quarantined', '2026-07-20 08:00:00', '2026-07-21 14:30:00'),
(4, 'LOT-20260725-A', 'manual', 1, 6000.0000, 'released', '2026-07-25 08:00:00', '2026-07-27 09:00:00'),
(5, 'LOT-20260722-A', 'manual', 5, 8000.0000, 'in_progress', '2026-07-22 08:00:00', '2026-07-22 08:00:00'),
(6, 'LOT-20260726-A', 'manual', 3, 2500.0000, 'released', '2026-07-26 08:00:00', '2026-07-28 11:00:00'),
(7, 'LOT-20260727-A', 'manual', 5, 10000.0000, 'released', '2026-07-27 08:00:00', '2026-07-29 16:00:00'),
(8, 'LOT-20260729-A', 'manual', 5, 10000.0000, 'quarantined', '2026-07-29 08:00:00', '2026-07-30 09:15:00'),
(9, 'LOT-20260720-B', 'manual', 3, 2500.0000, 'in_progress', '2026-07-20 14:00:00', '2026-07-20 14:00:00'),
(10, 'LOT-20260722-B', 'manual', 3, 3000.0000, 'in_progress', '2026-07-22 14:00:00', '2026-07-22 14:00:00');

-- ─── 2. fqc_inspections（检验单）─────────────────────────────────────────

INSERT INTO fqc_inspections (id, inspection_no, batch_id, inspection_type, aql_level, sample_size, total_checked, total_pass, total_fail, ac, re, conclusion, inspector_id, checked_at, created_at, updated_at) VALUES
(1, 'FQC-20260718-0001', 2, 'full', NULL, 300, 300, 295, 5, 0, 1, 'qualified', 3, '2026-07-18 16:00:00', '2026-07-18 10:00:00', '2026-07-18 16:00:00'),
(2, 'FQC-20260720-0001', 3, 'sampling', 1.0, 125, 125, 112, 13, 8, 13, 'unqualified', 3, '2026-07-20 17:00:00', '2026-07-20 10:00:00', '2026-07-20 17:00:00'),
(3, 'FQC-20260725-0001', 4, 'full', NULL, 600, 600, 598, 2, 0, 1, 'qualified', 3, '2026-07-25 17:00:00', '2026-07-25 10:00:00', '2026-07-25 17:00:00'),
(4, 'FQC-20260726-0001', 6, 'full', NULL, 250, 250, 250, 0, 0, 1, 'qualified', 3, '2026-07-26 16:30:00', '2026-07-26 10:00:00', '2026-07-26 16:30:00'),
(5, 'FQC-20260727-0001', 7, 'full', NULL, 500, 500, 499, 1, 0, 1, 'qualified', 3, '2026-07-27 16:00:00', '2026-07-27 10:00:00', '2026-07-27 16:00:00'),
(6, 'FQC-20260729-0001', 8, 'sampling', 0.65, 200, 200, 178, 22, 5, 22, 'unqualified', 3, '2026-07-29 18:00:00', '2026-07-29 10:00:00', '2026-07-29 18:00:00');

-- ─── 3. fqc_inspection_items（检验明细项）────────────────────────────────

INSERT INTO fqc_inspection_items (id, inspection_id, item_name, item_code, usl, lsl, data_type, actual_value, result) VALUES
(1, 1, '壁厚', 'DIM-WT', 2.1000, 1.9000, 'numeric', 2.0200, 'pass'),
(2, 1, '螺栓孔位置度', 'DIM-PD', 0.1500, NULL, 'numeric', 0.0800, 'pass'),
(3, 1, '螺栓孔直径', 'DIM-HD', 6.1000, 5.9000, 'numeric', 6.0200, 'pass'),
(4, 1, '毛刺', 'APP-BURR', NULL, NULL, 'visual', NULL, 'pass'),
(5, 1, '氧化', 'APP-OX', NULL, NULL, 'visual', NULL, 'pass'),
(6, 1, '飞边', 'APP-FLASH', NULL, NULL, 'attribute', NULL, 'fail'),
(7, 2, '引脚间距', 'DIM-PIN-P', 2.5450, 2.5350, 'numeric', 2.5420, 'pass'),
(8, 2, '共面性', 'DIM-COPLAN', 0.1000, NULL, 'numeric', 0.1500, 'fail'),
(9, 2, '划痕', 'APP-SCRATCH', NULL, NULL, 'visual', NULL, 'fail'),
(10, 2, '脏污', 'APP-DIRT', NULL, NULL, 'visual', NULL, 'fail'),
(11, 2, '铜箔露基体', 'APP-EXPOS', NULL, NULL, 'visual', NULL, 'fail'),
(12, 2, '焊盘氧化', 'APP-PAD-OX', NULL, NULL, 'visual', NULL, 'fail'),
(13, 2, '虚焊', 'WLD-VOID', NULL, NULL, 'attribute', NULL, 'fail'),
(14, 2, '漏焊', 'WLD-MISS', NULL, NULL, 'attribute', NULL, 'fail'),
(15, 3, '外径', 'DIM-OD', 25.0500, 24.9500, 'numeric', 25.0100, 'pass'),
(16, 3, '同轴度', 'DIM-CIR', 0.0200, NULL, 'numeric', 0.0080, 'pass'),
(17, 3, '表面粗糙度', 'DIM-RA', NULL, 0.8000, 'numeric', 0.4000, 'pass'),
(18, 3, '键槽深度', 'DIM-KeyD', 3.0500, 2.9500, 'numeric', 3.0100, 'pass'),
(19, 3, '端面跳动', 'DIM-RunT', 0.0150, NULL, 'numeric', 0.0050, 'pass'),
(20, 4, '引脚间距', 'DIM-PIN-P', 2.5450, 2.5350, 'numeric', 2.5400, 'pass'),
(21, 4, '共面性', 'DIM-COPLAN', 0.1000, NULL, 'numeric', 0.0600, 'pass'),
(22, 4, '板厚', 'DIM-THICK', 1.6500, 1.5500, 'numeric', 1.6000, 'pass'),
(23, 4, '孔径', 'DIM-HOLE', 1.0500, 0.9500, 'numeric', 1.0200, 'pass'),
(24, 4, '划痕', 'APP-SCRATCH', NULL, NULL, 'visual', NULL, 'pass'),
(25, 4, '脏污', 'APP-DIRT', NULL, NULL, 'visual', NULL, 'pass'),
(26, 4, '铜箔露基体', 'APP-EXPOS', NULL, NULL, 'visual', NULL, 'pass'),
(27, 4, '焊盘氧化', 'APP-PAD-OX', NULL, NULL, 'visual', NULL, 'pass'),
(28, 4, '字符清晰度', 'APP-CHAR', NULL, NULL, 'visual', NULL, 'pass'),
(29, 4, '绝缘电阻', 'ELC-IR', NULL, 100.0000, 'numeric', 500.0000, 'pass'),
(30, 5, '端子拉力', 'DIM-PULL', 50.0000, 30.0000, 'numeric', 45.0000, 'pass'),
(31, 5, '绝缘厚度', 'DIM-INS-T', 0.3500, 0.2500, 'numeric', 0.3000, 'pass'),
(32, 5, '导体电阻', 'DIM-RES', NULL, 0.0200, 'numeric', 0.0120, 'pass'),
(33, 5, '颜色', 'APP-COLOR', NULL, NULL, 'visual', NULL, 'pass'),
(34, 5, '标识', 'APP-MARK', NULL, NULL, 'visual', NULL, 'pass'),
(35, 5, '包装完整性', 'PKG-INT', NULL, NULL, 'attribute', NULL, 'pass'),
(36, 6, '端子拉力', 'DIM-PULL', 50.0000, 30.0000, 'numeric', 42.0000, 'pass'),
(37, 6, '绝缘厚度', 'DIM-INS-T', 0.3500, 0.2500, 'numeric', 0.2800, 'pass'),
(38, 6, '导体电阻', 'DIM-RES', NULL, 0.0200, 'numeric', 0.0150, 'pass'),
(39, 6, '颜色', 'APP-COLOR', NULL, NULL, 'visual', NULL, 'pass'),
(40, 6, '标识', 'APP-MARK', NULL, NULL, 'visual', NULL, 'fail'),
(41, 6, '包装完整性', 'PKG-INT', NULL, NULL, 'attribute', NULL, 'fail'),
(42, 6, '屏蔽层完整性', 'PKG-SHLD', NULL, NULL, 'attribute', NULL, 'fail');

-- ─── 4. oqc_releases（出货放行单）────────────────────────────────────────

INSERT INTO oqc_releases (id, batch_id, customer_id, release_number, release_date, quantity, authorized_by, status, signature_time, created_at, updated_at) VALUES
(1, 2, 1, 'REL-20260719-0001', '2026-07-19 09:00:00', 2800.0000, 1, 'released', '2026-07-19 09:30:00', '2026-07-19 09:00:00', '2026-07-19 09:30:00'),
(2, 4, 1, 'REL-20260726-0001', '2026-07-26 09:00:00', 5800.0000, 1, 'released', '2026-07-26 09:15:00', '2026-07-26 09:00:00', '2026-07-26 09:15:00'),
(3, 6, 3, 'REL-20260727-0001', '2026-07-27 09:00:00', 2400.0000, 1, 'released', '2026-07-27 09:20:00', '2026-07-27 09:00:00', '2026-07-27 09:20:00'),
(4, 7, 7, 'REL-20260728-0001', '2026-07-28 09:00:00', 9500.0000, 1, 'released', '2026-07-28 09:45:00', '2026-07-28 09:00:00', '2026-07-28 09:45:00');

-- ─── 5. packaging_confirmations（包装确认）────────────────────────────────

INSERT INTO packaging_confirmations (id, batch_id, packaging_method, qty_per_box, total_boxes, label_printed, confirmed_by, confirmed_at) VALUES
(1, 2, '纸箱', 50, 56, 1, 2, '2026-07-20 08:00:00'),
(2, 4, '纸箱', 20, 290, 1, 2, '2026-07-27 08:00:00'),
(3, 6, '防静电纸箱', 25, 96, 1, 2, '2026-07-28 09:00:00'),
(4, 7, '编织袋', 200, 47, 1, 2, '2026-07-29 14:00:00');

-- ═══════════════════════════════════════════════════════════════════════════
-- 数据验证
-- ═══════════════════════════════════════════════════════════════════════════

SELECT '=========================================' AS '';
SELECT '  FQC/Batches DEMO 数据验证' AS '';
SELECT '=========================================' AS '';

SELECT '--- 批次统计 ---' AS section;
SELECT
    COUNT(*) AS '总批次',
    SUM(CASE WHEN status = 'in_progress' THEN 1 ELSE 0 END) AS '生产中',
    SUM(CASE WHEN status = 'inspected' THEN 1 ELSE 0 END) AS '已检验',
    SUM(CASE WHEN status = 'released' THEN 1 ELSE 0 END) AS '已放行',
    SUM(CASE WHEN status = 'quarantined' THEN 1 ELSE 0 END) AS '隔离中'
FROM product_batches;

SELECT '--- 检验结论统计 ---' AS section;
SELECT
    conclusion AS '结论',
    COUNT(*) AS '数量'
FROM fqc_inspections
GROUP BY conclusion;

SELECT '--- OQC 放行统计 ---' AS section;
SELECT
    COUNT(*) AS '放行单总数',
    SUM(quantity) AS '放行总数量'
FROM oqc_releases;

SELECT '--- 包装确认统计 ---' AS section;
SELECT
    COUNT(*) AS '确认总数',
    SUM(CASE WHEN label_printed = 1 THEN 1 ELSE 0 END) AS '标签已打印',
    SUM(CASE WHEN label_printed = 0 THEN 1 ELSE 0 END) AS '标签未打印'
FROM packaging_confirmations;

SELECT '--- 检验明细项统计 ---' AS section;
SELECT
    COUNT(*) AS '总检验项',
    SUM(CASE WHEN result = 'pass' THEN 1 ELSE 0 END) AS '合格项',
    SUM(CASE WHEN result = 'fail' THEN 1 ELSE 0 END) AS '不合格项'
FROM fqc_inspection_items;

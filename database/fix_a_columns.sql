-- ============================================================
-- QM-AI 数据库一致性修复：A 类补列
-- 背景：以下 3 张表由旧版 EF EnsureCreated 创建，缺失后续实体新增的列
-- 列定义以 database/init.sql 权威定义为准
-- ============================================================

-- 1. inspection_plan_items：补充规格/管理限覆盖列
ALTER TABLE inspection_plan_items
    ADD COLUMN usl DECIMAL(15,6) NULL COMMENT '规格上限(覆盖)' AFTER sort_order,
    ADD COLUMN lsl DECIMAL(15,6) NULL COMMENT '规格下限(覆盖)' AFTER usl,
    ADD COLUMN target_value DECIMAL(15,6) NULL COMMENT '目标值(覆盖)' AFTER lsl,
    ADD COLUMN ucl DECIMAL(15,6) NULL COMMENT '管理上限(覆盖)' AFTER target_value,
    ADD COLUMN lcl DECIMAL(15,6) NULL COMMENT '管理下限(覆盖)' AFTER ucl,
    ADD COLUMN sample_size INT NULL COMMENT '抽样数量(覆盖)' AFTER lcl;

-- 2. iqc_inspection_items：补充动态参数关联与备注列
ALTER TABLE iqc_inspection_items
    ADD COLUMN param_id BIGINT NULL COMMENT '关联 dynamic_params.id' AFTER inspection_id,
    ADD COLUMN remark TEXT NULL AFTER defect_code_id;

-- 3. iqc_inspections：补充检验标准关联列
ALTER TABLE iqc_inspections
    ADD COLUMN standard_id BIGINT NULL COMMENT '关联检验标准' AFTER receipt_id;

-- 4. users 表补充 username 索引对齐（若原表无唯一索引）
-- 注：users.username 列已存在（snake_case），无需 ALTER

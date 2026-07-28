-- ============================================================
-- 供应商评分表增强 — 补充缺失字段
-- 数据库：qmai
-- 表：supplier_scores
-- 执行时机：在 init.sql 之后、seed_supplier_scores.sql 之前
-- 说明：EF Core 迁移只创建了 id / supplier_id / assessment_date / score 四个字段，
--       但 init.sql 中定义了 dimension_scores / grade / evaluation 三个字段，
--       此迁移将缺失字段补齐，使数据库结构与服务端模型保持一致。
-- ============================================================

-- 1. 添加评级字段（ENUM: A / B / C / D）
ALTER TABLE supplier_scores
  ADD COLUMN grade ENUM('A','B','C','D') DEFAULT NULL
  AFTER score;

-- 2. 添加评估意见字段
ALTER TABLE supplier_scores
  ADD COLUMN evaluation TEXT DEFAULT NULL
  AFTER grade;

-- 3. 添加维度评分字段（JSON 格式）
ALTER TABLE supplier_scores
  ADD COLUMN dimension_scores JSON DEFAULT NULL
  AFTER evaluation;

-- 4. 为 supplier_id 添加索引（如果尚未存在）
-- ALTER TABLE supplier_scores ADD INDEX idx_supplier_scores_supplier (supplier_id);
-- 已在 init.sql 中创建，此处注释说明

-- 5. 为 assessment_date 添加索引（如果尚未存在）
-- ALTER TABLE supplier_scores ADD INDEX idx_supplier_scores_date (assessment_date);
-- 已在 init.sql 中创建，此处注释说明

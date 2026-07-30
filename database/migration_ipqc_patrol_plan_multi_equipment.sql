-- ============================================================================
-- Migration: 巡检计划支持多设备选择 (ipqc_patrol_plan_equipment 中间表)
-- Description: 巡检计划从单设备改为多设备，新建中间表实现多对多关系
-- ============================================================================

-- 1. 创建中间表
CREATE TABLE IF NOT EXISTS ipqc_patrol_plan_equipment (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    patrol_plan_id BIGINT NOT NULL COMMENT '巡检计划ID',
    equipment_id BIGINT NOT NULL COMMENT '设备ID',
    sort_order INT DEFAULT 0 COMMENT '排序',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_plan_equipment UNIQUE (patrol_plan_id, equipment_id),
    CONSTRAINT fk_ppe_plan FOREIGN KEY (patrol_plan_id) REFERENCES ipqc_patrol_plans(id) ON DELETE CASCADE,
    CONSTRAINT fk_ppe_equipment FOREIGN KEY (equipment_id) REFERENCES equipment(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='巡检计划-设备关联表';

-- 2. 迁移已有数据：将 ipqc_patrol_plans.equipment_id 复制到中间表
INSERT INTO ipqc_patrol_plan_equipment (patrol_plan_id, equipment_id, sort_order, created_at)
SELECT id, equipment_id, 0, NOW()
FROM ipqc_patrol_plans
WHERE equipment_id IS NOT NULL AND equipment_id > 0
ON DUPLICATE KEY UPDATE sort_order = VALUES(sort_order);

-- 3. 添加唯一索引（如果还没有）
-- 上面的 CREATE TABLE 已包含 UNIQUE 约束，此处为安全起见再建一个
IF NOT EXISTS (
    SELECT 1 FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'ipqc_patrol_plan_equipment'
      AND INDEX_NAME = 'idx_plan_equipment'
) THEN
    ALTER TABLE ipqc_patrol_plan_equipment ADD INDEX idx_plan_equipment (patrol_plan_id, equipment_id);
END IF;

-- 4. 为巡检计划表添加 equipment_ids JSON 兼容列（用于快速查询，可选）
--    实际数据源为中间表，此列仅作缓存/兼容用途
CALL sp_column_exists('ipqc_patrol_plans', 'equipment_ids', @exists);
IF @exists = 0 THEN
    ALTER TABLE ipqc_patrol_plans ADD COLUMN equipment_ids JSON COMMENT '设备ID列表（JSON缓存，数据源为中间表）';
END IF;

-- 5. 清理旧的巡检记录中的设备数据（如果中间表创建后，后续代码会更新这些记录）
--    此处不做操作，由应用层在下次生成时处理

-- ============================================================================
-- QM-AI 数据库补充定义 - 缺失表和种子数据
-- 用于增量补充 init.sql 中未包含的表
-- ============================================================================

USE qmai;

-- M15 - 企业组织层级
CREATE TABLE IF NOT EXISTS organizations (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(50) NOT NULL COMMENT '组织编码',
    name VARCHAR(200) NOT NULL COMMENT '组织名称',
    level VARCHAR(20) NOT NULL COMMENT '层级：group/company/workshop/line',
    parent_id BIGINT COMMENT '父级组织 ID',
    sort_order INT DEFAULT 0 COMMENT '排序号',
    is_active TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    location VARCHAR(500) COMMENT '位置/地址',
    contact TEXT COMMENT '联系人信息 (JSON)',
    description TEXT COMMENT '描述',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by BIGINT COMMENT '创建人',
    FOREIGN KEY (parent_id) REFERENCES organizations(id),
    UNIQUE KEY uk_organizations_code (code),
    INDEX idx_organizations_parent (parent_id),
    INDEX idx_organizations_level (level)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- M05 - 包装确认
CREATE TABLE IF NOT EXISTS packaging_confirmations (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    batch_id BIGINT NOT NULL COMMENT '关联批次',
    packaging_method VARCHAR(200) NOT NULL COMMENT '包装方式',
    qty_per_box INT COMMENT '每箱数量',
    total_boxes INT COMMENT '总箱数',
    label_printed TINYINT(1) DEFAULT 0 COMMENT '标签是否已打印',
    confirmed_by INT NOT NULL COMMENT '确认人 ID',
    confirmed_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '确认时间',
    FOREIGN KEY (batch_id) REFERENCES product_batches(id),
    INDEX idx_packaging_batch (batch_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- M04 - IPQC AI 风险评分历史
CREATE TABLE IF NOT EXISTS ipqc_ai_risk_scores (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_id BIGINT NOT NULL COMMENT '关联设备',
    process_id BIGINT NOT NULL COMMENT '关联工序',
    work_order_id BIGINT COMMENT '关联工单',
    risk_score INT DEFAULT 0 COMMENT '风险评分 0-100',
    risk_level VARCHAR(20) DEFAULT 'normal' COMMENT '风险等级：normal/warning/critical',
    factors_json TEXT COMMENT '风险因素分解 (JSON)',
    trend_direction VARCHAR(10) COMMENT '趋势方向：stable/rising/falling',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (equipment_id) REFERENCES equipment(id),
    FOREIGN KEY (process_id) REFERENCES processes(id),
    INDEX idx_risk_equipment (equipment_id),
    INDEX idx_risk_process (process_id),
    INDEX idx_risk_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- M04 - 巡检计划 - 设备关联表（多对多中间表）
CREATE TABLE IF NOT EXISTS ipqc_patrol_plan_equipment (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    patrol_plan_id BIGINT NOT NULL COMMENT '关联巡检计划',
    equipment_id BIGINT NOT NULL COMMENT '关联设备',
    sort_order INT DEFAULT 0 COMMENT '排序',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (patrol_plan_id) REFERENCES ipqc_patrol_plans(id) ON DELETE CASCADE,
    FOREIGN KEY (equipment_id) REFERENCES equipment(id),
    UNIQUE KEY uk_plan_equipment (patrol_plan_id, equipment_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- M09 - 客诉时间线事件
CREATE TABLE IF NOT EXISTS complaint_events (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    complaint_id BIGINT NOT NULL COMMENT '关联客诉',
    event_type VARCHAR(20) NOT NULL COMMENT '事件类型',
    event_data TEXT COMMENT '事件数据（JSON）',
    created_by BIGINT NOT NULL COMMENT '操作用户',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '发生时间',
    FOREIGN KEY (complaint_id) REFERENCES complaints(id) ON DELETE CASCADE,
    INDEX idx_events_complaint (complaint_id),
    INDEX idx_events_type (event_type),
    INDEX idx_events_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 工艺路线头表
CREATE TABLE IF NOT EXISTS routing_headers (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT NOT NULL COMMENT '关联产品',
    route_code VARCHAR(50) NOT NULL COMMENT '工艺路线编码',
    route_name VARCHAR(200) NOT NULL COMMENT '工艺路线名称',
    route_type VARCHAR(20) DEFAULT 'STD' COMMENT '类型',
    description TEXT COMMENT '描述',
    is_default TINYINT(1) DEFAULT 0 COMMENT '是否默认',
    is_active TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    sort_order INT DEFAULT 0 COMMENT '排序',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id),
    UNIQUE KEY uk_routing_code (product_id, route_code),
    INDEX idx_routing_product (product_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 系统字典类型
CREATE TABLE IF NOT EXISTS sys_dict_types (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    type_code VARCHAR(50) NOT NULL COMMENT '字典类型编码',
    type_name VARCHAR(200) NOT NULL COMMENT '字典类型名称',
    is_system TINYINT(1) DEFAULT 0 COMMENT '是否系统内置',
    status TINYINT(1) DEFAULT 1 COMMENT '状态',
    remark TEXT COMMENT '备注',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_dict_type_code (type_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 系统字典项
CREATE TABLE IF NOT EXISTS sys_dict_items (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    type_code VARCHAR(50) NOT NULL COMMENT '字典类型编码',
    item_label VARCHAR(200) NOT NULL COMMENT '显示标签',
    item_value VARCHAR(100) NOT NULL COMMENT '选项值',
    sort_order INT DEFAULT 0 COMMENT '排序号',
    color VARCHAR(20) COMMENT '颜色标识',
    is_default TINYINT(1) DEFAULT 0 COMMENT '是否默认',
    status TINYINT(1) DEFAULT 1 COMMENT '状态',
    remark TEXT COMMENT '备注',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (type_code) REFERENCES sys_dict_types(type_code) ON DELETE CASCADE,
    INDEX idx_dict_type (type_code),
    UNIQUE KEY uk_dict_item (type_code, item_value)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 字典类型种子数据
INSERT IGNORE INTO sys_dict_types (type_code, type_name, is_system, status, remark) VALUES
('inspection_type', '检验类型', 1, 1, 'IQC/IPQC/FQC/OQC'),
('defect_severity', '缺陷严重度', 1, 1, 'MA/MI/CR'),
('status', '状态', 1, 1, '通用状态'),
('priority', '优先级', 1, 1, 'HIGH/MEDIUM/LOW'),
('equipment_status', '设备状态', 1, 1, 'RUNNING/STOPPED/MAINTENANCE'),
('org_level', '组织层级', 1, 1, 'group/company/workshop/line');

-- 字典项种子数据
INSERT IGNORE INTO sys_dict_items (type_code, item_label, item_value, sort_order, is_default) VALUES
('inspection_type', '来料检验', 'IQC', 1, 1),
('inspection_type', '过程检验', 'IPQC', 2, 0),
('inspection_type', '成品检验', 'FQC', 3, 0),
('inspection_type', '出货检验', 'OQC', 4, 0),
('defect_severity', '严重', 'MA', 1, 1),
('defect_severity', '轻微', 'MI', 2, 0),
('defect_severity', '致命', 'CR', 3, 0),
('status', '启用', 'active', 1, 1),
('status', '禁用', 'inactive', 2, 0),
('priority', '高', 'HIGH', 1, 0),
('priority', '中', 'MEDIUM', 2, 1),
('priority', '低', 'LOW', 3, 0),
('equipment_status', '运行中', 'RUNNING', 1, 1),
('equipment_status', '停机', 'STOPPED', 2, 0),
('equipment_status', '维护中', 'MAINTENANCE', 3, 0),
('org_level', '集团', 'group', 1, 1),
('org_level', '公司', 'company', 2, 0),
('org_level', '车间', 'workshop', 3, 0),
('org_level', '产线', 'line', 4, 0);

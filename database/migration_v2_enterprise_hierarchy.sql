-- ============================================================================
-- QM-AI v2 Enterprise Hierarchy Migration
-- 企业层级管理 + 质量数据模型挂靠组织层级
-- ============================================================================

USE qmai;

-- ============================================================================
-- 1. 创建组织层级表 (self-referencing tree)
-- ============================================================================
CREATE TABLE IF NOT EXISTS organizations (
    id          BIGINT AUTO_INCREMENT PRIMARY KEY,
    code        VARCHAR(50) NOT NULL COMMENT '组织编码',
    name        VARCHAR(200) NOT NULL COMMENT '组织名称',
    level       VARCHAR(20) NOT NULL COMMENT '层级: group/company/workshop/line',
    parent_id   BIGINT COMMENT '父级ID',
    sort_order  INT DEFAULT 0 COMMENT '排序号',
    is_active   TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    location    VARCHAR(500) COMMENT '位置/地址',
    contact     JSON COMMENT '联系人信息',
    description TEXT COMMENT '描述',
    created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by  BIGINT COMMENT '创建人',
    FOREIGN KEY (parent_id) REFERENCES organizations(id) ON DELETE SET NULL,
    UNIQUE KEY uk_org_code (code),
    INDEX idx_org_parent (parent_id),
    INDEX idx_org_level (level),
    INDEX idx_org_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 2. 基础数据表添加 organization_id
-- ============================================================================

-- 产品表添加组织ID
ALTER TABLE products
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_products_org (org_id);

-- 工序表添加组织ID，保留department字段作为冗余
ALTER TABLE processes
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_processes_org (org_id);

-- 工艺路线表添加组织ID
ALTER TABLE routings
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_routings_org (org_id);

-- BOM表添加组织ID
ALTER TABLE boms
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_boms_org (org_id);

-- 检验标准表添加组织ID
ALTER TABLE inspection_standards
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_standards_org (org_id);

-- 不良代码表添加组织ID
ALTER TABLE defect_codes
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_defect_codes_org (org_id);

-- ============================================================================
-- 3. 设备表改造：增加org_id + 关联车间/产线到组织层级
-- ============================================================================
ALTER TABLE equipments
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD COLUMN workshop_id BIGINT COMMENT '关联车间(组织ID，level=workshop)',
    ADD COLUMN line_id BIGINT COMMENT '关联产线(组织ID，level=line)',
    ADD INDEX idx_equipment_org (org_id),
    ADD INDEX idx_equipment_workshop (workshop_id),
    ADD INDEX idx_equipment_line (line_id);

-- 保留现有workshop/productionLine字段作为显示冗余，后续可迁移

-- 刀具表添加组织ID
ALTER TABLE tools
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_tools_org (org_id);

-- 供应商表添加组织ID
ALTER TABLE suppliers
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_suppliers_org (org_id);

-- 客户表添加组织ID
ALTER TABLE customers
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_customers_org (org_id);

-- ============================================================================
-- 4. 检验项目主数据添加组织ID
-- ============================================================================
ALTER TABLE inspection_items
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_inspection_items_org (org_id);

ALTER TABLE inspection_plans
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_plans_org (org_id);

-- ============================================================================
-- 5. 动态参数添加组织ID
-- ============================================================================
ALTER TABLE param_groups
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_param_groups_org (org_id);

ALTER TABLE dynamic_params
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_dynamic_params_org (org_id);

ALTER TABLE closure_rules
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_closure_rules_org (org_id);

-- ============================================================================
-- 6. IQC 来料检验添加组织ID
-- ============================================================================
ALTER TABLE iqc_receipts
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_iqc_receipts_org (org_id);

ALTER TABLE iqc_inspections
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_iqc_inspections_org (org_id);

ALTER TABLE iqc_anomalies
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_iqc_anomalies_org (org_id);

-- ============================================================================
-- 7. IPQC 过程检验添加组织ID
-- ============================================================================
ALTER TABLE ipqc_first_pieces
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_ipqc_fp_org (org_id);

ALTER TABLE ipqc_patrol_plans
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_ipqc_plans_org (org_id);

ALTER TABLE ipqc_patrols
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_ipqc_patrols_org (org_id);

ALTER TABLE ipqc_closure_status
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_ipqc_closure_org (org_id);

-- ============================================================================
-- 8. FQC/OQC 成品检验添加组织ID
-- ============================================================================
ALTER TABLE fqc_inspections
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_fqc_inspections_org (org_id);

ALTER TABLE oqc_releases
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_oqc_releases_org (org_id);

ALTER TABLE batches
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_batches_org (org_id);

-- ============================================================================
-- 9. SPC 统计分析添加组织ID
-- ============================================================================
ALTER TABLE spc_control_charts
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_spc_charts_org (org_id);

ALTER TABLE spc_data_sources
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_spc_ds_org (org_id);

-- ============================================================================
-- 10. 缺陷/CAPA/追溯/客诉添加组织ID
-- ============================================================================
ALTER TABLE defects
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_defects_org (org_id);

ALTER TABLE capa_records
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_capa_org (org_id);

ALTER TABLE trace_records
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_trace_org (org_id);

ALTER TABLE complaints
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_complaints_org (org_id);

-- ============================================================================
-- 11. 文档/审核添加组织ID
-- ============================================================================
ALTER TABLE documents
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_documents_org (org_id);

ALTER TABLE audits
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_audits_org (org_id);

-- ============================================================================
-- 12. 用户表添加所属组织
-- ============================================================================
ALTER TABLE users
    ADD COLUMN org_id BIGINT COMMENT '所属组织',
    ADD INDEX idx_users_org (org_id);

-- ============================================================================
-- 13. 基础数据字典表（为下拉列表提供统一数据源）
-- ============================================================================
CREATE TABLE IF NOT EXISTS sys_dict_types (
    id          BIGINT AUTO_INCREMENT PRIMARY KEY,
    type_code   VARCHAR(50) UNIQUE NOT NULL COMMENT '字典类型编码',
    type_name   VARCHAR(200) NOT NULL COMMENT '字典类型名称',
    is_system   TINYINT(1) DEFAULT 0 COMMENT '系统内置',
    status      TINYINT(1) DEFAULT 1 COMMENT '状态',
    remark      TEXT COMMENT '备注',
    created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sys_dict_items (
    id          BIGINT AUTO_INCREMENT PRIMARY KEY,
    type_code   VARCHAR(50) NOT NULL COMMENT '字典类型编码',
    item_label  VARCHAR(200) NOT NULL COMMENT '显示标签',
    item_value  VARCHAR(100) NOT NULL COMMENT '选项值',
    sort_order  INT DEFAULT 0 COMMENT '排序号',
    color       VARCHAR(20) COMMENT '颜色标识',
    is_default  TINYINT(1) DEFAULT 0 COMMENT '是否默认',
    status      TINYINT(1) DEFAULT 1 COMMENT '状态',
    remark      TEXT COMMENT '备注',
    created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (type_code) REFERENCES sys_dict_types(type_code) ON DELETE CASCADE,
    INDEX idx_dict_items_type (type_code),
    INDEX idx_dict_items_sort (type_code, sort_order)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 14. 初始化系统字典数据
-- ============================================================================

-- 设备类型
INSERT INTO sys_dict_types (type_code, type_name, is_system, remark) VALUES
('equipment_type', '设备类型', 1, '设备分类字典'),
('process_type', '工序类型', 1, '工序分类字典'),
('defect_category', '不良分类', 1, '不良代码分类'),
('severity', '严重等级', 1, 'CR/MA/MI等级'),
('inspection_type', '检验类型', 1, 'IQC/IPQC/FQC/OQC'),
('product_category', '产品类别', 1, '产品分类'),
('tool_type', '刀具类型', 1, '刀具分类'),
('supply_category', '供应类别', 1, '供应商供应类别'),
('audit_type', '审核类型', 1, '审核分类'),
('doc_type', '文档类型', 1, '文档分类'),
('material_unit', '物料单位', 1, '物料/产品基本单位');

-- 设备类型选项
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('equipment_type', 'CNC加工中心', 'CNC', 1),
('equipment_type', 'PLC设备', 'PLC', 2),
('equipment_type', '检测设备', '检测设备', 3),
('equipment_type', '机器人', '机器人', 4),
('equipment_type', '其他', '其他', 99);

-- 工序类型选项
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('process_type', '加工', '加工', 1),
('process_type', '检验', '检验', 2),
('process_type', '装配', '装配', 3),
('process_type', '包装', '包装', 4),
('process_type', '热处理', '热处理', 5),
('process_type', '表面处理', '表面处理', 6);

-- 不良分类
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('defect_category', '外观', '外观', 1),
('defect_category', '尺寸', '尺寸', 2),
('defect_category', '功能', '功能', 3),
('defect_category', '材料', '材料', 4),
('defect_category', '性能', '性能', 5),
('defect_category', '其他', '其他', 99);

-- 严重等级
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order, color) VALUES
('severity', 'CR - 严重', 'CR', 1, '#F56C6C'),
('severity', 'MA - 主要', 'MA', 2, '#E6A23C'),
('severity', 'MI - 次要', 'MI', 3, '#909399');

-- 检验类型
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('inspection_type', 'IQC来料检验', 'IQC', 1),
('inspection_type', 'IPQC过程检验', 'IPQC', 2),
('inspection_type', 'FQC成品检验', 'FQC', 3),
('inspection_type', 'OQC出货检验', 'OQC', 4);

-- 产品类别
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('product_category', '成品', '成品', 1),
('product_category', '半成品', '半成品', 2),
('product_category', '原材料', '原材料', 3),
('product_category', '辅料', '辅料', 4);

-- 刀具类型
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('tool_type', '车刀', '车刀', 1),
('tool_type', '铣刀', '铣刀', 2),
('tool_type', '钻头', '钻头', 3),
('tool_type', '磨具', '磨具', 4),
('tool_type', '丝锥', '丝锥', 5),
('tool_type', '其他', '其他', 99);

-- 物料单位
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('material_unit', '个', '个', 1),
('material_unit', '件', '件', 2),
('material_unit', '套', '套', 3),
('material_unit', 'kg', 'kg', 4),
('material_unit', 'g', 'g', 5),
('material_unit', 'm', 'm', 6),
('material_unit', 'L', 'L', 7),
('material_unit', 'pcs', 'pcs', 8);

-- 供应类别
INSERT INTO sys_dict_items (type_code, item_label, item_value, sort_order) VALUES
('supply_category', '原材料', '原材料', 1),
('supply_category', '零部件', '零部件', 2),
('supply_category', '包材', '包材', 3),
('supply_category', '设备', '设备', 4),
('supply_category', '服务', '服务', 5);

-- ============================================================================
-- 15. 创建一个默认的组织层级示例数据（可选）
-- ============================================================================
-- 以下为示例数据，可按需调整
INSERT INTO organizations (code, name, level, parent_id, sort_order, is_active) VALUES
('HQ', '集团总部', 'group', NULL, 1, 1),
('FACTORY_1', '第一工厂', 'company', 1, 1, 1),
('FACTORY_2', '第二工厂', 'company', 1, 2, 1),
('WS_MACHINING', '机加车间', 'workshop', 2, 1, 1),
('WS_HEAT_TREAT', '热处理车间', 'workshop', 2, 2, 1),
('WS_ASSEMBLY', '装配车间', 'workshop', 3, 1, 1),
('WS_QUALITY', '质量中心', 'workshop', 3, 2, 1),
('LINE_A', 'A线', 'line', 4, 1, 1),
('LINE_B', 'B线', 'line', 4, 2, 1),
('LINE_C', 'C线', 'line', 5, 1, 1),
('LINE_HEAT', '热处理线', 'line', 5, 2, 1),
('LINE_ASSY_1', '装配1线', 'line', 6, 1, 1),
('LINE_ASSY_2', '装配2线', 'line', 6, 2, 1),
('LINE_QC', '质量检测线', 'line', 7, 1, 1);

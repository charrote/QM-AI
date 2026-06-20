-- ============================================================================
-- QM-AI Industrial AI Quality Decision Platform
-- MySQL 8.0+ Database Initialization Script
-- Engine: InnoDB | Charset: utf8mb4 | Collation: utf8mb4_unicode_ci
-- ============================================================================

CREATE DATABASE IF NOT EXISTS qmai CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE qmai;

-- ============================================================================
-- M15 – System Management
-- ============================================================================

CREATE TABLE users (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    display_name VARCHAR(100),
    avatar VARCHAR(500),
    email VARCHAR(100),
    phone VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    role_id BIGINT,
    last_login_at DATETIME,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE roles (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    description VARCHAR(255),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE permissions (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    code VARCHAR(100) UNIQUE NOT NULL,
    module VARCHAR(50),
    description VARCHAR(255)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE role_permissions (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    role_id BIGINT NOT NULL,
    permission_id BIGINT NOT NULL,
    FOREIGN KEY (role_id) REFERENCES roles(id),
    FOREIGN KEY (permission_id) REFERENCES permissions(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M02 – Basic Data
-- ============================================================================

CREATE TABLE products (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    product_code VARCHAR(50) UNIQUE NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    product_type VARCHAR(50),
    specification TEXT,
    unit VARCHAR(20),
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE boms (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    product_id BIGINT NOT NULL,
    material_code VARCHAR(50),
    material_name VARCHAR(200),
    quantity DECIMAL(10,3),
    unit VARCHAR(20),
    level INT,
    path VARCHAR(500),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE processes (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    process_code VARCHAR(50) UNIQUE NOT NULL,
    process_name VARCHAR(200) NOT NULL,
    process_type VARCHAR(50),
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE routings (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    routing_code VARCHAR(50) UNIQUE NOT NULL,
    routing_name VARCHAR(200) NOT NULL,
    product_id BIGINT NOT NULL,
    steps JSON,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE routing_steps (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    routing_id BIGINT NOT NULL,
    step_order INT NOT NULL,
    process_id BIGINT NOT NULL,
    process_name VARCHAR(200),
    workcenter VARCHAR(100),
    standard_time INT,
    FOREIGN KEY (routing_id) REFERENCES routings(id),
    FOREIGN KEY (process_id) REFERENCES processes(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE inspection_standards (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    standard_code VARCHAR(50) UNIQUE NOT NULL,
    standard_name VARCHAR(200) NOT NULL,
    product_id BIGINT,
    process_id BIGINT,
    inspection_type ENUM('IQC','IPQC','FQC','OQC'),
    sampling_method VARCHAR(50),
    aql DECIMAL(5,2),
    inspection_level VARCHAR(20),
    items JSON,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id),
    FOREIGN KEY (process_id) REFERENCES processes(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE defect_codes (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    defect_code VARCHAR(50) UNIQUE NOT NULL,
    defect_name VARCHAR(200) NOT NULL,
    defect_category VARCHAR(50),
    defect_severity ENUM('Critical','Major','Minor'),
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE equipments (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_code VARCHAR(50) UNIQUE NOT NULL,
    equipment_name VARCHAR(200) NOT NULL,
    equipment_type VARCHAR(50),
    model VARCHAR(100),
    manufacturer VARCHAR(200),
    installation_date DATE,
    status ENUM('running','idle','fault','maintenance') DEFAULT 'idle',
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE tools (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    tool_code VARCHAR(50) UNIQUE NOT NULL,
    tool_name VARCHAR(200) NOT NULL,
    tool_type VARCHAR(50),
    life_standard INT,
    life_current INT DEFAULT 0,
    status ENUM('active','worn','broken','retired') DEFAULT 'active',
    equipment_id BIGINT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (equipment_id) REFERENCES equipments(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE suppliers (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    supplier_code VARCHAR(50) UNIQUE NOT NULL,
    supplier_name VARCHAR(200) NOT NULL,
    contact_person VARCHAR(100),
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT,
    status ENUM('active','inactive','blacklisted') DEFAULT 'active',
    rating DECIMAL(3,2),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE customers (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    customer_code VARCHAR(50) UNIQUE NOT NULL,
    customer_name VARCHAR(200) NOT NULL,
    contact_person VARCHAR(100),
    phone VARCHAR(20),
    email VARCHAR(100),
    address TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M02.5 – Dynamic Parameters (行业解耦核心)
-- ============================================================================

-- 参数组
CREATE TABLE param_groups (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(100) NOT NULL COMMENT '组名',
    code            VARCHAR(50) UNIQUE NOT NULL COMMENT '组编码',
    description     TEXT COMMENT '描述',
    sort_order      INT DEFAULT 0 COMMENT '排序号',
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by      BIGINT NOT NULL DEFAULT 1 COMMENT '创建人ID'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 自定义参数定义
CREATE TABLE dynamic_params (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    group_id        BIGINT NOT NULL COMMENT '所属参数组ID',
    name            VARCHAR(100) NOT NULL COMMENT '参数名',
    code            VARCHAR(50) NOT NULL COMMENT '参数编码(唯一)',
    data_type       VARCHAR(20) NOT NULL DEFAULT 'numeric' COMMENT '数据类型: numeric/categorical/boolean',
    unit            VARCHAR(20) COMMENT '单位',
    target_value    DECIMAL(15,6) COMMENT '目标值',
    usl             DECIMAL(15,6) COMMENT '上规格限',
    lsl             DECIMAL(15,6) COMMENT '下规格限',
    precision       DECIMAL(10,2) DEFAULT 1.0 COMMENT '精度/小数位数',
    ai_strategy     JSON COMMENT 'AI策略预置配置',
    sort_order      INT DEFAULT 0 COMMENT '排序号',
    is_active       TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by      BIGINT NOT NULL DEFAULT 1 COMMENT '创建人ID',
    FOREIGN KEY (group_id) REFERENCES param_groups(id) ON DELETE CASCADE,
    UNIQUE KEY uk_code (code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 关单策略模板
CREATE TABLE closure_rules (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    name            VARCHAR(100) NOT NULL COMMENT '规则名称',
    code            VARCHAR(50) UNIQUE NOT NULL COMMENT '规则编码',
    condition_json  JSON NOT NULL COMMENT '条件表达式JSON',
    logic           VARCHAR(5) DEFAULT 'AND' COMMENT '逻辑运算符 AND/OR',
    description     TEXT COMMENT '描述',
    is_active       TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by      BIGINT NOT NULL DEFAULT 1 COMMENT '创建人ID'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 实时参数值（高频写入，按参数编码+时间索引）
CREATE TABLE param_realtime_values (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    param_code      VARCHAR(50) NOT NULL COMMENT '参数编码',
    equipment_id    BIGINT COMMENT '设备ID',
    value           DECIMAL(15,6) COMMENT '数值(数值型)',
    value_raw       VARCHAR(100) COMMENT '原始值(枚举型/布尔型)',
    timestamp       DATETIME NOT NULL COMMENT '采集时间',
    quality_result  VARCHAR(10) DEFAULT 'UNKNOWN' COMMENT '质量结果 OK/NG/UNKNOWN',
    INDEX idx_param_time (param_code, timestamp),
    INDEX idx_equip_time (equipment_id, timestamp)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M03 – IQC (Incoming Quality Control)
-- ============================================================================

CREATE TABLE iqc_receipts (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    receipt_no VARCHAR(50) UNIQUE NOT NULL,
    supplier_id BIGINT,
    product_id BIGINT,
    batch_no VARCHAR(100),
    quantity INT,
    unit VARCHAR(20),
    receipt_date DATETIME,
    inspector VARCHAR(100),
    status ENUM('pending','inspecting','completed','anomaly') DEFAULT 'pending',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE iqc_inspections (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    inspection_no VARCHAR(50) UNIQUE NOT NULL,
    receipt_id BIGINT,
    standard_id BIGINT,
    sample_size INT,
    ac INT,
    re INT,
    defect_qty INT DEFAULT 0,
    result ENUM('pending','pass','fail','scrap') DEFAULT 'pending',
    inspector VARCHAR(100),
    inspected_at DATETIME,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (receipt_id) REFERENCES iqc_receipts(id),
    FOREIGN KEY (standard_id) REFERENCES inspection_standards(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE iqc_inspection_items (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    inspection_id BIGINT,
    param_id BIGINT COMMENT '关联dynamic_params.id',
    measured_value DECIMAL(12,4),
    result ENUM('pass','fail'),
    defect_code_id BIGINT,
    remark TEXT,
    FOREIGN KEY (inspection_id) REFERENCES iqc_inspections(id),
    FOREIGN KEY (param_id) REFERENCES dynamic_params(id),
    FOREIGN KEY (defect_code_id) REFERENCES defect_codes(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE iqc_anomalies (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    anomaly_no VARCHAR(50) UNIQUE NOT NULL,
    receipt_id BIGINT,
    inspection_id BIGINT,
    anomaly_type ENUM('quality','quantity','document','other'),
    severity ENUM('critical','major','minor'),
    description TEXT,
    status ENUM('open','processing','resolved','closed') DEFAULT 'open',
    handler VARCHAR(100),
    resolved_at DATETIME,
    FOREIGN KEY (receipt_id) REFERENCES iqc_receipts(id),
    FOREIGN KEY (inspection_id) REFERENCES iqc_inspections(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE supplier_scores (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    supplier_id BIGINT,
    score_date DATE,
    score DECIMAL(5,2),
    dimension_scores JSON,
    grade ENUM('A','B','C','D'),
    evaluation TEXT,
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M04 – IPQC (In-Process Quality Control)
-- ============================================================================

CREATE TABLE ipqc_first_pieces (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    fp_no VARCHAR(50) UNIQUE NOT NULL,
    product_id BIGINT,
    process_id BIGINT,
    equipment_id BIGINT,
    work_order VARCHAR(100),
    batch_no VARCHAR(100),
    inspector VARCHAR(100),
    result ENUM('pending','pass','fail') DEFAULT 'pending',
    checked_at DATETIME,
    FOREIGN KEY (product_id) REFERENCES products(id),
    FOREIGN KEY (process_id) REFERENCES processes(id),
    FOREIGN KEY (equipment_id) REFERENCES equipments(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ipqc_patrol_plans (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    plan_no VARCHAR(50) UNIQUE NOT NULL,
    equipment_id BIGINT,
    process_id BIGINT,
    interval_minutes INT,
    inspector VARCHAR(100),
    status ENUM('active','paused','completed') DEFAULT 'active',
    FOREIGN KEY (equipment_id) REFERENCES equipments(id),
    FOREIGN KEY (process_id) REFERENCES processes(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ipqc_patrols (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    patrol_no VARCHAR(50) UNIQUE NOT NULL,
    plan_id BIGINT,
    inspector VARCHAR(100),
    patrol_time DATETIME,
    equipment_id BIGINT,
    process_id BIGINT,
    result ENUM('pending','pass','fail') DEFAULT 'pending',
    remark TEXT,
    FOREIGN KEY (plan_id) REFERENCES ipqc_patrol_plans(id),
    FOREIGN KEY (equipment_id) REFERENCES equipments(id),
    FOREIGN KEY (process_id) REFERENCES processes(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ipqc_closure_status (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    work_order VARCHAR(100),
    status ENUM('open','closed') DEFAULT 'open',
    closed_at DATETIME,
    rule_id BIGINT,
    spc_result JSON,
    FOREIGN KEY (rule_id) REFERENCES closure_rules(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M05 – FQC (Final Quality Control) & OQC (Outgoing Quality Control)
-- ============================================================================

CREATE TABLE fqc_inspections (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    inspection_no VARCHAR(50) UNIQUE NOT NULL,
    product_id BIGINT,
    batch_no VARCHAR(100),
    quantity INT,
    sample_size INT,
    inspection_type ENUM('full','sampling'),
    result ENUM('pending','pass','fail') DEFAULT 'pending',
    inspector VARCHAR(100),
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE fqc_inspection_items (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    inspection_id BIGINT,
    param_id BIGINT,
    measured_value DECIMAL(12,4),
    result ENUM('pass','fail'),
    FOREIGN KEY (inspection_id) REFERENCES fqc_inspections(id),
    FOREIGN KEY (param_id) REFERENCES dynamic_params(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE oqc_releases (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    release_no VARCHAR(50) UNIQUE NOT NULL,
    inspection_id BIGINT,
    release_date DATETIME,
    released_by VARCHAR(100),
    signature_url VARCHAR(500),
    status ENUM('pending','released','rejected'),
    FOREIGN KEY (inspection_id) REFERENCES fqc_inspections(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE batches (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    batch_no VARCHAR(100) UNIQUE NOT NULL,
    product_id BIGINT,
    quantity INT,
    production_date DATE,
    expiry_date DATE,
    status ENUM('pending','released','blocked','scrapped'),
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M07 – Defects & CAPA (Corrective and Preventive Action)
-- ============================================================================

CREATE TABLE defects (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    defect_no VARCHAR(50) UNIQUE NOT NULL,
    source ENUM('IQC','IPQC','FQC','OQC','complaint','audit'),
    source_ref_id BIGINT,
    product_id BIGINT,
    defect_code_id BIGINT,
    quantity INT,
    severity ENUM('critical','major','minor'),
    description TEXT,
    discovered_at DATETIME,
    discovered_by VARCHAR(100),
    FOREIGN KEY (product_id) REFERENCES products(id),
    FOREIGN KEY (defect_code_id) REFERENCES defect_codes(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE capa_records (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_no VARCHAR(50) UNIQUE NOT NULL,
    defect_id BIGINT,
    title VARCHAR(200),
    description TEXT,
    root_cause TEXT,
    temp_action TEXT,
    corrective_action TEXT,
    preventive_action TEXT,
    status ENUM('open','analysis','temp_action','corrective','preventive','verify','closed') DEFAULT 'open',
    created_at DATETIME,
    closed_at DATETIME,
    FOREIGN KEY (defect_id) REFERENCES defects(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE capa_five_whys (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT,
    level INT,
    question TEXT,
    answer TEXT,
    FOREIGN KEY (capa_id) REFERENCES capa_records(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE capa_fishbone (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT,
    category VARCHAR(50),
    cause TEXT,
    FOREIGN KEY (capa_id) REFERENCES capa_records(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE scrap_rework (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    record_no VARCHAR(50) UNIQUE NOT NULL,
    defect_id BIGINT,
    type ENUM('scrap','rework'),
    quantity INT,
    cost DECIMAL(12,2),
    reason TEXT,
    handler VARCHAR(100),
    FOREIGN KEY (defect_id) REFERENCES defects(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M08 – Trace (Product Traceability)
-- ============================================================================

CREATE TABLE trace_records (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    trace_no VARCHAR(50),
    product_id BIGINT,
    batch_no VARCHAR(100),
    sn VARCHAR(100),
    trace_type ENUM('sn','batch','equipment','tool'),
    trace_data JSON,
    created_at DATETIME,
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M09 – Complaints & 8D Reports
-- ============================================================================

CREATE TABLE complaints (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    complaint_no VARCHAR(50) UNIQUE NOT NULL,
    customer_id BIGINT,
    product_id BIGINT,
    batch_no VARCHAR(100),
    complaint_date DATE,
    description TEXT,
    severity ENUM('critical','major','minor'),
    status ENUM('open','investigating','8d_in_progress','resolved','closed') DEFAULT 'open',
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE d8_reports (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    complaint_id BIGINT UNIQUE,
    d0_actions TEXT,
    d1_team TEXT,
    d2_problem TEXT,
    d3_interim TEXT,
    d4_root_cause TEXT,
    d5_permanent TEXT,
    d6_implement TEXT,
    d7_prevent TEXT,
    d8_celebrate TEXT,
    status VARCHAR(20) DEFAULT 'd0',
    created_at DATETIME,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (complaint_id) REFERENCES complaints(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M11 – Equipment
-- ============================================================================

CREATE TABLE equipment_param_mappings (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_id BIGINT,
    param_id BIGINT,
    mqtt_topic VARCHAR(500),
    data_path VARCHAR(200),
    transform_expression VARCHAR(500),
    FOREIGN KEY (equipment_id) REFERENCES equipments(id),
    FOREIGN KEY (param_id) REFERENCES dynamic_params(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE equipment_status_history (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_id BIGINT,
    status VARCHAR(20),
    started_at DATETIME,
    ended_at DATETIME,
    FOREIGN KEY (equipment_id) REFERENCES equipments(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE equipment_quality_correlation (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_id BIGINT,
    analysis_date DATE,
    correlation_data JSON,
    conclusion TEXT,
    FOREIGN KEY (equipment_id) REFERENCES equipments(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M10 – AI (Artificial Intelligence)
-- ============================================================================

CREATE TABLE ai_warnings (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    warning_type VARCHAR(50),
    severity ENUM('info','warning','critical'),
    title VARCHAR(200),
    description TEXT,
    source_module VARCHAR(50),
    source_ref_id BIGINT,
    is_read BOOLEAN DEFAULT FALSE,
    created_at DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ai_models (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    model_name VARCHAR(200),
    model_type VARCHAR(50),
    version VARCHAR(20),
    status ENUM('training','deployed','archived','failed'),
    metrics JSON,
    file_path VARCHAR(500),
    trained_at DATETIME,
    deployed_at DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE ai_analysis_results (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    analysis_type ENUM('risk','detection','prediction','root_cause','optimization'),
    source_module VARCHAR(50),
    source_ref_id BIGINT,
    result JSON,
    confidence DECIMAL(5,4),
    created_at DATETIME
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- M12–M14 – Documents, Audits & Knowledge
-- ============================================================================

CREATE TABLE documents (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    doc_code VARCHAR(50) UNIQUE NOT NULL,
    title VARCHAR(200),
    doc_type VARCHAR(50),
    version VARCHAR(20),
    file_url VARCHAR(500),
    file_size BIGINT,
    status ENUM('draft','review','approved','obsolete'),
    created_by BIGINT,
    created_at DATETIME,
    FOREIGN KEY (created_by) REFERENCES users(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE document_versions (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    document_id BIGINT,
    version VARCHAR(20),
    file_url VARCHAR(500),
    change_notes TEXT,
    uploaded_by BIGINT,
    uploaded_at DATETIME,
    FOREIGN KEY (document_id) REFERENCES documents(id),
    FOREIGN KEY (uploaded_by) REFERENCES users(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE audits (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    audit_no VARCHAR(50) UNIQUE NOT NULL,
    audit_type VARCHAR(50),
    title VARCHAR(200),
    plan_date DATE,
    auditor VARCHAR(100),
    audited_dept VARCHAR(100),
    status ENUM('planned','in_progress','completed','closed'),
    findings JSON,
    report_url VARCHAR(500)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- Indexes
-- ============================================================================

-- Users & Auth
CREATE INDEX idx_users_role_id ON users(role_id);
CREATE INDEX idx_users_is_active ON users(is_active);
CREATE INDEX idx_role_permissions_role_id ON role_permissions(role_id);
CREATE INDEX idx_role_permissions_permission_id ON role_permissions(permission_id);
CREATE INDEX idx_permissions_module ON permissions(module);

-- Basic Data
CREATE INDEX idx_products_type ON products(product_type);
CREATE INDEX idx_products_active ON products(is_active);
CREATE INDEX idx_boms_product_id ON boms(product_id);
CREATE INDEX idx_processes_type ON processes(process_type);
CREATE INDEX idx_routings_product_id ON routings(product_id);
CREATE INDEX idx_routing_steps_routing_id ON routing_steps(routing_id);
CREATE INDEX idx_routing_steps_process_id ON routing_steps(process_id);
CREATE INDEX idx_standards_product_id ON inspection_standards(product_id);
CREATE INDEX idx_standards_process_id ON inspection_standards(process_id);
CREATE INDEX idx_standards_type ON inspection_standards(inspection_type);
CREATE INDEX idx_defect_codes_category ON defect_codes(defect_category);
CREATE INDEX idx_defect_codes_severity ON defect_codes(defect_severity);
CREATE INDEX idx_equipment_status ON equipments(status);
CREATE INDEX idx_tools_equipment_id ON tools(equipment_id);
CREATE INDEX idx_tools_status ON tools(status);
CREATE INDEX idx_suppliers_status ON suppliers(status);
CREATE INDEX idx_customers_active ON customers(is_active);

-- Dynamic Parameters
CREATE INDEX idx_dynamic_params_group_id ON dynamic_params(group_id);
CREATE INDEX idx_dynamic_params_code ON dynamic_params(code);
CREATE INDEX idx_dynamic_params_active ON dynamic_params(is_active);
CREATE INDEX idx_param_realtime_values_param ON param_realtime_values(param_code, timestamp);
CREATE INDEX idx_param_realtime_values_equip ON param_realtime_values(equipment_id, timestamp);

-- IQC
CREATE INDEX idx_iqc_receipts_supplier ON iqc_receipts(supplier_id);
CREATE INDEX idx_iqc_receipts_product ON iqc_receipts(product_id);
CREATE INDEX idx_iqc_receipts_status ON iqc_receipts(status);
CREATE INDEX idx_iqc_inspections_receipt ON iqc_inspections(receipt_id);
CREATE INDEX idx_iqc_inspections_result ON iqc_inspections(result);
CREATE INDEX idx_iqc_inspection_items_inspection ON iqc_inspection_items(inspection_id);
CREATE INDEX idx_iqc_anomalies_status ON iqc_anomalies(status);
CREATE INDEX idx_supplier_scores_supplier ON supplier_scores(supplier_id);
CREATE INDEX idx_supplier_scores_date ON supplier_scores(score_date);

-- IPQC
CREATE INDEX idx_ipqc_fp_product ON ipqc_first_pieces(product_id);
CREATE INDEX idx_ipqc_fp_process ON ipqc_first_pieces(process_id);
CREATE INDEX idx_ipqc_fp_result ON ipqc_first_pieces(result);
CREATE INDEX idx_ipqc_patrols_plan ON ipqc_patrols(plan_id);
CREATE INDEX idx_ipqc_patrols_time ON ipqc_patrols(patrol_time);
CREATE INDEX idx_ipqc_patrols_result ON ipqc_patrols(result);
CREATE INDEX idx_ipqc_plans_equipment ON ipqc_patrol_plans(equipment_id);
CREATE INDEX idx_ipqc_closure_wo ON ipqc_closure_status(work_order);

-- FQC/OQC
CREATE INDEX idx_fqc_inspections_product ON fqc_inspections(product_id);
CREATE INDEX idx_fqc_inspections_result ON fqc_inspections(result);
CREATE INDEX idx_fqc_items_inspection ON fqc_inspection_items(inspection_id);
CREATE INDEX idx_oqc_releases_inspection ON oqc_releases(inspection_id);
CREATE INDEX idx_oqc_releases_status ON oqc_releases(status);
CREATE INDEX idx_batches_product ON batches(product_id);
CREATE INDEX idx_batches_status ON batches(status);

-- CAPA
CREATE INDEX idx_defects_source ON defects(source);
CREATE INDEX idx_defects_product ON defects(product_id);
CREATE INDEX idx_defects_severity ON defects(severity);
CREATE INDEX idx_capa_defect ON capa_records(defect_id);
CREATE INDEX idx_capa_status ON capa_records(status);
CREATE INDEX idx_capa_whys_capa ON capa_five_whys(capa_id);
CREATE INDEX idx_capa_fishbone_capa ON capa_fishbone(capa_id);
CREATE INDEX idx_scrap_defect ON scrap_rework(defect_id);

-- Trace
CREATE INDEX idx_trace_product ON trace_records(product_id);
CREATE INDEX idx_trace_batch ON trace_records(batch_no);
CREATE INDEX idx_trace_sn ON trace_records(sn);
CREATE INDEX idx_trace_type ON trace_records(trace_type);

-- Complaints
CREATE INDEX idx_complaints_customer ON complaints(customer_id);
CREATE INDEX idx_complaints_product ON complaints(product_id);
CREATE INDEX idx_complaints_status ON complaints(status);
CREATE INDEX idx_d8_complaint ON d8_reports(complaint_id);

-- Equipment
CREATE INDEX idx_equip_param_mapping_equipment ON equipment_param_mappings(equipment_id);
CREATE INDEX idx_equip_param_mapping_param ON equipment_param_mappings(param_id);
CREATE INDEX idx_equip_status_history_equipment ON equipment_status_history(equipment_id);
CREATE INDEX idx_equip_status_history_time ON equipment_status_history(started_at);
CREATE INDEX idx_equip_correlation_equipment ON equipment_quality_correlation(equipment_id);
CREATE INDEX idx_equip_correlation_date ON equipment_quality_correlation(analysis_date);

-- AI
CREATE INDEX idx_ai_warnings_type ON ai_warnings(warning_type);
CREATE INDEX idx_ai_warnings_severity ON ai_warnings(severity);
CREATE INDEX idx_ai_warnings_created ON ai_warnings(created_at);
CREATE INDEX idx_ai_models_type ON ai_models(model_type);
CREATE INDEX idx_ai_models_status ON ai_models(status);
CREATE INDEX idx_ai_results_type ON ai_analysis_results(analysis_type);
CREATE INDEX idx_ai_results_source ON ai_analysis_results(source_module, source_ref_id);
CREATE INDEX idx_ai_results_created ON ai_analysis_results(created_at);

-- Documents
CREATE INDEX idx_documents_status ON documents(status);
CREATE INDEX idx_documents_type ON documents(doc_type);
CREATE INDEX idx_doc_versions_document ON document_versions(document_id);

-- Audits
CREATE INDEX idx_audits_status ON audits(status);
CREATE INDEX idx_audits_type ON audits(audit_type);
CREATE INDEX idx_audits_date ON audits(plan_date);

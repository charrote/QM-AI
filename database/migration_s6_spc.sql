-- ============================================================================
-- QM-AI Sprint 6 — M06 SPC 统计分析 数据库迁移
-- 新增 7 张表：spc_control_charts, spc_data_points, spc_analysis_results,
--   spc_alert_rules, spc_alert_triggers, spc_anova_results, spc_data_sources
-- 基于：Part4_SPC统计分析_不良与异常管理.md
-- 日期：2026-07-07
-- ============================================================================

-- 1. SPC 控制图
CREATE TABLE IF NOT EXISTS spc_control_charts (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    name                VARCHAR(200) NOT NULL,
    process_id          BIGINT NOT NULL,
    parameter_code      VARCHAR(50) NOT NULL,
    chart_type          VARCHAR(10) NOT NULL COMMENT 'Xbar_R / Xbar_S / I_MR',
    subgroup_size       INT NOT NULL DEFAULT 5,
    usl                 DECIMAL(15,6),
    lsl                 DECIMAL(15,6),
    target_value        DECIMAL(15,6),
    cl                  DECIMAL(15,6) COMMENT 'Center Line',
    ucl                 DECIMAL(15,6) COMMENT 'Upper Control Limit',
    lcl                 DECIMAL(15,6) COMMENT 'Lower Control Limit',
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    created_by          BIGINT NOT NULL,
    FOREIGN KEY (process_id) REFERENCES processes(id),
    INDEX idx_spc_chart_name (name),
    INDEX idx_spc_chart_param (parameter_code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. SPC 数据点
CREATE TABLE IF NOT EXISTS spc_data_points (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    chart_id            BIGINT NOT NULL,
    subgroup_index      INT NOT NULL COMMENT '子组编号',
    individual_values   JSON NOT NULL COMMENT '子组内原始值数组',
    subgroup_mean       DECIMAL(15,6) COMMENT 'X̄',
    subgroup_range      DECIMAL(15,6) COMMENT 'R (或 S)',
    measured_at         DATETIME NOT NULL,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (chart_id) REFERENCES spc_control_charts(id) ON DELETE CASCADE,
    INDEX idx_spc_dp_chart_subgroup (chart_id, subgroup_index)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. SPC 分析结果
CREATE TABLE IF NOT EXISTS spc_analysis_results (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    chart_id            BIGINT NOT NULL,
    analysis_type       VARCHAR(20) NOT NULL COMMENT 'cpk / ppk / capability',
    cp                  DECIMAL(10,4),
    cpk                 DECIMAL(10,4),
    pp                  DECIMAL(10,4),
    ppk                 DECIMAL(10,4),
    sigma_within        DECIMAL(15,6),
    sigma_overall       DECIMAL(15,6),
    estimated_ppm       DECIMAL(15,2) COMMENT '估计 DPMO',
    data_points_used    INT COMMENT '参与分析的数据点数量',
    analysis_period_start DATE,
    analysis_period_end   DATE,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (chart_id) REFERENCES spc_control_charts(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 4. Western Electric 判异规则
CREATE TABLE IF NOT EXISTS spc_alert_rules (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    chart_id            BIGINT NOT NULL,
    rule_number         INT NOT NULL COMMENT '1-8',
    rule_name           VARCHAR(200) NOT NULL,
    rule_description    TEXT,
    enabled             TINYINT(1) DEFAULT 1,
    trigger_threshold   INT DEFAULT 1 COMMENT '触发阈值(如连续N点)',
    sigma_threshold     DECIMAL(5,2) DEFAULT 2.0 COMMENT 'σ阈值',
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (chart_id) REFERENCES spc_control_charts(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 5. 判异报警触发记录
CREATE TABLE IF NOT EXISTS spc_alert_triggers (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    chart_id            BIGINT NOT NULL,
    rule_id             BIGINT NOT NULL,
    rule_number         INT NOT NULL,
    triggered_at        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    violated_point_index INT NOT NULL COMMENT '触发点子组索引',
    detail              JSON COMMENT '触发详情',
    resolved            TINYINT(1) DEFAULT 0,
    resolved_at         DATETIME,
    FOREIGN KEY (chart_id) REFERENCES spc_control_charts(id),
    FOREIGN KEY (rule_id) REFERENCES spc_alert_rules(id),
    INDEX idx_spc_at_chart_time (chart_id, triggered_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 6. 方差分析结果
CREATE TABLE IF NOT EXISTS spc_anova_results (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    chart_id            BIGINT NOT NULL,
    source              VARCHAR(20) NOT NULL COMMENT 'operator/machine/material/method/environment',
    sum_of_squares      DECIMAL(20,4),
    degrees_freedom     INT,
    mean_square         DECIMAL(20,4),
    f_ratio             DECIMAL(10,4),
    p_value             DECIMAL(10,6),
    significant         TINYINT(1) DEFAULT 0,
    analysis_date       DATE NOT NULL,
    FOREIGN KEY (chart_id) REFERENCES spc_control_charts(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 7. SPC 数据源配置（贯通 IQC/IPQC/FQC）
CREATE TABLE IF NOT EXISTS spc_data_sources (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    chart_id            BIGINT NOT NULL,
    source_type         VARCHAR(10) NOT NULL COMMENT 'iqc / ipqc / fqc',
    source_category     VARCHAR(50),
    inspection_item_id  BIGINT,
    filter_conditions   JSON COMMENT '筛选条件（product_id, process_id, equipment_id 等）',
    field_mapping       JSON COMMENT '字段映射（measurement_value, measured_at 等）',
    enabled             TINYINT(1) DEFAULT 1,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (chart_id) REFERENCES spc_control_charts(id) ON DELETE CASCADE,
    FOREIGN KEY (inspection_item_id) REFERENCES inspection_items(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================================
-- 插入默认 Western Electric 8 大判异规则（在 SpcService 创建控制图时自动生成）
-- 此处仅做说明，不预先插入
-- ============================================================================
-- ============================================================
-- M07 不良与异常管理 - 数据库迁移脚本
-- 基于设计文档 Part4 Schema
-- ============================================================

-- 缺陷记录
CREATE TABLE IF NOT EXISTS defects (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    defect_code         VARCHAR(50) NOT NULL,
    severity            VARCHAR(10) NOT NULL DEFAULT 'major' COMMENT 'critical/major/minor',
    source_type         VARCHAR(10) NOT NULL COMMENT 'iqc/ipqc/fqc/oqc/customer',
    source_id           BIGINT COMMENT '来源ID(检验单/客诉等)',
    product_id          BIGINT,
    batch_id            BIGINT,
    equipment_id        BIGINT,
    quantity            DECIMAL(15,2) NOT NULL,
    description         TEXT NOT NULL,
    image_urls          JSON,
    discovered_by       BIGINT,
    discovered_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status              VARCHAR(20) NOT NULL DEFAULT 'open' COMMENT 'open/investigating/resolved/closed',
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_defect_code (defect_code),
    INDEX idx_source_type (source_type),
    INDEX idx_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- CAPA 单
CREATE TABLE IF NOT EXISTS capa (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    capa_code           VARCHAR(50) NOT NULL UNIQUE,
    defect_id           BIGINT,
    anomaly_id          BIGINT,
    complaint_id        BIGINT,
    severity            VARCHAR(10) NOT NULL DEFAULT 'major' COMMENT 'critical/major/minor',
    title               VARCHAR(500) NOT NULL,
    description         TEXT NOT NULL,
    current_phase        INT NOT NULL DEFAULT 0 COMMENT '0=创建/1=临时措施/2=根因分析/3=纠正措施/4=预防措施/5=验证/6=关闭',
    status              VARCHAR(20) NOT NULL DEFAULT 'open' COMMENT 'open/in_progress/completed/closed/cancelled',
    created_by          BIGINT NOT NULL,
    assigned_to         BIGINT,
    due_date            DATE,
    closed_at           DATETIME,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_status (status),
    INDEX idx_phase (current_phase),
    FOREIGN KEY (defect_id) REFERENCES defects(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- CAPA 临时措施（围堵）
CREATE TABLE IF NOT EXISTS capa_temporary_measures (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    capa_id             BIGINT NOT NULL,
    description         TEXT NOT NULL,
    executed_by         BIGINT,
    executed_at         DATETIME,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- CAPA 原因分析
CREATE TABLE IF NOT EXISTS capa_root_causes (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    capa_id             BIGINT NOT NULL,
    analysis_method     VARCHAR(20) NOT NULL DEFAULT 'five_whys' COMMENT 'five_whys/fishbone/other',
    content             JSON NOT NULL COMMENT '5Why问答或鱼骨图数据',
    root_cause_summary  TEXT NOT NULL,
    created_by          BIGINT NOT NULL,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- CAPA 纠正措施
CREATE TABLE IF NOT EXISTS capa_corrective_actions (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    capa_id             BIGINT NOT NULL,
    action_description  TEXT NOT NULL,
    responsible_person  BIGINT NOT NULL,
    due_date            DATE NOT NULL,
    status              VARCHAR(20) NOT NULL DEFAULT 'pending' COMMENT 'pending/in_progress/completed',
    completed_at        DATETIME,
    remarks             TEXT,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- CAPA 预防措施
CREATE TABLE IF NOT EXISTS capa_preventive_actions (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    capa_id             BIGINT NOT NULL,
    action_description  TEXT NOT NULL,
    responsible_person  BIGINT NOT NULL,
    due_date            DATE NOT NULL,
    status              VARCHAR(20) NOT NULL DEFAULT 'pending' COMMENT 'pending/in_progress/completed',
    completed_at        DATETIME,
    remarks             TEXT,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- CAPA 验证记录
CREATE TABLE IF NOT EXISTS capa_verifications (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    capa_id             BIGINT NOT NULL,
    verifier_id         BIGINT NOT NULL,
    verification_date   DATETIME NOT NULL,
    conclusion          VARCHAR(20) NOT NULL COMMENT 'effective/not_effective/requires_revision',
    evidence            TEXT,
    image_urls          JSON,
    remarks             TEXT,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 报废/返工记录
CREATE TABLE IF NOT EXISTS scrap_rework_records (
    id                  BIGINT PRIMARY KEY AUTO_INCREMENT,
    type                VARCHAR(10) NOT NULL COMMENT 'scrap/rework',
    defect_id           BIGINT,
    batch_id            BIGINT,
    quantity            DECIMAL(15,2) NOT NULL,
    reason              TEXT NOT NULL,
    rework_steps        JSON COMMENT '返工步骤(仅返工)',
    rework_inspection_required TINYINT(1) DEFAULT 0,
    rework_inspection_result VARCHAR(10) DEFAULT 'pending' COMMENT 'pass/fail/pending',
    authorized_by       BIGINT NOT NULL,
    authorized_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_type (type),
    FOREIGN KEY (defect_id) REFERENCES defects(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- 种子数据 - 在 DbInitializer 中通过 EF Core 注入
-- ============================================================
-- 注意：M07 的种子数据（如示范 CAPA 单）将在 DbInitializer 中通过 EF Core 管理
-- 此处仅提供 DDL 表结构
-- ============================================================================
-- QM-AI 修复脚本：补齐缺失表（d8_reports / documents / document_versions / complaint_events）
-- 用途：当前库缺少 EF 实体需要的 4 张表，导致客诉 8D、文档管理等页面丢数据
-- 幂等：表已存在时跳过（CREATE TABLE IF NOT EXISTS）
-- 执行方式：docker exec -i qm-ai-mysql mysql -u root -p... qmai < fix_missing_tables.sql
-- ============================================================================

USE qmai;

-- ---------------------------------------------------------------------------
-- 1. d8_reports（M09 客诉 8D 报告，一对一关联客诉）
--    按 EF 实体 D8Report.cs 对齐，含 D0~D8 全字段
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS d8_reports (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    complaint_id BIGINT NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'in_progress' COMMENT 'in_progress/completed/closed',
    d0_description TEXT COMMENT 'D0 问题概述',
    d1_team TEXT COMMENT 'D1 改善小组成员（JSON数组）',
    d2_description TEXT COMMENT 'D2 问题描述(5W2H)',
    d2_problem_desc TEXT COMMENT 'D2 问题描述别名',
    d3_measures TEXT COMMENT 'D3 临时围堵措施（JSON数组）',
    d4_analysis_method VARCHAR(20) COMMENT 'five_whys/fishbone/other',
    d4_content TEXT COMMENT 'D4 分析数据（5Why/鱼骨图 JSON）',
    d4_root_cause TEXT COMMENT 'D4 根本原因总结',
    d5_actions TEXT COMMENT 'D5 永久纠正措施（JSON数组）',
    d6_verification TEXT COMMENT 'D6 实施验证记录（JSON数组）',
    d7_preventive TEXT COMMENT 'D7 预防措施（JSON数组）',
    d8_thanks TEXT COMMENT 'D8 小组祝贺与知识共享',
    current_discipline INT NOT NULL DEFAULT 0 COMMENT '当前步骤 D0~D8',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    completed_at DATETIME COMMENT '完成时间',
    UNIQUE KEY uk_d8_complaint (complaint_id),
    CONSTRAINT fk_d8_complaint FOREIGN KEY (complaint_id) REFERENCES complaints(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- 2. documents（M12 文件管理，主表）
--    按 EF 实体 Document.cs 对齐
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS documents (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(500) NOT NULL,
    doc_type VARCHAR(20) NOT NULL COMMENT 'sop/work_instruction/inspection_standard/8d_report/audit_report/other',
    minio_key VARCHAR(500) NOT NULL COMMENT 'MinIO对象键',
    file_size_bytes BIGINT COMMENT '文件大小（字节）',
    file_hash VARCHAR(64) COMMENT 'SHA-256哈希',
    version INT NOT NULL DEFAULT 1,
    status VARCHAR(10) NOT NULL DEFAULT 'draft' COMMENT 'draft/reviewing/approved/archived',
    approved_by BIGINT COMMENT '审批人ID',
    approved_by_str VARCHAR(200) COMMENT '审批人标识（字符串）',
    rejection_reason VARCHAR(500) COMMENT '驳回理由',
    approved_at DATETIME COMMENT '审批时间',
    expires_at DATE COMMENT '有效期',
    created_by BIGINT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_documents_status (status),
    INDEX idx_documents_type (doc_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- 3. document_versions（M12 文件版本历史，级联删除）
--    按 EF 实体 DocumentVersion.cs 对齐
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS document_versions (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    document_id BIGINT NOT NULL,
    version INT NOT NULL,
    minio_key VARCHAR(500) NOT NULL,
    change_description TEXT COMMENT '变更说明',
    created_by BIGINT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_doc_versions_document (document_id),
    CONSTRAINT fk_doc_versions_document FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ---------------------------------------------------------------------------
-- 4. complaint_events（M09 客诉时间线事件）
--    按 EF 实体 ComplaintEvent.cs 对齐
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS complaint_events (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    complaint_id BIGINT NOT NULL COMMENT '关联客诉',
    event_type VARCHAR(20) NOT NULL COMMENT '事件类型',
    event_data TEXT COMMENT '事件数据（JSON）',
    created_by BIGINT NOT NULL COMMENT '操作用户',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '发生时间',
    INDEX idx_events_complaint (complaint_id),
    INDEX idx_events_type (event_type),
    INDEX idx_events_created (created_at),
    CONSTRAINT fk_events_complaint FOREIGN KEY (complaint_id) REFERENCES complaints(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 修复完成提示
SELECT 'fix_missing_tables.sql done' AS status;

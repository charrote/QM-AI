-- ============================================================================
-- QM-AI M10 — AI 预警中心
-- 建表：ai_alerts（预警记录，含级别/来源/解决状态）+ 演示种子数据
-- 幂等可重复执行（表不存在则建；种子数据按 alert_no 唯一键先删后插）。
-- 执行：cat database/migration_m10_ai_alerts.sql | docker exec -i qm-ai-mysql mysql -u root -p'qmai_root_2024' qmai
-- ============================================================================

SET NAMES utf8mb4;

-- ── 1. 建表 ──
CREATE TABLE IF NOT EXISTS ai_alerts (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    alert_no VARCHAR(50) NOT NULL,
    level VARCHAR(20) NOT NULL DEFAULT 'warning' COMMENT '预警级别：critical / warning / info',
    title VARCHAR(200) NOT NULL,
    description TEXT COMMENT '预警详情',
    source VARCHAR(20) NOT NULL COMMENT '来源模块：spc/iqc/fqc/ipqc/anomaly/capa/complaint',
    source_ref_id BIGINT DEFAULT NULL COMMENT '来源记录主键',
    product_id BIGINT DEFAULT NULL COMMENT '关联产品',
    resolved TINYINT(1) NOT NULL DEFAULT 0 COMMENT '是否已解决',
    resolved_at DATETIME DEFAULT NULL,
    resolved_by BIGINT DEFAULT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uk_ai_alerts_no (alert_no),
    INDEX idx_ai_alerts_level (level),
    INDEX idx_ai_alerts_source (source),
    INDEX idx_ai_alerts_resolved (resolved),
    INDEX idx_ai_alerts_created (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ── 2. 演示种子数据（贴近当前日期 2026-09，覆盖 4 个级别/来源）──
DELETE FROM ai_alerts WHERE alert_no LIKE 'AI-ALERT-2026%';

INSERT INTO ai_alerts (alert_no, level, title, description, source, source_ref_id, product_id, resolved, created_at) VALUES
('AI-ALERT-20260908-001', 'critical', 'SPC 控制图判异：精密转轴外径 X̄-R 出现连续 7 点上升趋势',
 'AI 模型检测 2026-09-07 08:00 起精密转轴 A100 外径控制图连续 7 点上升，触发 Western Electric 规则 2（连续 7 点同侧），过程可能失去统计受控状态。建议立即核查刀具磨损与设备状态。',
 'spc', 1, 2, 0, '2026-09-08 08:12:00'),
('AI-ALERT-20260908-002', 'critical', 'FQC 批次 LOT-20260729-A 不良率 11%，超阈值 5%',
 'AI 质量风险评估：PCB 主板 C300 批次 LOT-20260729-A 抽检 200pcs 中 22pcs 不合格（共面性、划痕、虚焊集中），不良率 11% 显著高于目标 5%，判定为批次性风险，建议整批隔离并追溯上游制程。',
 'fqc', 8, 4, 0, '2026-09-08 09:30:00'),
('AI-ALERT-20260907-003', 'warning', 'IQC 来料异常：精密转轴外径超差率周环比上升 40%',
 'AI 预测模型：供应商山特维克精密转轴来料外径超差率本周 3.2%，周环比上升 40%，接近 5% 预警阈值。建议加严下一批次检验并通知供应商。',
 'iqc', 6, 2, 0, '2026-09-07 15:20:00'),
('AI-ALERT-20260907-004', 'warning', '缺陷 Pareto 集中：虚焊 + 共面性占近 30% 缺陷',
 'AI 根因聚类：近 7 天缺陷中「虚焊」与「共面性」两类合计占比 28.5%，且集中于 PCB 制程，疑似同一制程根因。建议优先排查回流焊温度曲线与 PCB 来料平面度。',
 'anomaly', NULL, 4, 0, '2026-09-07 11:05:00'),
('AI-ALERT-20260906-005', 'info', 'IPQC 巡检：CNC-03 设备参数漂移趋势预警',
 'AI 趋势监测：CNC-03 主轴转速与切削参数的均值在过去 3 天缓慢漂移，虽未超控但呈单向趋势，建议提前安排预防性维护。',
 'ipqc', NULL, 2, 0, '2026-09-06 16:40:00'),
('AI-ALERT-20260905-006', 'info', '客诉早期信号：某客户反馈外观类投诉增加',
 'AI 文本聚类：近两周「外观」相关客诉/返工反馈出现增多趋势，虽未达阈值，列入早期观察信号，建议关注包装与转运环节。',
 'complaint', NULL, NULL, 0, '2026-09-05 10:00:00'),
('AI-ALERT-20260904-007', 'warning', 'CAPA CAPA-2026-003 逾期未关闭',
 'AI 进度追踪：纠正措施 CAPA-2026-003（精密转轴尺寸波动）已超截止 2 天未关闭，存在逾期风险，提醒责任人推进验证环节。',
 'capa', 3, 2, 0, '2026-09-04 09:00:00'),
('AI-ALERT-20260902-008', 'info', '供应商评分：OSG 精密刀具得分下滑',
 'AI 综合评分：供应商 OSG 精密刀具近 30 天综合质量得分由 92 降至 85，主要来自来料尺寸稳定性下降，建议纳入供应商改进跟踪。',
 'iqc', NULL, 3, 1, '2026-09-02 14:00:00');

-- ── 3. 验证 ──
SELECT '--- ai_alerts 验证 ---' AS section;
SELECT COUNT(*) AS '预警总数',
       SUM(resolved = 0) AS '未解决',
       SUM(resolved = 1) AS '已解决'
FROM ai_alerts;
SELECT level AS '级别', COUNT(*) AS '数量' FROM ai_alerts GROUP BY level ORDER BY level;

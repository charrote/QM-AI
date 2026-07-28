-- ============================================
-- 清理 MySQL 数据库中无效的 DateTime 值
-- 执行前请先备份数据库！
-- ============================================

-- 1. 查找所有 datetime 列中存在零日期或无效日期的记录
-- MySQL 的 0000-00-00 00:00:00 是常见的"无效"日期

-- M02.5 动态参数 - ClosureRules
SELECT 'ClosureRules' AS [Table], COUNT(*) AS [InvalidCount]
FROM closure_rules
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02.5 - DynamicParams
SELECT 'DynamicParams' AS [Table], COUNT(*) AS [InvalidCount]
FROM dynamic_params
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02.5 - ParamGroup
SELECT 'ParamGroups' AS [Table], COUNT(*) AS [InvalidCount]
FROM param_groups
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02 - User
SELECT 'Users' AS [Table], COUNT(*) AS [InvalidCount]
FROM users
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02 - Product
SELECT 'Products' AS [Table], COUNT(*) AS [InvalidCount]
FROM products
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02 - Bom
SELECT 'Boms' AS [Table], COUNT(*) AS [InvalidCount]
FROM boms
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02 - Routing
SELECT 'Routings' AS [Table], COUNT(*) AS [InvalidCount]
FROM routings
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02 - Process
SELECT 'Processes' AS [Table], COUNT(*) AS [InvalidCount]
FROM processes
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02.1 - InspectionItem
SELECT 'InspectionItems' AS [Table], COUNT(*) AS [InvalidCount]
FROM inspection_items
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M02.1 - InspectionPlan
SELECT 'InspectionPlans' AS [Table], COUNT(*) AS [InvalidCount]
FROM inspection_plans
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M03 - IqcReceipt
SELECT 'IqcReceipts' AS [Table], COUNT(*) AS [InvalidCount]
FROM iqc_receipts
WHERE receipt_date = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M03 - IqcInspection
SELECT 'IqcInspections' AS [Table], COUNT(*) AS [InvalidCount]
FROM iqc_inspections
WHERE inspected_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M03 - IqcAnomaly
SELECT 'IqcAnomalies' AS [Table], COUNT(*) AS [InvalidCount]
FROM iqc_anomalies
WHERE resolved_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M04 - IpqcFirstPiece
SELECT 'IpqcFirstPieces' AS [Table], COUNT(*) AS [InvalidCount]
FROM ipqc_first_pieces
WHERE checked_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M04 - IpqcPatrol
SELECT 'IpqcPatrols' AS [Table], COUNT(*) AS [InvalidCount]
FROM ipqc_patrols
WHERE scheduled_time = '0000-00-00 00:00:00'
   OR actual_time = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M04 - IpqcClosureStatus
SELECT 'IpqcClosureStatuses' AS [Table], COUNT(*) AS [InvalidCount]
FROM ipqc_closure_statuses
WHERE closed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M05 - FqcInspection
SELECT 'FqcInspections' AS [Table], COUNT(*) AS [InvalidCount]
FROM fqc_inspections
WHERE checked_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M05 - OqcRelease
SELECT 'OqcReleases' AS [Table], COUNT(*) AS [InvalidCount]
FROM oqc_releases
WHERE release_date = '0000-00-00 00:00:00'
   OR signature_time = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M05 - PackagingConfirmation
SELECT 'PackagingConfirmations' AS [Table], COUNT(*) AS [InvalidCount]
FROM packaging_confirmations
WHERE confirmed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M06 - SpcDataPoint
SELECT 'SpcDataPoints' AS [Table], COUNT(*) AS [InvalidCount]
FROM spc_data_points
WHERE measured_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00';

-- M06 - SpcAlertTrigger
SELECT 'SpcAlertTriggers' AS [Table], COUNT(*) AS [InvalidCount]
FROM spc_alert_triggers
WHERE triggered_at = '0000-00-00 00:00:00'
   OR resolved_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M06 - SpcAnovaResult
SELECT 'SpcAnovaResults' AS [Table], COUNT(*) AS [InvalidCount]
FROM spc_anova_results
WHERE analysis_date = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00';

-- M07 - Defect
SELECT 'Defects' AS [Table], COUNT(*) AS [InvalidCount]
FROM defects
WHERE discovered_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M07 - Capa
SELECT 'Capas' AS [Table], COUNT(*) AS [InvalidCount]
FROM capas
WHERE due_date = '0000-00-00 00:00:00'
   OR closed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M07 - CapaCorrectiveAction
SELECT 'CapaCorrectiveActions' AS [Table], COUNT(*) AS [InvalidCount]
FROM capa_corrective_actions
WHERE due_date = '0000-00-00 00:00:00'
   OR completed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M07 - CapaPreventiveAction
SELECT 'CapaPreventiveActions' AS [Table], COUNT(*) AS [InvalidCount]
FROM capa_preventive_actions
WHERE due_date = '0000-00-00 00:00:00'
   OR completed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M07 - CapaVerification
SELECT 'CapaVerifications' AS [Table], COUNT(*) AS [InvalidCount]
FROM capa_verifications
WHERE verification_date = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00';

-- M07 - ScrapReworkRecord
SELECT 'ScrapReworkRecords' AS [Table], COUNT(*) AS [InvalidCount]
FROM scrap_rework_records
WHERE authorized_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00';

-- M09 - Complaint
SELECT 'Complaints' AS [Table], COUNT(*) AS [InvalidCount]
FROM complaints
WHERE acknowledged_at = '0000-00-00 00:00:00'
   OR closed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M09 - D8Report
SELECT 'D8Reports' AS [Table], COUNT(*) AS [InvalidCount]
FROM d8_reports
WHERE completed_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M11 - EquipmentStatusHistory
SELECT 'EquipmentStatusHistories' AS [Table], COUNT(*) AS [InvalidCount]
FROM equipment_status_histories
WHERE recorded_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00';

-- M11 - EquipmentQualityCorrelation
SELECT 'EquipmentQualityCorrelations' AS [Table], COUNT(*) AS [InvalidCount]
FROM equipment_quality_correlations
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M12 - Document
SELECT 'Documents' AS [Table], COUNT(*) AS [InvalidCount]
FROM documents
WHERE approved_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M13 - Audit
SELECT 'Audits' AS [Table], COUNT(*) AS [InvalidCount]
FROM audits
WHERE created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M13 - AuditFinding
SELECT 'AuditFindings' AS [Table], COUNT(*) AS [InvalidCount]
FROM audit_findings
WHERE planned_completion_date = '0000-00-00 00:00:00'
   OR verified_at = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00'
   OR updated_at = '0000-00-00 00:00:00';

-- M06 - ParamRealtimeValue
SELECT 'ParamRealtimeValues' AS [Table], COUNT(*) AS [InvalidCount]
FROM param_realtime_values
WHERE timestamp = '0000-00-00 00:00:00'
   OR created_at = '0000-00-00 00:00:00';

-- ============================================
-- 如果上面有查出无效数据，执行下面的 UPDATE 语句
-- 将 '0000-00-00 00:00:00' 替换为 NULL（可空字段）或 '1970-01-01 00:00:00'（非空字段）
-- ============================================

-- 修复 ClosureRules 的零日期（created_at 和 updated_at 都是非空字段，用 1970-01-01 替换）
UPDATE closure_rules SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE closure_rules SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 DynamicParams
UPDATE dynamic_params SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE dynamic_params SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 ParamGroups
UPDATE param_groups SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE param_groups SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Users
UPDATE users SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE users SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Products
UPDATE products SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE products SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Boms
UPDATE boms SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE boms SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Routings
UPDATE routings SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE routings SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Processes
UPDATE processes SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE processes SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 InspectionItems
UPDATE inspection_items SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE inspection_items SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 InspectionPlans
UPDATE inspection_plans SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE inspection_plans SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 IqcReceipts（receipt_date 是可空的，用 NULL 替换）
UPDATE iqc_receipts SET receipt_date = NULL WHERE receipt_date = '0000-00-00 00:00:00';
UPDATE iqc_receipts SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_receipts SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 IqcInspections
UPDATE iqc_inspections SET inspected_at = NULL WHERE inspected_at = '0000-00-00 00:00:00';
UPDATE iqc_inspections SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_inspections SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 IqcAnomalies
UPDATE iqc_anomalies SET resolved_at = NULL WHERE resolved_at = '0000-00-00 00:00:00';
UPDATE iqc_anomalies SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE iqc_anomalies SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 IpqcFirstPieces
UPDATE ipqc_first_pieces SET checked_at = NULL WHERE checked_at = '0000-00-00 00:00:00';
UPDATE ipqc_first_pieces SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE ipqc_first_pieces SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 IpqcPatrols
UPDATE ipqc_patrols SET scheduled_time = '1970-01-01 00:00:00' WHERE scheduled_time = '0000-00-00 00:00:00';
UPDATE ipqc_patrols SET actual_time = NULL WHERE actual_time = '0000-00-00 00:00:00';
UPDATE ipqc_patrols SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE ipqc_patrols SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 IpqcClosureStatuses
UPDATE ipqc_closure_statuses SET closed_at = NULL WHERE closed_at = '0000-00-00 00:00:00';
UPDATE ipqc_closure_statuses SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE ipqc_closure_statuses SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 FqcInspections
UPDATE fqc_inspections SET checked_at = NULL WHERE checked_at = '0000-00-00 00:00:00';
UPDATE fqc_inspections SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE fqc_inspections SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 OqcReleases
UPDATE oqc_releases SET release_date = '1970-01-01 00:00:00' WHERE release_date = '0000-00-00 00:00:00';
UPDATE oqc_releases SET signature_time = NULL WHERE signature_time = '0000-00-00 00:00:00';
UPDATE oqc_releases SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE oqc_releases SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 PackagingConfirmations
UPDATE packaging_confirmations SET confirmed_at = '1970-01-01 00:00:00' WHERE confirmed_at = '0000-00-00 00:00:00';
UPDATE packaging_confirmations SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE packaging_confirmations SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 SpcDataPoints
UPDATE spc_data_points SET measured_at = '1970-01-01 00:00:00' WHERE measured_at = '0000-00-00 00:00:00';
UPDATE spc_data_points SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';

-- 修复 SpcAlertTriggers
UPDATE spc_alert_triggers SET triggered_at = '1970-01-01 00:00:00' WHERE triggered_at = '0000-00-00 00:00:00';
UPDATE spc_alert_triggers SET resolved_at = NULL WHERE resolved_at = '0000-00-00 00:00:00';
UPDATE spc_alert_triggers SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE spc_alert_triggers SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 SpcAnovaResults
UPDATE spc_anova_results SET analysis_date = '1970-01-01 00:00:00' WHERE analysis_date = '0000-00-00 00:00:00';
UPDATE spc_anova_results SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';

-- 修复 Defects
UPDATE defects SET discovered_at = '1970-01-01 00:00:00' WHERE discovered_at = '0000-00-00 00:00:00';
UPDATE defects SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE defects SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Capas
UPDATE capas SET due_date = NULL WHERE due_date = '0000-00-00 00:00:00';
UPDATE capas SET closed_at = NULL WHERE closed_at = '0000-00-00 00:00:00';
UPDATE capas SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE capas SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 CapaCorrectiveActions
UPDATE capa_corrective_actions SET due_date = '1970-01-01 00:00:00' WHERE due_date = '0000-00-00 00:00:00';
UPDATE capa_corrective_actions SET completed_at = NULL WHERE completed_at = '0000-00-00 00:00:00';
UPDATE capa_corrective_actions SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE capa_corrective_actions SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 CapaPreventiveActions
UPDATE capa_preventive_actions SET due_date = '1970-01-01 00:00:00' WHERE due_date = '0000-00-00 00:00:00';
UPDATE capa_preventive_actions SET completed_at = NULL WHERE completed_at = '0000-00-00 00:00:00';
UPDATE capa_preventive_actions SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE capa_preventive_actions SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 CapaVerifications
UPDATE capa_verifications SET verification_date = '1970-01-01 00:00:00' WHERE verification_date = '0000-00-00 00:00:00';
UPDATE capa_verifications SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';

-- 修复 ScrapReworkRecords
UPDATE scrap_rework_records SET authorized_at = '1970-01-01 00:00:00' WHERE authorized_at = '0000-00-00 00:00:00';
UPDATE scrap_rework_records SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';

-- 修复 Complaints
UPDATE complaints SET acknowledged_at = NULL WHERE acknowledged_at = '0000-00-00 00:00:00';
UPDATE complaints SET closed_at = NULL WHERE closed_at = '0000-00-00 00:00:00';
UPDATE complaints SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE complaints SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 D8Reports
UPDATE d8_reports SET completed_at = NULL WHERE completed_at = '0000-00-00 00:00:00';
UPDATE d8_reports SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE d8_reports SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 EquipmentStatusHistories
UPDATE equipment_status_histories SET recorded_at = '1970-01-01 00:00:00' WHERE recorded_at = '0000-00-00 00:00:00';
UPDATE equipment_status_histories SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';

-- 修复 EquipmentQualityCorrelations
UPDATE equipment_quality_correlations SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE equipment_quality_correlations SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Documents
UPDATE documents SET approved_at = NULL WHERE approved_at = '0000-00-00 00:00:00';
UPDATE documents SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE documents SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 Audits
UPDATE audits SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE audits SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 AuditFindings
UPDATE audit_findings SET planned_completion_date = NULL WHERE planned_completion_date = '0000-00-00 00:00:00';
UPDATE audit_findings SET verified_at = NULL WHERE verified_at = '0000-00-00 00:00:00';
UPDATE audit_findings SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';
UPDATE audit_findings SET updated_at = '1970-01-01 00:00:00' WHERE updated_at = '0000-00-00 00:00:00';

-- 修复 ParamRealtimeValues
UPDATE param_realtime_values SET `timestamp` = '1970-01-01 00:00:00' WHERE `timestamp` = '0000-00-00 00:00:00';
UPDATE param_realtime_values SET created_at = '1970-01-01 00:00:00' WHERE created_at = '0000-00-00 00:00:00';

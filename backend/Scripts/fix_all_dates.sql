-- ============================================
-- 诊断并修复所有表的零日期数据（修正列名）
-- ============================================

-- 诊断
SELECT 'closure_rules' AS tbl, COUNT(*) AS cnt FROM closure_rules WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'dynamic_params', COUNT(*) FROM dynamic_params WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'param_groups', COUNT(*) FROM param_groups WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'users', COUNT(*) FROM users WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'products', COUNT(*) FROM products WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'boms', COUNT(*) FROM boms WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'routings', COUNT(*) FROM routings WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'processes', COUNT(*) FROM processes WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'inspection_items', COUNT(*) FROM inspection_items WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'inspection_plans', COUNT(*) FROM inspection_plans WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'iqc_receipts', COUNT(*) FROM iqc_receipts WHERE YEAR(created_at)=0
UNION ALL SELECT 'iqc_inspections', COUNT(*) FROM iqc_inspections WHERE YEAR(created_at)=0
UNION ALL SELECT 'iqc_anomalies', COUNT(*) FROM iqc_anomalies WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'ipqc_first_pieces', COUNT(*) FROM ipqc_first_pieces WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'ipqc_patrols', COUNT(*) FROM ipqc_patrols WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'ipqc_closure_status', COUNT(*) FROM ipqc_closure_status WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'fqc_inspections', COUNT(*) FROM fqc_inspections WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'oqc_releases', COUNT(*) FROM oqc_releases WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'packaging_confirmations', COUNT(*) FROM packaging_confirmations WHERE YEAR(ConfirmedAt)=0
UNION ALL SELECT 'spc_data_points', COUNT(*) FROM spc_data_points WHERE YEAR(created_at)=0 OR YEAR(MeasuredAt)=0
UNION ALL SELECT 'spc_alert_triggers', COUNT(*) FROM spc_alert_triggers WHERE YEAR(TriggeredAt)=0
UNION ALL SELECT 'spc_anova_results', COUNT(*) FROM spc_anova_results WHERE YEAR(analysis_date)=0
UNION ALL SELECT 'defects', COUNT(*) FROM defects WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa', COUNT(*) FROM capa WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa_corrective_actions', COUNT(*) FROM capa_corrective_actions WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa_preventive_actions', COUNT(*) FROM capa_preventive_actions WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa_verifications', COUNT(*) FROM capa_verifications WHERE YEAR(created_at)=0
UNION ALL SELECT 'scrap_rework_records', COUNT(*) FROM scrap_rework_records WHERE YEAR(created_at)=0 OR YEAR(authorized_at)=0
UNION ALL SELECT 'complaints', COUNT(*) FROM complaints WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'd8_reports', COUNT(*) FROM d8_reports WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'equipment_status_history', COUNT(*) FROM equipment_status_history WHERE YEAR(recorded_at)=0
UNION ALL SELECT 'equipment_quality_correlation', COUNT(*) FROM equipment_quality_correlation WHERE YEAR(analysis_date)=0
UNION ALL SELECT 'documents', COUNT(*) FROM documents WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'audits', COUNT(*) FROM audits WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'audit_findings', COUNT(*) FROM audit_findings WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'param_realtime_values', COUNT(*) FROM param_realtime_values WHERE YEAR(created_at)=0;

-- 修复
UPDATE closure_rules SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE closure_rules SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE dynamic_params SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE dynamic_params SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE param_groups SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE param_groups SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE users SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE users SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE products SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE products SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE boms SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE boms SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE routings SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE routings SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE processes SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE processes SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE inspection_items SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE inspection_items SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE inspection_plans SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE inspection_plans SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE iqc_receipts SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;

UPDATE iqc_inspections SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;

UPDATE iqc_anomalies SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE iqc_anomalies SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE ipqc_first_pieces SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE ipqc_first_pieces SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE ipqc_patrols SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE ipqc_patrols SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE ipqc_closure_status SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE ipqc_closure_status SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE fqc_inspections SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE fqc_inspections SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE oqc_releases SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE oqc_releases SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE packaging_confirmations SET ConfirmedAt = '1970-01-01 00:00:00' WHERE YEAR(ConfirmedAt)=0;

UPDATE spc_data_points SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE spc_data_points SET MeasuredAt = '1970-01-01 00:00:00' WHERE YEAR(MeasuredAt)=0;

UPDATE spc_alert_triggers SET TriggeredAt = '1970-01-01 00:00:00' WHERE YEAR(TriggeredAt)=0;

UPDATE spc_anova_results SET analysis_date = '1970-01-01 00:00:00' WHERE YEAR(analysis_date)=0;

UPDATE defects SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE defects SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE capa SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE capa SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE capa_corrective_actions SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE capa_corrective_actions SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE capa_preventive_actions SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE capa_preventive_actions SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE capa_verifications SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;

UPDATE scrap_rework_records SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE scrap_rework_records SET authorized_at = '1970-01-01 00:00:00' WHERE YEAR(authorized_at)=0;

UPDATE complaints SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE complaints SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE d8_reports SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE d8_reports SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE equipment_status_history SET recorded_at = '1970-01-01 00:00:00' WHERE YEAR(recorded_at)=0;

UPDATE equipment_quality_correlation SET analysis_date = '1970-01-01 00:00:00' WHERE YEAR(analysis_date)=0;

UPDATE documents SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE documents SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE audits SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE audits SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE audit_findings SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;
UPDATE audit_findings SET updated_at = '1970-01-01 00:00:00' WHERE YEAR(updated_at)=0;

UPDATE param_realtime_values SET created_at = '1970-01-01 00:00:00' WHERE YEAR(created_at)=0;

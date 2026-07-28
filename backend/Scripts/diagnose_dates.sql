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
UNION ALL SELECT 'iqc_inspections', COUNT(*) FROM iqc_inspections WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'iqc_anomalies', COUNT(*) FROM iqc_anomalies WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'ipqc_first_pieces', COUNT(*) FROM ipqc_first_pieces WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'ipqc_patrols', COUNT(*) FROM ipqc_patrols WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'ipqc_closure_status', COUNT(*) FROM ipqc_closure_status WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'fqc_inspections', COUNT(*) FROM fqc_inspections WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'oqc_releases', COUNT(*) FROM oqc_releases WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'packaging_confirmations', COUNT(*) FROM packaging_confirmations WHERE YEAR(ConfirmedAt)=0
UNION ALL SELECT 'spc_data_points', COUNT(*) FROM spc_data_points WHERE YEAR(created_at)=0 OR YEAR(MeasuredAt)=0
UNION ALL SELECT 'spc_alert_triggers', COUNT(*) FROM spc_alert_triggers WHERE YEAR(created_at)=0 OR YEAR(TriggeredAt)=0
UNION ALL SELECT 'spc_anova_results', COUNT(*) FROM spc_anova_results WHERE YEAR(created_at)=0 OR YEAR(analysis_date)=0
UNION ALL SELECT 'defects', COUNT(*) FROM defects WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa', COUNT(*) FROM capa WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa_corrective_actions', COUNT(*) FROM capa_corrective_actions WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa_preventive_actions', COUNT(*) FROM capa_preventive_actions WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'capa_verifications', COUNT(*) FROM capa_verifications WHERE YEAR(created_at)=0
UNION ALL SELECT 'scrap_rework_records', COUNT(*) FROM scrap_rework_records WHERE YEAR(created_at)=0 OR YEAR(authorized_at)=0
UNION ALL SELECT 'complaints', COUNT(*) FROM complaints WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'd8_reports', COUNT(*) FROM d8_reports WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'equipment_status_history', COUNT(*) FROM equipment_status_history WHERE YEAR(created_at)=0 OR YEAR(recorded_at)=0
UNION ALL SELECT 'equipment_quality_correlation', COUNT(*) FROM equipment_quality_correlation WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'documents', COUNT(*) FROM documents WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'audits', COUNT(*) FROM audits WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'audit_findings', COUNT(*) FROM audit_findings WHERE YEAR(created_at)=0 OR YEAR(updated_at)=0
UNION ALL SELECT 'param_realtime_values', COUNT(*) FROM param_realtime_values WHERE YEAR(created_at)=0;

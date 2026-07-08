export const REPORT_TYPE_OPTIONS = [
  { value: 'quality_overview', label: '质量总览' },
  { value: 'iqc_report', label: 'IQC 报告' },
  { value: 'ipqc_report', label: 'IPQC 报告' },
  { value: 'fqc_report', label: 'FQC 报告' },
  { value: 'defect_analysis', label: '不良分析' },
  { value: 'supplier_report', label: '供应商报告' },
  { value: 'equipment_report', label: '设备报告' },
]

export const REPORT_MODULE_OPTIONS = [
  { value: 'all', label: '全部模块' },
  { value: 'iqc', label: 'IQC' },
  { value: 'ipqc', label: 'IPQC' },
  { value: 'fqc', label: 'FQC' },
  { value: 'spc', label: 'SPC' },
  { value: 'defects', label: '不良管理' },
]

export const EXPORT_STATUS_OPTIONS = [
  { value: 'pending', label: '生成中', type: 'warning' },
  { value: 'completed', label: '已完成', type: 'success' },
  { value: 'failed', label: '失败', type: 'danger' },
]

export const EXPORT_STATUS_MAP: Record<string, string> = {
  pending: '生成中', completed: '已完成', failed: '失败',
}

export const REPORT_FORMAT_OPTIONS = [
  { value: 'csv', label: 'CSV' },
  { value: 'xlsx', label: 'Excel' },
  { value: 'pdf', label: 'PDF' },
]

export const REPORT_TYPE_MAP: Record<string, string> = {
  quality_overview: '质量总览',
  iqc_report: 'IQC 报告',
  ipqc_report: 'IPQC 报告',
  fqc_report: 'FQC 报告',
  defect_analysis: '不良分析',
  supplier_report: '供应商报告',
  equipment_report: '设备报告',
}

// ─── 文档实体 ──────────────────────────────────────────────

export interface Document {
  id: number
  title: string
  docType: string
  minioKey?: string
  fileSizeBytes?: number
  fileHash?: string
  version: number
  status: string
  approvedBy?: number
  approvedAt?: string
  expiresAt?: string
  createdAt: string
  updatedAt: string
  createdBy?: string
}

export interface DocumentVersion {
  id: number
  documentId: number
  version: number
  minioKey?: string
  changeDescription?: string
  createdBy?: string
  createdAt: string
}

// ─── DTO ────────────────────────────────────────────────────

export interface CreateDocument {
  title: string
  docType: string
  minioKey?: string
  fileSizeBytes?: number
  fileHash?: string
}

export interface UploadResponse {
  minioKey: string
  size: number
  hash: string
}

export interface ApproveDocument {
  approvedBy: number
}

export interface RejectDocument {
  reason: string
}

// ─── 常量（下拉选项） ──────────────────────────────────────

export const DOC_TYPE_OPTIONS = [
  { value: 'sop', label: 'SOP' },
  { value: 'work_instruction', label: '作业指导书' },
  { value: 'inspection_standard', label: '检验标准' },
  { value: '8d_report', label: '8D报告' },
  { value: 'audit_report', label: '审核报告' },
  { value: 'other', label: '其他' },
]

export const DOC_TYPE_MAP: Record<string, string> = {
  sop: 'SOP',
  work_instruction: '作业指导书',
  inspection_standard: '检验标准',
  '8d_report': '8D报告',
  audit_report: '审核报告',
  other: '其他',
}

export const DOC_STATUS_OPTIONS = [
  { value: 'draft', label: '草稿', type: 'info' },
  { value: 'reviewing', label: '审批中', type: 'warning' },
  { value: 'approved', label: '已批准', type: 'success' },
  { value: 'archived', label: '已归档', type: '' },
]

export const DOC_STATUS_MAP: Record<string, { label: string; type: '' | 'info' | 'warning' | 'success' }> = {
  draft: { label: '草稿', type: 'info' },
  reviewing: { label: '审批中', type: 'warning' },
  approved: { label: '已批准', type: 'success' },
  archived: { label: '已归档', type: '' },
}

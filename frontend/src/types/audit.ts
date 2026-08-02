// ─── M13 审核管理 ──────────────────────────────────────

export interface Audit {
  id: number
  auditCode: string
  auditType: string
  title: string
  description?: string
  startDate: string
  endDate: string
  auditorId: number
  scope?: string
  status: string
  createdAt: string
  updatedAt: string
}

export interface AuditFinding {
  id: number
  auditId: number
  findingType: string
  severity?: string
  description: string
  evidence?: string
  requirementRef?: string
  status: string
  rectificationPlan?: string
  responsibleUserId?: number
  rectificationDueDate?: string
  verifiedBy?: number
  verifiedAt?: string
  createdAt: string
  updatedAt: string
}

// ─── DTOs ──────────────────────────────────────────────

export interface CreateAudit {
  auditCode: string
  auditType: string
  title: string
  description?: string
  startDate: string
  endDate: string
  auditorId: number
  scope?: string
}

export interface CreateAuditFinding {
  auditId: number
  findingType: string
  severity?: string
  description: string
  evidence?: string
  requirementRef?: string
}

export interface UpdateFindingStatus {
  status: string
  rectificationPlan?: string
  responsibleUserId?: number
  rectificationDueDate?: string
}

export interface VerifyFinding {
  verifierId: string
  passed: boolean
}

// ─── Constants ──────────────────────────────────────────

export const AUDIT_TYPE_OPTIONS = [
  { value: 'internal', label: '内审' },
  { value: 'process', label: '过程审核' },
  { value: 'product', label: '产品审核' },
]

export const AUDIT_STATUS_OPTIONS = [
  { value: 'planned', label: '计划中', type: 'info' },
  { value: 'in_progress', label: '进行中', type: 'warning' },
  { value: 'completed', label: '已完成', type: 'success' },
  { value: 'archived', label: '已归档', type: '' },
]

export const FINDING_TYPE_OPTIONS = [
  { value: 'conformity', label: '符合项', type: 'success' },
  { value: 'non_conformity', label: '不符合项', type: 'danger' },
  { value: 'opportunity', label: '改进机会', type: 'warning' },
]

export const FINDING_STATUS_OPTIONS = [
  { value: 'open', label: '未关闭', type: 'danger' },
  { value: 'corrected', label: '已纠正', type: 'warning' },
  { value: 'verified', label: '已验证', type: 'success' },
  { value: 'closed', label: '已结案', type: '' },
]

// ─── Maps ──────────────────────────────────────────────

export const AUDIT_STATUS_MAP: Record<string, string> = {
  planned: '计划中',
  in_progress: '进行中',
  completed: '已完成',
  archived: '已归档',
}

export const FINDING_STATUS_MAP: Record<string, string> = {
  open: '未关闭',
  corrected: '已纠正',
  verified: '已验证',
  closed: '已结案',
}

export const FINDING_TYPE_MAP: Record<string, string> = {
  conformity: '符合项',
  non_conformity: '不符合项',
  opportunity: '改进机会',
}

export const SEVERITY_OPTIONS = [
  { value: 'critical', label: '严重', type: 'danger' },
  { value: 'major', label: '主要', type: 'warning' },
  { value: 'minor', label: '次要', type: 'info' },
  { value: 'observation', label: '观察项', type: '' },
]

export const SEVERITY_MAP: Record<string, string> = {
  critical: '严重',
  major: '主要',
  minor: '次要',
  observation: '观察项',
}

// ─── 不良记录 ──────────────────────────────────────────────

export interface Defect {
  id: number
  defectCode: string
  severity: string
  sourceType: string
  sourceId: number
  productId: number
  batchId: number
  equipmentId: number
  quantity: number
  description: string
  imageUrls: string[]
  discoveredBy: string
  discoveredAt: string
  status: string
  createdAt: string
  updatedAt: string
}

// ─── CAPA 流程 ──────────────────────────────────────────────

export interface Capa {
  id: number
  capaCode: string
  defectId: number
  anomalyId: number
  complaintId: number
  severity: string
  title: string
  description: string
  currentPhase: number
  status: string
  createdBy: string
  assignedTo: string
  dueDate: string
  closedAt: string
  createdAt: string
  updatedAt: string
  defect?: Defect
  temporaryMeasures?: CapaTemporaryMeasure[]
  rootCauses?: CapaRootCause[]
  correctiveActions?: CapaCorrectiveAction[]
  preventiveActions?: CapaPreventiveAction[]
  verifications?: CapaVerification[]
}

export interface CapaTemporaryMeasure {
  id: number
  capaId: number
  description: string
  executedBy: string
  executedAt: string
  createdAt: string
}

export interface CapaRootCause {
  id: number
  capaId: number
  analysisMethod: string
  content: string
  rootCauseSummary: string
  createdBy: string
  createdAt: string
}

export interface CapaCorrectiveAction {
  id: number
  capaId: number
  actionDescription: string
  responsiblePerson: string
  dueDate: string
  status: string
  completedAt: string
  remarks: string
  createdAt: string
  updatedAt: string
}

export interface CapaPreventiveAction {
  id: number
  capaId: number
  actionDescription: string
  responsiblePerson: string
  dueDate: string
  status: string
  completedAt: string
  remarks: string
  createdAt: string
  updatedAt: string
}

export interface CapaVerification {
  id: number
  capaId: number
  verifierId: number
  verificationDate: string
  conclusion: string
  evidence: string
  imageUrls: string[]
  remarks: string
  createdAt: string
}

// ─── 报废/返工记录 ──────────────────────────────────────────

export interface ScrapReworkRecord {
  id: number
  type: string
  defectId: number
  batchId: number
  quantity: number
  reason: string
  reworkSteps: string
  reworkInspectionRequired: boolean
  reworkInspectionResult: string
  authorizedBy: string
  authorizedAt: string
  createdAt: string
}

// ─── 创建/更新 请求类型 ─────────────────────────────────────

export interface CreateDefect {
  defectCode: string
  severity: string
  sourceType: string
  sourceId: number
  productId: number
  batchId: number
  equipmentId: number
  quantity: number
  description: string
  imageUrls?: string[]
  discoveredBy: string
  discoveredAt: string
}

export interface UpdateDefect {
  defectCode?: string
  severity?: string
  sourceType?: string
  sourceId?: number
  productId?: number
  batchId?: number
  equipmentId?: number
  quantity?: number
  description?: string
  imageUrls?: string[]
  status?: string
  discoveredBy?: string
  discoveredAt?: string
}

export interface CreateCapa {
  defectId: number
  anomalyId?: number
  complaintId?: number
  severity: string
  title: string
  description: string
  assignedTo: string
  dueDate: string
}

export interface CreateScrapRework {
  type: string
  defectId: number
  batchId: number
  quantity: number
  reason: string
  reworkSteps?: string
  reworkInspectionRequired?: boolean
  authorizedBy: string
}

export interface UpdateReworkResult {
  reworkInspectionResult: string
  reworkSteps?: string
}

// ─── CAPA 阶段定义 ──────────────────────────────────────────

export const CAPA_PHASE_LABELS = [
  { phase: 0, label: '创建', icon: 'Edit' },
  { phase: 1, label: '临时措施', icon: 'AlarmClock' },
  { phase: 2, label: '根本原因分析', icon: 'Search' },
  { phase: 3, label: '纠正措施', icon: 'Tools' },
  { phase: 4, label: '预防措施', icon: 'Shield' },
  { phase: 5, label: '效果验证', icon: 'Select' },
  { phase: 6, label: '关闭', icon: 'CircleCheck' },
]

export const CAPA_PHASE_MAP: Record<number, string> = {
  0: '创建',
  1: '临时措施',
  2: '根本原因分析',
  3: '纠正措施',
  4: '预防措施',
  5: '效果验证',
  6: '关闭',
}

// ─── 常量（下拉选项） ─────────────────────────────────────

export const SEVERITY_OPTIONS = [
  { value: 'critical', label: '致命', type: 'danger' },
  { value: 'major', label: '严重', type: 'warning' },
  { value: 'minor', label: '轻微', type: 'info' },
]

export const SEVERITY_MAP: Record<string, string> = {
  critical: '致命',
  major: '严重',
  minor: '轻微',
}

export const DEFECT_STATUS_OPTIONS = [
  { value: 'open', label: '未处理', type: 'danger' },
  { value: 'in_progress', label: '处理中', type: 'warning' },
  { value: 'resolved', label: '已解决', type: 'success' },
  { value: 'closed', label: '已关闭', type: 'info' },
]

export const DEFECT_STATUS_MAP: Record<string, string> = {
  open: '未处理',
  in_progress: '处理中',
  resolved: '已解决',
  closed: '已关闭',
}

export const SOURCE_TYPE_OPTIONS = [
  { value: 'IQC', label: '来料检验' },
  { value: 'IPQC-PATROL', label: 'IPQC巡检' },
  { value: 'IPQC-FIRSTPIECE', label: 'IPQC首件检验' },
  { value: 'FQC', label: '成品检验' },
  { value: 'CUSTOMER', label: '客户投诉' },
  { value: 'PRODUCTION', label: '生产异常' },
]

export const SOURCE_TYPE_MAP: Record<string, string> = {
  IQC: '来料检验',
  'IPQC-PATROL': 'IPQC巡检',
  'IPQC-FIRSTPIECE': 'IPQC首件检验',
  FQC: '成品检验',
  CUSTOMER: '客户投诉',
  PRODUCTION: '生产异常',
}

export const ACTION_STATUS_OPTIONS = [
  { value: 'pending', label: '待执行', type: 'warning' },
  { value: 'in_progress', label: '执行中', type: 'primary' },
  { value: 'completed', label: '已完成', type: 'success' },
  { value: 'overdue', label: '已逾期', type: 'danger' },
]

export const ACTION_STATUS_MAP: Record<string, string> = {
  pending: '待执行',
  in_progress: '执行中',
  completed: '已完成',
  overdue: '已逾期',
}

export const REWORK_TYPE_OPTIONS = [
  { value: 'scrap', label: '报废', type: 'danger' },
  { value: 'rework', label: '返工', type: 'warning' },
]

export const REWORK_TYPE_MAP: Record<string, string> = {
  scrap: '报废',
  rework: '返工',
}

export const REWORK_INSPECTION_RESULT_OPTIONS = [
  { value: 'pass', label: '合格', type: 'success' },
  { value: 'fail', label: '不合格', type: 'danger' },
]

export const CAPA_STATUS_OPTIONS = [
  { value: 'active', label: '进行中', type: 'primary' },
  { value: 'completed', label: '已完成', type: 'success' },
  { value: 'cancelled', label: '已取消', type: 'info' },
]

export const CAPA_STATUS_MAP: Record<string, string> = {
  active: '进行中',
  completed: '已完成',
  cancelled: '已取消',
}
// ─── 首件检验 ─────────────────────────────────────────
export interface IpqcFirstPiece {
  id: number
  fpNo: string
  workOrderId: number
  processId: number
  processName?: string
  equipmentId: number
  equipmentName?: string
  shift?: string
  reason: string
  conclusion: string
  allowedToProduce: boolean
  inspector?: string
  checkedAt?: string
  createdAt: string
}

export interface IpqcFirstPieceDetail extends IpqcFirstPiece {
  operatorId: number
  operatorName?: string
  items?: IpqcFirstPieceItem[]
}

export interface IpqcFirstPieceItem {
  id: number
  firstPieceId: number
  itemName: string
  itemCode?: string
  usl?: number
  lsl?: number
  dataType: string
  actualValue?: number
  result: string
  imageUrls?: string
  remarks?: string
}

export interface CreateIpqcFirstPiece {
  fpNo?: string
  workOrderId: number
  processId: number
  equipmentId: number
  operatorId: number
  shift?: string
  reason: string
  inspector?: string
  items: CreateIpqcFirstPieceItem[]
}

export interface CreateIpqcFirstPieceItem {
  itemName: string
  itemCode?: string
  usl?: number
  lsl?: number
  dataType: string
  actualValue?: number
  result: string
  imageUrls?: string
  remarks?: string
}

export interface SubmitIpqcFirstPiece {
  conclusion: string
  allowedToProduce: boolean
  inspector?: string
  items?: IpqcFirstPieceItem[]
}

// ─── 巡检计划 ─────────────────────────────────────────
export interface IpqcPatrolPlan {
  id: number
  planNo: string
  processId: number
  processName?: string
  equipmentId: number
  equipmentName?: string
  patrolIntervalMin: number
  autoGenerate: boolean
  status: string
  inspector?: string
  createdAt: string
}

export interface CreateIpqcPatrolPlan {
  planNo?: string
  processId: number
  equipmentId: number
  patrolIntervalMin: number
  autoGenerate: boolean
  inspector?: string
}

export interface UpdateIpqcPatrolPlan {
  patrolIntervalMin?: number
  autoGenerate?: boolean
  status?: string
  inspector?: string
}

// ─── 巡检记录 ─────────────────────────────────────────
export interface IpqcPatrol {
  id: number
  patrolNo: string
  patrolPlanId: number
  planNo?: string
  workOrderId?: number
  processId: number
  processName?: string
  equipmentId: number
  equipmentName?: string
  inspector?: string
  scheduledTime: string
  actualTime?: string
  totalChecked: number
  totalPass: number
  totalFail: number
  conclusion: string
  status: string
  createdAt: string
}

export interface IpqcPatrolDetail extends IpqcPatrol {
  remarks?: string
  items?: IpqcPatrolItem[]
}

export interface IpqcPatrolItem {
  id: number
  patrolId: number
  itemName: string
  itemCode?: string
  usl?: number
  lsl?: number
  dataType: string
  actualValue?: number
  result: string
  imageUrls?: string
}

export interface SubmitIpqcPatrol {
  conclusion: string
  remarks?: string
  inspector?: string
  items?: IpqcPatrolItemSubmit[]
}

export interface IpqcPatrolItemSubmit {
  id?: number
  itemName: string
  itemCode?: string
  usl?: number
  lsl?: number
  dataType: string
  actualValue?: number
  result: string
  imageUrls?: string
}

// ─── AI 风险评分 ──────────────────────────────────────
export interface IpqcAiRiskScore {
  score: number
  level: string
  trend?: string
  factors?: RiskFactor[]
  recommendations?: string[]
  lastUpdated?: string
}

export interface RiskFactor {
  name: string
  description: string
  impact: number
  currentValue?: number
  targetValue?: number
}

// ─── 关单评估 ─────────────────────────────────────────
export interface IpqcClosureEvaluation {
  isSatisfied: boolean
  failedConditions?: string[]
  status: string
  ruleName?: string
}

// ─── 状态枚举选项 ─────────────────────────────────────
export const IPQC_FIRST_PIECE_CONCLUSION_OPTIONS = [
  { value: 'pending', label: '待检验', type: 'info' },
  { value: 'qualified', label: '合格', type: 'success' },
  { value: 'unqualified', label: '不合格', type: 'danger' },
]

export const IPQC_FIRST_PIECE_REASON_OPTIONS = [
  { value: '班次切换', label: '班次切换' },
  { value: '换线', label: '换线' },
  { value: '换刀', label: '换刀' },
  { value: '设备维修', label: '设备维修' },
  { value: '首次开机', label: '首次开机' },
]

export const IPQC_SHIFT_OPTIONS = [
  { value: '早班', label: '早班' },
  { value: '中班', label: '中班' },
  { value: '晚班', label: '晚班' },
]

export const IPQC_PATROL_PLAN_STATUS_OPTIONS = [
  { value: 'active', label: '启用', type: 'success' },
  { value: 'paused', label: '暂停', type: 'warning' },
  { value: 'completed', label: '已完成', type: 'info' },
]

export const IPQC_PATROL_STATUS_OPTIONS = [
  { value: 'scheduled', label: '待执行', type: 'info' },
  { value: 'in_progress', label: '执行中', type: 'primary' },
  { value: 'completed', label: '已完成', type: 'success' },
  { value: 'missed', label: '已错过', type: 'danger' },
  { value: 'cancelled', label: '已取消', type: 'warning' },
]

export const IPQC_PATROL_CONCLUSION_OPTIONS = [
  { value: 'pending', label: '待判定', type: 'info' },
  { value: 'qualified', label: '合格', type: 'success' },
  { value: 'unqualified', label: '不合格', type: 'danger' },
]

export const IPQC_RISK_LEVEL_OPTIONS = [
  { value: 'normal', label: '正常', type: 'success' },
  { value: 'warning', label: '预警', type: 'warning' },
  { value: 'critical', label: '严重', type: 'danger' },
]

export const IPQC_TREND_OPTIONS = [
  { value: 'stable', label: '稳定', type: 'info' },
  { value: 'rising', label: '上升', type: 'danger' },
  { value: 'falling', label: '下降', type: 'success' },
]

export const DATA_TYPE_OPTIONS = [
  { value: 'numeric', label: '数值' },
  { value: 'visual', label: '外观' },
  { value: 'attribute', label: '属性' },
]

export const INSPECTION_RESULT_OPTIONS = [
  { value: 'pass', label: '合格', type: 'success' },
  { value: 'fail', label: '不合格', type: 'danger' },
  { value: 'pending', label: '待检', type: 'info' },
]

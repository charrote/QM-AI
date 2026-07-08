// ─── 质量追溯 ──────────────────────────────────────────────────

export interface MaterialChainItem {
  materialCode: string
  materialName: string
  batchCode: string
  supplier?: string
  quantity?: number
  receivedDate?: string
}

export interface FirstPieceRecord {
  id: number
  serialNumber: string
  productCode?: string
  productName?: string
  inspectedAt: string
  result: string
  inspector?: string
  items?: string[]
}

export interface PatrolRecord {
  id: number
  patrolNo: string
  processName?: string
  equipmentName?: string
  inspectedAt: string
  result: string
  inspector?: string
  defectCount?: number
}

export interface FqcInspection {
  id: number
  inspectionNo: string
  batchCode?: string
  productName?: string
  inspectedAt: string
  result: string
  conclusion?: string
  inspector?: string
  defectCount?: number
}

export interface OqcRelease {
  id: number
  releaseNo: string
  batchCode?: string
  productName?: string
  releasedAt: string
  quantity: number
  customer?: string
  releasedBy?: string
}

export interface TraceResult {
  serialNumber: string
  product?: {
    code: string
    name: string
    specification?: string
  }
  batch?: {
    code: string
    quantity: number
    producedAt: string
  }
  equipment?: {
    id: number
    name: string
    code: string
  }
  materialChain?: MaterialChainItem[]
  firstPieces?: FirstPieceRecord[]
  patrols?: PatrolRecord[]
  fqcInspections?: FqcInspection[]
  oqcReleases?: OqcRelease[]
}

export interface NgDiffusionResult {
  causeBatch: string
  message?: string
  affectedBatches: Array<{
    batchCode: string
    productName?: string
    quantity: number
    defectRate?: number
    affectedStage: string
  }>
  totalAffectedCount: number
  riskLevel: 'low' | 'medium' | 'high' | 'critical'
}

export interface RecallSimulationResult {
  scenarioDescription: string
  batchCode?: string
  productName?: string
  batchQuantity: number
  affectedCustomers?: Array<{
    name: string
    region?: string
    quantity: number
    estimatedLoss?: number
  }>
  estimatedRecallCost: number
}

// ─── 追溯方式枚举 ──────────────────────────────────────────────

export type TraceMethod = 'sn' | 'batch' | 'equipment'

export const TRACE_METHOD_OPTIONS = [
  { value: 'sn', label: 'SN 编码' },
  { value: 'batch', label: '批次号' },
  { value: 'equipment', label: '设备' },
]

export const TRACE_METHOD_MAP: Record<string, string> = {
  sn: 'SN 编码',
  batch: '批次号',
  equipment: '设备',
}

// ─── 风险等级配置 ──────────────────────────────────────────────

export const RISK_LEVEL_CONFIG: Record<string, { label: string; type: string; icon: string }> = {
  low:     { label: '低风险', type: 'success', icon: 'SuccessFilled' },
  medium:  { label: '中风险', type: 'warning', icon: 'WarningFilled' },
  high:    { label: '高风险', type: 'danger',  icon: 'WarningFilled' },
  critical: { label: '严重风险', type: 'danger', icon: 'CircleCloseFilled' },
}
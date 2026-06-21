// ─── 来料登记 ─────────────────────────────────────────
export interface IqcReceipt {
  id: number
  receiptNo: string
  supplierId: number
  supplierName: string
  productId: number
  productName: string
  batchNo?: string
  quantity: number
  unit?: string
  receiptDate?: string
  inspector?: string
  status: string
  createdAt: string
}

export interface IqcReceiptDetail extends IqcReceipt {
  inspections?: IqcInspection[]
  anomalies?: IqcAnomaly[]
}

export interface CreateIqcReceipt {
  receiptNo: string
  supplierId: number
  productId: number
  batchNo?: string
  quantity: number
  unit?: string
  receiptDate?: string
  inspector?: string
}

export interface UpdateIqcReceipt {
  batchNo?: string
  quantity: number
  unit?: string
  receiptDate?: string
  inspector?: string
  status?: string
}

// ─── 检验单 ─────────────────────────────────────────
export interface IqcInspection {
  id: number
  inspectionNo: string
  receiptId: number
  receiptNo?: string
  standardId?: number
  sampleSize: number
  ac: number
  re: number
  defectQty: number
  samplingLevel?: string
  aqlValue?: number
  result: string
  inspector?: string
  inspectedAt?: string
  createdAt: string
}

export interface IqcInspectionDetail extends IqcInspection {
  supplierName: string
  productName: string
  items?: IqcInspectionItem[]
}

export interface CreateIqcInspection {
  receiptId: number
  standardId?: number
  sampleSize: number
  ac: number
  re: number
  samplingLevel?: string
  aqlValue?: number
  inspector?: string
}

export interface SubmitIqcInspection {
  items: IqcInspectionItemSubmit[]
  inspector?: string
}

export interface IqcInspectionItemSubmit {
  id?: number
  paramId?: number
  inspectionItemId?: number
  itemName?: string
  measuredValue?: number
  usl?: number
  lsl?: number
  result: string
  defectCodeId?: number
  remark?: string
}

// ─── 检验明细 ─────────────────────────────────────────
export interface IqcInspectionItem {
  id: number
  inspectionId: number
  paramId?: number
  itemName?: string
  measuredValue?: number
  usl?: number
  lsl?: number
  result: string
  defectCodeId?: number
  defectCodeName?: string
  remark?: string
}

// ─── 异常单 ─────────────────────────────────────────
export interface IqcAnomaly {
  id: number
  anomalyNo: string
  receiptId: number
  receiptNo?: string
  inspectionId?: number
  anomalyType: string
  severity: string
  description?: string
  status: string
  handler?: string
  resolvedAt?: string
  createdAt: string
}

export interface CreateIqcAnomaly {
  anomalyNo?: string
  receiptId: number
  inspectionId?: number
  anomalyType: string
  severity: string
  description?: string
  handler?: string
}

export interface UpdateIqcAnomaly {
  status?: string
  handler?: string
  description?: string
}

export interface ResolveIqcAnomaly {
  resolution: string
  handler?: string
}

// ─── 供应商评分 ───────────────────────────────────────
export interface SupplierScore {
  id: number
  supplierId: number
  supplierName: string
  scoreDate?: string
  score?: number
  dimensionScores?: string
  grade?: string
  evaluation?: string
}

export interface UpdateSupplierScore {
  scoreDate?: string
  score?: number
  dimensionScores?: string
  grade?: string
  evaluation?: string
}

// ─── 抽样方案 ─────────────────────────────────────────
export interface SamplingPlan {
  sampleCode: string
  sampleSize: number
  ac: number
  re: number
  samplingLevel: string
  aqlValue: number
  lotSize: number
  isReduced: boolean
  isNormal: boolean
  isStricter: boolean
}

export interface SamplingPlanRequest {
  lotSize: number
  samplingLevel: string
  aqlValue: number
}

// ─── AI 风险评分 ──────────────────────────────────────
export interface AiRiskScore {
  score: number
  level: string
  factors: RiskFactor[]
  recommendations: string[]
}

export interface RiskFactor {
  name: string
  description: string
  impact: number
}

// ─── 批次追溯 ─────────────────────────────────────────
export interface BatchTrace {
  receipt?: IqcReceipt
  inspections: IqcInspection[]
  anomalies: IqcAnomaly[]
  supplierScore?: SupplierScore
}

// ─── 状态枚举 ─────────────────────────────────────────
export const IQC_RECEIPT_STATUS_OPTIONS = [
  { value: 'pending', label: '待检验', type: 'info' },
  { value: 'inspecting', label: '检验中', type: 'warning' },
  { value: 'qualified', label: '合格', type: 'success' },
  { value: 'unqualified', label: '不合格', type: 'danger' },
  { value: 'anomaly', label: '异常', type: 'danger' },
]

export const IQC_INSPECTION_RESULT_OPTIONS = [
  { value: 'pending', label: '待检验', type: 'info' },
  { value: 'pass', label: '合格', type: 'success' },
  { value: 'fail', label: '不合格', type: 'danger' },
  { value: 'scrap', label: '报废', type: 'danger' },
]

export const IQC_ANOMALY_TYPE_OPTIONS = [
  { value: 'quality', label: '质量问题' },
  { value: 'quantity', label: '数量问题' },
  { value: 'document', label: '单据问题' },
  { value: 'other', label: '其他' },
]

export const IQC_SEVERITY_OPTIONS = [
  { value: 'critical', label: '严重', type: 'danger' },
  { value: 'major', label: '主要', type: 'warning' },
  { value: 'minor', label: '轻微', type: 'info' },
]

export const IQC_ANOMALY_STATUS_OPTIONS = [
  { value: 'open', label: '待处理', type: 'danger' },
  { value: 'processing', label: '处理中', type: 'warning' },
  { value: 'resolved', label: '已解决', type: 'success' },
  { value: 'closed', label: '已关闭', type: 'info' },
]

export const SAMPLING_LEVEL_OPTIONS = [
  { value: 'S-1', label: 'S-1 (特殊水平)' },
  { value: 'S-2', label: 'S-2 (特殊水平)' },
  { value: 'S-3', label: 'S-3 (特殊水平)' },
  { value: 'S-4', label: 'S-4 (特殊水平)' },
  { value: 'I', label: 'I (一般水平)' },
  { value: 'II', label: 'II (一般水平)' },
  { value: 'III', label: 'III (一般水平)' },
]

// ─── SPC 控制图 ──────────────────────────────────────────────────

export interface SpcControlChart {
  id: number
  name: string
  processId: number
  processName?: string
  parameterCode: string
  chartType: string
  subgroupSize: number
  usl?: number
  lsl?: number
  targetValue?: number
  cl?: number
  ucl?: number
  lcl?: number
  createdAt: string
}

export interface SpcControlChartDetail extends SpcControlChart {
  createdBy: number
  updatedAt: string
  dataPointCount: number
}

export interface CreateSpcControlChart {
  name: string
  processId: number
  parameterCode: string
  chartType: string
  subgroupSize: number
  usl?: number
  lsl?: number
  targetValue?: number
}

export interface UpdateSpcControlChart {
  name: string
  subgroupSize: number
  usl?: number
  lsl?: number
  targetValue?: number
}

// ─── SPC 数据点 ──────────────────────────────────────────────────

export interface SpcDataPoint {
  id: number
  chartId: number
  subgroupIndex: number
  individualValues: string
  subgroupMean?: number
  subgroupRange?: number
  measuredAt: string
  createdAt: string
}

export interface CreateSpcDataPoint {
  chartId: number
  subgroupIndex: number
  individualValues: string
  measuredAt: string
}

export interface BatchCreateDataPoints {
  chartId: number
  dataPoints: CreateSpcDataPoint[]
}

// ─── SPC 分析结果 ────────────────────────────────────────────────

export interface SpcAnalysisResult {
  id: number
  chartId: number
  analysisType: string
  cp?: number
  cpk?: number
  pp?: number
  ppk?: number
  sigmaWithin?: number
  sigmaOverall?: number
  estimatedPpm?: number
  dataPointsUsed?: number
  grade?: string
  analysisPeriodStart?: string
  analysisPeriodEnd?: string
  createdAt: string
}

// ─── SPC 判异规则 ────────────────────────────────────────────────

export interface SpcAlertRule {
  id: number
  chartId: number
  ruleNumber: number
  ruleName: string
  ruleDescription?: string
  enabled: boolean
  triggerThreshold: number
  sigmaThreshold: number
  updatedAt: string
}

export interface UpdateSpcAlertRule {
  enabled: boolean
  triggerThreshold: number
  sigmaThreshold: number
}

// ─── SPC 报警触发 ────────────────────────────────────────────────

export interface SpcAlertTrigger {
  id: number
  chartId: number
  ruleId: number
  ruleNumber: number
  ruleName: string
  triggeredAt: string
  violatedPointIndex: number
  detail?: string
  resolved: boolean
  resolvedAt?: string
}

// ─── ANOVA 方差分析 ──────────────────────────────────────────────

export interface SpcAnovaResult {
  id: number
  chartId: number
  source: string
  sumOfSquares: number
  degreesFreedom: number
  meanSquare: number
  fRatio: number
  pValue: number
  significant: boolean
  analysisDate: string
}

export interface SpcAnovaFactor {
  source: string
  label: string
  values: number[]
}

export interface SpcAnovaRequest {
  chartId: number
  factors: SpcAnovaFactor[]
}

// ─── SPC 控制限 ──────────────────────────────────────────────────

export interface SpcControlLimits {
  clXbar: number
  uclXbar: number
  lclXbar: number
  clR: number
  uclR: number
  lclR: number
}

// ─── SPC 判异违规 ────────────────────────────────────────────────

export interface SpcRuleViolation {
  ruleNumber: number
  index: number
  description: string
}

// ─── SPC 分析报告 ────────────────────────────────────────────────

export interface SpcAnalysisReport {
  chart?: SpcControlChart
  controlLimits?: SpcControlLimits
  capability?: SpcAnalysisResult
  violations: SpcRuleViolation[]
  dataPoints: SpcDataPoint[]
}

// ─── SPC 分析请求 ────────────────────────────────────────────────

export interface SpcAnalyzeRequest {
  chartId: number
  periodStart?: string
  periodEnd?: string
}

// ─── 常量（下拉选项） ─────────────────────────────────────────────

export const CHART_TYPE_OPTIONS = [
  { value: 'Xbar_R', label: 'X̄-R 均值-极差图' },
  { value: 'Xbar_S', label: 'X̄-S 均值-标准差图' },
  { value: 'I_MR', label: 'I-MR 单值-移动极差图' },
]

export const CHART_TYPE_MAP: Record<string, string> = {
  Xbar_R: 'X̄-R 均值-极差图',
  Xbar_S: 'X̄-S 均值-标准差图',
  I_MR: 'I-MR 单值-移动极差图',
}

export const CPK_GRADE_OPTIONS = [
  { value: '优秀', label: '优秀 (Cpk ≥ 1.67)', type: 'success' },
  { value: '良好', label: '良好 (Cpk ≥ 1.33)', type: 'primary' },
  { value: '临界', label: '临界 (Cpk ≥ 1.0)', type: 'warning' },
  { value: '不足', label: '不足 (Cpk < 1.0)', type: 'danger' },
]

export const CPK_GRADE_TYPE: Record<string, string> = {
  优秀: 'success',
  良好: 'primary',
  临界: 'warning',
  不足: 'danger',
}

export const ANOVA_SOURCE_OPTIONS = [
  { value: 'operator', label: '操作人员' },
  { value: 'machine', label: '设备' },
  { value: 'material', label: '物料' },
  { value: 'method', label: '工艺方法' },
  { value: 'environment', label: '环境' },
]

export const ANOVA_SOURCE_MAP: Record<string, string> = {
  operator: '操作人员',
  machine: '设备',
  material: '物料',
  method: '工艺方法',
  environment: '环境',
}

// ─── SPC 数据源（贯通S3/S4/S5） ──────────────────────────────

export interface SpcDataSource {
  id: number
  chartId: number
  sourceType: string
  inspectionItemId?: number
  inspectionItemName?: string
  productId?: number
  processId?: number
  supplierId?: number
  customerId?: number
  equipmentId?: number
  createdAt: string
}

export interface CreateSpcDataSource {
  chartId: number
  sourceType: string
  inspectionItemId?: number
  productId?: number
  processId?: number
  supplierId?: number
  customerId?: number
  equipmentId?: number
}

// SPC 数据源类型选项
export const SOURCE_TYPE_OPTIONS = [
  { value: 'IQC', label: 'IQC来料检验' },
  { value: 'IPQC-PATROL', label: 'IPQC巡检' },
  { value: 'IPQC-FIRSTPIECE', label: 'IPQC首件检验' },
  { value: 'FQC', label: 'FQC成品检验' },
]

// ─── 业务检验数据（SPC从业务模块拉取） ─────────────────────────

export interface BusinessInspectionData {
  id: number
  sourceType: string
  sourceNo: string
  inspectionItemId?: number
  inspectionItemName?: string
  measuredValue?: number
  result: string
  inspectedAt: string
  productId?: number
  processId?: number
  supplierId?: number
  customerId?: number
  equipmentId?: number
}

// Western Electric 8大判异规则默认配置
export const WESTERN_ELECTRIC_RULES = [
  { ruleNumber: 1, ruleName: '1点超出3σ控制限', ruleDescription: '任何数据点超出UCL或LCL', defaultThreshold: 1, defaultSigma: 3.0 },
  { ruleNumber: 2, ruleName: '连续9点在CL同侧', ruleDescription: '连续9个点位于中心线同一侧', defaultThreshold: 9, defaultSigma: 0 },
  { ruleNumber: 3, ruleName: '连续6点递增或递减', ruleDescription: '连续6个点单调上升或下降', defaultThreshold: 6, defaultSigma: 0 },
  { ruleNumber: 4, ruleName: '连续14点上下交替', ruleDescription: '连续14个点呈现上下交替模式', defaultThreshold: 14, defaultSigma: 0 },
  { ruleNumber: 5, ruleName: '连续3点中2点超出2σ', ruleDescription: '连续3点中有2点落在2σ和3σ之间（同一侧）', defaultThreshold: 2, defaultSigma: 2.0 },
  { ruleNumber: 6, ruleName: '连续5点中4点超出1σ', ruleDescription: '连续5点中有4点落在1σ和2σ之间（同一侧）', defaultThreshold: 4, defaultSigma: 1.0 },
  { ruleNumber: 7, ruleName: '连续15点在1σ内', ruleDescription: '连续15个点落在中心线1σ范围内（任一侧）', defaultThreshold: 15, defaultSigma: 1.0 },
  { ruleNumber: 8, ruleName: '连续8点超出1σ', ruleDescription: '连续8个点落在1σ范围外（双侧）', defaultThreshold: 8, defaultSigma: 1.0 },
]

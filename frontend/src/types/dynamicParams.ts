// ─── 通用 ────────────────────────────────────────────
export interface PagedRequest {
  page?: number
  pageSize?: number
  keyword?: string
  sortBy?: string
  sortOrder?: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

// ─── ParamGroup 参数组 ───────────────────────────────
export interface ParamGroup {
  id: number
  name: string
  code: string
  description?: string
  sortOrder: number
  paramCount: number
  createdAt: string
}

export interface ParamGroupDetail {
  id: number
  name: string
  code: string
  description?: string
  sortOrder: number
  createdAt: string
  updatedAt: string
}

export interface CreateParamGroup {
  name: string
  code: string
  description?: string
  sortOrder?: number
}

export interface UpdateParamGroup {
  name: string
  description?: string
  sortOrder?: number
}

// ─── DynamicParam 动态参数 ────────────────────────────
export interface DynamicParam {
  id: number
  groupId: number
  groupName: string
  name: string
  code: string
  dataType: string // 'numeric' | 'categorical' | 'boolean'
  unit?: string
  targetValue?: number
  usl?: number
  lsl?: number
  precision: number
  aiStrategy?: string
  sortOrder: number
  isActive: boolean
  createdAt: string
}

export interface DynamicParamDetail {
  id: number
  groupId: number
  groupName: string
  name: string
  code: string
  dataType: string
  unit?: string
  targetValue?: number
  usl?: number
  lsl?: number
  precision: number
  aiStrategy?: string
  sortOrder: number
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateDynamicParam {
  groupId: number
  name: string
  code: string
  dataType: string
  unit?: string
  targetValue?: number
  usl?: number
  lsl?: number
  precision?: number
  aiStrategy?: string
  sortOrder?: number
}

export interface UpdateDynamicParam {
  groupId: number
  name: string
  dataType: string
  unit?: string
  targetValue?: number
  usl?: number
  lsl?: number
  precision?: number
  aiStrategy?: string
  sortOrder?: number
  isActive: boolean
}

// ─── ClosureRule 关单规则 ────────────────────────────
export interface ClosureRule {
  id: number
  name: string
  code: string
  logic: string // 'AND' | 'OR'
  description?: string
  isActive: boolean
  createdAt: string
}

export interface ClosureRuleDetail {
  id: number
  name: string
  code: string
  conditionJson: string
  logic: string
  description?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateClosureRule {
  name: string
  code: string
  conditionJson: string
  logic: string
  description?: string
}

export interface UpdateClosureRule {
  name: string
  conditionJson: string
  logic: string
  description?: string
  isActive: boolean
}

export interface ClosureCondition {
  type: string        // 'consecutive_ok' | 'spk_cpk' | 'sampling_rate' | 'ai_risk_score'
  paramCode: string
  threshold: number
  operator: string    // '>' | '>=' | '<' | '<=' | '='
}

export interface ClosureEvaluationRequest {
  ruleId: number
}

export interface ClosureEvaluationResult {
  isSatisfied: boolean
  failedConditions?: string[]
  ruleName: string
  logic: string
}

// ─── AI Strategy ─────────────────────────────────────
export interface AiStrategyPreset {
  id: string
  name: string
  dataType: string
  description: string
}

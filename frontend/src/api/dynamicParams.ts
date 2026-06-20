import request from './request'
import type {
  PagedResult, PagedRequest,
  ParamGroup, ParamGroupDetail, CreateParamGroup, UpdateParamGroup,
  DynamicParam, DynamicParamDetail, CreateDynamicParam, UpdateDynamicParam,
  ClosureRule, ClosureRuleDetail, CreateClosureRule, UpdateClosureRule,
  ClosureEvaluationRequest, ClosureEvaluationResult,
  AiStrategyPreset,
} from '@/types/dynamicParams'

// ─── ParamGroup API ─────────────────────────────────
export const paramGroupApi = {
  list(params?: PagedRequest): Promise<PagedResult<ParamGroup>> {
    return request.get('/param-groups', { params }).then(r => r.data)
  },
  getAll(): Promise<ParamGroup[]> {
    return request.get('/param-groups/all').then(r => r.data)
  },
  get(id: number): Promise<ParamGroupDetail> {
    return request.get(`/param-groups/${id}`).then(r => r.data)
  },
  create(data: CreateParamGroup): Promise<ParamGroupDetail> {
    return request.post('/param-groups', data).then(r => r.data)
  },
  update(id: number, data: UpdateParamGroup): Promise<ParamGroupDetail> {
    return request.put(`/param-groups/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`/param-groups/${id}`)
  },
}

// ─── DynamicParam API ───────────────────────────────
export const dynamicParamApi = {
  list(params?: PagedRequest & { groupId?: number }): Promise<PagedResult<DynamicParam>> {
    return request.get('/dynamic-params', { params }).then(r => r.data)
  },
  getByGroup(groupId: number): Promise<DynamicParam[]> {
    return request.get(`/dynamic-params/by-group/${groupId}`).then(r => r.data)
  },
  get(id: number): Promise<DynamicParamDetail> {
    return request.get(`/dynamic-params/${id}`).then(r => r.data)
  },
  create(data: CreateDynamicParam): Promise<DynamicParamDetail> {
    return request.post('/dynamic-params', data).then(r => r.data)
  },
  update(id: number, data: UpdateDynamicParam): Promise<DynamicParamDetail> {
    return request.put(`/dynamic-params/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`/dynamic-params/${id}`)
  },
  getAiStrategies(): Promise<AiStrategyPreset[]> {
    return request.get('/dynamic-params/ai-strategies').then(r => r.data)
  },
}

// ─── ClosureRule API ────────────────────────────────
export const closureRuleApi = {
  list(params?: PagedRequest): Promise<PagedResult<ClosureRule>> {
    return request.get('/closure-rules', { params }).then(r => r.data)
  },
  getAll(): Promise<ClosureRule[]> {
    return request.get('/closure-rules/all').then(r => r.data)
  },
  get(id: number): Promise<ClosureRuleDetail> {
    return request.get(`/closure-rules/${id}`).then(r => r.data)
  },
  create(data: CreateClosureRule): Promise<ClosureRuleDetail> {
    return request.post('/closure-rules', data).then(r => r.data)
  },
  update(id: number, data: UpdateClosureRule): Promise<ClosureRuleDetail> {
    return request.put(`/closure-rules/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`/closure-rules/${id}`)
  },
  evaluate(data: ClosureEvaluationRequest): Promise<ClosureEvaluationResult> {
    return request.post('/closure-rules/evaluate', data).then(r => r.data)
  },
}

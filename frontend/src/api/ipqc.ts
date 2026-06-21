import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  IpqcFirstPiece, IpqcFirstPieceDetail, CreateIpqcFirstPiece, SubmitIpqcFirstPiece,
  IpqcPatrolPlan, CreateIpqcPatrolPlan, UpdateIpqcPatrolPlan,
  IpqcPatrol, IpqcPatrolDetail, SubmitIpqcPatrol,
  IpqcAiRiskScore, IpqcClosureEvaluation,
} from '@/types/ipqc'

const BASE = '/ipqc'

// ─── 首件检验 API ───────────────────────────────────
export const firstPieceApi = {
  list(params: PagedRequest): Promise<PagedResult<IpqcFirstPiece>> {
    return request.get(`${BASE}/first-pieces`, { params }).then(r => r.data)
  },
  get(id: number): Promise<IpqcFirstPieceDetail> {
    return request.get(`${BASE}/first-pieces/${id}`).then(r => r.data)
  },
  create(data: CreateIpqcFirstPiece): Promise<IpqcFirstPieceDetail> {
    return request.post(`${BASE}/first-pieces`, data).then(r => r.data)
  },
  submit(id: number, data: SubmitIpqcFirstPiece): Promise<IpqcFirstPieceDetail> {
    return request.post(`${BASE}/first-pieces/${id}/submit`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/first-pieces/${id}`)
  },
}

// ─── 巡检计划 API ───────────────────────────────────
export const patrolPlanApi = {
  list(params: PagedRequest): Promise<PagedResult<IpqcPatrolPlan>> {
    return request.get(`${BASE}/patrol-plans`, { params }).then(r => r.data)
  },
  get(id: number): Promise<IpqcPatrolPlan> {
    return request.get(`${BASE}/patrol-plans/${id}`).then(r => r.data)
  },
  create(data: CreateIpqcPatrolPlan): Promise<IpqcPatrolPlan> {
    return request.post(`${BASE}/patrol-plans`, data).then(r => r.data)
  },
  update(id: number, data: UpdateIpqcPatrolPlan): Promise<IpqcPatrolPlan> {
    return request.put(`${BASE}/patrol-plans/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/patrol-plans/${id}`)
  },
  generate(planId: number, data: { workOrderId?: number; startTime: string; count: number }): Promise<IpqcPatrol[]> {
    return request.post(`${BASE}/patrol-plans/${planId}/generate`, data).then(r => r.data)
  },
}

// ─── 巡检记录 API ───────────────────────────────────
export const patrolApi = {
  list(params: PagedRequest): Promise<PagedResult<IpqcPatrol>> {
    return request.get(`${BASE}/patrols`, { params }).then(r => r.data)
  },
  get(id: number): Promise<IpqcPatrolDetail> {
    return request.get(`${BASE}/patrols/${id}`).then(r => r.data)
  },
  submit(id: number, data: SubmitIpqcPatrol): Promise<IpqcPatrolDetail> {
    return request.post(`${BASE}/patrols/${id}/submit`, data).then(r => r.data)
  },
  miss(id: number): Promise<void> {
    return request.post(`${BASE}/patrols/${id}/miss`)
  },
}

// ─── AI 风险评分 API ───────────────────────────────
export const riskScoreApi = {
  analyze(equipmentId: number, processId: number, workOrderId?: number): Promise<IpqcAiRiskScore> {
    return request.get(`${BASE}/risk-score`, {
      params: { equipmentId, processId, workOrderId },
    }).then(r => r.data)
  },
  latest(equipmentId: number, processId: number): Promise<IpqcAiRiskScore> {
    return request.get(`${BASE}/risk-score/latest`, {
      params: { equipmentId, processId },
    }).then(r => r.data)
  },
  history(equipmentId: number, processId: number, hours = 24): Promise<IpqcAiRiskScore[]> {
    return request.get(`${BASE}/risk-score/history`, {
      params: { equipmentId, processId, hours },
    }).then(r => r.data)
  },
}

// ─── 关单评估 API ───────────────────────────────────
export const closureApi = {
  evaluate(workOrderId: number, processId: number, equipmentId: number): Promise<IpqcClosureEvaluation> {
    return request.get(`${BASE}/closure/evaluate`, {
      params: { workOrderId, processId, equipmentId },
    }).then(r => r.data)
  },
}

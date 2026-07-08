import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'

const BASE = '/m10/ai'

export const aiApi = {
  // ─── 预警中心 ─────────────────────────────────────────

  alerts(params?: PagedRequest & { level?: string; resolved?: boolean }): Promise<PagedResult<{
    id: number
    level: string
    title: string
    description: string
    source: string
    resolved: boolean
    createdAt: string
    resolvedAt?: string
  }>> {
    return request.get(`${BASE}/alerts`, { params }).then(r => r.data)
  },

  resolveAlert(id: number): Promise<void> {
    return request.put(`${BASE}/alerts/${id}/resolve`).then(r => r.data)
  },

  alertStats(): Promise<{
    totalAlerts: number
    unresolvedAlerts: number
    byLevel: Record<string, number>
    bySource: Record<string, number>
  }> {
    return request.get(`${BASE}/alerts/stats`).then(r => r.data)
  },

  // ─── 根因分析 ─────────────────────────────────────────

  runRootCauseAnalysis(params: {
    productId?: number
    defectCode?: string
    startDate?: string
    endDate?: string
    processId?: number
  }): Promise<{
    findings: Array<{
      cause: string
      confidence: number
      evidence: string
      recommendation: string
    }>
    summary: string
  }> {
    return request.post(`${BASE}/root-cause`, params).then(r => r.data)
  },

  // ─── 模型管理 ─────────────────────────────────────────

  models(params?: PagedRequest & { status?: string }): Promise<PagedResult<{
    id: number
    name: string
    type: string
    status: string
    accuracy?: number
    trainedAt?: string
    createdAt: string
  }>> {
    return request.get(`${BASE}/models`, { params }).then(r => r.data)
  },

  trainModel(data: { name: string; type: string; trainingData?: string }): Promise<{ id: number }> {
    return request.post(`${BASE}/models/train`, data).then(r => r.data)
  },

  getModel(id: number): Promise<{
    id: number
    name: string
    type: string
    status: string
    accuracy?: number
    parameters: Record<string, any>
    metrics?: Record<string, number>
    trainedAt?: string
    createdAt: string
  }> {
    return request.get(`${BASE}/models/${id}`).then(r => r.data)
  },

  deleteModel(id: number): Promise<void> {
    return request.delete(`${BASE}/models/${id}`).then(() => {})
  },
}

import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  Defect,
  Capa,
  CreateDefect,
  UpdateDefect,
  CreateCapa,
  CapaTemporaryMeasure,
  CapaRootCause,
  CapaCorrectiveAction,
  CapaPreventiveAction,
  CapaVerification,
  ScrapReworkRecord,
  CreateScrapRework,
  UpdateReworkResult,
} from '@/types/defect'

const BASE = '/defects'

export const defectApi = {
  // ─── Defect CRUD ────────────────────────────────────────

  /** 获取不良列表 */
  getAllDefects(params?: PagedRequest & { sourceType?: string; severity?: string; status?: string }): Promise<PagedResult<Defect>> {
    return request.get(`${BASE}`, { params }).then(r => r.data)
  },

  /** 获取不良详情 */
  getDefectById(id: number): Promise<Defect> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },

  /** 创建不良记录 */
  createDefect(data: CreateDefect): Promise<Defect> {
    return request.post(`${BASE}`, data).then(r => r.data)
  },

  /** 更新不良记录 */
  updateDefect(id: number, data: UpdateDefect): Promise<Defect> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },

  /** 删除不良记录 */
  deleteDefect(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },

  /** 获取不良数量统计 */
  getDefectCount(sourceType?: string): Promise<number> {
    return request.get(`${BASE}/count`, { params: { sourceType } }).then(r => r.data)
  },

  // ─── CAPA ───────────────────────────────────────────────

  /** 获取CAPA列表 */
  getAllCapa(params?: { status?: string; phase?: number; page?: number; pageSize?: number }): Promise<PagedResult<Capa>> {
    return request.get(`${BASE}/capa`, { params }).then(r => r.data)
  },

  /** 获取CAPA详情 */
  getCapaById(id: number): Promise<Capa> {
    return request.get(`${BASE}/capa/${id}`).then(r => r.data)
  },

  /** 创建CAPA */
  createCapa(data: CreateCapa): Promise<Capa> {
    return request.post(`${BASE}/capa`, data).then(r => r.data)
  },

  /** 更新CAPA */
  updateCapa(id: number, data: Partial<CreateCapa>): Promise<Capa> {
    return request.put(`${BASE}/capa/${id}`, data).then(r => r.data)
  },

  /** 更新CAPA阶段 */
  updateCapaPhase(id: number, phase: number): Promise<Capa> {
    return request.put(`${BASE}/capa/${id}/phase`, { phase }).then(r => r.data)
  },

  /** 删除CAPA */
  deleteCapa(id: number): Promise<void> {
    return request.delete(`${BASE}/capa/${id}`)
  },

  // ─── 临时措施 ───────────────────────────────────────────

  /** 添加临时措施 */
  addTemporaryMeasure(capaId: number, data: { description: string; executedBy: string; executedAt: string }): Promise<CapaTemporaryMeasure> {
    return request.post(`${BASE}/capa/${capaId}/temporary-measures`, data).then(r => r.data)
  },

  // ─── 根本原因分析 ───────────────────────────────────────

  /** 添加根本原因分析 */
  addRootCause(capaId: number, data: { analysisMethod: string; content: string; rootCauseSummary: string }): Promise<CapaRootCause> {
    return request.post(`${BASE}/capa/${capaId}/root-causes`, data).then(r => r.data)
  },

  /** 获取根本原因列表 */
  getRootCauses(capaId: number): Promise<CapaRootCause[]> {
    return request.get(`${BASE}/capa/${capaId}/root-causes`).then(r => r.data)
  },

  // ─── 纠正措施 ───────────────────────────────────────────

  /** 添加纠正措施 */
  addCorrectiveAction(capaId: number, data: { actionDescription: string; responsiblePerson: string; dueDate: string }): Promise<CapaCorrectiveAction> {
    return request.post(`${BASE}/capa/${capaId}/corrective-actions`, data).then(r => r.data)
  },

  /** 更新纠正措施状态 */
  updateCorrectiveActionStatus(actionId: number, status: string): Promise<CapaCorrectiveAction> {
    return request.put(`${BASE}/capa/corrective-actions/${actionId}/status`, { status }).then(r => r.data)
  },

  // ─── 预防措施 ───────────────────────────────────────────

  /** 添加预防措施 */
  addPreventiveAction(capaId: number, data: { actionDescription: string; responsiblePerson: string; dueDate: string }): Promise<CapaPreventiveAction> {
    return request.post(`${BASE}/capa/${capaId}/preventive-actions`, data).then(r => r.data)
  },

  /** 更新预防措施状态 */
  updatePreventiveActionStatus(actionId: number, status: string): Promise<CapaPreventiveAction> {
    return request.put(`${BASE}/capa/preventive-actions/${actionId}/status`, { status }).then(r => r.data)
  },

  // ─── 效果验证 ───────────────────────────────────────────

  /** 添加效果验证 */
  addVerification(capaId: number, data: { verifierId: number; verificationDate: string; conclusion: string; evidence?: string; remarks?: string }): Promise<CapaVerification> {
    return request.post(`${BASE}/capa/${capaId}/verifications`, data).then(r => r.data)
  },

  // ─── 报废/返工 ─────────────────────────────────────────

  /** 获取报废返工记录 */
  getScrapReworkRecords(defectId?: number): Promise<ScrapReworkRecord[]> {
    return request.get(`${BASE}/scrap-rework`, { params: { defectId } }).then(r => r.data)
  },

  /** 创建报废返工记录 */
  createScrapRework(data: CreateScrapRework): Promise<ScrapReworkRecord> {
    return request.post(`${BASE}/scrap-rework`, data).then(r => r.data)
  },

  /** 更新返工检验结果 */
  updateReworkResult(id: number, data: UpdateReworkResult): Promise<ScrapReworkRecord> {
    return request.put(`${BASE}/scrap-rework/${id}/result`, data).then(r => r.data)
  },
}
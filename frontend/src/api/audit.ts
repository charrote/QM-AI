import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  Audit, AuditFinding, CreateAudit, CreateAuditFinding,
  UpdateFindingStatus, VerifyFinding,
} from '@/types/audit'

const BASE = '/m13/audits'

export const auditApi = {
  list(params?: PagedRequest & { auditType?: string; status?: string; keyword?: string }): Promise<PagedResult<Audit>> {
    return request.get(BASE, { params }).then(r => r.data)
  },

  getById(id: number): Promise<Audit> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },

  create(data: CreateAudit): Promise<Audit> {
    return request.post(BASE, data).then(r => r.data)
  },

  update(id: number, data: Partial<CreateAudit>): Promise<Audit> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },

  remove(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },

  // ─── Findings ─────────────────────────────────────────

  findingsList(auditId: number, params?: { findingType?: string; status?: string }): Promise<AuditFinding[]> {
    return request.get(`${BASE}/${auditId}/findings`, { params }).then(r => r.data)
  },

  findings(auditId: number): Promise<AuditFinding[]> {
    return request.get(`${BASE}/${auditId}/findings`).then(r => r.data)
  },

  allFindings(params?: { findingType?: string; status?: string }): Promise<AuditFinding[]> {
    return request.get(`${BASE}/findings`, { params }).then(r => r.data)
  },

  createFinding(auditId: number, data: CreateAuditFinding): Promise<AuditFinding> {
    return request.post(`${BASE}/${auditId}/findings`, data).then(r => r.data)
  },

  updateFindingStatus(findingId: number, data: UpdateFindingStatus): Promise<AuditFinding> {
    return request.put(`${BASE}/findings/${findingId}`, data).then(r => r.data)
  },

  verifyFinding(id: number, data: VerifyFinding): Promise<AuditFinding> {
    return request.post(`${BASE}/findings/${id}/verify`, data).then(r => r.data)
  },

  removeFinding(id: number): Promise<void> {
    return request.delete(`${BASE}/findings/${id}`)
  },
}

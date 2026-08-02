import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  Complaint, D8Report, ComplaintEvent,
  CreateComplaint, UpdateComplaint, TransitionStatus,
  CreateD8Report, AdvanceDiscipline,
} from '@/types/complaint'

const BASE = '/m09/complaints'

export const complaintApi = {
  list(p?: PagedRequest & { severity?: string; status?: string; customerId?: number }): Promise<PagedResult<Complaint>> {
    return request.get(BASE, { params: p }).then(r => r.data)
  },
  getById(id: number): Promise<Complaint> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },
  create(data: CreateComplaint): Promise<Complaint> {
    return request.post(BASE, data).then(r => r.data)
  },
  update(id: number, data: UpdateComplaint): Promise<Complaint> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },
  remove(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },
  transitionStatus(id: number, data: TransitionStatus): Promise<{ ok: boolean }> {
    return request.post(`${BASE}/${id}/transition`, data).then(r => r.data)
  },
  timeline(id: number): Promise<ComplaintEvent[]> {
    return request.get(`${BASE}/${id}/timeline`).then(r => r.data)
  },

  stats(): Promise<any> {
    return request.get(`${BASE}/stats`).then(r => r.data)
  },

  // ─── D8 Reports ────────────────────────────────────────
  // Note: D8 reports are managed via /api/v1/m09/d8reports controller

  // ─── PDF Export ────────────────────────────────────────

  downloadPdf(id: number): void {
    const url = `${request.defaults.baseURL ?? ''}${BASE}/${id}/export-pdf`
    window.open(url.startsWith('/') ? location.origin + url : url, '_blank')
  },
}

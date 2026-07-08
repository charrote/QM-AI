import request from './request'
import type { D8Report, CreateD8Report, AdvanceDiscipline } from '@/types/complaint'

const BASE = '/m09/d8reports'

export const d8ReportApi = {
  list(complaintId?: number): Promise<D8Report[]> {
    return request.get(`${BASE}/complaint/${complaintId ?? 0}`, { params: { complaintId } }).then(r => r.data)
  },
  create(data: CreateD8Report): Promise<D8Report> {
    return request.post(`${BASE}/complaint/${data.complaintId}`, data).then(r => r.data)
  },
  get(id: number): Promise<D8Report> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },
  update(id: number, data: Partial<D8Report>): Promise<D8Report> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },
  advance(id: number, data: AdvanceDiscipline): Promise<D8Report> {
    return request.post(`${BASE}/${id}/advance`, data).then(r => r.data)
  },
}

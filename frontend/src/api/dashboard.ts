import request from './request'

const BASE = '/m14/dashboard'

export interface PendingItem {
  id: number
  type: 'iqc' | 'fqc' | 'defect' | 'capa' | 'complaint' | 'firstpiece'
  title: string
  detail: string
  time: string
  priority: number
  label: string
}

export const dashboardApi = {
  /** 今日待处理（priority 降序 + time 降序，最多 15 条；total 为全量计数） */
  pending(): Promise<{ items: PendingItem[]; total: number }> {
    return request.get(`${BASE}/pending`).then(r => r.data)
  },
}

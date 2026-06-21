import request from './request'
import type { PagedResult, PagedRequest } from '@/types/basicData'
import type {
  InspectionItem, InspectionItemDetail,
  CreateInspectionItem, UpdateInspectionItem,
} from '@/types/inspectionItem'

const BASE = '/inspection-items'

export const inspectionItemApi = {
  /** 获取检验项目列表（分页） */
  list(params?: PagedRequest): Promise<PagedResult<InspectionItem>> {
    return request.get(BASE, { params }).then(r => r.data)
  },

  /** 获取检验项目下拉列表（全部启用项） */
  getSelectList(): Promise<InspectionItem[]> {
    return request.get(`${BASE}/select-list`).then(r => r.data)
  },

  /** 获取检验项目详情 */
  get(id: number): Promise<InspectionItemDetail> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },

  /** 创建检验项目 */
  create(data: CreateInspectionItem): Promise<InspectionItemDetail> {
    return request.post(BASE, data).then(r => r.data)
  },

  /** 更新检验项目 */
  update(id: number, data: UpdateInspectionItem): Promise<InspectionItemDetail> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },

  /** 删除检验项目 */
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },
}

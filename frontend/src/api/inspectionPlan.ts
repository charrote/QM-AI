import request from './request'
import type { PagedResult, PagedRequest } from '@/types/basicData'
import type {
  InspectionPlan, InspectionPlanDetail,
  CreateInspectionPlan, UpdateInspectionPlan,
} from '@/types/inspectionItem'

const BASE = '/inspection-plans'

export const inspectionPlanApi = {
  /** 获取检验计划列表（分页） */
  list(params?: PagedRequest): Promise<PagedResult<InspectionPlan>> {
    return request.get(BASE, { params }).then(r => r.data)
  },

  /** 根据业务上下文获取检验计划 */
  getByContext(params: {
    inspectionType: string
    productId?: number
    supplierId?: number
    customerId?: number
    processId?: number
    equipmentId?: number
  }): Promise<InspectionPlanDetail[]> {
    return request.get(`${BASE}/by-context`, { params }).then(r => r.data)
  },

  /** 获取检验计划详情 */
  get(id: number): Promise<InspectionPlanDetail> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },

  /** 创建检验计划 */
  create(data: CreateInspectionPlan): Promise<InspectionPlanDetail> {
    return request.post(BASE, data).then(r => r.data)
  },

  /** 更新检验计划 */
  update(id: number, data: UpdateInspectionPlan): Promise<InspectionPlanDetail> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },

  /** 删除检验计划 */
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },
}

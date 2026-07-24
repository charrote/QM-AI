import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  EquipmentParamMapping,
  EquipmentStatusHistory,
  EquipmentQualityCorrelation,
  CreateParamMapping,
  CreateStatusRecord,
} from '@/types/equipmentLink'
import type { Equipment } from '@/types/basicData'

const BASE = '/m11/equipment-link'

export const equipmentLinkApi = {
  // ─── Param Mapping CRUD ───────────────────────────────

  /** Get param mapping list */
  mappings(params?: PagedRequest): Promise<PagedResult<EquipmentParamMapping>> {
    return request.get(`${BASE}/map`, { params }).then(r => r.data)
  },

  /** Get param mapping by ID */
  mappingById(id: number): Promise<EquipmentParamMapping> {
    return request.get(`${BASE}/map/${id}`).then(r => r.data)
  },

  /** Create param mapping */
  createMapping(dto: CreateParamMapping): Promise<EquipmentParamMapping> {
    return request.post(`${BASE}/map`, dto).then(r => r.data)
  },

  /** Update param mapping */
  updateMapping(id: number, dto: CreateParamMapping): Promise<EquipmentParamMapping> {
    return request.put(`${BASE}/map/${id}`, dto).then(r => r.data)
  },

  /** Remove param mapping */
  removeMapping(id: number): Promise<void> {
    return request.delete(`${BASE}/map/${id}`)
  },

  /** Get equipment status history */
  statusHistory(mappingId: number, limit: number = 100): Promise<EquipmentStatusHistory[]> {
    return request.get(`${BASE}/recent-status/${mappingId}`, { params: { limit } }).then(r => r.data)
  },

  /** Get equipment drift data */
  drift(equipmentId: number): Promise<any[]> {
    return request.get(`${BASE}/drift`, { params: { equipmentId } }).then(r => r.data)
  },
}

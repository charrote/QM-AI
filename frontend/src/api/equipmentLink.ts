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
    return request.get(`${BASE}/map`, { params }).then(r => {
      const data = r.data
      // Handle both PagedResult format and raw array response
      if (Array.isArray(data)) {
        return { items: data, total: data.length, page: params?.page || 1, pageSize: params?.pageSize || 20 } as PagedResult<EquipmentParamMapping>
      }
      return data as PagedResult<EquipmentParamMapping>
    })
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
    return request.get(`${BASE}/recent-status/${mappingId}`, { params: { limit } }).then(r => r.data).catch(err => {
      // Silently handle 500 (column mapping issue in older backend versions)
      if (err?.response?.status === 500) {
        return [] as EquipmentStatusHistory[]
      }
      throw err
    })
  },

  /** Get equipment drift data */
  drift(equipmentId: number): Promise<any[]> {
    return request.get(`${BASE}/drift`, { params: { equipmentId } }).then(r => r.data)
  },

  /** Get quality correlations */
  correlations(params?: { equipmentId?: number; dateFrom?: string; dateTo?: string }): Promise<PagedResult<EquipmentQualityCorrelation>> {
    return request.get(`${BASE}/correlations`, { params }).then(r => {
      const data = r.data
      if (Array.isArray(data)) {
        return { items: data, total: data.length } as PagedResult<EquipmentQualityCorrelation>
      }
      return data as PagedResult<EquipmentQualityCorrelation>
    }).catch((err: any) => {
      // Silently handle 404 (endpoint may not exist in older backend versions)
      if (err?.response?.status === 404) {
        return { items: [], total: 0 } as PagedResult<EquipmentQualityCorrelation>
      }
      throw err
    })
  },
}

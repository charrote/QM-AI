import request from './request'
import type { SysDictType, SysDictItem, SysDictFull } from '@/types/sysDict'

const BASE = '/sys-dict'

export const sysDictApi = {
  /** 获取所有字典类型 */
  getTypes(): Promise<SysDictType[]> {
    return request.get(`${BASE}/types`).then(r => r.data)
  },

  /** 获取指定类型的字典项 */
  getItems(typeCode: string): Promise<SysDictItem[]> {
    return request.get(`${BASE}/items/${typeCode}`).then(r => r.data)
  },

  /** 获取所有字典（含选项） */
  getAllFull(): Promise<SysDictFull[]> {
    return request.get(`${BASE}/full`).then(r => r.data)
  },

  /** 批量获取多个字典类型的选项 */
  getBatch(typeCodes: string[]): Promise<Record<string, SysDictItem[]>> {
    return request.post(`${BASE}/batch`, typeCodes).then(r => r.data)
  },

  // ─── Type CRUD ────────────────────────────────
  createType(data: { typeCode: string; typeName: string; remark?: string }): Promise<SysDictType> {
    return request.post(`${BASE}/types`, data).then(r => r.data)
  },

  updateType(id: number, data: { typeName?: string; remark?: string; status?: boolean }): Promise<SysDictType> {
    return request.put(`${BASE}/types/${id}`, data).then(r => r.data)
  },

  deleteType(id: number): Promise<void> {
    return request.delete(`${BASE}/types/${id}`)
  },

  // ─── Item CRUD ────────────────────────────────
  createItem(data: { typeCode: string; itemLabel: string; itemValue: string; sortOrder?: number; color?: string }): Promise<SysDictItem> {
    return request.post(`${BASE}/items`, data).then(r => r.data)
  },

  updateItem(id: number, data: { itemLabel?: string; itemValue?: string; sortOrder?: number; color?: string; status?: boolean }): Promise<SysDictItem> {
    return request.put(`${BASE}/items/${id}`, data).then(r => r.data)
  },

  deleteItem(id: number): Promise<void> {
    return request.delete(`${BASE}/items/${id}`)
  },
}

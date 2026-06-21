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
}

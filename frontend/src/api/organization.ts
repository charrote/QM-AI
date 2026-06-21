import request from './request'
import type {
  OrganizationTreeNode,
  Organization,
  OrganizationDetail,
  CreateOrganization,
  UpdateOrganization,
} from '@/types/organization'

const BASE = '/organizations'

export const organizationApi = {
  /** 获取组织列表（扁平） */
  list(): Promise<Organization[]> {
    return request.get(BASE).then(r => r.data)
  },

  /** 获取组织树 */
  tree(): Promise<OrganizationTreeNode[]> {
    return request.get(`${BASE}/tree`).then(r => r.data)
  },

  /** 获取组织详情 */
  get(id: number): Promise<OrganizationDetail> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },

  /** 创建组织 */
  create(data: CreateOrganization): Promise<OrganizationDetail> {
    return request.post(BASE, data).then(r => r.data)
  },

  /** 更新组织 */
  update(id: number, data: UpdateOrganization): Promise<OrganizationDetail> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },

  /** 删除组织 */
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },

  /** 获取下一级层级 */
  getNextLevel(parentLevel: string): Promise<{ nextLevel: string | null }> {
    return request.get(`${BASE}/next-level/${parentLevel}`).then(r => r.data)
  },
}

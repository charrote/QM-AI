import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type { Role, CreateRole, UpdateRole, Permission, CreatePermission, UpdatePermission } from '@/types/system'

// ─── User ───────────────────────────────────────────────
const USER_BASE = '/users'
export const userApi = {
  list(params: PagedRequest): Promise<PagedResult<import('@/types/system').User>> {
    return request.get(USER_BASE, { params }).then(r => r.data)
  },
  get(id: number): Promise<import('@/types/system').User> {
    return request.get(`${USER_BASE}/${id}`).then(r => r.data)
  },
  create(data: import('@/types/system').CreateUser): Promise<import('@/types/system').User> {
    return request.post(USER_BASE, data).then(r => r.data)
  },
  update(id: number, data: import('@/types/system').UpdateUser): Promise<import('@/types/system').User> {
    return request.put(`${USER_BASE}/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${USER_BASE}/${id}`)
  },
  getRoles(): Promise<Array<{ id: number; name: string }>> {
    return request.get(`${USER_BASE}/roles`).then(r => r.data)
  },
}

// ─── Role ───────────────────────────────────────────────
const ROLE_BASE = '/roles'
export const roleApi = {
  list(params: PagedRequest): Promise<PagedResult<Role>> {
    return request.get(ROLE_BASE, { params }).then(r => r.data)
  },
  get(id: number): Promise<Role> {
    return request.get(`${ROLE_BASE}/${id}`).then(r => r.data)
  },
  create(data: CreateRole): Promise<Role> {
    return request.post(ROLE_BASE, data).then(r => r.data)
  },
  update(id: number, data: UpdateRole): Promise<Role> {
    return request.put(`${ROLE_BASE}/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${ROLE_BASE}/${id}`)
  },
}

// ─── Permission ─────────────────────────────────────────
const PERM_BASE = '/permissions'
export const permissionApi = {
  list(params: PagedRequest): Promise<PagedResult<Permission>> {
    return request.get(PERM_BASE, { params }).then(r => r.data)
  },
  get(id: number): Promise<Permission> {
    return request.get(`${PERM_BASE}/${id}`).then(r => r.data)
  },
  create(data: CreatePermission): Promise<Permission> {
    return request.post(PERM_BASE, data).then(r => r.data)
  },
  update(id: number, data: UpdatePermission): Promise<Permission> {
    return request.put(`${PERM_BASE}/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${PERM_BASE}/${id}`)
  },
}

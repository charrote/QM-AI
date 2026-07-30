// ─── User ───────────────────────────────────────────────
export interface User {
  id: number
  username: string
  displayName?: string
  email?: string
  roleName?: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface CreateUser {
  username: string
  password: string
  displayName?: string
  email?: string
  roleId: number
}

export interface UpdateUser {
  displayName?: string
  email?: string
  roleId?: number
  isActive?: boolean
  password?: string
}

// ─── Role ───────────────────────────────────────────────
export interface Role {
  id: number
  name: string
  description?: string
  userCount: number
  createdAt: string
}

export interface CreateRole {
  name: string
  description?: string
}

export interface UpdateRole {
  name?: string
  description?: string
}

// ─── Permission ─────────────────────────────────────────
export interface Permission {
  id: number
  name: string
  code: string
  module?: string
}

export interface CreatePermission {
  name: string
  code: string
  module?: string
}

export interface UpdatePermission {
  name?: string
  code?: string
  module?: string
}

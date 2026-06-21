// ─── Organization ─────────────────────────────────────────
export interface OrganizationTreeNode {
  id: number
  code: string
  name: string
  level: 'group' | 'company' | 'workshop' | 'line'
  parentId: number | null
  sortOrder: number
  isActive: boolean
  location?: string
  description?: string
  createdBy?: number
  childCount: number
  children: OrganizationTreeNode[]
}

export interface Organization {
  id: number
  code: string
  name: string
  level: string
  parentId: number | null
  parentName?: string
  sortOrder: number
  isActive: boolean
  location?: string
  description?: string
  createdAt: string
}

export interface OrganizationDetail {
  id: number
  code: string
  name: string
  level: string
  parentId: number | null
  parentName?: string
  sortOrder: number
  isActive: boolean
  location?: string
  contact?: string
  description?: string
  createdBy?: number
  createdAt: string
  updatedAt: string
}

export interface CreateOrganization {
  code: string
  name: string
  level: string
  parentId?: number | null
  sortOrder?: number
  location?: string
  contact?: string
  description?: string
  createdBy?: number
}

export interface UpdateOrganization {
  code: string
  name: string
  parentId?: number | null
  sortOrder?: number
  isActive?: boolean
  location?: string
  contact?: string
  description?: string
}

// 层级配置常量
export const LEVEL_CONFIG: Record<string, { label: string; color: string }> = {
  group: { label: '集团', color: '#8B5CF6' },
  company: { label: '公司', color: '#409EFF' },
  workshop: { label: '车间', color: '#E6A23C' },
  line: { label: '产线', color: '#67C23A' },
}

export const NEXT_LEVEL: Record<string, string> = {
  group: 'company',
  company: 'workshop',
  workshop: 'line',
  line: '',
}

export const LEVEL_ORDER: Record<string, number> = {
  group: 0,
  company: 1,
  workshop: 2,
  line: 3,
}

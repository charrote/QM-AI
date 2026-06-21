// ─── SysDict ─────────────────────────────────────────────
export interface SysDictType {
  id: number
  typeCode: string
  typeName: string
  isSystem: boolean
  status: boolean
  remark?: string
}

export interface SysDictItem {
  id: number
  typeCode: string
  itemLabel: string
  itemValue: string
  sortOrder: number
  color?: string
  isDefault: boolean
  status: boolean
}

export interface SysDictFull {
  type: SysDictType
  items: SysDictItem[]
}

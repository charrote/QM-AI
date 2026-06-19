export interface TabItem {
  id: string
  name: string
  icon?: string
  path: string
  closable: boolean
  order: number
}

export interface MenuConfig extends TabItem {
  module: string
}

export interface PageResult<T> {
  items: T[]
  total: number
  currentPage: number
  pageSize: number
}

export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
}

export interface PaginationParams {
  currentPage: number
  pageSize: number
}

export interface SortParam {
  field: string
  order: 'asc' | 'desc'
}

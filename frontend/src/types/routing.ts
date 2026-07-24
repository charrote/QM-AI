// ─── 路线类型 ──────────────────────────────────────
export type RouteType = 'STD' | 'ALT' | 'EMG' | 'CUS'

export const ROUTE_TYPE_LABELS: Record<RouteType, string> = {
  STD: '标准',
  ALT: '替代',
  EMG: '紧急',
  CUS: '客制',
}

export const ROUTE_TYPE_COLORS: Record<RouteType, string> = {
  STD: 'primary',
  ALT: 'warning',
  EMG: 'danger',
  CUS: 'success',
}

export const ROUTE_TYPE_OPTIONS = [
  { label: '标准', value: 'STD' as RouteType },
  { label: '替代', value: 'ALT' as RouteType },
  { label: '紧急', value: 'EMG' as RouteType },
  { label: '客制', value: 'CUS' as RouteType },
]

// ─── 路线头 DTO ──────────────────────────────────────
export interface RouteHeaderDto {
  id: number
  productId: number
  routeCode: string
  routeName: string
  routeType: RouteType
  description?: string
  isDefault: boolean
  isActive: boolean
  sortOrder: number
  stepCount: number
  totalStandardTimeMinutes: number
  createdAt: string
  updatedAt: string
}

export interface RouteListDto {
  productId: number
  productName: string
  productCode: string
  routes: RouteHeaderDto[]
}

export interface RouteDetailDto {
  id: number
  productId: number
  productName: string
  productCode: string
  routeCode: string
  routeName: string
  routeType: RouteType
  description?: string
  isDefault: boolean
  isActive: boolean
  sortOrder: number
  stepCount: number
  totalStandardTimeMinutes: number
  steps: ProductRouteStepDto[]
  createdAt: string
  updatedAt: string
}

export interface CreateRouteHeaderDto {
  productId: number
  routeCode: string
  routeName: string
  routeType: RouteType
  description?: string
  isDefault?: boolean
}

export interface UpdateRouteHeaderDto {
  routeCode?: string
  routeName?: string
  routeType?: RouteType
  description?: string
  isDefault?: boolean
}

export interface CloneRouteHeaderDto {
  sourceHeaderId: number
  targetProductId: number
  targetRouteCode: string
  targetRouteName: string
  targetRouteType: RouteType
}

export interface CreateRouteStepDto {
  processId: number
  stepOrder?: number
  standardTimeMinutes?: number
  description?: string
}

// ─── 产品工艺路线步骤 ──────────────────────────────────────
export interface ProductRouteStepDto {
  id: number
  routingHeaderId: number
  stepOrder: number
  processId: number
  processCode: string
  processName: string
  standardTimeMinutes?: number
  description?: string
  _isPlaceholder?: boolean
}

// ─── 产品工艺路线（单条路线） ──────────────────────────────────
export interface ProductRouteDto {
  productId: number
  productCode: string
  productName: string
  routeCode: string
  routeName: string
  routeType: RouteType
  totalSteps: number
  totalStandardTimeMinutes: number
  steps: ProductRouteStepDto[]
}
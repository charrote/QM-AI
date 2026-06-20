// ─── 通用 ────────────────────────────────────────────
export interface PagedRequest {
  page?: number
  pageSize?: number
  keyword?: string
  sortBy?: string
  sortOrder?: string
}

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}

// ─── Product ─────────────────────────────────────────
export interface Product {
  id: number
  code: string
  name: string
  description?: string
  unit?: string
  category?: string
  defaultInspectionLevel?: string
  defaultAql?: number
  isActive: boolean
  createdAt: string
}

export interface CreateProduct {
  code: string
  name: string
  description?: string
  unit?: string
  category?: string
  defaultInspectionLevel?: string
  defaultAql?: number
}

// ─── Bom ─────────────────────────────────────────────
export interface Bom {
  id: number
  productId: number
  productName: string
  materialCode: string
  materialName: string
  quantity: number
  unit?: string
  level: number
}

export interface CreateBom {
  productId: number
  materialCode: string
  materialName: string
  quantity: number
  unit?: string
  level?: number
  remark?: string
}

// ─── Process ─────────────────────────────────────────
export interface Process {
  id: number
  code: string
  name: string
  description?: string
  processType?: string
  department?: string
  isActive: boolean
}

export interface CreateProcess {
  code: string
  name: string
  description?: string
  processType?: string
  department?: string
}

// ─── Routing ─────────────────────────────────────────
export interface Routing {
  id: number
  productId: number
  productName: string
  code: string
  stepOrder: number
  processId: number
  processName: string
  standardTimeMinutes?: number
}

export interface CreateRouting {
  productId: number
  code: string
  description?: string
  stepOrder: number
  processId: number
  standardTimeMinutes?: number
}

// ─── InspectionStandard ──────────────────────────────
export interface InspectionStandard {
  id: number
  code: string
  name: string
  inspectionType: string
  productName?: string
  processName?: string
  itemName: string
  usl?: number
  lsl?: number
  unit?: string
  isActive: boolean
}

export interface CreateInspectionStandard {
  code: string
  name: string
  description?: string
  inspectionType: string
  productId?: number
  processId?: number
  itemName: string
  usl?: number
  lsl?: number
  target?: number
  unit?: string
  inspectionMethod?: string
  samplingFrequency?: string
}

// ─── DefectCode ──────────────────────────────────────
export interface DefectCode {
  id: number
  code: string
  name: string
  description?: string
  defectType?: string
  severity: string
  isReworkable: boolean
  isActive: boolean
}

export interface CreateDefectCode {
  code: string
  name: string
  description?: string
  defectType?: string
  severity?: string
  isReworkable?: boolean
}

// ─── Equipment ───────────────────────────────────────
export interface Equipment {
  id: number
  code: string
  name: string
  model?: string
  productionLine?: string
  workshop?: string
  status: string
  equipmentType?: string
  hasMqttConnection: boolean
  isActive: boolean
}

export interface CreateEquipment {
  code: string
  name: string
  model?: string
  productionLine?: string
  workshop?: string
  equipmentType?: string
  hasMqttConnection?: boolean
  mqttTopicPrefix?: string
}

// ─── Tool ────────────────────────────────────────────
export interface Tool {
  id: number
  code: string
  name: string
  model?: string
  toolType?: string
  designLife?: number
  lifeUnit?: string
  currentLife: number
  supplier?: string
  isActive: boolean
}

export interface CreateTool {
  code: string
  name: string
  model?: string
  toolType?: string
  designLife?: number
  lifeUnit?: string
  currentLife?: number
  supplier?: string
}

// ─── Supplier ────────────────────────────────────────
export interface Supplier {
  id: number
  code: string
  name: string
  address?: string
  contactPerson?: string
  contactPhone?: string
  email?: string
  grade?: string
  supplyCategory?: string
  score?: number
  isActive: boolean
}

export interface CreateSupplier {
  code: string
  name: string
  address?: string
  contactPerson?: string
  contactPhone?: string
  email?: string
  grade?: string
  supplyCategory?: string
}

// ─── Customer ────────────────────────────────────────
export interface Customer {
  id: number
  code: string
  name: string
  address?: string
  contactPerson?: string
  contactPhone?: string
  isActive: boolean
}

export interface CreateCustomer {
  code: string
  name: string
  address?: string
  contactPerson?: string
  contactPhone?: string
  email?: string
}

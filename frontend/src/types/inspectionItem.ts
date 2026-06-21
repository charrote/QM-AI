// ─── 检验项目主数据 ──────────────────────────────────────────────

export interface InspectionItem {
  id: number
  itemCode: string
  itemName: string
  dataType: string
  unit?: string
  usl?: number
  lsl?: number
  targetValue?: number
  chartType?: string
  isActive: boolean
  createdAt: string
}

export interface InspectionItemDetail extends InspectionItem {
  description?: string
  ucl?: number
  lcl?: number
  dataCollectionParamCode?: string
  subgroupSize?: number
  inspectionMethod?: string
  sampleSize?: number
  createdBy: number
  updatedAt: string
}

export interface CreateInspectionItem {
  itemCode: string
  itemName: string
  description?: string
  dataType: string
  unit?: string
  usl?: number
  lsl?: number
  targetValue?: number
  ucl?: number
  lcl?: number
  dataCollectionParamCode?: string
  chartType?: string
  subgroupSize?: number
  inspectionMethod?: string
  sampleSize?: number
}

export interface UpdateInspectionItem extends CreateInspectionItem {
  isActive: boolean
}

// ─── 检验计划 ──────────────────────────────────────────────────

export interface InspectionPlan {
  id: number
  planCode: string
  planName: string
  inspectionType: string
  productId?: number
  productName?: string
  supplierId?: number
  supplierName?: string
  customerId?: number
  customerName?: string
  processId?: number
  processName?: string
  isActive: boolean
  itemCount: number
  createdAt: string
}

export interface InspectionPlanItem {
  id: number
  inspectionItemId: number
  inspectionItemCode: string
  inspectionItemName: string
  dataType: string
  unit?: string
  sortOrder: number
  usl?: number
  lsl?: number
  targetValue?: number
  ucl?: number
  lcl?: number
  sampleSize?: number
  isRequired: boolean
}

export interface InspectionPlanDetail extends InspectionPlan {
  materialId?: number
  materialName?: string
  equipmentId?: number
  equipmentName?: string
  description?: string
  createdBy: number
  updatedAt: string
  items: InspectionPlanItem[]
}

export interface CreateInspectionPlanItem {
  inspectionItemId: number
  sortOrder: number
  usl?: number
  lsl?: number
  targetValue?: number
  ucl?: number
  lcl?: number
  sampleSize?: number
  isRequired?: boolean
}

export interface CreateInspectionPlan {
  planCode: string
  planName: string
  inspectionType: string
  description?: string
  productId?: number
  materialId?: number
  supplierId?: number
  customerId?: number
  processId?: number
  equipmentId?: number
  items: CreateInspectionPlanItem[]
}

export interface UpdateInspectionPlan {
  planName: string
  description?: string
  productId?: number
  materialId?: number
  supplierId?: number
  customerId?: number
  processId?: number
  equipmentId?: number
  isActive: boolean
  items: CreateInspectionPlanItem[]
}

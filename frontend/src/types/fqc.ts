// ─── 成品批次 ─────────────────────────────────────────
export interface ProductBatch {
  id: number
  batchCode: string
  productId: number
  productName: string
  workOrderId?: number
  quantity: number
  source: string
  status: string
  createdAt: string
}

export interface ProductBatchDetail extends ProductBatch {
  inspections?: FqcInspection[]
  releases?: OqcRelease[]
  packagingConfirmations?: PackagingConfirmation[]
}

export interface CreateProductBatch {
  batchCode?: string
  productId: number
  workOrderId?: number
  quantity: number
  source?: string
}

export interface UpdateProductBatch {
  quantity?: number
  status?: string
}

// ─── FQC 成品检验 ─────────────────────────────────────
export interface FqcInspection {
  id: number
  inspectionNo: string
  batchId: number
  batchCode?: string
  workOrderId?: number
  inspectionType: string
  aqlLevel?: number
  sampleSize: number
  totalChecked: number
  totalPass: number
  totalFail: number
  ac: number
  re: number
  conclusion: string
  inspectorName?: string
  checkedAt?: string
  createdAt: string
}

export interface FqcInspectionDetail extends FqcInspection {
  productName: string
  batchQuantity: number
  items?: FqcInspectionItem[]
}

export interface CreateFqcInspection {
  batchId: number
  workOrderId?: number
  inspectionType: string
  aqlLevel?: number
  sampleSize: number
  ac: number
  re: number
  inspectorId?: number
}

export interface SubmitFqcInspection {
  totalChecked: number
  totalPass: number
  totalFail: number
  inspectorId?: number
  items?: FqcInspectionItemSubmit[]
}

export interface FqcInspectionItemSubmit {
  id?: number
  itemName?: string
  itemCode?: string
  usl?: number
  lsl?: number
  dataType: string
  actualValue?: number
  result: string
  imageUrls?: string
}

// ─── FQC 检验项目 ─────────────────────────────────────
export interface FqcInspectionItem {
  id: number
  inspectionId: number
  itemName: string
  itemCode?: string
  usl?: number
  lsl?: number
  dataType: string
  actualValue?: number
  result: string
  imageUrls?: string
}

// ─── OQC 出货放行 ─────────────────────────────────────
export interface OqcRelease {
  id: number
  batchId: number
  batchCode?: string
  customerId: number
  customerName: string
  releaseNumber: string
  releaseDate: string
  quantity: number
  authorizedName?: string
  eSignatureUrl?: string
  signatureTime?: string
  status: string
  createdAt: string
}

export interface CreateOqcRelease {
  batchId: number
  customerId: number
  releaseNumber?: string
  releaseDate: string
  quantity: number
}

export interface SignOqcRelease {
  authorizedBy: number
  eSignatureUrl: string
}

// ─── 包装确认 ─────────────────────────────────────────
export interface PackagingConfirmation {
  id: number
  batchId: number
  batchCode?: string
  packagingMethod: string
  qtyPerBox?: number
  totalBoxes?: number
  labelPrinted: boolean
  confirmedByName: string
  confirmedAt: string
}

export interface CreatePackagingConfirmation {
  batchId: number
  packagingMethod: string
  qtyPerBox?: number
  totalBoxes?: number
  labelPrinted: boolean
  confirmedBy: number
}

// ─── 批次号生成 ─────────────────────────────────────
export interface BatchNumberGenerate {
  batchCode: string
}

// ─── 状态选项常量 ─────────────────────────────────────
export const FQC_INSPECTION_TYPE_OPTIONS = [
  { value: 'full', label: '全检', type: 'primary' },
  { value: 'sampling', label: '抽检', type: 'warning' },
]

export const FQC_CONCLUSION_OPTIONS = [
  { value: 'pending', label: '待检验', type: 'info' },
  { value: 'qualified', label: '合格', type: 'success' },
  { value: 'unqualified', label: '不合格', type: 'danger' },
]

export const FQC_CONCLUSION_MAP: Record<string, string> = {
  pending: '待检验',
  qualified: '合格',
  unqualified: '不合格',
}

export const BATCH_STATUS_OPTIONS = [
  { value: 'in_progress', label: '生产中', type: 'primary' },
  { value: 'inspected', label: '已检验', type: 'warning' },
  { value: 'released', label: '已放行', type: 'success' },
  { value: 'quarantined', label: '隔离中', type: 'danger' },
]

export const BATCH_STATUS_MAP: Record<string, string> = {
  in_progress: '生产中',
  inspected: '已检验',
  released: '已放行',
  quarantined: '隔离中',
}

export const RELEASE_STATUS_OPTIONS = [
  { value: 'pending', label: '待签名', type: 'info' },
  { value: 'signed', label: '已签名', type: 'warning' },
  { value: 'released', label: '已放行', type: 'success' },
  { value: 'cancelled', label: '已取消', type: 'danger' },
]

export const RELEASE_STATUS_MAP: Record<string, string> = {
  pending: '待签名',
  signed: '已签名',
  released: '已放行',
  cancelled: '已取消',
}

export function statusTagType(status: string, map: Record<string, string>): string {
  for (const opt of [...FQC_CONCLUSION_OPTIONS, ...BATCH_STATUS_OPTIONS, ...RELEASE_STATUS_OPTIONS]) {
    if (opt.value === status) return opt.type
  }
  return 'info'
}

export function statusLabel(status: string, map: Record<string, string>): string {
  return map[status] || status
}

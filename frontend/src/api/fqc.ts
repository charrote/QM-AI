import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  ProductBatch,
  ProductBatchDetail,
  CreateProductBatch,
  UpdateProductBatch,
  FqcInspection,
  FqcInspectionDetail,
  CreateFqcInspection,
  SubmitFqcInspection,
  OqcRelease,
  CreateOqcRelease,
  SignOqcRelease,
  PackagingConfirmation,
  CreatePackagingConfirmation,
  BatchNumberGenerate,
} from '@/types/fqc'

const BASE = '/fqc'

// ─── 批次管理 ───────────────────────────────────────
export const batchApi = {
  list(params: PagedRequest): Promise<PagedResult<ProductBatch>> {
    return request.get(`${BASE}/batches`, { params }).then(r => r.data)
  },
  get(id: number): Promise<ProductBatchDetail> {
    return request.get(`${BASE}/batches/${id}`).then(r => r.data)
  },
  create(data: CreateProductBatch): Promise<ProductBatchDetail> {
    return request.post(`${BASE}/batches`, data).then(r => r.data)
  },
  update(id: number, data: UpdateProductBatch): Promise<ProductBatchDetail> {
    return request.put(`${BASE}/batches/${id}`, data).then(r => r.data)
  },
  generateNumber(): Promise<BatchNumberGenerate> {
    return request.post(`${BASE}/batches/generate-number`).then(r => r.data)
  },
}

// ─── 成品检验 ───────────────────────────────────────
export const inspectionApi = {
  list(params: PagedRequest): Promise<PagedResult<FqcInspection>> {
    return request.get(`${BASE}/inspections`, { params }).then(r => r.data)
  },
  get(id: number): Promise<FqcInspectionDetail> {
    return request.get(`${BASE}/inspections/${id}`).then(r => r.data)
  },
  create(data: CreateFqcInspection): Promise<FqcInspectionDetail> {
    return request.post(`${BASE}/inspections`, data).then(r => r.data)
  },
  submit(id: number, data: SubmitFqcInspection): Promise<FqcInspectionDetail> {
    return request.post(`${BASE}/inspections/${id}/submit`, data).then(r => r.data)
  },
}

// ─── 出货放行 ───────────────────────────────────────
export const releaseApi = {
  list(params: PagedRequest): Promise<PagedResult<OqcRelease>> {
    return request.get(`${BASE}/releases`, { params }).then(r => r.data)
  },
  get(id: number): Promise<OqcRelease> {
    return request.get(`${BASE}/releases/${id}`).then(r => r.data)
  },
  create(data: CreateOqcRelease): Promise<OqcRelease> {
    return request.post(`${BASE}/releases`, data).then(r => r.data)
  },
  sign(id: number, data: SignOqcRelease): Promise<OqcRelease> {
    return request.post(`${BASE}/releases/${id}/sign`, data).then(r => r.data)
  },
  confirm(id: number): Promise<void> {
    return request.post(`${BASE}/releases/${id}/confirm`)
  },
}

// ─── 包装确认 ───────────────────────────────────────
export const packagingApi = {
  list(params: PagedRequest): Promise<PagedResult<PackagingConfirmation>> {
    return request.get(`${BASE}/packaging`, { params }).then(r => r.data)
  },
  create(data: CreatePackagingConfirmation): Promise<PackagingConfirmation> {
    return request.post(`${BASE}/packaging`, data).then(r => r.data)
  },
  updateLabelPrinted(id: number, printed: boolean): Promise<void> {
    return request.put(`${BASE}/packaging/${id}/label-printed`, printed)
  },
}

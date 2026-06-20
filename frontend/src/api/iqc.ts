import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  IqcReceipt,
  IqcReceiptDetail,
  CreateIqcReceipt,
  UpdateIqcReceipt,
  IqcInspection,
  IqcInspectionDetail,
  CreateIqcInspection,
  SubmitIqcInspection,
  IqcAnomaly,
  CreateIqcAnomaly,
  UpdateIqcAnomaly,
  ResolveIqcAnomaly,
  SupplierScore,
  UpdateSupplierScore,
  SamplingPlan,
  SamplingPlanRequest,
  AiRiskScore,
  BatchTrace,
} from '@/types/iqc'

// Base URL: request.ts already prefixes /api/v1, so routes are relative
const BASE = '/iqc'

// ─── 来料登记 ───────────────────────────────────────
export const receiptApi = {
  list(params: PagedRequest): Promise<PagedResult<IqcReceipt>> {
    return request.get(`${BASE}/receipts`, { params }).then(r => r.data)
  },
  get(id: number): Promise<IqcReceiptDetail> {
    return request.get(`${BASE}/receipts/${id}`).then(r => r.data)
  },
  create(data: CreateIqcReceipt): Promise<IqcReceiptDetail> {
    return request.post(`${BASE}/receipts`, data).then(r => r.data)
  },
  update(id: number, data: UpdateIqcReceipt): Promise<IqcReceiptDetail> {
    return request.put(`${BASE}/receipts/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/receipts/${id}`)
  },
}

// ─── 检验单 ───────────────────────────────────────
export const inspectionApi = {
  list(params: PagedRequest): Promise<PagedResult<IqcInspection>> {
    return request.get(`${BASE}/inspections`, { params }).then(r => r.data)
  },
  get(id: number): Promise<IqcInspectionDetail> {
    return request.get(`${BASE}/inspections/${id}`).then(r => r.data)
  },
  create(data: CreateIqcInspection): Promise<IqcInspectionDetail> {
    return request.post(`${BASE}/inspections`, data).then(r => r.data)
  },
  submit(id: number, data: SubmitIqcInspection): Promise<IqcInspectionDetail> {
    return request.post(`${BASE}/inspections/${id}/submit`, data).then(r => r.data)
  },
}

// ─── 异常单 ───────────────────────────────────────
export const anomalyApi = {
  list(params: PagedRequest): Promise<PagedResult<IqcAnomaly>> {
    return request.get(`${BASE}/anomalies`, { params }).then(r => r.data)
  },
  create(data: CreateIqcAnomaly): Promise<IqcAnomaly> {
    return request.post(`${BASE}/anomalies`, data).then(r => r.data)
  },
  update(id: number, data: UpdateIqcAnomaly): Promise<IqcAnomaly> {
    return request.put(`${BASE}/anomalies/${id}`, data).then(r => r.data)
  },
  resolve(id: number, data: ResolveIqcAnomaly): Promise<void> {
    return request.post(`${BASE}/anomalies/${id}/resolve`, data)
  },
}

// ─── 供应商评分 ─────────────────────────────────────
export const supplierScoreApi = {
  get(supplierId: number): Promise<SupplierScore> {
    return request.get(`${BASE}/suppliers/${supplierId}/score`).then(r => r.data)
  },
  update(supplierId: number, data: UpdateSupplierScore): Promise<SupplierScore> {
    return request.put(`${BASE}/suppliers/${supplierId}/score`, data).then(r => r.data)
  },
}

// ─── 抽样方案计算 ───────────────────────────────────
export const samplingPlanApi = {
  calculate(data: SamplingPlanRequest): Promise<SamplingPlan> {
    return request.post(`${BASE}/sampling-plan`, data).then(r => r.data)
  },
}

// ─── AI 风险分析 ────────────────────────────────────
export const aiRiskApi = {
  analyze(receiptId: number): Promise<AiRiskScore> {
    return request.get(`${BASE}/receipts/${receiptId}/risk-analysis`).then(r => r.data)
  },
}

// ─── 批次追溯 ─────────────────────────────────────
export const traceApi = {
  byBatch(batchNo: string): Promise<BatchTrace> {
    return request.get(`${BASE}/trace`, { params: { batchNo } }).then(r => r.data)
  },
}

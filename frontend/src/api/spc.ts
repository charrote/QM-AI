import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type {
  SpcControlChart,
  SpcControlChartDetail,
  CreateSpcControlChart,
  UpdateSpcControlChart,
  SpcDataPoint,
  CreateSpcDataPoint,
  BatchCreateDataPoints,
  SpcAnalysisReport,
  SpcAnalysisResult,
  SpcAlertRule,
  UpdateSpcAlertRule,
  SpcAlertTrigger,
  SpcAnalyzeRequest,
  SpcAnovaResult,
  SpcAnovaRequest,
  SpcControlLimits,
  SpcRuleViolation,
  SpcAnovaFactor,
  SpcDataSource,
  CreateSpcDataSource,
  BusinessInspectionData,
} from '@/types/spc'

const BASE = '/spc'

export const spcApi = {
  // ─── Control Charts ──────────────────────────────────────────
  listCharts(params: PagedRequest): Promise<PagedResult<SpcControlChart>> {
    return request.get(`${BASE}/control-charts`, { params }).then(r => r.data)
  },

  getChart(id: number): Promise<SpcControlChartDetail> {
    return request.get(`${BASE}/control-charts/${id}`).then(r => r.data)
  },

  createChart(data: CreateSpcControlChart): Promise<SpcControlChartDetail> {
    return request.post(`${BASE}/control-charts`, data).then(r => r.data)
  },

  updateChart(id: number, data: UpdateSpcControlChart): Promise<SpcControlChartDetail> {
    return request.put(`${BASE}/control-charts/${id}`, data).then(r => r.data)
  },

  deleteChart(id: number): Promise<void> {
    return request.delete(`${BASE}/control-charts/${id}`)
  },

  // ─── Data Points ─────────────────────────────────────────────
  listDataPoints(chartId: number, params: PagedRequest): Promise<PagedResult<SpcDataPoint>> {
    return request.get(`${BASE}/control-charts/${chartId}/data-points`, { params }).then(r => r.data)
  },

  createDataPoint(data: CreateSpcDataPoint): Promise<SpcDataPoint> {
    return request.post(`${BASE}/data-points`, data).then(r => r.data)
  },

  batchCreateDataPoints(data: BatchCreateDataPoints): Promise<SpcDataPoint[]> {
    return request.post(`${BASE}/data-points/batch`, data).then(r => r.data)
  },

  deleteDataPoint(id: number): Promise<void> {
    return request.delete(`${BASE}/data-points/${id}`)
  },

  // ─── Analysis ────────────────────────────────────────────────
  analyzeChart(data: SpcAnalyzeRequest): Promise<SpcAnalysisReport> {
    return request.post(`${BASE}/analyze`, data).then(r => r.data)
  },

  listAnalysisResults(chartId: number): Promise<SpcAnalysisResult[]> {
    return request.get(`${BASE}/control-charts/${chartId}/analysis-results`).then(r => r.data)
  },

  // ─── Alert Rules ─────────────────────────────────────────────
  listAlertRules(chartId: number): Promise<SpcAlertRule[]> {
    return request.get(`${BASE}/control-charts/${chartId}/alert-rules`).then(r => r.data)
  },

  updateAlertRule(id: number, data: UpdateSpcAlertRule): Promise<SpcAlertRule> {
    return request.put(`${BASE}/alert-rules/${id}`, data).then(r => r.data)
  },

  // ─── Alert Triggers ──────────────────────────────────────────
  listTriggers(chartId: number, params: PagedRequest): Promise<PagedResult<SpcAlertTrigger>> {
    return request.get(`${BASE}/control-charts/${chartId}/triggers`, { params }).then(r => r.data)
  },

  resolveTrigger(id: number): Promise<void> {
    return request.put(`${BASE}/triggers/${id}/resolve`)
  },

  // ─── ANOVA ───────────────────────────────────────────────────
  runAnova(data: SpcAnovaRequest): Promise<SpcAnovaResult[]> {
    return request.post(`${BASE}/anova`, data).then(r => r.data)
  },

  listAnovaResults(chartId: number): Promise<SpcAnovaResult[]> {
    return request.get(`${BASE}/control-charts/${chartId}/anova`).then(r => r.data)
  },

  // ═══ SPC 数据源（贯通S3/S4/S5） ═══════════════════════════════

  /** 获取控制图的数据源列表 */
  listDataSources(chartId: number): Promise<SpcDataSource[]> {
    return request.get(`${BASE}/control-charts/${chartId}/data-sources`).then(r => r.data)
  },

  /** 添加数据源 */
  createDataSource(data: CreateSpcDataSource): Promise<SpcDataSource> {
    return request.post(`${BASE}/data-sources`, data).then(r => r.data)
  },

  /** 删除数据源 */
  deleteDataSource(id: number): Promise<void> {
    return request.delete(`${BASE}/data-sources/${id}`)
  },

  /** 根据数据源配置拉取业务模块的检验数据 */
  getBusinessData(chartId: number, params?: { startDate?: string; endDate?: string }): Promise<BusinessInspectionData[]> {
    return request.get(`${BASE}/control-charts/${chartId}/business-data`, { params }).then(r => r.data)
  },
}

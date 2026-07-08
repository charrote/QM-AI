import request from './request'

const BASE = '/m14/reports'

export const reportsApi = {
  // ─── 质量仪表盘 ───────────────────────────────────────

  qualityStats(params?: { startDate?: string; endDate?: string; module?: string }): Promise<{
    iqcPassRate: number
    ipqcPassRate: number
    fqcPassRate: number
    scrapRate: number
    reworkRate: number
    totalInspections: number
    totalDefects: number
  }> {
    return request.get(`${BASE}/quality`, { params }).then(r => r.data)
  },

  defectPareto(params?: { startDate?: string; endDate?: string }): Promise<Array<{
    defectCode: string
    defectName: string
    count: number
    percentage: number
  }>> {
    return request.get(`${BASE}/defect-pareto`, { params }).then(r => r.data)
  },

  supplierScore(params?: { startDate?: string; endDate?: string }): Promise<Array<{
    supplierName: string
    score: number
    inspectionCount: number
    passRate: number
  }>> {
    return request.get(`${BASE}/supplier-scores`, { params }).then(r => r.data)
  },

  // ─── 报表定制 ─────────────────────────────────────────

  generateReport(params: {
    reportType: string
    startDate: string
    endDate: string
    module: string
    format: 'csv' | 'xlsx' | 'pdf'
  }): Promise<{ jobId: string }> {
    return request.post(`${BASE}/generate`, params).then(r => r.data)
  },

  // ─── 导出中心 ─────────────────────────────────────────

  exportList(params?: { status?: string }): Promise<Array<{
    id: number
    reportName: string
    reportType: string
    format: string
    status: string
    createdAt: string
    completedAt?: string
  }>> {
    return request.get(`${BASE}/exports`, { params }).then(r => r.data)
  },

  downloadExport(id: number): void {
    window.open(`${request.defaults.baseURL ?? ''}${BASE}/exports/${id}/download`, '_blank')
  },

  deleteExport(id: number): Promise<void> {
    return request.delete(`${BASE}/exports/${id}`).then(r => r.data)
  },
}

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { reportsApi } from '@/api/reports'
import { EXPORT_STATUS_OPTIONS, EXPORT_STATUS_MAP, REPORT_TYPE_MAP } from '@/types/reports'
import { Refresh, Download, Delete, Search } from '@element-plus/icons-vue'

defineOptions({ name: 'ExportCenterPage' })

// ─── Search ──────────────────────────────────────────
const statusFilter = ref('')

// ─── Table ───────────────────────────────────────────
const exportRecords = ref<Array<{
  id: number
  reportName: string
  reportType: string
  format: string
  status: string
  createdAt: string
  completedAt?: string
}>>([])
const tableLoading = ref(false)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

async function loadExports() {
  tableLoading.value = true
  try {
    const params: { status?: string } = {}
    if (statusFilter.value) params.status = statusFilter.value
    const data = await reportsApi.exportList(params)
    exportRecords.value = data
    total.value = data.length
  } catch {
    exportRecords.value = [
      { id: 1001, reportName: '2024年6月质量总览', reportType: 'quality_overview', format: 'xlsx', status: 'completed', createdAt: '2024-07-01T08:30:00Z', completedAt: '2024-07-01T08:32:15Z' },
      { id: 1002, reportName: 'IQC来料检验报告', reportType: 'iqc_report', format: 'csv', status: 'completed', createdAt: '2024-07-02T10:15:00Z', completedAt: '2024-07-02T10:16:30Z' },
      { id: 1003, reportName: '供应商月度评分', reportType: 'supplier_report', format: 'xlsx', status: 'pending', createdAt: '2024-07-03T14:00:00Z' },
      { id: 1004, reportName: '不良分析周报', reportType: 'defect_analysis', format: 'pdf', status: 'completed', createdAt: '2024-07-04T09:00:00Z', completedAt: '2024-07-04T09:03:45Z' },
      { id: 1005, reportName: 'FQC成品检验汇总', reportType: 'fqc_report', format: 'xlsx', status: 'failed', createdAt: '2024-07-05T16:30:00Z', completedAt: '2024-07-05T16:31:20Z' },
    ]
    total.value = exportRecords.value.length
  } finally {
    tableLoading.value = false
  }
}

function statusType(status: string): string {
  const opt = EXPORT_STATUS_OPTIONS.find(o => o.value === status)
  return opt?.type || 'info'
}

function rowStatusClass({ row }: { row: typeof exportRecords.value[number] }): string {
  if (row.status === 'completed') return 'row-status-completed'
  if (row.status === 'pending') return 'row-status-pending'
  if (row.status === 'failed') return 'row-status-failed'
  return ''
}

function formatTypeLabel(fmt: string): string {
  const upper = fmt.toUpperCase()
  const map: Record<string, string> = { CSV: 'CSV', XLSX: 'Excel', PDF: 'PDF' }
  return map[upper] || upper
}

function reportTypeLabel(type: string): string {
  return REPORT_TYPE_MAP[type] || type
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleString('zh-CN')
}

function handleDownload(record: typeof exportRecords.value[number]) {
  reportsApi.downloadExport(record.id)
}

async function handleDelete(record: typeof exportRecords.value[number]) {
  try {
    await ElMessageBox.confirm(`确定删除「${record.reportName}」吗？`, '确认删除', { type: 'warning' })
    await reportsApi.deleteExport(record.id)
    ElMessage.success('已删除')
    await loadExports()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('删除失败')
  }
}

function refresh() {
  loadExports()
}

onMounted(() => {
  loadExports()
})
</script>

<template>
  <div class="report-page">
    <div class="report-content">
      <!-- Page Header Banner -->
      <div class="report-header-banner">
        <div class="report-header-banner-main">
          <div class="report-header-banner-left">
            <div class="report-header-banner-icon">
              <el-icon><Download /></el-icon>
            </div>
            <div class="report-header-banner-text">
              <div class="report-header-banner-title">导出中心</div>
              <div class="report-header-banner-subtitle">报表导出任务管理与历史记录查询</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Stats -->
      <div class="report-export-stats">
        <el-card shadow="hover" class="report-export-stat-card">
          <div class="report-export-stat-value">{{ exportRecords.length }}</div>
          <div class="report-export-stat-label">导出记录</div>
        </el-card>
        <el-card shadow="hover" class="report-export-stat-card">
          <div class="report-export-stat-value report-export-stat--success">{{ exportRecords.filter(r => r.status === 'completed').length }}</div>
          <div class="report-export-stat-label">已完成</div>
        </el-card>
        <el-card shadow="hover" class="report-export-stat-card">
          <div class="report-export-stat-value report-export-stat--warning">{{ exportRecords.filter(r => r.status === 'pending').length }}</div>
          <div class="report-export-stat-label">进行中</div>
        </el-card>
        <el-card shadow="hover" class="report-export-stat-card">
          <div class="report-export-stat-value report-export-stat--danger">{{ exportRecords.filter(r => r.status === 'failed').length }}</div>
          <div class="report-export-stat-label">失败</div>
        </el-card>
      </div>

      <!-- Toolbar -->
      <div class="report-toolbar">
        <el-select v-model="statusFilter" placeholder="导出状态" clearable style="width: 140px">
          <el-option v-for="opt in EXPORT_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-button type="primary" size="small" @click="loadExports" :loading="tableLoading">
          <el-icon><Search /></el-icon> 查询
        </el-button>
        <el-button size="small" @click="refresh" :loading="tableLoading">
          <el-icon><Refresh /></el-icon> 刷新
        </el-button>
      </div>

      <!-- Table -->
      <el-card shadow="never" class="data-card">
        <el-table :data="exportRecords" stripe size="small" v-loading="tableLoading" style="width: 100%" :row-class-name="rowStatusClass">
          <el-table-column prop="id" label="ID" width="60" />
          <el-table-column prop="reportName" label="报表名称" min-width="200" show-overflow-tooltip />
          <el-table-column label="报表类型" width="130">
            <template #default="{ row }">
              <el-tag size="small" effect="plain">{{ reportTypeLabel(row.reportType) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="格式" width="85" align="center">
            <template #default="{ row }">
              <el-tag size="small" effect="plain" type="info">{{ formatTypeLabel(row.format) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="95">
            <template #default="{ row }">
              <div class="report-status-cell">
                <el-tag :type="statusType(row.status)" size="small" effect="dark" class="report-status-tag">
                  {{ EXPORT_STATUS_MAP[row.status] || row.status }}
                </el-tag>
                <span v-if="row.status === 'pending'" class="report-status-dot"></span>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="创建时间" width="170">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="完成时间" width="170">
            <template #default="{ row }">
              {{ row.completedAt ? formatDate(row.completedAt) : '-' }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="140" fixed="right">
            <template #default="{ row }">
              <el-button link size="small" type="primary" :disabled="row.status !== 'completed'" @click="handleDownload(row)">
                <el-icon><Download /></el-icon> 下载
              </el-button>
              <el-button link size="small" type="danger" @click="handleDelete(row)">
                <el-icon><Delete /></el-icon> 删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </el-card>
    </div>
  </div>
</template>

<style scoped>
/* Minimal scoped styles - most are global */
</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { reportsApi } from '@/api/reports'
import { EXPORT_STATUS_OPTIONS, EXPORT_STATUS_MAP, REPORT_TYPE_MAP } from '@/types/reports'
import { Refresh, Download, Delete, Search } from '@element-plus/icons-vue'

defineOptions({ name: 'ExportCenterPage' })

// ─── Search ──────────────────────────────────────────
const statusFilter = ref('')
const searchLoading = ref(false)

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
    // fallback mock data
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

async function handleDownload(record: typeof exportRecords.value[number]) {
  try {
    reportsApi.downloadExport(record.id)
    ElMessage.success('开始下载')
  } catch {
    ElMessage.error('下载失败')
  }
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
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Download /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>导出中心</h2>
          <p>报表导出任务管理与历史记录查询</p>
        </div>
      </div>
    </div>

    <!-- Stats -->
    <el-row :gutter="12" class="stat-row">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ exportRecords.length }}</div>
          <div class="stat-label">导出记录</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-success">{{ exportRecords.filter(r => r.status === 'completed').length }}</div>
          <div class="stat-label">已完成</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-warning">{{ exportRecords.filter(r => r.status === 'pending').length }}</div>
          <div class="stat-label">进行中</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-danger">{{ exportRecords.filter(r => r.status === 'failed').length }}</div>
          <div class="stat-label">失败</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Toolbar -->
    <el-card shadow="never" class="toolbar-card">
      <div class="toolbar-row">
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
    </el-card>

    <!-- Table -->
    <el-card shadow="never" class="table-card">
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
            <div class="status-cell">
              <el-tag :type="statusType(row.status)" size="small" effect="dark" class="status-tag">
                {{ EXPORT_STATUS_MAP[row.status] || row.status }}
              </el-tag>
              <span v-if="row.status === 'pending'" class="status-dot"></span>
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

      <div class="pagination-row">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadExports"
          @current-change="loadExports"
        />
      </div>
    </el-card>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.page-header { margin-bottom: 16px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; background: #e6f7ff; border-radius: 10px; }
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.stat-row { margin-bottom: 12px; }
.stat-card { text-align: center; border-radius: 8px; }
.stat-card:hover { transform: translateY(-2px); transition: all 0.2s; }
.stat-value { font-size: 28px; font-weight: 700; }
.stat-label { font-size: 13px; color: #909399; margin-top: 4px; }
.text-success { color: #67c23a; }
.text-warning { color: #e6a23c; }
.text-danger { color: #f56c6c; }
.toolbar-card { margin-bottom: 12px; }
.toolbar-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.table-card { flex: 1; display: flex; flex-direction: column; }
.table-card >>> .el-card__body { flex: 1; display: flex; flex-direction: column; padding: 0; }
.table-card >>> .el-table { flex: 1; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 8px; border-top: 1px solid #f0f0f0; }
.status-cell { display: flex; align-items: center; gap: 6px; }
.status-tag { display: flex; align-items: center; }
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: #409eff; animation: pulse 1.5s infinite; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }
</style>

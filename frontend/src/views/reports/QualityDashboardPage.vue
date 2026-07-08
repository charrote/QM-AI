<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { reportsApi } from '@/api/reports'
import { REPORT_TYPE_OPTIONS, REPORT_MODULE_OPTIONS, REPORT_TYPE_MAP } from '@/types/reports'
import { DataBoard, Odometer, TrendCharts, PieChart, DataAnalysis } from '@element-plus/icons-vue'

defineOptions({ name: 'QualityDashboardPage' })

// ─── Filters ─────────────────────────────────────────
const startDate = ref('')
const endDate = ref('')
const moduleFilter = ref('all')

function getDefaultDates() {
  const now = new Date()
  const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
  endDate.value = now.toISOString().slice(0, 10)
  startDate.value = monthAgo.toISOString().slice(0, 10)
}

// ─── Stats ───────────────────────────────────────────
const loading = ref(false)
const qualityStats = ref<{
  iqcPassRate: number
  ipqcPassRate: number
  fqcPassRate: number
  scrapRate: number
  reworkRate: number
  totalInspections: number
  totalDefects: number
} | null>(null)

async function loadQualityStats() {
  loading.value = true
  try {
    qualityStats.value = await reportsApi.qualityStats({
      startDate: startDate.value, endDate: endDate.value, module: moduleFilter.value
    })
  } catch {
    // fallback to mock data
    qualityStats.value = {
      iqcPassRate: 97.5, ipqcPassRate: 95.8, fqcPassRate: 98.2,
      scrapRate: 0.8, reworkRate: 1.5, totalInspections: 1256, totalDefects: 89
    }
  } finally {
    loading.value = false
  }
}

// ─── Pareto ──────────────────────────────────────────
const paretoData = ref<Array<{
  defectCode: string
  defectName: string
  count: number
  percentage: number
}>>([])
const paretoLoading = ref(false)

async function loadPareto() {
  paretoLoading.value = true
  try {
    paretoData.value = await reportsApi.defectPareto({
      startDate: startDate.value, endDate: endDate.value
    })
  } catch {
    // fallback mock data
    paretoData.value = [
      { defectCode: 'D001', defectName: '尺寸超差', count: 32, percentage: 36.0 },
      { defectCode: 'D003', defectName: '表面划伤', count: 18, percentage: 20.2 },
      { defectCode: 'D007', defectName: '颜色不良', count: 12, percentage: 13.5 },
      { defectCode: 'D002', defectName: '毛边', count: 10, percentage: 11.2 },
      { defectCode: 'D005', defectName: '变形', count: 7, percentage: 7.9 },
      { defectCode: 'D009', defectName: '裂纹', count: 4, percentage: 4.5 },
      { defectCode: 'D011', defectName: '装配不良', count: 3, percentage: 3.4 },
      { defectCode: 'D004', defectName: '氧化', count: 2, percentage: 2.2 },
      { defectCode: 'D008', defectName: '异色', count: 1, percentage: 1.1 },
    ]
  } finally {
    paretoLoading.value = false
  }
}

// ─── Supplier Scores ─────────────────────────────────
const supplierScores = ref<Array<{
  supplierName: string
  score: number
  inspectionCount: number
  passRate: number
}>>([])
const supplierLoading = ref(false)

async function loadSupplierScores() {
  supplierLoading.value = true
  try {
    supplierScores.value = await reportsApi.supplierScore({
      startDate: startDate.value, endDate: endDate.value
    })
  } catch {
    supplierScores.value = [
      { supplierName: '深圳精密科技', score: 95.2, inspectionCount: 48, passRate: 98.1 },
      { supplierName: '东莞材料供应', score: 91.5, inspectionCount: 36, passRate: 95.6 },
      { supplierName: '广州电子元件', score: 88.7, inspectionCount: 52, passRate: 94.2 },
      { supplierName: '佛山五金制品', score: 86.3, inspectionCount: 24, passRate: 91.7 },
      { supplierName: '惠州塑胶原料', score: 82.1, inspectionCount: 30, passRate: 89.3 },
    ]
  } finally {
    supplierLoading.value = false
  }
}

function scoreColor(score: number) {
  if (score >= 90) return 'success'
  if (score >= 80) return 'warning'
  return 'danger'
}

// ─── Date quick select ───────────────────────────────
function setLast7Days() {
  const now = new Date()
  const weekAgo = new Date(now.getTime() - 6 * 24 * 60 * 60 * 1000)
  startDate.value = weekAgo.toISOString().slice(0, 10)
  endDate.value = now.toISOString().slice(0, 10)
}

function setLast30Days() {
  getDefaultDates()
}

function setThisMonth() {
  const now = new Date()
  const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
  startDate.value = firstDay.toISOString().slice(0, 10)
  endDate.value = now.toISOString().slice(0, 10)
}

function refresh() {
  loadQualityStats()
  loadPareto()
  loadSupplierScores()
}

onMounted(() => {
  getDefaultDates()
  refresh()
})
</script>

<template>
  <div class="page-container" v-loading="loading">
    <!-- Filters -->
    <div class="toolbar-row">
      <el-date-picker
        v-model="startDate"
        type="date"
        placeholder="开始日期"
        size="small"
        style="width: 150px"
      />
      <span style="color: var(--el-text-color-secondary)">~</span>
      <el-date-picker
        v-model="endDate"
        type="date"
        placeholder="结束日期"
        size="small"
        style="width: 150px"
      />
      <el-select v-model="moduleFilter" placeholder="模块" size="small" style="width: 120px">
        <el-option v-for="opt in REPORT_MODULE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button-group>
        <el-button size="small" @click="setLast7Days">近7天</el-button>
        <el-button size="small" @click="setLast30Days">近30天</el-button>
        <el-button size="small" @click="setThisMonth">本月</el-button>
      </el-button-group>
      <el-button type="primary" size="small" @click="refresh" :loading="loading">查询</el-button>
    </div>

    <!-- Stats Cards -->
    <el-row :gutter="16" style="margin-bottom: 16px">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card" body-class="stat-card-body">
          <div class="stat-icon" style="background: #ecf5ff; color: #409eff">
            <el-icon :size="24"><Odometer /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">IQC 合格率</div>
            <div class="stat-value" :style="{ color: (qualityStats?.iqcPassRate ?? 0) >= 95 ? '#67c23a' : '#f56c6c' }">
              {{ qualityStats ? `${qualityStats.iqcPassRate}%` : '-' }}
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card" body-class="stat-card-body">
          <div class="stat-icon" style="background: #f0f9eb; color: #67c23a">
            <el-icon :size="24"><TrendCharts /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">IPQC 合格率</div>
            <div class="stat-value" :style="{ color: (qualityStats?.ipqcPassRate ?? 0) >= 95 ? '#67c23a' : '#f56c6c' }">
              {{ qualityStats ? `${qualityStats.ipqcPassRate}%` : '-' }}
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card" body-class="stat-card-body">
          <div class="stat-icon" style="background: #e8f8f0; color: #67c23a">
            <el-icon :size="24"><DataBoard /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">FQC 合格率</div>
            <div class="stat-value" :style="{ color: (qualityStats?.fqcPassRate ?? 0) >= 95 ? '#67c23a' : '#f56c6c' }">
              {{ qualityStats ? `${qualityStats.fqcPassRate}%` : '-' }}
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card" body-class="stat-card-body">
          <div class="stat-icon" style="background: #fef0f0; color: #f56c6c">
            <el-icon :size="24"><PieChart /></el-icon>
          </div>
          <div class="stat-content">
            <div class="stat-label">报废率</div>
            <div class="stat-value" :style="{ color: (qualityStats?.scrapRate ?? 0) <= 1 ? '#67c23a' : '#f56c6c' }">
              {{ qualityStats ? `${qualityStats.scrapRate}%` : '-' }}
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="16" style="margin-bottom: 16px">
      <el-col :span="8">
        <el-card shadow="hover">
          <div class="stat-inline">
            <span class="stat-inline-label">返工率</span>
            <span class="stat-inline-value">{{ qualityStats ? `${qualityStats.reworkRate}%` : '-' }}</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <div class="stat-inline">
            <span class="stat-inline-label">总检验次数</span>
            <span class="stat-inline-value">{{ qualityStats?.totalInspections ?? '-' }}</span>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card shadow="hover">
          <div class="stat-inline">
            <span class="stat-inline-label">总不良数</span>
            <span class="stat-inline-value">{{ qualityStats?.totalDefects ?? '-' }}</span>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Pareto Chart -->
    <el-card shadow="hover" style="margin-bottom: 16px">
      <template #header>
        <div class="card-header">
          <span><el-icon><DataAnalysis /></el-icon> 不良帕累托分析（Top 缺陷）</span>
          <el-button size="small" text @click="loadPareto" :loading="paretoLoading">刷新</el-button>
        </div>
      </template>
      <div v-loading="paretoLoading">
        <div class="pareto-bars">
          <div v-for="item in paretoData" :key="item.defectCode" class="pareto-bar-row">
            <div class="pareto-label" style="width:120px;flex-shrink:0">
              <span class="defect-code">{{ item.defectCode }}</span>
              <span class="defect-name">{{ item.defectName }}</span>
            </div>
            <div class="pareto-bar-track">
              <div class="pareto-bar-fill" :style="{ width: `${item.percentage}%`, background: item.percentage > 30 ? '#f56c6c' : item.percentage > 15 ? '#e6a23c' : '#409eff' }"></div>
            </div>
            <div class="pareto-values" style="width:130px;text-align:right;flex-shrink:0">
              <span class="bar-count">{{ item.count }}次</span>
              <span class="bar-pct">{{ item.percentage }}%</span>
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- Supplier Scores Table -->
    <el-card shadow="hover">
      <template #header>
        <div class="card-header">
          <span><el-icon><DataAnalysis /></el-icon> 供应商质量评分</span>
          <el-button size="small" text @click="loadSupplierScores" :loading="supplierLoading">刷新</el-button>
        </div>
      </template>
      <el-table :data="supplierScores" stripe size="small" v-loading="supplierLoading" max-height="360">
        <el-table-column prop="supplierName" label="供应商" min-width="160" />
        <el-table-column label="综合评分" width="120">
          <template #default="{ row }">
            <el-tag :type="scoreColor(row.score)" size="small" effect="plain">{{ row.score }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="检验次数" width="100" align="right">
          <template #default="{ row }">{{ row.inspectionCount }}</template>
        </el-table-column>
        <el-table-column label="合格率" width="100" align="right">
          <template #default="{ row }">
            <el-tag :type="row.passRate >= 95 ? 'success' : row.passRate >= 90 ? 'warning' : 'danger'" size="small">
              {{ row.passRate }}%
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow-y: auto;
}
.toolbar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.stat-card {
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.2s;
}
.stat-card:hover {
  transform: translateY(-2px);
}
.stat-card-body {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px !important;
}
.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.stat-content {
  flex: 1;
}
.stat-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  margin-bottom: 4px;
}
.stat-value {
  font-size: 24px;
  font-weight: 700;
  line-height: 1.2;
}
.stat-inline {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 0;
}
.stat-inline-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.stat-inline-value {
  font-size: 22px;
  font-weight: 700;
  color: var(--el-text-color-primary);
}
.pareto-bar-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 10px;
}
.pareto-bar-track {
  flex: 1;
  height: 24px;
  background: var(--el-fill-color-lighter);
  border-radius: 4px;
  overflow: hidden;
}
.pareto-bar-fill {
  height: 100%;
  border-radius: 4px;
  transition: width 0.5s ease;
}
.bar-count {
  font-size: 12px;
  color: var(--el-text-color-regular);
}
.bar-pct {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-left: 6px;
}
.defect-code {
  font-weight: 600;
  margin-right: 6px;
  font-size: 12px;
}
.defect-name {
  font-size: 13px;
  color: var(--el-text-color-regular);
}
</style>

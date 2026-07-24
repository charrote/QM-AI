<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { reportsApi } from '@/api/reports'
import { REPORT_TYPE_OPTIONS, REPORT_MODULE_OPTIONS, REPORT_TYPE_MAP } from '@/types/reports'
import { DataBoard, Odometer, TrendCharts, PieChart, DataAnalysis, Timer, Calendar, Refresh, Top, Bottom } from '@element-plus/icons-vue'

defineOptions({ name: 'QualityDashboardPage' })

// ─── Filters ─────────────────────────────────────────
const startDate = ref('')
const endDate = ref('')
const moduleFilter = ref('all')
const dateRange = ref<[string, string] | null>(null)
const quickSelect = ref('30')

const dateShortcuts = [
  { text: '近7天', value: () => { const e = new Date(); const s = new Date(e.getTime() - 6 * 24 * 60 * 60 * 1000); return [s.toISOString().slice(0, 10), e.toISOString().slice(0, 10)] } },
  { text: '近30天', value: () => { const e = new Date(); const s = new Date(e.getTime() - 29 * 24 * 60 * 60 * 1000); return [s.toISOString().slice(0, 10), e.toISOString().slice(0, 10)] } },
  { text: '本月', value: () => { const now = new Date(); return [new Date(now.getFullYear(), now.getMonth(), 1).toISOString().slice(0, 10), now.toISOString().slice(0, 10)] } },
]

function syncDateRange() {
  if (startDate.value && endDate.value) {
    dateRange.value = [startDate.value, endDate.value]
  }
}

function onDateRangeChange(val: [string, string] | null) {
  if (val) {
    startDate.value = val[0]
    endDate.value = val[1]
    quickSelect.value = ''
  } else {
    quickSelect.value = ''
  }
}

function onQuickSelectChange(val: string) {
  if (val === '7') setLast7Days()
  else if (val === '30') setLast30Days()
  else if (val === 'month') setThisMonth()
  syncDateRange()
}

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

const paretoCumulative = computed(() => {
  let cum = 0
  return paretoData.value.map(item => {
    cum += item.percentage
    return { ...item, cumulative: Math.round(cum * 10) / 10 }
  })
})

const paretoBarWidth = computed(() => {
  const maxCount = Math.max(...paretoData.value.map(i => i.count), 1)
  return (itemCount: number) => {
    const total = paretoData.value.length
    if (total <= 1) return 80
    const base = 90
    const itemShare = base / total
    return Math.max(itemShare - 2, 4)
  }
})

function getBarLeft(index: number): string {
  const total = paretoData.value.length
  if (total <= 1) return '40'
  const gap = 6
  const barW = Math.max((90 - (total - 1) * gap) / total, 4)
  return (index * (barW + gap) + barW / 2).toFixed(1)
}

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
  syncDateRange()
}

onMounted(() => {
  getDefaultDates()
  syncDateRange()
  refresh()
})
</script>

<template>
  <div class="page-container" v-loading="loading">
    <!-- Page Header -->
    <el-page-header class="page-header">
      <template #content>
        <div class="page-header-content">
          <div class="header-title-wrap">
            <el-icon class="header-icon"><DataBoard /></el-icon>
            <h1 class="header-title">质量仪表盘</h1>
            <span class="header-subtitle">全面质量数据可视化与分析</span>
          </div>
        </div>
      </template>
      <template #extra>
        <el-button type="primary" size="default" @click="refresh" :loading="loading" class="refresh-btn">
          <el-icon><TrendCharts /></el-icon> 刷新数据
        </el-button>
      </template>
    </el-page-header>

    <!-- Date Filter Bar -->
    <div class="filter-bar">
      <div class="filter-bar-left">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期"
          value-format="YYYY-MM-DD"
          size="default"
          style="width: 280px"
          :shortcuts="dateShortcuts"
          @change="onDateRangeChange"
        />
        <el-select v-model="moduleFilter" placeholder="模块筛选" style="width: 120px" clearable>
          <el-option v-for="opt in REPORT_MODULE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
      </div>
      <div class="filter-bar-right">
        <span class="quick-select-label">快速选择</span>
        <el-radio-group v-model="quickSelect" size="default" @change="onQuickSelectChange" class="quick-select-group">
          <el-radio-button value="7">
            <el-icon><Timer /></el-icon> 近7天
          </el-radio-button>
          <el-radio-button value="30">
            <el-icon><Calendar /></el-icon> 近30天
          </el-radio-button>
          <el-radio-button value="month">
            <el-icon><Calendar /></el-icon> 本月
          </el-radio-button>
        </el-radio-group>
      </div>
    </div>

    <!-- Primary Stats Row -->
    <el-row :gutter="16" class="stats-row">
      <el-col :span="6">
        <el-card shadow="never" class="stat-card stat-card-primary" body-class="stat-card-body">
          <div class="stat-card-accent" style="background: linear-gradient(135deg, #409eff, #66b1ff)"></div>
          <div class="stat-card-inner">
            <div class="stat-icon-wrap" style="background: linear-gradient(135deg, #ecf5ff, #b3d8ff); color: #409eff">
              <el-icon :size="28"><Odometer /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-label">IQC 合格率</div>
              <div class="stat-value" :class="{ 'stat-ok': (qualityStats?.iqcPassRate ?? 0) >= 95, 'stat-warn': (qualityStats?.iqcPassRate ?? 0) < 95 }">
                {{ qualityStats ? `${qualityStats.iqcPassRate}%` : '-' }}
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never" class="stat-card stat-card-primary" body-class="stat-card-body">
          <div class="stat-card-accent" style="background: linear-gradient(135deg, #67c23a, #85ce61)"></div>
          <div class="stat-card-inner">
            <div class="stat-icon-wrap" style="background: linear-gradient(135deg, #f0f9eb, #c6e7b7); color: #67c23a">
              <el-icon :size="28"><TrendCharts /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-label">IPQC 合格率</div>
              <div class="stat-value" :class="{ 'stat-ok': (qualityStats?.ipqcPassRate ?? 0) >= 95, 'stat-warn': (qualityStats?.ipqcPassRate ?? 0) < 95 }">
                {{ qualityStats ? `${qualityStats.ipqcPassRate}%` : '-' }}
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never" class="stat-card stat-card-primary" body-class="stat-card-body">
          <div class="stat-card-accent" style="background: linear-gradient(135deg, #409eff, #66b1ff)"></div>
          <div class="stat-card-inner">
            <div class="stat-icon-wrap" style="background: linear-gradient(135deg, #e8f8f0, #a8d8c0); color: #409eff">
              <el-icon :size="28"><DataAnalysis /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-label">FQC 合格率</div>
              <div class="stat-value" :class="{ 'stat-ok': (qualityStats?.fqcPassRate ?? 0) >= 95, 'stat-warn': (qualityStats?.fqcPassRate ?? 0) < 95 }">
                {{ qualityStats ? `${qualityStats.fqcPassRate}%` : '-' }}
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="never" class="stat-card stat-card-primary stat-card-danger" body-class="stat-card-body">
          <div class="stat-card-accent" style="background: linear-gradient(135deg, #f56c6c, #ff8585)"></div>
          <div class="stat-card-inner">
            <div class="stat-icon-wrap" style="background: linear-gradient(135deg, #fef0f0, #fbbcb7); color: #f56c6c">
              <el-icon :size="28"><PieChart /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-label">报废率</div>
              <div class="stat-value" :class="{ 'stat-ok': (qualityStats?.scrapRate ?? 0) <= 1, 'stat-warn': (qualityStats?.scrapRate ?? 0) > 1 }">
                {{ qualityStats ? `${qualityStats.scrapRate}%` : '-' }}
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Secondary Stats Row -->
    <div class="secondary-stats">
      <div class="secondary-stat-item" :style="{ '--accent': '#e6a23c' }">
        <div class="sec-icon" style="background: linear-gradient(135deg, #fdf6ec, #faecd8); color: #e6a23c">
          <el-icon :size="18"><TrendCharts /></el-icon>
        </div>
        <div class="sec-info">
          <div class="sec-label">返工率</div>
          <div class="sec-value">{{ qualityStats ? `${qualityStats.reworkRate}%` : '-' }}</div>
        </div>
      </div>
      <div class="secondary-stat-item" :style="{ '--accent': '#409eff' }">
        <div class="sec-icon" style="background: linear-gradient(135deg, #ecf5ff, #b3d8ff); color: #409eff">
          <el-icon :size="18"><DataAnalysis /></el-icon>
        </div>
        <div class="sec-info">
          <div class="sec-label">总检验次数</div>
          <div class="sec-value">{{ qualityStats?.totalInspections ?? '-' }}</div>
        </div>
      </div>
      <div class="secondary-stat-item" :style="{ '--accent': '#f56c6c' }">
        <div class="sec-icon" style="background: linear-gradient(135deg, #fef0f0, #fbbcb7); color: #f56c6c">
          <el-icon :size="18"><PieChart /></el-icon>
        </div>
        <div class="sec-info">
          <div class="sec-label">总不良数</div>
          <div class="sec-value">{{ qualityStats?.totalDefects ?? '-' }}</div>
        </div>
      </div>
    </div>

    <!-- Pareto Chart -->
    <el-card shadow="never" class="chart-card" style="margin-bottom: 16px">
      <template #header>
        <div class="card-header">
          <div class="card-header-left">
            <el-icon class="card-header-icon"><DataAnalysis /></el-icon>
            <span class="card-header-title">不良帕累托分析（Top 缺陷）</span>
          </div>
          <el-button size="small" text @click="loadPareto" :loading="paretoLoading">
            <el-icon><Refresh /></el-icon> 刷新
          </el-button>
        </div>
      </template>
      <div v-loading="paretoLoading">
        <div class="pareto-container">
          <div class="pareto-chart-area">
            <div class="pareto-threshold">
              <div class="pareto-threshold-line">
                <span class="pareto-threshold-label">80% 帕累托阈值</span>
              </div>
            </div>
            <div class="pareto-bars">
              <div v-for="(item, index) in paretoCumulative" :key="item.defectCode" class="pareto-bar-row">
                <div class="pareto-label">
                  <span class="defect-code">{{ item.defectCode }}</span>
                  <span class="defect-name">{{ item.defectName }}</span>
                </div>
                <div class="pareto-bar-wrapper">
                  <div class="pareto-bar-track">
                    <div
                      class="pareto-bar-fill"
                      :class="{ 'bar-critical': item.percentage > 30, 'bar-major': item.percentage > 15, 'bar-normal': item.percentage <= 15 }"
                      :style="{ width: `${item.percentage}%` }"
                    ></div>
                  </div>
                </div>
                <div class="pareto-values">
                  <span class="bar-count">{{ item.count }}次</span>
                  <span class="bar-pct">{{ item.percentage }}%</span>
                </div>
              </div>
            </div>
            <div class="pareto-cumulative-row">
              <div
                v-for="(item, index) in paretoCumulative"
                :key="'cum-' + item.defectCode"
                class="pareto-cum-point"
                :style="{ left: `${getBarLeft(index)}%` }"
                :title="`累计: ${item.cumulative}%`"
              >
                <span class="pareto-cum-label">{{ item.cumulative }}%</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-card>

    <!-- Supplier Scores Table -->
    <el-card shadow="never" class="table-card">
      <template #header>
        <div class="card-header">
          <div class="card-header-left">
            <el-icon class="card-header-icon"><DataBoard /></el-icon>
            <span class="card-header-title">供应商质量评分</span>
          </div>
          <el-button size="small" text @click="loadSupplierScores" :loading="supplierLoading">
            <el-icon><Refresh /></el-icon> 刷新
          </el-button>
        </div>
      </template>
      <el-table :data="supplierScores" stripe v-loading="supplierLoading" max-height="360" class="supplier-table">
        <el-table-column prop="supplierName" label="供应商" min-width="160" header-align="center" align="left">
          <template #default="{ row }">
            <div class="supplier-name-cell">
              <div class="supplier-avatar">{{ row.supplierName.charAt(0) }}</div>
              <span class="supplier-text">{{ row.supplierName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="综合评分" width="180" header-align="center" align="center">
          <template #default="{ row }">
            <div class="score-cell">
              <el-progress
                :percentage="Math.round(row.score)"
                :color="scoreColor(row.score) === 'success' ? '#67c23a' : scoreColor(row.score) === 'warning' ? '#e6a23c' : '#f56c6c'"
                :stroke-width="8"
                :text-inside="false"
                :show-text="false"
                style="width: 120px"
              />
              <span class="score-number" :class="scoreColor(row.score)">{{ row.score }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="检验次数" width="100" header-align="center" align="right">
          <template #default="{ row }">
            <span class="num-cell">{{ row.inspectionCount }}</span>
          </template>
        </el-table-column>
        <el-table-column label="合格率" width="140" header-align="center" align="center">
          <template #default="{ row }">
            <div class="pass-rate-cell">
              <el-tag
                :type="row.passRate >= 95 ? 'success' : row.passRate >= 90 ? 'warning' : 'danger'"
                size="small"
                :effect="row.passRate >= 95 ? 'dark' : 'plain'"
                round
              >
                {{ row.passRate }}%
              </el-tag>
              <el-icon :size="12" :color="row.passRate >= 95 ? '#67c23a' : '#f56c6c'" class="pass-trend">
                <component :is="row.passRate >= 93 ? 'Top' : 'Bottom'" />
              </el-icon>
            </div>
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
  background: var(--el-bg-color-page);
}

/* ─── Page Header ─── */
.page-header {
  margin-bottom: 16px;
}
.header-title-wrap {
  display: flex;
  align-items: baseline;
  gap: 12px;
}
.header-icon {
  font-size: 22px;
  color: var(--el-color-primary);
}
.header-title {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  letter-spacing: 1px;
}
.header-subtitle {
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.refresh-btn {
  border-radius: 20px;
  padding: 8px 20px;
}

/* ─── Filter Bar ─── */
.filter-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 16px;
  padding: 12px 16px;
  background: var(--el-bg-color);
  border-radius: 10px;
  border: 1px solid var(--el-border-color-lighter);
}
.filter-bar-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}
.filter-bar-right {
  display: flex;
  align-items: center;
  gap: 10px;
}
.quick-select-label {
  font-size: 13px;
  color: var(--el-text-color-regular);
  font-weight: 500;
}
.quick-select-group {
  display: flex;
  gap: 0;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid var(--el-border-color-light);
}
.quick-select-group :deep(.el-radio-button__inner) {
  padding: 6px 16px;
  font-size: 13px;
  border-radius: 0;
  gap: 4px;
}
.quick-select-group :deep(.el-radio-button__inner .el-icon) {
  font-size: 14px;
}

/* ─── Primary Stats Cards ─── */
.stats-row {
  margin-bottom: 12px;
}
.stat-card {
  border-radius: 12px;
  border: 1px solid var(--el-border-color-lighter);
  overflow: hidden;
  transition: all 0.25s ease;
  position: relative;
}
.stat-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
}
.stat-card-accent {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 3px;
}
.stat-card-body {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px 20px !important;
}
.stat-card-inner {
  display: flex;
  align-items: center;
  gap: 16px;
  width: 100%;
}
.stat-icon-wrap {
  width: 56px;
  height: 56px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}
.stat-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.stat-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}
.stat-value {
  font-size: 28px;
  font-weight: 800;
  line-height: 1.1;
  letter-spacing: -0.5px;
  font-variant-numeric: tabular-nums;
}
.stat-ok {
  color: #67c23a;
}
.stat-warn {
  color: #f56c6c;
}

/* ─── Secondary Stats ─── */
.secondary-stats {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}
.secondary-stat-item {
  flex: 1;
  min-width: 160px;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 10px;
  border-left: 3px solid var(--accent, #409eff);
  transition: all 0.2s ease;
}
.secondary-stat-item:hover {
  border-left-color: var(--accent, #409eff);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.04);
}
.sec-icon {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.sec-info {
  display: flex;
  flex-direction: column;
}
.sec-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}
.sec-value {
  font-size: 20px;
  font-weight: 700;
  color: var(--accent, var(--el-text-color-primary));
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}

/* ─── Chart Card ─── */
.chart-card,
.table-card {
  border-radius: 12px;
  border: 1px solid var(--el-border-color-lighter);
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.card-header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}
.card-header-icon {
  font-size: 18px;
  color: var(--el-color-primary);
}
.card-header-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

/* ─── Pareto ─── */
.pareto-container {
  padding: 8px 0;
}
.pareto-chart-area {
  position: relative;
}
.pareto-threshold {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  pointer-events: none;
  z-index: 2;
}
.pareto-threshold-line {
  border-top: 2px dashed #e6a23c;
  position: relative;
}
.pareto-threshold-label {
  position: absolute;
  top: -20px;
  right: 0;
  font-size: 11px;
  color: #e6a23c;
  font-weight: 600;
  background: var(--el-bg-color);
  padding: 0 6px;
}
.pareto-bars {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding-top: 20px;
}
.pareto-bar-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
.pareto-label {
  width: 130px;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}
.pareto-bar-wrapper {
  flex: 1;
  min-width: 0;
}
.pareto-bar-track {
  height: 28px;
  background: var(--el-fill-color-lighter);
  border-radius: 6px;
  overflow: hidden;
  position: relative;
}
.pareto-bar-fill {
  height: 100%;
  border-radius: 6px;
  transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
}
.pareto-bar-fill::after {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(180deg, rgba(255,255,255,0.15) 0%, transparent 60%);
  border-radius: 6px;
}
.pareto-bar-fill.bar-critical {
  background: linear-gradient(90deg, #f56c6c, #ff8585);
}
.pareto-bar-fill.bar-major {
  background: linear-gradient(90deg, #e6a23c, #ebb563);
}
.pareto-bar-fill.bar-normal {
  background: linear-gradient(90deg, #409eff, #66b1ff);
}
.pareto-values {
  width: 100px;
  flex-shrink: 0;
  text-align: right;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 1px;
}
.bar-count {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}
.bar-pct {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.defect-code {
  font-weight: 700;
  font-size: 12px;
  color: var(--el-text-color-primary);
}
.defect-name {
  font-size: 12px;
  color: var(--el-text-color-regular);
}
.pareto-cumulative-row {
  position: relative;
  height: 24px;
  margin-top: 4px;
}
.pareto-cum-point {
  position: absolute;
  transform: translateX(-50%);
  z-index: 3;
}
.pareto-cum-label {
  font-size: 11px;
  font-weight: 700;
  color: #909399;
  background: var(--el-bg-color-page);
  padding: 1px 4px;
  border-radius: 3px;
  white-space: nowrap;
}

/* ─── Supplier Table ─── */
.supplier-table {
  --el-table-border-color: var(--el-border-color-lighter);
}
.supplier-name-cell {
  display: flex;
  align-items: center;
  gap: 10px;
}
.supplier-avatar {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: linear-gradient(135deg, var(--el-color-primary), var(--el-color-primary-light-3));
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 700;
  flex-shrink: 0;
}
.supplier-text {
  font-weight: 500;
  font-size: 14px;
}
.score-cell {
  display: flex;
  align-items: center;
  gap: 8px;
  justify-content: center;
}
.score-number {
  font-weight: 700;
  font-size: 15px;
  white-space: nowrap;
  font-variant-numeric: tabular-nums;
}
.score-number.success { color: #67c23a; }
.score-number.warning { color: #e6a23c; }
.score-number.danger { color: #f56c6c; }
.num-cell {
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  font-size: 14px;
}
.pass-rate-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}
.pass-trend {
  flex-shrink: 0;
}
</style>

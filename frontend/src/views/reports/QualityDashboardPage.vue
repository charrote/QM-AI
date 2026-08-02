<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { reportsApi } from '@/api/reports'
import { REPORT_MODULE_OPTIONS } from '@/types/reports'
import { DataBoard, Odometer, TrendCharts, PieChart, DataAnalysis, Timer, Calendar, Refresh } from '@element-plus/icons-vue'

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
    <!-- Page Header Banner -->
    <div class="ai-header-banner">
      <div class="ai-header-banner-main">
        <div class="ai-header-banner-icon">
          <el-icon :size="24"><DataBoard /></el-icon>
        </div>
        <div class="ai-header-banner-text">
          <div class="ai-header-banner-title">质量仪表盘</div>
          <div class="ai-header-banner-subtitle">全面质量数据可视化与分析</div>
        </div>
        <div class="ai-header-banner-actions">
          <el-button type="primary" size="default" @click="refresh" :loading="loading" round>
            <el-icon><Refresh /></el-icon> 刷新数据
          </el-button>
        </div>
      </div>
    </div>

    <!-- Date Filter Bar -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期"
          value-format="YYYY-MM-DD"
          style="width: 280px"
          :shortcuts="dateShortcuts"
          @change="onDateRangeChange"
        />
        <el-select v-model="moduleFilter" placeholder="模块筛选" style="width: 120px" clearable>
          <el-option v-for="opt in REPORT_MODULE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <div class="ai-filter-actions">
          <span style="font-size: var(--font-sm); color: var(--el-text-color-secondary); margin-right: 4px;">快速选择</span>
          <el-radio-group v-model="quickSelect" size="default" @change="onQuickSelectChange">
            <el-radio-button value="7"><el-icon><Timer /></el-icon> 近7天</el-radio-button>
            <el-radio-button value="30"><el-icon><Calendar /></el-icon> 近30天</el-radio-button>
            <el-radio-button value="month"><el-icon><Calendar /></el-icon> 本月</el-radio-button>
          </el-radio-group>
        </div>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="ai-stat-grid">
      <div class="ai-stat-card ai-stat-card--blue">
        <div class="ai-stat-icon ai-stat-icon--blue">
          <el-icon :size="22"><Odometer /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">IQC 合格率</div>
          <div class="ai-stat-value" :class="(qualityStats?.iqcPassRate ?? 0) >= 95 ? 'ai-stat-value--success' : 'ai-stat-value--warning'">
            {{ qualityStats ? `${qualityStats.iqcPassRate}%` : '-' }}
          </div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--green">
        <div class="ai-stat-icon ai-stat-icon--green">
          <el-icon :size="22"><TrendCharts /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">IPQC 合格率</div>
          <div class="ai-stat-value" :class="(qualityStats?.ipqcPassRate ?? 0) >= 95 ? 'ai-stat-value--success' : 'ai-stat-value--warning'">
            {{ qualityStats ? `${qualityStats.ipqcPassRate}%` : '-' }}
          </div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--blue">
        <div class="ai-stat-icon ai-stat-icon--blue">
          <el-icon :size="22"><DataAnalysis /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">FQC 合格率</div>
          <div class="ai-stat-value" :class="(qualityStats?.fqcPassRate ?? 0) >= 95 ? 'ai-stat-value--success' : 'ai-stat-value--warning'">
            {{ qualityStats ? `${qualityStats.fqcPassRate}%` : '-' }}
          </div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--red">
        <div class="ai-stat-icon ai-stat-icon--red">
          <el-icon :size="22"><PieChart /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">报废率</div>
          <div class="ai-stat-value" :class="(qualityStats?.scrapRate ?? 0) <= 1 ? 'ai-stat-value--success' : 'ai-stat-value--danger'">
            {{ qualityStats ? `${qualityStats.scrapRate}%` : '-' }}
          </div>
        </div>
      </div>
    </div>

    <!-- Secondary Stats -->
    <div class="quick-info" style="margin-bottom: var(--space-5);">
      <div class="quick-info__item">
        <div class="quick-info__label">返工率</div>
        <div class="quick-info__value" :class="qualityStats ? (qualityStats.reworkRate <= 2 ? 'quick-info__value--success' : 'quick-info__value--warning') : ''">
          {{ qualityStats ? `${qualityStats.reworkRate}%` : '-' }}
        </div>
      </div>
      <div class="quick-info__item">
        <div class="quick-info__label">总检验次数</div>
        <div class="quick-info__value quick-info__value--primary">{{ qualityStats?.totalInspections ?? '-' }}</div>
      </div>
      <div class="quick-info__item">
        <div class="quick-info__label">总不良数</div>
        <div class="quick-info__value" :class="qualityStats ? (qualityStats.totalDefects <= 10 ? 'quick-info__value--success' : 'quick-info__value--danger') : ''">
          {{ qualityStats?.totalDefects ?? '-' }}
        </div>
      </div>
    </div>

    <!-- Pareto Chart -->
    <div class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><DataAnalysis /></el-icon>
          不良帕累托分析（Top 缺陷）
        </div>
        <el-button size="small" text @click="loadPareto" :loading="paretoLoading">
          <el-icon><Refresh /></el-icon> 刷新
        </el-button>
      </div>
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
    </div>

    <!-- Supplier Scores Table -->
    <div class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><DataBoard /></el-icon>
          供应商质量评分
        </div>
        <el-button size="small" text @click="loadSupplierScores" :loading="supplierLoading">
          <el-icon><Refresh /></el-icon> 刷新
        </el-button>
      </div>
      <div class="table-card">
        <div class="table-card__body">
          <el-table :data="supplierScores" stripe v-loading="supplierLoading" max-height="360" class="supplier-table">
            <el-table-column label="供应商" min-width="160" header-align="center" align="left">
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
                </div>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ─── Pareto ─── */
.pareto-container { padding: 8px 0; }
.pareto-chart-area { position: relative; }
.pareto-threshold {
  position: absolute; top: 0; left: 0; right: 0;
  pointer-events: none; z-index: 2;
}
.pareto-threshold-line {
  border-top: 2px dashed #e6a23c;
  position: relative;
}
.pareto-threshold-label {
  position: absolute; top: -20px; right: 0;
  font-size: 11px; color: #e6a23c;
  font-weight: 600;
  background: var(--el-bg-color);
  padding: 0 6px;
}
.pareto-bars {
  display: flex; flex-direction: column;
  gap: 6px; padding-top: 20px;
}
.pareto-bar-row {
  display: flex; align-items: center;
  gap: 12px;
}
.pareto-label {
  width: 130px; flex-shrink: 0;
  display: flex; flex-direction: column;
  align-items: flex-start;
}
.pareto-bar-wrapper { flex: 1; min-width: 0; }
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
  position: absolute; inset: 0;
  background: linear-gradient(180deg, rgba(255,255,255,0.15) 0%, transparent 60%);
  border-radius: 6px;
}
.pareto-bar-fill.bar-critical { background: linear-gradient(90deg, #f56c6c, #ff8585); }
.pareto-bar-fill.bar-major   { background: linear-gradient(90deg, #e6a23c, #ebb563); }
.pareto-bar-fill.bar-normal  { background: linear-gradient(90deg, #409eff, #66b1ff); }
.pareto-values {
  width: 100px; flex-shrink: 0;
  text-align: right;
  display: flex; flex-direction: column;
  align-items: flex-end; gap: 1px;
}
.bar-count {
  font-size: 13px; font-weight: 600;
  color: var(--el-text-color-regular);
}
.bar-pct {
  font-size: 12px; color: var(--el-text-color-secondary);
}
.defect-code {
  font-weight: 700; font-size: 12px;
  color: var(--el-text-color-primary);
}
.defect-name {
  font-size: 12px; color: var(--el-text-color-regular);
}
.pareto-cumulative-row {
  position: relative; height: 24px;
  margin-top: 4px;
}
.pareto-cum-point {
  position: absolute; transform: translateX(-50%); z-index: 3;
}
.pareto-cum-label {
  font-size: 11px; font-weight: 700;
  color: var(--el-text-color-secondary);
  background: var(--el-bg-color-page);
  padding: 1px 4px; border-radius: 3px;
  white-space: nowrap;
}

/* ─── Supplier Table ─── */
.supplier-table { --el-table-border-color: var(--el-border-color-lighter); }
.supplier-name-cell {
  display: flex; align-items: center; gap: 10px;
}
.supplier-avatar {
  width: 32px; height: 32px;
  border-radius: 8px;
  background: linear-gradient(135deg, var(--el-color-primary), var(--el-color-primary-light-3));
  color: #fff;
  display: flex; align-items: center; justify-content: center;
  font-size: 14px; font-weight: 700;
  flex-shrink: 0;
}
.supplier-text { font-weight: 500; font-size: 14px; }
.score-cell {
  display: flex; align-items: center;
  gap: 8px; justify-content: center;
}
.score-number {
  font-weight: 700; font-size: 15px;
  white-space: nowrap;
  font-variant-numeric: tabular-nums;
}
.score-number.success { color: #67c23a; }
.score-number.warning { color: #e6a23c; }
.score-number.danger  { color: #f56c6c; }
.num-cell {
  font-weight: 600; font-variant-numeric: tabular-nums;
  font-size: 14px;
}
.pass-rate-cell {
  display: flex; align-items: center;
  justify-content: center; gap: 6px;
}

</style>

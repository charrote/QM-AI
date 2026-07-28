<script setup lang="ts">
import { ref, onMounted, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import {
  DataAnalysis,
  WarningFilled,
  CircleCheckFilled,
  TrendCharts,
  Monitor,
  Setting,
  Refresh,
  Document,
  ArrowUp,
  ArrowDown,
  Remove,
} from '@element-plus/icons-vue'
import { riskScoreApi } from '@/api/ipqc'
import { equipmentApi, processApi } from '@/api/basicData'
import type { IpqcAiRiskScore, RiskFactor } from '@/types/ipqc'
import { IPQC_RISK_LEVEL_OPTIONS, IPQC_TREND_OPTIONS } from '@/types/ipqc'
import type { Equipment, Process } from '@/types/basicData'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
} from 'echarts/components'

use([
  CanvasRenderer,
  LineChart,
  GridComponent,
  TooltipComponent,
  LegendComponent,
])

defineOptions({ name: 'IpqcRiskDashboard' })

// ─── State ──────────────────────────────────────
const equipmentList = ref<Equipment[]>([])
const processList = ref<Process[]>([])
const selectedEquipmentId = ref(0)
const selectedProcessId = ref(0)

const riskScore = ref<IpqcAiRiskScore | null>(null)
const scoreHistory = ref<IpqcAiRiskScore[]>([])
const loading = ref(false)
const historyLoading = ref(false)
const chartReady = ref(false)

// ─── Load Data ─────────────────────────────────
async function loadEquipment() {
  try {
    const res = await equipmentApi.list({ pageSize: 100 })
    equipmentList.value = res.items
    if (res.items.length > 0) selectedEquipmentId.value = res.items[0].id
  } catch { /* ignore */ }
}

async function loadProcesses() {
  try {
    const res = await processApi.list({ pageSize: 100 })
    processList.value = res.items
    if (res.items.length > 0) selectedProcessId.value = res.items[0].id
  } catch { /* ignore */ }
}

async function analyzeRisk() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) {
    ElMessage.warning('请选择设备和工序')
    return
  }
  loading.value = true
  try {
    const result = await riskScoreApi.analyze(selectedEquipmentId.value, selectedProcessId.value)
    riskScore.value = result
    await nextTick()
    chartReady.value = true
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '分析失败')
  } finally {
    loading.value = false
  }
}

async function loadHistory() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) return
  historyLoading.value = true
  try {
    const history = await riskScoreApi.history(selectedEquipmentId.value, selectedProcessId.value, 24)
    scoreHistory.value = history
    await nextTick()
    chartReady.value = true
  } catch {
    // silent
  } finally {
    historyLoading.value = false
  }
}

async function loadLatest() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) return
  try {
    const latest = await riskScoreApi.latest(selectedEquipmentId.value, selectedProcessId.value)
    if (latest) riskScore.value = latest
  } catch {
    // no data yet
  }
}

function levelTag(level: string) {
  const opt = IPQC_RISK_LEVEL_OPTIONS.find(o => o.value === level)
  return opt?.type || 'info'
}

function levelLabel(level: string) {
  const opt = IPQC_RISK_LEVEL_OPTIONS.find(o => o.value === level)
  return opt?.label || level
}

// ─── ECharts Trend Chart ───────────────────────
const trendChartOption = ref<any>(null)

watch(() => scoreHistory.value, () => {
  if (!scoreHistory.value.length) {
    trendChartOption.value = null
    return
  }
  const sorted = [...scoreHistory.value].sort((a, b) =>
    new Date(a.lastUpdated!).getTime() - new Date(b.lastUpdated!).getTime()
  )
  const xData = sorted.map(s => {
    const d = new Date(s.lastUpdated!)
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
  })
  const yData = sorted.map(s => s.score)

  const lineColor = sorted[sorted.length - 1].score >= sorted[0].score ? '#ff4d4f' : '#52c41a'

  trendChartOption.value = {
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const p = params[0]
        return `<strong>${p.name}</strong><br/>风险评分: <strong>${p.value}</strong>`
      },
    },
    grid: {
      left: 40,
      right: 16,
      top: 16,
      bottom: 24,
    },
    xAxis: {
      type: 'category',
      data: xData,
      axisLabel: { fontSize: 10, color: '#8c8c8c' },
      axisLine: { lineStyle: { color: '#e8e8e8' } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value',
      min: 0,
      max: 100,
      axisLabel: { fontSize: 10, color: '#8c8c8c' },
      splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } },
      axisLine: { show: false },
      axisTick: { show: false },
    },
    series: [{
      data: yData,
      type: 'line',
      smooth: true,
      symbol: 'circle',
      symbolSize: 6,
      lineStyle: { width: 2, color: lineColor },
      itemStyle: { color: lineColor },
      areaStyle: {
        color: {
          type: 'linear',
          x: 0, y: 0, x2: 0, y2: 1,
          colorStops: [
            { offset: 0, color: lineColor + '30' },
            { offset: 1, color: lineColor + '05' },
          ],
        },
      },
    }],
  }
}, { deep: true })

onMounted(async () => {
  await Promise.all([loadEquipment(), loadProcesses()])
  await loadHistory()
})
</script>

<template>
  <div class="page-content">
    <!-- Page Header Banner -->
    <div class="page-header-banner page-header-banner--danger">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="24"><DataAnalysis /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h1 class="page-header-banner-title">AI 风险驾驶舱</h1>
          <p class="page-header-banner-subtitle">基于实时数据智能评估生产质量风险等级</p>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="action-bar">
      <el-select
        v-model="selectedEquipmentId"
        filterable
        placeholder="选择设备"
        :prefix-icon="Monitor"
        style="width: 220px"
      >
        <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
      </el-select>
      <el-select
        v-model="selectedProcessId"
        filterable
        placeholder="选择工序"
        :prefix-icon="Setting"
        style="width: 220px"
      >
        <el-option v-for="p in processList" :key="p.id" :label="p.name" :value="p.id" />
      </el-select>
      <el-button type="primary" @click="analyzeRisk" :loading="loading">
        <el-icon><DataAnalysis /></el-icon>
        分析风险
      </el-button>
      <el-button @click="loadLatest">
        <el-icon><Refresh /></el-icon>
        最新评分
      </el-button>
    </div>

    <!-- Risk Score Card -->
    <div v-if="riskScore" class="data-card" style="padding: 20px;">
      <!-- Stat Grid -->
      <div class="stat-grid" style="margin-bottom: 20px;">
        <div class="stat-grid__item" :class="'stat-grid__item--' + (riskScore.score >= 70 ? 'danger' : riskScore.score >= 40 ? 'warning' : 'success')">
          <div class="stat-grid__icon" :class="'stat-grid__icon--' + (riskScore.score >= 70 ? 'danger' : riskScore.score >= 40 ? 'warning' : 'success')">
            <el-icon :size="20">
              <WarningFilled v-if="riskScore.score >= 70" />
              <WarningFilled v-else-if="riskScore.score >= 40" />
              <CircleCheckFilled v-else />
            </el-icon>
          </div>
          <div class="stat-grid__text">
            <div class="stat-grid__label">风险评分</div>
            <div class="stat-grid__value">{{ riskScore.score }}</div>
          </div>
        </div>

        <div class="stat-grid__item">
          <div class="stat-grid__text">
            <div class="stat-grid__label">风险等级</div>
            <div class="stat-grid__value">
              <el-tag :type="levelTag(riskScore.level)" size="large" effect="dark">{{ levelLabel(riskScore.level) }}</el-tag>
            </div>
          </div>
        </div>

        <div class="stat-grid__item">
          <div class="stat-grid__text">
            <div class="stat-grid__label">趋势走向</div>
            <div class="stat-grid__value" style="font-size: 16px;">
              <el-icon :size="16" :style="{ color: riskScore.trend === 'rising' ? '#ff4d4f' : riskScore.trend === 'falling' ? '#52c41a' : '#8c8c8c' }">
                <ArrowUp v-if="riskScore.trend === 'rising'" />
                <ArrowDown v-else-if="riskScore.trend === 'falling'" />
                <Remove v-else />
              </el-icon>
              {{ riskScore.trend === 'rising' ? '上升' : riskScore.trend === 'falling' ? '下降' : '稳定' }}
            </div>
          </div>
        </div>

        <div class="stat-grid__item">
          <div class="stat-grid__text">
            <div class="stat-grid__label">最后更新</div>
            <div v-if="riskScore.lastUpdated" style="font-size: 13px; font-weight: 400; color: var(--el-text-color-regular);">
              {{ new Date(riskScore.lastUpdated).toLocaleString('zh-CN') }}
            </div>
            <div v-else style="font-size: 13px; color: var(--el-text-color-placeholder);">暂无数据</div>
          </div>
        </div>
      </div>

      <!-- ECharts Trend Chart -->
      <div v-if="scoreHistory.length > 0" class="section" style="margin-bottom: 20px;">
        <div class="section__title">
          <el-icon><TrendCharts /></el-icon>
          风险评分趋势（近 24 小时）
        </div>
        <v-chart :option="trendChartOption" autoresize style="height: 280px;" />
      </div>

      <!-- Recommendations -->
      <div class="section" v-if="riskScore.recommendations && riskScore.recommendations.length > 0" style="margin-bottom: 20px;">
        <div class="section__title">
          <el-icon><Document /></el-icon>
          建议措施
        </div>
        <el-timeline>
          <el-timeline-item
            v-for="(rec, idx) in riskScore.recommendations"
            :key="idx"
            :timestamp="'建议 ' + (idx + 1)"
            size="small"
            placement="top"
          >
            {{ rec }}
          </el-timeline-item>
        </el-timeline>
      </div>

      <!-- Risk Factors -->
      <div class="section" v-if="riskScore.factors && riskScore.factors.length > 0">
        <div class="section__title">
          <el-icon><WarningFilled /></el-icon>
          风险因素
        </div>
        <el-table :data="riskScore.factors" stripe>
          <el-table-column prop="name" label="因素" min-width="140" show-overflow-tooltip />
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column label="当前值" width="100" align="center">
            <template #default="{ row }">
              <span v-if="row.currentValue != null">{{ row.currentValue.toFixed(1) }}</span>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="目标值" width="100" align="center">
            <template #default="{ row }">
              <span v-if="row.targetValue != null">{{ row.targetValue.toFixed(1) }}</span>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="影响" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="row.impact > 15 ? 'danger' : row.impact > 5 ? 'warning' : 'info'" size="small" effect="dark">
                +{{ row.impact }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <el-icon class="empty-state__icon" :size="64"><DataAnalysis /></el-icon>
      <p class="empty-state__title">请选择设备和工序</p>
      <p class="empty-state__desc">选择后点击"分析风险"开始智能评估</p>
    </div>
  </div>
</template>

<style scoped>
</style>

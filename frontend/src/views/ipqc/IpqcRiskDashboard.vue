<script setup lang="ts">
import { ref, onMounted, nextTick, watch } from 'vue'
import { ElMessage } from 'element-plus'
import {
  DataAnalysis,
  WarningFilled,
  Document,
  Refresh,
  Monitor,
  Setting,
  TrendCharts,
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

const equipmentList = ref<Equipment[]>([])
const processList = ref<Process[]>([])
const selectedEquipmentId = ref(0)
const selectedProcessId = ref(0)

const riskScore = ref<IpqcAiRiskScore | null>(null)
const scoreHistory = ref<IpqcAiRiskScore[]>([])
const loading = ref(false)
const historyLoading = ref(false)

async function loadEquipment() {
  try {
    const res = await equipmentApi.list({ pageSize: 100 })
    equipmentList.value = res.items
    if (res.items.length > 0) selectedEquipmentId.value = res.items[0].id
  } catch (e) {
    console.error('[IpqcRiskDashboard] Failed to load equipment list:', e)
  }
}

async function loadProcesses() {
  try {
    const res = await processApi.list({ pageSize: 100 })
    processList.value = res.items
    if (res.items.length > 0) selectedProcessId.value = res.items[0].id
  } catch (e) {
    console.error('[IpqcRiskDashboard] Failed to load process list:', e)
  }
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
  } catch (e) {
    console.error('[IpqcRiskDashboard] Failed to load risk history:', e)
  } finally {
    historyLoading.value = false
  }
}

async function loadLatest() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) return
  try {
    const latest = await riskScoreApi.latest(selectedEquipmentId.value, selectedProcessId.value)
    if (latest) riskScore.value = latest
  } catch (e) {
    console.error('[IpqcRiskDashboard] Failed to load latest risk score:', e)
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

function getTrendIcon(trend?: string) {
  if (trend === 'rising') return 'up'
  if (trend === 'falling') return 'down'
  return 'stable'
}

function getTrendLabel(trend?: string) {
  if (trend === 'rising') return '上升'
  if (trend === 'falling') return '下降'
  return '稳定'
}

function getRiskScoreClass(score: number) {
  if (score >= 70) return 'risk-score-circle--critical'
  if (score >= 40) return 'risk-score-circle--warning'
  return 'risk-score-circle--normal'
}

function getImpactClass(impact: number) {
  if (impact > 15) return 'risk-factor-impact--high'
  if (impact > 5) return 'risk-factor-impact--med'
  return 'risk-factor-impact--low'
}

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
  const lineColor = yData[yData.length - 1] >= yData[0] ? '#ff4d4f' : '#52c41a'

  trendChartOption.value = {
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const p = params[0]
        return `<strong>${p.name}</strong><br/>风险评分：<strong>${p.value}</strong>`
      },
    },
    grid: { left: 40, right: 16, top: 16, bottom: 24 },
    xAxis: {
      type: 'category', data: xData,
      axisLabel: { fontSize: 10, color: '#8c8c8c' },
      axisLine: { lineStyle: { color: '#e8e8e8' } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value', min: 0, max: 100,
      axisLabel: { fontSize: 10, color: '#8c8c8c' },
      splitLine: { lineStyle: { color: '#f0f0f0', type: 'dashed' } },
      axisLine: { show: false }, axisTick: { show: false },
    },
    series: [{
      data: yData, type: 'line', smooth: true, symbol: 'circle', symbolSize: 6,
      lineStyle: { width: 2, color: lineColor }, itemStyle: { color: lineColor },
      areaStyle: {
        color: {
          type: 'linear', x: 0, y: 0, x2: 0, y2: 1,
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
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="ai-header-banner">
      <div class="ai-header-banner-main">
        <div class="ai-header-banner-icon">
          <el-icon :size="24"><DataAnalysis /></el-icon>
        </div>
        <div class="ai-header-banner-text">
          <div class="ai-header-banner-title">AI 风险驾驶舱</div>
          <div class="ai-header-banner-subtitle">基于实时数据智能评估生产质量风险等级</div>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
        <el-select v-model="selectedEquipmentId" filterable placeholder="选择设备" style="width: 220px">
          <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
        </el-select>
        <el-select v-model="selectedProcessId" filterable placeholder="选择工序" style="width: 220px">
          <el-option v-for="p in processList" :key="p.id" :label="p.name" :value="p.id" />
        </el-select>
        <div class="ai-filter-actions">
          <el-button @click="loadLatest">
            <el-icon><Refresh /></el-icon> 最新评分
          </el-button>
          <el-button type="primary" :loading="loading" @click="analyzeRisk" size="default">
            <el-icon><DataAnalysis /></el-icon> 分析风险
          </el-button>
        </div>
      </div>
    </div>

    <!-- Risk Score Display -->
    <div v-if="riskScore" class="section">
      <div class="data-card">
        <div class="data-card__body">
          <div class="risk-score-display">
            <div :class="['risk-score-circle', getRiskScoreClass(riskScore.score)]">
              <div class="risk-score-number">{{ riskScore.score }}</div>
              <div class="risk-score-label">{{ levelLabel(riskScore.level) }}</div>
            </div>
            <div class="risk-score-details">
              <div class="risk-score-detail-item">
                <span class="risk-score-detail-label">趋势走向</span>
                <span class="risk-score-detail-value">
                  {{ getTrendLabel(riskScore.trend) }}
                </span>
              </div>
              <div class="risk-score-detail-item">
                <span class="risk-score-detail-label">最后更新</span>
                <span class="risk-score-detail-value">
                  {{ riskScore.lastUpdated ? new Date(riskScore.lastUpdated).toLocaleString('zh-CN') : '-' }}
                </span>
              </div>
              <div class="risk-score-detail-item">
                <span class="risk-score-detail-label">风险因素</span>
                <span class="risk-score-detail-value">{{ riskScore.factors?.length || 0 }} 项</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Trend Chart -->
    <div v-if="scoreHistory.length > 0" class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><TrendCharts /></el-icon>
          风险评分趋势（近 24 小时）
        </div>
      </div>
      <div class="data-card">
        <div class="data-card__body" style="padding: 12px;">
          <v-chart :option="trendChartOption" autoresize style="height: 280px;" />
        </div>
      </div>
    </div>

    <!-- Recommendations -->
    <div v-if="riskScore?.recommendations && riskScore.recommendations.length > 0" class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><Document /></el-icon>
          建议措施
        </div>
      </div>
      <div class="ai-recommendations">
        <div v-for="(rec, idx) in riskScore.recommendations" :key="idx" class="ai-recommendation-item">
          <div class="ai-recommendation-number">{{ idx + 1 }}</div>
          <div class="ai-recommendation-text">{{ rec }}</div>
        </div>
      </div>
    </div>

    <!-- Risk Factors -->
    <div v-if="riskScore?.factors && riskScore.factors.length > 0" class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><WarningFilled /></el-icon>
          风险因素
        </div>
      </div>
      <div class="risk-factors-grid">
        <div v-for="(factor, idx) in riskScore.factors" :key="idx" class="risk-factor-card">
          <div class="risk-factor-name">{{ factor.name }}</div>
          <div class="risk-factor-desc">{{ factor.description }}</div>
          <div class="risk-factor-values">
            <div class="risk-factor-current">
              <span class="risk-factor-current-label">当前</span>
              <span>{{ factor.currentValue != null ? factor.currentValue.toFixed(1) : '-' }}</span>
            </div>
            <div class="risk-factor-target">
              <span class="risk-factor-target-label">目标</span>
              <span>{{ factor.targetValue != null ? factor.targetValue.toFixed(1) : '-' }}</span>
            </div>
            <span :class="['risk-factor-impact', getImpactClass(factor.impact)]">
              +{{ factor.impact }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="!riskScore && !loading" class="empty-state">
      <div class="empty-state__icon"><el-icon :size="48"><DataAnalysis /></el-icon></div>
      <div class="empty-state__title">尚未进行风险分析</div>
      <div class="empty-state__desc">选择设备和工序后点击"分析风险"开始智能评估</div>
    </div>
  </div>
</template>

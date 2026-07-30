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

const equipmentList = ref<Equipment[]>([])
const processList = ref<Process[]>([])
const selectedEquipmentId = ref(0)
const selectedProcessId = ref(0)

const riskScore = ref<IpqcAiRiskScore | null>(null)
const scoreHistory = ref<IpqcAiRiskScore[]>([])
const loading = ref(false)
const historyLoading = ref(false)
const chartReady = ref(false)

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
  } catch { /* silent */ }
  finally {
    historyLoading.value = false
  }
}

async function loadLatest() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) return
  try {
    const latest = await riskScoreApi.latest(selectedEquipmentId.value, selectedProcessId.value)
    if (latest) riskScore.value = latest
  } catch { /* no data */ }
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
    <!-- Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><DataAnalysis /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">AI 风险驾驶舱</h2>
          <span class="page-header-banner-subtitle">基于实时数据智能评估生产质量风险等级</span>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="ipqc-content">
      <!-- Filter Bar -->
      <div class="action-bar">
        <el-select
          v-model="selectedEquipmentId"
          filterable
          placeholder="选择设备"
          size="small"
          :prefix-icon="Monitor"
          style="width: 220px"
        >
          <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
        </el-select>
        <el-select
          v-model="selectedProcessId"
          filterable
          placeholder="选择工序"
          size="small"
          :prefix-icon="Setting"
          style="width: 220px"
        >
          <el-option v-for="p in processList" :key="p.id" :label="p.name" :value="p.id" />
        </el-select>
        <el-button size="small" @click="loadLatest">
          <el-icon><Refresh /></el-icon>最新评分
        </el-button>
        <el-button type="primary" size="small" @click="analyzeRisk" :loading="loading">
          <el-icon><DataAnalysis /></el-icon>分析风险
        </el-button>
      </div>

      <!-- Risk Score Card - 始终显示 -->
      <div class="data-card" style="padding: 20px;">
        <!-- 无数据分析时的提示 -->
        <div v-if="!riskScore" style="text-align: center; padding: 40px 20px; color: var(--el-text-color-secondary);">
          <el-icon :size="48" style="margin-bottom: 12px; color: var(--el-text-color-placeholder);"><DataAnalysis /></el-icon>
          <p style="font-size: 14px; margin-bottom: 8px;">尚未进行风险分析</p>
          <p style="font-size: 13px; color: var(--el-text-color-placeholder);">选择设备和工序后点击"分析风险"开始智能评估</p>
        </div>

        <!-- 有分析结果时显示内容 -->
        <template v-else>
          <!-- Stat Grid - 统一计数卡片样式 -->
          <div class="stats-bar" style="margin-bottom: 20px;">
            <div class="stat-item" :class="riskScore.score >= 70 ? 'stat-anomaly' : riskScore.score >= 40 ? 'stat-pending' : 'stat-qualified'">
              <div class="stat-accent"></div>
              <div class="stat-content">
                <div class="stat-value">{{ riskScore.score }}</div>
                <div class="stat-label">风险评分</div>
              </div>
            </div>
            <div class="stat-item stat-total">
              <div class="stat-accent"></div>
              <div class="stat-content">
                <div class="stat-value" style="font-size: 18px;">
                  <el-tag :type="levelTag(riskScore.level)" size="large" effect="dark">{{ levelLabel(riskScore.level) }}</el-tag>
                </div>
                <div class="stat-label">风险等级</div>
              </div>
            </div>
            <div class="stat-item stat-inspecting">
              <div class="stat-accent"></div>
              <div class="stat-content">
                <div class="stat-value" style="font-size: 18px;">
                  <el-icon :size="16" :style="{ color: riskScore.trend === 'rising' ? '#ff4d4f' : riskScore.trend === 'falling' ? '#52c41a' : '#8c8c8c' }">
                    <ArrowUp v-if="riskScore.trend === 'rising'" />
                    <ArrowDown v-else-if="riskScore.trend === 'falling'" />
                    <Remove v-else />
                  </el-icon>
                  {{ riskScore.trend === 'rising' ? '上升' : riskScore.trend === 'falling' ? '下降' : '稳定' }}
                </div>
                <div class="stat-label">趋势走向</div>
              </div>
            </div>
            <div class="stat-item stat-pending">
              <div class="stat-accent"></div>
              <div class="stat-content">
                <div v-if="riskScore.lastUpdated" style="font-size: 14px; font-weight: 600; color: var(--el-text-color-regular);">
                  {{ new Date(riskScore.lastUpdated).toLocaleString('zh-CN', { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' }) }}
                </div>
                <div v-else style="font-size: 14px; color: var(--el-text-color-placeholder);">-</div>
                <div class="stat-label">最后更新</div>
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
            <el-table :data="riskScore.factors" stripe style="width: 100%" size="small">
              <el-table-column type="index" label="序号" width="55" />
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
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* ─── 主体布局 ──────────────────────────────────── */
.page-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.ipqc-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  overflow-y: auto;
}

/* ── 统计栏 ── */
.stats-bar {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  flex-shrink: 0;
}
.stat-item {
  display: flex;
  align-items: center;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  padding: 14px 18px;
  transition: box-shadow 0.2s, transform 0.2s;
}
.stat-item:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
}
.stat-accent {
  width: 4px;
  border-radius: 2px;
}
.stat-total .stat-accent { background: var(--el-color-primary); }
.stat-qualified .stat-accent { background: var(--el-color-success); }
.stat-anomaly .stat-accent { background: var(--el-color-danger); }
.stat-pending .stat-accent { background: var(--el-color-warning); }
.stat-inspecting .stat-accent { background: var(--el-color-info); }
.stat-content {
  display: flex;
  flex-direction: column;
}
.stat-value {
  font-size: 28px;
  font-weight: 700;
  line-height: 1.2;
}
.stat-total .stat-value { color: var(--el-color-primary); }
.stat-qualified .stat-value { color: var(--el-color-success); }
.stat-anomaly .stat-value { color: var(--el-color-danger); }
.stat-pending .stat-value { color: var(--el-color-warning); }
.stat-inspecting .stat-value { color: var(--el-color-info); }
.stat-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  margin-top: 2px;
}

/* ── 通用 data-card ── */
.data-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

/* ── 风险因素表格 ── */
.risk-factors-table {
  width: 100%;
}

.risk-factors-table :deep(.el-table__cell) {
  white-space: nowrap;
}

.risk-factors-table :deep(.el-table__header-wrapper) {
  flex-shrink: 0;
}

.risk-factors-table :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

/* ── 操作栏 ── */
.action-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-shrink: 0;
}
</style>

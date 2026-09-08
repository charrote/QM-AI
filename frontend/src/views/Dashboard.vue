<script setup lang="ts">
defineOptions({ name: 'Dashboard' })

import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import {
  DataAnalysis, TrendCharts, CircleCheck, Warning, Bell,
  Calendar, Box, Monitor, Checked, Search,
} from '@element-plus/icons-vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart } from 'echarts/charts'
import {
  GridComponent, TooltipComponent, MarkLineComponent,
} from 'echarts/components'
import { reportsApi } from '@/api/reports'
import { aiApi } from '@/api/ai'
import { dashboardApi, type PendingItem } from '@/api/dashboard'

use([
  CanvasRenderer,
  LineChart,
  BarChart,
  GridComponent,
  TooltipComponent,
  MarkLineComponent,
])

const router = useRouter()

// ─── 时间 ────────────────────────────────────────────────────
const now = ref(new Date())
let clockTimer: number | undefined

const dateText = computed(() =>
  now.value.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long',
  }),
)

function todayStr(): string {
  const d = new Date()
  const p = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${p(d.getMonth() + 1)}-${p(d.getDate())}`
}

function fmtTime(iso: string): string {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return ''
  const hm = d.toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit', hour12: false })
  if (d.toDateString() === now.value.toDateString()) return hm
  return `${d.getMonth() + 1}月${d.getDate()}日 ${hm}`
}

// ─── 数据状态（全部来自接口，无硬编码数据） ──────────────────
const loading = ref(true)

type QualityStats = Awaited<ReturnType<typeof reportsApi.qualityStats>>

const quality = ref<QualityStats | null>(null)
const todayStats = ref<QualityStats | null>(null)
const trend = ref<Array<{ date: string; total: number; pass: number; fpy: number | null }>>([])
const pareto = ref<Array<{ defectCode: string; defectName: string; count: number; percentage: number }>>([])
const pendingItems = ref<PendingItem[]>([])
const pendingTotal = ref(0)
const aiAlerts = ref<Array<{
  id: number
  level: string
  title: string
  description: string
  source: string
  resolved: boolean
  createdAt: string
}>>([])
const aiUnresolvedTotal = ref(0)

// ─── KPI ─────────────────────────────────────────────────────
const kpiInspections = computed<number | null>(() =>
  todayStats.value ? todayStats.value.totalInspections : null,
)

const kpiFpy = computed<number | null>(() =>
  quality.value && typeof quality.value.fpy === 'number' ? quality.value.fpy : null,
)

// 检验数日环比（基于趋势真实数据；任一天无数据则不显示）
const inspectionDelta = computed<{ text: string; dir: 'up' | 'down' } | null>(() => {
  const today = trend.value[trend.value.length - 1]
  const yest = trend.value[trend.value.length - 2]
  if (!today || !yest || yest.total <= 0) return null
  const pct = Math.round(((today.total - yest.total) / yest.total) * 100)
  if (pct === 0) return null
  return { text: `${Math.abs(pct)}% 较昨日`, dir: pct > 0 ? 'up' : 'down' }
})

const fpyMeta = computed(() => {
  const v = kpiFpy.value
  if (v == null) return { color: 'var(--text-placeholder)', tag: '暂无数据', tagCls: 'tag-neutral' }
  if (v >= 95) return { color: 'var(--success)', tag: '达标（目标 95%）', tagCls: 'tag-good' }
  if (v >= 90) return { color: 'var(--warning)', tag: '低于目标 95%', tagCls: 'tag-mid' }
  return { color: 'var(--danger)', tag: '低于目标 95%', tagCls: 'tag-bad' }
})

const pendingMeta = computed(() => {
  if (pendingTotal.value > 0) return { color: 'var(--danger)', tag: '需关注', tagCls: 'tag-bad' }
  return { color: 'var(--success)', tag: '全部清空', tagCls: 'tag-good' }
})

const aiMeta = computed(() => {
  if (aiUnresolvedTotal.value > 0) return { color: 'var(--warning)', tag: '未解决', tagCls: 'tag-mid' }
  return { color: 'var(--success)', tag: '无预警', tagCls: 'tag-good' }
})

// ─── 趋势图（近 7 天 FPY + 目标线 95%，稀疏点如实留空） ──────
const trendOption = computed(() => {
  const fps = trend.value.map(t => t.fpy ?? null)
  if (!fps.some(v => v != null)) return null

  const vals = fps.filter((v): v is number => v != null)
  const min = Math.max(0, Math.floor((Math.min(...vals, 95) - 5) / 5) * 5)
  const max = Math.min(100, Math.ceil((Math.max(...vals, 95) + 5) / 5) * 5)

  return {
    grid: { left: 44, right: 12, top: 20, bottom: 24 },
    tooltip: {
      trigger: 'axis',
      valueFormatter: (v: unknown) => (v == null ? '-' : `${v}%`),
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: trend.value.map(t => t.date.slice(5)),
      axisLine: { lineStyle: { color: 'var(--border-base)' } },
      axisTick: { show: false },
      axisLabel: { color: 'var(--text-secondary)', fontSize: 11 },
    },
    yAxis: {
      type: 'value',
      min,
      max,
      axisLabel: { formatter: '{value}%', color: 'var(--text-secondary)', fontSize: 11 },
      splitLine: { lineStyle: { color: 'var(--border-light)' } },
    },
    series: [
      {
        name: 'FPY',
        type: 'line',
        data: fps,
        connectNulls: false,
        symbol: 'circle',
        symbolSize: 6,
        lineStyle: { color: 'var(--primary)', width: 2 },
        itemStyle: { color: 'var(--primary)' },
        areaStyle: { color: 'rgba(22, 119, 255, 0.06)' },
        markLine: {
          silent: true,
          symbol: 'none',
          lineStyle: { color: 'var(--danger)', type: 'dashed', width: 1 },
          label: { formatter: '目标 95%', color: 'var(--danger)', fontSize: 10, position: 'insideEndTop' },
          data: [{ yAxis: 95 }],
        },
      },
    ],
  }
})

// ─── 帕累托（Top5 横向条形） ─────────────────────────────────
const paretoTop5 = computed(() => pareto.value.slice(0, 5))

const paretoOption = computed(() => {
  const top = paretoTop5.value
  if (top.length === 0) return null

  const maxCount = Math.max(...top.map(p => p.count))
  const reversed = [...top].reverse()

  return {
    grid: { left: 8, right: 44, top: 8, bottom: 8, containLabel: true },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: (params: unknown) => {
        const ps = params as Array<{ dataIndex: number; name: string; value: number }>
        if (!ps?.length) return ''
        const item = reversed[ps[0].dataIndex]
        return `${item.defectName}<br/>数量：${item.count} 条 · 占比 ${item.percentage}%`
      },
    },
    xAxis: { type: 'value', show: false, max: maxCount },
    yAxis: {
      type: 'category',
      data: reversed.map(p => p.defectName),
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: {
        color: 'var(--text-regular)',
        fontSize: 12,
        width: 96,
        overflow: 'truncate',
      },
    },
    series: [
      {
        type: 'bar',
        data: reversed.map(p => p.count),
        barWidth: 10,
        itemStyle: { color: 'var(--primary)', borderRadius: [0, 4, 4, 0] },
        label: {
          show: true,
          position: 'right',
          color: 'var(--text-secondary)',
          fontSize: 11,
          formatter: '{c}',
        },
      },
    ],
  }
})

// ─── 待处理列表 ──────────────────────────────────────────────
const LABEL_STYLES: Record<string, { color: string; bg: string }> = {
  紧急: { color: 'var(--danger)', bg: 'var(--danger-bg)' },
  待判定: { color: 'var(--warning)', bg: 'var(--warning-bg)' },
  逾期风险: { color: 'var(--primary)', bg: 'var(--primary-bg)' },
  待放行: { color: 'var(--success)', bg: 'var(--success-bg)' },
  处理中: { color: 'var(--text-secondary)', bg: 'var(--info-bg)' },
}

function labelStyle(label: string) {
  return LABEL_STYLES[label] ?? { color: 'var(--text-secondary)', bg: 'var(--info-bg)' }
}

const TYPE_ROUTES: Record<string, string> = {
  iqc: '/iqc/inspections',
  fqc: '/fqc/inspections',
  defect: '/defects',
  capa: '/defects/capa',
  complaint: '/complaints/list',
  firstpiece: '/ipqc/first-pieces',
}

function openPending(item: PendingItem) {
  router.push(TYPE_ROUTES[item.type] ?? '/')
}

// ─── AI 风险预警 ─────────────────────────────────────────────
function alertLevelStyle(level: string) {
  const l = (level || '').toLowerCase()
  if (l === 'critical' || l === 'high' || l === '紧急')
    return { color: 'var(--danger)', bg: 'var(--danger-bg)' }
  if (l === 'medium' || l === 'warning' || l === 'major' || l === '中等')
    return { color: 'var(--warning)', bg: 'var(--warning-bg)' }
  return { color: 'var(--primary)', bg: 'var(--primary-bg)' }
}

// ─── 常用入口（精简 6 个） ───────────────────────────────────
const quickEntries = [
  { name: 'IQC 来料', path: '/iqc/receipts', icon: Box },
  { name: 'IPQC 首件', path: '/ipqc/first-pieces', icon: Monitor },
  { name: 'FQC 成品', path: '/fqc/inspections', icon: Checked },
  { name: 'SPC 分析', path: '/spc', icon: TrendCharts },
  { name: '追溯查询', path: '/trace', icon: Search },
  { name: '质量报表', path: '/reports/dashboard', icon: DataAnalysis },
]

function openQuick(path: string) {
  router.push(path)
}

// ─── 数据加载 ────────────────────────────────────────────────
onMounted(async () => {
  clockTimer = window.setInterval(() => {
    now.value = new Date()
  }, 60000)

  const t = todayStr()
  const results = await Promise.allSettled([
    reportsApi.qualityStats(),
    reportsApi.qualityStats({ startDate: t, endDate: t }),
    reportsApi.qualityTrend({ days: 7 }),
    reportsApi.defectPareto(),
    dashboardApi.pending(),
    aiApi.alerts({ resolved: false, page: 1, pageSize: 5 }),
  ])

  const [q, today, tr, pa, pd, ai] = results
  if (q.status === 'fulfilled') quality.value = q.value
  else console.error('[Dashboard] qualityStats failed:', q.reason)
  if (today.status === 'fulfilled') todayStats.value = today.value
  else console.error('[Dashboard] todayStats failed:', today.reason)
  if (tr.status === 'fulfilled') trend.value = tr.value
  else console.error('[Dashboard] qualityTrend failed:', tr.reason)
  if (pa.status === 'fulfilled') pareto.value = pa.value
  else console.error('[Dashboard] defectPareto failed:', pa.reason)
  if (pd.status === 'fulfilled') {
    pendingItems.value = pd.value.items ?? []
    pendingTotal.value = pd.value.total ?? 0
  } else console.error('[Dashboard] pending failed:', pd.reason)
  if (ai.status === 'fulfilled') {
    aiAlerts.value = ai.value.items ?? []
    aiUnresolvedTotal.value = ai.value.total ?? ai.value.items?.length ?? 0
  } else console.error('[Dashboard] aiAlerts failed:', ai.reason)

  loading.value = false
})

onBeforeUnmount(() => {
  if (clockTimer !== undefined) window.clearInterval(clockTimer)
})
</script>

<template>
  <div class="cockpit">
    <!-- 头部 -->
    <div class="cockpit-header">
      <div>
        <h1 class="title">质量决策驾驶舱</h1>
        <div class="subtitle">全厂质量实时态势 · 待处理事项 · AI 风险预警</div>
      </div>
      <div class="date-pill">
        <el-icon :size="14"><Calendar /></el-icon>
        <span>{{ dateText }}</span>
      </div>
    </div>

    <!-- KPI 条 -->
    <div class="kpi-row">
      <div class="kpi">
        <div class="kpi-label"><el-icon :size="15"><DataAnalysis /></el-icon>今日检验数</div>
        <div class="kpi-num" :style="{ color: 'var(--primary)' }">
          {{ kpiInspections ?? '-' }}
        </div>
        <div v-if="inspectionDelta" class="kpi-tag" :class="inspectionDelta.dir === 'up' ? 'tag-good' : 'tag-bad'">
          {{ inspectionDelta.dir === 'up' ? '▲' : '▼' }} {{ inspectionDelta.text }}
        </div>
        <div v-else class="kpi-tag tag-neutral">实时</div>
      </div>

      <div class="kpi">
        <div class="kpi-label"><el-icon :size="15"><CircleCheck /></el-icon>一次合格率 FPY</div>
        <div class="kpi-num" :style="{ color: fpyMeta.color }">
          {{ kpiFpy ?? '-' }}<small v-if="kpiFpy != null">%</small>
        </div>
        <div class="kpi-tag" :class="fpyMeta.tagCls">{{ fpyMeta.tag }}</div>
      </div>

      <div class="kpi">
        <div class="kpi-label"><el-icon :size="15"><Warning /></el-icon>异常待处理</div>
        <div class="kpi-num" :style="{ color: pendingMeta.color }">
          {{ pendingTotal }}
        </div>
        <div class="kpi-tag" :class="pendingMeta.tagCls">{{ pendingMeta.tag }}</div>
      </div>

      <div class="kpi">
        <div class="kpi-label"><el-icon :size="15"><Bell /></el-icon>AI 预警</div>
        <div class="kpi-num" :style="{ color: aiMeta.color }">
          {{ aiUnresolvedTotal }}
        </div>
        <div class="kpi-tag" :class="aiMeta.tagCls">{{ aiMeta.tag }}</div>
      </div>
    </div>

    <!-- 趋势 + 帕累托 -->
    <div class="row row-21">
      <div class="card">
        <div class="card-head">
          <b>近 7 天质量趋势</b>
          <el-link type="primary" :underline="false" @click="openQuick('/reports/dashboard')">查看报表 ›</el-link>
        </div>
        <div class="card-body">
          <template v-if="loading">
            <el-skeleton :rows="5" animated />
          </template>
          <template v-else>
            <div v-if="trendOption" class="chart-wrap">
              <div class="legend">
                <span><i class="chip chip-line" style="background: var(--primary)" />FPY 合格率</span>
                <span><i class="chip chip-line" style="background: var(--danger)" />目标线 95%</span>
              </div>
              <v-chart :option="trendOption" autoresize class="chart" />
            </div>
            <div v-else class="empty">暂无数据</div>
          </template>
        </div>
      </div>

      <div class="card">
        <div class="card-head">
          <b>缺陷帕累托 Top5</b>
          <el-link type="primary" :underline="false" @click="openQuick('/defects')">分析 ›</el-link>
        </div>
        <div class="card-body">
          <template v-if="loading">
            <el-skeleton :rows="5" animated />
          </template>
          <template v-else>
            <div v-if="paretoOption">
              <v-chart :option="paretoOption" autoresize class="chart" />
            </div>
            <div v-else class="empty">暂无数据</div>
          </template>
        </div>
      </div>
    </div>

    <!-- 待处理 + AI 风险 -->
    <div class="row row-11">
      <div class="card">
        <div class="card-head">
          <b>今日待处理</b>
          <el-link type="primary" :underline="false" @click="openQuick('/defects')">全部 ›</el-link>
        </div>
        <div class="card-body pad-top-sm">
          <template v-if="loading">
            <el-skeleton :rows="4" animated />
          </template>
          <template v-else-if="pendingItems.length">
            <div
              v-for="item in pendingItems"
              :key="`${item.type}-${item.id}`"
              class="todo"
              @click="openPending(item)"
            >
              <span class="dot" :style="{ background: labelStyle(item.label).color }" />
              <div class="todo-tx">
                <div class="todo-t">{{ item.title }}</div>
                <div class="todo-m">{{ item.detail }} · {{ fmtTime(item.time) }}</div>
              </div>
              <span
                class="tag"
                :style="{ color: labelStyle(item.label).color, background: labelStyle(item.label).bg }"
              >{{ item.label }}</span>
            </div>
          </template>
          <div v-else class="empty">暂无待处理事项</div>
        </div>
      </div>

      <div class="card">
        <div class="card-head">
          <b>AI 风险预警</b>
          <el-link type="primary" :underline="false" @click="openQuick('/ai/alerts')">预警中心 ›</el-link>
        </div>
        <div class="card-body pad-top-sm">
          <template v-if="loading">
            <el-skeleton :rows="4" animated />
          </template>
          <template v-else-if="aiAlerts.length">
            <div v-for="alert in aiAlerts" :key="alert.id" class="risk" @click="openQuick('/ai/alerts')">
              <div class="risk-ic" :style="alertLevelStyle(alert.level)">
                <el-icon :size="15"><Warning /></el-icon>
              </div>
              <div class="risk-tx">
                <div class="risk-t">{{ alert.title }}</div>
                <div class="risk-m">{{ alert.description }}</div>
              </div>
              <span class="risk-go">详情 ›</span>
            </div>
          </template>
          <div v-else class="empty">暂无数据</div>
        </div>
      </div>
    </div>

    <!-- 常用入口 -->
    <div class="card">
      <div class="card-head">
        <b>常用入口</b>
      </div>
      <div class="quick">
        <div
          v-for="q in quickEntries"
          :key="q.name"
          class="quick-item"
          @click="openQuick(q.path)"
        >
          <span class="quick-ic">
            <el-icon :size="17"><component :is="q.icon" /></el-icon>
          </span>
          <span class="quick-name">{{ q.name }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.cockpit {
  padding: var(--space-5);
  max-width: var(--layout-content-max-width);
  margin: 0 auto;
}

/* ═══ 头部 ═══ */
.cockpit-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: var(--space-3);
  margin-bottom: var(--space-4);
}

.title {
  font-size: var(--font-xl);
  font-weight: var(--font-semibold);
  color: var(--text-primary);
  margin: 0;
  line-height: 1.3;
}

.subtitle {
  font-size: var(--font-sm);
  color: var(--text-secondary);
  margin-top: var(--space-1);
}

.date-pill {
  display: flex;
  align-items: center;
  gap: 7px;
  font-size: var(--font-sm);
  color: var(--text-secondary);
  background: var(--bg-container);
  border: 1px solid var(--border-base);
  border-radius: var(--radius-full);
  padding: 7px 14px;
}

/* ═══ KPI 条（语义色数字，无左边彩条，细边框） ═══ */
.kpi-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: var(--space-4);
}

.kpi {
  background: var(--bg-container);
  border: 1px solid var(--border-base);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xs);
  padding: 16px 18px;
}

.kpi-label {
  font-size: var(--font-sm);
  color: var(--text-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
}

.kpi-label .el-icon {
  color: var(--text-placeholder);
}

.kpi-num {
  font-size: 28px;
  font-weight: var(--font-bold);
  line-height: 1;
  margin-top: var(--space-2);
  font-variant-numeric: tabular-nums;
  letter-spacing: -0.5px;
}

.kpi-num small {
  font-size: var(--font-md);
  font-weight: var(--font-medium);
  color: var(--text-secondary);
  margin-left: 2px;
}

.kpi-tag {
  font-size: var(--font-xs);
  font-weight: var(--font-medium);
  margin-top: 7px;
  display: inline-flex;
  align-items: center;
  gap: 3px;
  padding: 2px 7px;
  border-radius: var(--radius-full);
}

.tag-good { color: var(--success); background: var(--success-bg); }
.tag-mid  { color: var(--warning); background: var(--warning-bg); }
.tag-bad  { color: var(--danger); background: var(--danger-bg); }
.tag-neutral { color: var(--text-secondary); background: var(--info-bg); }

/* ═══ 网格行 ═══ */
.row {
  display: grid;
  gap: 12px;
  margin-bottom: var(--space-4);
}

.row-21 { grid-template-columns: 1.4fr 1fr; }
.row-11 { grid-template-columns: 1fr 1fr; }

/* ═══ 卡片（统一 1px 浅灰边框，无彩色竖条） ═══ */
.card {
  background: var(--bg-container);
  border: 1px solid var(--border-base);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-xs);
  overflow: hidden;
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid var(--border-light);
}

.card-head b {
  font-size: 13.5px;
  font-weight: var(--font-semibold);
  color: var(--text-primary);
}

.card-body {
  padding: 14px 16px;
}

.card-body.pad-top-sm {
  padding-top: 2px;
}

/* ═══ 图表 ═══ */
.chart-wrap {
  width: 100%;
}

.chart {
  width: 100%;
  height: 220px;
}

.legend {
  display: flex;
  gap: 16px;
  font-size: var(--font-xs);
  color: var(--text-secondary);
  padding: 0 4px 6px;
}

.chip {
  display: inline-block;
  width: 10px;
  height: 3px;
  border-radius: 2px;
  margin-right: 5px;
  vertical-align: middle;
}

/* ═══ 待处理列表 ═══ */
.todo {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 10px 0;
  border-bottom: 1px solid var(--border-light);
  cursor: pointer;
}

.todo:last-child {
  border-bottom: none;
}

.dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  margin-top: 6px;
  flex-shrink: 0;
}

.todo-tx {
  flex: 1;
  min-width: 0;
}

.todo-t {
  font-size: 12.5px;
  line-height: 1.5;
  color: var(--text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.todo-m {
  font-size: var(--font-xs);
  color: var(--text-placeholder);
  margin-top: 3px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.tag {
  font-size: 10.5px;
  padding: 1px 7px;
  border-radius: var(--radius-sm);
  flex-shrink: 0;
}

/* ═══ AI 风险 ═══ */
.risk {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 11px 0;
  border-bottom: 1px solid var(--border-light);
  cursor: pointer;
}

.risk:last-child {
  border-bottom: none;
}

.risk-ic {
  width: 28px;
  height: 28px;
  border-radius: 7px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.risk-tx {
  flex: 1;
  min-width: 0;
}

.risk-t {
  font-size: 12.5px;
  font-weight: var(--font-medium);
  color: var(--text-primary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.risk-m {
  font-size: 11.5px;
  color: var(--text-secondary);
  margin-top: 3px;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.risk-go {
  font-size: var(--font-xs);
  color: var(--primary);
  flex-shrink: 0;
  margin-top: 2px;
}

/* ═══ 空态 ═══ */
.empty {
  padding: 32px 0;
  text-align: center;
  font-size: var(--font-sm);
  color: var(--text-placeholder);
}

/* ═══ 常用入口（精简 6 个） ═══ */
.quick {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 10px;
  padding: 14px 16px;
}

.quick-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 7px;
  padding: 12px 6px;
  border: 1px solid var(--border-light);
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--text-primary);
  font-size: var(--font-sm);
  text-align: center;
  transition: all var(--duration-fast) var(--ease-out);
}

.quick-item:hover {
  border-color: var(--primary);
  background: var(--primary-bg);
}

.quick-ic {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--primary-bg);
  color: var(--primary);
  display: flex;
  align-items: center;
  justify-content: center;
}

/* ═══ 响应式 ═══ */
@media (max-width: 1200px) {
  .kpi-row { grid-template-columns: repeat(2, 1fr); }
  .row-21,
  .row-11 { grid-template-columns: 1fr; }
  .quick { grid-template-columns: repeat(3, 1fr); }
}
</style>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick, watch, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Plus, Edit, Delete, DataAnalysis, Refresh, DataLine, HelpFilled } from '@element-plus/icons-vue'
import { spcApi } from '@/api/spc'
import { inspectionItemApi } from '@/api/inspectionItem'
import type { SpcControlChart, SpcControlChartDetail, CreateSpcControlChart, UpdateSpcControlChart, SpcDataPoint, SpcAnalysisReport, SpcAlertRule, SpcAlertTrigger, SpcAnovaResult, SpcAnovaRequest, SpcAnovaFactor, SpcDataSource, CreateSpcDataSource, BusinessInspectionData } from '@/types/spc'
import type { InspectionItem } from '@/types/inspectionItem'
import type { PagedRequest } from '@/types/basicData'
import { CHART_TYPE_OPTIONS, CHART_TYPE_MAP, CPK_GRADE_TYPE, ANOVA_SOURCE_MAP, SOURCE_TYPE_OPTIONS, WESTERN_ELECTRIC_RULES } from '@/types/spc'
import { useSpcHelpers } from '@/composables/useSpcHelpers'

defineOptions({ name: 'SpcChartDetailPage' })

const { formatDate, formatNumber, cpkGradeType, anovaSourceLabel, sourceTypeLabel, getRuleName, violationCount } = useSpcHelpers()
const router = useRouter()

const route = useRoute()
const chartId = ref<number>(Number(route.params.id) || Number(route.query.chartId) || 0)
const loading = ref(false)
const analysisLoading = ref(false)
const anovaLoading = ref(false)
const businessDataLoading = ref(false)

const activeTab = ref('chart')

// Chart detail
const selectedChart = ref<SpcControlChartDetail | null>(null)

// Data points
const dataPoints = ref<SpcDataPoint[]>([])
const dataPointsTotal = ref(0)
const dpQuery = reactive<PagedRequest>({ page: 1, pageSize: 50 })

// Add data point
const dpDialogVisible = ref(false)
const dpForm = reactive({
  chartId: 0,
  individualValues: '',
  measuredAt: new Date().toISOString().slice(0, 16),
})

// Analysis report
const analysisReport = ref<SpcAnalysisReport | null>(null)

// Alert rules
const alertRules = ref<SpcAlertRule[]>([])

// Alert triggers
const triggers = ref<any[]>([])
const triggersTotal = ref(0)
const triggerQuery = reactive<PagedRequest>({ page: 1, pageSize: 20 })

// ANOVA
const anovaResults = ref<SpcAnovaResult[]>([])

// Data Sources
const dataSources = ref<SpcDataSource[]>([])
const businessData = ref<BusinessInspectionData[]>([])
const businessDateRange = ref<[Date, Date] | null>(null)
const selectedBusinessData = ref<BusinessInspectionData[]>([])
const dsDialogVisible = ref(false)
const dsForm = reactive<CreateSpcDataSource>({
  chartId: 0,
  sourceType: 'IQC',
  inspectionItemId: undefined,
})
const inspectionItemOptions = ref<InspectionItem[]>([])

// Rule config
const ruleConfigVisible = ref(false)

// ECharts refs
const chart1Ref = ref<HTMLElement>()
const chart2Ref = ref<HTMLElement>()
let chart1Instance: any = null
let chart2Instance: any = null

const resizeHandler1 = () => chart1Instance?.resize?.()
const resizeHandler2 = () => chart2Instance?.resize?.()

function cleanupCharts() {
  if (chart1Instance) {
    window.removeEventListener('resize', resizeHandler1)
    chart1Instance.dispose()
    chart1Instance = null
  }
  if (chart2Instance) {
    window.removeEventListener('resize', resizeHandler2)
    chart2Instance.dispose()
    chart2Instance = null
  }
}

onUnmounted(() => {
  cleanupCharts()
})

// ─── Load Data ─────────────────────────────────────────

async function loadAll() {
  if (!chartId.value) {
    ElMessage.warning('请选择一个控制图')
    router.push('/spc/charts')
    return
  }
  loading.value = true
  try {
    selectedChart.value = await spcApi.getChart(chartId.value)
    await Promise.all([
      fetchDataPoints(),
      runAnalysis(),
      fetchAlertRules(),
    ])
  } catch (e: any) {
    ElMessage.error('加载控制图详情失败: ' + (e.message || ''))
  } finally {
    loading.value = false
  }
}

async function fetchDataPoints() {
  if (!chartId.value) return
  try {
    const res = await spcApi.listDataPoints(chartId.value, { ...dpQuery })
    dataPoints.value = res.items
    dataPointsTotal.value = res.total
  } catch { /* ignore */ }
}

async function runAnalysis() {
  if (!chartId.value) return
  analysisLoading.value = true
  try {
    analysisReport.value = await spcApi.analyzeChart({ chartId: chartId.value })
    await nextTick()
    renderCharts()
  } catch (e: any) {
    ElMessage.error('SPC分析失败: ' + (e.message || ''))
  } finally {
    analysisLoading.value = false
  }
}

async function fetchAlertRules() {
  if (!chartId.value) return
  try {
    alertRules.value = await spcApi.listAlertRules(chartId.value)
  } catch { /* ignore */ }
}

async function fetchTriggers() {
  if (!chartId.value) return
  try {
    const res = await spcApi.listTriggers(chartId.value, { ...triggerQuery })
    triggers.value = res.items as any
    triggersTotal.value = res.total
  } catch { /* ignore */ }
}

async function fetchAnovaResults() {
  if (!chartId.value) return
  try {
    anovaResults.value = await spcApi.listAnovaResults(chartId.value)
  } catch { /* ignore */ }
}

async function fetchDataSources() {
  if (!chartId.value) return
  try {
    dataSources.value = await spcApi.listDataSources(chartId.value)
  } catch { /* ignore */ }
}

// ─── ECharts Rendering ─────────────────────────────────

function renderCharts() {
  if (!analysisReport.value?.dataPoints || !analysisReport.value?.controlLimits) return
  const limits = analysisReport.value.controlLimits
  const dps = analysisReport.value.dataPoints
  const categories = dps.map(dp => `#${dp.subgroupIndex}`)
  const chartType = selectedChart.value?.chartType || 'Xbar_R'

  if (chartType === 'I_MR') {
    const iValues = dps.map(dp => Number(dp.subgroupMean || 0))
    const mrValues = dps.map(dp => Number(dp.subgroupRange || 0))
    renderLineChart(chart1Ref.value, 'I 单值控制图', categories, iValues, limits, 'X̄', '#409eff',
      { cl: limits.clXbar, ucl: limits.uclXbar, lcl: limits.lclXbar }, chart1Instance)
    renderLineChart(chart2Ref.value, 'MR 移动极差控制图', categories, mrValues, limits, 'MR', '#e6a23c',
      { cl: limits.clR, ucl: limits.uclR, lcl: limits.lclR }, chart2Instance)
  } else {
    const xbarValues = dps.map(dp => Number(dp.subgroupMean || 0))
    const rangeValues = dps.map(dp => Number(dp.subgroupRange || 0))
    const secondLabel = chartType === 'Xbar_S' ? 'S' : 'R'
    renderLineChart(chart1Ref.value, 'X̄ 均值控制图', categories, xbarValues, limits, 'X̄', '#409eff',
      { cl: limits.clXbar, ucl: limits.uclXbar, lcl: limits.lclXbar }, chart1Instance)
    renderLineChart(chart2Ref.value, `${secondLabel} ${chartType === 'Xbar_S' ? '标准差' : '极差'}控制图`, categories, rangeValues, limits, secondLabel, '#e6a23c',
      { cl: limits.clR, ucl: limits.uclR, lcl: limits.lclR }, chart2Instance)
  }
}

function renderLineChart(container: HTMLElement | undefined, title: string, categories: string[], values: number[], limits: any, seriesName: string, color: string, lineLimits: { cl: number, ucl: number, lcl: number }, instanceRef: any) {
  if (!container) return
  import('echarts').then(echarts => {
    if (instanceRef.current) instanceRef.current.dispose()
    const chart = echarts.init(container)

    const violations = analysisReport.value?.violations || []
    const violationIndices = new Set(violations.map(v => v.index))
    const markPoints: any[] = []
    values.forEach((v, i) => {
      if (violationIndices.has(i)) {
        markPoints.push({
          name: '违规点', value: v, xAxis: i, yAxis: v,
          symbol: 'circle', symbolSize: 12,
          itemStyle: { color: '#f56c6c', borderColor: '#f56c6c', borderWidth: 2 },
          label: { show: true, formatter: '违规', fontSize: 12, color: '#f56c6c', position: 'top' },
        })
      }
    })

    chart.setOption({
      title: { text: title, left: 'center', textStyle: { fontSize: 14 } },
      tooltip: { trigger: 'axis', formatter: (params: any) => `子组: ${params[0].axisValue}<br/>${seriesName}: ${params[0].value?.toFixed(4)}` },
      legend: { data: [seriesName, 'CL', 'UCL', 'LCL'], bottom: 0 },
      grid: { left: 60, right: 20, top: 40, bottom: 40 },
      xAxis: { type: 'category', data: categories, axisLabel: { rotate: 45, fontSize: 10 } },
      yAxis: { type: 'value', name: seriesName, nameTextStyle: { fontSize: 11 } },
      series: [{
        name: seriesName, type: 'line', data: values, smooth: false,
        symbol: 'circle', symbolSize: 6,
        lineStyle: { color, width: 2 },
        itemStyle: { color },
        markPoint: { data: markPoints },
        markLine: {
          silent: true,
          data: [
            { name: 'UCL', yAxis: lineLimits.ucl, lineStyle: { color: '#f56c6c', type: 'dashed' }, label: { formatter: `UCL=${lineLimits.ucl?.toFixed(4)}` } },
            { name: 'CL', yAxis: lineLimits.cl, lineStyle: { color: '#67c23a', type: 'solid' }, label: { formatter: `CL=${lineLimits.cl?.toFixed(4)}` } },
            { name: 'LCL', yAxis: lineLimits.lcl, lineStyle: { color: '#f56c6c', type: 'dashed' }, label: { formatter: `LCL=${lineLimits.lcl?.toFixed(4)}` } },
          ],
        },
      }],
    })
    instanceRef.current = chart
    window.removeEventListener('resize', resizeHandler1)
    window.addEventListener('resize', resizeHandler1)
  })
}

// ─── Data Point CRUD ───────────────────────────────────

function openAddDataPoint() {
  dpForm.chartId = chartId.value
  dpForm.individualValues = ''
  dpForm.measuredAt = new Date().toISOString().slice(0, 16)
  dpDialogVisible.value = true
}

async function addDataPoint() {
  if (!dpForm.individualValues) {
    ElMessage.warning('请输入测量值（逗号分隔）')
    return
  }
  try {
    const values = dpForm.individualValues.split(',').map(v => v.trim()).filter(Boolean)
    await spcApi.createDataPoint({
      chartId: dpForm.chartId,
      subgroupIndex: dataPointsTotal.value + 1,
      individualValues: JSON.stringify(values.map(Number)),
      measuredAt: new Date(dpForm.measuredAt).toISOString(),
    })
    ElMessage.success('数据点已添加')
    dpDialogVisible.value = false
    await fetchDataPoints()
    await runAnalysis()
  } catch (e: any) {
    ElMessage.error('添加失败: ' + (e.message || ''))
  }
}

// ─── Alert Rules ───────────────────────────────────────

async function toggleRule(rule: SpcAlertRule) {
  try {
    await spcApi.updateAlertRule(rule.id, {
      enabled: !rule.enabled,
      triggerThreshold: rule.triggerThreshold,
      sigmaThreshold: rule.sigmaThreshold,
    })
    rule.enabled = !rule.enabled
    ElMessage.success(`规则 ${rule.ruleNumber} 已${rule.enabled ? '启用' : '禁用'}`)
  } catch (e: any) {
    ElMessage.error('更新失败: ' + (e.message || ''))
  }
}

async function saveRuleConfig() {
  try {
    for (const rule of alertRules.value) {
      await spcApi.updateAlertRule(rule.id, {
        enabled: rule.enabled,
        triggerThreshold: rule.triggerThreshold,
        sigmaThreshold: rule.sigmaThreshold,
      })
    }
    ElMessage.success('规则配置已保存')
    ruleConfigVisible.value = false
    await runAnalysis()
  } catch (e: any) {
    ElMessage.error('保存失败: ' + (e.message || ''))
  }
}

// ─── Triggers ──────────────────────────────────────────

async function resolveTrigger(id: number) {
  try {
    await spcApi.resolveTrigger(id)
    ElMessage.success('报警已标记为已处理')
    await fetchTriggers()
  } catch (e: any) {
    ElMessage.error('操作失败: ' + (e.message || ''))
  }
}

// ─── ANOVA ─────────────────────────────────────────────

async function runAnovaAnalysis() {
  if (!chartId.value) return
  anovaLoading.value = true
  try {
    const dps = dataPoints.value
    if (dps.length < 4) {
      ElMessage.warning('数据点不足，至少需要4个数据点进行方差分析')
      return
    }
    const midIdx = Math.floor(dps.length / 2)
    const earlyValues = dps.slice(0, midIdx).map(dp => Number(dp.subgroupMean || 0)).filter(v => v > 0)
    const lateValues = dps.slice(midIdx).map(dp => Number(dp.subgroupMean || 0)).filter(v => v > 0)
    if (earlyValues.length < 2 || lateValues.length < 2) {
      ElMessage.warning('分组后数据不足')
      return
    }
    const request: SpcAnovaRequest = {
      chartId: chartId.value,
      factors: [
        { source: 'method', label: '前半段', values: earlyValues },
        { source: 'method', label: '后半段', values: lateValues },
      ],
    }
    anovaResults.value = await spcApi.runAnova(request)
    ElMessage.success('方差分析完成')
  } catch (e: any) {
    ElMessage.error('方差分析失败: ' + (e.message || ''))
  } finally {
    anovaLoading.value = false
  }
}

// ─── Data Sources (贯通S3/S4/S5) ───────────────────────

async function fetchInspectionItems() {
  try {
    inspectionItemOptions.value = await inspectionItemApi.getSelectList()
  } catch { /* ignore */ }
}

function showAddDataSourceDialog() {
  dsForm.chartId = chartId.value
  dsForm.sourceType = 'IQC'
  dsForm.inspectionItemId = undefined
  fetchInspectionItems()
  dsDialogVisible.value = true
}

async function addDataSource() {
  try {
    dsForm.chartId = chartId.value
    await spcApi.createDataSource(dsForm)
    ElMessage.success('数据源添加成功')
    dsDialogVisible.value = false
    fetchDataSources()
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '添加失败')
  }
}

async function removeDataSource(id: number) {
  try {
    await spcApi.deleteDataSource(id)
    ElMessage.success('数据源已删除')
    fetchDataSources()
  } catch {
    ElMessage.error('删除失败')
  }
}

async function fetchBusinessData() {
  if (!chartId.value) return
  businessDataLoading.value = true
  try {
    const params: any = {}
    if (businessDateRange.value?.[0]) params.startDate = businessDateRange.value[0].toISOString()
    if (businessDateRange.value?.[1]) params.endDate = businessDateRange.value[1].toISOString()
    businessData.value = await spcApi.getBusinessData(chartId.value, params)
  } catch {
    ElMessage.error('拉取数据失败')
  } finally {
    businessDataLoading.value = false
  }
}

function onBusinessDataSelectionChange(rows: BusinessInspectionData[]) {
  selectedBusinessData.value = rows
}

async function importBusinessDataToChart() {
  if (!chartId.value || selectedBusinessData.value.length === 0) {
    ElMessage.warning('请先选择要导入的数据点')
    return
  }
  try {
    const values = selectedBusinessData.value
      .filter(d => d.measuredValue != null)
      .map(d => Number(d.measuredValue))
    if (values.length === 0) {
      ElMessage.warning('选中的数据没有有效的测量值')
      return
    }
    const subgroupSize = selectedChart.value?.subgroupSize || 5
    const batches = []
    for (let i = 0; i < values.length; i += subgroupSize) {
      batches.push({
        chartId: chartId.value,
        subgroupIndex: Math.floor(i / subgroupSize) + 1,
        individualValues: JSON.stringify(values.slice(i, i + subgroupSize)),
        measuredAt: new Date().toISOString(),
      })
    }
    await spcApi.batchCreateDataPoints({ chartId: chartId.value, dataPoints: batches })
    ElMessage.success(`成功导入 ${batches.length} 个子组数据`)
    fetchDataPoints()
    runAnalysis()
  } catch {
    ElMessage.error('导入失败')
  }
}

// ─── Navigation ────────────────────────────────────────

function goBack() {
  router.push('/spc/charts')
}

function goToRules() {
  router.push({ path: '/spc/rules', query: { chartId: chartId.value } })
}

function goToDataPoints() {
  router.push({ path: '/spc/data-points', query: { chartId: chartId.value } })
}

function goToAnova() {
  router.push({ path: '/spc/anova', query: { chartId: chartId.value } })
}

// ─── Watch ─────────────────────────────────────────────

watch(activeTab, (tab) => {
  if (tab === 'triggers') fetchTriggers()
  if (tab === 'anova') fetchAnovaResults()
  if (tab === 'datasources') fetchDataSources()
})

// ─── Lifecycle ─────────────────────────────────────────

onMounted(loadAll)
</script>

<template>
  <div class="spc-detail-page">
    <!-- Header -->
    <div class="detail-header">
      <div class="header-left">
        <el-button text @click="goBack">
          <el-icon><ArrowLeft /></el-icon>
          返回列表
        </el-button>
        <template v-if="selectedChart">
          <h2>
            <el-icon><DataAnalysis /></el-icon>
            {{ selectedChart.name }}
          </h2>
          <el-tag size="small" type="info" effect="plain">{{ CHART_TYPE_MAP[selectedChart.chartType] || selectedChart.chartType }}</el-tag>
          <el-tag size="small" effect="plain">n={{ selectedChart.subgroupSize }}</el-tag>
          <span v-if="selectedChart.usl != null" class="spec-badge">USL={{ selectedChart.usl }}</span>
          <span v-if="selectedChart.lsl != null" class="spec-badge">LSL={{ selectedChart.lsl }}</span>
        </template>
      </div>
      <div class="header-right">
        <el-button size="small" @click="goToRules">判异规则</el-button>
        <el-button size="small" @click="goToDataPoints">数据点</el-button>
        <el-button size="small" @click="goToAnova">方差分析</el-button>
        <el-button size="small" @click="openAddDataPoint" type="success">添加数据</el-button>
        <el-button size="small" @click="runAnalysis" :loading="analysisLoading" type="primary">重新分析</el-button>
      </div>
    </div>

    <div v-loading="loading" class="detail-body">
      <!-- CPK Cards -->
      <div v-if="analysisReport?.capability" class="cpk-cards">
        <el-card class="cpk-card" :class="cpkGradeType(analysisReport.capability.grade)">
          <div class="cpk-label">
            Cpk
            <el-tooltip placement="top" popper-class="cpk-tooltip">
              <template #content>
                <div class="tip-title">Cpk — 过程能力指数</div>
                <div class="tip-desc">衡量过程满足规格要求的实际能力，考虑过程均值与目标值的偏移</div>
                <div class="tip-formula">Cpk = min((USL − μ)/3σ, (μ − LSL)/3σ)</div>
                <div class="tip-criteria">≥1.67 优秀 | ≥1.33 良好 | ≥1.0 临界 | &lt;1.0 不足</div>
              </template>
              <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
            </el-tooltip>
          </div>
          <div class="cpk-value">{{ formatNumber(analysisReport.capability.cpk, 4) }}</div>
          <el-tag :type="cpkGradeType(analysisReport.capability.grade)" size="small">{{ analysisReport.capability.grade }}</el-tag>
        </el-card>
        <el-card class="cpk-card">
          <div class="cpk-label">
            Cp
            <el-tooltip placement="top" popper-class="cpk-tooltip">
              <template #content>
                <div class="tip-title">Cp — 过程能力指数（无偏移）</div>
                <div class="tip-desc">仅考虑过程固有变异，不考虑均值偏移</div>
                <div class="tip-formula">Cp = (USL − LSL) / (6 × σ<sub>组内</sub>)</div>
              </template>
              <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
            </el-tooltip>
          </div>
          <div class="cpk-value">{{ formatNumber(analysisReport.capability.cp, 4) }}</div>
        </el-card>
        <el-card class="cpk-card">
          <div class="cpk-label">
            Ppk
            <el-tooltip placement="top" popper-class="cpk-tooltip">
              <template #content>
                <div class="tip-title">Ppk — 过程性能指数</div>
                <div class="tip-desc">使用总标准差，反映过程长期稳定性</div>
              </template>
              <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
            </el-tooltip>
          </div>
          <div class="cpk-value">{{ formatNumber(analysisReport.capability.ppk, 4) }}</div>
        </el-card>
        <el-card class="cpk-card">
          <div class="cpk-label">σ (组内)</div>
          <div class="cpk-value">{{ formatNumber(analysisReport.capability.sigmaWithin, 6) }}</div>
        </el-card>
        <el-card class="cpk-card">
          <div class="cpk-label">
            DPMO
            <el-tooltip placement="top" popper-class="cpk-tooltip">
              <template #content>
                <div class="tip-title">DPMO — 百万机会缺陷数</div>
                <div class="tip-desc">每百万个产品/机会中的缺陷数</div>
                <div class="tip-criteria">6σ ≈ 3.4 ppm | 5σ ≈ 233 ppm</div>
              </template>
              <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
            </el-tooltip>
          </div>
          <div class="cpk-value">{{ formatNumber(analysisReport.capability.estimatedPpm, 0) }}</div>
          <div class="cpk-unit">ppm</div>
        </el-card>
      </div>

      <!-- Tabs -->
      <el-tabs v-model="activeTab" class="spc-tabs" type="border-card">
        <!-- Tab 1: Control Chart -->
        <el-tab-pane name="chart">
          <template #label><el-icon><DataLine /></el-icon><span>控制图</span></template>
          <div v-loading="analysisLoading">
            <div class="charts-row">
              <div ref="chart1Ref" class="chart-box" style="flex:1; height:320px;"></div>
              <div ref="chart2Ref" class="chart-box" style="flex:1; height:320px;"></div>
            </div>

            <el-descriptions v-if="analysisReport?.controlLimits" title="控制限参数" :column="6" size="small" border class="limits-table">
              <el-descriptions-item label="X̄ CL">{{ formatNumber(analysisReport.controlLimits.clXbar) }}</el-descriptions-item>
              <el-descriptions-item label="X̄ UCL">{{ formatNumber(analysisReport.controlLimits.uclXbar) }}</el-descriptions-item>
              <el-descriptions-item label="X̄ LCL">{{ formatNumber(analysisReport.controlLimits.lclXbar) }}</el-descriptions-item>
              <el-descriptions-item label="R CL">{{ formatNumber(analysisReport.controlLimits.clR) }}</el-descriptions-item>
              <el-descriptions-item label="R UCL">{{ formatNumber(analysisReport.controlLimits.uclR) }}</el-descriptions-item>
              <el-descriptions-item label="R LCL">{{ formatNumber(analysisReport.controlLimits.lclR) }}</el-descriptions-item>
            </el-descriptions>

            <div class="section-title">
              <span>数据点列表</span>
              <div class="section-actions">
                <el-button size="small" text @click="goToDataPoints">更多操作</el-button>
                <el-button size="small" text @click="fetchDataPoints">刷新</el-button>
              </div>
            </div>
            <el-table :data="dataPoints" stripe size="small" max-height="200">
              <el-table-column prop="subgroupIndex" label="子组#" width="70" />
              <el-table-column label="测量值" min-width="200">
                <template #default="{ row }">{{ row.individualValues }}</template>
              </el-table-column>
              <el-table-column label="X̄" width="100" align="right">
                <template #default="{ row }">{{ formatNumber(Number(row.subgroupMean), 4) }}</template>
              </el-table-column>
              <el-table-column label="R" width="100" align="right">
                <template #default="{ row }">{{ formatNumber(Number(row.subgroupRange), 4) }}</template>
              </el-table-column>
              <el-table-column label="测量时间" width="160">
                <template #default="{ row }">{{ formatDate(row.measuredAt) }}</template>
              </el-table-column>
              <el-table-column label="操作" width="60" fixed="right">
                <template #default="{ row }">
                  <el-button size="small" text type="danger" @click="spcApi.deleteDataPoint(row.id).then(() => { fetchDataPoints(); runAnalysis() })">删除</el-button>
                </template>
              </el-table-column>
            </el-table>
            <div class="pagination-row">
              <el-pagination v-model:current-page="dpQuery.page" v-model:page-size="dpQuery.pageSize" :total="dataPointsTotal" small layout="total, prev, pager, next" @current-change="fetchDataPoints" />
            </div>
          </div>
        </el-tab-pane>

        <!-- Tab 2: Western Electric Rules -->
        <el-tab-pane name="rules">
          <template #label><el-icon><WarningFilled /></el-icon><span>判异规则</span></template>
          <div class="rules-section">
            <div class="rules-header">
              <h3>Western Electric 8大判异规则</h3>
              <div class="rules-header-actions">
                <el-button size="small" text @click="goToRules">独立页面</el-button>
                <el-button size="small" type="primary" @click="ruleConfigVisible = true">配置规则</el-button>
              </div>
            </div>
            <div v-if="analysisReport?.violations?.length" class="violation-alert">
              <el-alert title="检测到判异" :description="`共 ${analysisReport.violations.length} 条违规`" type="warning" show-icon :closable="false" />
            </div>
            <div class="rules-grid">
              <div v-for="rule in alertRules" :key="rule.id" class="rule-item">
                <div class="rule-header">
                  <span class="rule-num">#{{ rule.ruleNumber }}</span>
                  <el-switch :model-value="rule.enabled" size="small" @change="toggleRule(rule)" />
                </div>
                <div class="rule-name">{{ rule.ruleName }}</div>
                <div class="rule-desc">{{ rule.ruleDescription }}</div>
                <div class="rule-meta">
                  <span>阈值: {{ rule.triggerThreshold }}点</span>
                  <span v-if="rule.sigmaThreshold > 0">σ: {{ rule.sigmaThreshold }}</span>
                </div>
                <div v-if="violationCount(analysisReport?.violations, rule.ruleNumber) > 0" class="rule-violation">
                  <el-tag size="small" type="danger">触发 {{ violationCount(analysisReport?.violations, rule.ruleNumber) }} 次</el-tag>
                </div>
                <div v-else class="rule-safe">
                  <el-tag size="small" type="success">未触发</el-tag>
                </div>
              </div>
            </div>
          </div>
        </el-tab-pane>

        <!-- Tab 3: Violation Triggers -->
        <el-tab-pane name="triggers">
          <template #label><el-icon><Bell /></el-icon><span>报警记录</span></template>
          <el-table :data="triggers" stripe size="small">
            <el-table-column label="规则" width="60">
              <template #default="{ row }">#{{ row.ruleNumber }}</template>
            </el-table-column>
            <el-table-column prop="ruleName" label="规则名称" min-width="160" />
            <el-table-column label="违规点" width="100">
              <template #default="{ row }">点 #{{ row.violatedPointIndex + 1 }}</template>
            </el-table-column>
            <el-table-column label="详情" min-width="200">
              <template #default="{ row }">{{ row.detail ? (JSON.parse(row.detail).description || row.detail) : '-' }}</template>
            </el-table-column>
            <el-table-column label="触发时间" width="160">
              <template #default="{ row }">{{ formatDate(row.triggeredAt) }}</template>
            </el-table-column>
            <el-table-column label="状态" width="80">
              <template #default="{ row }">
                <el-tag :type="row.resolved ? 'success' : 'danger'" size="small">{{ row.resolved ? '已处理' : '待处理' }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="100" fixed="right">
              <template #default="{ row }">
                <el-button v-if="!row.resolved" size="small" text type="primary" @click="resolveTrigger(row.id)">标记已处理</el-button>
              </template>
            </el-table-column>
          </el-table>
          <div class="pagination-row">
            <el-pagination v-model:current-page="triggerQuery.page" v-model:page-size="triggerQuery.pageSize" :total="triggersTotal" small layout="total, prev, pager, next" @current-change="fetchTriggers" />
          </div>
        </el-tab-pane>

        <!-- Tab 4: ANOVA -->
        <el-tab-pane name="anova">
          <template #label><el-icon><Document /></el-icon><span>方差分析</span></template>
          <div v-loading="anovaLoading">
            <div class="anova-header-bar" v-if="anovaResults.length > 0">
              <span class="text-sm text-gray-400">点击下方按钮运行方差分析，或查看已有结果</span>
              <div class="anova-header-actions">
                <el-button size="small" text @click="goToAnova">独立页面</el-button>
                <el-button size="small" type="primary" @click="runAnovaAnalysis">运行方差分析</el-button>
              </div>
            </div>
            <div v-if="anovaResults.length === 0" class="empty-anova">
              <el-empty description="暂无方差分析结果">
                <el-button type="primary" @click="runAnovaAnalysis">运行方差分析</el-button>
              </el-empty>
            </div>
            <div v-for="result in anovaResults" :key="result.id" class="anova-item">
              <el-card>
                <template #header>
                  <div class="anova-header">
                    <strong>{{ anovaSourceLabel(result.source) }}</strong>
                    <el-tag :type="result.significant ? 'danger' : 'success'" size="small">
                      {{ result.significant ? '显著影响' : '无显著影响' }}
                    </el-tag>
                  </div>
                </template>
                <el-descriptions :column="4" size="small" border>
                  <el-descriptions-item label="平方和(SS)">{{ formatNumber(result.sumOfSquares, 2) }}</el-descriptions-item>
                  <el-descriptions-item label="自由度(df)">{{ result.degreesFreedom }}</el-descriptions-item>
                  <el-descriptions-item label="均方(MS)">{{ formatNumber(result.meanSquare, 2) }}</el-descriptions-item>
                  <el-descriptions-item label="F值">{{ formatNumber(result.fRatio, 4) }}</el-descriptions-item>
                  <el-descriptions-item label="P值">{{ formatNumber(result.pValue, 6) }}</el-descriptions-item>
                  <el-descriptions-item label="显著性">
                    <el-tag :type="result.pValue < 0.05 ? 'danger' : 'success'" size="small">
                      {{ result.pValue < 0.05 ? 'p &lt; 0.05' : 'p ≥ 0.05' }}
                    </el-tag>
                  </el-descriptions-item>
                </el-descriptions>
              </el-card>
            </div>
          </div>
        </el-tab-pane>

        <!-- Tab 5: Data Sources -->
        <el-tab-pane name="datasources">
          <template #label><el-icon><Connection /></el-icon><span>数据源</span></template>
          <div>
            <div class="datasource-header">
              <span class="text-sm text-gray-400">配置SPC从IQC/IPQC/FQC业务模块自动拉取检验数据</span>
              <el-button type="primary" size="small" @click="showAddDataSourceDialog">+ 添加数据源</el-button>
            </div>
            <el-table :data="dataSources" stripe size="small" class="mt-3">
              <el-table-column label="数据源类型" width="160">
                <template #default="{ row }"><el-tag>{{ sourceTypeLabel(row.sourceType) }}</el-tag></template>
              </el-table-column>
              <el-table-column label="检验项目" min-width="150">
                <template #default="{ row }">{{ row.inspectionItemName || '全部项目' }}</template>
              </el-table-column>
              <el-table-column label="操作" width="80">
                <template #default="{ row }">
                  <el-button size="small" type="danger" text @click="removeDataSource(row.id)">删除</el-button>
                </template>
              </el-table-column>
            </el-table>
            <el-divider />
            <div class="flex items-center justify-between mb-3">
              <strong>业务数据预览</strong>
              <div class="flex gap-2">
                <el-date-picker v-model="businessDateRange" type="daterange" range-separator="至" start-placeholder="开始日期" end-placeholder="结束日期" size="small" />
                <el-button size="small" type="primary" @click="fetchBusinessData" :loading="businessDataLoading">拉取数据</el-button>
                <el-button size="small" @click="importBusinessDataToChart">导入到控制图</el-button>
              </div>
            </div>
            <el-table :data="businessData" stripe size="small" max-height="300" v-loading="businessDataLoading" @selection-change="onBusinessDataSelectionChange">
              <el-table-column type="selection" width="40" />
              <el-table-column label="来源" width="80">
                <template #default="{ row }">{{ row.sourceType }}</template>
              </el-table-column>
              <el-table-column prop="sourceNo" label="单据号" width="140" />
              <el-table-column prop="inspectionItemName" label="检验项目" min-width="120" />
              <el-table-column label="测量值" width="100" align="right">
                <template #default="{ row }">{{ row.measuredValue ?? '-' }}</template>
              </el-table-column>
              <el-table-column label="结果" width="70">
                <template #default="{ row }">
                  <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">{{ row.result }}</el-tag>
                </template>
              </el-table-column>
              <el-table-column label="检验时间" width="160">
                <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
              </el-table-column>
            </el-table>
            <el-empty v-if="businessData.length === 0 && !businessDataLoading" description="点击「拉取数据」从业务模块获取检验数据" />
          </div>
        </el-tab-pane>
      </el-tabs>
    </div>

    <!-- Add Data Point Dialog -->
    <el-dialog v-model="dpDialogVisible" title="添加数据点" width="450px" :close-on-click-modal="false">
      <el-form :model="dpForm" label-width="120px" size="small">
        <el-form-item label="测量值" required>
          <el-input v-model="dpForm.individualValues" placeholder="如: 10.01, 10.02, 9.99, 10.00, 10.01" />
          <div class="form-tip">逗号分隔的数值，数量应与子组大小一致</div>
        </el-form-item>
        <el-form-item label="测量时间">
          <el-date-picker v-model="dpForm.measuredAt" type="datetime" placeholder="选择时间" style="width:100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dpDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="addDataPoint">添加</el-button>
      </template>
    </el-dialog>

    <!-- Data Source Dialog -->
    <el-dialog v-model="dsDialogVisible" title="添加数据源" width="500px">
      <el-form label-width="100px">
        <el-form-item label="数据源类型">
          <el-select v-model="dsForm.sourceType" style="width: 100%">
            <el-option v-for="o in SOURCE_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="检验项目">
          <el-select v-model="dsForm.inspectionItemId" placeholder="不选则取全部" clearable filterable style="width: 100%">
            <el-option v-for="item in inspectionItemOptions" :key="item.id" :label="`${item.itemCode} - ${item.itemName}`" :value="item.id" />
          </el-select>
        </el-form-item>
        <p class="text-gray-400 text-sm">提示：不选择检验项目则拉取该业务模块的所有检验数据</p>
      </el-form>
      <template #footer>
        <el-button @click="dsDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="addDataSource">添加</el-button>
      </template>
    </el-dialog>

    <!-- Rule Config Dialog -->
    <el-dialog v-model="ruleConfigVisible" title="判异规则配置" width="650px">
      <el-table :data="alertRules" stripe size="small">
        <el-table-column label="规则" width="50">
          <template #default="{ row }">#{{ row.ruleNumber }}</template>
        </el-table-column>
        <el-table-column prop="ruleName" label="规则名称" min-width="150" />
        <el-table-column label="启用" width="60">
          <template #default="{ row }">
            <el-switch v-model="row.enabled" size="small" />
          </template>
        </el-table-column>
        <el-table-column label="连续点数(N)" width="110">
          <template #default="{ row }">
            <el-input-number v-model="row.triggerThreshold" :min="1" :max="25" size="small" controls-position="right" style="width:90px" />
          </template>
        </el-table-column>
        <el-table-column label="σ阈值" width="100">
          <template #default="{ row }">
            <el-input-number v-model="row.sigmaThreshold" :min="0" :max="5" :step="0.5" size="small" controls-position="right" style="width:80px" />
          </template>
        </el-table-column>
      </el-table>
      <template #footer>
        <el-button @click="ruleConfigVisible = false">取消</el-button>
        <el-button type="primary" @click="saveRuleConfig">保存配置</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.spc-detail-page {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 16px;
  border-bottom: 1px solid var(--el-border-color-light, #e4e7ed);
  flex-shrink: 0;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-left h2 {
  margin: 0;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.header-right {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.spec-badge {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  background: var(--el-fill-color, #f0f2f5);
  padding: 2px 6px;
  border-radius: 4px;
}

.detail-body {
  flex: 1;
  overflow-y: auto;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* Tabs */
.spc-tabs {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.spc-tabs :deep(.el-tabs__content) {
  flex: 1;
  overflow-y: auto;
}

.spc-tabs :deep(.el-tabs__item) {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

/* CPK Cards */
.cpk-cards {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.cpk-card {
  flex: 1;
  min-width: 120px;
  text-align: center;
}

.cpk-card :deep(.el-card__body) {
  padding: 12px;
}

.cpk-label {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  margin-bottom: 4px;
  display: inline-flex;
  align-items: center;
  gap: 2px;
}

.cpk-help-icon {
  font-size: 12px;
  color: var(--el-color-info, #909399);
  cursor: help;
  transition: color 0.2s;
}

.cpk-help-icon:hover {
  color: var(--el-color-primary, #409eff);
}

:global(.cpk-tooltip) {
  max-width: 360px;
  line-height: 1.6;
  font-size: 12px;
}

:global(.cpk-tooltip .tip-title) {
  font-weight: 600;
  font-size: 13px;
  color: #303133;
  margin-bottom: 4px;
}

:global(.cpk-tooltip .tip-desc) {
  color: #606266;
  margin-bottom: 4px;
}

:global(.cpk-tooltip .tip-formula) {
  color: #409eff;
  font-family: 'Courier New', monospace;
  background: #ecf5ff;
  padding: 2px 6px;
  border-radius: 3px;
  margin-bottom: 4px;
  font-size: 12px;
}

:global(.cpk-tooltip .tip-criteria) {
  color: #909399;
  font-size: 11px;
}

.cpk-value {
  font-size: 20px;
  font-weight: 700;
  margin-bottom: 4px;
}

.cpk-unit {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
}

/* Charts */
.charts-row {
  display: flex;
  gap: 12px;
  margin-bottom: 12px;
}

.chart-box {
  border: 1px solid var(--el-border-color-light, #e4e7ed);
  border-radius: 4px;
  background: var(--el-bg-color, #fff);
}

/* Sections */
.section-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
  font-size: 13px;
  margin: 8px 0 4px;
}

.section-actions {
  display: flex;
  gap: 4px;
}

.limits-table {
  margin-bottom: 12px;
}

.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: 8px 0;
}

/* Rules */
.rules-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.rules-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.rules-header h3 {
  margin: 0;
  font-size: 15px;
}

.rules-header-actions {
  display: flex;
  gap: 6px;
}

.rules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 10px;
}

.rule-item {
  border: 1px solid var(--el-border-color-light, #e4e7ed);
  border-radius: 8px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.rule-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.rule-num {
  font-weight: 700;
  color: var(--el-color-primary, #409eff);
  font-size: 14px;
}

.rule-name {
  font-weight: 500;
  font-size: 13px;
}

.rule-desc {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
}

.rule-meta {
  display: flex;
  gap: 12px;
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
}

.rule-violation, .rule-safe {
  margin-top: auto;
}

.violation-alert {
  margin-bottom: 8px;
}

/* ANOVA */
.anova-item {
  margin-bottom: 12px;
}

.anova-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.anova-header-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.anova-header-actions {
  display: flex;
  gap: 6px;
}

.empty-anova {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}

/* Data Sources */
.datasource-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Utilities */
.form-tip {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  margin-top: 4px;
}

.text-sm { font-size: 13px; }
.text-gray-400 { color: #909399; }
.mt-3 { margin-top: 12px; }
.mb-3 { margin-bottom: 12px; }
.flex { display: flex; }
.items-center { align-items: center; }
.justify-between { justify-content: space-between; }
.gap-2 { gap: 8px; }
</style>

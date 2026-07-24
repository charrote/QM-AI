<script setup lang="ts">
import { ref, onMounted, reactive, computed, nextTick, watch, onUnmounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, Search, MoreFilled, Edit, Delete, Refresh, Setting, Clock, TrendCharts, CircleCheck, Top, Document, Warning, InfoFilled } from '@element-plus/icons-vue'
import { spcApi } from '@/api/spc'
import { inspectionItemApi } from '@/api/inspectionItem'
import type { SpcControlChart, SpcControlChartDetail, CreateSpcControlChart, UpdateSpcControlChart } from '@/types/spc'
import type { SpcDataPoint, SpcAnalysisReport, SpcRuleViolation, SpcAlertRule, UpdateSpcAlertRule } from '@/types/spc'
import type { SpcAnovaResult, SpcAnovaRequest, SpcAnovaFactor, SpcDataSource, CreateSpcDataSource, BusinessInspectionData } from '@/types/spc'
import { SOURCE_TYPE_OPTIONS } from '@/types/spc'
import type { InspectionItem } from '@/types/inspectionItem'
import type { PagedRequest } from '@/types/basicData'
import {
  CHART_TYPE_OPTIONS, CHART_TYPE_MAP,
  CPK_GRADE_TYPE, ANOVA_SOURCE_MAP, ANOVA_SOURCE_OPTIONS,
  WESTERN_ELECTRIC_RULES
} from '@/types/spc'

defineOptions({ name: 'SPC' })

// ═════════════════════════════════════════════════════════════════
//  State
// ═════════════════════════════════════════════════════════════════

const activeTab = ref('chart')
const chartsLoading = ref(false)
const analysisLoading = ref(false)

// Chart list
const charts = ref<SpcControlChart[]>([])
const chartsTotal = ref(0)
const chartQuery = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '' })

// Selected chart
const selectedChartId = ref<number | null>(null)
const selectedChart = ref<SpcControlChartDetail | null>(null)

// Chart CRUD dialog
const chartDialogVisible = ref(false)
const chartDialogTitle = ref('')
const isEdit = ref(false)
const chartForm = reactive<CreateSpcControlChart>({
  name: '',
  processId: 0,
  parameterCode: '',
  chartType: 'Xbar_R',
  subgroupSize: 5,
  usl: undefined,
  lsl: undefined,
  targetValue: undefined,
})

// Data points (shown in chart table)
const dataPoints = ref<SpcDataPoint[]>([])
const dataPointsTotal = ref(0)
const dpQuery = reactive<PagedRequest>({ page: 1, pageSize: 50 })

// Add data point dialog
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
const anovaLoading = ref(false)

// Data Sources (贯通S3/S4/S5)
const dataSources = ref<SpcDataSource[]>([])
const businessData = ref<BusinessInspectionData[]>([])
const businessDataLoading = ref(false)
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

// ─── ECharts refs ─────────────────────────────────────────

const xbarChartRef = ref<HTMLElement>()
const rChartRef = ref<HTMLElement>()
let xbarChartInstance: any = null
let rChartInstance: any = null

function resizeHandlerXbar() {
  xbarChartInstance?.resize?.()
}

function resizeHandlerR() {
  rChartInstance?.resize?.()
}

function safeJsonParse(str: string): any {
  try {
    return JSON.parse(str)
  } catch {
    return null
  }
}

function cleanupCharts() {
  if (xbarChartInstance) {
    window.removeEventListener('resize', resizeHandlerXbar)
    xbarChartInstance.dispose()
    xbarChartInstance = null
  }
  if (rChartInstance) {
    window.removeEventListener('resize', resizeHandlerR)
    rChartInstance.dispose()
    rChartInstance = null
  }
}

onUnmounted(() => {
  cleanupCharts()
})

// ═════════════════════════════════════════════════════════════════
//  Computed
// ═════════════════════════════════════════════════════════════════

const hasChart = computed(() => selectedChartId.value !== null && selectedChartId.value > 0)

const xbarData = computed(() => {
  if (!analysisReport.value?.dataPoints) return []
  return analysisReport.value.dataPoints
    .filter(dp => dp.subgroupMean != null)
    .map(dp => ({
      subgroup: dp.subgroupIndex,
      value: Number(dp.subgroupMean),
    }))
})

const rData = computed(() => {
  if (!analysisReport.value?.dataPoints) return []
  return analysisReport.value.dataPoints
    .filter(dp => dp.subgroupRange != null)
    .map(dp => ({
      subgroup: dp.subgroupIndex,
      value: Number(dp.subgroupRange),
    }))
})

// ═════════════════════════════════════════════════════════════════
//  Methods — Chart CRUD
// ═════════════════════════════════════════════════════════════════

async function fetchCharts() {
  chartsLoading.value = true
  try {
    const res = await spcApi.listCharts({ ...chartQuery })
    charts.value = res.items
    chartsTotal.value = res.total
  } catch (e: any) {
    ElMessage.error('获取SPC控制图列表失败: ' + (e.message || ''))
  } finally {
    chartsLoading.value = false
  }
}

async function selectChart(id: number) {
  selectedChartId.value = id
  try {
    selectedChart.value = await spcApi.getChart(id)
    dpQuery.page = 1
    await fetchDataPoints()
    await runAnalysis()
    await fetchAlertRules()
    await fetchTriggers()
  } catch (e: any) {
    ElMessage.error('获取控制图详情失败: ' + (e.message || ''))
  }
}

function openCreateChart() {
  isEdit.value = false
  chartDialogTitle.value = '新建SPC控制图'
  chartForm.name = ''
  chartForm.processId = 0
  chartForm.parameterCode = ''
  chartForm.chartType = 'Xbar_R'
  chartForm.subgroupSize = 5
  chartForm.usl = undefined
  chartForm.lsl = undefined
  chartForm.targetValue = undefined
  chartDialogVisible.value = true
}

function openEditChart() {
  if (!selectedChart.value) return
  isEdit.value = true
  chartDialogTitle.value = '编辑SPC控制图'
  chartForm.name = selectedChart.value.name
  chartForm.processId = selectedChart.value.processId
  chartForm.parameterCode = selectedChart.value.parameterCode
  chartForm.chartType = selectedChart.value.chartType
  chartForm.subgroupSize = selectedChart.value.subgroupSize
  chartForm.usl = selectedChart.value.usl
  chartForm.lsl = selectedChart.value.lsl
  chartForm.targetValue = selectedChart.value.targetValue
  chartDialogVisible.value = true
}

async function saveChart() {
  if (!chartForm.name || !chartForm.parameterCode) {
    ElMessage.warning('请填写控制图名称和参数代码')
    return
  }
  try {
    if (isEdit.value && selectedChart.value) {
      await spcApi.updateChart(selectedChart.value.id, chartForm as UpdateSpcControlChart)
      ElMessage.success('控制图已更新')
    } else {
      const result = await spcApi.createChart(chartForm)
      ElMessage.success('控制图已创建')
      selectedChartId.value = result.id
      selectedChart.value = result
    }
    chartDialogVisible.value = false
    await fetchCharts()
    if (selectedChartId.value) {
      await selectChart(selectedChartId.value)
    }
  } catch (e: any) {
    ElMessage.error('保存失败: ' + (e.message || ''))
  }
}

async function deleteChart(id: number) {
  try {
    await ElMessageBox.confirm('确定删除此控制图及其所有数据？', '确认删除', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning',
    })
    await spcApi.deleteChart(id)
    ElMessage.success('控制图已删除')
    if (selectedChartId.value === id) {
      selectedChartId.value = null
      selectedChart.value = null
      analysisReport.value = null
    }
    await fetchCharts()
  } catch (e: any) {
    if (e !== 'cancel') {
      ElMessage.error('删除失败: ' + (e.message || ''))
    }
  }
}

// ═════════════════════════════════════════════════════════════════
//  Methods — Data Points
// ═════════════════════════════════════════════════════════════════

async function fetchDataPoints() {
  if (!selectedChartId.value) return
  try {
    const res = await spcApi.listDataPoints(selectedChartId.value, { ...dpQuery })
    dataPoints.value = res.items
    dataPointsTotal.value = res.total
  } catch {
    // ignore
  }
}

function openAddDataPoint() {
  dpForm.chartId = selectedChartId.value || 0
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
    const jsonValues = JSON.stringify(values.map(Number))

    await spcApi.createDataPoint({
      chartId: dpForm.chartId,
      subgroupIndex: dataPointsTotal.value + 1,
      individualValues: jsonValues,
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

// ═════════════════════════════════════════════════════════════════
//  Methods — Analysis Engine
// ═════════════════════════════════════════════════════════════════

async function runAnalysis() {
  if (!selectedChartId.value) return
  analysisLoading.value = true
  try {
    const report = await spcApi.analyzeChart({ chartId: selectedChartId.value })
    analysisReport.value = report
    await nextTick()
    renderCharts()
  } catch (e: any) {
    ElMessage.error('SPC分析失败: ' + (e.message || ''))
  } finally {
    analysisLoading.value = false
  }
}

// ═════════════════════════════════════════════════════════════════
//  Methods — ECharts Rendering
// ═════════════════════════════════════════════════════════════════

function renderCharts() {
  if (!analysisReport.value?.controlLimits || !analysisReport.value?.dataPoints) return

  const limits = analysisReport.value.controlLimits
  const dps = analysisReport.value.dataPoints

  // X̄ chart data
  const xbarCategories = dps.map(dp => `#${dp.subgroupIndex}`)
  const xbarValues = dps.map(dp => Number(dp.subgroupMean || 0))

  // R chart data
  const rValues = dps.map(dp => Number(dp.subgroupRange || 0))

  renderXbarChart(xbarCategories, xbarValues, limits)
  renderRChart(xbarCategories, rValues, limits)
}

function renderXbarChart(categories: string[], values: number[], limits: any) {
  if (!xbarChartRef.value) return

  // Dynamic import ECharts
  import('echarts').then(echarts => {
    if (xbarChartInstance) xbarChartInstance.dispose()
    xbarChartInstance = echarts.init(xbarChartRef.value!)

    // Mark violation points
    const violations = analysisReport.value?.violations || []
    const violationIndices = new Set(violations.map(v => v.index))

    const markAreas: any[] = []
    const markPoints: any[] = []

    // Mark violation points
    values.forEach((v, i) => {
      if (violationIndices.has(i)) {
        markPoints.push({
          name: '违规点',
          value: v,
          xAxis: i,
          yAxis: v,
          symbol: 'circle',
          symbolSize: 12,
          itemStyle: { color: '#f56c6c', borderColor: '#f56c6c', borderWidth: 2 },
          label: { show: true, formatter: '违规', fontSize: 12, color: '#f56c6c', position: 'top' },
        })
      }
    })

    const option = {
      title: { text: 'X̄ 均值控制图', left: 'center', textStyle: { fontSize: 14 } },
      tooltip: { trigger: 'axis', formatter: (params: any) => `子组: ${params[0].axisValue}<br/>均值: ${params[0].value?.toFixed(4)}` },
      legend: { data: ['X̄', 'CL', 'UCL', 'LCL'], bottom: 0 },
      grid: { left: 60, right: 20, top: 40, bottom: 40 },
      xAxis: { type: 'category', data: categories, axisLabel: { rotate: 45, fontSize: 10 } },
      yAxis: { type: 'value', name: '均值', nameTextStyle: { fontSize: 11 } },
      series: [
        {
          name: 'X̄',
          type: 'line',
          data: values,
          smooth: false,
          symbol: 'circle',
          symbolSize: 6,
          lineStyle: { color: '#409eff', width: 2 },
          itemStyle: { color: '#409eff' },
          markPoint: { data: markPoints },
          markLine: {
            silent: true,
            data: [
              { name: 'UCL', yAxis: limits.uclXbar, lineStyle: { color: '#f56c6c', type: 'dashed' }, label: { formatter: `UCL=${limits.uclXbar?.toFixed(4)}` } },
              { name: 'CL', yAxis: limits.clXbar, lineStyle: { color: '#67c23a', type: 'solid' }, label: { formatter: `CL=${limits.clXbar?.toFixed(4)}` } },
              { name: 'LCL', yAxis: limits.lclXbar, lineStyle: { color: '#f56c6c', type: 'dashed' }, label: { formatter: `LCL=${limits.lclXbar?.toFixed(4)}` } },
            ],
          },
        },
      ],
    }

    xbarChartInstance.setOption(option)
    window.removeEventListener('resize', resizeHandlerXbar)
    window.addEventListener('resize', resizeHandlerXbar)
  })
}

function renderRChart(categories: string[], values: number[], limits: any) {
  if (!rChartRef.value) return

  import('echarts').then(echarts => {
    if (rChartInstance) rChartInstance.dispose()
    rChartInstance = echarts.init(rChartRef.value!)

    const option = {
      title: { text: 'R 极差控制图', left: 'center', textStyle: { fontSize: 14 } },
      tooltip: { trigger: 'axis', formatter: (params: any) => `子组: ${params[0].axisValue}<br/>极差: ${params[0].value?.toFixed(4)}` },
      legend: { data: ['R', 'CL', 'UCL', 'LCL'], bottom: 0 },
      grid: { left: 60, right: 20, top: 40, bottom: 40 },
      xAxis: { type: 'category', data: categories, axisLabel: { rotate: 45, fontSize: 10 } },
      yAxis: { type: 'value', name: '极差', nameTextStyle: { fontSize: 11 } },
      series: [
        {
          name: 'R',
          type: 'line',
          data: values,
          smooth: false,
          symbol: 'circle',
          symbolSize: 6,
          lineStyle: { color: '#e6a23c', width: 2 },
          itemStyle: { color: '#e6a23c' },
          markLine: {
            silent: true,
            data: [
              { name: 'UCL', yAxis: limits.uclR, lineStyle: { color: '#f56c6c', type: 'dashed' }, label: { formatter: `UCL=${limits.uclR?.toFixed(4)}` } },
              { name: 'CL', yAxis: limits.clR, lineStyle: { color: '#67c23a', type: 'solid' }, label: { formatter: `CL=${limits.clR?.toFixed(4)}` } },
              { name: 'LCL', yAxis: limits.lclR, lineStyle: { color: '#f56c6c', type: 'dashed' }, label: { formatter: `LCL=${limits.lclR?.toFixed(4)}` } },
            ],
          },
        },
      ],
    }

    rChartInstance.setOption(option)
    window.removeEventListener('resize', resizeHandlerR)
    window.addEventListener('resize', resizeHandlerR)
  })
}

// ═════════════════════════════════════════════════════════════════
//  Methods — Alert Rules
// ═════════════════════════════════════════════════════════════════

async function fetchAlertRules() {
  if (!selectedChartId.value) return
  try {
    alertRules.value = await spcApi.listAlertRules(selectedChartId.value)
  } catch {
    // ignore
  }
}

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

// ═════════════════════════════════════════════════════════════════
//  Methods — Triggers
// ═════════════════════════════════════════════════════════════════

async function fetchTriggers() {
  if (!selectedChartId.value) return
  try {
    const res = await spcApi.listTriggers(selectedChartId.value, { ...triggerQuery })
    triggers.value = res.items as any
    triggersTotal.value = res.total
  } catch {
    // ignore
  }
}

async function resolveTrigger(id: number) {
  try {
    await spcApi.resolveTrigger(id)
    ElMessage.success('报警已标记为已处理')
    await fetchTriggers()
  } catch (e: any) {
    ElMessage.error('操作失败: ' + (e.message || ''))
  }
}

// ═════════════════════════════════════════════════════════════════
//  Methods — ANOVA
// ═════════════════════════════════════════════════════════════════

async function fetchAnovaResults() {
  if (!selectedChartId.value) return
  try {
    anovaResults.value = await spcApi.listAnovaResults(selectedChartId.value)
  } catch {
    // ignore
  }
}

async function runAnovaAnalysis() {
  if (!selectedChartId.value) return
  anovaLoading.value = true
  try {
    // Build sample factor data from data points
    const dps = dataPoints.value
    if (dps.length < 4) {
      ElMessage.warning('数据点不足，至少需要4个数据点进行方差分析')
      return
    }

    // Use data point values as sample groups by time periods
    const midIdx = Math.floor(dps.length / 2)
    const earlyValues = dps.slice(0, midIdx).map(dp => Number(dp.subgroupMean || 0)).filter(v => v > 0)
    const lateValues = dps.slice(midIdx).map(dp => Number(dp.subgroupMean || 0)).filter(v => v > 0)

    if (earlyValues.length < 2 || lateValues.length < 2) {
      ElMessage.warning('分组后数据不足')
      return
    }

    const request: SpcAnovaRequest = {
      chartId: selectedChartId.value,
      factors: [
        { source: 'method', label: '前半段', values: earlyValues },
        { source: 'method', label: '后半段', values: lateValues },
      ],
    }

    const results = await spcApi.runAnova(request)
    anovaResults.value = results
    ElMessage.success('方差分析完成')
  } catch (e: any) {
    ElMessage.error('方差分析失败: ' + (e.message || ''))
  } finally {
    anovaLoading.value = false
  }
}

// ═════════════════════════════════════════════════════════════════
//  Data Sources (贯通S3/S4/S5)
// ═════════════════════════════════════════════════════════════════

async function fetchDataSources() {
  if (!selectedChartId.value) return
  try {
    dataSources.value = await spcApi.listDataSources(selectedChartId.value)
  } catch {
    // ignore
  }
}

async function fetchInspectionItems() {
  try {
    inspectionItemOptions.value = await inspectionItemApi.getSelectList()
  } catch {
    // ignore
  }
}

function showAddDataSourceDialog() {
  dsForm.chartId = selectedChartId.value ?? 0
  dsForm.sourceType = 'IQC'
  dsForm.inspectionItemId = undefined
  fetchInspectionItems()
  dsDialogVisible.value = true
}

async function addDataSource() {
  try {
    dsForm.chartId = selectedChartId.value!
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
  if (!selectedChartId.value) return
  businessDataLoading.value = true
  try {
    const params: any = {}
    if (businessDateRange.value?.[0]) params.startDate = businessDateRange.value[0].toISOString()
    if (businessDateRange.value?.[1]) params.endDate = businessDateRange.value[1].toISOString()
    businessData.value = await spcApi.getBusinessData(selectedChartId.value, params)
  } catch (err: any) {
    ElMessage.error('拉取数据失败')
  } finally {
    businessDataLoading.value = false
  }
}

function onBusinessDataSelectionChange(rows: BusinessInspectionData[]) {
  selectedBusinessData.value = rows
}

async function importBusinessDataToChart() {
  if (!selectedChartId.value || selectedBusinessData.value.length === 0) {
    ElMessage.warning('请先选择要导入的数据点')
    return
  }

  try {
    // Group by date to create subgroups
    const values = selectedBusinessData.value
      .filter(d => d.measuredValue != null)
      .map(d => Number(d.measuredValue))

    if (values.length === 0) {
      ElMessage.warning('选中的数据没有有效的测量值')
      return
    }

    // Create a single data point with all values as the subgroup
    const chart = selectedChart.value
    const subgroupSize = chart?.subgroupSize || 5

    // Batch create data points
    const batches = []
    for (let i = 0; i < values.length; i += subgroupSize) {
      const batch = values.slice(i, i + subgroupSize)
      batches.push({
        chartId: selectedChartId.value,
        subgroupIndex: Math.floor(i / subgroupSize) + 1,
        individualValues: JSON.stringify(batch),
        measuredAt: new Date().toISOString(),
      })
    }

    await spcApi.batchCreateDataPoints({ chartId: selectedChartId.value, dataPoints: batches })
    ElMessage.success(`成功导入 ${batches.length} 个子组数据`)
    fetchDataPoints()
    runAnalysis()
  } catch (err: any) {
    ElMessage.error('导入失败')
  }
}

function getSourceTypeLabel(type: string): string {
  const opt = SOURCE_TYPE_OPTIONS.find(o => o.value === type)
  return opt?.label || type
}

// ═════════════════════════════════════════════════════════════════
//  Helpers
// ═════════════════════════════════════════════════════════════════

function formatDate(d?: string): string {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function formatNumber(v?: number, digits = 4): string {
  if (v == null) return '-'
  return v.toFixed(digits)
}

function cpkGradeType(grade?: string): string {
  return grade ? CPK_GRADE_TYPE[grade] || 'info' : 'info'
}

function violationCount(ruleNum: number): number {
  return analysisReport.value?.violations?.filter(v => v.ruleNumber === ruleNum).length || 0
}

function getRuleName(ruleNum: number): string {
  const rule = WESTERN_ELECTRIC_RULES.find(r => r.ruleNumber === ruleNum)
  return rule?.ruleName || `规则 ${ruleNum}`
}

function getChartTypeTagType(chartType: string): string {
  const type = chartType?.replace(/_.*/, '').toLowerCase()
  const typeMap: Record<string, string> = { xbar: 'primary', r: 'warning', x: 'success', s: 'info' }
  return typeMap[type] || 'info'
}

function handleChartAction(cmd: string, chart: SpcControlChart) {
  if (cmd === 'edit') {
    selectChart(chart.id).then(() => openEditChart())
  } else if (cmd === 'delete') {
    deleteChart(chart.id)
  }
}

// ═════════════════════════════════════════════════════════════════
//  Watch
// ═════════════════════════════════════════════════════════════════

watch(activeTab, (tab) => {
  if (tab === 'triggers') fetchTriggers()
  if (tab === 'anova') fetchAnovaResults()
  if (tab === 'datasources') fetchDataSources()
})

// ═════════════════════════════════════════════════════════════════
//  Lifecycle
// ═════════════════════════════════════════════════════════════════

onMounted(async () => {
  await fetchCharts()
})

onUnmounted(() => {
  cleanupCharts()
})
</script>

<template>
  <div class="spc-container">
    <!-- Page Header -->
    <div class="spc-header">
      <div class="spc-header-main">
        <div class="spc-header-icon">
          <el-icon :size="22"><DataAnalysis /></el-icon>
        </div>
        <div class="spc-header-text">
          <h2>SPC 统计分析</h2>
          <span class="spc-subtitle">统计过程控制与过程能力分析</span>
        </div>
      </div>
      <div class="spc-header-actions">
        <el-button type="primary" size="small" @click="openCreateChart" :icon="Plus">
          新建控制图
        </el-button>
      </div>
    </div>

    <!-- Main Layout: Chart List (left) + Detail (right) -->
    <div class="spc-layout">
      <!-- Left: Chart List -->
      <div class="spc-sidebar">
        <div class="sidebar-search">
          <el-input
            v-model="chartQuery.keyword"
            placeholder="搜索控制图名称、参数代码..."
            size="default"
            clearable
            @keyup.enter="fetchCharts"
            @clear="fetchCharts"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
            <template #append>
              <el-button @click="fetchCharts" :icon="Search" link />
            </template>
          </el-input>
        </div>

        <div class="sidebar-list-header">
          <span class="sidebar-list-title">控制图列表</span>
          <el-tag size="small" effect="plain" type="info">{{ charts.length }}</el-tag>
        </div>

        <div class="chart-list" v-loading="chartsLoading" v-if="charts.length > 0">
          <div
            v-for="chart in charts"
            :key="chart.id"
            class="chart-list-item"
            :class="{ active: chart.id === selectedChartId }"
            @click="selectChart(chart.id)"
          >
            <div class="chart-list-item-top">
              <div class="chart-name">
                <el-icon v-if="chart.id === selectedChartId" class="chart-active-icon"><DataAnalysis /></el-icon>
                {{ chart.name }}
              </div>
              <el-dropdown trigger="click" @command="(cmd: string) => handleChartAction(cmd, chart)">
                <el-icon class="chart-more" @click.stop><MoreFilled /></el-icon>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item command="edit" :icon="Edit">编辑</el-dropdown-item>
                    <el-dropdown-item command="delete" :icon="Delete" :disabled="!hasChart || chart.id !== selectedChartId">删除</el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </div>
            <div class="chart-list-item-meta">
              <el-tag size="small" :type="getChartTypeTagType(chart.chartType)" effect="dark" round>
                {{ CHART_TYPE_MAP[chart.chartType] || chart.chartType }}
              </el-tag>
              <span class="chart-param-code">{{ chart.parameterCode }}</span>
            </div>
          </div>
        </div>
        <div v-if="!chartsLoading && charts.length === 0" class="sidebar-empty">
          <el-empty :image-size="60" description="暂无控制图" />
          <el-button type="primary" size="small" @click="openCreateChart">创建第一个</el-button>
        </div>

        <div class="chart-list-footer" v-if="chartsTotal > 20">
          <el-pagination
            v-model:current-page="chartQuery.page"
            v-model:page-size="chartQuery.pageSize"
            :total="chartsTotal"
            small
            layout="prev, pager, next"
            @current-change="fetchCharts"
          />
        </div>
      </div>

      <!-- Right: Detail Panel -->
      <div class="spc-content" v-if="hasChart">
        <!-- Toolbar -->
        <div class="detail-toolbar">
          <div class="chart-info">
            <div class="chart-info-main">
              <h3 class="chart-info-title">{{ selectedChart?.name }}</h3>
              <div class="chart-info-badges">
                <el-tag size="small" :type="getChartTypeTagType(selectedChart?.chartType || '')" effect="dark" round>
                  {{ CHART_TYPE_MAP[selectedChart?.chartType || ''] || selectedChart?.chartType }}
                </el-tag>
                <el-tag size="small" effect="plain" class="badge-pill">n = {{ selectedChart?.subgroupSize }}</el-tag>
                <el-tag v-if="selectedChart?.usl != null" size="small" type="danger" effect="plain" class="badge-pill">
                  USL: {{ selectedChart.usl }}
                </el-tag>
                <el-tag v-if="selectedChart?.lsl != null" size="small" type="warning" effect="plain" class="badge-pill">
                  LSL: {{ selectedChart.lsl }}
                </el-tag>
              </div>
            </div>
            <div v-if="selectedChart?.parameterCode" class="chart-info-code">
              参数: <strong>{{ selectedChart.parameterCode }}</strong>
            </div>
          </div>
          <div class="detail-actions">
            <el-button size="small" @click="openEditChart" :icon="Edit">编辑</el-button>
            <el-button size="small" @click="openAddDataPoint" type="success" :icon="Plus">添加数据</el-button>
            <el-button size="small" @click="runAnalysis" :loading="analysisLoading" type="primary" :icon="Refresh">重新分析</el-button>
            <el-button size="small" @click="deleteChart(selectedChartId!)" type="danger" plain :icon="Delete">删除</el-button>
          </div>
        </div>

        <!-- Tabs -->
        <el-tabs v-model="activeTab" class="spc-tabs" type="border-card">
          <!-- Tab 1: Control Chart -->
          <el-tab-pane name="chart">
            <template #label>
              <el-icon><DataLine /></el-icon><span>控制图</span>
            </template>
            <div v-loading="analysisLoading">
              <!-- Capability Cards -->
              <div v-if="analysisReport?.capability" class="cpk-cards">
                <el-card class="cpk-card" :class="cpkGradeType(analysisReport.capability.grade)">
                  <div class="cpk-card-accent" :class="cpkGradeType(analysisReport.capability.grade)"></div>
                  <div class="cpk-label">
                    Cpk
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">Cpk — 过程能力指数（考虑中心偏移）</div>
                        <div class="tip-desc">衡量过程满足规格要求的实际能力，考虑了过程均值与目标值的偏移。</div>
                        <div class="tip-formula">Cpk = min( (USL − μ) / 3σ, (μ − LSL) / 3σ )</div>
                        <div class="tip-criteria">判断标准：Cpk ≥ 1.67 优秀 | ≥ 1.33 良好 | ≥ 1.0 临界 | &lt; 1.0 不足</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.cpk, 2) }}</div>
                  <el-tag :type="cpkGradeType(analysisReport.capability.grade)" size="small" effect="dark" round>
                    {{ analysisReport.capability.grade }}
                  </el-tag>
                </el-card>
                <el-card class="cpk-card">
                  <div class="cpk-card-accent"></div>
                  <div class="cpk-label">
                    Cp
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">Cp — 过程能力指数（无偏移）</div>
                        <div class="tip-desc">衡量过程在规格范围内的潜在能力，仅考虑过程的固有变异，不考虑均值偏移。</div>
                        <div class="tip-formula">Cp = (USL − LSL) / (6 × σ<sub>组内</sub>)</div>
                        <div class="tip-criteria">判断标准：Cp ≥ 1.67 优秀 | ≥ 1.33 良好 | ≥ 1.0 临界 | &lt; 1.0 不足</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.cp, 2) }}</div>
                </el-card>
                <el-card class="cpk-card">
                  <div class="cpk-card-accent"></div>
                  <div class="cpk-label">
                    Ppk
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">Ppk — 过程性能指数</div>
                        <div class="tip-desc">衡量过程长期实际性能，使用总标准差（包含组间与组内全部变异），反映过程长期稳定性。</div>
                        <div class="tip-formula">Ppk = min( (USL − μ) / 3σ<sub>总</sub>, (μ − LSL) / 3σ<sub>总</sub> )</div>
                        <div class="tip-criteria">判断标准：Ppk ≥ 1.67 优秀 | ≥ 1.33 良好 | ≥ 1.0 临界 | &lt; 1.0 不足</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.ppk, 2) }}</div>
                </el-card>
                <el-card class="cpk-card">
                  <div class="cpk-card-accent"></div>
                  <div class="cpk-label">
                    σ
                    <span class="cpk-subscript">组内</span>
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">σ<sub>组内</sub> — 组内标准差</div>
                        <div class="tip-desc">反映过程短期变异（仅组内波动），是计算 Cp/Cpk 的基础。通过子组极差或标准差估计。</div>
                        <div class="tip-formula">σ<sub>组内</sub> = R̄ / d₂ （极差法）<br>σ<sub>组内</sub> = S̄ / c₄ （标准差法）</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value cpk-value-sm">{{ formatNumber(analysisReport.capability.sigmaWithin, 6) }}</div>
                </el-card>
                <el-card class="cpk-card">
                  <div class="cpk-card-accent"></div>
                  <div class="cpk-label">
                    DPMO
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">DPMO — 百万机会缺陷数</div>
                        <div class="tip-desc">Defects Per Million Opportunities，每百万个产品/机会中的缺陷数。基于过程能力推算的长期预期值。</div>
                        <div class="tip-formula">DPMO = (1 − Φ(3 × Cpk)) × 1,000,000</div>
                        <div class="tip-criteria">σ 水平越高，DPMO 越低：<br>6σ ≈ 3.4 ppm | 5σ ≈ 233 ppm | 4σ ≈ 6,210 ppm | 3σ ≈ 66,807 ppm</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.estimatedPpm, 0) }}</div>
                  <div class="cpk-unit">ppm</div>
                </el-card>
              </div>

              <!-- Charts -->
              <div class="charts-row">
                <div class="chart-container">
                  <div ref="xbarChartRef" class="chart-box"></div>
                </div>
                <div class="chart-container">
                  <div ref="rChartRef" class="chart-box"></div>
                </div>
              </div>

              <!-- Control Limits Info -->
              <el-descriptions v-if="analysisReport?.controlLimits" title="控制限参数" :column="6" border class="limits-table">
                <el-descriptions-item label="X̄ CL">{{ formatNumber(analysisReport.controlLimits.clXbar) }}</el-descriptions-item>
                <el-descriptions-item label="X̄ UCL">{{ formatNumber(analysisReport.controlLimits.uclXbar) }}</el-descriptions-item>
                <el-descriptions-item label="X̄ LCL">{{ formatNumber(analysisReport.controlLimits.lclXbar) }}</el-descriptions-item>
                <el-descriptions-item label="R CL">{{ formatNumber(analysisReport.controlLimits.clR) }}</el-descriptions-item>
                <el-descriptions-item label="R UCL">{{ formatNumber(analysisReport.controlLimits.uclR) }}</el-descriptions-item>
                <el-descriptions-item label="R LCL">{{ formatNumber(analysisReport.controlLimits.lclR) }}</el-descriptions-item>
              </el-descriptions>

              <!-- Data Points Table -->
              <div class="section-title">
                <span>数据点列表</span>
                <el-button size="small" text @click="fetchDataPoints">刷新</el-button>
              </div>
              <el-table :data="dataPoints" stripe  max-height="200" v-loading="dataPointsLoading">
                <el-table-column prop="subgroupIndex" label="子组#" width="70" />
                <el-table-column prop="individualValues" label="测量值" min-width="200">
                  <template #default="{ row }">
                    {{ row.individualValues }}
                  </template>
                </el-table-column>
                <el-table-column prop="subgroupMean" label="X̄" width="100" align="right">
                  <template #default="{ row }">{{ formatNumber(Number(row.subgroupMean), 4) }}</template>
                </el-table-column>
                <el-table-column prop="subgroupRange" label="R" width="100" align="right">
                  <template #default="{ row }">{{ formatNumber(Number(row.subgroupRange), 4) }}</template>
                </el-table-column>
                <el-table-column prop="measuredAt" label="测量时间" width="160">
                  <template #default="{ row }">{{ formatDate(row.measuredAt) }}</template>
                </el-table-column>
                <el-table-column label="操作" width="60" fixed="right">
                  <template #default="{ row }">
                    <el-button size="small" text type="danger" @click="spcApi.deleteDataPoint(row.id).then(() => { fetchDataPoints(); runAnalysis() })">删除</el-button>
                  </template>
                </el-table-column>
              </el-table>
              <div class="pagination-row">
                <el-pagination
                  v-model:current-page="dpQuery.page"
                  v-model:page-size="dpQuery.pageSize"
                  :total="dataPointsTotal"
                  small
                  layout="total, prev, pager, next"
                  @current-change="fetchDataPoints"
                />
              </div>
            </div>
          </el-tab-pane>

          <!-- Tab 2: Western Electric Rules -->
          <el-tab-pane name="rules">
            <template #label>
              <el-icon><WarningFilled /></el-icon><span>判异规则</span>
            </template>
            <div class="rules-section">
              <div class="rules-header">
                <div class="rules-header-left">
                  <h3>Western Electric 8大判异规则</h3>
                  <el-tag size="small" :type="analysisReport?.violations && analysisReport.violations.length > 0 ? 'danger' : 'success'" effect="dark" round>
                    {{ analysisReport?.violations ? analysisReport.violations.length : 0 }} 条违规
                  </el-tag>
                </div>
                <el-button size="small" type="primary" @click="ruleConfigVisible = true" :icon="Setting">配置规则</el-button>
              </div>

              <div v-if="analysisReport?.violations && analysisReport.violations.length > 0" class="violation-alert">
                <el-alert title="检测到判异" :description="`共 ${analysisReport.violations.length} 条违规记录`" type="warning" show-icon :closable="false" />
              </div>

              <div class="rules-grid">
                <div
                  v-for="rule in alertRules"
                  :key="rule.id"
                  class="rule-item"
                  :class="{ 'rule-has-violation': violationCount(rule.ruleNumber) > 0 }"
                >
                  <div class="rule-header">
                    <div class="rule-num-group">
                      <span class="rule-num">#{{ rule.ruleNumber }}</span>
                      <el-tag v-if="violationCount(rule.ruleNumber) > 0" size="small" type="danger" effect="dark" round class="violation-badge">
                        {{ violationCount(rule.ruleNumber) }}
                      </el-tag>
                    </div>
                    <el-switch
                      :model-value="rule.enabled"
                      size="small"
                      :active-text="''"
                      :inactive-text="''"
                      @change="toggleRule(rule)"
                    />
                  </div>
                  <div class="rule-name">{{ rule.ruleName }}</div>
                  <div class="rule-desc">{{ rule.ruleDescription }}</div>
                  <div class="rule-meta">
                    <el-icon><Clock /></el-icon>
                    <span>阈值: {{ rule.triggerThreshold }}点</span>
                    <span v-if="rule.sigmaThreshold > 0">
                      <el-icon><TrendCharts /></el-icon>
                      {{ rule.sigmaThreshold }}σ
                    </span>
                  </div>
                  <div v-if="violationCount(rule.ruleNumber) > 0" class="rule-status rule-status-danger">
                    <el-icon><WarningFilled /></el-icon>
                    <span>已触发 {{ violationCount(rule.ruleNumber) }} 次</span>
                  </div>
                  <div v-else class="rule-status rule-status-ok">
                    <el-icon><CircleCheck /></el-icon>
                    <span>未触发</span>
                  </div>
                </div>
              </div>
            </div>
          </el-tab-pane>

          <!-- Tab 3: Violation Triggers -->
          <el-tab-pane name="triggers">
            <template #label>
              <el-icon><Bell /></el-icon><span>报警记录</span>
            </template>
            <el-table :data="triggers" stripe  v-loading="loading">
              <el-table-column prop="ruleNumber" label="规则" width="60">
                <template #default="{ row }">{{ `#${row.ruleNumber}` }}</template>
              </el-table-column>
              <el-table-column prop="ruleName" label="规则名称" min-width="160" />
              <el-table-column prop="violatedPointIndex" label="违规点" width="100">
                <template #default="{ row }">点 #{{ row.violatedPointIndex + 1 }}</template>
              </el-table-column>
              <el-table-column prop="detail" label="详情" min-width="200">
                <template #default="{ row }">
                  <template v-if="row.detail">
                    {{ safeJsonParse(row.detail)?.description || row.detail }}
                  </template>
                </template>
              </el-table-column>
              <el-table-column prop="triggeredAt" label="触发时间" width="160">
                <template #default="{ row }">{{ formatDate(row.triggeredAt) }}</template>
              </el-table-column>
              <el-table-column label="状态" width="80">
                <template #default="{ row }">
                  <el-tag :type="row.resolved ? 'success' : 'danger'" size="small">
                    {{ row.resolved ? '已处理' : '待处理' }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column label="操作" width="100" fixed="right">
                <template #default="{ row }">
                  <el-button v-if="!row.resolved" size="small" text type="primary" @click="resolveTrigger(row.id)">标记已处理</el-button>
                </template>
              </el-table-column>
            </el-table>
            <div class="pagination-row">
              <el-pagination
                v-model:current-page="triggerQuery.page"
                v-model:page-size="triggerQuery.pageSize"
                :total="triggersTotal"
                small
                layout="total, prev, pager, next"
                @current-change="fetchTriggers"
              />
            </div>
          </el-tab-pane>

          <!-- Tab 4: ANOVA -->
          <el-tab-pane name="anova">
            <template #label>
              <el-icon><Document /></el-icon><span>方差分析</span>
            </template>
            <div v-loading="anovaLoading">
              <div v-if="anovaResults.length === 0" class="empty-anova">
                <el-empty description="暂无方差分析结果。点击「运行方差分析」来分析各因素对质量的影响" :image-size="100">
                  <el-button type="primary" @click="runAnovaAnalysis" :icon="TrendCharts">运行方差分析</el-button>
                </el-empty>
              </div>

              <div v-for="result in anovaResults" :key="result.id" class="anova-item">
                <el-card>
                  <template #header>
                    <div class="anova-header">
                      <strong>{{ ANOVA_SOURCE_MAP[result.source] || result.source }}</strong>
                      <el-tag :type="result.significant ? 'danger' : 'success'" size="small">
                        {{ result.significant ? '显著影响' : '无显著影响' }}
                      </el-tag>
                    </div>
                  </template>
                  <el-descriptions :column="4" border>
                    <el-descriptions-item label="平方和(SS)">{{ formatNumber(result.sumOfSquares, 2) }}</el-descriptions-item>
                    <el-descriptions-item label="自由度(df)">{{ result.degreesFreedom }}</el-descriptions-item>
                    <el-descriptions-item label="均方(MS)">{{ formatNumber(result.meanSquare, 2) }}</el-descriptions-item>
                    <el-descriptions-item label="F值">{{ formatNumber(result.fRatio, 4) }}</el-descriptions-item>
                    <el-descriptions-item label="P值">{{ formatNumber(result.pValue, 6) }}</el-descriptions-item>
                    <el-descriptions-item label="显著性">
                      <el-tag :type="result.significant ? 'danger' : 'success'" size="small">
                        {{ result.pValue < 0.05 ? 'p < 0.05' : 'p ≥ 0.05' }}
                      </el-tag>
                    </el-descriptions-item>
                  </el-descriptions>
                </el-card>
              </div>
            </div>
          </el-tab-pane>

          <!-- Tab 5: Data Sources (贯通S3/S4/S5) -->
          <el-tab-pane name="datasources">
            <template #label>
              <el-icon><Connection /></el-icon><span>数据源</span>
            </template>
            <div v-if="!selectedChartId" style="padding: 60px 0; text-align: center;">
              <el-empty description="请先选择一个控制图" :image-size="80">
                <el-button type="primary" @click="openCreateChart">创建控制图</el-button>
              </el-empty>
            </div>
            <div v-else>
              <!-- 数据源配置区 -->
              <div class="ds-section">
                <div class="ds-section-header">
                  <div class="ds-section-title">
                    <el-icon class="ds-icon"><Connection /></el-icon>
                    <h3>数据源配置</h3>
                  </div>
                  <p class="ds-section-desc">配置SPC从IQC/IPQC/FQC业务模块自动拉取检验数据</p>
                  <el-button type="primary" size="small" @click="showAddDataSourceDialog" :icon="Plus">
                    添加数据源
                  </el-button>
                </div>

                <el-table :data="dataSources" stripe border class="ds-table">
                  <el-table-column label="数据源类型" width="160" align="center">
                    <template #default="{ row }">
                      <el-tag type="primary" effect="plain" round>{{ getSourceTypeLabel(row.sourceType) }}</el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column prop="inspectionItemName" label="检验项目" min-width="150" align="center">
                    <template #default="{ row }">{{ row.inspectionItemName || '全部项目' }}</template>
                  </el-table-column>
                  <el-table-column label="操作" width="100" align="center" fixed="right">
                    <template #default="{ row }">
                      <el-button size="small" type="danger" text :icon="Delete" @click="removeDataSource(row.id)">删除</el-button>
                    </template>
                  </el-table-column>
                </el-table>
                <el-empty v-if="dataSources.length === 0" description="暂无数据源，点击「添加数据源」配置" />
              </div>

              <!-- 业务数据拉取区 -->
              <div class="ds-section">
                <div class="ds-section-header">
                  <div class="ds-section-title">
                    <el-icon class="ds-icon"><Document /></el-icon>
                    <h3>业务数据预览</h3>
                  </div>
                  <div class="ds-actions">
                    <el-date-picker
                      v-model="businessDateRange"
                      type="daterange"
                      range-separator="至"
                      start-placeholder="开始日期"
                      end-placeholder="结束日期"
                      size="small"
                      style="width: 300px"
                    />
                    <el-button size="small" type="primary" @click="fetchBusinessData" :loading="businessDataLoading" :icon="Refresh">拉取数据</el-button>
                    <el-button size="small" @click="importBusinessDataToChart" :icon="Top">导入到控制图</el-button>
                  </div>
                </div>

                <el-table
                  :data="businessData"
                  stripe
                  border
                  max-height="300"
                  v-loading="businessDataLoading"
                  @selection-change="onBusinessDataSelectionChange"
                  class="ds-table"
                >
                  <el-table-column type="selection" width="40" align="center" />
                  <el-table-column label="来源" width="80" align="center">
                    <template #default="{ row }">{{ row.sourceType }}</template>
                  </el-table-column>
                  <el-table-column prop="sourceNo" label="单据号" width="140" align="center" />
                  <el-table-column prop="inspectionItemName" label="检验项目" min-width="120" align="center" />
                  <el-table-column prop="measuredValue" label="测量值" width="100" align="right">
                    <template #default="{ row }">{{ row.measuredValue ?? '-' }}</template>
                  </el-table-column>
                  <el-table-column prop="result" label="结果" width="80" align="center">
                    <template #default="{ row }">
                      <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small" round>
                        {{ row.result }}
                      </el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column prop="inspectedAt" label="检验时间" width="160" align="center">
                    <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
                  </el-table-column>
                </el-table>
                <el-empty v-if="businessData.length === 0 && !businessDataLoading" description="点击「拉取数据」从业务模块获取检验数据" />
              </div>
            </div>
          </el-tab-pane>
        </el-tabs>
      </div>

      <!-- Empty state -->
      <div class="spc-content spc-empty" v-else>
        <el-empty description="请选择或创建一个SPC控制图开始分析">
          <el-button type="primary" @click="openCreateChart">新建控制图</el-button>
        </el-empty>
      </div>
    </div>

    <!-- ── Create/Edit Chart Dialog ── -->
    <el-dialog v-model="chartDialogVisible" :title="chartDialogTitle" width="560px" :close-on-click-modal="false">
      <el-form :model="chartForm" label-width="110px" label-position="left">
        <div class="form-section-title">
          <el-icon><Document /></el-icon>基本信息
        </div>
        <el-form-item label="控制图名称" required>
          <el-input v-model="chartForm.name" placeholder="如：精加工-CNC-001 Xbar-R图" clearable />
        </el-form-item>
        <el-form-item label="参数代码" required>
          <el-input v-model="chartForm.parameterCode" placeholder="如：DIM_A" clearable />
        </el-form-item>
        <el-form-item label="工序ID">
          <el-input-number v-model="chartForm.processId" :min="0" :step="1" style="width:100%" />
        </el-form-item>

        <el-divider />
        <div class="form-section-title">
          <el-icon><DataAnalysis /></el-icon>图表配置
        </div>
        <el-form-item label="控制图类型">
          <el-select v-model="chartForm.chartType" style="width:100%">
            <el-option v-for="opt in CHART_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="子组大小(n)">
          <el-input-number v-model="chartForm.subgroupSize" :min="2" :max="25" style="width:100%" />
        </el-form-item>

        <el-divider />
        <div class="form-section-title">
          <el-icon><Warning /></el-icon>规格限
        </div>
        <el-form-item label="USL(上限)">
          <el-input-number v-model="chartForm.usl" :min="0" :step="0.001" :precision="6" placeholder="规格上限" style="width:100%" />
        </el-form-item>
        <el-form-item label="LSL(下限)">
          <el-input-number v-model="chartForm.lsl" :min="0" :step="0.001" :precision="6" placeholder="规格下限" style="width:100%" />
        </el-form-item>
        <el-form-item label="目标值">
          <el-input-number v-model="chartForm.targetValue" :min="0" :step="0.001" :precision="6" style="width:100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="chartDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveChart">保存</el-button>
      </template>
    </el-dialog>

    <!-- ── Add Data Point Dialog ── -->
    <el-dialog v-model="dpDialogVisible" title="添加数据点" width="480px" :close-on-click-modal="false">
      <el-form :model="dpForm" label-width="100px" label-position="left">
        <el-form-item label="测量值" required>
          <el-input
            v-model="dpForm.individualValues"
            type="textarea"
            :rows="3"
            placeholder="如: 10.01, 10.02, 9.99, 10.00, 10.01"
          />
          <div class="form-tip">逗号分隔的数值，数量应与子组大小（n={{ selectedChart?.subgroupSize || '—' }}）一致</div>
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

    <!-- ── Data Source Dialog ── -->
    <el-dialog v-model="dsDialogVisible" title="添加数据源" width="520px" :close-on-click-modal="false">
      <el-form label-width="100px" label-position="left">
        <div class="form-section-title">
          <el-icon><Connection /></el-icon>数据源配置
        </div>
        <el-form-item label="数据源类型">
          <el-select v-model="dsForm.sourceType" style="width: 100%">
            <el-option v-for="o in SOURCE_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="检验项目">
          <el-select v-model="dsForm.inspectionItemId" placeholder="不选则取全部" clearable filterable style="width: 100%">
            <el-option v-for="item in inspectionItemOptions" :key="item.id"
              :label="`${item.itemCode} - ${item.itemName}`" :value="item.id" />
          </el-select>
        </el-form-item>
        <div class="form-tip" style="margin-top: 0;">
          <el-icon><InfoFilled /></el-icon>
          不选择检验项目则拉取该业务模块的所有检验数据
        </div>
      </el-form>
      <template #footer>
        <el-button @click="dsDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="addDataSource">添加</el-button>
      </template>
    </el-dialog>

    <!-- ── Rule Config Dialog ── -->
    <el-dialog v-model="ruleConfigVisible" title="判异规则配置" width="700px" :close-on-click-modal="false">
      <div class="form-section-title">
        <el-icon><WarningFilled /></el-icon>Western Electric 8大规则配置
      </div>
      <p class="form-tip" style="margin-bottom: 16px;">配置各判异规则的阈值参数，保存后重新运行分析以生效。</p>
      <el-table :data="alertRules" stripe border class="rule-config-table">
        <el-table-column label="规则" width="60" align="center">
          <template #default="{ row }">#{{ row.ruleNumber }}</template>
        </el-table-column>
        <el-table-column prop="ruleName" label="规则名称" min-width="180" align="center" />
        <el-table-column label="启用" width="80" align="center">
          <template #default="{ row }">
            <el-switch v-model="row.enabled" size="small" />
          </template>
        </el-table-column>
        <el-table-column label="连续点数(N)" width="120" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.triggerThreshold" :min="1" :max="25" controls-position="right" style="width:90px" />
          </template>
        </el-table-column>
        <el-table-column label="σ阈值" width="100" align="center">
          <template #default="{ row }">
            <el-input-number v-model="row.sigmaThreshold" :min="0" :max="5" :step="0.5" controls-position="right" style="width:80px" />
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
/* ═══════════════════════════════════════════════════════════════
   Layout & Root
   ═══════════════════════════════════════════════════════════════ */

.spc-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
  background: var(--el-bg-color-page, #f5f7fa);
}

/* ═══════════════════════════════════════════════════════════════
   Page Header
   ═══════════════════════════════════════════════════════════════ */

.spc-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 20px;
  background: var(--el-bg-color, #fff);
  border-bottom: 1px solid var(--el-border-color-light, #e4e7ed);
  flex-shrink: 0;
}

.spc-header-main {
  display: flex;
  align-items: center;
  gap: 12px;
}

.spc-header-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
  color: #fff;
}

.spc-header-text h2 {
  margin: 0;
  font-size: 17px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
  line-height: 1.3;
}

.spc-subtitle {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
}

.spc-header-actions {
  display: flex;
  gap: 8px;
}

/* ═══════════════════════════════════════════════════════════════
   Main Layout
   ═══════════════════════════════════════════════════════════════ */

.spc-layout {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* ═══════════════════════════════════════════════════════════════
   Left Sidebar
   ═══════════════════════════════════════════════════════════════ */

.spc-sidebar {
  width: 300px;
  min-width: 300px;
  background: var(--el-bg-color, #fff);
  border-right: 1px solid var(--el-border-color-light, #e4e7ed);
  display: flex;
  flex-direction: column;
  padding: 12px;
  gap: 10px;
  overflow: hidden;
}

.sidebar-search :deep(.el-input) {
  border-radius: 8px;
}

.sidebar-list-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 2px;
}

.sidebar-list-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular, #606266);
}

.chart-list {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 4px;
  padding: 2px;
}

.chart-list-item {
  padding: 10px 14px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid transparent;
  background: var(--el-bg-color, #fff);
  position: relative;
}

.chart-list-item:hover {
  background-color: var(--el-fill-color-light, #f5f7fa);
  border-color: var(--el-border-color, #dcdfe6);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
}

.chart-list-item.active {
  background: linear-gradient(135deg, #ecf5ff 0%, #f0f9eb 100%);
  border-color: var(--el-color-primary, #409eff);
  box-shadow: 0 2px 8px rgba(64, 158, 255, 0.15);
}

.chart-list-item.active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 3px;
  height: 60%;
  background: var(--el-color-primary, #409eff);
  border-radius: 0 2px 2px 0;
}

.chart-list-item-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.chart-name {
  font-weight: 500;
  font-size: 13px;
  color: var(--el-text-color-primary, #303133);
  display: flex;
  align-items: center;
  gap: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.chart-active-icon {
  color: var(--el-color-primary, #409eff);
  flex-shrink: 0;
}

.chart-more {
  opacity: 0;
  color: var(--el-text-color-secondary, #909399);
  transition: opacity 0.2s;
  padding: 4px;
  border-radius: 4px;
}

.chart-list-item:hover .chart-more {
  opacity: 1;
}

.chart-more:hover {
  background: var(--el-fill-color, #f0f2f5);
}

.chart-list-item-meta {
  display: flex;
  align-items: center;
  gap: 6px;
}

.chart-param-code {
  font-size: 11px;
  color: var(--el-text-color-regular, #606266);
  font-family: 'Courier New', monospace;
}

.sidebar-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px;
  gap: 8px;
}

.chart-list-footer {
  flex-shrink: 0;
  display: flex;
  justify-content: center;
  padding: 4px 0;
}

/* ═══════════════════════════════════════════════════════════════
   Right Content
   ═══════════════════════════════════════════════════════════════ */

.spc-content {
  flex: 1;
  overflow-y: auto;
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.spc-empty {
  display: flex;
  align-items: center;
  justify-content: center;
}

/* ═══════════════════════════════════════════════════════════════
   Detail Toolbar
   ═══════════════════════════════════════════════════════════════ */

.detail-toolbar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  flex-shrink: 0;
  padding: 14px 16px;
  background: var(--el-bg-color, #fff);
  border-radius: 10px;
  border: 1px solid var(--el-border-color-light, #e4e7ed);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.chart-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
  flex: 1;
  min-width: 0;
}

.chart-info-main {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.chart-info-title {
  margin: 0;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
}

.chart-info-badges {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.badge-pill {
  font-family: 'Courier New', monospace;
}

.chart-info-code {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  padding-left: 2px;
}

.detail-actions {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

/* ═══════════════════════════════════════════════════════════════
   Tabs
   ═══════════════════════════════════════════════════════════════ */

.spc-tabs {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.spc-tabs :deep(.el-tabs__content) {
  flex: 1;
  overflow-y: auto;
  padding: 14px 0;
}

.spc-tabs :deep(.el-tabs__item) {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
}

.spc-tabs :deep(.el-tabs__item .el-icon) {
  font-size: 15px;
}

.spc-tabs :deep(.el-tabs__header) {
  margin: 0;
  padding: 0 16px;
}

/* ═══════════════════════════════════════════════════════════════
   CPK Cards
   ═══════════════════════════════════════════════════════════════ */

.cpk-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
  margin-bottom: 14px;
}

.cpk-card {
  position: relative;
  overflow: hidden;
  border-radius: 10px;
}

.cpk-card :deep(.el-card__body) {
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.cpk-card-accent {
  position: absolute;
  left: 0;
  top: 0;
  width: 4px;
  height: 100%;
  border-radius: 4px 0 0 4px;
}

.cpk-card.success .cpk-card-accent {
  background: linear-gradient(180deg, #67c23a 0%, #85ce61 100%);
}

.cpk-card.primary .cpk-card-accent {
  background: linear-gradient(180deg, #409eff 0%, #66b1ff 100%);
}

.cpk-card.warning .cpk-card-accent {
  background: linear-gradient(180deg, #e6a23c 0%, #ebb563 100%);
}

.cpk-card.danger .cpk-card-accent {
  background: linear-gradient(180deg, #f56c6c 0%, #f89898 100%);
}

/* Cpk card gets a gradient background based on grade */
.cpk-card.success :deep(.el-card__body) {
  background: linear-gradient(135deg, #f0faf0 0%, #ffffff 100%);
}

.cpk-card.primary :deep(.el-card__body) {
  background: linear-gradient(135deg, #ecf5ff 0%, #ffffff 100%);
}

.cpk-card.warning :deep(.el-card__body) {
  background: linear-gradient(135deg, #fdf6ec 0%, #ffffff 100%);
}

.cpk-card.danger :deep(.el-card__body) {
  background: linear-gradient(135deg, #fef0f0 0%, #ffffff 100%);
}

.cpk-label {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  display: inline-flex;
  align-items: baseline;
  gap: 3px;
}

.cpk-subscript {
  font-size: 9px;
  vertical-align: sub;
}

.cpk-help-icon {
  font-size: 12px;
  color: var(--el-color-info-light-3, #c0c4cc);
  cursor: help;
  transition: color 0.2s;
}

.cpk-help-icon:hover {
  color: var(--el-color-primary, #409eff);
}

.cpk-value {
  font-size: 26px;
  font-weight: 700;
  color: var(--el-text-color-primary, #303133);
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}

.cpk-value-sm {
  font-size: 20px;
}

.cpk-unit {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
}

/* ═══════════════════════════════════════════════════════════════
   Tooltip
   ═══════════════════════════════════════════════════════════════ */

:global(.cpk-tooltip) {
  max-width: 380px;
  line-height: 1.7;
  font-size: 12px;
  padding: 8px;
}

:global(.cpk-tooltip .tip-title) {
  font-weight: 600;
  font-size: 13px;
  color: #303133;
  margin-bottom: 6px;
  padding-bottom: 4px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

:global(.cpk-tooltip .tip-desc) {
  color: #606266;
  margin-bottom: 6px;
}

:global(.cpk-tooltip .tip-formula) {
  color: #409eff;
  font-family: 'Courier New', monospace;
  background: #ecf5ff;
  padding: 4px 8px;
  border-radius: 4px;
  margin-bottom: 6px;
  font-size: 12px;
  display: block;
}

:global(.cpk-tooltip .tip-criteria) {
  color: #909399;
  font-size: 11px;
}

/* ═══════════════════════════════════════════════════════════════
   Chart Containers
   ═══════════════════════════════════════════════════════════════ */

.charts-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
  margin-bottom: 14px;
}

.chart-container {
  border: 1px solid var(--el-border-color-light, #e4e7ed);
  border-radius: 10px;
  background: var(--el-bg-color, #fff);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  transition: box-shadow 0.2s;
}

.chart-container:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.chart-box {
  width: 100%;
  height: 320px;
}

/* ═══════════════════════════════════════════════════════════════
   Sections
   ═══════════════════════════════════════════════════════════════ */

.section-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
  font-size: 14px;
  color: var(--el-text-color-primary, #303133);
  margin: 10px 0 6px;
  padding-bottom: 6px;
  border-bottom: 1px solid var(--el-border-color-light, #e4e7ed);
}

.limits-table {
  margin-bottom: 14px;
}

.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: 8px 0;
}

/* ═══════════════════════════════════════════════════════════════
   Rules Tab
   ═══════════════════════════════════════════════════════════════ */

.rules-section {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.rules-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 8px;
}

.rules-header-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

.rules-header h3 {
  margin: 0;
  font-size: 15px;
  color: var(--el-text-color-primary, #303133);
}

.rules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(290px, 1fr));
  gap: 12px;
}

.rule-item {
  border: 1px solid var(--el-border-color-light, #e4e7ed);
  border-radius: 10px;
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: var(--el-bg-color, #fff);
  transition: all 0.2s ease;
}

.rule-item:hover {
  box-shadow: 0 3px 8px rgba(0, 0, 0, 0.06);
  border-color: var(--el-border-color, #dcdfe6);
}

.rule-item.rule-has-violation {
  border-color: var(--el-color-danger-light-5, #fab6b6);
  background: linear-gradient(135deg, #fef0f0 0%, #ffffff 100%);
}

.rule-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.rule-num-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.rule-num {
  font-weight: 700;
  font-size: 15px;
  color: var(--el-color-primary, #409eff);
  font-family: 'Courier New', monospace;
}

.violation-badge {
  font-size: 11px;
  min-width: 20px;
}

.rule-name {
  font-weight: 500;
  font-size: 13px;
  color: var(--el-text-color-primary, #303133);
}

.rule-desc {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.rule-meta {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 11px;
  color: var(--el-text-color-regular, #606266);
  flex-wrap: wrap;
}

.rule-meta .el-icon {
  font-size: 13px;
  color: var(--el-color-info, #909399);
}

.rule-status {
  margin-top: auto;
  padding-top: 8px;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 500;
  padding: 6px 10px;
  border-radius: 6px;
}

.rule-status-danger {
  color: var(--el-color-danger, #f56c6c);
  background: var(--el-color-danger-light-9, #fef0f0);
}

.rule-status-danger .el-icon {
  color: var(--el-color-danger, #f56c6c);
}

.rule-status-ok {
  color: var(--el-color-success, #67c23a);
  background: var(--el-color-success-light-9, #f0f9eb);
}

.rule-status-ok .el-icon {
  color: var(--el-color-success, #67c23a);
}

.violation-alert {
  margin-bottom: 4px;
}

/* ═══════════════════════════════════════════════════════════════
   Triggers Tab
   ═══════════════════════════════════════════════════════════════ */

/* ═══════════════════════════════════════════════════════════════
   ANOVA
   ═══════════════════════════════════════════════════════════════ */

.anova-item {
  margin-bottom: 12px;
  border-radius: 10px;
  overflow: hidden;
}

.anova-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.empty-anova {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  padding: 40px;
}

/* ═══════════════════════════════════════════════════════════════
   Data Sources Tab
   ═══════════════════════════════════════════════════════════════ */

.ds-section {
  background: var(--el-bg-color, #fff);
  border-radius: 10px;
  border: 1px solid var(--el-border-color-light, #e4e7ed);
  padding: 16px;
  margin-bottom: 14px;
}

.ds-section-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 14px;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

.ds-section-title {
  display: flex;
  align-items: center;
  gap: 8px;
}

.ds-icon {
  font-size: 18px;
  color: var(--el-color-primary, #409eff);
}

.ds-section-title h3 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
}

.ds-section-desc {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  margin: 0;
  width: 100%;
}

.ds-actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.ds-table {
  border-radius: 8px;
}

.ds-table :deep(.el-table th) {
  background: var(--el-fill-color-light, #f5f7fa);
  font-weight: 600;
  font-size: 12px;
}

/* ═══════════════════════════════════════════════════════════════
   Dialogs — Form Sections
   ═══════════════════════════════════════════════════════════════ */

.form-section-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
  margin-bottom: 12px;
  padding-bottom: 6px;
  border-bottom: 1px dashed var(--el-border-color-light, #e4e7ed);
}

.form-section-title .el-icon {
  font-size: 15px;
  color: var(--el-color-primary, #409eff);
}

.form-tip {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  margin-top: 4px;
  display: flex;
  align-items: flex-start;
  gap: 4px;
}

.form-tip .el-icon {
  font-size: 12px;
  margin-top: 1px;
  flex-shrink: 0;
}

.rule-config-table {
  border-radius: 8px;
  overflow: hidden;
}

.rule-config-table :deep(.el-table th) {
  background: var(--el-fill-color-light, #f5f7fa);
  font-weight: 600;
  font-size: 12px;
}
</style>

<script setup lang="ts">
import { ref, onMounted, reactive, computed, nextTick, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
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
const loading = ref(false)
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
    window.addEventListener('resize', () => xbarChartInstance?.resize?.())
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
    window.addEventListener('resize', () => rChartInstance?.resize?.())
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
</script>

<template>
  <div class="spc-container">
    <!-- Header -->
    <div class="spc-header">
      <h2>
        <el-icon><DataAnalysis /></el-icon>
        SPC统计分析
      </h2>
      <div class="spc-header-actions">
        <el-button type="primary" size="small" @click="openCreateChart">新建控制图</el-button>
      </div>
    </div>

    <!-- Main Layout: Chart List (left) + Detail (right) -->
    <div class="spc-layout">
      <!-- Left: Chart List -->
      <div class="spc-sidebar">
        <el-input
          v-model="chartQuery.keyword"
          placeholder="搜索控制图..."
          size="small"
          clearable
          @keyup.enter="fetchCharts"
          @clear="fetchCharts"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>

        <div class="chart-list" v-loading="chartsLoading">
          <div
            v-for="chart in charts"
            :key="chart.id"
            class="chart-list-item"
            :class="{ active: chart.id === selectedChartId }"
            @click="selectChart(chart.id)"
          >
            <div class="chart-name">{{ chart.name }}</div>
            <div class="chart-meta">
              <el-tag size="small" type="info">{{ CHART_TYPE_MAP[chart.chartType] || chart.chartType }}</el-tag>
              <span class="chart-param">{{ chart.parameterCode }}</span>
            </div>
          </div>
          <el-empty v-if="!chartsLoading && charts.length === 0" description="暂无控制图" />
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
            <strong>{{ selectedChart?.name }}</strong>
            <el-tag size="small" type="info" effect="plain">
              {{ CHART_TYPE_MAP[selectedChart?.chartType || ''] || selectedChart?.chartType }}
            </el-tag>
            <el-tag size="small" effect="plain">n={{ selectedChart?.subgroupSize }}</el-tag>
            <span v-if="selectedChart?.usl != null" class="spec-limit">USL={{ selectedChart.usl }}</span>
            <span v-if="selectedChart?.lsl != null" class="spec-limit">LSL={{ selectedChart.lsl }}</span>
          </div>
          <div class="detail-actions">
            <el-button size="small" @click="openEditChart">编辑</el-button>
            <el-button size="small" @click="openAddDataPoint" type="success">添加数据点</el-button>
            <el-button size="small" @click="runAnalysis" :loading="analysisLoading" type="primary">重新分析</el-button>
            <el-button size="small" @click="deleteChart(selectedChartId!)" type="danger" plain>删除</el-button>
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
                  <div class="cpk-label">
                    Cpk
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">Cpk — 过程能力指数（考虑中心偏移）</div>
                        <div class="tip-desc">衡量过程满足规格要求的实际能力，考虑了过程均值与目标值的偏移。</div>
                        <div class="tip-formula">Cpk = min( (USL − μ) / 3σ, (μ − LSL) / 3σ )</div>
                        <div class="tip-criteria">判断标准：Cpk ≥ 1.67 优秀 | ≥ 1.33 良好 | ≥ 1.0 临界 | < 1.0 不足</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.cpk, 4) }}</div>
                  <el-tag :type="cpkGradeType(analysisReport.capability.grade)" size="small">
                    {{ analysisReport.capability.grade }}
                  </el-tag>
                </el-card>
                <el-card class="cpk-card">
                  <div class="cpk-label">
                    Cp
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">Cp — 过程能力指数（无偏移）</div>
                        <div class="tip-desc">衡量过程在规格范围内的潜在能力，仅考虑过程的固有变异，不考虑均值偏移。</div>
                        <div class="tip-formula">Cp = (USL − LSL) / (6 × σ<sub>组内</sub>)</div>
                        <div class="tip-criteria">判断标准：Cp ≥ 1.67 优秀 | ≥ 1.33 良好 | ≥ 1.0 临界 | < 1.0 不足</div>
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
                        <div class="tip-desc">衡量过程长期实际性能，使用总标准差（包含组间与组内全部变异），反映过程长期稳定性。</div>
                        <div class="tip-formula">Ppk = min( (USL − μ) / 3σ<sub>总</sub>, (μ − LSL) / 3σ<sub>总</sub> )</div>
                        <div class="tip-criteria">判断标准：Ppk ≥ 1.67 优秀 | ≥ 1.33 良好 | ≥ 1.0 临界 | < 1.0 不足</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.ppk, 4) }}</div>
                </el-card>
                <el-card class="cpk-card">
                  <div class="cpk-label">
                    σ (组内)
                    <el-tooltip placement="top" popper-class="cpk-tooltip">
                      <template #content>
                        <div class="tip-title">σ<sub>组内</sub> — 组内标准差</div>
                        <div class="tip-desc">反映过程短期变异（仅组内波动），是计算 Cp/Cpk 的基础。通过子组极差或标准差估计。</div>
                        <div class="tip-formula">σ<sub>组内</sub> = R̄ / d₂ （极差法）<br>σ<sub>组内</sub> = S̄ / c₄ （标准差法）</div>
                      </template>
                      <el-icon class="cpk-help-icon"><HelpFilled /></el-icon>
                    </el-tooltip>
                  </div>
                  <div class="cpk-value">{{ formatNumber(analysisReport.capability.sigmaWithin, 6) }}</div>
                </el-card>
                <el-card class="cpk-card">
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
                <div ref="xbarChartRef" class="chart-box" style="width: 50%; height: 320px;"></div>
                <div ref="rChartRef" class="chart-box" style="width: 50%; height: 320px;"></div>
              </div>

              <!-- Control Limits Info -->
              <el-descriptions v-if="analysisReport?.controlLimits" title="控制限参数" :column="6" size="small" border class="limits-table">
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
              <el-table :data="dataPoints" stripe size="small" max-height="200" v-loading="loading">
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
                <h3>Western Electric 8大判异规则</h3>
                <el-button size="small" type="primary" @click="ruleConfigVisible = true">配置规则</el-button>
              </div>

              <div v-if="analysisReport?.violations && analysisReport.violations.length > 0" class="violation-alert">
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
                  <div v-if="violationCount(rule.ruleNumber) > 0" class="rule-violation">
                    <el-tag size="small" type="danger">触发 {{ violationCount(rule.ruleNumber) }} 次</el-tag>
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
            <template #label>
              <el-icon><Bell /></el-icon><span>报警记录</span>
            </template>
            <el-table :data="triggers" stripe size="small" v-loading="loading">
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
                    {{ JSON.parse(row.detail).description || row.detail }}
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
                <el-empty description="暂无方差分析结果。点击「运行方差分析」来分析各因素对质量的影响">
                  <el-button type="primary" @click="runAnovaAnalysis">运行方差分析</el-button>
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
                  <el-descriptions :column="4" size="small" border>
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
            <div v-if="!selectedChartId" style="padding: 40px 0;">
              <el-empty description="请先选择一个控制图" />
            </div>
            <div v-else>
              <div class="datasource-header">
                <span class="text-sm text-gray-400">
                  配置SPC从IQC/IPQC/FQC业务模块自动拉取检验数据
                </span>
                <el-button type="primary" size="small" @click="showAddDataSourceDialog">
                  + 添加数据源
                </el-button>
              </div>

              <!-- 数据源列表 -->
              <el-table :data="dataSources" stripe size="small" class="mt-3">
                <el-table-column label="数据源类型" width="160">
                  <template #default="{ row }">
                    <el-tag>{{ getSourceTypeLabel(row.sourceType) }}</el-tag>
                  </template>
                </el-table-column>
                <el-table-column prop="inspectionItemName" label="检验项目" min-width="150">
                  <template #default="{ row }">{{ row.inspectionItemName || '全部项目' }}</template>
                </el-table-column>
                <el-table-column label="操作" width="80">
                  <template #default="{ row }">
                    <el-button size="small" type="danger" text
                      @click="removeDataSource(row.id)">删除</el-button>
                  </template>
                </el-table-column>
              </el-table>

              <el-divider />

              <!-- 拉取数据预览 -->
              <div class="flex items-center justify-between mb-3">
                <strong>业务数据预览</strong>
                <div class="flex gap-2">
                  <el-date-picker v-model="businessDateRange" type="daterange" range-separator="至"
                    start-placeholder="开始日期" end-placeholder="结束日期" size="small" />
                  <el-button size="small" type="primary" @click="fetchBusinessData"
                    :loading="businessDataLoading">拉取数据</el-button>
                  <el-button size="small" @click="importBusinessDataToChart">导入到控制图</el-button>
                </div>
              </div>

              <el-table :data="businessData" stripe size="small" max-height="300" v-loading="businessDataLoading"
                @selection-change="onBusinessDataSelectionChange">
                <el-table-column type="selection" width="40" />
                <el-table-column label="来源" width="80">
                  <template #default="{ row }">{{ row.sourceType }}</template>
                </el-table-column>
                <el-table-column prop="sourceNo" label="单据号" width="140" />
                <el-table-column prop="inspectionItemName" label="检验项目" min-width="120" />
                <el-table-column prop="measuredValue" label="测量值" width="100" align="right">
                  <template #default="{ row }">{{ row.measuredValue ?? '-' }}</template>
                </el-table-column>
                <el-table-column prop="result" label="结果" width="70">
                  <template #default="{ row }">
                    <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">
                      {{ row.result }}
                    </el-tag>
                  </template>
                </el-table-column>
                <el-table-column prop="inspectedAt" label="检验时间" width="160">
                  <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
                </el-table-column>
              </el-table>

              <el-empty v-if="businessData.length === 0 && !businessDataLoading" description="点击「拉取数据」从业务模块获取检验数据" />
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
    <el-dialog v-model="chartDialogVisible" :title="chartDialogTitle" width="520px" :close-on-click-modal="false">
      <el-form :model="chartForm" label-width="120px" size="small">
        <el-form-item label="控制图名称" required>
          <el-input v-model="chartForm.name" placeholder="如：精加工-CNC-001 Xbar-R图" />
        </el-form-item>
        <el-form-item label="参数代码" required>
          <el-input v-model="chartForm.parameterCode" placeholder="如：DIM_A" />
        </el-form-item>
        <el-form-item label="工序ID">
          <el-input-number v-model="chartForm.processId" :min="0" :step="1" />
        </el-form-item>
        <el-form-item label="控制图类型">
          <el-select v-model="chartForm.chartType" style="width:100%">
            <el-option v-for="opt in CHART_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="子组大小(n)">
          <el-input-number v-model="chartForm.subgroupSize" :min="2" :max="25" />
        </el-form-item>
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

    <!-- ── Data Source Dialog ── -->
    <el-dialog v-model="dsDialogVisible" title="添加数据源" width="500px">
      <el-form label-width="100px">
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
        <p class="text-gray-400 text-sm">提示：不选择检验项目则拉取该业务模块的所有检验数据</p>
      </el-form>
      <template #footer>
        <el-button @click="dsDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="addDataSource">添加</el-button>
      </template>
    </el-dialog>

    <!-- ── Rule Config Dialog ── -->
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
.spc-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.spc-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 16px;
  border-bottom: 1px solid var(--el-border-color-light, #e4e7ed);
  flex-shrink: 0;
}

.spc-header h2 {
  margin: 0;
  font-size: 16px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.spc-layout {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* Left sidebar */
.spc-sidebar {
  width: 280px;
  min-width: 280px;
  border-right: 1px solid var(--el-border-color-light, #e4e7ed);
  display: flex;
  flex-direction: column;
  padding: 8px;
  gap: 8px;
  overflow: hidden;
}

.chart-list {
  flex: 1;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.chart-list-item {
  padding: 8px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.chart-list-item:hover {
  background-color: var(--el-fill-color-light, #f5f7fa);
}

.chart-list-item.active {
  background-color: var(--el-color-primary-light-9, #ecf5ff);
  border-color: var(--el-color-primary, #409eff);
}

.chart-name {
  font-weight: 500;
  font-size: 13px;
  margin-bottom: 4px;
}

.chart-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
}

.chart-param {
  color: var(--el-text-color-secondary, #909399);
}

.chart-list-footer {
  flex-shrink: 0;
}

/* Right content */
.spc-content {
  flex: 1;
  overflow-y: auto;
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.spc-empty {
  display: flex;
  align-items: center;
  justify-content: center;
}

.detail-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 8px;
  flex-shrink: 0;
}

.chart-info {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
}

.spec-limit {
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  background: var(--el-fill-color, #f0f2f5);
  padding: 2px 6px;
  border-radius: 4px;
}

.detail-actions {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
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

/* Tab label icon + text alignment */
.spc-tabs :deep(.el-tabs__item) {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.spc-tabs :deep(.el-tabs__item .el-icon) {
  font-size: 15px;
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

/* ── 能力指标 tooltip 样式 ── */
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

/* Chart row */
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

.empty-anova {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 300px;
}

.form-tip {
  font-size: 11px;
  color: var(--el-text-color-secondary, #909399);
  margin-top: 4px;
}
</style>

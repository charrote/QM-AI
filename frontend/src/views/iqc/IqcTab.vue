<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  receiptApi, inspectionApi, anomalyApi,
  aiRiskApi
} from '@/api/iqc'
import { Box, Document, WarningFilled, Cpu, Edit } from '@element-plus/icons-vue'
import { supplierApi } from '@/api/basicData'
import { productApi } from '@/api/basicData'
import type { PagedResult } from '@/types/basicData'
import type {
  IqcReceipt, IqcReceiptDetail, CreateIqcReceipt, UpdateIqcReceipt,
  IqcInspection, IqcInspectionDetail, CreateIqcInspection, SubmitIqcInspection,
  IqcAnomaly, CreateIqcAnomaly, UpdateIqcAnomaly, ResolveIqcAnomaly,
  AiRiskScore,
} from '@/types/iqc'
import {
  IQC_RECEIPT_STATUS_OPTIONS, IQC_INSPECTION_RESULT_OPTIONS,
  IQC_ANOMALY_TYPE_OPTIONS, IQC_SEVERITY_OPTIONS,
  IQC_ANOMALY_STATUS_OPTIONS, IQC_DISPOSITION_OPTIONS, SAMPLING_LEVEL_OPTIONS,
} from '@/types/iqc'
import SamplingPlanCalculator from '@/components/SamplingPlanCalculator.vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'

defineOptions({ name: 'IqcTab' })

// ═══════════════════════════════════════════════════════════════════
// State
// ═══════════════════════════════════════════════════════════════════

const activeSubTab = ref<'receipts' | 'inspections' | 'anomalies'>('receipts')
const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// ─── 来料登记 ────────────────────────────────────────
const receipts = ref<IqcReceipt[]>([])
const { visible: receiptDialogVisible, open: openReceiptDialog, close: closeReceiptDialog } = useRightPanel()
const isEditingReceipt = ref(false)
const currentReceiptId = ref<number | null>(null)
const receiptForm = reactive<CreateIqcReceipt>({
  receiptNo: '', supplierId: 0, productId: 0, batchNo: '',
  quantity: 0, unit: '', receiptDate: '', inspector: ''
})
const receiptDetail = ref<IqcReceiptDetail | null>(null)
const receiptDetailVisible = ref(false)

// ─── 检验单 ────────────────────────────────────────
const inspections = ref<IqcInspection[]>([])
const inspectionDetail = ref<IqcInspectionDetail | null>(null)
const inspectionDetailVisible = ref(false)
const newInspectionForm = reactive<CreateIqcInspection>({
  receiptId: 0, sampleSize: 0, ac: 0, re: 0, samplingLevel: 'II', aqlValue: 1.0
})
const { visible: newInspectionVisible, open: openNewInspection, close: closeNewInspection } = useRightPanel()

// ─── 异常单 ────────────────────────────────────────
const anomalies = ref<IqcAnomaly[]>([])
const anomalyDialogVisible = ref(false)
const isEditingAnomaly = ref(false)
const editingAnomalyId = ref<number>(0)
const anomalyForm = reactive<CreateIqcAnomaly>({
  receiptId: 0, anomalyType: 'quality', severity: 'major', description: ''
})
const resolveAnomalyVisible = ref(false)
const resolveForm = reactive<ResolveIqcAnomaly>({ resolution: '' })
const resolveAnomalyId = ref<number>(0)

// ─── 下拉选项 ─────────────────────────────────────
const supplierOptions = ref<Array<{ value: number; label: string }>>([])
const productOptions = ref<Array<{ value: number; label: string }>>([])

// ─── AI 风险面板 ────────────────────────────────────
const aiRiskResult = ref<AiRiskScore | null>(null)
const aiRiskLoading = ref(false)
const selectedReceiptId = ref<number | null>(null)

// ─── 抽样方案 (shared component) ──────────────

// ═══════════════════════════════════════════════════════════════════
// Helpers
// ═══════════════════════════════════════════════════════════════════

const statusLabel = (status: string, options: any[]) => {
  const opt = options.find(o => o.value === status)
  return opt?.label || status
}

const statusType = (status: string, options: any[]) => {
  const opt = options.find(o => o.value === status)
  return opt?.type || 'info'
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function calculateResultColor(result: string) {
  switch (result) {
    case 'pass': return 'success'
    case 'fail': return 'danger'
    case 'qualified': return 'success'
    case 'unqualified': return 'danger'
    case 'anomaly': return 'warning'
    default: return 'info'
  }
}

// ═══════════════════════════════════════════════════════════════════
// 来料登记 CRUD
// ═══════════════════════════════════════════════════════════════════

async function loadReceipts() {
  try {
    const res = await receiptApi.list({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value
    })
    receipts.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load receipts', e)
  }
}

function openCreateReceipt() {
  isEditingReceipt.value = false
  currentReceiptId.value = null
  receiptForm.receiptNo = ''
  receiptForm.supplierId = 0
  receiptForm.productId = 0
  receiptForm.batchNo = ''
  receiptForm.quantity = 0
  receiptForm.unit = ''
  receiptForm.receiptDate = new Date().toISOString().slice(0, 10)
  receiptForm.inspector = ''
  openReceiptDialog()
}

function openEditReceipt(row: IqcReceipt) {
  isEditingReceipt.value = true
  currentReceiptId.value = row.id
  receiptForm.receiptNo = row.receiptNo
  receiptForm.supplierId = row.supplierId
  receiptForm.productId = row.productId
  receiptForm.batchNo = row.batchNo || ''
  receiptForm.quantity = row.quantity
  receiptForm.unit = row.unit || ''
  receiptForm.receiptDate = row.receiptDate?.slice(0, 10) || ''
  receiptForm.inspector = row.inspector || ''
  openReceiptDialog()
}

async function saveReceipt() {
  if (!receiptForm.receiptNo || !receiptForm.supplierId || !receiptForm.productId) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    if (isEditingReceipt.value && currentReceiptId.value) {
      await receiptApi.update(currentReceiptId.value, receiptForm as UpdateIqcReceipt)
      ElMessage.success('来料登记已更新')
    } else {
      await receiptApi.create(receiptForm)
      ElMessage.success('来料登记已创建，检验单自动生成')
    }
    closeReceiptDialog()
    await loadReceipts()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteReceipt(row: IqcReceipt) {
  try {
    await ElMessageBox.confirm(`确定删除来料登记「${row.receiptNo}」吗？`, '确认', { type: 'warning' })
    await receiptApi.delete(row.id)
    ElMessage.success('已删除')
    await loadReceipts()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

async function viewReceiptDetail(row: IqcReceipt) {
  try {
    receiptDetail.value = await receiptApi.get(row.id)
    receiptDetailVisible.value = true
    selectedReceiptId.value = row.id
    loadAiRisk(row.id)
  } catch (e) {
    ElMessage.error('加载详情失败')
  }
}

// ═══════════════════════════════════════════════════════════════════
// AI 风险分析
// ═══════════════════════════════════════════════════════════════════

async function loadAiRisk(receiptId: number) {
  aiRiskLoading.value = true
  aiRiskResult.value = null
  try {
    aiRiskResult.value = await aiRiskApi.analyze(receiptId)
  } catch {
    aiRiskResult.value = null
  } finally {
    aiRiskLoading.value = false
  }
}

// ═══════════════════════════════════════════════════════════════════
// 检验单
// ═══════════════════════════════════════════════════════════════════

async function loadInspections() {
  try {
    const res = await inspectionApi.list({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value
    })
    inspections.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load inspections', e)
  }
}

async function viewInspectionDetail(row: IqcInspection) {
  try {
    inspectionDetail.value = await inspectionApi.get(row.id)
    inspectionDetailVisible.value = true
  } catch (e) {
    ElMessage.error('加载检验单详情失败')
  }
}

function openNewInspectionForm(receiptId?: number) {
  newInspectionForm.receiptId = receiptId || 0
  newInspectionForm.sampleSize = 0
  newInspectionForm.ac = 0
  newInspectionForm.re = 0
  newInspectionForm.samplingLevel = 'II'
  newInspectionForm.aqlValue = 1.0
  openNewInspection()
}

async function createInspection() {
  try {
    await inspectionApi.create(newInspectionForm)
    ElMessage.success('检验单已创建')
    closeNewInspection()
    await loadInspections()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
}

// ─── 提交检验结果 ──────────────────────────────────
const submitItems = ref<Array<{
  paramId?: number; itemName?: string; measuredValue?: number;
  usl?: number; lsl?: number; result: string; defectCodeId?: number; remark?: string
}>>([])
const submitInspector = ref('')
const submitInspectionId = ref<number>(0)
const { visible: submitDialogVisible, open: openSubmitDialog, close: closeSubmitDialog } = useRightPanel()

function openSubmitInspection(row: IqcInspection) {
  if (row.result !== 'pending') {
    ElMessage.warning('该检验单已提交')
    return
  }
  submitInspectionId.value = row.id
  submitInspector.value = ''
  // 检查是否有明细
  submitItems.value = [{
    itemName: '外观检查', result: 'pending', measuredValue: undefined,
    usl: undefined, lsl: undefined
  }]
  openSubmitDialog()
}

async function addSubmitItem() {
  submitItems.value.push({
    itemName: '', result: 'pending', measuredValue: undefined,
    usl: undefined, lsl: undefined
  })
}

function removeSubmitItem(index: number) {
  submitItems.value.splice(index, 1)
}

async function submitInspection() {
  if (submitItems.value.length === 0) {
    ElMessage.warning('请至少填写一个检验项目')
    return
  }
  try {
    await inspectionApi.submit(submitInspectionId.value, {
      items: submitItems.value.map(it => ({
        ...it,
        result: it.result === 'pass' ? 'pass' : it.result === 'fail' ? 'fail' : 'pending',
      })),
      inspector: submitInspector.value || undefined,
    })
    ElMessage.success('检验结果已提交')
    closeSubmitDialog()
    await loadInspections()
    // 刷新来料列表
    if (activeSubTab.value === 'receipts') await loadReceipts()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  }
}

// ═══════════════════════════════════════════════════════════════════
// 异常单
// ═══════════════════════════════════════════════════════════════════

async function loadAnomalies() {
  try {
    const res = await anomalyApi.list({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value
    })
    anomalies.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load anomalies', e)
  }
}

function openCreateAnomaly(receiptId?: number) {
  isEditingAnomaly.value = false
  editingAnomalyId.value = 0
  anomalyForm.receiptId = receiptId || 0
  anomalyForm.inspectionId = undefined
  anomalyForm.anomalyType = 'quality'
  anomalyForm.severity = 'major'
  anomalyForm.description = ''
  anomalyForm.handler = ''
  anomalyDialogVisible.value = true
}

function openEditAnomaly(row: IqcAnomaly) {
  isEditingAnomaly.value = true
  editingAnomalyId.value = row.id
  anomalyForm.receiptId = row.receiptId
  anomalyForm.anomalyType = row.anomalyType
  anomalyForm.severity = row.severity
  anomalyForm.description = row.description || ''
  anomalyForm.handler = row.handler || ''
  anomalyDialogVisible.value = true
}

async function saveAnomaly() {
  if (!anomalyForm.receiptId) {
    ElMessage.warning('请选择来料登记')
    return
  }
  if (!anomalyForm.anomalyType || !anomalyForm.severity || !anomalyForm.description) {
    ElMessage.warning('请填写异常类型、严重程度和描述')
    return
  }
  try {
    if (isEditingAnomaly.value) {
      await anomalyApi.update(editingAnomalyId.value, anomalyForm)
      ElMessage.success('异常单已更新')
    } else {
      await anomalyApi.create(anomalyForm)
      ElMessage.success('异常单已创建')
    }
    anomalyDialogVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteAnomaly(row: IqcAnomaly) {
  try {
    await ElMessageBox.confirm(`确定关闭异常单「${row.anomalyNo}」吗？`, '确认关闭', {
      type: 'warning',
      confirmButtonText: '关闭',
      cancelButtonText: '取消',
    })
    await anomalyApi.update(row.id, { status: 'closed' })
    ElMessage.success('异常单已关闭')
    await loadAnomalies()
  } catch (e: any) {
    if (e !== 'cancel') {
      ElMessage.error(e?.response?.data?.message || '操作失败')
    }
  }
}

function openResolveAnomaly(row: IqcAnomaly) {
  resolveAnomalyId.value = row.id
  resolveForm.resolution = ''
  resolveForm.handler = ''
  resolveAnomalyVisible.value = true
}

async function resolveAnomaly() {
  if (!resolveForm.resolution) {
    ElMessage.warning('请填写解决方案')
    return
  }
  try {
    await anomalyApi.resolve(resolveAnomalyId.value, resolveForm)
    ElMessage.success('异常单已解决')
    resolveAnomalyVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ═══════════════════════════════════════════════════════════════════
// 下拉选项加载
// ═══════════════════════════════════════════════════════════════════

async function loadSuppliers() {
  try {
    const res = await supplierApi.list({ page: 1, pageSize: 200 })
    supplierOptions.value = res.items.map((s: any) => ({ value: s.id, label: `${s.code} - ${s.name}` }))
  } catch (e) {
    console.error('Failed to load suppliers', e)
  }
}

async function loadProducts() {
  try {
    const res = await productApi.list({ page: 1, pageSize: 200 })
    productOptions.value = res.items.map((p: any) => ({ value: p.id, label: `${p.code} - ${p.name}` }))
  } catch (e) {
    console.error('Failed to load products', e)
  }
}

// ═══════════════════════════════════════════════════════════════════
// Lifecycle
// ═══════════════════════════════════════════════════════════════════

onMounted(async () => {
  await loadReceipts()
  await loadSuppliers()
  await loadProducts()
})

// ═══════════════════════════════════════════════════════════════════
// Tab switcher
// ═══════════════════════════════════════════════════════════════════

watchActiveTab()

function watchActiveTab() {
  // Use a watcher-like approach via the sub-tab clicks
}

async function onSubTabChange(tab: string) {
  page.value = 1
  switch (tab) {
    case 'receipts': await loadReceipts(); break
    case 'inspections': await loadInspections(); break
    case 'anomalies': await loadAnomalies(); break
  }
}
</script>

<template>
  <div class="iqc-tab-container">
    <!-- ─── Sub Navigation ─── -->
    <div class="iqc-sub-tabs">
      <el-tabs v-model="activeSubTab" @tab-change="onSubTabChange">
        <el-tab-pane name="receipts">
          <template #label>
            <el-icon style="vertical-align: middle"><Box /></el-icon>
            <span style="vertical-align: middle">来料登记</span>
          </template>
        </el-tab-pane>
        <el-tab-pane name="inspections">
          <template #label>
            <el-icon style="vertical-align: middle"><Document /></el-icon>
            <span style="vertical-align: middle">检验单</span>
          </template>
        </el-tab-pane>
        <el-tab-pane name="anomalies">
          <template #label>
            <el-icon style="vertical-align: middle"><WarningFilled /></el-icon>
            <span style="vertical-align: middle">来料异常</span>
          </template>
        </el-tab-pane>
      </el-tabs>
    </div>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- TAB: 来料登记 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <div v-if="activeSubTab === 'receipts'" class="subtab-content">
      <!-- Toolbar -->
      <div class="toolbar-row">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索单号/批次/供应商/物料..."
          clearable
          style="width: 300px"
          @keyup.enter="loadReceipts"
        />
        <el-button type="primary" @click="openCreateReceipt">+ 新建来料登记</el-button>
        <el-button @click="loadReceipts">刷新</el-button>
      </div>

      <!-- Table -->
      <el-table :data="receipts" stripe style="width: 100%"  v-loading="false">
        <el-table-column prop="receiptNo" label="收货单号" width="160" />
        <el-table-column prop="supplierName" label="供应商" width="150" show-overflow-tooltip />
        <el-table-column prop="productName" label="物料" width="150" show-overflow-tooltip />
        <el-table-column prop="batchNo" label="批次号" width="140" />
        <el-table-column prop="quantity" label="数量" width="80" />
        <el-table-column prop="inspector" label="检验员" width="90" />
        <el-table-column label="状态" width="90">
          <template #default="{ row }">
            <el-tag :type="calculateResultColor(row.status)" size="small" effect="plain">
              {{ statusLabel(row.status, IQC_RECEIPT_STATUS_OPTIONS) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="viewReceiptDetail(row)">详情</el-button>
            <el-button link size="small" type="primary" @click="openEditReceipt(row)">编辑</el-button>
            <el-button
              v-if="row.status === 'pending' || row.status === 'inspecting'"
              link size="small" type="success"
              @click="openNewInspectionForm(row.id)"
            >检验</el-button>
            <el-button link size="small" type="danger" @click="deleteReceipt(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div class="pagination-row">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          layout="total, prev, pager, next"
          size="small"
          @current-change="loadReceipts"
        />
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- TAB: 检验单 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <div v-if="activeSubTab === 'inspections'" class="subtab-content">
      <div class="toolbar-row">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索检验单号..."
          clearable
          style="width: 300px"
          @keyup.enter="loadInspections"
        />
        <el-button @click="loadInspections">刷新</el-button>
      </div>

      <el-table :data="inspections" stripe style="width: 100%" >
        <el-table-column prop="inspectionNo" label="检验单号" width="180" />
        <el-table-column prop="receiptNo" label="来源单号" width="150" />
        <el-table-column prop="sampleSize" label="样本量" width="70" />
        <el-table-column label="Ac/Re" width="70">
          <template #default="{ row }">{{ row.ac }}/{{ row.re }}</template>
        </el-table-column>
        <el-table-column prop="defectQty" label="不合格数" width="80" />
        <el-table-column prop="samplingLevel" label="水平" width="60" />
        <el-table-column label="结果" width="80">
          <template #default="{ row }">
            <el-tag :type="calculateResultColor(row.result)" size="small" effect="plain">
              {{ statusLabel(row.result, IQC_INSPECTION_RESULT_OPTIONS) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="inspector" label="检验员" width="90" />
        <el-table-column prop="inspectedAt" label="检验时间" width="160">
          <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="viewInspectionDetail(row)">详情</el-button>
            <el-button
              v-if="row.result === 'pending'"
              link size="small" type="success"
              @click="openSubmitInspection(row)"
            >提交结果</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-row">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          layout="total, prev, pager, next"
          size="small"
          @current-change="loadInspections"
        />
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- TAB: 来料异常 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <div v-if="activeSubTab === 'anomalies'" class="subtab-content">
      <div class="data-card">
        <div class="panel-header">
          <div class="panel-header-left">
            <el-icon class="panel-icon"><WarningFilled /></el-icon>
            <span>异常记录</span>
          </div>
          <div class="panel-actions">
            <el-button type="primary" size="small" @click="openCreateAnomaly()">
              + 新建异常单
            </el-button>
          </div>
        </div>

        <div class="data-card-body">
          <!-- Search Toolbar -->
          <div class="toolbar-row">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索异常单号..."
              clearable
              style="width: 280px"
            >
              <template #prefix>
                <el-icon><WarningFilled /></el-icon>
              </template>
            </el-input>
            <el-button link @click="loadAnomalies">刷新</el-button>
          </div>

          <!-- Table -->
          <el-table :data="anomalies" stripe style="width: 100%">
            <el-table-column prop="anomalyNo" label="异常单号" width="180" />
            <el-table-column prop="receiptNo" label="来料单号" width="150" />
            <el-table-column label="类型" width="100">
              <template #default="{ row }">
                <el-tag size="small" effect="plain">
                  {{ statusLabel(row.anomalyType, IQC_ANOMALY_TYPE_OPTIONS) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="严重程度" width="100">
              <template #default="{ row }">
                <el-tag :type="row.severity === 'critical' ? 'danger' : row.severity === 'major' ? 'warning' : 'info'" size="small" effect="plain">
                  {{ statusLabel(row.severity, IQC_SEVERITY_OPTIONS) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
            <el-table-column label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="calculateResultColor(row.status)" size="small" effect="plain">
                  {{ statusLabel(row.status, IQC_ANOMALY_STATUS_OPTIONS) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="handler" label="处理人" width="90" />
            <el-table-column prop="createdAt" label="创建时间" width="170">
              <template #default="{ row }">{{ row.createdAt ? row.createdAt.replace('T', ' ').substring(0, 16) : '' }}</template>
            </el-table-column>
            <el-table-column label="操作" width="200" fixed="right">
              <template #default="{ row }">
                <el-button link size="small" type="primary" @click="openEditAnomaly(row)">编辑</el-button>
                <el-button
                  v-if="row.status === 'open' || row.status === 'processing'"
                  link size="small" type="success"
                  @click="openResolveAnomaly(row)"
                >解决</el-button>
                <el-button link size="small" type="danger" @click="deleteAnomaly(row)">关闭</el-button>
              </template>
            </el-table-column>
          </el-table>

          <div v-if="anomalies.length === 0" class="empty-hint">
            暂无异常记录
          </div>

          <!-- Pagination -->
          <div class="pagination-row">
            <el-pagination
              v-model:current-page="page"
              v-model:page-size="pageSize"
              :total="total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              size="small"
              @size-change="loadAnomalies"
              @current-change="loadAnomalies"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 来料登记 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel
      v-model:visible="receiptDialogVisible"
      :title="isEditingReceipt ? '编辑来料登记' : '新建来料登记'"
    >
      <template #body>
        <el-form :model="receiptForm" label-width="100px" >
          <el-form-item label="收货单号" required>
            <el-input v-model="receiptForm.receiptNo" placeholder="如: REC-20260620-001" />
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="供应商" required>
                <el-select v-model="receiptForm.supplierId" filterable placeholder="选择供应商" style="width: 100%">
                  <el-option v-for="opt in supplierOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="物料" required>
                <el-select v-model="receiptForm.productId" filterable placeholder="选择物料" style="width: 100%">
                  <el-option
                    v-for="opt in productOptions"
                    :key="opt.value"
                    :label="opt.label"
                    :value="opt.value"
                  />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="批次号">
                <el-input v-model="receiptForm.batchNo" placeholder="如: BATCH-001" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="数量" required>
                <el-input-number v-model="receiptForm.quantity" :min="1" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="单位">
                <el-input v-model="receiptForm.unit" placeholder="pcs" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="到货日期">
                <el-date-picker v-model="receiptForm.receiptDate" type="date" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="检验员">
                <el-input v-model="receiptForm.inspector" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closeReceiptDialog">取消</el-button>
        <el-button type="primary" @click="saveReceipt">保存</el-button>
      </template>
    </RightPanel>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 来料登记详情 + AI 风险面板 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel v-model:visible="receiptDetailVisible" :title="`来料详情: ${receiptDetail?.receiptNo || ''}`" :width="600" :show-close="true">
      <template #body>
        <template v-if="receiptDetail">
          <el-descriptions :column="2" border style="margin-bottom: 16px">
            <el-descriptions-item label="供应商">{{ receiptDetail.supplierName }}</el-descriptions-item>
            <el-descriptions-item label="物料">{{ receiptDetail.productName }}</el-descriptions-item>
            <el-descriptions-item label="批次号">{{ receiptDetail.batchNo }}</el-descriptions-item>
            <el-descriptions-item label="数量">{{ receiptDetail.quantity }} {{ receiptDetail.unit }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="calculateResultColor(receiptDetail.status)" size="small">
                {{ statusLabel(receiptDetail.status, IQC_RECEIPT_STATUS_OPTIONS) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="到货日期">{{ formatDate(receiptDetail.receiptDate) }}</el-descriptions-item>
          </el-descriptions>

          <!-- AI Risk Panel -->
          <el-card class="risk-panel" :class="aiRiskResult?.level || ''">
            <template #header>
              <div class="card-header">
                <span>
                  <el-icon style="vertical-align: middle"><Cpu /></el-icon>
                  <span style="vertical-align: middle">AI 风险分析</span>
                </span>
                <el-tag v-if="aiRiskResult" :type="aiRiskResult.level === 'high' ? 'danger' : aiRiskResult.level === 'warning' ? 'warning' : 'success'" size="small">
                  {{ aiRiskResult.level === 'high' ? '高风险' : aiRiskResult.level === 'warning' ? '预警' : '低风险' }}
                </el-tag>
              </div>
            </template>
            <div v-if="aiRiskLoading" class="risk-loading">
              <el-skeleton :rows="3" animated />
            </div>
            <div v-else-if="aiRiskResult" class="risk-content">
              <div class="risk-score">
                <span class="score-label">风险评分</span>
                <span :class="['score-value', aiRiskResult.level]">{{ aiRiskResult.score }}/100</span>
              </div>
              <div v-for="(factor, i) in aiRiskResult.factors" :key="i" class="risk-factor">
                <span class="factor-name">{{ factor.name }}</span>
                <span class="factor-desc">{{ factor.description }}</span>
                <el-progress
                  :percentage="factor.impact * 6.67"
                  :stroke-width="6"
                  :color="factor.impact > 10 ? '#e6a23c' : '#67c23a'"
                />
              </div>
              <div v-if="aiRiskResult.recommendations.length > 0" class="risk-recommendations">
                <h4>建议措施</h4>
                <ul>
                  <li v-for="(rec, i) in aiRiskResult.recommendations" :key="i">{{ rec }}</li>
                </ul>
              </div>
            </div>
            <div v-else class="risk-empty">
              <p>暂无风险数据</p>
            </div>
          </el-card>

          <!-- Inspections summary -->
          <el-card v-if="receiptDetail.inspections && receiptDetail.inspections.length > 0" class="trace-card">
            <template #header>检验记录</template>
            <el-table :data="receiptDetail.inspections"  stripe>
              <el-table-column prop="inspectionNo" label="检验单号" />
              <el-table-column prop="sampleSize" label="样本量" width="60" />
              <el-table-column label="Ac/Re" width="70">
                <template #default="{ row }">{{ row.ac }}/{{ row.re }}</template>
              </el-table-column>
              <el-table-column prop="defectQty" label="不良" width="60" />
              <el-table-column label="结果" width="70">
                <template #default="{ row }">
                  <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">
                    {{ row.result === 'pass' ? '合格' : '不合格' }}
                  </el-tag>
                </template>
              </el-table-column>
            </el-table>
          </el-card>

          <!-- Anomalies summary -->
          <el-card v-if="receiptDetail.anomalies && receiptDetail.anomalies.length > 0" class="trace-card">
            <template #header>异常记录</template>
            <el-table :data="receiptDetail.anomalies"  stripe>
              <el-table-column prop="anomalyNo" label="异常单号" />
              <el-table-column label="严重程度" width="80">
                <template #default="{ row }">
                  <el-tag :type="row.severity === 'critical' ? 'danger' : 'warning'" size="small">
                    {{ statusLabel(row.severity, IQC_SEVERITY_OPTIONS) }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="status" label="状态" width="80">
                <template #default="{ row }">
                  <el-tag :type="row.status === 'resolved' ? 'success' : 'danger'" size="small">
                    {{ statusLabel(row.status, IQC_ANOMALY_STATUS_OPTIONS) }}
                  </el-tag>
                </template>
              </el-table-column>
            </el-table>
          </el-card>
        </template>
      </template>
    </RightPanel>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 新建检验单 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel v-model:visible="newInspectionVisible" title="新建检验单">
      <template #body>
        <el-form :model="newInspectionForm" label-width="120px" >
          <el-form-item label="来料登记ID">
            <el-input-number v-model="newInspectionForm.receiptId" :min="1" style="width: 100%" />
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="样本量" required>
                <el-input-number v-model="newInspectionForm.sampleSize" :min="1" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Ac" required>
                <el-input-number v-model="newInspectionForm.ac" :min="0" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Re" required>
                <el-input-number v-model="newInspectionForm.re" :min="1" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="检验水平">
                <el-select v-model="newInspectionForm.samplingLevel" style="width: 100%">
                  <el-option v-for="opt in SAMPLING_LEVEL_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="AQL 值">
                <el-input-number v-model="newInspectionForm.aqlValue" :min="0.01" :step="0.1" :precision="2" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closeNewInspection">取消</el-button>
        <el-button type="primary" @click="createInspection">创建</el-button>
      </template>
    </RightPanel>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 检验单详情 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel v-model:visible="inspectionDetailVisible" :title="`检验单: ${inspectionDetail?.inspectionNo || ''}`" :width="500" :show-close="true">
      <template #body>
        <template v-if="inspectionDetail">
          <el-descriptions :column="2" border style="margin-bottom: 16px">
            <el-descriptions-item label="来源">{{ inspectionDetail.receiptNo }}</el-descriptions-item>
            <el-descriptions-item label="供应商">{{ inspectionDetail.supplierName }}</el-descriptions-item>
            <el-descriptions-item label="物料">{{ inspectionDetail.productName }}</el-descriptions-item>
            <el-descriptions-item label="抽样方案">
              n={{ inspectionDetail.sampleSize }}, Ac={{ inspectionDetail.ac }}, Re={{ inspectionDetail.re }}
            </el-descriptions-item>
            <el-descriptions-item label="结果">
              <el-tag :type="calculateResultColor(inspectionDetail.result)" size="small">
                {{ statusLabel(inspectionDetail.result, IQC_INSPECTION_RESULT_OPTIONS) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="检验员">{{ inspectionDetail.inspector }}</el-descriptions-item>
          </el-descriptions>

          <h4 style="margin-bottom: 8px">检验项目</h4>
          <el-table :data="inspectionDetail.items || []"  stripe>
            <el-table-column prop="itemName" label="项目" min-width="120" />
            <el-table-column prop="measuredValue" label="实测值" width="90" />
            <el-table-column label="规格" width="130">
              <template #default="{ row }">
                {{ row.lsl ?? '-' }} ~ {{ row.usl ?? '-' }}
              </template>
            </el-table-column>
            <el-table-column label="结果" width="70">
              <template #default="{ row }">
                <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">
                  {{ row.result === 'pass' ? 'OK' : 'NG' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="remark" label="备注" show-overflow-tooltip />
          </el-table>
        </template>
      </template>
    </RightPanel>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 提交检验结果 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel v-model:visible="submitDialogVisible" title="提交检验结果">
      <template #body>
        <el-form label-width="100px" >
          <el-form-item label="检验员">
            <el-input v-model="submitInspector" placeholder="检验员姓名" style="width: 200px" />
          </el-form-item>
          <el-form-item label="检验项目">
            <div class="submit-items">
              <div v-for="(item, index) in submitItems" :key="index" class="submit-item-row">
                <el-input v-model="item.itemName" placeholder="项目名称"  style="width: 150px" />
                <el-input-number
                  v-model="item.measuredValue"
                  :precision="4"
                  :step="0.1"
                  size="small"
                  style="width: 140px"
                  placeholder="实测值"
                />
                <el-select v-model="item.result"  style="width: 100px">
                  <el-option label="合格" value="pass" />
                  <el-option label="不合格" value="fail" />
                  <el-option label="待定" value="pending" />
                </el-select>
                <el-input v-model="item.remark" placeholder="备注"  style="width: 120px" />
                <el-button link size="small" type="danger" @click="removeSubmitItem(index)">删除</el-button>
              </div>
            </div>
            <el-button size="small" @click="addSubmitItem" style="margin-top: 8px">+ 添加项目</el-button>
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closeSubmitDialog">取消</el-button>
        <el-button type="primary" @click="submitInspection">提交判定</el-button>
      </template>
    </RightPanel>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 新建/编辑异常单 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel v-model:visible="anomalyDialogVisible" :title="isEditingAnomaly ? '编辑异常单' : '新建异常单'" :width="560">
      <template #body>
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Document /></el-icon>
            <span class="dialog-section-title">来料信息</span>
          </div>
          <el-form :model="anomalyForm" label-width="100px">
            <el-form-item label="来料登记ID" required>
              <el-input-number v-model="anomalyForm.receiptId" :min="1" style="width: 100%" />
            </el-form-item>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><WarningFilled /></el-icon>
            <span class="dialog-section-title">异常信息</span>
          </div>
          <el-form :model="anomalyForm" label-width="100px">
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="异常类型" required>
                  <el-select v-model="anomalyForm.anomalyType" style="width: 100%">
                    <el-option
                      v-for="opt in IQC_ANOMALY_TYPE_OPTIONS"
                      :key="opt.value"
                      :label="opt.label"
                      :value="opt.value"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="严重程度" required>
                  <el-select v-model="anomalyForm.severity" style="width: 100%">
                    <el-option
                      v-for="opt in IQC_SEVERITY_OPTIONS"
                      :key="opt.value"
                      :label="opt.label"
                      :value="opt.value"
                    />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="描述" required>
              <el-input v-model="anomalyForm.description" type="textarea" :rows="3" placeholder="请描述异常详情..." />
            </el-form-item>
            <el-form-item label="处理人">
              <el-input v-model="anomalyForm.handler" placeholder="请输入处理人姓名" />
            </el-form-item>
          </el-form>
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="anomalyDialogVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="saveAnomaly">
            {{ isEditingAnomaly ? '保存修改' : '创建' }}
          </el-button>
        </div>
      </template>
    </RightPanel>

    <!-- ══════════════════════════════════════════════════════════════ -->
    <!-- PANEL: 解决异常单 -->
    <!-- ══════════════════════════════════════════════════════════════ -->
    <RightPanel v-model:visible="resolveAnomalyVisible" title="解决异常单" :width="520">
      <template #body>
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Edit /></el-icon>
            <span class="dialog-section-title">解决方案</span>
          </div>
          <el-form :model="resolveForm" label-width="80px">
            <el-form-item label="解决方案" required>
              <el-input v-model="resolveForm.resolution" type="textarea" :rows="5" placeholder="请描述解决方案..." />
            </el-form-item>
            <el-form-item label="处理人">
              <el-input v-model="resolveForm.handler" placeholder="请输入处理人姓名" />
            </el-form-item>
          </el-form>
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="resolveAnomalyVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="resolveAnomaly">确认解决</el-button>
        </div>
      </template>
    </RightPanel>

    <!-- 抽样方案计算器 (shared) -->
    <SamplingPlanCalculator v-if="activeSubTab === 'receipts' || activeSubTab === 'inspections'" mode="panel" />
  </div>
</template>

<style scoped>
.iqc-tab-container {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.iqc-sub-tabs {
  margin-bottom: 8px;
}

.subtab-content {
  flex: 1;
  display: flex;
  flex-direction: column;
}

/* ─── Data Card ──────────────────────────────── */
.data-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  overflow: hidden;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.data-card-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  padding: 12px;
  overflow-y: auto;
}

/* ─── Panel Header ───────────────────────────── */
.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  border-bottom: 1px solid var(--el-border-color-light);
  background: var(--el-fill-color-blank);
  flex-shrink: 0;
}

.panel-header-left {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.panel-icon {
  font-size: 16px;
  color: var(--el-color-danger);
}

.panel-actions {
  display: flex;
  gap: 8px;
}

/* ─── Dialog Sections ─────────────────────────── */
.dialog-section {
  margin-bottom: 16px;
}

.dialog-section:last-of-type {
  margin-bottom: 0;
}

.dialog-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
}

.dialog-section-icon {
  font-size: 15px;
  color: var(--el-color-danger);
}

.dialog-section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}

/* ─── Common ──────────────────────────────────── */
.empty-hint {
  text-align: center;
  color: var(--el-text-color-disabled);
  font-size: 12px;
  padding: 20px 0;
}

/* ─── Table ───────────────────────────────────── */
.el-table :deep(.el-table__header-wrapper th) {
  background: var(--el-fill-color-blank) !important;
  font-weight: 600;
  font-size: 13px;
  color: var(--el-text-color-regular);
}

.toolbar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: 12px 0;
}

/* Risk Panel */
.risk-panel {
  margin-bottom: 16px;
}

.risk-panel.high {
  border-left: 3px solid #f56c6c;
}

.risk-panel.warning {
  border-left: 3px solid #e6a23c;
}

.risk-panel.low {
  border-left: 3px solid #67c23a;
}

.risk-loading {
  padding: 16px;
}

.risk-content {
  padding: 4px 0;
}

.risk-score {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.score-label {
  font-weight: 600;
  font-size: 14px;
}

.score-value {
  font-size: 24px;
  font-weight: 700;
}

.score-value.high { color: #f56c6c; }
.score-value.warning { color: #e6a23c; }
.score-value.low { color: #67c23a; }

.risk-factor {
  margin-bottom: 10px;
}

.factor-name {
  font-weight: 500;
  font-size: 13px;
  display: block;
}

.factor-desc {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  display: block;
  margin-bottom: 4px;
}

.risk-recommendations {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--el-border-color-light);
}

.risk-recommendations h4 {
  margin: 0 0 6px;
  font-size: 14px;
}

.risk-recommendations ul {
  margin: 0;
  padding-left: 20px;
}

.risk-recommendations li {
  font-size: 13px;
  line-height: 1.8;
}

.risk-empty {
  text-align: center;
  padding: 20px;
  color: var(--el-text-color-secondary);
}

/* Trace */
.trace-result {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.trace-card {
  margin-bottom: 0;
}

/* Score */
.score-card {
  margin-top: 12px;
  max-width: 600px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

/* Submit items */
.submit-items {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.submit-item-row {
  display: flex;
  align-items: center;
  gap: 6px;
}

.empty-state {
  text-align: center;
  padding: 40px;
  color: var(--el-text-color-secondary);
}
</style>

<style>
/* Non-scoped styles for Element Plus link buttons in table */
.subtab-content .el-button.is-link {
  border: none !important;
  box-shadow: none !important;
  background-color: transparent !important;
}

.subtab-content .el-button.is-link:active,
.subtab-content .el-button.is-link:focus,
.subtab-content .el-button.is-link:focus-visible,
.subtab-content .el-button.is-link:hover {
  color: inherit !important;
  background-color: transparent !important;
  border: none !important;
  box-shadow: none !important;
  outline: none !important;
}
</style>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  receiptApi, inspectionApi, aiRiskApi, samplingPlanApi
} from '@/api/iqc'
import { Box, Document, WarningFilled, DataAnalysis, ScaleToOriginal, Search, Refresh, View, TrendCharts, CaretBottom, Cpu, CircleCheck, CircleClose } from '@element-plus/icons-vue'
import { supplierApi, productApi } from '@/api/basicData'
import type { PagedResult } from '@/types/basicData'
import type {
  IqcReceipt, IqcReceiptDetail, CreateIqcReceipt, UpdateIqcReceipt,
  IqcInspection, CreateIqcInspection, IqcInspectionDetail,
  AiRiskScore, SamplingPlan, SamplingPlanRequest,
} from '@/types/iqc'
import {
  IQC_RECEIPT_STATUS_OPTIONS, IQC_INSPECTION_RESULT_OPTIONS,
  SAMPLING_LEVEL_OPTIONS,
} from '@/types/iqc'

defineOptions({ name: 'IqcReceiptsPage' })

// ─── Shared State ──────────────────────────────────────
const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

// ─── 来料登记 ────────────────────────────────────────
const receipts = ref<IqcReceipt[]>([])
const receiptDialogVisible = ref(false)
const isEditingReceipt = ref(false)
const currentReceiptId = ref<number | null>(null)
const receiptForm = reactive<CreateIqcReceipt>({
  receiptNo: '', supplierId: 0, productId: 0, batchNo: '',
  quantity: 0, unit: '', receiptDate: '', inspector: ''
})
const receiptDetail = ref<IqcReceiptDetail | null>(null)
const receiptDetailVisible = ref(false)
const supplierOptions = ref<Array<{ value: number; label: string }>>([])
const productOptions = ref<Array<{ value: number; label: string }>>([])

// ─── AI Risk ──────────────────────────────────────────
const aiRiskResult = ref<AiRiskScore | null>(null)
const aiRiskLoading = ref(false)
const selectedReceiptId = ref<number | null>(null)

// ─── Sampling Plan ────────────────────────────────────
const samplingPlanResult = ref<SamplingPlan | null>(null)
const samplingCollapsed = ref(false)
const samplingPlanForm = reactive<SamplingPlanRequest>({
  lotSize: 100, samplingLevel: 'II', aqlValue: 1.0
})

// ─── View Mode ──────────────────────────────────────────
const viewMode = ref<'table' | 'card'>('table')
const statusFilter = ref('')

const stats = computed(() => {
  const items = receipts.value
  return {
    pending: items.filter(i => i.status === 'pending').length,
    inspecting: items.filter(i => i.status === 'inspecting').length,
    qualified: items.filter(i => i.status === 'qualified').length,
    anomaly: items.filter(i => i.status === 'unqualified' || i.status === 'anomaly').length,
  }
})

// ─── Helpers ──────────────────────────────────────────
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

const statusLabel = (status: string, options: any[]) => {
  const opt = options.find((o: any) => o.value === status)
  return opt?.label || status
}

// ─── CRUD ────────────────────────────────────────────
async function loadReceipts() {
  try {
    const res = await receiptApi.list({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value, status: statusFilter.value || undefined
    })
    receipts.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load receipts', e)
  }
}

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
  receiptDialogVisible.value = true
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
  receiptDialogVisible.value = true
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
    receiptDialogVisible.value = false
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

// ─── AI Risk ──────────────────────────────────────────
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

// ─── Sampling Plan ────────────────────────────────────
async function calculateSamplingPlan() {
  try {
    samplingPlanResult.value = await samplingPlanApi.calculate(samplingPlanForm)
  } catch (e) {
    ElMessage.error('抽样方案计算失败')
  }
}

// ─── Inspection creation from receipt ────────────────
const newInspectionForm = reactive<CreateIqcInspection>({
  receiptId: 0, sampleSize: 0, ac: 0, re: 0, samplingLevel: 'II', aqlValue: 1.0
})
const newInspectionVisible = ref(false)

function openNewInspection(receiptId: number) {
  newInspectionForm.receiptId = receiptId
  newInspectionForm.sampleSize = 0
  newInspectionForm.ac = 0
  newInspectionForm.re = 0
  newInspectionForm.samplingLevel = 'II'
  newInspectionForm.aqlValue = 1.0
  newInspectionVisible.value = true
}

async function createInspection() {
  try {
    await inspectionApi.create(newInspectionForm)
    ElMessage.success('检验单已创建')
    newInspectionVisible.value = false
    await loadReceipts()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
}

onMounted(async () => {
  await loadReceipts()
  await loadSuppliers()
  await loadProducts()
  await calculateSamplingPlan()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-left">
        <el-icon class="page-header-icon"><Box /></el-icon>
        <div class="page-header-text">
          <h1>来料检验</h1>
          <p>管理供应商来料登记与检验流程</p>
        </div>
      </div>
      <div class="page-header-right">
        <el-button :icon="Refresh" circle @click="loadReceipts" title="刷新" />
        <el-radio-group v-model="viewMode" size="default" class="view-mode-switch">
          <el-radio-button value="table" :icon="Document" label="列表" />
          <el-radio-button value="card" :icon="DataAnalysis" label="卡片" />
        </el-radio-group>
      </div>
    </div>

    <!-- Stats Bar -->
    <div v-if="receipts.length > 0" class="stats-bar">
      <div class="stat-item stat-pending">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.pending }}</div>
          <div class="stat-label">待检验</div>
        </div>
      </div>
      <div class="stat-item stat-inspecting">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.inspecting }}</div>
          <div class="stat-label">检验中</div>
        </div>
      </div>
      <div class="stat-item stat-qualified">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.qualified }}</div>
          <div class="stat-label">已合格</div>
        </div>
      </div>
      <div class="stat-item stat-anomaly">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.anomaly }}</div>
          <div class="stat-label">异常</div>
        </div>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="toolbar-row">
      <div class="toolbar-left">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索单号/批次/供应商/物料..."
          clearable
          :prefix-icon="Search"
          style="width: 300px"
          @keyup.enter="loadReceipts"
        />
        <el-select
          v-model="statusFilter"
          clearable
          placeholder="状态筛选"
          style="width: 140px"
          @change="loadReceipts"
        >
          <el-option
            v-for="opt in IQC_RECEIPT_STATUS_OPTIONS"
            :key="opt.value"
            :label="opt.label"
            :value="opt.value"
          />
        </el-select>
      </div>
      <div class="toolbar-right">
        <el-button type="primary" :icon="TrendCharts" @click="openCreateReceipt">
          新建来料登记
        </el-button>
      </div>
    </div>

    <!-- Data Card (Table Wrapper) -->
    <div class="data-card">
      <div class="data-card-header">
        <span class="data-card-title">
          <el-icon><Document /></el-icon>
          来料清单
        </span>
        <span class="data-card-count">共 {{ total }} 条</span>
      </div>
      <el-table
        :data="receipts"
        stripe
        style="width: 100%"
        v-loading="false"
        @row-click="viewReceiptDetail"
        :row-class-name="'receipt-row'"
        :header-cell-class-name="'receipt-header-cell'"
      >
        <el-table-column prop="receiptNo" label="收货单号" width="160" fixed />
        <el-table-column prop="supplierName" label="供应商" width="160" show-overflow-tooltip />
        <el-table-column prop="productName" label="物料名称" width="160" show-overflow-tooltip />
        <el-table-column prop="batchNo" label="批次号" width="140" />
        <el-table-column prop="quantity" label="数量" width="90" align="right">
          <template #default="{ row }">
            <span class="quantity-cell">{{ row.quantity }}</span>
            <span class="unit-cell">{{ row.unit }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="inspector" label="检验员" width="100" />
        <el-table-column prop="receiptDate" label="到货日期" width="130">
          <template #default="{ row }">
            <span class="date-cell">{{ formatDate(row.receiptDate) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <span class="status-dot" :class="calculateResultColor(row.status)"></span>
            <el-tag :type="calculateResultColor(row.status)" size="small" effect="plain" round>
              {{ statusLabel(row.status, IQC_RECEIPT_STATUS_OPTIONS) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <div class="action-group">
              <el-button link size="small" type="primary" @click.stop="viewReceiptDetail(row)">
                <el-icon><Document /></el-icon> 详情
              </el-button>
              <el-button link size="small" type="primary" @click.stop="openEditReceipt(row)">
                <el-icon><WarningFilled /></el-icon> 编辑
              </el-button>
              <el-button
                v-if="row.status === 'pending' || row.status === 'inspecting'"
                link size="small" type="success" @click.stop="openNewInspection(row.id)"
              >
                <el-icon><ScaleToOriginal /></el-icon> 检验
              </el-button>
              <el-button link size="small" type="danger" @click.stop="deleteReceipt(row)">
                <el-icon><WarningFilled /></el-icon> 删除
              </el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div class="pagination-row">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadReceipts"
          @current-change="loadReceipts"
        />
      </div>

      <!-- Sampling Calculator (Collapsible) -->
      <el-collapse-transition>
        <div v-show="true" class="sampling-section">
          <div class="sampling-header" @click="samplingCollapsed = !samplingCollapsed">
            <span>
              <el-icon><ScaleToOriginal /></el-icon>
              GB/T 2828.1 抽样方案计算器
            </span>
            <el-icon class="collapse-arrow" :class="{ collapsed: samplingCollapsed }">
              <CaretBottom />
            </el-icon>
          </div>
          <div v-show="!samplingCollapsed" class="sampling-body">
            <el-row :gutter="16">
              <el-col :span="6">
                <el-form-item label="批量">
                  <el-input-number v-model="samplingPlanForm.lotSize" :min="1" :max="500000" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="检验水平">
                  <el-select v-model="samplingPlanForm.samplingLevel" style="width: 100%">
                    <el-option v-for="opt in SAMPLING_LEVEL_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="AQL 值">
                  <el-input-number v-model="samplingPlanForm.aqlValue" :min="0.01" :step="0.1" :precision="2" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="6" style="display: flex; align-items: flex-start; padding-top: 2px">
                <el-button type="primary" size="small" @click="calculateSamplingPlan">计算</el-button>
              </el-col>
            </el-row>
            <div v-if="samplingPlanResult" class="sampling-result">
              <div class="result-tag">
                <el-icon><Document /></el-icon> 字母代码: {{ samplingPlanResult.sampleCode }}
              </div>
              <div class="result-tag result-success">
                <el-icon><CircleCheck /></el-icon> 样本量: {{ samplingPlanResult.sampleSize }}
              </div>
              <div class="result-tag result-warning">
                <el-icon><WarningFilled /></el-icon> Ac: {{ samplingPlanResult.ac }}
              </div>
              <div class="result-tag result-danger">
                <el-icon><CircleClose /></el-icon> Re: {{ samplingPlanResult.re }}
              </div>
            </div>
          </div>
        </div>
      </el-collapse-transition>
    </div>

    <!-- Dialog: 来料登记 -->
    <el-dialog
      v-model="receiptDialogVisible"
      :title="isEditingReceipt ? '编辑来料登记' : '新建来料登记'"
      width="620px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="receiptForm" label-width="90px" size="default">
        <el-divider content-position="left">
          <el-icon><Document /></el-icon> 基本信息
        </el-divider>
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
                <el-option v-for="opt in productOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">
          <el-icon><TrendCharts /></el-icon> 来料信息
        </el-divider>
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
        <el-divider content-position="left">
          <el-icon><WarningFilled /></el-icon> 检验信息
        </el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="到货日期">
              <el-date-picker v-model="receiptForm.receiptDate" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="检验员">
              <el-input v-model="receiptForm.inspector" placeholder="检验员姓名" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="receiptDialogVisible = false">取消</el-button>
          <el-button type="primary" @click="saveReceipt">
            <el-icon><CircleCheck /></el-icon> 保存
          </el-button>
        </div>
      </template>
    </el-dialog>

    <!-- Drawer: 来料详情 + AI Risk -->
    <el-drawer
      v-model="receiptDetailVisible"
      size="640px"
      :show-close="true"
      :with-header="true"
    >
      <template #header>
        <div class="drawer-header">
          <el-icon class="drawer-header-icon"><Box /></el-icon>
          <div class="drawer-header-text">
            <span class="drawer-title">来料详情</span>
            <span class="drawer-subtitle">{{ receiptDetail?.receiptNo }}</span>
          </div>
          <el-tag v-if="receiptDetail" :type="calculateResultColor(receiptDetail.status)" effect="dark" round>
            {{ statusLabel(receiptDetail.status, IQC_RECEIPT_STATUS_OPTIONS) }}
          </el-tag>
        </div>
      </template>

      <template v-if="receiptDetail">
        <!-- Basic Info -->
        <div class="drawer-section">
          <div class="section-header">
            <el-divider style="margin: 0">
              <el-icon><Document /></el-icon> 基本信息
            </el-divider>
          </div>
          <el-descriptions :column="2" border size="default">
            <el-descriptions-item label="供应商">
              <span class="desc-highlight">{{ receiptDetail.supplierName }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="物料">
              <span class="desc-highlight">{{ receiptDetail.productName }}</span>
            </el-descriptions-item>
            <el-descriptions-item label="批次号">{{ receiptDetail.batchNo || '-' }}</el-descriptions-item>
            <el-descriptions-item label="数量">
              {{ receiptDetail.quantity }} {{ receiptDetail.unit || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="检验员">{{ receiptDetail.inspector || '-' }}</el-descriptions-item>
            <el-descriptions-item label="到货日期">{{ formatDate(receiptDetail.receiptDate) }}</el-descriptions-item>
          </el-descriptions>
        </div>

        <!-- AI Risk Panel -->
        <div class="drawer-section">
          <el-divider content-position="left">
            <el-icon><Cpu /></el-icon> AI 风险分析
          </el-divider>
          <el-card class="risk-panel" shadow="never">
            <div v-if="aiRiskLoading" class="risk-loading">
              <el-skeleton :rows="3" animated />
            </div>
            <div v-else-if="aiRiskResult" class="risk-content">
              <div class="risk-score-card">
                <div class="risk-score-circle">
                  <div :class="['risk-score-value', aiRiskResult.level]">
                    {{ aiRiskResult.score }}
                  </div>
                  <div class="risk-score-suffix">/ 100</div>
                </div>
                <div class="risk-level-badge">
                  <el-tag :type="aiRiskResult.level === 'high' ? 'danger' : aiRiskResult.level === 'warning' ? 'warning' : 'success'" effect="dark" size="large" round>
                    {{ aiRiskResult.level === 'high' ? '高风险' : aiRiskResult.level === 'warning' ? '预警' : '低风险' }}
                  </el-tag>
                </div>
              </div>
              <div class="risk-factors">
                <div v-for="(factor, i) in aiRiskResult.factors" :key="i" class="risk-factor">
                  <div class="risk-factor-header">
                    <span class="factor-name">{{ factor.name }}</span>
                    <span class="factor-impact">影响力 {{ factor.impact }}</span>
                  </div>
                  <span class="factor-desc">{{ factor.description }}</span>
                  <el-progress
                    :percentage="factor.impact * 6.67"
                    :stroke-width="8"
                    :color="factor.impact > 10 ? '#e6a23c' : '#67c23a'"
                    :format="() => ''"
                  />
                </div>
              </div>
              <div v-if="aiRiskResult.recommendations.length > 0" class="risk-recommendations">
                <h4>
                  <el-icon><WarningFilled /></el-icon> 建议措施
                </h4>
                <ul>
                  <li v-for="(rec, i) in aiRiskResult.recommendations" :key="i">{{ rec }}</li>
                </ul>
              </div>
            </div>
            <div v-else class="risk-empty">
              <el-empty description="暂无风险数据" :image-size="60" />
            </div>
          </el-card>
        </div>

        <!-- Inspections -->
        <div v-if="receiptDetail.inspections && receiptDetail.inspections.length > 0" class="drawer-section">
          <el-divider content-position="left">
            <el-icon><DataAnalysis /></el-icon> 检验记录
            <span class="section-count">{{ receiptDetail.inspections.length }} 条</span>
          </el-divider>
          <el-table :data="receiptDetail.inspections" stripe size="small">
            <el-table-column prop="inspectionNo" label="检验单号" show-overflow-tooltip />
            <el-table-column prop="sampleSize" label="样本量" width="70" align="right" />
            <el-table-column label="Ac / Re" width="70" align="center">
              <template #default="{ row }">
                <span class="ac-re-cell">{{ row.ac }}/{{ row.re }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="defectQty" label="不良数" width="70" align="right" />
            <el-table-column label="结果" width="80" align="center">
              <template #default="{ row }">
                <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small" round>
                  {{ row.result === 'pass' ? '合格' : '不合格' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </div>

        <!-- Anomalies -->
        <div v-if="receiptDetail.anomalies && receiptDetail.anomalies.length > 0" class="drawer-section">
          <el-divider content-position="left">
            <el-icon><WarningFilled /></el-icon> 异常记录
            <span class="section-count">{{ receiptDetail.anomalies.length }} 条</span>
          </el-divider>
          <el-table :data="receiptDetail.anomalies" stripe size="small">
            <el-table-column prop="anomalyNo" label="异常单号" show-overflow-tooltip />
            <el-table-column label="严重程度" width="90" align="center">
              <template #default="{ row }">
                <el-tag :type="row.severity === 'critical' ? 'danger' : 'warning'" size="small" round>
                  {{ row.severity === 'critical' ? '严重' : row.severity === 'major' ? '主要' : '轻微' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="90" align="center">
              <template #default="{ row }">
                <el-tag :type="row.status === 'resolved' ? 'success' : 'danger'" size="small" round>
                  {{ row.status === 'resolved' ? '已解决' : '待处理' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </template>
    </el-drawer>

    <!-- Dialog: 新建检验单 -->
    <el-dialog
      v-model="newInspectionVisible"
      title="新建检验单"
      width="560px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form :model="newInspectionForm" label-width="100px">
        <el-divider content-position="left">
          <el-icon><TrendCharts /></el-icon> 抽样参数
        </el-divider>
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
        <el-divider content-position="left">
          <el-icon><ScaleToOriginal /></el-icon> 检验标准
        </el-divider>
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
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="newInspectionVisible = false">取消</el-button>
          <el-button type="primary" @click="createInspection">
            <el-icon><CircleCheck /></el-icon> 创建检验单
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  min-height: 0;
  padding: 20px;
  background: var(--el-bg-color);
}

/* ─── Page Header ─── */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.page-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}
.page-header-icon {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-color-primary-light-9);
  border-radius: 10px;
  color: var(--el-color-primary);
  font-size: 22px;
}
.page-header-text h1 {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  line-height: 1.3;
}
.page-header-text p {
  margin: 2px 0 0;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.page-header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}
.view-mode-switch {
  border: 1px solid var(--el-border-color);
  border-radius: 6px;
}

/* ─── Stats Bar ─── */
.stats-bar {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}
.stat-item {
  display: flex;
  align-items: center;
  background: #fff;
  border-radius: 8px;
  padding: 16px 18px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  transition: box-shadow 0.2s, transform 0.2s;
}
.stat-item:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
}
.stat-accent {
  width: 4px;
  height: 36px;
  border-radius: 2px;
  margin-right: 14px;
  flex-shrink: 0;
}
.stat-pending .stat-accent { background: var(--el-color-info); }
.stat-inspecting .stat-accent { background: var(--el-color-warning); }
.stat-qualified .stat-accent { background: var(--el-color-success); }
.stat-anomaly .stat-accent { background: var(--el-color-danger); }
.stat-content {
  display: flex;
  flex-direction: column;
}
.stat-value {
  font-size: 28px;
  font-weight: 700;
  line-height: 1.2;
}
.stat-pending .stat-value { color: var(--el-color-info); }
.stat-inspecting .stat-value { color: var(--el-color-warning); }
.stat-qualified .stat-value { color: var(--el-color-success); }
.stat-anomaly .stat-value { color: var(--el-color-danger); }
.stat-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  margin-top: 2px;
}

/* ─── Toolbar ─── */
.toolbar-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}
.toolbar-left {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.toolbar-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* ─── Data Card ─── */
.data-card {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  overflow: hidden;
  margin-bottom: 16px;
}
.data-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
}
.data-card-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}
.data-card-count {
  font-size: 13px;
  color: var(--el-text-color-regular);
}
.receipt-header-cell {
  background: var(--el-fill-color-light) !important;
  font-weight: 600;
  font-size: 13px;
}
:deep(.receipt-row:hover) {
  background-color: var(--el-fill-color-light) !important;
}
.quantity-cell {
  font-weight: 600;
  color: var(--el-text-color-primary);
}
.unit-cell {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-left: 2px;
}
.date-cell {
  font-size: 12px;
  color: var(--el-text-color-regular);
}
.status-dot {
  display: inline-block;
  width: 6px;
  height: 6px;
  border-radius: 50%;
  margin-right: 4px;
  vertical-align: middle;
}
.status-dot.info { background: var(--el-color-info); }
.status-dot.warning { background: var(--el-color-warning); }
.status-dot.success { background: var(--el-color-success); }
.status-dot.danger { background: var(--el-color-danger); }
.action-group {
  display: flex;
  align-items: center;
  gap: 2px;
}

/* ─── Pagination ─── */
.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: 14px 20px;
  border-top: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
}

/* ─── Sampling Section ─── */
.sampling-section {
  border-top: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
}
.sampling-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 20px;
  font-size: 14px;
  font-weight: 500;
  color: var(--el-text-color-regular);
  cursor: pointer;
  user-select: none;
  transition: background 0.2s;
}
.sampling-header:hover {
  background: var(--el-fill-color-light);
}
.sampling-header .el-icon {
  margin-right: 6px;
  vertical-align: middle;
}
.collapse-arrow {
  transition: transform 0.3s;
}
.collapse-arrow.collapsed {
  transform: rotate(-90deg);
}
.sampling-body {
  padding: 16px 20px 20px;
}

.sampling-result {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed var(--el-border-color-lighter);
}
.result-tag {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 14px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  color: var(--el-text-color-regular);
}
.result-success {
  background: var(--el-color-success-light-9);
  color: var(--el-color-success);
}
.result-warning {
  background: var(--el-color-warning-light-9);
  color: var(--el-color-warning);
}
.result-danger {
  background: var(--el-color-danger-light-9);
  color: var(--el-color-danger);
}

/* ─── Dialog Footer ─── */
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

/* ─── Drawer ─── */
.drawer-header {
  display: flex;
  align-items: center;
  gap: 12px;
}
.drawer-header-icon {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-color-primary-light-9);
  border-radius: 8px;
  color: var(--el-color-primary);
  font-size: 18px;
  flex-shrink: 0;
}
.drawer-header-text {
  display: flex;
  flex-direction: column;
}
.drawer-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  line-height: 1.3;
}
.drawer-subtitle {
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.drawer-section {
  margin-bottom: 8px;
}
.section-header {
  margin-bottom: 8px;
}
.section-count {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-left: 6px;
  font-weight: 400;
}
.desc-highlight {
  font-weight: 500;
  color: var(--el-text-color-primary);
}
.ac-re-cell {
  font-weight: 600;
  font-family: monospace;
}

/* ─── Risk Panel ─── */
.risk-panel {
  border: none;
  border-radius: 8px;
}
.risk-panel.high { border-left: 3px solid #f56c6c; }
.risk-panel.warning { border-left: 3px solid #e6a23c; }
.risk-panel.low { border-left: 3px solid #67c23a; }
.risk-loading { padding: 16px; }
.risk-content { padding: 4px 0; }
.risk-score-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.risk-score-circle {
  display: flex;
  align-items: baseline;
  gap: 4px;
}
.risk-score-value {
  font-size: 40px;
  font-weight: 800;
  line-height: 1;
}
.risk-score-value.high { color: #f56c6c; }
.risk-score-value.warning { color: #e6a23c; }
.risk-score-value.low { color: #67c23a; }
.risk-score-suffix {
  font-size: 16px;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}
.risk-factors {
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.risk-factor {
  padding: 10px 12px;
  background: var(--el-fill-color-blank);
  border-radius: 6px;
  border: 1px solid var(--el-border-color-lighter);
}
.risk-factor-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 4px;
}
.factor-name {
  font-weight: 600;
  font-size: 13px;
  color: var(--el-text-color-primary);
}
.factor-impact {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}
.factor-desc {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  display: block;
  margin-bottom: 6px;
}
.risk-recommendations {
  margin-top: 16px;
  padding: 12px 14px;
  background: var(--el-color-warning-light-9);
  border-radius: 6px;
  border: 1px solid var(--el-color-warning-light-5);
}
.risk-recommendations h4 {
  margin: 0 0 8px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-color-warning);
  display: flex;
  align-items: center;
  gap: 4px;
}
.risk-recommendations ul {
  margin: 0;
  padding-left: 20px;
}
.risk-recommendations li {
  font-size: 13px;
  line-height: 1.8;
  color: var(--el-text-color-regular);
}
.risk-empty {
  text-align: center;
  padding: 16px 0;
}
</style>

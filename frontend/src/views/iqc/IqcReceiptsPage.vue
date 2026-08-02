<script setup lang="ts">
import { ref, reactive, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  receiptApi, inspectionApi, aiRiskApi
} from '@/api/iqc'
import { Box, Document, WarningFilled, DataAnalysis, ScaleToOriginal, Search, Refresh, TrendCharts, Cpu, Edit, Delete, Plus } from '@element-plus/icons-vue'
import { supplierApi, productApi } from '@/api/basicData'
import type { PagedResult } from '@/types/basicData'
import type {
  IqcReceipt, IqcReceiptDetail, CreateIqcReceipt, UpdateIqcReceipt,
  IqcInspection, CreateIqcInspection, IqcInspectionDetail,
  AiRiskScore,
} from '@/types/iqc'
import {
  IQC_RECEIPT_STATUS_OPTIONS, IQC_INSPECTION_RESULT_OPTIONS, SAMPLING_LEVEL_OPTIONS,
} from '@/types/iqc'
import SamplingPlanCalculator from '@/components/SamplingPlanCalculator.vue'
import RightPanel from '@/components/layout/RightPanel.vue'

defineOptions({ name: 'IqcReceiptsPage' })

const route = useRoute()

// 路由变化时自动关闭抽屉，防止干扰路由过渡
watch(() => route.path, () => {
  samplingDrawerVisible.value = false
})

// ─── Shared State ──────────────────────────────────────
const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const loading = ref(false)

// ─── 来料登记 ────────────────────────────────────────
const receipts = ref<IqcReceipt[]>([])
const receiptDrawerVisible = ref(false)
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

// ─── Sampling Plan (shared component) ─────────────────
const samplingDrawerVisible = ref(false)

// ─── View Mode ──────────────────────────────────────────
const viewMode = ref<'table' | 'card'>('table')
const statusFilter = ref('')
const receiptTableRef = ref<any>(null)

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
  receiptDrawerVisible.value = true
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
  receiptDrawerVisible.value = true
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
    receiptDrawerVisible.value = false
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
})
</script>

<template>
  <div class="iqc-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Box /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">来料登记</h2>
          <span class="page-header-banner-subtitle">管理供应商来料登记与检验流程</span>
        </div>
      </div>
    </div>

    <!-- Content Area -->
    <div class="iqc-content">
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

      <!-- Data Card with Toolbar -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            来料清单
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索单号/批次/供应商/物料..."
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 220px"
              @keyup.enter="loadReceipts"
            />
            <el-select
              v-model="statusFilter"
              clearable
              placeholder="状态筛选"
              size="small"
              style="width: 110px"
              @change="loadReceipts"
            >
              <el-option
                v-for="opt in IQC_RECEIPT_STATUS_OPTIONS"
                :key="opt.value"
                :label="opt.label"
                :value="opt.value"
              />
            </el-select>
            <el-button size="small" @click="loadReceipts">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button size="small" @click="samplingDrawerVisible = true">
              <el-icon><ScaleToOriginal /></el-icon>抽样计算器
            </el-button>
            <el-button type="primary" size="small" @click="openCreateReceipt">
              <el-icon><Plus /></el-icon>新建来料登记
            </el-button>
          </div>
        </div>

        <el-table
          ref="receiptTableRef"
          :data="receipts"
          border
          stripe
          v-loading="loading"
          @row-click="viewReceiptDetail"
          style="width: 100%"
          size="small"
          class="receipts-table"
        >
          <el-table-column type="index" label="序号" width="55" />
          <el-table-column prop="receiptNo" label="收货单号" width="160" />
          <el-table-column prop="supplierName" label="供应商" width="160" show-overflow-tooltip />
          <el-table-column prop="productName" label="物料名称" min-width="160" show-overflow-tooltip />
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
              <el-tag :type="calculateResultColor(row.status)" size="small" effect="plain" round>
                {{ statusLabel(row.status, IQC_RECEIPT_STATUS_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="220" align="center" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="viewReceiptDetail(row)">详情</el-button>
              <el-button size="small" type="primary" link @click.stop="openEditReceipt(row)">编辑</el-button>
              <el-button
                v-if="row.status === 'pending' || row.status === 'inspecting'"
                size="small" type="success" link @click.stop="openNewInspection(row.id)"
              >检验</el-button>
              <el-button size="small" type="danger" link @click.stop="deleteReceipt(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Pagination -->
        <div class="data-card__pagination">
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
      </div>
    </div>
    <!-- ================================================================== -->
    <!-- Drawers -->
    <!-- ================================================================== -->

    <!-- 抽样计算器抽屉 -->
    <SamplingPlanCalculator mode="drawer" v-model="samplingDrawerVisible" />

    <!-- RightPanel: 新建/编辑来料登记 -->
    <RightPanel v-model:visible="receiptDrawerVisible" :title="isEditingReceipt ? '编辑来料登记' : '新建来料登记'" :width="580">
      <template #body>
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Document /></el-icon>
            <span class="dialog-section-title">基本信息</span>
          </div>
          <el-form :model="receiptForm" label-width="90px">
            <el-form-item label="收货单号" required>
              <el-input v-model="receiptForm.receiptNo" placeholder="如: REC-20260620-001" />
            </el-form-item>
            <el-form-item label="供应商" required>
              <el-select v-model="receiptForm.supplierId" filterable placeholder="选择供应商" style="width: 100%">
                <el-option v-for="opt in supplierOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="物料" required>
              <el-select v-model="receiptForm.productId" filterable placeholder="选择物料" style="width: 100%">
                <el-option v-for="opt in productOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><TrendCharts /></el-icon>
            <span class="dialog-section-title">来料信息</span>
          </div>
          <el-form :model="receiptForm" label-width="90px">
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="批次号">
                  <el-input v-model="receiptForm.batchNo" placeholder="如: BATCH-001" />
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="数量" required>
                  <el-input-number v-model="receiptForm.quantity" :min="1" style="width: 100%" controls-position="right" />
                </el-form-item>
              </el-col>
              <el-col :span="6">
                <el-form-item label="单位">
                  <el-input v-model="receiptForm.unit" placeholder="pcs" />
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><WarningFilled /></el-icon>
            <span class="dialog-section-title">检验信息</span>
          </div>
          <el-form :model="receiptForm" label-width="90px">
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
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="receiptDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="saveReceipt">保存</el-button>
        </div>
      </template>
    </RightPanel>

    <!-- RightPanel: 来料详情 + AI Risk -->
    <RightPanel v-model:visible="receiptDetailVisible" :title="`来料详情: ${receiptDetail?.receiptNo}`" :width="640" :show-close="true">
      <template #body>
        <template v-if="receiptDetail">
          <!-- Basic Info -->
          <div class="drawer-section">
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><Document /></el-icon>
              <span>基本信息</span>
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
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><Cpu /></el-icon>
              <span>AI 风险分析</span>
            </div>
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
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><DataAnalysis /></el-icon>
              <span>检验记录</span>
              <el-tag type="info" size="small" effect="plain">{{ receiptDetail.inspections.length }} 条</el-tag>
            </div>
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
            <div class="drawer-section-header">
              <el-icon class="drawer-section-icon"><WarningFilled /></el-icon>
              <span>异常记录</span>
              <el-tag type="danger" size="small" effect="plain">{{ receiptDetail.anomalies.length }} 条</el-tag>
            </div>
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
      </template>
    </RightPanel>

    <!-- RightPanel: 新建检验单 -->
    <RightPanel v-model:visible="newInspectionVisible" title="新建检验单" :width="520">
      <template #body>
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><TrendCharts /></el-icon>
            <span class="dialog-section-title">抽样参数</span>
          </div>
          <el-form :model="newInspectionForm" label-width="100px">
            <el-form-item label="来料登记ID">
              <el-input-number v-model="newInspectionForm.receiptId" :min="1" style="width: 100%" controls-position="right" />
            </el-form-item>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="样本量" required>
                  <el-input-number v-model="newInspectionForm.sampleSize" :min="1" style="width: 100%" controls-position="right" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="Ac" required>
                  <el-input-number v-model="newInspectionForm.ac" :min="0" style="width: 100%" controls-position="right" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="Re" required>
                  <el-input-number v-model="newInspectionForm.re" :min="1" style="width: 100%" controls-position="right" />
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><ScaleToOriginal /></el-icon>
            <span class="dialog-section-title">检验标准</span>
          </div>
          <el-form :model="newInspectionForm" label-width="100px">
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
                  <el-input-number v-model="newInspectionForm.aqlValue" :min="0.01" :step="0.1" :precision="2" style="width: 100%" controls-position="right" />
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="newInspectionVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="createInspection">创建检验单</el-button>
        </div>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.iqc-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* ─── 主体布局 ──────────────────────────────────── */
.iqc-content {
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

.data-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
  flex-shrink: 0;
}

.data-card__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.data-card__actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.data-card__pagination {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
  border-top: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
}

/* ── 来料登记表格 ── */
.receipts-table {
  flex: 1;
  min-height: 0;
  width: 100%;
}

.receipts-table :deep(.el-table__cell) {
  white-space: nowrap;
}

.receipts-table :deep(.el-table__header-wrapper) {
  flex-shrink: 0;
}

.receipts-table :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.receipts-table :deep(.el-table__row) {
  height: 32px;
  line-height: 32px;
}

.receipts-table :deep(.el-table__header-wrapper .el-table__cell) {
  height: 32px;
  line-height: 32px;
  padding: 0 8px;
}

.receipts-table :deep(.el-table__body-wrapper .el-table__cell) {
  padding: 0 8px;
}

.receipts-table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.receipts-table :deep(.el-button--primary.is-link:hover),
.receipts-table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.receipts-table :deep(.el-button--primary.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

.receipts-table :deep(.el-button--danger.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.receipts-table :deep(.el-button--danger.is-link:hover),
.receipts-table :deep(.el-button--danger.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.receipts-table :deep(.el-button--danger.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
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

/* Dialog Sections */
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
  color: var(--el-color-primary);
}
.dialog-section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}

/* Drawer Styles */
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
.drawer-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}
.drawer-section-icon {
  font-size: 15px;
  color: var(--el-color-primary);
}
.desc-highlight {
  font-weight: 500;
  color: var(--el-text-color-primary);
}
.ac-re-cell {
  font-weight: 600;
  font-family: monospace;
}

/* Risk Panel */
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

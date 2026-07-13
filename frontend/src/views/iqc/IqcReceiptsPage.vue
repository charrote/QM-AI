<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  receiptApi, inspectionApi, aiRiskApi, samplingPlanApi
} from '@/api/iqc'
import { Box, Document, WarningFilled, DataAnalysis, ScaleToOriginal, Search } from '@element-plus/icons-vue'
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
const samplingPlanForm = reactive<SamplingPlanRequest>({
  lotSize: 100, samplingLevel: 'II', aqlValue: 1.0
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
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value
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
            @click="openNewInspection(row.id)"
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

    <!-- Sampling Panel -->
    <div class="sampling-panel">
      <el-collapse>
        <el-collapse-item name="sampling">
          <template #title>
            <el-icon style="vertical-align: middle"><ScaleToOriginal /></el-icon>
            <span style="vertical-align: middle">GB/T 2828.1 抽样方案计算器</span>
          </template>
          <el-row :gutter="16" style="margin-bottom: 8px">
            <el-col :span="6">
              <el-form-item label="批量" >
                <el-input-number v-model="samplingPlanForm.lotSize" :min="1" :max="500000" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="检验水平" >
                <el-select v-model="samplingPlanForm.samplingLevel" style="width: 100%">
                  <el-option v-for="opt in SAMPLING_LEVEL_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="AQL 值" >
                <el-input-number v-model="samplingPlanForm.aqlValue" :min="0.01" :step="0.1" :precision="2" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="6" style="display: flex; align-items: flex-start; padding-top: 2px">
              <el-button type="primary" size="small" @click="calculateSamplingPlan">计算</el-button>
            </el-col>
          </el-row>
          <div v-if="samplingPlanResult" class="sampling-result">
            <el-tag>字母代码: {{ samplingPlanResult.sampleCode }}</el-tag>
            <el-tag type="success">样本量: {{ samplingPlanResult.sampleSize }}</el-tag>
            <el-tag type="warning">Ac: {{ samplingPlanResult.ac }}</el-tag>
            <el-tag type="danger">Re: {{ samplingPlanResult.re }}</el-tag>
          </div>
        </el-collapse-item>
      </el-collapse>
    </div>

    <!-- Dialog: 来料登记 -->
    <el-dialog
      v-model="receiptDialogVisible"
      :title="isEditingReceipt ? '编辑来料登记' : '新建来料登记'"
      width="560px"
      :close-on-click-modal="false"
    >
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
      <template #footer>
        <el-button @click="receiptDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveReceipt">保存</el-button>
      </template>
    </el-dialog>

    <!-- Drawer: 来料详情 + AI Risk -->
    <el-drawer
      v-model="receiptDetailVisible"
      :title="`来料详情: ${receiptDetail?.receiptNo || ''}`"
      size="600px"
    >
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
                  {{ row.severity === 'critical' ? '严重' : row.severity === 'major' ? '主要' : '轻微' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="80">
              <template #default="{ row }">
                <el-tag :type="row.status === 'resolved' ? 'success' : 'danger'" size="small">
                  {{ row.status === 'resolved' ? '已解决' : '待处理' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </template>
    </el-drawer>

    <!-- Dialog: 新建检验单 -->
    <el-dialog
      v-model="newInspectionVisible"
      title="新建检验单"
      width="500px"
      :close-on-click-modal="false"
    >
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
      <template #footer>
        <el-button @click="newInspectionVisible = false">取消</el-button>
        <el-button type="primary" @click="createInspection">创建</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
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
.sampling-panel {
  margin-top: 8px;
}
.sampling-result {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.risk-panel { margin-bottom: 16px; }
.risk-panel.high { border-left: 3px solid #f56c6c; }
.risk-panel.warning { border-left: 3px solid #e6a23c; }
.risk-panel.low { border-left: 3px solid #67c23a; }
.risk-loading { padding: 16px; }
.risk-content { padding: 4px 0; }
.risk-score { display: flex; align-items: center; justify-content: space-between; margin-bottom: 12px; }
.score-label { font-weight: 600; font-size: 14px; }
.score-value { font-size: 24px; font-weight: 700; }
.score-value.high { color: #f56c6c; }
.score-value.warning { color: #e6a23c; }
.score-value.low { color: #67c23a; }
.risk-factor { margin-bottom: 10px; }
.factor-name { font-weight: 500; font-size: 13px; display: block; }
.factor-desc { font-size: 12px; color: var(--el-text-color-secondary); display: block; margin-bottom: 4px; }
.risk-recommendations { margin-top: 12px; padding-top: 12px; border-top: 1px solid var(--el-border-color-light); }
.risk-recommendations h4 { margin: 0 0 6px; font-size: 14px; }
.risk-recommendations ul { margin: 0; padding-left: 20px; }
.risk-recommendations li { font-size: 13px; line-height: 1.8; }
.risk-empty { text-align: center; padding: 20px; color: var(--el-text-color-secondary); }
.trace-card { margin-bottom: 0; }
</style>

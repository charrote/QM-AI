<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance } from 'element-plus'
import { anomalyApi } from '@/api/iqc'
import type {
  IqcAnomaly, CreateIqcAnomaly, ResolveIqcAnomaly,
  MrbReview, DispositionDecision, NotifySupplier,
} from '@/types/iqc'
import {
  IQC_ANOMALY_TYPE_OPTIONS, IQC_SEVERITY_OPTIONS, IQC_ANOMALY_STATUS_OPTIONS, IQC_DISPOSITION_OPTIONS,
} from '@/types/iqc'
import {
  WarningFilled, Search, Plus, Refresh, Edit, Setting, Document, Check, Delete,
  Close, Bell, Flag, User, Clock, Link, Box,
} from '@element-plus/icons-vue'

defineOptions({ name: 'IqcAnomaliesPage' })

// ═══════════════════════════════════════════════════════════════
// State
// ═══════════════════════════════════════════════════════════════

const searchKeyword = ref('')
const filterStatus = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const anomalies = ref<IqcAnomaly[]>([])
const selectedAnomaly = ref<IqcAnomaly | null>(null)

// ─── 新建/编辑异常单 ──────────────────────────────
const anomalyDrawerVisible = ref(false)
const isEditingAnomaly = ref(false)
const editingAnomalyId = ref<number>(0)
const anomalyFormRef = ref<FormInstance>()
const anomalyForm = reactive<CreateIqcAnomaly>({
  receiptId: 0, anomalyType: 'quality', severity: 'major', description: ''
})

// ─── 解决异常单 ──────────────────────────────
const resolveDrawerVisible = ref(false)
const resolveForm = reactive<ResolveIqcAnomaly>({ resolution: '' })
const resolveAnomalyId = ref<number>(0)
const resolveAnomalyNo = ref('')

// ─── MRB 评审 ──────────────────────────────
const mrbDrawerVisible = ref(false)
const mrbForm = reactive<MrbReview>({ approved: true, reviewer: '', reviewComments: '' })
const mrbAnomalyId = ref<number>(0)

// ─── 处置决定 ──────────────────────────────
const dispositionDrawerVisible = ref(false)
const dispositionForm = reactive<DispositionDecision>({ disposition: 'none' })
const dispositionAnomalyId = ref<number>(0)

// ─── 通知供应商 ──────────────────────────────
const notifyDrawerVisible = ref(false)
const notifyForm = reactive<NotifySupplier>({ message: '' })
const notifyAnomalyId = ref<number>(0)

// ═══════════════════════════════════════════════════════════════
// Helpers
// ═══════════════════════════════════════════════════════════════

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

const statusType = (status: string, options: any[]) => {
  const opt = options.find((o: any) => o.value === status)
  return opt?.type || 'info'
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

// ═══════════════════════════════════════════════════════════════
// 加载数据
// ═══════════════════════════════════════════════════════════════

async function loadAnomalies() {
  try {
    const res = await anomalyApi.list({
      page: page.value, pageSize: pageSize.value,
      keyword: searchKeyword.value, status: filterStatus.value || undefined
    })
    anomalies.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load anomalies', e)
  }
}

// ═══════════════════════════════════════════════════════════════
// 创建/编辑异常单
// ═══════════════════════════════════════════════════════════════

function openCreateAnomaly(receiptId?: number) {
  isEditingAnomaly.value = false
  editingAnomalyId.value = 0
  anomalyForm.receiptId = receiptId || 0
  anomalyForm.inspectionId = undefined
  anomalyForm.anomalyType = 'quality'
  anomalyForm.severity = 'major'
  anomalyForm.description = ''
  anomalyForm.handler = ''
  anomalyForm.isolatedInventory = 0
  anomalyForm.handlerDept = undefined
  anomalyForm.failedItemIds = undefined
  anomalyForm.defectQty = 0
  anomalyDrawerVisible.value = true
}

function openEditAnomaly(row: IqcAnomaly) {
  isEditingAnomaly.value = true
  editingAnomalyId.value = row.id
  anomalyForm.receiptId = row.receiptId
  anomalyForm.inspectionId = row.inspectionId
  anomalyForm.anomalyType = row.anomalyType
  anomalyForm.severity = row.severity
  anomalyForm.description = row.description || ''
  anomalyForm.handler = row.handler || ''
  anomalyForm.isolatedInventory = row.isolatedInventory || 0
  anomalyForm.handlerDept = row.handlerDept
  anomalyDrawerVisible.value = true
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
    anomalyDrawerVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ═══════════════════════════════════════════════════════════════
// 解决异常单
// ═══════════════════════════════════════════════════════════════

function openResolveAnomaly(row: IqcAnomaly) {
  resolveAnomalyId.value = row.id
  resolveAnomalyNo.value = row.anomalyNo
  resolveForm.resolution = ''
  resolveForm.handler = ''
  resolveDrawerVisible.value = true
}

async function resolveAnomaly() {
  if (!resolveForm.resolution) {
    ElMessage.warning('请填写解决方案')
    return
  }
  try {
    await anomalyApi.resolve(resolveAnomalyId.value, resolveForm)
    ElMessage.success('异常单已解决')
    resolveDrawerVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ═══════════════════════════════════════════════════════════════
// MRB 评审
// ═══════════════════════════════════════════════════════════════

function openMrbReview(row: IqcAnomaly) {
  mrbAnomalyId.value = row.id
  mrbForm.approved = true
  mrbForm.reviewer = ''
  mrbForm.reviewComments = ''
  mrbDrawerVisible.value = true
}

async function submitMrbReview() {
  if (!mrbForm.reviewer) {
    ElMessage.warning('请填写评审人')
    return
  }
  try {
    await anomalyApi.mrbReview(mrbAnomalyId.value, mrbForm)
    ElMessage.success(mrbForm.approved ? 'MRB评审已通过' : 'MRB评审已驳回')
    mrbDrawerVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ═══════════════════════════════════════════════════════════════
// 处置决定
// ═══════════════════════════════════════════════════════════════

function openDisposition(row: IqcAnomaly) {
  dispositionAnomalyId.value = row.id
  dispositionForm.disposition = row.disposition || 'none'
  dispositionForm.dispositionBy = ''
  dispositionDrawerVisible.value = true
}

async function submitDisposition() {
  if (dispositionForm.disposition === 'none') {
    ElMessage.warning('请选择处置方式')
    return
  }
  if (!dispositionForm.dispositionBy) {
    ElMessage.warning('请填写处置决定人')
    return
  }
  try {
    await anomalyApi.disposition(dispositionAnomalyId.value, dispositionForm)
    ElMessage.success('处置决定已提交')
    dispositionDrawerVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ═══════════════════════════════════════════════════════════════
// 通知供应商
// ═══════════════════════════════════════════════════════════════

function openNotifySupplier(row: IqcAnomaly) {
  notifyAnomalyId.value = row.id
  notifyForm.message = ''
  notifyDrawerVisible.value = true
}

async function submitNotifySupplier() {
  try {
    await anomalyApi.notifySupplier(notifyAnomalyId.value, notifyForm)
    ElMessage.success('供应商通知已发送')
    notifyDrawerVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ═══════════════════════════════════════════════════════════════
// 关闭异常单
// ═══════════════════════════════════════════════════════════════

async function closeAnomaly(row: IqcAnomaly) {
  try {
    await ElMessageBox.confirm(`确定关闭异常单「${row.anomalyNo}」吗？`, '确认关闭', {
      type: 'warning',
      confirmButtonText: '确定',
      cancelButtonText: '取消',
    })
    await anomalyApi.close(row.id)
    ElMessage.success('异常单已关闭')
    await loadAnomalies()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error('操作失败')
  }
}

onMounted(async () => {
  await loadAnomalies()
})
</script>

<template>
  <div class="iqc-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--danger">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><WarningFilled /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">来料异常管理</h2>
          <span class="page-header-banner-subtitle">来料异常全流程管理：创建 → 隔离 → MRB评审 → 处置决定 → 解决 → 关闭</span>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="content-area">
      <!-- Data Card -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            <el-icon style="color: var(--el-color-danger)"><WarningFilled /></el-icon>
            异常记录
            <el-tag v-if="total > 0" type="danger" size="small" effect="dark" round>{{ total }} 条</el-tag>
          </span>
          <div class="data-card__toolbar">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索异常单号、来料单号..."
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 220px"
              @keyup.enter="loadAnomalies"
              @clear="loadAnomalies"
            />
            <el-select
              v-model="filterStatus"
              placeholder="状态筛选"
              clearable
              size="small"
              style="width: 120px"
              @change="loadAnomalies"
            >
              <el-option
                v-for="opt in IQC_ANOMALY_STATUS_OPTIONS"
                :key="opt.value"
                :label="opt.label"
                :value="opt.value"
              />
            </el-select>
            <el-button size="small" @click="loadAnomalies">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button type="primary" size="small" @click="openCreateAnomaly()">
              <el-icon><Plus /></el-icon>新建异常单
            </el-button>
          </div>
        </div>
        <el-table
          :data="anomalies"
          border
          stripe
          size="small"
          style="width: 100%"
          class="data-card__table"
          :header-cell-class-name="'receipt-header-cell'"
        >
          <el-table-column prop="anomalyNo" label="异常单号" width="170" />
          <el-table-column prop="receiptNo" label="来料单号" width="150" />
          <el-table-column label="类型" width="90">
            <template #default="{ row }">{{ statusLabel(row.anomalyType, IQC_ANOMALY_TYPE_OPTIONS) }}</template>
          </el-table-column>
          <el-table-column label="严重程度" width="80">
            <template #default="{ row }">
              <el-tag :type="row.severity === 'critical' ? 'danger' : row.severity === 'major' ? 'warning' : 'info'" size="small" effect="plain" round>
                {{ statusLabel(row.severity, IQC_SEVERITY_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="不合格数" width="80" align="right">
            <template #default="{ row }">
              <span :class="row.defectQty && row.defectQty > 0 ? 'text-danger' : ''">
                {{ row.defectQty || '-' }}
              </span>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="描述" min-width="220" show-overflow-tooltip />
          <el-table-column label="隔离库存" width="90" align="right">
            <template #default="{ row }">{{ row.isolatedInventory ? row.isolatedInventory + ' pcs' : '-' }}</template>
          </el-table-column>
          <el-table-column label="处置" width="90">
            <template #default="{ row }">
              <el-tag v-if="row.disposition && row.disposition !== 'none'" size="small" effect="plain" round>
                {{ statusLabel(row.disposition, IQC_DISPOSITION_OPTIONS) }}
              </el-tag>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="MRB" width="70">
            <template #default="{ row }">
              <el-tag v-if="row.mrbReviewed" type="success" size="small" effect="plain" round>已审</el-tag>
              <el-tag v-else-if="row.status === 'mrb_reviewing'" type="warning" size="small" effect="plain" round>评审中</el-tag>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="100">
            <template #default="{ row }">
              <el-tag :type="statusType(row.status, IQC_ANOMALY_STATUS_OPTIONS)" size="small" effect="plain" round>
                {{ statusLabel(row.status, IQC_ANOMALY_STATUS_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="240" fixed="right" align="center">
            <template #default="{ row }">
              <el-button
                v-if="row.status === 'open' || row.status === 'quarantined'"
                size="small" type="warning" link @click.stop="openMrbReview(row)"
              >MRB</el-button>
              <el-button
                v-if="row.status === 'mrb_approved' || row.status === 'mrb_reviewing'"
                size="small" type="primary" link @click.stop="openDisposition(row)"
              >处置</el-button>
              <el-button
                v-if="row.status !== 'closed' && row.status !== 'resolved'"
                size="small" type="success" link @click.stop="openResolveAnomaly(row)"
              >解决</el-button>
              <el-button
                v-if="!row.supplierNotified && row.status !== 'closed' && row.status !== 'resolved'"
                size="small" type="info" link @click.stop="openNotifySupplier(row)"
              >通知</el-button>
              <el-button
                v-if="row.status !== 'closed' && row.status !== 'resolved'"
                size="small" type="primary" link @click.stop="openEditAnomaly(row)"
              >编辑</el-button>
              <el-button
                v-if="row.status === 'closed' || row.status === 'resolved'"
                size="small" type="info" link @click.stop="closeAnomaly(row)"
              >关闭</el-button>
            </template>
          </el-table-column>
        </el-table>

        <div class="data-card__footer">
          <div class="data-card__pagination">
            <el-pagination
              v-model:current-page="page"
              v-model:page-size="pageSize"
              :total="total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              @size-change="loadAnomalies"
              @current-change="loadAnomalies"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!-- Drawer: 新建/编辑异常单                                  -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <el-drawer
      v-model="anomalyDrawerVisible"
      :title="isEditingAnomaly ? '编辑异常单' : '新建异常单'"
      size="580px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Document /></el-icon>
          <span class="dialog-section-title">来料信息</span>
        </div>
        <el-form :model="anomalyForm" label-width="110px">
          <el-form-item label="来料登记ID" required>
            <el-input-number v-model="anomalyForm.receiptId" :min="1" style="width: 100%" controls-position="right" />
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><WarningFilled /></el-icon>
          <span class="dialog-section-title">异常信息</span>
        </div>
        <el-form :model="anomalyForm" label-width="110px">
          <el-form-item label="异常类型" required>
            <el-select v-model="anomalyForm.anomalyType" style="width: 100%">
              <el-option v-for="opt in IQC_ANOMALY_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="严重程度" required>
            <el-select v-model="anomalyForm.severity" style="width: 100%">
              <el-option v-for="opt in IQC_SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="描述" required>
            <el-input v-model="anomalyForm.description" type="textarea" :rows="3" placeholder="请描述异常详情，包括不合格项目、实测值、规格范围等..." />
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Box /></el-icon>
          <span class="dialog-section-title">隔离管理</span>
        </div>
        <el-form :model="anomalyForm" label-width="110px">
          <el-form-item label="隔离库存量">
            <el-input-number v-model="anomalyForm.isolatedInventory" :min="0" :step="1" style="width: 100%" controls-position="right" />
          </el-form-item>
          <el-form-item label="不合格数量">
            <el-input-number v-model="anomalyForm.defectQty" :min="0" :step="1" style="width: 100%" controls-position="right" />
          </el-form-item>
          <el-form-item label="处理部门">
            <el-select v-model="anomalyForm.handlerDept" placeholder="选择处理部门" style="width: 100%" clearable>
              <el-option value="quality" label="质量部" />
              <el-option value="purchasing" label="采购部" />
              <el-option value="engineering" label="工程部" />
              <el-option value="production" label="生产部" />
              <el-option value="other" label="其他" />
            </el-select>
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><User /></el-icon>
          <span class="dialog-section-title">处理人</span>
        </div>
        <el-form :model="anomalyForm" label-width="110px">
          <el-form-item label="处理人">
            <el-input v-model="anomalyForm.handler" placeholder="请输入处理人姓名" />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="anomalyDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="saveAnomaly">
            {{ isEditingAnomaly ? '保存修改' : '创建' }}
          </el-button>
        </div>
      </template>
    </el-drawer>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!-- Drawer: 解决异常单                                       -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <el-drawer
      v-model="resolveDrawerVisible"
      title="解决异常单"
      size="480px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <div style="margin-bottom: 4px; font-size: 13px; color: var(--el-text-color-secondary);">
        异常单号: <strong>{{ resolveAnomalyNo }}</strong>
      </div>
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Check /></el-icon>
          <span class="dialog-section-title">解决方案</span>
        </div>
        <el-form label-width="110px">
          <el-form-item label="解决方案" required>
            <el-input v-model="resolveForm.resolution" type="textarea" :rows="4" placeholder="请描述解决方案，包括根因分析、纠正措施等..." />
          </el-form-item>
          <el-form-item label="处理人">
            <el-input v-model="resolveForm.handler" placeholder="请输入处理人姓名" />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="resolveDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="resolveAnomaly">确认解决</el-button>
        </div>
      </template>
    </el-drawer>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!-- Drawer: MRB 评审                                       -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <el-drawer
      v-model="mrbDrawerVisible"
      title="MRB 评审（多部门会签）"
      size="480px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <div style="margin-bottom: 4px; font-size: 13px; color: var(--el-text-color-secondary);">
        评审不合格品，决定是否让步接收或退货
      </div>
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Flag /></el-icon>
          <span class="dialog-section-title">评审结果</span>
        </div>
        <el-form label-width="110px">
          <el-form-item label="评审结论" required>
            <el-radio-group v-model="mrbForm.approved">
              <el-radio :value="true">通过（让步接收/特采）</el-radio>
              <el-radio :value="false">驳回（退货/返工）</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="评审人" required>
            <el-input v-model="mrbForm.reviewer" placeholder="多部门会签，用逗号分隔（如：张明/李强/王华）" />
          </el-form-item>
          <el-form-item label="评审意见">
            <el-input v-model="mrbForm.reviewComments" type="textarea" :rows="3" placeholder="请描述评审意见和依据..." />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="mrbDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="submitMrbReview">提交评审</el-button>
        </div>
      </template>
    </el-drawer>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!-- Drawer: 处置决定                                       -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <el-drawer
      v-model="dispositionDrawerVisible"
      title="处置决定"
      size="480px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <div style="margin-bottom: 4px; font-size: 13px; color: var(--el-text-color-secondary);">
        MRB评审通过后，做出处置决定
      </div>
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Setting /></el-icon>
          <span class="dialog-section-title">处置方案</span>
        </div>
        <el-form label-width="110px">
          <el-form-item label="处置方式" required>
            <el-select v-model="dispositionForm.disposition" style="width: 100%">
              <el-option v-for="opt in IQC_DISPOSITION_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="处置决定人" required>
            <el-input v-model="dispositionForm.dispositionBy" placeholder="请输入处置决定人姓名" />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="dispositionDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="submitDisposition">确认处置</el-button>
        </div>
      </template>
    </el-drawer>

    <!-- ═══════════════════════════════════════════════════════ -->
    <!-- Drawer: 通知供应商                                     -->
    <!-- ═══════════════════════════════════════════════════════ -->
    <el-drawer
      v-model="notifyDrawerVisible"
      title="通知供应商"
      size="480px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <div style="margin-bottom: 4px; font-size: 13px; color: var(--el-text-color-secondary);">
        发送异常通知给供应商，要求回复和处理
      </div>
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Bell /></el-icon>
          <span class="dialog-section-title">通知内容</span>
        </div>
        <el-form label-width="110px">
          <el-form-item label="通知内容">
            <el-input v-model="notifyForm.message" type="textarea" :rows="4" placeholder="请描述通知内容，包括异常详情、期望回复时间等..." />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="notifyDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="submitNotifySupplier">发送通知</el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<style scoped>
.iqc-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* Content Area */
.content-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  overflow-y: auto;
}

/* Text Helpers */
.text-danger {
  color: var(--el-color-danger);
  font-weight: 600;
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
  color: var(--el-color-danger);
}
.dialog-section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}
</style>

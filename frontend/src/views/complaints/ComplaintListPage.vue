<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, Plus, Edit, Delete, Document, TrendCharts, Check, Close, ChatLineSquare, WarningFilled, Clock } from '@element-plus/icons-vue'
import { complaintApi } from '@/api/complaint'
import { customerApi } from '@/api/basicData'
import type { Complaint, CreateComplaint, UpdateComplaint } from '@/types/complaint'
import {
  COMPLAINT_SEVERITY_OPTIONS as SEVERITY_OPTIONS, COMPLAINT_SEVERITY_MAP as SEVERITY_MAP,
  COMPLAINT_STATUS_OPTIONS as STATUS_OPTIONS, COMPLAINT_STATUS_MAP as STATUS_MAP,
} from '@/types/complaint'
import DataTable, { type Column } from '@/components/common/DataTable.vue'
import StatusTag from '@/components/common/StatusTag.vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'
import { usePagination } from '@/composables/usePagination'

defineOptions({ name: 'ComplaintListPage' })

const router = useRouter()

// ─── Pagination ──────────────────────────────────────────
const pagination = usePagination(1, 20)

// ─── Search state ────────────────────────────────────────
const searchKeyword = ref('')
const severityFilter = ref('')
const statusFilter = ref('')
const customerIdFilter = ref<number | undefined>(undefined)

const complaints = ref<Complaint[]>([])

// Customer options
const customerOptions = ref<Array<{ value: number; label: string }>>([])

// Stats (stub — API not yet available)
const stats = reactive({
  totalCount: 0,
  bySeverity: {} as Record<string, number>,
  byStatus: {} as Record<string, number>,
  avgDaysToClose: 0,
})

// Dialog state
const formPanel = useRightPanel()
const isEditing = ref(false)
const currentId = ref<number | null>(null)
const submitLoading = ref(false)

const form = reactive<Omit<CreateComplaint, 'customerId'> & { customerId: number | undefined }>({
complaintCode: '', customerId: undefined, severity: 'minor',
    subject: '', description: '', assignedTo: undefined, dueDate: '',
  })

// ─── Table columns ───────────────────────────────────────
const tableColumns = computed<Column[]>(() => [
  { prop: 'complaintCode', label: '投诉代码', width: 140 },
  { prop: 'customerName', label: '客户', width: 150, showOverflowTooltip: true },
  { label: '严重程度', slotName: 'severity', width: 110, align: 'center' },
  { prop: 'subject', label: '主题', minWidth: 200, showOverflowTooltip: true },
  { label: '状态', slotName: 'status', width: 100, align: 'center' },
  { label: '负责人', slotName: 'assignedTo', width: 100 },
  { label: '截止日期', slotName: 'dueDate', width: 120 },
  { label: '创建时间', slotName: 'createdAt', width: 180 },
  { label: '操作', slotName: 'actions', width: 320, fixed: 'right' as const },
])

// ─── Data loading ────────────────────────────────────────
async function loadComplaints() {
  pagination.loading.value = true
  try {
    const res = await complaintApi.list({
      page: pagination.currentPage.value,
      pageSize: pagination.pageSize.value,
      keyword: searchKeyword.value || undefined,
      severity: severityFilter.value || undefined,
      status: statusFilter.value || undefined,
      customerId: customerIdFilter.value,
    })
    complaints.value = res.items
    pagination.total.value = res.total
  } catch (e) {
    console.error('Failed to load complaints', e)
  } finally {
    pagination.loading.value = false
  }
}

async function loadStats() {
  try {
    const res = await complaintApi.stats()
    Object.assign(stats, res)
  } catch (e) {
    console.error('Failed to load stats', e)
  }
}

async function loadCustomers() {
  try {
    const res = await customerApi.list({ page: 1, pageSize: 500 })
    customerOptions.value = res.items.map((c: any) => ({
      value: c.id,
      label: `${c.code || ''} - ${c.name || ''}`.trim(),
    }))
  } catch (e) {
    console.error('Failed to load customers', e)
  }
}

function handlePageChange(page: number) {
  pagination.goToPage(page)
  loadComplaints()
}

function handleSizeChange(size: number) {
  pagination.pageSize.value = size
  pagination.goToPage(1)
  loadComplaints()
}

// ─── Search helpers ──────────────────────────────────────
function applySearch() {
  pagination.goToPage(1)
  loadComplaints()
}

// ─── CRUD operations ─────────────────────────────────────
function openCreate() {
 isEditing.value = false
  currentId.value = null
  Object.assign(form, {
    complaintCode: '', customerId: undefined, severity: 'minor',
    subject: '', description: '', assignedTo: undefined, dueDate: '',
  })
  formPanel.open()
}

function openEdit(row: Complaint) {
  isEditing.value = true
  currentId.value = row.id
  Object.assign(form, {
    complaintCode: row.complaintCode,
    customerId: row.customerId,
    severity: row.severity,
    subject: row.subject,
    description: row.description,
    assignedTo: row.assignedTo?.toString() || '',
    dueDate: row.dueDate?.slice(0, 10) || '',
  })
  formPanel.open()
}

async function saveComplaint(formData: Partial<CreateComplaint>) {
  if (!formData.complaintCode || !formData.customerId || !formData.subject) {
    ElMessage.warning('请填写完整信息')
    return
  }
  submitLoading.value = true
  try {
    if (isEditing.value && currentId.value) {
      await complaintApi.update(currentId.value, formData as UpdateComplaint)
      ElMessage.success('客诉已更新')
    } else {
      await complaintApi.create(formData as CreateComplaint)
      ElMessage.success('客诉已创建')
    }
          formPanel.close()
    await Promise.all([loadComplaints(), loadStats()])
  
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

async function deleteComplaint(row: Complaint) {
  try {
    await ElMessageBox.confirm(`确定删除客诉「${row.complaintCode}」吗？`, '确认', { type: 'warning' })
    await complaintApi.remove(row.id)
    ElMessage.success('已删除')
    await Promise.all([loadComplaints(), loadStats()])
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

// ─── Navigation ──────────────────────────────────────────
function viewDetail(row: Complaint) {
  router.push({ name: 'D8Report', query: { complaintId: String(row.id) } })
}

function goToCapa(row: Complaint) {
  router.push({ name: 'Capa', query: { complaintId: String(row.id) } })
}

async function transitionStatus(row: Complaint, newStatus: string) {
  try {
    await ElMessageBox.confirm(`确认将状态变更为「${STATUS_MAP[newStatus] || newStatus}」吗？`, '状态变更', { type: 'info' })
    await complaintApi.transitionStatus(row.id, { newStatus })
    ElMessage.success('状态已更新')
    await loadComplaints()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '状态更新失败')
  }
}

onMounted(async () => {
  await Promise.all([loadComplaints(), loadStats(), loadCustomers()])
})

// ─── Stat card helpers ───────────────────────────────────
function severityClass(v: string): string {
  const map: Record<string, string> = { critical: 'danger', major: 'warning', minor: 'success' }
  return map[v] || 'info'
}
function severityIconClass(v: string): string {
  const map: Record<string, string> = { critical: 'stat-grid__icon--danger', major: 'stat-grid__icon--warning', minor: 'stat-grid__icon--success' }
  return map[v] || 'stat-grid__icon--info'
}
function statusClass(v: string): string {
  const map: Record<string, string> = { new: 'info', acknowledged: 'primary', in_progress: 'warning', closed: 'success' }
  return map[v] || 'info'
}
function statusIconClass(v: string): string {
  const map: Record<string, string> = { new: 'stat-grid__icon--info', acknowledged: 'stat-grid__icon--primary', in_progress: 'stat-grid__icon--warning', closed: 'stat-grid__icon--success' }
  return map[v] || 'stat-grid__icon--info'
}
</script>

<template>
  <div class="iqc-container">
    <!-- Header Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><ChatLineSquare /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">客户投诉管理</h2>
          <span class="page-header-banner-subtitle">管理客户投诉记录，追踪处理进度，推进8D分析与CAPA</span>
        </div>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="stat-grid">
      <div class="stat-grid__item stat-grid__item--primary">
        <div class="stat-grid__icon stat-grid__icon--primary"><el-icon><ChatLineSquare /></el-icon></div>
        <div class="stat-grid__text">
          <div class="stat-grid__label">投诉总数</div>
          <div class="stat-grid__value">{{ stats.totalCount }}</div>
        </div>
      </div>
      <div v-for="item in SEVERITY_OPTIONS" :key="item.value" class="stat-grid__item"
           :class="'stat-grid__item--' + severityClass(item.value)">
        <div :class="['stat-grid__icon', severityIconClass(item.value)]">
          <el-icon><WarningFilled /></el-icon>
        </div>
        <div class="stat-grid__text">
          <div class="stat-grid__label">{{ item.label }}</div>
          <div class="stat-grid__value">{{ stats.bySeverity[item.value] || 0 }}</div>
        </div>
      </div>
      <div v-for="item in STATUS_OPTIONS" :key="item.value" class="stat-grid__item"
           :class="'stat-grid__item--' + statusClass(item.value)">
        <div :class="['stat-grid__icon', statusIconClass(item.value)]">
          <el-icon><Document /></el-icon>
        </div>
        <div class="stat-grid__text">
          <div class="stat-grid__label">{{ item.label }}</div>
          <div class="stat-grid__value">{{ stats.byStatus[item.value] || 0 }}</div>
        </div>
      </div>
      <div class="stat-grid__item stat-grid__item--info">
        <div class="stat-grid__icon stat-grid__icon--info"><el-icon><Clock /></el-icon></div>
        <div class="stat-grid__text">
          <div class="stat-grid__label">平均处理天数</div>
          <div class="stat-grid__value">{{ stats.avgDaysToClose.toFixed(1) }}</div>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="data-card">
      <div class="data-card__header">
        <span class="data-card__title">
          <el-icon style="color: var(--primary)"><ChatLineSquare /></el-icon>
          客诉清单
          <el-tag v-if="pagination.total.value" type="info" size="small">{{ pagination.total.value }} 条</el-tag>
        </span>
        <div class="data-card__toolbar">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索代码/主题/客户..."
            clearable
            size="small"
            :prefix-icon="Search"
            style="width: 220px"
            @keyup.enter="applySearch"
          />
          <el-select v-model="customerIdFilter" placeholder="客户" clearable size="small" filterable style="width: 160px" @change="applySearch">
            <el-option v-for="opt in customerOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-select v-model="severityFilter" placeholder="严重程度" clearable size="small" style="width: 120px" @change="applySearch">
            <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-select v-model="statusFilter" placeholder="状态" clearable size="small" style="width: 100px" @change="applySearch">
            <el-option v-for="opt in STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-button size="small" @click="loadComplaints"><el-icon><Refresh /></el-icon>刷新</el-button>
          <el-button type="primary" size="small" @click="openCreate"><el-icon><Plus /></el-icon>新建客诉</el-button>
        </div>
      </div>
      <div class="data-card__table">
        <DataTable
          :data="complaints"
          :columns="tableColumns"
          :loading="pagination.loading.value"
          :total="pagination.total.value"
          :current-page="pagination.currentPage.value"
          :page-size="pagination.pageSize.value"
          :page-sizes="[10, 20, 50, 100]"
          @update:current-page="handlePageChange"
          @update:page-size="handleSizeChange"
        >
          <template #customerName="{ row }">
            {{ (row.customer && row.customer.name) || row.customerName || '-' }}
          </template>
          <template #severity="{ row }">
            <StatusTag :status="row.severity" :text="SEVERITY_MAP[row.severity] || row.severity" />
          </template>
          <template #status="{ row }">
            <StatusTag :status="row.status" :text="STATUS_MAP[row.status] || row.status" />
          </template>
          <template #assignedTo="{ row }">
            {{ row.assignedTo || '-' }}
          </template>
          <template #dueDate="{ row }">
            {{ row.dueDate?.slice(0, 10) || '-' }}
          </template>
          <template #createdAt="{ row }">
            {{ row.createdAt ? new Date(row.createdAt).toLocaleString('zh-CN') : '-' }}
          </template>
          <template #actions="{ row }">
            <el-button link size="small" type="primary" :icon="Edit" @click.stop="openEdit(row)">编辑</el-button>
            <el-button link size="small" type="primary" :icon="Document" @click.stop="viewDetail(row)">详情</el-button>
            <el-button link size="small" type="warning" :icon="TrendCharts" @click.stop="goToCapa(row)">CAPA</el-button>
            <el-button
              v-if="row.status === 'new'"
              link size="small" type="success" :icon="Check"
              @click.stop="transitionStatus(row, 'acknowledged')"
            >确认接收</el-button>
            <el-button
              v-if="row.status === 'in_progress'"
              link size="small" type="success" :icon="Close"
              @click.stop="transitionStatus(row, 'closed')"
            >关闭</el-button>
            <el-button link size="small" type="danger" :icon="Delete" @click.stop="deleteComplaint(row)">删除</el-button>
          </template>
        </DataTable>
      </div>
      <div class="data-card__footer">
        <div class="data-card__pagination">
          <el-pagination
            v-model:current-page="pagination.currentPage.value"
            v-model:page-size="pagination.pageSize.value"
            :total="pagination.total.value"
            :page-sizes="[10, 20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="handleSizeChange"
            @current-change="handlePageChange"
          />
        </div>
      </div>
    </div>

    <!-- Create/Edit Panel -->
    <RightPanel v-model:visible="formPanel.visible" :title="isEditing ? '编辑客诉' : '新建客诉'" :width="680">
      <template #body>
        <el-form :model="form" label-width="90px" size="default">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="投诉代码" required>
                <el-input v-model="form.complaintCode" placeholder="如: CMP-2026-001" :disabled="isEditing" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="客户" required>
                <el-select v-model="form.customerId" filterable placeholder="选择客户" style="width: 100%">
                  <el-option v-for="opt in customerOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="严重程度" required>
                <el-select v-model="form.severity" style="width: 100%">
                  <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="负责人">
                <el-input v-model="form.assignedTo" placeholder="负责人姓名" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="主题" required>
            <el-input v-model="form.subject" placeholder="客诉主题" />
          </el-form-item>
          <el-form-item label="描述">
            <el-input v-model="form.description" type="textarea" :rows="3" placeholder="客诉描述" />
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="截止日期">
                <el-date-picker v-model="form.dueDate" type="date" value-format="YYYY-MM-DD" style="width: 100%" placeholder="选择日期" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="formPanel.close()">取消</el-button>
        <el-button type="primary" :loading="submitLoading" @click="saveComplaint(form as CreateComplaint)">保存</el-button>
      </template>
    </RightPanel>
  </div>
</template>
<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, Plus, Edit, Delete, Document, TrendCharts, Check, Close } from '@element-plus/icons-vue'
import { complaintApi } from '@/api/complaint'
import { customerApi } from '@/api/basicData'
import type { Complaint, CreateComplaint, UpdateComplaint } from '@/types/complaint'
import {
  COMPLAINT_SEVERITY_OPTIONS as SEVERITY_OPTIONS, COMPLAINT_SEVERITY_MAP as SEVERITY_MAP,
  COMPLAINT_STATUS_OPTIONS as STATUS_OPTIONS, COMPLAINT_STATUS_MAP as STATUS_MAP,
} from '@/types/complaint'
import DataTable, { type Column } from '@/components/common/DataTable.vue'
import StatusTag from '@/components/common/StatusTag.vue'
import FormDialog from '@/components/common/FormDialog.vue'
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
const dialogVisible = ref(false)
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
  { label: '严重程度', slotName: 'severity', width: 90, align: 'center' },
  { prop: 'subject', label: '主题', minWidth: 180, showOverflowTooltip: true },
  { label: '状态', slotName: 'status', width: 90, align: 'center' },
  { label: '负责人', slotName: 'assignedTo', width: 90 },
  { label: '截止日期', slotName: 'dueDate', width: 110 },
  { label: '创建时间', slotName: 'createdAt', width: 160 },
  { label: '操作', slotName: 'actions', width: 300, fixed: 'right' as const },
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
    Object.assign(stats, { totalCount: 0, bySeverity: {}, byStatus: {}, avgDaysToClose: 0 })
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
  dialogVisible.value = true
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
  dialogVisible.value = true
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
    dialogVisible.value = false
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
</script>

<template>
  <div class="page-container">
    <!-- Stats Cards -->
    <el-row :gutter="16" class="stats-row">
      <el-col :xs="12" :sm="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.totalCount }}</div>
          <div class="stat-label">投诉总数</div>
        </el-card>
      </el-col>
      <el-col v-for="item in SEVERITY_OPTIONS" :key="item.value" :xs="12" :sm="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.bySeverity[item.value] || 0 }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
      <el-col v-for="item in STATUS_OPTIONS" :key="item.value" :xs="12" :sm="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.byStatus[item.value] || 0 }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
      <el-col :xs="12" :sm="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.avgDaysToClose.toFixed(1) }}</div>
          <div class="stat-label">平均处理天数</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Search Toolbar -->
    <div class="search-bar">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索代码/主题/客户..."
        :prefix-icon="Search"
        clearable
        style="width: 260px"
        @keyup.enter="applySearch"
      />
      <el-select v-model="customerIdFilter" placeholder="客户" clearable filterable style="width: 180px" @change="applySearch">
        <el-option v-for="opt in customerOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="severityFilter" placeholder="严重程度" clearable style="width: 130px" @change="applySearch">
        <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 130px" @change="applySearch">
        <el-option v-for="opt in STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button :icon="Refresh" @click="loadComplaints">刷新</el-button>
      <div class="search-spacer" />
      <el-button type="primary" :icon="Plus" @click="openCreate">新建客诉</el-button>
    </div>

    <!-- Data Table -->
    <DataTable
      :data="complaints"
      :columns="tableColumns"
      :loading="pagination.loading.value"
      :total="pagination.total.value"
      :current-page="pagination.currentPage.value"
      :page-size="pagination.pageSize.value"
      @update:current-page="handlePageChange"
      @update:page-size="handleSizeChange"
    >
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

    <!-- Create/Edit Dialog -->
    <FormDialog
      :visible="dialogVisible"
      :title="isEditing ? '编辑客诉' : '新建客诉'"
      :loading="submitLoading"
      width="680px"
      @update:visible="dialogVisible = $event"
      @submit="saveComplaint(form as CreateComplaint)"
    >
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
    </FormDialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }

.search-bar {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.search-spacer { flex: 1; }

.stats-row { margin-bottom: 12px; }

.stat-card { text-align: center; }

.stat-value {
  font-size: 28px;
  font-weight: 700;
  color: var(--el-text-color-primary);
}

.stat-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  margin-top: 4px;
}
</style>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { complaintApi } from '@/api/complaint'
import { customerApi } from '@/api/basicData'
import type { PagedResult } from '@/types/basicData'
import type { Complaint } from '@/types/complaint'
import {
  COMPLAINT_SEVERITY_OPTIONS as SEVERITY_OPTIONS, COMPLAINT_SEVERITY_MAP as SEVERITY_MAP, COMPLAINT_STATUS_OPTIONS as STATUS_OPTIONS, COMPLAINT_STATUS_MAP as STATUS_MAP,
  EVENT_TYPE_MAP,
} from '@/types/complaint'

defineOptions({ name: 'ComplaintListPage' })

const router = useRouter()

const searchKeyword = ref('')
const severityFilter = ref('')
const statusFilter = ref('')
const customerIdFilter = ref<number | undefined>(undefined)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const complaints = ref<Complaint[]>([])

// Stats
const stats = reactive({
  totalCount: 0,
  bySeverity: {} as Record<string, number>,
  byStatus: {} as Record<string, number>,
  avgDaysToClose: 0,
})

// Customer options
const customerOptions = ref<Array<{ value: number; label: string }>>([])

// Dialog
const dialogVisible = ref(false)
const isEditing = ref(false)
const currentId = ref<number | null>(null)
const form = reactive({
  complaintCode: '',
  customerId: 0,
  severity: 'minor',
  subject: '',
  description: '',
  assignedTo: '',
  dueDate: '',
})

const statusLabel = (status: string) => STATUS_MAP[status] || status
const severityLabel = (sev: string) => SEVERITY_MAP[sev] || sev

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadComplaints() {
  try {
    const res = await complaintApi.list({
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
      severity: severityFilter.value || undefined,
      status: statusFilter.value || undefined,
      customerId: customerIdFilter.value,
    })
    complaints.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load complaints', e)
  }
}

async function loadStats() {
  try {
    const res = await Promise.resolve({ totalCount: 0, bySeverity: {}, byStatus: {}, avgDaysToClose: 0 })
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

function openCreate() {
  isEditing.value = false
  currentId.value = null
  form.complaintCode = ''
  form.customerId = 0
  form.severity = 'minor'
  form.subject = ''
  form.description = ''
  form.assignedTo = ''
  form.dueDate = ''
  dialogVisible.value = true
}

function openEdit(row: Complaint) {
  isEditing.value = true
  currentId.value = row.id
  form.complaintCode = row.complaintCode
  form.customerId = row.customerId
  form.severity = row.severity
  form.subject = row.subject
  form.description = row.description
  form.assignedTo = row.assignedTo?.toString() || ''
  form.dueDate = row.dueDate?.slice(0, 10) || ''
  dialogVisible.value = true
}

async function saveComplaint() {
  if (!form.complaintCode || !form.customerId || !form.subject) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    if (isEditing.value && currentId.value) {
      await complaintApi.update(currentId.value, {
        severity: form.severity,
        subject: form.subject,
        description: form.description,
        assignedTo: form.assignedTo ? Number(form.assignedTo) : null,
        dueDate: form.dueDate || null,
      })
      ElMessage.success('客诉已更新')
    } else {
      await complaintApi.create({
        complaintCode: form.complaintCode,
        customerId: form.customerId,
        severity: form.severity,
        subject: form.subject,
        description: form.description,
        assignedTo: form.assignedTo ? Number(form.assignedTo) : undefined,
        dueDate: form.dueDate || undefined,
      })
      ElMessage.success('客诉已创建')
    }
    dialogVisible.value = false
    await Promise.all([loadComplaints(), loadStats()])
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
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

function viewDetail(row: Complaint) {
  router.push({ name: 'D8Report', query: { complaintId: String(row.id) } })
}

function goToCapa(row: Complaint) {
  router.push({ name: 'Capa', query: { complaintId: String(row.id) } })
}

async function transitionStatus(row: Complaint, newStatus: string) {
  try {
    await ElMessageBox.confirm(`确认将状态变更为「${statusLabel(newStatus)}」吗？`, '状态变更', { type: 'info' })
    await complaintApi.transitionStatus(row.id, { newStatus })
    ElMessage.success('状态已更新')
    await loadComplaints()
  } catch (e: any) {
    if (e !== 'cancel') {
      ElMessage.error(e?.response?.data?.message || '状态更新失败')
    }
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
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.totalCount }}</div>
          <div class="stat-label">投诉总数</div>
        </el-card>
      </el-col>
      <el-col :span="6" v-for="item in SEVERITY_OPTIONS" :key="item.value">
        <el-card shadow="hover" class="stat-card" :class="item.value">
          <div class="stat-value">{{ stats.bySeverity[item.value] || 0 }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
      <el-col :span="6" v-for="item in STATUS_OPTIONS" :key="item.value">
        <el-card shadow="hover" class="stat-card" :class="item.value">
          <div class="stat-value">{{ stats.byStatus[item.value] || 0 }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.avgDaysToClose.toFixed(1) }}</div>
          <div class="stat-label">平均处理天数</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Toolbar -->
    <div class="toolbar-row">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索代码/主题/客户..."
        clearable
        style="width: 260px"
        @keyup.enter="loadComplaints"
      />
      <el-select v-model="customerIdFilter" placeholder="客户" clearable style="width: 180px" @change="loadComplaints">
        <el-option v-for="opt in customerOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="severityFilter" placeholder="严重程度" clearable style="width: 130px" @change="loadComplaints">
        <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 130px" @change="loadComplaints">
        <el-option v-for="opt in STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button type="primary" @click="openCreate">+ 新建客诉</el-button>
      <el-button @click="loadComplaints">刷新</el-button>
    </div>

    <!-- Table -->
    <el-table :data="complaints" stripe style="width: 100%" size="small">
      <el-table-column prop="complaintCode" label="投诉代码" width="140" />
      <el-table-column prop="customerName" label="客户" width="150" show-overflow-tooltip />
      <el-table-column label="严重程度" width="90">
        <template #default="{ row }">
          <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small" effect="plain">
            {{ SEVERITY_MAP[row.severity] || row.severity }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="subject" label="主题" min-width="180" show-overflow-tooltip />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
            {{ STATUS_MAP[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="assignedTo" label="负责人" width="80">
        <template #default="{ row }">{{ row.assignedTo || '-' }}</template>
      </el-table-column>
      <el-table-column label="截止日期" width="110">
        <template #default="{ row }">{{ row.dueDate?.slice(0, 10) || '-' }}</template>
      </el-table-column>
      <el-table-column label="创建时间" width="150">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="320" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link size="small" type="primary" @click="viewDetail(row)">详情</el-button>
          <el-button link size="small" type="warning" @click="goToCapa(row)">CAPA</el-button>
          <el-button
            v-if="row.status === 'new'"
            link size="small" type="success"
            @click="transitionStatus(row, 'acknowledged')"
          >确认接收</el-button>
          <el-button
            v-if="row.status === 'in_progress'"
            link size="small" type="success"
            @click="transitionStatus(row, 'closed')"
          >关闭</el-button>
          <el-button link size="small" type="danger" @click="deleteComplaint(row)">删除</el-button>
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
        @current-change="loadComplaints"
      />
    </div>

    <!-- Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? '编辑客诉' : '新建客诉'"
      width="640px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" size="small">
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
              <el-input v-model="form.assignedTo" placeholder="负责人ID" />
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
              <el-date-picker v-model="form.dueDate" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveComplaint">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin: 12px 0; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
.stats-row { margin-bottom: 8px; }
.stat-card { text-align: center; }
.stat-value { font-size: 28px; font-weight: 700; color: var(--el-text-color-primary); }
.stat-label { font-size: 13px; color: var(--el-text-color-secondary); margin-top: 4px; }
</style>

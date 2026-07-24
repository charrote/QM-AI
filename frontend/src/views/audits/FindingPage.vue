<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { WarningFilled, Plus, Back } from '@element-plus/icons-vue'
import { auditApi } from '@/api/audit'
import type { PagedResult } from '@/types/basicData'
import type { AuditFinding } from '@/types/audit'
import {
  FINDING_TYPE_OPTIONS, FINDING_STATUS_OPTIONS,
  FINDING_STATUS_MAP, FINDING_TYPE_MAP, SEVERITY_OPTIONS, SEVERITY_MAP,
} from '@/types/audit'

defineOptions({ name: 'FindingPage' })

const route = useRoute()
const router = useRouter()

const routeAuditId = ref<number | undefined>(route.query.auditId ? Number(route.query.auditId) : undefined)

const searchAuditId = ref('')
const findingTypeFilter = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const findings = ref<AuditFinding[]>([])

const createDialogVisible = ref(false)
const statusDialogVisible = ref(false)
const verifyDialogVisible = ref(false)
const currentFinding = ref<AuditFinding | null>(null)

const createForm = reactive({
  auditId: 0,
  findingType: 'non_conformity',
  severity: 'major',
  description: '',
  evidence: '',
  requirementRef: '',
})

const statusForm = reactive({
  status: 'corrected',
  rectificationPlan: '',
  responsibleUserId: 0,
  rectificationDueDate: '',
})

const verifyForm = reactive({
  verifiedBy: 0,
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadFindings() {
  try {
    const auditId = routeAuditId.value || 0
    const res = await auditApi.findingsList(auditId, {
      findingType: findingTypeFilter.value || undefined,
      status: statusFilter.value || undefined,
    })
    findings.value = res
    total.value = res.length
  } catch (e) {
    console.error('Failed to load findings', e)
  }
}

function openCreate() {
  createForm.auditId = routeAuditId.value || 0
  createForm.findingType = 'non_conformity'
  createForm.severity = 'major'
  createForm.description = ''
  createForm.evidence = ''
  createForm.requirementRef = ''
  createDialogVisible.value = true
}

function openUpdateStatus(row: AuditFinding) {
  currentFinding.value = row
  statusForm.status = row.status || 'open'
  statusForm.rectificationPlan = row.rectificationPlan || ''
  statusForm.responsibleUserId = row.responsibleUserId || 0
  statusForm.rectificationDueDate = row.rectificationDueDate?.slice(0, 10) || ''
  statusDialogVisible.value = true
}

function openVerify(row: AuditFinding) {
  currentFinding.value = row
  verifyForm.verifiedBy = 0
  verifyDialogVisible.value = true
}

async function createFinding() {
  if (!createForm.auditId || !createForm.description) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    await auditApi.createFinding(createForm.auditId, {
      auditId: createForm.auditId,
      findingType: createForm.findingType,
      severity: createForm.severity,
      description: createForm.description,
      evidence: createForm.evidence,
      requirementRef: createForm.requirementRef,
    })
    ElMessage.success('不符合项已创建')
    createDialogVisible.value = false
    await loadFindings()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function updateStatus() {
  if (!currentFinding.value) return
  try {
    await auditApi.updateFindingStatus(currentFinding.value.id, {
      status: statusForm.status,
      rectificationPlan: statusForm.rectificationPlan,
      responsibleUserId: statusForm.responsibleUserId || undefined,
      rectificationDueDate: statusForm.rectificationDueDate || undefined,
    })
    ElMessage.success('状态已更新')
    statusDialogVisible.value = false
    await loadFindings()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function verifyFinding() {
  if (!currentFinding.value) return
  try {
    await auditApi.verifyFinding(currentFinding.value.id, {
      verifiedBy: verifyForm.verifiedBy,
    })
    ElMessage.success('已验证')
    verifyDialogVisible.value = false
    await loadFindings()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteFinding(row: AuditFinding) {
  try {
    await ElMessageBox.confirm(`确定删除该不符合项吗？`, '确认', { type: 'warning' })
    await auditApi.removeFinding(row.id)
    ElMessage.success('已删除')
    await loadFindings()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

onMounted(async () => {
  await loadFindings()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__main">
        <el-button :icon="Back" link @click="router.go(-1)" style="margin-right: 8px">返回</el-button>
        <el-icon class="page-header__icon" :size="28"><WarningFilled /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">不符合项</h2>
          <p class="page-header__subtitle">审核不符合项管理与跟踪</p>
        </div>
      </div>
      <div class="page-header__actions">
        <el-button :icon="Plus" type="primary" @click="openCreate">新建不符合项</el-button>
      </div>
    </div>

    <!-- Search Toolbar -->
    <div class="toolbar-row">
      <el-input
        v-model="searchAuditId"
        placeholder="搜索审核ID..."
        clearable
        style="width: 200px"
        @keyup.enter="loadFindings"
      />
      <el-select v-model="findingTypeFilter" placeholder="发现类型" clearable style="width: 140px" @change="loadFindings">
        <el-option v-for="opt in FINDING_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 140px" @change="loadFindings">
        <el-option v-for="opt in FINDING_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button @click="loadFindings">刷新</el-button>
      <div class="toolbar-spacer" />
    </div>

    <!-- Data Table Card -->
    <div class="data-card">
      <el-table :data="findings" stripe style="width: 100%">
        <el-table-column prop="id" label="ID" width="60" align="center" />
        <el-table-column prop="auditId" label="审核ID" width="90" align="center" />
        <el-table-column label="发现类型" width="120" align="center">
          <template #default="{ row }">
            <el-tag :type="(FINDING_TYPE_OPTIONS.find(o => o.value === row.findingType)?.type) || 'info'" size="small" effect="plain">
              {{ FINDING_TYPE_MAP[row.findingType] || row.findingType }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="严重程度" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small" effect="plain">
              {{ SEVERITY_MAP[row.severity] || row.severity }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="description" label="描述" min-width="240" show-overflow-tooltip />
        <el-table-column label="状态" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="FINDING_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
              {{ FINDING_STATUS_MAP[row.status] || row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="责任人" width="90" align="center">
          <template #default="{ row }">{{ row.responsibleUserId || '-' }}</template>
        </el-table-column>
        <el-table-column label="截止日期" width="120" align="center">
          <template #default="{ row }">{{ row.rectificationDueDate?.slice(0, 10) || '-' }}</template>
        </el-table-column>
        <el-table-column label="创建时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="openUpdateStatus(row)">更新状态</el-button>
            <el-button link size="small" type="success" @click="openVerify(row)">验证</el-button>
            <el-button link size="small" type="danger" @click="deleteFinding(row)">删除</el-button>
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
          size="small"
          @current-change="loadFindings"
        />
      </div>
    </div>

    <!-- Create Dialog -->
    <el-dialog
      v-model="createDialogVisible"
      title="新建不符合项"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form :model="createForm" label-width="100px">
        <el-form-item label="审核ID" required>
          <el-input v-model="createForm.auditId" placeholder="审核ID" type="number" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="发现类型" required>
              <el-select v-model="createForm.findingType" style="width: 100%">
                <el-option v-for="opt in FINDING_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="严重程度">
              <el-select v-model="createForm.severity" style="width: 100%">
                <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="描述" required>
          <el-input v-model="createForm.description" type="textarea" :rows="3" placeholder="不符合项描述" />
        </el-form-item>
        <el-form-item label="证据">
          <el-input v-model="createForm.evidence" type="textarea" :rows="2" placeholder="相关证据" />
        </el-form-item>
        <el-form-item label="要求引用">
          <el-input v-model="createForm.requirementRef" placeholder="相关标准要求" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="createFinding">保存</el-button>
      </template>
    </el-dialog>

    <!-- Update Status Dialog -->
    <el-dialog
      v-model="statusDialogVisible"
      title="更新状态"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form :model="statusForm" label-width="110px">
        <el-form-item label="状态" required>
          <el-select v-model="statusForm.status" style="width: 100%">
            <el-option v-for="opt in FINDING_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="整改计划">
          <el-input v-model="statusForm.rectificationPlan" type="textarea" :rows="3" placeholder="整改措施" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="责任人">
              <el-input v-model="statusForm.responsibleUserId" placeholder="责任人ID" type="number" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="截止日期">
              <el-date-picker v-model="statusForm.rectificationDueDate" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="statusDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="updateStatus">保存</el-button>
      </template>
    </el-dialog>

    <!-- Verify Dialog -->
    <el-dialog
      v-model="verifyDialogVisible"
      title="验证不符合项"
      width="400px"
      :close-on-click-modal="false"
    >
      <el-form :model="verifyForm" label-width="80px">
        <el-form-item label="验证人ID" required>
          <el-input v-model="verifyForm.verifiedBy" placeholder="验证人ID" type="number" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="verifyDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="verifyFinding">验证</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; gap: 16px; }

/* Page Header */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--el-bg-color);
  border-radius: var(--radius-lg, 8px);
  padding: 16px 20px;
  border: 1px solid var(--el-border-color-lighter);
}
.page-header__main { display: flex; align-items: center; gap: 12px; }
.page-header__icon { color: var(--el-color-primary); flex-shrink: 0; }
.page-header__title { margin: 0; font-size: 20px; font-weight: 600; color: var(--el-text-color-primary); line-height: 1.2; }
.page-header__subtitle { margin: 4px 0 0; font-size: 13px; color: var(--el-text-color-secondary); }
.page-header__actions { display: flex; gap: 8px; }

/* Toolbar */
.toolbar-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.toolbar-spacer { flex: 1; }

/* Data Card */
.data-card {
  background: var(--el-bg-color);
  border-radius: var(--radius-lg, 8px);
  border: 1px solid var(--el-border-color-lighter);
  overflow: hidden;
}
.data-card :deep(.el-table th.el-table__cell) {
  background: var(--el-fill-color-light) !important;
}

/* Pagination */
.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: var(--space-4, 12px);
  border-top: 1px solid var(--el-border-color-lighter);
}
</style>
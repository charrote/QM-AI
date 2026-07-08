<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { auditApi } from '@/api/audit'
import type { PagedResult } from '@/types/basicData'
import type { Audit } from '@/types/audit'
import {
  AUDIT_TYPE_OPTIONS, AUDIT_STATUS_OPTIONS,
  AUDIT_STATUS_MAP,
} from '@/types/audit'

defineOptions({ name: 'AuditListPage' })

const router = useRouter()

const searchKeyword = ref('')
const auditTypeFilter = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const audits = ref<Audit[]>([])

const dialogVisible = ref(false)
const isEditing = ref(false)
const currentId = ref<number | null>(null)
const form = reactive({
  auditCode: '',
  auditType: 'internal',
  title: '',
  description: '',
  startDate: '',
  endDate: '',
  auditorId: 0,
  scope: '',
  status: 'planned',
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadAudits() {
  try {
    const res = await auditApi.list({
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
      auditType: auditTypeFilter.value || undefined,
      status: statusFilter.value || undefined,
    })
    audits.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load audits', e)
  }
}

function openCreate() {
  isEditing.value = false
  currentId.value = null
  form.auditCode = ''
  form.auditType = 'internal'
  form.title = ''
  form.description = ''
  form.startDate = ''
  form.endDate = ''
  form.auditorId = 0
  form.scope = ''
  form.status = 'planned'
  dialogVisible.value = true
}

function openEdit(row: Audit) {
  isEditing.value = true
  currentId.value = row.id
  form.auditCode = row.auditCode
  form.auditType = row.auditType
  form.title = row.title
  form.description = row.description || ''
  form.startDate = row.startDate?.slice(0, 10) || ''
  form.endDate = row.endDate?.slice(0, 10) || ''
  form.auditorId = row.auditorId
  form.scope = row.scope || ''
  dialogVisible.value = true
}

async function saveAudit() {
  if (!form.auditCode || !form.title) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    if (isEditing.value && currentId.value) {
      await auditApi.update(currentId.value, {
        auditType: form.auditType,
        title: form.title,
        description: form.description,
        startDate: form.startDate,
        endDate: form.endDate,
        auditorId: form.auditorId,
        scope: form.scope,
      })
      ElMessage.success('审核已更新')
    } else {
      await auditApi.create({
        auditCode: form.auditCode,
        auditType: form.auditType,
        title: form.title,
        description: form.description,
        startDate: form.startDate,
        endDate: form.endDate,
        auditorId: form.auditorId,
        scope: form.scope,
      })
      ElMessage.success('审核已创建')
    }
    dialogVisible.value = false
    await loadAudits()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteAudit(row: Audit) {
  try {
    await ElMessageBox.confirm(`确定删除审核「${row.auditCode}」吗？`, '确认', { type: 'warning' })
    await auditApi.remove(row.id)
    ElMessage.success('已删除')
    await loadAudits()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

function viewDetail(row: Audit) {
  router.push({ name: 'AuditDetail', query: { id: String(row.id) } })
}

onMounted(async () => {
  await loadAudits()
})
</script>

<template>
  <div class="page-container">
    <!-- Toolbar -->
    <div class="toolbar-row">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索代码/标题/范围..."
        clearable
        style="width: 260px"
        @keyup.enter="loadAudits"
      />
      <el-select v-model="auditTypeFilter" placeholder="审核类型" clearable style="width: 130px" @change="loadAudits">
        <el-option v-for="opt in AUDIT_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 130px" @change="loadAudits">
        <el-option v-for="opt in AUDIT_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button type="primary" @click="openCreate">+ 新建审核</el-button>
      <el-button @click="loadAudits">刷新</el-button>
    </div>

    <!-- Table -->
    <el-table :data="audits" stripe style="width: 100%" size="small">
      <el-table-column prop="auditCode" label="审核代码" width="140" />
      <el-table-column label="审核类型" width="100">
        <template #default="{ row }">
          <el-tag size="small" effect="plain">{{ AUDIT_TYPE_OPTIONS.find(o => o.value === row.auditType)?.label || row.auditType }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="title" label="标题" min-width="200" show-overflow-tooltip />
      <el-table-column label="审核人ID" width="80">
        <template #default="{ row }">{{ row.auditorId || '-' }}</template>
      </el-table-column>
      <el-table-column prop="scope" label="范围" min-width="150" show-overflow-tooltip />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="AUDIT_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
            {{ AUDIT_STATUS_MAP[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="开始日期" width="110">
        <template #default="{ row }">{{ row.startDate?.slice(0, 10) || '-' }}</template>
      </el-table-column>
      <el-table-column label="结束日期" width="110">
        <template #default="{ row }">{{ row.endDate?.slice(0, 10) || '-' }}</template>
      </el-table-column>
      <el-table-column label="创建时间" width="150">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="180" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link size="small" type="primary" @click="viewDetail(row)">详情</el-button>
          <el-button link size="small" type="danger" @click="deleteAudit(row)">删除</el-button>
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
        @current-change="loadAudits"
      />
    </div>

    <!-- Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? '编辑审核' : '新建审核'"
      width="640px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="100px" size="small">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="审核代码" required>
              <el-input v-model="form.auditCode" placeholder="如: AUD-2026-001" :disabled="isEditing" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="审核类型" required>
              <el-select v-model="form.auditType" style="width: 100%">
                <el-option v-for="opt in AUDIT_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="标题" required>
          <el-input v-model="form.title" placeholder="审核标题" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="审核描述" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="开始日期">
              <el-date-picker v-model="form.startDate" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="结束日期">
              <el-date-picker v-model="form.endDate" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="审核人ID">
              <el-input v-model="form.auditorId" placeholder="审核人ID" type="number" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="范围">
              <el-input v-model="form.scope" placeholder="审核范围" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveAudit">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin: 12px 0; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

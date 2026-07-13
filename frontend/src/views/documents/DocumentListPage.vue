<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { documentApi } from '@/api/document'
import { DOC_TYPE_OPTIONS, DOC_STATUS_OPTIONS, DOC_STATUS_MAP } from '@/types/document'
import { Search, Upload, Refresh, Download, Edit, Delete, Check, Close } from '@element-plus/icons-vue'
import type { Document } from '@/types/document'
import type { PagedResult } from '@/types/basicData'
import type { UploadRawFile, UploadFile } from 'element-plus'

defineOptions({ name: 'DocumentListPage' })

// ─── State ──────────────────────────────────────────────
const searchKeyword = ref('')
const searchDocType = ref('')
const searchStatus = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const documents = ref<Document[]>([])
const tableLoading = ref(false)

// ─── Dialog ─────────────────────────────────────────────
const dialogVisible = ref(false)
const isEditing = ref(false)
const currentDocId = ref<number | null>(null)
const docForm = reactive({ title: '', docType: '', minioKey: '' })

// ─── Upload ─────────────────────────────────────────────

// ─── Approve / Reject ──────────────────────────────────
const approveDialogVisible = ref(false)
const approveDocId = ref<number | null>(null)
const approveForm = reactive({ approvedBy: '' })

const rejectDialogVisible = ref(false)
const rejectDocId = ref<number | null>(null)
const rejectForm = reactive({ reason: '' })

// ─── Helpers ────────────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function formatFileSize(bytes?: number) {
  if (!bytes || bytes === 0) return '-'
  const units = ['B', 'KB', 'MB', 'GB']
  let i = 0
  let size = bytes
  while (size >= 1024 && i < units.length - 1) { size /= 1024; i++ }
  return size.toFixed(i > 0 ? 2 : 0) + ' ' + units[i]
}

function statusTagType(status: string) {
  return DOC_STATUS_MAP[status]?.type || 'info'
}

function statusLabel(status: string) {
  return DOC_STATUS_MAP[status]?.label || status
}

// ─── CRUD ───────────────────────────────────────────────
async function loadDocuments() {
  tableLoading.value = true
  try {
    const res = await documentApi.getList({
      page: page.value, pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
      docType: searchDocType.value || undefined,
      status: searchStatus.value || undefined,
    })
    documents.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load documents', e)
  } finally {
    tableLoading.value = false
  }
}

function handleSearch() {
  page.value = 1
  loadDocuments()
}

function resetSearch() {
  searchKeyword.value = ''
  searchDocType.value = ''
  searchStatus.value = ''
  page.value = 1
  loadDocuments()
}

function openCreate() {
  isEditing.value = false
  currentDocId.value = null
  docForm.title = ''
  docForm.docType = ''
  docForm.minioKey = ''
  dialogVisible.value = true
}

function openEdit(row: Document) {
  isEditing.value = true
  currentDocId.value = row.id
  docForm.title = row.title
  docForm.docType = row.docType
  docForm.minioKey = row.minioKey || ''
  dialogVisible.value = true
}

async function saveDoc() {
  if (!docForm.title || !docForm.docType) {
    ElMessage.warning('请填写标题和文档类型')
    return
  }
  try {
    if (isEditing.value && currentDocId.value) {
      await documentApi.update(currentDocId.value, { title: docForm.title, docType: docForm.docType })
      ElMessage.success('文档已更新')
    } else {
      await documentApi.create({ title: docForm.title, docType: docForm.docType, minioKey: docForm.minioKey || undefined })
      ElMessage.success('文档已创建')
    }
    dialogVisible.value = false
    await loadDocuments()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteDoc(row: Document) {
  try {
    await ElMessageBox.confirm(`确定删除文档「${row.title}」吗？`, '确认', { type: 'warning' })
    await documentApi.remove(row.id)
    ElMessage.success('已删除')
    await loadDocuments()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

async function downloadDoc(row: Document) {
  const url = row.minioKey ? `/api/v1/m12/documents/download?key=${encodeURIComponent(row.minioKey)}` : ''
  if (url) {
    window.open(url, '_blank')
  } else {
    ElMessage.warning('该文档暂无可下载的文件')
  }
}

// ─── Approve / Reject ──────────────────────────────────
function openApprove(row: Document) {
  approveDocId.value = row.id
  approveForm.approvedBy = ''
  approveDialogVisible.value = true
}

async function approveDoc() {
  if (!approveDocId.value) return
  if (!approveForm.approvedBy) {
    ElMessage.warning('请填写审批人')
    return
  }
  try {
    await documentApi.approve(approveDocId.value, { approvedBy: Number(approveForm.approvedBy) })
    ElMessage.success('文档已批准')
    approveDialogVisible.value = false
    await loadDocuments()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '审批失败')
  }
}

function openReject(row: Document) {
  rejectDocId.value = row.id
  rejectForm.reason = ''
  rejectDialogVisible.value = true
}

async function rejectDoc() {
  if (!rejectDocId.value) return
  if (!rejectForm.reason) {
    ElMessage.warning('请填写驳回原因')
    return
  }
  try {
    await documentApi.reject(rejectDocId.value, { reason: rejectForm.reason })
    ElMessage.success('文档已驳回')
    rejectDialogVisible.value = false
    await loadDocuments()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '驳回失败')
  }
}

onMounted(() => {
  loadDocuments()
})
</script>

<template>
  <div class="page-container">
    <!-- Toolbar -->
    <div class="toolbar-row">
      <el-input v-model="searchKeyword" placeholder="搜索标题/关键词..." clearable style="width: 240px" @keyup.enter="handleSearch" />
      <el-select v-model="searchDocType" placeholder="文档类型" clearable style="width: 160px">
        <el-option v-for="opt in DOC_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="searchStatus" placeholder="状态" clearable style="width: 140px">
        <el-option v-for="opt in DOC_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button type="primary" :icon="Search" @click="handleSearch">搜索</el-button>
      <el-button @click="resetSearch">重置</el-button>
      <el-button type="primary" @click="openCreate">+ 新建文档</el-button>
      <el-button :icon="Refresh" @click="loadDocuments">刷新</el-button>
    </div>

    <!-- Table -->
    <el-table :data="documents" stripe v-loading="tableLoading" style="width: 100%" >
      <el-table-column prop="title" label="标题" min-width="180" show-overflow-tooltip />
      <el-table-column prop="docType" label="文档类型" width="140">
        <template #default="{ row }">
          {{ DOC_TYPE_OPTIONS.find(o => o.value === row.docType)?.label || row.docType }}
        </template>
      </el-table-column>
      <el-table-column prop="version" label="版本" width="60" />
      <el-table-column prop="status" label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="statusTagType(row.status)" size="small" effect="plain">{{ statusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="fileSizeBytes" label="大小" width="90">
        <template #default="{ row }">{{ formatFileSize(row.fileSizeBytes) }}</template>
      </el-table-column>
      <el-table-column prop="createdBy" label="创建人" width="100" />
      <el-table-column prop="createdAt" label="创建时间" width="170">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="300" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          
          <el-button v-if="row.status === 'draft'" link size="small" type="primary" @click="downloadDoc(row)">下载</el-button>
          <el-button v-if="row.status === 'draft'" link size="small" type="success" :icon="Check" @click="openApprove(row)">审批</el-button>
          <el-button v-if="row.status === 'reviewing'" link size="small" type="danger" :icon="Close" @click="openReject(row)">驳回</el-button>
          <el-button link size="small" type="danger" @click="deleteDoc(row)">删除</el-button>
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
        @current-change="loadDocuments"
      />
    </div>

    <!-- Dialog: Create/Edit -->
    <el-dialog v-model="dialogVisible" :title="isEditing ? '编辑文档' : '新建文档'" width="520px" :close-on-click-modal="false">
      <el-form :model="docForm" label-width="90px" >
        <el-form-item label="标题" required>
          <el-input v-model="docForm.title" placeholder="请输入文档标题" />
        </el-form-item>
        <el-form-item label="文档类型" required>
          <el-select v-model="docForm.docType" placeholder="选择类型" style="width: 100%">
            <el-option v-for="opt in DOC_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="MinIO Key">
          <el-input v-model="docForm.minioKey" placeholder="上传后回写" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveDoc">保存</el-button>
      </template>
    </el-dialog>

    <!-- Dialog: Upload -->

    <!-- Dialog: Approve -->
    <el-dialog v-model="approveDialogVisible" title="审批文档" width="440px" :close-on-click-modal="false">
      <el-form :model="approveForm" label-width="90px" >
        <el-form-item label="审批人ID" required>
          <el-input-number v-model="approveForm.approvedBy" :min="1" placeholder="审批人ID" style="width: 100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="approveDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="approveDoc">确认批准</el-button>
      </template>
    </el-dialog>

    <!-- Dialog: Reject -->
    <el-dialog v-model="rejectDialogVisible" title="驳回文档" width="440px" :close-on-click-modal="false">
      <el-form :model="rejectForm" label-width="90px" >
        <el-form-item label="驳回原因" required>
          <el-input v-model="rejectForm.reason" type="textarea" :rows="4" placeholder="请输入驳回原因" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="rejectDialogVisible = false">取消</el-button>
        <el-button type="danger" @click="rejectDoc">确认驳回</el-button>
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
</style>

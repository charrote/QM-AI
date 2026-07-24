<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, Plus, Edit, Delete, TrendCharts, Box } from '@element-plus/icons-vue'
import { defectApi } from '@/api/defect'
import type { Defect, CreateDefect } from '@/types/defect'
import {
  SEVERITY_OPTIONS, SEVERITY_MAP, DEFECT_STATUS_OPTIONS, DEFECT_STATUS_MAP,
  SOURCE_TYPE_OPTIONS, SOURCE_TYPE_MAP,
} from '@/types/defect'
import DataTable, { type Column } from '@/components/common/DataTable.vue'
import StatusTag from '@/components/common/StatusTag.vue'
import FormDialog from '@/components/common/FormDialog.vue'
import { usePagination } from '@/composables/usePagination'

defineOptions({ name: 'DefectsPage' })

const router = useRouter()

// ─── Pagination ──────────────────────────────────────────
const pagination = usePagination(1, 20)

// ─── Search state ────────────────────────────────────────
const searchKeyword = ref('')
const sourceTypeFilter = ref('')
const severityFilter = ref('')
const statusFilter = ref('')

const defects = ref<Defect[]>([])

// ─── Dialog state ────────────────────────────────────────
const dialogVisible = ref(false)
const isEditing = ref(false)
const currentDefectId = ref<number | null>(null)
const submitLoading = ref(false)

const defectForm = reactive<CreateDefect>({
  defectCode: '', severity: 'minor', sourceType: 'IQC', sourceId: 0,
  productId: 0, batchId: 0, equipmentId: 0, quantity: 0,
  description: '', discoveredBy: '', discoveredAt: new Date().toISOString().slice(0, 10),
})

// ─── Table columns ───────────────────────────────────────
const tableColumns = computed<Column[]>(() => [
  { prop: 'defectCode', label: '缺陷代码', width: 140 },
  { label: '严重程度', slotName: 'severity', width: 90, align: 'center' },
  { prop: 'sourceType', label: '来源', slotName: 'sourceType', width: 110 },
  { prop: 'quantity', label: '不良数', width: 70, align: 'right' },
  { prop: 'description', label: '描述', minWidth: 180, showOverflowTooltip: true },
  { prop: 'discoveredBy', label: '发现人', width: 90 },
  { label: '发现时间', slotName: 'discoveredAt', width: 160 },
  { label: '状态', slotName: 'status', width: 80, align: 'center' },
  { label: '操作', slotName: 'actions', width: 260, fixed: 'right' as const },
])

// ─── Data loading ────────────────────────────────────────
async function loadDefects() {
  pagination.loading.value = true
  try {
    const res = await defectApi.getAllDefects({
      page: pagination.currentPage.value,
      pageSize: pagination.pageSize.value,
      keyword: searchKeyword.value,
      sourceType: sourceTypeFilter.value || undefined,
      severity: severityFilter.value || undefined,
      status: statusFilter.value || undefined,
    })
    defects.value = res.items
    pagination.total.value = res.total
  } catch (e) {
    console.error('Failed to load defects', e)
  } finally {
    pagination.loading.value = false
  }
}

function handlePageChange(page: number) {
  pagination.goToPage(page)
  loadDefects()
}

function handleSizeChange(size: number) {
  pagination.pageSize.value = size
  pagination.goToPage(1)
  loadDefects()
}

// ─── Search helpers ──────────────────────────────────────
function applySearch() {
  pagination.goToPage(1)
  loadDefects()
}

function resetSearch() {
  searchKeyword.value = ''
  sourceTypeFilter.value = ''
  severityFilter.value = ''
  statusFilter.value = ''
  applySearch()
}

// ─── CRUD operations ─────────────────────────────────────
function openCreate() {
  isEditing.value = false
  currentDefectId.value = null
  Object.assign(defectForm, {
    defectCode: '', severity: 'minor', sourceType: 'IQC', sourceId: 0,
    productId: 0, batchId: 0, equipmentId: 0, quantity: 0,
    description: '', discoveredBy: '', discoveredAt: new Date().toISOString().slice(0, 10),
  })
  dialogVisible.value = true
}

function openEdit(row: Defect) {
  isEditing.value = true
  currentDefectId.value = row.id
  Object.assign(defectForm, {
    defectCode: row.defectCode, severity: row.severity, sourceType: row.sourceType,
    sourceId: row.sourceId, productId: row.productId, batchId: row.batchId,
    equipmentId: row.equipmentId, quantity: row.quantity, description: row.description,
    discoveredBy: row.discoveredBy, discoveredAt: row.discoveredAt?.slice(0, 10) || '',
  })
  dialogVisible.value = true
}

async function saveDefect(formData: Partial<CreateDefect>) {
  if (!formData.defectCode || !formData.description) {
    ElMessage.warning('请填写缺陷代码和描述')
    return
  }
  submitLoading.value = true
  try {
    if (isEditing.value && currentDefectId.value) {
      await defectApi.updateDefect(currentDefectId.value, formData)
      ElMessage.success('缺陷记录已更新')
    } else {
      await defectApi.createDefect(formData as CreateDefect)
      ElMessage.success('缺陷记录已创建')
    }
    dialogVisible.value = false
    await loadDefects()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

async function deleteDefect(row: Defect) {
  try {
    await ElMessageBox.confirm(`确定删除缺陷「${row.defectCode}」吗？`, '确认', { type: 'warning' })
    await defectApi.deleteDefect(row.id)
    ElMessage.success('已删除')
    await loadDefects()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

// ─── Navigation ──────────────────────────────────────────
function goToCapa(row: Defect) {
  router.push({ name: 'Capa', query: { defectId: String(row.id) } })
}

function goToScrapRework(row: Defect) {
  router.push({ name: 'ScrapRework', query: { defectId: String(row.id) } })
}

onMounted(loadDefects)
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><WarningFilled /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>缺陷管理</h2>
          <p>质量缺陷记录、分类与处置的中央管控平台</p>
        </div>
      </div>
    </div>

    <!-- Search Toolbar -->
    <el-card shadow="never" class="search-card">
      <div class="search-bar">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索缺陷代码/描述..."
          :prefix-icon="Search"
          clearable
          style="width: 260px"
          @keyup.enter="applySearch"
        />
        <el-select v-model="sourceTypeFilter" placeholder="来源类型" clearable style="width: 140px" @change="applySearch">
          <el-option v-for="opt in SOURCE_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-select v-model="severityFilter" placeholder="严重程度" clearable style="width: 130px" @change="applySearch">
          <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 130px" @change="applySearch">
          <el-option v-for="opt in DEFECT_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-button :icon="Refresh" @click="loadDefects">刷新</el-button>
        <div class="search-spacer" />
        <el-button type="primary" :icon="Plus" @click="openCreate">新建缺陷</el-button>
      </div>
    </el-card>

    <!-- Data Table -->
    <el-card shadow="never" class="table-card">
      <DataTable
        :data="defects"
        :columns="tableColumns"
        :loading="pagination.loading.value"
        :total="pagination.total.value"
        :current-page="pagination.currentPage.value"
        :page-size="pagination.pageSize.value"
        @update:current-page="handlePageChange"
        @update:page-size="handleSizeChange"
        @row-click="(row: Defect) => openEdit(row)"
      >
        <template #severity="{ row }">
          <StatusTag :status="row.severity" :text="SEVERITY_MAP[row.severity] || row.severity" />
        </template>
        <template #sourceType="{ row }">
          {{ SOURCE_TYPE_MAP[row.sourceType] || row.sourceType }}
        </template>
        <template #discoveredAt="{ row }">
          {{ row.discoveredAt ? new Date(row.discoveredAt).toLocaleString('zh-CN') : '-' }}
        </template>
        <template #status="{ row }">
          <StatusTag :status="row.status" :text="DEFECT_STATUS_MAP[row.status] || row.status" />
        </template>
        <template #actions="{ row }">
          <el-button link size="small" type="primary" :icon="Edit" @click.stop="openEdit(row)">编辑</el-button>
          <el-button link size="small" type="warning" :icon="TrendCharts" @click.stop="goToCapa(row)">CAPA</el-button>
          <el-button link size="small" type="success" :icon="Box" @click.stop="goToScrapRework(row)">报废/返工</el-button>
          <el-button link size="small" type="danger" :icon="Delete" @click.stop="deleteDefect(row)">删除</el-button>
        </template>
      </DataTable>
    </el-card>

    <!-- Create/Edit Dialog -->
    <FormDialog
      :visible="dialogVisible"
      :title="isEditing ? '编辑缺陷记录' : '新建缺陷记录'"
      :loading="submitLoading"
      width="680px"
      @update:visible="dialogVisible = $event"
      @submit="saveDefect(defectForm as CreateDefect)"
    >
      <el-form :model="defectForm" label-width="90px" size="default">
        <el-divider content-position="left">基本信息</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="缺陷代码" required>
              <el-input v-model="defectForm.defectCode" placeholder="如: DEF-001" :disabled="isEditing" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="严重程度" required>
              <el-select v-model="defectForm.severity" style="width: 100%">
                <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="来源类型" required>
              <el-select v-model="defectForm.sourceType" style="width: 100%">
                <el-option v-for="opt in SOURCE_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="不良数量" required>
              <el-input-number v-model="defectForm.quantity" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">关联信息</el-divider>
        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item label="产品ID">
              <el-input-number v-model="defectForm.productId" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="批次ID">
              <el-input-number v-model="defectForm.batchId" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="设备ID">
              <el-input-number v-model="defectForm.equipmentId" :min="0" controls-position="right" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">发现信息</el-divider>
        <el-form-item label="描述" required>
          <el-input v-model="defectForm.description" type="textarea" :rows="3" placeholder="描述不良现象" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="发现人">
              <el-input v-model="defectForm.discoveredBy" placeholder="发现人姓名" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="发现时间">
              <el-date-picker v-model="defectForm.discoveredAt" type="date" value-format="YYYY-MM-DD" style="width: 100%" placeholder="选择日期" />
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
</style>

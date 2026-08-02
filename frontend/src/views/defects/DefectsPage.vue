<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, Plus, WarningFilled } from '@element-plus/icons-vue'
import { defectApi } from '@/api/defect'
import type { Defect, CreateDefect } from '@/types/defect'
import {
  SEVERITY_OPTIONS, SEVERITY_MAP, DEFECT_STATUS_OPTIONS, DEFECT_STATUS_MAP,
  SOURCE_TYPE_OPTIONS, SOURCE_TYPE_MAP,
} from '@/types/defect'
import StatusTag from '@/components/common/StatusTag.vue'
import RightPanel from '@/components/layout/RightPanel.vue'
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

// ─── Stats ─────────────────────────────────────────────
const stats = computed(() => ({
  total: defects.value.length,
  critical: defects.value.filter(d => d.severity === 'critical').length,
  major: defects.value.filter(d => d.severity === 'major').length,
  minor: defects.value.filter(d => d.severity === 'minor').length,
  active: defects.value.filter(d => d.status === 'open' || d.status === 'active').length,
  resolved: defects.value.filter(d => d.status === 'resolved' || d.status === 'closed').length,
}))

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

async function saveDefect() {
  if (!defectForm.defectCode || !defectForm.description) {
    ElMessage.warning('请填写缺陷代码和描述')
    return
  }
  submitLoading.value = true
  try {
    if (isEditing.value && currentDefectId.value) {
      await defectApi.updateDefect(currentDefectId.value, defectForm as CreateDefect)
      ElMessage.success('缺陷记录已更新')
    } else {
      await defectApi.createDefect(defectForm as CreateDefect)
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
    <!-- Page Header Banner -->
    <div class="m07-header-banner m07-header-banner--danger">
      <div class="m07-header-banner-main">
        <div class="m07-header-banner-left">
          <div class="m07-header-banner-icon">
            <el-icon :size="24"><WarningFilled /></el-icon>
          </div>
          <div class="m07-header-banner-text">
            <div class="m07-header-banner-title">缺陷管理</div>
            <div class="m07-header-banner-subtitle">质量缺陷记录、分类与处置的中央管控平台</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stats Bar -->
    <div v-if="stats.total > 0" class="m07-stat-grid">
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--primary">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">缺陷总数</div>
          <div class="m07-stat-value m07-stat-value--primary">{{ stats.total }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--danger">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">严重</div>
          <div class="m07-stat-value m07-stat-value--danger">{{ stats.critical }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--warning">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">主要</div>
          <div class="m07-stat-value" style="color: #ca8a04">{{ stats.major }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--info">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">一般</div>
          <div class="m07-stat-value" style="color: #6b7280">{{ stats.minor }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--primary">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">待处理</div>
          <div class="m07-stat-value m07-stat-value--primary">{{ stats.active }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--success">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">已处理</div>
          <div class="m07-stat-value m07-stat-value--success">{{ stats.resolved }}</div>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="m07-table-card">
      <div class="m07-table-card__header">
        <div class="m07-table-card__title">
          <el-icon><WarningFilled /></el-icon>
          <span>缺陷记录列表</span>
          <el-tag v-if="pagination.total.value" type="info" size="small" class="m07-table-card__count">
            共 {{ pagination.total.value }} 条
          </el-tag>
        </div>
        <div class="m07-table-card__toolbar">
          <div class="m07-filter-bar">
            <el-input v-model="searchKeyword" placeholder="搜索缺陷代码/描述..." :prefix-icon="Search" clearable style="width: 220px" @keyup.enter="applySearch" @clear="applySearch" />
            <el-select v-model="sourceTypeFilter" placeholder="来源类型" clearable style="width: 120px" @clear="applySearch">
              <el-option v-for="opt in SOURCE_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
            <el-select v-model="severityFilter" placeholder="严重程度" clearable style="width: 110px" @clear="applySearch">
              <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
            <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 110px" @clear="applySearch">
              <el-option v-for="opt in DEFECT_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </div>
          <el-button @click="resetSearch" text>
            <el-icon><Refresh /></el-icon>重置
          </el-button>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>新建缺陷
          </el-button>
        </div>
      </div>

      <div class="m07-table-card__body">
        <el-table
          :data="defects"
          v-loading="pagination.loading.value"
          border
          stripe
          style="width: 100%"
          size="small"
          empty-text="暂无数据"
          class="m07-table"
          @row-click="(row: Defect) => openEdit(row)"
        >
          <el-table-column prop="defectCode" label="缺陷代码" width="140" />
          <el-table-column label="严重程度" width="90">
            <template #default="{ row }">
              <StatusTag :status="row.severity" :text="SEVERITY_MAP[row.severity] || row.severity" />
            </template>
          </el-table-column>
          <el-table-column label="来源" width="110">
            <template #default="{ row }">
              {{ SOURCE_TYPE_MAP[row.sourceType] || row.sourceType }}
            </template>
          </el-table-column>
          <el-table-column prop="quantity" label="不良数" width="70" align="right" />
          <el-table-column prop="description" label="描述" min-width="180" show-overflow-tooltip />
          <el-table-column prop="discoveredBy" label="发现人" width="90" />
          <el-table-column label="发现时间" width="160">
            <template #default="{ row }">
              {{ row.discoveredAt ? new Date(row.discoveredAt).toLocaleString('zh-CN') : '-' }}
            </template>
          </el-table-column>
          <el-table-column label="状态" width="80">
            <template #default="{ row }">
              <StatusTag :status="row.status" :text="DEFECT_STATUS_MAP[row.status] || row.status" />
            </template>
          </el-table-column>
          <el-table-column label="操作" width="260" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="openEdit(row)">编辑</el-button>
              <el-button size="small" type="warning" link @click.stop="goToCapa(row)">CAPA</el-button>
              <el-button size="small" type="success" link @click.stop="goToScrapRework(row)">报废/返工</el-button>
              <el-button size="small" type="danger" link @click.stop="deleteDefect(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="m07-table-card__footer">
        <el-pagination
          v-model:current-page="pagination.currentPage.value"
          v-model:page-size="pagination.pageSize.value"
          :total="pagination.total.value"
          :page-sizes="[10, 20, 50, 100]"
          :small="true"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
          class="data-card__pagination"
        />
      </div>
    </div>

    <!-- Create/Edit RightPanel -->
    <RightPanel
      v-model:visible="dialogVisible"
      :title="isEditing ? '编辑缺陷记录' : '新建缺陷记录'"
      :width="680"
    >
      <template #body>
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
      </template>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveDefect" :loading="submitLoading">保存</el-button>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
/* ─── Stats Grid ──────────────────────────── */
.m07-stat-grid {
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
}

/* ─── Table Link Buttons ──────────────────── */
.m07-table .el-button.is-link {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
}

.m07-table .el-button.is-link:hover,
.m07-table .el-button.is-link:focus,
.m07-table .el-button.is-link:focus-visible,
.m07-table .el-button.is-link:active {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
  outline: none;
}

/* ─── Status Tags ─────────────────────────── */
.m07-table .el-tag {
  border-radius: var(--radius-sm, 4px);
  font-weight: 500;
}
</style>

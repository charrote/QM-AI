<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Checked, Search, Plus, Check, Refresh } from '@element-plus/icons-vue'
import { inspectionItemApi } from '@/api/inspectionItem'
import type { InspectionItem, InspectionItemDetail, CreateInspectionItem, UpdateInspectionItem } from '@/types/inspectionItem'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'InspectionItemsPage' })

const DATA_TYPE_OPTIONS = [
  { value: 'numeric', label: '数值型' },
  { value: 'visual', label: '外观型' },
  { value: 'attribute', label: '属性型' },
]

const CHART_TYPE_OPTIONS = [
  { value: '', label: '不使用控制图' },
  { value: 'Xbar_R', label: 'Xbar-R (均值-极差)' },
  { value: 'Xbar_S', label: 'Xbar-S (均值-标准差)' },
  { value: 'I_MR', label: 'I-MR (单值-移动极差)' },
  { value: 'P', label: 'p图 (不合格品率)' },
  { value: 'U', label: 'u图 (单位缺陷数)' },
  { value: 'C', label: 'c图 (缺陷数)' },
]

const loading = ref(false)
const items = ref<InspectionItem[]>([])
const total = ref(0)
const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref<number | null>(null)
const saving = ref(false)

const form = reactive<CreateInspectionItem>({
  itemCode: '',
  itemName: '',
  description: '',
  dataType: 'numeric',
  unit: '',
  usl: undefined,
  lsl: undefined,
  targetValue: undefined,
  ucl: undefined,
  lcl: undefined,
  dataCollectionParamCode: '',
  chartType: '',
  subgroupSize: undefined,
  inspectionMethod: '',
  sampleSize: undefined,
})

const formRules = {
  itemCode: [{ required: true, message: '请输入检验项目编码', trigger: 'blur' }],
  itemName: [{ required: true, message: '请输入检验项目名称', trigger: 'blur' }],
}

async function loadData() {
  loading.value = true
  try {
    const params: PagedRequest = { page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value || undefined }
    const res = await inspectionItemApi.list(params)
    items.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  page.value = 1
  loadData()
}

function handleReset() {
  searchKeyword.value = ''
  handleSearch()
}

function handlePageChange(p: number) {
  page.value = p
  loadData()
}

function handleSizeChange(s: number) {
  pageSize.value = s
  page.value = 1
  loadData()
}

function openCreate() {
  isEdit.value = true
  editingId.value = 0
  dialogTitle.value = '新增检验项目'
  Object.assign(form, {
    itemCode: '', itemName: '', description: '', dataType: 'numeric',
    unit: '', usl: undefined, lsl: undefined, targetValue: undefined,
    ucl: undefined, lcl: undefined, dataCollectionParamCode: '',
    chartType: '', subgroupSize: undefined, inspectionMethod: '', sampleSize: undefined,
  })
  dialogVisible.value = true
}

async function openEdit(id: number) {
  isEdit.value = true
  editingId.value = id
  dialogTitle.value = '编辑检验项目'
  try {
    const detail = await inspectionItemApi.get(id)
    form.itemCode = detail.itemCode
    form.itemName = detail.itemName
    form.description = detail.description ?? ''
    form.dataType = detail.dataType
    form.unit = detail.unit ?? ''
    form.usl = detail.usl ?? undefined
    form.lsl = detail.lsl ?? undefined
    form.targetValue = detail.targetValue ?? undefined
    form.ucl = detail.ucl ?? undefined
    form.lcl = detail.lcl ?? undefined
    form.dataCollectionParamCode = detail.dataCollectionParamCode ?? ''
    form.chartType = detail.chartType ?? ''
    form.subgroupSize = detail.subgroupSize ?? undefined
    form.inspectionMethod = detail.inspectionMethod ?? ''
    form.sampleSize = detail.sampleSize ?? undefined
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载检验项目详情失败')
  }
}

async function handleSave() {
  try {
    if (isEdit.value && editingId.value) {
      await inspectionItemApi.update(editingId.value, form as UpdateInspectionItem)
      ElMessage.success('更新成功')
    } else {
      await inspectionItemApi.create(form)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '操作失败')
  }
}

async function handleDelete(id: number, name: string) {
  try {
    await ElMessageBox.confirm(`确定删除检验项目「${name}」？`, '确认删除', {
      type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消',
    })
    await inspectionItemApi.delete(id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // cancelled
  }
}

function getDataTypeLabel(val: string) {
  const opt = DATA_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

function getChartTypeLabel(val?: string) {
  if (!val) return '-'
  const opt = CHART_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

onMounted(loadData)
</script>

<template>
  <div class="inspection-items-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="page-header__main">
        <el-icon class="page-header__icon"><Checked /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">检验项目管理</h2>
          <p class="page-header__subtitle">品质部统一管理的检验项目主数据，贯通IQC/IPQC/FQC/SPC</p>
        </div>
      </div>
    </div>

    <!-- 数据卡片 -->
    <div class="data-card">
      <div class="data-card__header">
        <div class="data-card__title">
          <el-icon><Search /></el-icon>
          <span>检验项目列表</span>
          <el-tag v-if="total" type="info" size="small" class="data-card__count">
            共 {{ total }} 条
          </el-tag>
        </div>
        <div class="data-card__toolbar">
          <div class="filter-bar">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索编码/名称..."
              clearable
              style="width: 220px"
              @keyup.enter="handleSearch"
              @clear="handleSearch"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </div>
          <el-button @click="handleReset" text>
            <el-icon><Refresh /></el-icon>重置
          </el-button>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>新增检验项目
          </el-button>
        </div>
      </div>

      <!-- 数据表格 -->
      <el-table
        :data="items"
        v-loading="loading"
        border
        style="width: 100%"
        size="small"
        empty-text="暂无数据"
        class="data-card__table"
        @row-click="row => openEdit(row.id)"
      >
        <el-table-column type="index" label="#" width="50" fixed class-name="index-cell" />
        <el-table-column prop="itemCode" label="项目编码" width="120" show-overflow-tooltip />
        <el-table-column prop="itemName" label="项目名称" min-width="150" show-overflow-tooltip />
        <el-table-column label="数据类型" width="100" show-overflow-tooltip>
          <template #default="{ row }"><el-tag size="small" effect="plain">{{ getDataTypeLabel(row.dataType) }}</el-tag></template>
        </el-table-column>
        <el-table-column prop="unit" label="单位" width="70" show-overflow-tooltip />
        <el-table-column label="规格范围" width="180" show-overflow-tooltip>
          <template #default="{ row }">
            <span v-if="row.lsl != null || row.usl != null" class="spec-range">{{ row.lsl ?? '-' }} ~ {{ row.usl ?? '-' }}</span>
            <span v-else class="text-muted">-</span>
          </template>
        </el-table-column>
        <el-table-column label="目标值" width="100" show-overflow-tooltip>
          <template #default="{ row }">{{ row.targetValue ?? '-' }}</template>
        </el-table-column>
        <el-table-column label="控制图" width="130" show-overflow-tooltip>
          <template #default="{ row }">{{ getChartTypeLabel(row.chartType) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="80" class-name="status-cell">
          <template #default="{ row }">
            {{ row.isActive ? '启用' : '停用' }}
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="170" show-overflow-tooltip>
          <template #default="{ row }">{{ row.createdAt }}</template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click.stop="openEdit(row.id)">编辑</el-button>
            <el-button size="small" type="danger" link @click.stop="handleDelete(row.id, row.itemName)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          :small="true"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
          class="data-card__pagination"
        />
      </div>
    </div>

    <!-- 新增/编辑 Drawer -->
    <el-drawer
      v-model="dialogVisible"
      :title="dialogTitle"
      size="600px"
      direction="rtl"
      :close-on-click-modal="true"
      destroy-on-close
      class="data-drawer"
    >
      <div class="dialog-body-wrap">
        <el-form
          :model="form"
          :rules="formRules"
          label-width="140px"
          label-position="top"
          size="default"
          class="dialog-form"
        >
          <!-- 基本信息 -->
          <div class="form-section">
            <div class="form-section__title">基本信息</div>
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="项目编码" prop="itemCode">
                  <el-input v-model="form.itemCode" :disabled="isEdit" placeholder="如 DIM-001" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="项目名称" prop="itemName">
                  <el-input v-model="form.itemName" :disabled="isEdit" placeholder="如 直径测量" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="数据类型">
                  <el-select v-model="form.dataType" style="width: 100%">
                    <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="单位">
                  <el-input v-model="form.unit" placeholder="如 mm" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="检验方法">
                  <el-input v-model="form.inspectionMethod" placeholder="如 游标卡尺" />
                </el-form-item>
              </el-col>
            </el-row>
          </div>

          <!-- 规格限 & 目标 -->
          <div class="form-section">
            <div class="form-section__title">规格限 & 目标</div>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="规格上限 (USL)">
                  <el-input-number v-model="form.usl" :min="undefined" :max="undefined" :precision="4" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="规格下限 (LSL)">
                  <el-input-number v-model="form.lsl" :min="undefined" :max="undefined" :precision="4" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="目标值 (Target)">
                  <el-input-number v-model="form.targetValue" :min="undefined" :max="undefined" :precision="4" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
            </el-row>
          </div>

          <!-- 管理限 (SPC控制图用) -->
          <div class="form-section">
            <div class="form-section__title">管理限 (SPC控制图用)</div>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="管理上限 (UCL)">
                  <el-input-number v-model="form.ucl" :min="undefined" :max="undefined" :precision="4" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="管理下限 (LCL)">
                  <el-input-number v-model="form.lcl" :min="undefined" :max="undefined" :precision="4" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="默认抽样数量">
                  <el-input-number v-model="form.sampleSize" :min="1" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
            </el-row>
          </div>

          <!-- 数采 & SPC配置 -->
          <div class="form-section">
            <div class="form-section__title">数采 & SPC配置</div>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="数采参数编码">
                  <el-input v-model="form.dataCollectionParamCode" placeholder="关联 dynamic_params.code" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="控制图类型">
                  <el-select v-model="form.chartType" style="width: 100%" clearable>
                    <el-option v-for="o in CHART_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="子组大小">
                  <el-input-number v-model="form.subgroupSize" :min="1" :max="20" controls-position="right" style="width: 100%" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-form-item label="描述">
              <el-input v-model="form.description" type="textarea" :rows="2" placeholder="请输入描述" />
            </el-form-item>
          </div>
        </el-form>
      </div>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSave" :loading="saving">
            <el-icon><Check /></el-icon>保存
          </el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<style scoped>
/* ─── Layout ──────────────────────────────── */
.inspection-items-page {
  display: flex;
  flex-direction: column;
  gap: var(--space-4, 16px);
}

/* ─── Page Header ─────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-4, 16px);
}

.page-header__main {
  display: flex;
  align-items: center;
  gap: var(--space-3, 12px);
}

.page-header__icon {
  font-size: 28px;
  color: var(--primary, #1677ff);
  background: var(--primary-bg, #f0f7ff);
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg, 8px);
  display: flex;
  align-items: center;
  justify-content: center;
}

.page-header__title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary, #1a1a1a);
  line-height: 1.3;
}

.page-header__subtitle {
  margin: 2px 0 0;
  font-size: 13px;
  color: var(--text-secondary, #8c8c8c);
}

/* ─── Data Card ───────────────────────────── */
.data-card {
  background: var(--bg-white, #fff);
  border-radius: var(--radius-lg, 8px);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.data-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-4, 16px) var(--space-5, 20px);
  border-bottom: 1px solid var(--border-color, #e4e7ed);
  background: var(--bg-white, #fff);
}

.data-card__title {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary, #1a1a1a);
}

.data-card__title .el-icon {
  color: var(--primary, #1677ff);
}

.data-card__count {
  font-weight: 400;
}

.data-card__toolbar {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
}

/* ─── Filter Bar ──────────────────────────── */
.filter-bar {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
  flex: 1;
}

/* ─── Table ───────────────────────────────── */
.data-card__table {
  border-radius: 0;
}

.data-card__table :deep(.el-table__row) {
  transition: background-color 0.15s ease;
}

.data-card__table :deep(.el-table__row:hover) {
  background-color: var(--primary-light, #e6f4ff);
}

.data-card__table :deep(.el-table th.el-table__cell) {
  background: var(--bg-gray, #fafafa) !important;
  color: var(--text-primary, #1a1a1a);
  font-weight: 600;
  font-size: 12px;
}

/* ─── Table Index Column ──────────────────── */
.data-card__table :deep(.el-table__row > .el-table__cell.index-cell),
.data-card__table :deep(.index-cell) {
  white-space: nowrap !important;
}

.data-card__table :deep(.index-cell .el-table__cell) {
  white-space: nowrap !important;
}

.data-card__table :deep(.index-cell .cell) {
  white-space: nowrap !important;
  display: inline-block;
  min-width: 100%;
}

/* ─── Table Action Buttons ────────────────── */
.data-card__table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.data-card__table :deep(.el-button--primary.is-link:hover),
.data-card__table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.data-card__table :deep(.el-button--primary.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}



/* ─── Pagination ──────────────────────────── */
.data-card__footer {
  display: flex;
  justify-content: flex-end;
  padding: var(--space-3, 12px) var(--space-5, 20px);
  border-top: 1px solid var(--border-color, #e4e7ed);
  background: var(--bg-white, #fff);
}

.data-card__pagination {
  display: flex;
  align-items: center;
}

/* ─── Text Utilities ──────────────────────── */
.text-muted {
  color: var(--text-secondary, #8c8c8c);
}

.spec-range {
  font-family: monospace;
  font-size: 12px;
  color: var(--primary, #1677ff);
}

/* ─── Dialog / Drawer ─────────────────────── */
.dialog-body-wrap {
  display: flex;
  flex-direction: column;
  flex: 1;
  overflow-y: auto;
  padding-right: 8px;
}

.dialog-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-2, 8px);
}

.dialog-form :deep(.el-form-item) {
  margin-bottom: 0;
}

.dialog-form :deep(.el-form-item__label) {
  font-weight: 500;
  color: var(--text-primary, #303133);
  font-size: 13px;
}

/* Form sections */
.form-section {
  padding: var(--space-3, 12px) 0;
  border-bottom: 1px dashed var(--border-color-light, #ebeef5);
}

.form-section:last-child {
  border-bottom: none;
}

.form-section__title {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
  font-size: 13px;
  font-weight: 600;
  color: var(--primary, #1677ff);
  margin-bottom: var(--space-2, 8px);
  padding-bottom: var(--space-1, 4px);
  border-bottom: 1px solid var(--primary-light-5, #8bc5ff);
}

.form-section__title::before {
  content: '';
  width: 3px;
  height: 14px;
  background: var(--primary, #1677ff);
  border-radius: 2px;
  flex-shrink: 0;
}

/* Drawer styles */
.data-drawer :deep(.el-drawer) {
  border-radius: var(--radius-lg, 8px) 0 0 var(--radius-lg, 8px);
  overflow: hidden;
}

.data-drawer :deep(.el-drawer__header) {
  padding: 18px 24px;
  border-bottom: 1px solid var(--border-color, #e4e7ed);
  margin: 0;
  background: var(--bg-white, #fff);
}

.data-drawer :deep(.el-drawer__close-btn) {
  top: 18px;
  right: 24px;
}

.data-drawer :deep(.el-drawer__title) {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary, #1a1a1a);
}

.data-drawer :deep(.el-drawer__body) {
  padding: 0;
  background: var(--bg-white, #fff);
}

/* ─── Dialog Footer Buttons ──────────────── */
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3, 12px);
}

/* ─── Responsive ──────────────────────────── */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .data-card__header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--space-3, 12px);
  }
}
</style>

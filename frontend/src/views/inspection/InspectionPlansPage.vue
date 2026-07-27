<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { List, Search, Plus, Check, Refresh, Close } from '@element-plus/icons-vue'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import { inspectionItemApi } from '@/api/inspectionItem'
import type { InspectionPlan, InspectionPlanDetail, InspectionPlanItem, CreateInspectionPlan, CreateInspectionPlanItem, UpdateInspectionPlan } from '@/types/inspectionItem'
import type { InspectionItem } from '@/types/inspectionItem'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'InspectionPlansPage' })

const INSPECTION_TYPE_OPTIONS = [
  { value: 'IQC', label: 'IQC来料检验' },
  { value: 'IPQC', label: 'IPQC过程检验' },
  { value: 'FQC', label: 'FQC成品检验' },
  { value: 'OQC', label: 'OQC出货检验' },
]

const loading = ref(false)
const items = ref<InspectionPlan[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref<number | null>(null)
const saving = ref(false)
const formRef = ref()

const inspectionItemOptions = ref<InspectionItem[]>([])

// ─── Form ──────────────────────────────────────────────────────

const form = reactive({
  planCode: '',
  planName: '',
  inspectionType: 'IQC' as string,
  description: '',
  productId: undefined as number | undefined,
  materialId: undefined as number | undefined,
  supplierId: undefined as number | undefined,
  customerId: undefined as number | undefined,
  processId: undefined as number | undefined,
  equipmentId: undefined as number | undefined,
})

const planItems = ref<CreateInspectionPlanItem[]>([])

const formRules = {
  planCode: [{ required: true, message: '请输入计划编码', trigger: 'blur' }],
  planName: [{ required: true, message: '请输入计划名称', trigger: 'blur' }],
  inspectionType: [{ required: true, message: '请选择检验类型', trigger: 'change' }],
}

// ─── Methods ───────────────────────────────────────────────────

async function loadInspectionItems() {
  try {
    inspectionItemOptions.value = await inspectionItemApi.getSelectList()
  } catch {
    // ignore
  }
}

async function loadData() {
  loading.value = true
  try {
    const res = await inspectionPlanApi.list(query)
    items.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  query.page = 1
  loadData()
}

function handleReset() {
  query.keyword = ''
  handleSearch()
}

function handlePageChange(page: number) {
  query.page = page
  loadData()
}

function openCreate() {
  dialogTitle.value = '新增检验计划'
  isEdit.value = false
  editingId.value = null
  form.planCode = ''
  form.planName = ''
  form.inspectionType = 'IQC'
  form.description = ''
  form.productId = undefined
  form.materialId = undefined
  form.supplierId = undefined
  form.customerId = undefined
  form.processId = undefined
  form.equipmentId = undefined
  planItems.value = []
  loadInspectionItems()
  dialogVisible.value = true
}

async function openEdit(id: number) {
  dialogTitle.value = '编辑检验计划'
  isEdit.value = true
  editingId.value = id
  try {
    const detail = await inspectionPlanApi.get(id)
    form.planCode = detail.planCode
    form.planName = detail.planName
    form.inspectionType = detail.inspectionType
    form.description = detail.description ?? ''
    form.productId = detail.productId ?? undefined
    form.materialId = detail.materialId ?? undefined
    form.supplierId = detail.supplierId ?? undefined
    form.customerId = detail.customerId ?? undefined
    form.processId = detail.processId ?? undefined
    form.equipmentId = detail.equipmentId ?? undefined
    planItems.value = detail.items.map(i => ({
      inspectionItemId: i.inspectionItemId,
      sortOrder: i.sortOrder,
      usl: i.usl,
      lsl: i.lsl,
      targetValue: i.targetValue,
      ucl: i.ucl,
      lcl: i.lcl,
      sampleSize: i.sampleSize,
      isRequired: i.isRequired,
    }))
    await loadInspectionItems()
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载检验计划详情失败')
  }
}

function addPlanItem() {
  planItems.value.push({
    inspectionItemId: 0,
    sortOrder: planItems.value.length + 1,
    usl: undefined,
    lsl: undefined,
    targetValue: undefined,
    ucl: undefined,
    lcl: undefined,
    sampleSize: undefined,
    isRequired: true,
  })
}

function removePlanItem(index: number) {
  planItems.value.splice(index, 1)
}

function onItemSelectChange(index: number) {
  const selectedId = planItems.value[index].inspectionItemId
  const item = inspectionItemOptions.value.find(opt => opt.id === selectedId)
  if (item) {
    planItems.value[index].usl = planItems.value[index].usl ?? item.usl
    planItems.value[index].lsl = planItems.value[index].lsl ?? item.lsl
    planItems.value[index].targetValue = planItems.value[index].targetValue ?? item.targetValue
  }
}

function getItemLabel(id: number): string {
  const item = inspectionItemOptions.value.find(opt => opt.id === id)
  return item ? `${item.itemCode} - ${item.itemName}` : ''
}

async function handleSave() {
  try {
    const valid = await formRef.value.validate()
    if (!valid) return

    const selectedItems = planItems.value.filter(pi => pi.inspectionItemId && pi.inspectionItemId > 0)
    if (selectedItems.length === 0) {
      ElMessage.warning('请至少选择一个检验项目')
      return
    }

    saving.value = true
    try {
      if (isEdit.value && editingId.value) {
        const updateDto: UpdateInspectionPlan = {
          planName: form.planName,
          description: form.description || undefined,
          productId: form.productId,
          materialId: form.materialId,
          supplierId: form.supplierId,
          customerId: form.customerId,
          processId: form.processId,
          equipmentId: form.equipmentId,
          isActive: true,
          items: selectedItems.map((pi, idx) => ({ ...pi, sortOrder: idx + 1 })),
        }
        await inspectionPlanApi.update(editingId.value, updateDto)
        ElMessage.success('更新成功')
      } else {
        const createDto: CreateInspectionPlan = {
          planCode: form.planCode,
          planName: form.planName,
          inspectionType: form.inspectionType,
          description: form.description || undefined,
          productId: form.productId,
          materialId: form.materialId,
          supplierId: form.supplierId,
          customerId: form.customerId,
          processId: form.processId,
          equipmentId: form.equipmentId,
          items: selectedItems.map((pi, idx) => ({ ...pi, sortOrder: idx + 1 })),
        }
        await inspectionPlanApi.create(createDto)
        ElMessage.success('创建成功')
      }
      dialogVisible.value = false
      loadData()
    } finally {
      saving.value = false
    }
  } catch (err: any) {
    if (err !== false) {
      ElMessage.error(err?.response?.data?.message || '操作失败')
      saving.value = false
    }
  }
}

async function handleDelete(id: number, name: string) {
  try {
    await ElMessageBox.confirm(`确定删除检验计划「${name}」？`, '确认删除', {
      type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消',
    })
    await inspectionPlanApi.delete(id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // cancelled
  }
}

function getTypeLabel(val: string) {
  const opt = INSPECTION_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="inspection-plans-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="page-header__main">
        <el-icon class="page-header__icon"><List /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">检验计划管理</h2>
          <p class="page-header__subtitle">定义"什么维度组合→检验哪些项目"，贯通检验项目主数据与业务执行</p>
        </div>
      </div>
    </div>

    <!-- 数据卡片 -->
    <div class="data-card">
      <div class="data-card__header">
        <div class="data-card__title">
          <el-icon><Search /></el-icon>
          <span>检验计划列表</span>
          <el-tag v-if="total" type="info" size="small" class="data-card__count">
            共 {{ total }} 条
          </el-tag>
        </div>
        <div class="data-card__toolbar">
          <div class="filter-bar">
            <el-input
              v-model="query.keyword"
              placeholder="搜索计划编码/名称..."
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
            <el-icon><Plus /></el-icon>新增检验计划
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
        <el-table-column prop="planCode" label="计划编码" width="130" show-overflow-tooltip />
        <el-table-column prop="planName" label="计划名称" min-width="150" show-overflow-tooltip />
        <el-table-column label="检验类型" width="120" show-overflow-tooltip>
          <template #default="{ row }"><el-tag size="small" effect="plain">{{ getTypeLabel(row.inspectionType) }}</el-tag></template>
        </el-table-column>
        <el-table-column prop="productName" label="产品" min-width="120" show-overflow-tooltip>
          <template #default="{ row }">{{ row.productName || '-' }}</template>
        </el-table-column>
        <el-table-column prop="supplierName" label="供应商" min-width="120" show-overflow-tooltip>
          <template #default="{ row }">{{ row.supplierName || '-' }}</template>
        </el-table-column>
        <el-table-column prop="processName" label="工序" min-width="120" show-overflow-tooltip>
          <template #default="{ row }">{{ row.processName || '-' }}</template>
        </el-table-column>
        <el-table-column label="检验项目数" width="100" show-overflow-tooltip>
          <template #default="{ row }">{{ row.itemCount }}</template>
        </el-table-column>
        <el-table-column label="状态" width="80" class-name="status-cell">
          <template #default="{ row }">
            {{ row.isActive ? '启用' : '停用' }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click.stop="openEdit(row.id)">编辑</el-button>
            <el-button size="small" type="danger" link @click.stop="handleDelete(row.id, row.planName)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="query.page"
          v-model:page-size="query.pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          :small="true"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadData"
          @current-change="handlePageChange"
          class="data-card__pagination"
        />
      </div>
    </div>

    <!-- 新增/编辑 Drawer -->
    <el-drawer
      v-model="dialogVisible"
      :title="dialogTitle"
      size="800px"
      direction="rtl"
      :close-on-click-modal="true"
      destroy-on-close
      class="data-drawer"
    >
      <div class="dialog-body-wrap">
        <el-form
          ref="formRef"
          :model="form"
          :rules="formRules"
          label-width="120px"
          label-position="top"
          size="default"
          class="dialog-form"
        >
          <!-- 基本信息 -->
          <div class="form-section">
            <div class="form-section__title">基本信息</div>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="计划编码" prop="planCode">
                  <el-input v-model="form.planCode" placeholder="如 PLAN-IQC-001" :disabled="isEdit" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="计划名称" prop="planName">
                  <el-input v-model="form.planName" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="检验类型" prop="inspectionType">
                  <el-select v-model="form.inspectionType" style="width: 100%">
                    <el-option v-for="o in INSPECTION_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>
          </div>

          <!-- 业务维度 -->
          <div class="form-section">
            <div class="form-section__title">业务维度（设置后系统自动匹配）</div>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="产品">
                  <el-input v-model="form.productId" placeholder="产品ID" type="number" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="供应商">
                  <el-input v-model="form.supplierId" placeholder="供应商ID" type="number" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="客户">
                  <el-input v-model="form.customerId" placeholder="客户ID" type="number" />
                </el-form-item>
              </el-col>
            </el-row>
            <el-row :gutter="16">
              <el-col :span="8">
                <el-form-item label="工序">
                  <el-input v-model="form.processId" placeholder="工序ID" type="number" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="设备">
                  <el-input v-model="form.equipmentId" placeholder="设备ID" type="number" />
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="材料">
                  <el-input v-model="form.materialId" placeholder="材料ID" type="number" />
                </el-form-item>
              </el-col>
            </el-row>
          </div>

          <!-- 检验项目清单 -->
          <div class="form-section">
            <div class="form-section__title">检验项目清单</div>
            <div class="plan-items-section">
              <div v-for="(pi, idx) in planItems" :key="idx" class="plan-item-row">
                <div class="plan-item-row__body">
                  <el-form-item :label="`项目 ${idx + 1}`" :prop="`items.${idx}.inspectionItemId`"
                    :rules="[{ required: true, message: '请选择检验项目', trigger: 'change' }]">
                    <el-select v-model="pi.inspectionItemId" filterable style="width: 180px"
                      @change="onItemSelectChange(idx)">
                      <el-option v-for="opt in inspectionItemOptions" :key="opt.id"
                        :label="`${opt.itemCode} - ${opt.itemName}`" :value="opt.id" />
                    </el-select>
                  </el-form-item>
                  <el-form-item label="USL">
                    <el-input-number v-model="pi.usl" :precision="4" controls-position="right" style="width: 100px" />
                  </el-form-item>
                  <el-form-item label="LSL">
                    <el-input-number v-model="pi.lsl" :precision="4" controls-position="right" style="width: 100px" />
                  </el-form-item>
                  <el-form-item label="Target">
                    <el-input-number v-model="pi.targetValue" :precision="4" controls-position="right" style="width: 100px" />
                  </el-form-item>
                  <el-form-item label="样本数">
                    <el-input-number v-model="pi.sampleSize" :min="1" controls-position="right" style="width: 80px" />
                  </el-form-item>
                  <el-form-item label="必检">
                    <el-switch v-model="pi.isRequired" />
                  </el-form-item>
                  <span class="plan-item-remove">
                    <el-button type="danger" plain size="small" @click="removePlanItem(idx)">
                      <el-icon><Close /></el-icon>删除
                    </el-button>
                  </span>
                </div>
              </div>

              <el-button type="primary" plain @click="addPlanItem" class="mt-2">
                <el-icon><Plus /></el-icon>添加检验项目
              </el-button>
            </div>
          </div>

          <!-- 描述 -->
          <div class="form-section">
            <div class="form-section__title">其他信息</div>
            <el-form-item label="描述">
              <el-input v-model="form.description" type="textarea" :rows="2" />
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
.inspection-plans-page {
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

/* Plan items */
.plan-items-section {
  padding: 8px 0;
}

.plan-item-row {
  padding: 12px;
  margin-bottom: 8px;
  background: #fafafa;
  border: 1px solid #e4e7ed;
  border-radius: 6px;
}

/* ─── Input Number ─── */
.plan-item-row :deep(.el-input-number) {
  width: 100%;
}

/* ─── Plan Item Row Body ─── */
.plan-item-row__body {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  flex-wrap: nowrap;
}

.plan-item-row__body :deep(.el-form-item) {
  margin-bottom: 0;
  margin-top: 0;
}

.plan-item-row__body :deep(.el-form-item__label) {
  font-size: 12px;
  white-space: nowrap;
}

.plan-item-row__body .plan-item-remove {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  padding-top: 18px;
}

.plan-item-remove {
  text-align: right;
}

.mt-2 {
  margin-top: 8px;
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

<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Check, Plus, Refresh, Search, DocumentAdd, Delete } from '@element-plus/icons-vue'
import { firstPieceApi } from '@/api/ipqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import { processApi, equipmentApi } from '@/api/basicData'
import type { IpqcFirstPiece, IpqcFirstPieceDetail, CreateIpqcFirstPiece, CreateIpqcFirstPieceItem, SubmitIpqcFirstPiece, IpqcFirstPieceItem } from '@/types/ipqc'
import {
  IPQC_FIRST_PIECE_CONCLUSION_OPTIONS,
  IPQC_FIRST_PIECE_REASON_OPTIONS,
  IPQC_SHIFT_OPTIONS,
  INSPECTION_RESULT_OPTIONS,
  DATA_TYPE_OPTIONS,
} from '@/types/ipqc'
import type { PagedRequest } from '@/types/basicData'
import type { Process, Equipment } from '@/types/basicData'

defineOptions({ name: 'IpqcFirstPiecesPage' })

// ─── State ──────────────────────────────────────
const searchKeyword = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const items = ref<IpqcFirstPiece[]>([])
const loading = ref(false)

const dialogVisible = ref(false)
const dialogTitle = ref('新建首件检验')
const isSubmit = ref(false)
const processes = ref<Process[]>([])
const equipmentList = ref<Equipment[]>([])

const form = reactive<CreateIpqcFirstPiece>({
  workOrderId: 0,
  processId: 0,
  equipmentId: 0,
  operatorId: 0,
  shift: '早班',
  reason: '班次切换',
  items: [],
})

const submitForm = reactive<SubmitIpqcFirstPiece>({
  conclusion: 'pending',
  allowedToProduce: false,
  items: [],
})

const submitId = ref<number>(0)

const loadingPlanItems = ref(false)

// ─── Auto-load plans ──────────────────────────
watch([() => form.processId, () => form.equipmentId], async ([processId, equipmentId]) => {
  if (isSubmit.value) return        // only auto-load in create mode
  if (!processId || processId <= 0) return

  loadingPlanItems.value = true
  try {
    const plans = await inspectionPlanApi.getByContext({
      inspectionType: 'IPQC_FIRST_PIECE',
      processId,
      equipmentId: equipmentId > 0 ? equipmentId : undefined,
    })
    if (plans.length > 0) {
      const allItems = plans.flatMap(p => p.items)
      const seen = new Set<number>()
      const newItems: CreateIpqcFirstPieceItem[] = []
      for (const item of allItems) {
        if (!seen.has(item.inspectionItemId)) {
          seen.add(item.inspectionItemId)
          newItems.push({
            inspectionItemId: item.inspectionItemId,
            itemName: item.inspectionItemName,
            dataType: item.dataType,
            usl: item.usl ?? undefined,
            lsl: item.lsl ?? undefined,
            result: 'pending',
          })
        }
      }
      if (newItems.length > 0) {
        form.items = newItems
      }
    }
  } catch (e) {
    console.error('加载检验计划失败', e)
  } finally {
    loadingPlanItems.value = false
  }
})

// ─── Helpers ────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function conclusionTag(c: string) {
  const opt = IPQC_FIRST_PIECE_CONCLUSION_OPTIONS.find(o => o.value === c)
  return opt?.type || 'info'
}

function conclusionLabel(c: string) {
  const opt = IPQC_FIRST_PIECE_CONCLUSION_OPTIONS.find(o => o.value === c)
  return opt?.label || c
}

// ─── Load Data ─────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const params: PagedRequest = {
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
      status: statusFilter.value || undefined,
    }
    const res = await firstPieceApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch (e: any) {
    ElMessage.error('加载首件检验列表失败')
  } finally {
    loading.value = false
  }
}

async function loadProcesses() {
  try {
    const res = await processApi.list({ pageSize: 100 })
    processes.value = res.items
  } catch { /* ignore */ }
}

async function loadEquipment() {
  try {
    const res = await equipmentApi.list({ pageSize: 100 })
    equipmentList.value = res.items
  } catch { /* ignore */ }
}

// ─── CRUD ──────────────────────────────────────
function openCreate() {
  dialogTitle.value = '新建首件检验'
  isSubmit.value = false
  form.workOrderId = 0
  form.processId = 0
  form.equipmentId = 0
  form.operatorId = 0
  form.shift = '早班'
  form.reason = '班次切换'
  form.items = [{ itemName: '', dataType: 'numeric', result: 'pending' }]
  dialogVisible.value = true
}

function addItem() {
  form.items.push({ itemName: '', dataType: 'numeric', result: 'pending' })
}

function removeItem(index: number) {
  form.items.splice(index, 1)
}

async function handleSave() {
  if (!form.workOrderId || !form.processId || !form.equipmentId) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    await firstPieceApi.create(form)
    ElMessage.success('首件检验已创建')
    dialogVisible.value = false
    await loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该首件检验？', '提示', { type: 'warning' })
    await firstPieceApi.delete(id)
    ElMessage.success('已删除')
    await loadData()
  } catch { /* cancelled */ }
}

async function openSubmit(id: number) {
  const detail = await firstPieceApi.get(id)
  if (!detail) return

  isSubmit.value = true
  dialogTitle.value = `提交首件检验 - ${detail.fpNo}`
  submitForm.conclusion = 'pending'
  submitForm.allowedToProduce = false
  submitForm.items = (detail.items || []).map(i => ({ ...i }))
  submitForm.inspector = detail.inspector || undefined

  submitId.value = id
  dialogVisible.value = true
}

async function handleSubmit() {
  const id = submitId.value
  if (!id) return
  try {
    await firstPieceApi.submit(id, {
      conclusion: submitForm.conclusion,
      allowedToProduce: submitForm.allowedToProduce,
      inspector: submitForm.inspector,
      items: submitForm.items,
    })
    ElMessage.success('首件检验已提交')
    dialogVisible.value = false
    await loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  }
}

onMounted(async () => {
  await Promise.all([loadData(), loadProcesses(), loadEquipment()])
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-left">
        <el-icon class="page-header-icon"><Check /></el-icon>
        <div class="page-header-text">
          <h2 class="page-title">首件检验</h2>
          <p class="page-subtitle">管理产线首件检验流程与结论判定</p>
        </div>
      </div>
      <div class="page-header-actions">
        <el-button type="primary" :icon="Plus" @click="openCreate" round>新建首件检验</el-button>
        <el-button :icon="Refresh" @click="loadData" :loading="loading">刷新</el-button>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="action-bar">
      <div class="action-bar-left">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索首件编号..."
          clearable
          class="search-input"
          :prefix-icon="Search"
          @clear="loadData"
          @keyup.enter="loadData"
          style="width: 260px"
        />
        <el-select
          v-model="statusFilter"
          placeholder="筛选结论"
          clearable
          class="filter-select"
          @change="loadData"
        >
          <el-option v-for="o in IPQC_FIRST_PIECE_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
        </el-select>
      </div>
    </div>

    <!-- Table -->
    <div class="data-card">
      <el-table
        :data="items"
        stripe
        v-loading="loading"
        class="styled-table"
        @row-click="(row: IpqcFirstPiece) => {}"
      >
        <el-table-column prop="fpNo" label="首件编号" width="180" fixed />
        <el-table-column prop="processName" label="工序" width="140" />
        <el-table-column prop="equipmentName" label="设备" width="140" />
        <el-table-column prop="shift" label="班次" width="80" align="center" />
        <el-table-column prop="reason" label="原因" width="120" />
        <el-table-column label="结论" width="120" align="center">
          <template #default="{ row }">
            <el-tag
              :type="conclusionTag(row.conclusion)"
              effect="dark"
              size="default"
              round
              class="conclusion-tag"
            >
              {{ conclusionLabel(row.conclusion) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="允许量产" width="100" align="center">
          <template #default="{ row }">
            <el-tag v-if="row.allowedToProduce" type="success" size="default" round effect="dark">
              <el-icon><Check /></el-icon> 允许
            </el-tag>
            <el-tag v-else type="info" size="default" round>未允许</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="checkedAt" label="检验时间" width="170" align="center">
          <template #default="{ row }">{{ formatDate(row.checkedAt) }}</template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="170" align="center">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right" align="center">
          <template #default="{ row }">
            <el-button
              link
              size="small"
              type="primary"
              :icon="Plus"
              @click="openSubmit(row.id)"
              v-if="row.conclusion === 'pending'"
            >提交</el-button>
            <el-button link size="small" type="primary" :icon="DocumentAdd">详情</el-button>
            <el-popconfirm title="确认删除该首件检验？" @confirm="handleDelete(row.id)">
              <template #reference>
                <el-button link size="small" type="danger" :icon="Delete">删除</el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- Pagination -->
    <div class="pagination-row">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next, jumper"
        :page-sizes="[10, 20, 50, 100]"
        background
        @current-change="loadData"
      />
    </div>

    <!-- Create Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="680px"
      :close-on-click-modal="false"
      class="styled-dialog"
      destroy-on-close
    >
      <template v-if="!isSubmit">
        <div class="dialog-section">
          <h3 class="section-title-text"><el-icon><DocumentAdd /></el-icon> 基本信息</h3>
          <el-form :model="form" label-width="96px" label-position="left" size="default">
            <el-form-item label="工单ID" required>
              <el-input-number v-model="form.workOrderId" :min="1" :controls="false" style="width:200px" />
            </el-form-item>
            <el-form-item label="工序" required>
              <el-select v-model="form.processId" filterable placeholder="请选择工序" style="width:200px"
                :loading="processes.length === 0">
                <el-option v-for="p in processes" :key="p.id" :label="p.name" :value="p.id" />
              </el-select>
            </el-form-item>
            <el-form-item label="设备" required>
              <el-select v-model="form.equipmentId" filterable placeholder="请选择设备" style="width:200px"
                :loading="equipmentList.length === 0">
                <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
              </el-select>
            </el-form-item>
            <el-form-item label="操作员ID">
              <el-input-number v-model="form.operatorId" :min="0" :controls="false" style="width:200px" />
            </el-form-item>
            <el-form-item label="班次">
              <el-select v-model="form.shift" style="width:200px">
                <el-option v-for="o in IPQC_SHIFT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="原因">
              <el-select v-model="form.reason" style="width:200px">
                <el-option v-for="o in IPQC_FIRST_PIECE_REASON_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
          </el-form>
          <el-divider />
        </div>

        <div class="dialog-section">
          <h3 class="section-title-text"><el-icon><Plus /></el-icon> 检验项目</h3>
          <div v-if="loadingPlanItems" class="plan-loading">⏳ 正在根据工序加载检验计划...</div>
          <div class="items-list">
            <div v-for="(item, idx) in form.items" :key="idx" class="item-row">
              <div class="item-fields">
                <el-input v-model="item.itemName" placeholder="项目名称" :readonly="!!item.inspectionItemId" />
                <el-select v-model="item.dataType" :disabled="!!item.inspectionItemId">
                  <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                </el-select>
                <el-input-number v-model="item.usl" placeholder="USL" :precision="4" :step="0.1" controls-position="right" :disabled="!!item.inspectionItemId" />
                <el-input-number v-model="item.lsl" placeholder="LSL" :precision="4" :step="0.1" controls-position="right" :disabled="!!item.inspectionItemId" />
                <el-tag v-if="item.inspectionItemId" size="small" type="success" round effect="dark">计划</el-tag>
              </div>
              <el-button
                link
                type="danger"
                :icon="Delete"
                class="remove-btn"
                @click="removeItem(idx)"
                :disabled="form.items.length <= 1"
              />
            </div>
            <el-button size="small" @click="addItem" round>
              <el-icon><Plus /></el-icon> 添加项目
            </el-button>
          </div>
        </div>
      </template>

      <template v-else>
        <el-form :model="submitForm" label-width="96px" label-position="left" size="default">
          <div class="dialog-section">
            <h3 class="section-title-text"><el-icon><Check /></el-icon> 结论判定</h3>
            <el-form-item label="检验结论" required>
              <el-select v-model="submitForm.conclusion" style="width:240px">
                <el-option v-for="o in IPQC_FIRST_PIECE_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="允许量产">
              <el-switch v-model="submitForm.allowedToProduce" active-text="允许" inactive-text="不允许" />
            </el-form-item>
            <el-form-item label="检验员">
              <el-input v-model="submitForm.inspector" style="width:200px" placeholder="检验员姓名" />
            </el-form-item>
          </div>
          <el-divider />
          <div class="dialog-section">
            <h3 class="section-title-text"><el-icon><DocumentAdd /></el-icon> 检验项结果</h3>
            <div class="items-list">
              <div v-for="(item, idx) in submitForm.items" :key="idx" class="item-row">
                <div class="item-fields">
                  <span class="item-name">{{ item.itemName }}</span>
                  <el-input-number v-model="item.actualValue" :precision="4" :step="0.1" controls-position="right" />
                  <el-select v-model="item.result">
                    <el-option v-for="o in INSPECTION_RESULT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                  </el-select>
                  <span v-if="item.usl != null" class="range-info">({{ item.lsl }} ~ {{ item.usl }})</span>
                </div>
              </div>
            </div>
          </div>
        </el-form>
      </template>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">取 消</el-button>
          <el-button type="primary" @click="isSubmit ? handleSubmit() : handleSave()">
            {{ isSubmit ? '提 交' : '保 存' }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--el-bg-color, #f5f7fa);
}

/* ─── Page Header ────────────────────────────── */

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: #fff;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  flex-shrink: 0;
}

.page-header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-header-icon {
  font-size: 24px;
  color: var(--el-color-primary, #409eff);
}

.page-header-text {
  display: flex;
  flex-direction: column;
}

.page-title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
  line-height: 1.3;
}

.page-subtitle {
  margin: 0;
  font-size: 12px;
  color: var(--el-text-color-secondary, #909399);
  line-height: 1.2;
}

.page-header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* ─── Action Bar ─────────────────────────────── */

.action-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 20px;
  background: #fff;
  margin: 12px 0 0;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  flex-shrink: 0;
}

.action-bar-left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.search-input :deep(.el-input__wrapper) {
  border-radius: 6px;
}

.filter-select :deep(.el-input__wrapper) {
  border-radius: 6px;
}

/* ─── Data Card ──────────────────────────────── */

.data-card {
  flex: 1;
  overflow: auto;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  margin-bottom: 12px;
}

.styled-table {
  width: 100%;
}

.styled-table :deep(.el-table__header-wrapper th) {
  background: #f5f7fa !important;
  font-weight: 600;
  font-size: 13px;
  color: var(--el-text-color-regular, #606266);
}

.styled-table :deep(.el-table__row) {
  transition: background-color 0.2s;
}

.styled-table :deep(.el-table__row:hover) {
  background-color: #ecf5ff !important;
}

.conclusion-tag {
  font-weight: 500;
}

/* ─── Pagination ─────────────────────────────── */

.pagination-row {
  display: flex;
  justify-content: flex-end;
  padding: 12px 20px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  flex-shrink: 0;
}

/* ─── Dialog ─────────────────────────────────── */

.dialog-section {
  margin-bottom: 8px;
}

.section-title-text {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin: 0 0 16px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
}

.section-title-text .el-icon {
  color: var(--el-color-primary, #409eff);
}

.plan-loading {
  color: var(--el-text-color-secondary, #909399);
  padding: 8px 0;
  font-size: 13px;
}

.items-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.item-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: var(--el-fill-color-lighter, #f2f3f5);
  border-radius: 6px;
  transition: background-color 0.2s;
}

.item-row:hover {
  background: var(--el-fill-color-light, #f0f2f5);
}

.item-fields {
  display: flex;
  align-items: center;
  gap: 8px;
  flex: 1;
  flex-wrap: wrap;
}

.item-fields :deep(.el-input),
.item-fields :deep(.el-select),
.item-fields :deep(.el-input-number) {
  min-width: 0;
  flex: 0 0 auto;
}

.item-name {
  min-width: 120px;
  font-size: 13px;
  font-weight: 500;
  color: var(--el-text-color-regular, #606266);
  flex-shrink: 0;
}

.range-info {
  color: var(--el-text-color-placeholder, #c0c4cc);
  font-size: 11px;
  flex-shrink: 0;
}

.remove-btn {
  flex-shrink: 0;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.styled-dialog :deep(.el-dialog__header) {
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  padding: 16px 20px;
  margin-right: 0;
}

.styled-dialog :deep(.el-dialog__body) {
  padding: 20px;
}

.styled-dialog :deep(.el-dialog__footer) {
  border-top: 1px solid var(--el-border-color-lighter, #ebeef5);
  padding: 12px 20px;
}
</style>

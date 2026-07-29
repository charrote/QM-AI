<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Check, Plus, Refresh, Search, DocumentAdd, Delete } from '@element-plus/icons-vue'
import { firstPieceApi } from '@/api/ipqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import { processApi, equipmentApi } from '@/api/basicData'
import type { IpqcFirstPiece, CreateIpqcFirstPiece, CreateIpqcFirstPieceItem, SubmitIpqcFirstPiece } from '@/types/ipqc'
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

// Stats
const stats = ref({ total: 0, qualified: 0, unqualified: 0, pending: 0 })

const drawerVisible = ref(false)
const drawerTitle = ref('新建首件检验')
const isSubmit = ref(false)
const isEditing = ref(false)
const editingId = ref<number | null>(null)
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
const saving = ref(false)

// ─── Auto-load plans ──────────────────────────
watch([() => form.processId, () => form.equipmentId], async ([processId, equipmentId]) => {
  if (isSubmit.value) return
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
async function loadStats() {
  try {
    const [allRes, qualRes, unqualRes, pendRes] = await Promise.allSettled([
      firstPieceApi.list({ page: 1, pageSize: 1, keyword: undefined, status: undefined }),
      firstPieceApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'qualified' }),
      firstPieceApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'unqualified' }),
      firstPieceApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'pending' }),
    ])
    stats.value = {
      total: allRes.status === 'fulfilled' ? allRes.value.total : 0,
      qualified: qualRes.status === 'fulfilled' ? qualRes.value.total : 0,
      unqualified: unqualRes.status === 'fulfilled' ? unqualRes.value.total : 0,
      pending: pendRes.status === 'fulfilled' ? pendRes.value.total : 0,
    }
  } catch { /* ignore */ }
}

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

// ─── Drawer CRUD ──────────────────────────────
function openCreate() {
  isEditing.value = false
  isSubmit.value = false
  editingId.value = null
  drawerTitle.value = '新建首件检验'
  form.workOrderId = 0
  form.processId = 0
  form.equipmentId = 0
  form.operatorId = 0
  form.shift = '早班'
  form.reason = '班次切换'
  form.items = [{ itemName: '', dataType: 'numeric', result: 'pending' }]
  drawerVisible.value = true
}

function openSubmit(id: number) {
  isEditing.value = false
  isSubmit.value = true
  editingId.value = id
  drawerTitle.value = `提交首件检验`
  submitForm.conclusion = 'pending'
  submitForm.allowedToProduce = false
  submitForm.items = []
  submitForm.inspector = undefined

  firstPieceApi.get(id).then(detail => {
    if (detail) {
      submitForm.items = (detail.items || []).map(i => ({ ...i }))
      submitForm.inspector = detail.inspector || undefined
    }
  }).catch(() => {})

  submitId.value = id
  drawerVisible.value = true
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
  saving.value = true
  try {
    await firstPieceApi.create(form)
    ElMessage.success('首件检验已创建')
    drawerVisible.value = false
    await loadData()
    await loadStats()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  } finally {
    saving.value = false
  }
}

async function handleSubmit() {
  const id = submitId.value
  if (!id) return
  saving.value = true
  try {
    await firstPieceApi.submit(id, {
      conclusion: submitForm.conclusion,
      allowedToProduce: submitForm.allowedToProduce,
      inspector: submitForm.inspector,
      items: submitForm.items,
    })
    ElMessage.success('首件检验已提交')
    drawerVisible.value = false
    await loadData()
    await loadStats()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  } finally {
    saving.value = false
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该首件检验？', '提示', { type: 'warning' })
    await firstPieceApi.delete(id)
    ElMessage.success('已删除')
    await loadData()
    await loadStats()
  } catch { /* cancelled */ }
}

async function viewDetail(id: number) {
  isEditing.value = false
  isSubmit.value = false
  editingId.value = id
  drawerTitle.value = `首件检验详情 - ${id}`
  try {
    const detail = await firstPieceApi.get(id)
    if (detail) {
      form.workOrderId = detail.workOrderId || 0
      form.processId = detail.processId || 0
      form.equipmentId = detail.equipmentId || 0
      form.operatorId = detail.operatorId || 0
      form.shift = detail.shift || '早班'
      form.reason = detail.reason || '班次切换'
      form.items = (detail.items || []).map(i => ({
        inspectionItemId: i.inspectionItemId || 0,
        itemName: i.itemName || '',
        dataType: i.dataType || 'numeric',
        usl: i.usl,
        lsl: i.lsl,
        actualValue: i.actualValue,
        result: i.result || 'pending',
      }))
    }
  } catch {
    ElMessage.error('加载详情失败')
    return
  }
  drawerVisible.value = true
}

onMounted(async () => {
  await Promise.all([loadData(), loadProcesses(), loadEquipment()])
  await loadStats()
})
</script>

<template>
  <div class="page-container">
    <!-- Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Check /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">首件检验</h2>
          <span class="page-header-banner-subtitle">管理产线首件检验流程与结论判定</span>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="ipqc-content">
      <!-- Stats Bar -->
      <div v-if="items.length > 0" class="stats-bar">
        <div class="stat-item stat-total">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.total }}</div>
            <div class="stat-label">首件总数</div>
          </div>
        </div>
        <div class="stat-item stat-qualified">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.qualified }}</div>
            <div class="stat-label">合格</div>
          </div>
        </div>
        <div class="stat-item stat-anomaly">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.unqualified }}</div>
            <div class="stat-label">不合格</div>
          </div>
        </div>
        <div class="stat-item stat-pending">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.pending }}</div>
            <div class="stat-label">待检验</div>
          </div>
        </div>
      </div>

      <!-- Data Card with Toolbar -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            首件检验列表
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索首件编号..."
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 200px"
              @keyup.enter="loadData"
            />
            <el-select
              v-model="statusFilter"
              clearable
              placeholder="筛选结论"
              size="small"
              style="width: 120px"
              @change="loadData"
            >
              <el-option
                v-for="o in IPQC_FIRST_PIECE_CONCLUSION_OPTIONS"
                :key="o.value"
                :label="o.label"
                :value="o.value"
              />
            </el-select>
            <el-button size="small" @click="loadData">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button type="primary" size="small" @click="openCreate">
              <el-icon><Plus /></el-icon>新建首件检验
            </el-button>
          </div>
        </div>

        <el-table
          :data="items"
          border
          stripe
          v-loading="loading"
          style="width: 100%"
          size="small"
          class="first-pieces-table"
        >
          <el-table-column type="index" label="序号" width="55" fixed />
          <el-table-column prop="fpNo" label="首件编号" min-width="160" show-overflow-tooltip />
          <el-table-column prop="processName" label="工序" min-width="120" show-overflow-tooltip />
          <el-table-column prop="equipmentName" label="设备" min-width="120" show-overflow-tooltip />
          <el-table-column prop="shift" label="班次" width="70" align="center" />
          <el-table-column prop="reason" label="原因" min-width="100" />
          <el-table-column label="结论" width="90" align="center">
            <template #default="{ row }">
              <el-tag
                :type="conclusionTag(row.conclusion)"
                size="small"
                effect="plain"
                round
              >
                {{ conclusionLabel(row.conclusion) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="允许量产" width="90" align="center">
            <template #default="{ row }">
              <el-tag v-if="row.allowedToProduce" type="success" size="small" round effect="plain">
                允许
              </el-tag>
              <el-tag v-else type="info" size="small" round>未允许</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="checkedAt" label="检验时间" min-width="160" show-overflow-tooltip>
            <template #default="{ row }">{{ formatDate(row.checkedAt) }}</template>
          </el-table-column>
          <el-table-column prop="createdAt" label="创建时间" min-width="160" show-overflow-tooltip>
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="240" align="center" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="viewDetail(row.id)">详情</el-button>
              <el-button
                v-if="row.conclusion === 'pending'"
                size="small"
                type="success"
                link
                @click.stop="openSubmit(row.id)"
              >提交</el-button>
              <el-button size="small" type="danger" link @click.stop="handleDelete(row.id)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Pagination -->
        <div class="data-card__pagination">
          <el-pagination
            v-model:current-page="page"
            v-model:page-size="pageSize"
            :total="total"
            :page-sizes="[10, 20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="loadData"
            @current-change="loadData"
          />
        </div>
      </div>
    </div>

    <!-- ================================================================== -->
    <!-- Drawer: 新建首件检验 -->
    <!-- ================================================================== -->
    <el-drawer
      v-model="drawerVisible"
      :title="drawerTitle"
      size="580px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <!-- Create / Detail Mode -->
      <template v-if="!isSubmit">
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><DocumentAdd /></el-icon>
            <span class="dialog-section-title">基本信息</span>
          </div>
          <el-form :model="form" label-width="90px">
            <el-form-item label="工单ID" required>
              <el-input-number v-model="form.workOrderId" :min="1" style="width: 100%" controls-position="right" />
            </el-form-item>
            <el-form-item label="工序" required>
              <el-select v-model="form.processId" filterable placeholder="选择工序" style="width: 100%">
                <el-option v-for="p in processes" :key="p.id" :label="p.name" :value="p.id" />
              </el-select>
            </el-form-item>
            <el-form-item label="设备" required>
              <el-select v-model="form.equipmentId" filterable placeholder="选择设备" style="width: 100%">
                <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
              </el-select>
            </el-form-item>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Plus /></el-icon>
            <span class="dialog-section-title">检验项目</span>
          </div>
          <div v-if="loadingPlanItems" style="padding: 8px 0; font-size: 13px; color: var(--el-text-color-secondary);">
            ⏳ 正在根据工序加载检验计划...
          </div>
          <div class="submit-list">
            <div v-for="(item, idx) in form.items" :key="idx" class="submit-row">
              <div class="submit-index">{{ idx + 1 }}</div>
              <el-input v-model="item.itemName" placeholder="项目名称" :readonly="!!item.inspectionItemId" style="width: 160px" size="default" />
              <el-select v-model="item.dataType" :disabled="!!item.inspectionItemId" style="width: 100px" size="default">
                <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
              <el-input-number v-model="item.usl" placeholder="USL" :precision="4" :step="0.1" controls-position="right" style="width: 120px" size="default" :disabled="!!item.inspectionItemId" />
              <el-input-number v-model="item.lsl" placeholder="LSL" :precision="4" :step="0.1" controls-position="right" style="width: 120px" size="default" :disabled="!!item.inspectionItemId" />
              <el-tag v-if="item.inspectionItemId" size="small" type="success" round effect="plain">计划</el-tag>
              <el-button link type="danger" size="small" :icon="Delete" @click="removeItem(idx)" :disabled="form.items.length <= 1" />
            </div>
            <el-button type="primary" link @click="addItem">
              <el-icon><Plus /></el-icon> 添加项目
            </el-button>
          </div>
        </div>
      </template>

      <!-- Submit Mode -->
      <template v-else>
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Check /></el-icon>
            <span class="dialog-section-title">结论判定</span>
          </div>
          <el-form :model="submitForm" label-width="90px">
            <el-form-item label="检验结论" required>
              <el-select v-model="submitForm.conclusion" style="width: 100%">
                <el-option v-for="o in IPQC_FIRST_PIECE_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
            <el-form-item label="允许量产">
              <el-switch v-model="submitForm.allowedToProduce" active-text="允许" inactive-text="不允许" />
            </el-form-item>
            <el-form-item label="检验员">
              <el-input v-model="submitForm.inspector" placeholder="检验员姓名" />
            </el-form-item>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><DocumentAdd /></el-icon>
            <span class="dialog-section-title">检验项结果</span>
          </div>
          <div class="submit-list">
            <div v-for="(item, idx) in submitForm.items" :key="idx" class="submit-row">
              <div class="submit-index">{{ idx + 1 }}</div>
              <span style="min-width: 120px; font-weight: 500; color: var(--el-text-color-regular);">{{ item.itemName }}</span>
              <el-input-number v-model="item.actualValue" :precision="4" :step="0.1" controls-position="right" style="width: 130px" size="default" />
              <el-select v-model="item.result" style="width: 110px" size="default">
                <el-option v-for="o in INSPECTION_RESULT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
              <span v-if="item.usl != null" style="color: var(--el-text-color-placeholder); font-size: 11px;">({{ item.lsl }} ~ {{ item.usl }})</span>
            </div>
          </div>
        </div>
      </template>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="drawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="isSubmit ? handleSubmit() : handleSave()" :loading="saving">
            {{ isSubmit ? '提交' : '保存' }}
          </el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<style scoped>
/* ─── 主体布局 ──────────────────────────────────── */
.page-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.ipqc-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  overflow-y: auto;
}

/* ── 统计栏 ── */
.stats-bar {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  flex-shrink: 0;
}
.stat-item {
  display: flex;
  align-items: center;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  padding: 14px 18px;
  transition: box-shadow 0.2s, transform 0.2s;
}
.stat-item:hover {
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transform: translateY(-1px);
}
.stat-accent {
  width: 4px;
  border-radius: 2px;
}
.stat-total .stat-accent { background: var(--el-color-primary); }
.stat-qualified .stat-accent { background: var(--el-color-success); }
.stat-anomaly .stat-accent { background: var(--el-color-danger); }
.stat-pending .stat-accent { background: var(--el-color-warning); }
.stat-content {
  display: flex;
  flex-direction: column;
}
.stat-value {
  font-size: 28px;
  font-weight: 700;
  line-height: 1.2;
}
.stat-total .stat-value { color: var(--el-color-primary); }
.stat-qualified .stat-value { color: var(--el-color-success); }
.stat-anomaly .stat-value { color: var(--el-color-danger); }
.stat-pending .stat-value { color: var(--el-color-warning); }
.stat-label {
  font-size: 13px;
  color: var(--el-text-color-secondary);
  margin-top: 2px;
}

/* ── 通用 data-card ── */
.data-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.data-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
  flex-shrink: 0;
}

.data-card__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.data-card__actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.data-card__pagination {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
  border-top: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
}

/* ── 首件检验表格 ── */
.first-pieces-table {
  flex: 1;
  min-height: 0;
  width: 100%;
}

.first-pieces-table :deep(.el-table__cell) {
  white-space: nowrap;
}

.first-pieces-table :deep(.el-table__header-wrapper) {
  flex-shrink: 0;
}

.first-pieces-table :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.first-pieces-table :deep(.el-table__row) {
  height: 32px;
  line-height: 32px;
}

.first-pieces-table :deep(.el-table__header-wrapper .el-table__cell) {
  height: 32px;
  line-height: 32px;
  padding: 0 8px;
}

.first-pieces-table :deep(.el-table__body-wrapper .el-table__cell) {
  padding: 0 8px;
}

.first-pieces-table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.first-pieces-table :deep(.el-button--primary.is-link:hover),
.first-pieces-table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.first-pieces-table :deep(.el-button--primary.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

.first-pieces-table :deep(.el-button--danger.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.first-pieces-table :deep(.el-button--danger.is-link:hover),
.first-pieces-table :deep(.el-button--danger.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.first-pieces-table :deep(.el-button--danger.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

/* ── 通用 Dialog / Drawer 分区 ── */
.dialog-section {
  margin-bottom: 16px;
}

.dialog-section:last-of-type {
  margin-bottom: 0;
}

.dialog-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
}

.dialog-section-icon {
  font-size: 15px;
  color: var(--el-color-primary);
}

.dialog-section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}

.submit-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.submit-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.submit-index {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 22px;
  height: 22px;
  border-radius: 4px;
  background: var(--el-color-primary-light-9);
  color: var(--el-color-primary);
  font-size: 11px;
  font-weight: 600;
  flex-shrink: 0;
}
</style>

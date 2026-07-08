<script setup lang="ts">
import { ref, reactive, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
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
    <!-- Toolbar -->
    <div class="toolbar-row">
      <el-input v-model="searchKeyword" placeholder="搜索单号..." clearable style="width: 260px" @clear="loadData" @keyup.enter="loadData" />
      <el-select v-model="statusFilter" placeholder="结论" clearable style="width: 140px" @change="loadData">
        <el-option v-for="o in IPQC_FIRST_PIECE_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
      </el-select>
      <el-button type="primary" @click="openCreate">+ 新建</el-button>
      <el-button @click="loadData">刷新</el-button>
    </div>

    <!-- Table -->
    <el-table :data="items" stripe size="small" v-loading="loading" style="flex:1">
      <el-table-column prop="fpNo" label="首件编号" width="180" />
      <el-table-column prop="processName" label="工序" width="140" />
      <el-table-column prop="equipmentName" label="设备" width="140" />
      <el-table-column prop="shift" label="班次" width="80" />
      <el-table-column prop="reason" label="原因" width="120" />
      <el-table-column label="结论" width="100">
        <template #default="{ row }">
          <el-tag :type="conclusionTag(row.conclusion)" size="small">
            {{ conclusionLabel(row.conclusion) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="允许量产" width="100">
        <template #default="{ row }">
          <el-tag v-if="row.allowedToProduce" type="success" size="small">允许</el-tag>
          <el-tag v-else type="info" size="small">未允许</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="checkedAt" label="检验时间" width="160">
        <template #default="{ row }">{{ formatDate(row.checkedAt) }}</template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="160">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="openSubmit(row.id)" v-if="row.conclusion === 'pending'">提交</el-button>
          <el-button link size="small" type="primary" @click="firstPieceApi.get(row.id).then(d => { /* detail view */ })">详情</el-button>
          <el-button link size="small" type="danger" @click="handleDelete(row.id)">删除</el-button>
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
        @current-change="loadData"
      />
    </div>

    <!-- Create Dialog -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="640px" :close-on-click-modal="false">
      <template v-if="!isSubmit">
        <el-form :model="form" label-width="100px" size="small">
          <el-form-item label="工单ID" required>
            <el-input-number v-model="form.workOrderId" :min="1" style="width:200px" />
          </el-form-item>
          <el-form-item label="工序" required>
            <el-select v-model="form.processId" filterable style="width:200px">
              <el-option v-for="p in processes" :key="p.id" :label="p.name" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="设备" required>
            <el-select v-model="form.equipmentId" filterable style="width:200px">
              <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="操作员ID">
            <el-input-number v-model="form.operatorId" :min="0" style="width:200px" />
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
          <el-form-item label="检验项">
            <div style="width:100%">
              <div v-if="loadingPlanItems" style="color:#909399;padding:8px 0;">⏳ 正在加载检验计划...</div>
              <div v-for="(item, idx) in form.items" :key="idx" style="display:flex;gap:8px;margin-bottom:8px;align-items:center">
                <el-input v-model="item.itemName" placeholder="项目名称" style="width:140px" :readonly="!!item.inspectionItemId" />
                <el-select v-model="item.dataType" style="width:100px" :disabled="!!item.inspectionItemId">
                  <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                </el-select>
                <el-input-number v-model="item.usl" placeholder="USL" :precision="4" :step="0.1" style="width:110px" controls-position="right" :disabled="!!item.inspectionItemId" />
                <el-input-number v-model="item.lsl" placeholder="LSL" :precision="4" :step="0.1" style="width:110px" controls-position="right" :disabled="!!item.inspectionItemId" />
                <el-tag v-if="item.inspectionItemId" size="small" type="info">计划</el-tag>
                <el-button link type="danger" @click="removeItem(idx)" :disabled="form.items.length <= 1">✕</el-button>
              </div>
              <el-button size="small" @click="addItem">+ 添加项目</el-button>
            </div>
          </el-form-item>
        </el-form>
      </template>
      <template v-else>
        <el-form :model="submitForm" label-width="100px" size="small">
          <el-form-item label="检验结论" required>
            <el-select v-model="submitForm.conclusion" style="width:200px">
              <el-option v-for="o in IPQC_FIRST_PIECE_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="允许量产">
            <el-switch v-model="submitForm.allowedToProduce" />
          </el-form-item>
          <el-form-item label="检验员">
            <el-input v-model="submitForm.inspector" style="width:200px" />
          </el-form-item>
          <el-form-item label="检验项结果">
            <div style="width:100%">
              <div v-for="(item, idx) in submitForm.items" :key="idx" style="display:flex;gap:8px;margin-bottom:8px;align-items:center">
                <span style="min-width:120px">{{ item.itemName }}</span>
                <el-input-number v-model="item.actualValue" :precision="4" :step="0.1" style="width:140px" controls-position="right" />
                <el-select v-model="item.result" style="width:100px">
                  <el-option v-for="o in INSPECTION_RESULT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                </el-select>
                <span v-if="item.usl != null" style="color:#909399;font-size:12px">({{ item.lsl }} ~ {{ item.usl }})</span>
              </div>
            </div>
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="isSubmit ? handleSubmit() : handleSave()">
          {{ isSubmit ? '提交' : '保存' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

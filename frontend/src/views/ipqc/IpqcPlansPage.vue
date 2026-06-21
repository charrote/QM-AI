<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { patrolPlanApi } from '@/api/ipqc'
import { processApi, equipmentApi } from '@/api/basicData'
import type { IpqcPatrolPlan, CreateIpqcPatrolPlan, UpdateIpqcPatrolPlan } from '@/types/ipqc'
import { IPQC_PATROL_PLAN_STATUS_OPTIONS } from '@/types/ipqc'
import type { PagedRequest } from '@/types/basicData'
import type { Process, Equipment } from '@/types/basicData'

defineOptions({ name: 'IpqcPlansPage' })

// ─── State ──────────────────────────────────────
const searchKeyword = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const items = ref<IpqcPatrolPlan[]>([])
const loading = ref(false)
const dialogVisible = ref(false)
const processes = ref<Process[]>([])
const equipmentList = ref<Equipment[]>([])

const form = ref<CreateIpqcPatrolPlan>({
  processId: 0,
  equipmentId: 0,
  patrolIntervalMin: 120,
  autoGenerate: true,
})

const editingId = ref<number | null>(null)

// ─── Helpers ────────────────────────────────────
function statusTag(s: string) {
  const opt = IPQC_PATROL_PLAN_STATUS_OPTIONS.find(o => o.value === s)
  return opt?.type || 'info'
}

function statusLabel(s: string) {
  const opt = IPQC_PATROL_PLAN_STATUS_OPTIONS.find(o => o.value === s)
  return opt?.label || s
}

// ─── Load ──────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const params: PagedRequest = {
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
      status: statusFilter.value || undefined,
    }
    const res = await patrolPlanApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch {
    ElMessage.error('加载巡检计划失败')
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
  editingId.value = null
  form.value = { processId: 0, equipmentId: 0, patrolIntervalMin: 120, autoGenerate: true }
  dialogVisible.value = true
}

async function openEdit(id: number) {
  const plan = await patrolPlanApi.get(id)
  if (!plan) return
  editingId.value = id
  form.value = {
    processId: plan.processId,
    equipmentId: plan.equipmentId,
    patrolIntervalMin: plan.patrolIntervalMin,
    autoGenerate: plan.autoGenerate,
    inspector: plan.inspector,
  }
  dialogVisible.value = true
}

async function handleSave() {
  if (!form.value.processId || !form.value.equipmentId) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    if (editingId.value) {
      await patrolPlanApi.update(editingId.value, form.value as UpdateIpqcPatrolPlan)
      ElMessage.success('已更新')
    } else {
      await patrolPlanApi.create(form.value)
      ElMessage.success('已创建')
    }
    dialogVisible.value = false
    await loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该巡检计划？', '提示', { type: 'warning' })
    await patrolPlanApi.delete(id)
    ElMessage.success('已删除')
    await loadData()
  } catch { /* cancelled */ }
}

async function handleGenerate(planId: number) {
  try {
    const res = await patrolPlanApi.generate(planId, {
      startTime: new Date().toISOString(),
      count: 4,
    })
    ElMessage.success(`已生成 ${res.length} 条巡检任务`)
    await loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '生成失败')
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
      <el-input v-model="searchKeyword" placeholder="搜索计划编号..." clearable style="width:260px" @clear="loadData" @keyup.enter="loadData" />
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width:120px" @change="loadData">
        <el-option v-for="o in IPQC_PATROL_PLAN_STATUS_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
      </el-select>
      <el-button type="primary" @click="openCreate">+ 新建计划</el-button>
      <el-button @click="loadData">刷新</el-button>
    </div>

    <!-- Table -->
    <el-table :data="items" stripe size="small" v-loading="loading" style="flex:1">
      <el-table-column prop="planNo" label="计划编号" width="180" />
      <el-table-column prop="processName" label="工序" width="140" />
      <el-table-column prop="equipmentName" label="设备" width="140" />
      <el-table-column prop="patrolIntervalMin" label="间隔(分钟)" width="110" />
      <el-table-column label="自动生成" width="90">
        <template #default="{ row }">
          <el-tag :type="row.autoGenerate ? 'success' : 'info'" size="small">{{ row.autoGenerate ? '是' : '否' }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="inspector" label="检验员" width="120" />
      <el-table-column label="操作" width="220" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="handleGenerate(row.id)">生成任务</el-button>
          <el-button link size="small" type="primary" @click="openEdit(row.id)">编辑</el-button>
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

    <!-- Dialog -->
    <el-dialog v-model="dialogVisible" :title="editingId ? '编辑巡检计划' : '新建巡检计划'" width="500px" :close-on-click-modal="false">
      <el-form :model="form" label-width="110px" size="small">
        <el-form-item label="工序" required>
          <el-select v-model="form.processId" filterable style="width:250px">
            <el-option v-for="p in processes" :key="p.id" :label="p.name" :value="p.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="设备" required>
          <el-select v-model="form.equipmentId" filterable style="width:250px">
            <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="巡检间隔(分钟)" required>
          <el-input-number v-model="form.patrolIntervalMin" :min="10" :max="1440" style="width:250px" />
        </el-form-item>
        <el-form-item label="自动生成">
          <el-switch v-model="form.autoGenerate" />
        </el-form-item>
        <el-form-item label="检验员">
          <el-input v-model="form.inspector" style="width:250px" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

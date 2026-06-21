<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { patrolApi } from '@/api/ipqc'
import type { IpqcPatrol, IpqcPatrolDetail, SubmitIpqcPatrol, IpqcPatrolItemSubmit } from '@/types/ipqc'
import {
  IPQC_PATROL_STATUS_OPTIONS,
  IPQC_PATROL_CONCLUSION_OPTIONS,
  INSPECTION_RESULT_OPTIONS,
  DATA_TYPE_OPTIONS,
} from '@/types/ipqc'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'IpqcPatrolsPage' })

// ─── State ──────────────────────────────────────
const searchKeyword = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const items = ref<IpqcPatrol[]>([])
const loading = ref(false)

const dialogVisible = ref(false)
const dialogTitle = ref('')
const patrolDetail = ref<IpqcPatrolDetail | null>(null)
const submitForm = ref<SubmitIpqcPatrol>({
  conclusion: 'pending',
  items: [],
})

// ─── Helpers ────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function statusTag(s: string) {
  const opt = IPQC_PATROL_STATUS_OPTIONS.find(o => o.value === s)
  return opt?.type || 'info'
}

function statusLabel(s: string) {
  const opt = IPQC_PATROL_STATUS_OPTIONS.find(o => o.value === s)
  return opt?.label || s
}

function conclusionTag(s: string) {
  const opt = IPQC_PATROL_CONCLUSION_OPTIONS.find(o => o.value === s)
  return opt?.type || 'info'
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
    const res = await patrolApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch {
    ElMessage.error('加载巡检记录失败')
  } finally {
    loading.value = false
  }
}

// ─── Patrol Submit ──────────────────────────────
async function openSubmit(id: number) {
  try {
    const detail = await patrolApi.get(id)
    patrolDetail.value = detail
    dialogTitle.value = `提交巡检 - ${detail.patrolNo}`
    submitForm.value = {
      conclusion: 'pending',
      remarks: detail.remarks || '',
      items: (detail.items || []).map(i => ({
        id: i.id,
        itemName: i.itemName,
        itemCode: i.itemCode,
        usl: i.usl,
        lsl: i.lsl,
        dataType: i.dataType,
        actualValue: i.actualValue,
        result: i.result || 'pending',
        imageUrls: i.imageUrls,
      })),
    }
    // If no items yet, add a default one
    if (!submitForm.value.items || submitForm.value.items.length === 0) {
      submitForm.value.items = [{ itemName: '外观检查', dataType: 'visual', result: 'pending' }]
    }
    ;(window as any).__submitPatrolId = id
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载巡检详情失败')
  }
}

function addPatrolItem() {
  submitForm.value.items!.push({ itemName: '', dataType: 'numeric', result: 'pending' })
}

function removePatrolItem(index: number) {
  submitForm.value.items!.splice(index, 1)
}

async function handleSubmit() {
  const id = (window as any).__submitPatrolId
  if (!id) return
  try {
    await patrolApi.submit(id, {
      conclusion: submitForm.value.conclusion,
      remarks: submitForm.value.remarks,
      items: submitForm.value.items,
    })
    ElMessage.success('巡检已提交')
    dialogVisible.value = false
    await loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  }
}

async function handleMiss(id: number) {
  try {
    await ElMessageBox.confirm('确认标记为错过？', '提示', { type: 'warning' })
    await patrolApi.miss(id)
    ElMessage.success('已标记为错过')
    await loadData()
  } catch { /* cancelled */ }
}

async function viewDetail(id: number) {
  try {
    const detail = await patrolApi.get(id)
    patrolDetail.value = detail
    dialogTitle.value = `巡检详情 - ${detail.patrolNo}`
    submitForm.value = {
      conclusion: detail.conclusion,
      remarks: detail.remarks || '',
      items: (detail.items || []).map(i => ({
        id: i.id,
        itemName: i.itemName,
        itemCode: i.itemCode,
        usl: i.usl,
        lsl: i.lsl,
        dataType: i.dataType,
        actualValue: i.actualValue,
        result: i.result,
        imageUrls: i.imageUrls,
      })),
    }
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载巡检详情失败')
  }
}

onMounted(async () => {
  await loadData()
})
</script>

<template>
  <div class="page-container">
    <!-- Toolbar -->
    <div class="toolbar-row">
      <el-input v-model="searchKeyword" placeholder="搜索巡检编号..." clearable style="width:260px" @clear="loadData" @keyup.enter="loadData" />
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width:120px" @change="loadData">
        <el-option v-for="o in IPQC_PATROL_STATUS_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
      </el-select>
      <el-button @click="loadData">刷新</el-button>
    </div>

    <!-- Table -->
    <el-table :data="items" stripe size="small" v-loading="loading" style="flex:1">
      <el-table-column prop="patrolNo" label="巡检编号" width="170" />
      <el-table-column prop="planNo" label="计划编号" width="170" />
      <el-table-column prop="processName" label="工序" width="120" />
      <el-table-column prop="equipmentName" label="设备" width="120" />
      <el-table-column label="计划时间" width="150">
        <template #default="{ row }">{{ formatDate(row.scheduledTime) }}</template>
      </el-table-column>
      <el-table-column label="实际时间" width="150">
        <template #default="{ row }">{{ formatDate(row.actualTime) }}</template>
      </el-table-column>
      <el-table-column prop="totalChecked" label="检验数" width="80" />
      <el-table-column prop="totalPass" label="合格" width="60" />
      <el-table-column prop="totalFail" label="不合格" width="70" />
      <el-table-column label="结论" width="80">
        <template #default="{ row }">
          <el-tag :type="conclusionTag(row.conclusion)" size="small">
            {{ row.conclusion === 'qualified' ? '合格' : row.conclusion === 'unqualified' ? '不合格' : '待定' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ statusLabel(row.status) }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="220" fixed="right">
        <template #default="{ row }">
          <el-button v-if="row.status === 'scheduled'" link size="small" type="primary" @click="openSubmit(row.id)">执行</el-button>
          <el-button v-else link size="small" type="primary" @click="viewDetail(row.id)">详情</el-button>
          <el-button v-if="row.status === 'scheduled'" link size="small" type="warning" @click="handleMiss(row.id)">跳过</el-button>
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

    <!-- Patrol Submit / Detail Dialog -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="700px" :close-on-click-modal="false">
      <template v-if="patrolDetail">
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:16px;font-size:13px;color:#606266">
          <div><strong>设备：</strong>{{ patrolDetail.equipmentName || '-' }}</div>
          <div><strong>工序：</strong>{{ patrolDetail.processName || '-' }}</div>
          <div><strong>计划时间：</strong>{{ formatDate(patrolDetail.scheduledTime) }}</div>
          <div><strong>实际时间：</strong>{{ formatDate(patrolDetail.actualTime) }}</div>
        </div>
      </template>

      <el-form :model="submitForm" label-width="100px" size="small">
        <el-form-item label="检验结论">
          <el-select v-model="submitForm.conclusion" style="width:200px">
            <el-option v-for="o in IPQC_PATROL_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
          </el-select>
        </el-form-item>
        <el-form-item label="备注">
          <el-input v-model="submitForm.remarks" type="textarea" :rows="2" style="width:400px" />
        </el-form-item>
        <el-form-item label="检验项">
          <div style="width:100%">
            <div v-for="(item, idx) in submitForm.items" :key="idx" style="display:flex;gap:8px;margin-bottom:8px;align-items:center;flex-wrap:wrap">
              <el-input v-model="item.itemName" placeholder="项目名称" style="width:130px" :disabled="!!item.id" />
              <el-select v-model="item.dataType" style="width:90px" :disabled="!!item.id">
                <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
              <el-input-number v-model="item.actualValue" :precision="4" :step="0.1" style="width:140px" controls-position="right" />
              <el-select v-model="item.result" style="width:100px">
                <el-option v-for="o in INSPECTION_RESULT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
              <el-button v-if="!item.id" link type="danger" @click="removePatrolItem(idx)">✕</el-button>
            </div>
            <el-button size="small" @click="addPatrolItem">+ 添加项目</el-button>
          </div>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">关闭</el-button>
        <el-button type="primary" @click="handleSubmit" v-if="patrolDetail?.status === 'scheduled'">提交</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

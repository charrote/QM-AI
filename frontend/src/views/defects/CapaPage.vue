<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { defectApi } from '@/api/defect'
import type { Capa, CreateCapa } from '@/types/defect'
import {
  CAPA_PHASE_LABELS, CAPA_PHASE_MAP, SEVERITY_OPTIONS, SEVERITY_MAP,
  CAPA_STATUS_OPTIONS, CAPA_STATUS_MAP,
} from '@/types/defect'

defineOptions({ name: 'CapaPage' })

const router = useRouter()
const route = useRoute()

const searchKeyword = ref('')
const statusFilter = ref('')
const phaseFilter = ref<number | undefined>(undefined)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const capas = ref<Capa[]>([])

const dialogVisible = ref(false)
const isEditing = ref(false)
const currentCapaId = ref<number | null>(null)
const capaForm = reactive<CreateCapa>({
  defectId: 0, severity: 'major', title: '', description: '',
  assignedTo: '', dueDate: new Date().toISOString().slice(0, 10),
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

const phaseLabel = (phase: number) => CAPA_PHASE_MAP[phase] || String(phase)

const phaseColor = (phase: number) => {
  if (phase <= 2) return 'warning'
  if (phase <= 5) return 'primary'
  return 'success'
}

async function loadCapas() {
  try {
    const res = await defectApi.getAllCapa({
      page: page.value, pageSize: pageSize.value,
      status: statusFilter.value || undefined,
      phase: phaseFilter.value !== undefined ? phaseFilter.value : undefined,
    })
    capas.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load CAPAs', e)
  }
}

function openCreate(queryDefectId?: number) {
  isEditing.value = false
  currentCapaId.value = null
  capaForm.defectId = queryDefectId || 0
  capaForm.severity = 'major'
  capaForm.title = ''
  capaForm.description = ''
  capaForm.assignedTo = ''
  capaForm.dueDate = new Date().toISOString().slice(0, 10)
  dialogVisible.value = true
}

function openEdit(row: Capa) {
  isEditing.value = true
  currentCapaId.value = row.id
  capaForm.defectId = row.defectId
  capaForm.severity = row.severity
  capaForm.title = row.title
  capaForm.description = row.description
  capaForm.assignedTo = row.assignedTo
  capaForm.dueDate = row.dueDate?.slice(0, 10) || ''
  dialogVisible.value = true
}

async function saveCapa() {
  if (!capaForm.title || !capaForm.assignedTo) {
    ElMessage.warning('请填写CAPA标题和负责人')
    return
  }
  try {
    if (isEditing.value && currentCapaId.value) {
      await defectApi.updateCapa(currentCapaId.value, capaForm)
      ElMessage.success('CAPA已更新')
    } else {
      await defectApi.createCapa(capaForm)
      ElMessage.success('CAPA已创建')
    }
    dialogVisible.value = false
    await loadCapas()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function advancePhase(row: Capa) {
  if (row.currentPhase >= 6) {
    ElMessage.warning('CAPA已全部完成')
    return
  }
  try {
    await ElMessageBox.confirm(
      `确认推进到「${CAPA_PHASE_MAP[row.currentPhase + 1]}」阶段？`,
      '确认推进',
      { type: 'info' }
    )
    await defectApi.updateCapaPhase(row.id, row.currentPhase + 1)
    ElMessage.success('阶段已推进')
    await loadCapas()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteCapa(row: Capa) {
  try {
    await ElMessageBox.confirm(`确定删除CAPA「${row.capaCode}」吗？`, '确认', { type: 'warning' })
    await defectApi.deleteCapa(row.id)
    ElMessage.success('已删除')
    await loadCapas()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

function viewDetail(row: Capa) {
  router.push({ name: 'CapaDetail', params: { id: String(row.id) } })
}

onMounted(() => {
  loadCapas()
  if (route.query.defectId) {
    openCreate(Number(route.query.defectId))
  }
})
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索CAPA编号/标题..."
        clearable
        style="width: 240px"
        @keyup.enter="loadCapas"
      />
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 110px" @change="loadCapas">
        <el-option v-for="opt in CAPA_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="phaseFilter" placeholder="阶段" clearable style="width: 130px" @change="loadCapas">
        <el-option v-for="opt in CAPA_PHASE_LABELS" :key="opt.phase" :label="opt.label" :value="opt.phase" />
      </el-select>
      <el-button type="primary" @click="openCreate">+ 新建CAPA</el-button>
      <el-button @click="loadCapas">刷新</el-button>
    </div>

    <el-table :data="capas" stripe style="width: 100%" size="small">
      <el-table-column prop="capaCode" label="CAPA编号" width="140" />
      <el-table-column prop="title" label="标题" min-width="180" show-overflow-tooltip />
      <el-table-column label="严重程度" width="80">
        <template #default="{ row }">
          <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small" effect="plain">
            {{ SEVERITY_MAP[row.severity] || row.severity }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="阶段" width="120">
        <template #default="{ row }">
          <el-steps :active="row.currentPhase" finish-status="success" simple size="small">
            <el-step
              v-for="step in CAPA_PHASE_LABELS"
              :key="step.phase"
              :title="step.label"
              :status="row.currentPhase > step.phase ? 'success' : row.currentPhase === step.phase ? step.phase <= 2 ? 'warning' : 'primary' : ''"
            />
          </el-steps>
        </template>
      </el-table-column>
      <el-table-column label="阶段名" width="110">
        <template #default="{ row }">
          <el-tag :type="phaseColor(row.currentPhase)" size="small">
            {{ phaseLabel(row.currentPhase) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="assignedTo" label="负责人" width="90" />
      <el-table-column label="截止日期" width="110">
        <template #default="{ row }">{{ row.dueDate?.slice(0, 10) || '-' }}</template>
      </el-table-column>
      <el-table-column label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="CAPA_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
            {{ CAPA_STATUS_MAP[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="viewDetail(row)">详情</el-button>
          <el-button link size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button
            v-if="row.currentPhase < 6 && row.status === 'active'"
            link size="small" type="success"
            @click="advancePhase(row)"
          >推进阶段</el-button>
          <el-button link size="small" type="danger" @click="deleteCapa(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-row">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next"
        size="small"
        @current-change="loadCapas"
      />
    </div>

    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? '编辑CAPA' : '新建CAPA'"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form :model="capaForm" label-width="100px" size="small">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="缺陷ID">
              <el-input-number v-model="capaForm.defectId" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="严重程度" required>
              <el-select v-model="capaForm.severity" style="width: 100%">
                <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="标题" required>
          <el-input v-model="capaForm.title" placeholder="CAPA标题" />
        </el-form-item>
        <el-form-item label="描述">
          <el-input v-model="capaForm.description" type="textarea" :rows="3" placeholder="问题描述" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="负责人" required>
              <el-input v-model="capaForm.assignedTo" placeholder="负责人姓名" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="截止日期" required>
              <el-date-picker v-model="capaForm.dueDate" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveCapa">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

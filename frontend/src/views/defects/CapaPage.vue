<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { CircleCheck, Refresh, Plus } from '@element-plus/icons-vue'
import { defectApi } from '@/api/defect'
import RightPanel from '@/components/layout/RightPanel.vue'
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

// ─── Stats ─────────────────────────────────────────────
const stats = computed(() => ({
  total: capas.value.length,
  open: capas.value.filter(c => c.status === 'open').length,
  inProgress: capas.value.filter(c => c.status === 'in_progress').length,
  closed: capas.value.filter(c => c.status === 'closed').length,
  overdue: capas.value.filter(c => isOverdue(c)).length,
}))

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

function isOverdue(row: Capa): boolean {
  if (!row.dueDate) return false
  return new Date(row.dueDate) < new Date() && row.status !== 'closed'
}

function rowClassName({ row }: { row: Capa }) {
  return isOverdue(row) ? 'overdue-row' : ''
}

const formRules = {
  title: [{ required: true, message: '请输入CAPA标题', trigger: 'blur' }],
  assignedTo: [{ required: true, message: '请输入负责人', trigger: 'blur' }],
}

const capaFormRef = ref()

onMounted(() => {
  loadCapas()
  if (route.query.defectId) {
    openCreate(Number(route.query.defectId))
  }
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="m07-header-banner m07-header-banner--primary">
      <div class="m07-header-banner-main">
        <div class="m07-header-banner-left">
          <div class="m07-header-banner-icon">
            <el-icon :size="24"><CircleCheck /></el-icon>
          </div>
          <div class="m07-header-banner-text">
            <div class="m07-header-banner-title">CAPA 纠正预防措施</div>
            <div class="m07-header-banner-subtitle">缺陷纠正与预防措施的全流程跟踪管理</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stats Bar -->
    <div v-if="stats.total > 0" class="m07-stat-grid">
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--primary">
          <el-icon :size="22"><CircleCheck /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">CAPA 总数</div>
          <div class="m07-stat-value m07-stat-value--primary">{{ stats.total }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--primary">
          <el-icon :size="22"><CircleCheck /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">未处理</div>
          <div class="m07-stat-value m07-stat-value--danger">{{ stats.open }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--success">
          <el-icon :size="22"><CircleCheck /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">进行中</div>
          <div class="m07-stat-value m07-stat-value--primary">{{ stats.inProgress }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--info">
          <el-icon :size="22"><CircleCheck /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">已完成</div>
          <div class="m07-stat-value m07-stat-value--success">{{ stats.closed }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--danger">
          <el-icon :size="22"><CircleCheck /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">已逾期</div>
          <div class="m07-stat-value m07-stat-value--danger">{{ stats.overdue }}</div>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="m07-table-card">
      <div class="m07-table-card__header">
        <div class="m07-table-card__title">
          <el-icon><CircleCheck /></el-icon>
          <span>CAPA 记录列表</span>
          <el-tag v-if="total" type="info" size="small" class="m07-table-card__count">
            共 {{ total }} 条
          </el-tag>
        </div>
        <div class="m07-table-card__toolbar">
          <div class="m07-filter-bar">
            <el-input v-model="searchKeyword" placeholder="搜索CAPA编号/标题..." clearable style="width: 220px" @keyup.enter="loadCapas" @clear="loadCapas" />
            <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 100px" @clear="loadCapas">
              <el-option v-for="opt in CAPA_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
            <el-select v-model="phaseFilter" placeholder="阶段" clearable style="width: 120px" @clear="loadCapas">
              <el-option v-for="opt in CAPA_PHASE_LABELS" :key="opt.phase" :label="opt.label" :value="opt.phase" />
            </el-select>
          </div>
          <el-button @click="loadCapas" text>
            <el-icon><Refresh /></el-icon>刷新
          </el-button>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>新建CAPA
          </el-button>
        </div>
      </div>

      <div class="m07-table-card__body">
        <el-table
          :data="capas"
          border
          stripe
          style="width: 100%"
          size="small"
          empty-text="暂无数据"
          class="m07-table"
          :row-class-name="rowClassName"
        >
          <el-table-column prop="capaCode" label="CAPA编号" width="140" />
          <el-table-column prop="title" label="标题" min-width="180" show-overflow-tooltip />
          <el-table-column label="严重程度" width="110">
            <template #default="{ row }">
              <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small" effect="plain">
                {{ SEVERITY_MAP[row.severity] || row.severity }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="阶段" width="130">
            <template #default="{ row }">
              <el-tag :type="phaseColor(row.currentPhase)" size="small" effect="dark">
                {{ phaseLabel(row.currentPhase) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="assignedTo" label="负责人" width="90" />
          <el-table-column label="截止日期" width="115">
            <template #default="{ row }">
              <span :class="{ 'overdue-text': isOverdue(row) }">{{ row.dueDate?.slice(0, 10) || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="85">
            <template #default="{ row }">
              <el-tag :type="CAPA_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
                {{ CAPA_STATUS_MAP[row.status] || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="230" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="viewDetail(row)">详情</el-button>
              <el-button size="small" type="primary" link @click.stop="openEdit(row)">编辑</el-button>
              <template v-if="row.currentPhase < 6 && row.status === 'in_progress'">
                <el-button size="small" type="success" link @click.stop="advancePhase(row)">推进阶段</el-button>
              </template>
              <el-button size="small" type="danger" link @click.stop="deleteCapa(row)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="m07-table-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          :small="true"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="loadCapas"
          @size-change="loadCapas"
          class="data-card__pagination"
        />
      </div>
    </div>

    <!-- Create/Edit RightPanel -->
    <RightPanel
      v-model:visible="dialogVisible"
      :title="isEditing ? '编辑CAPA' : '新建CAPA'"
      :width="600"
    >
      <template #body>
        <el-form :model="capaForm" label-width="100px" :rules="formRules" ref="capaFormRef">
          <el-divider content-position="left">基本信息</el-divider>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="缺陷ID">
                <el-input-number v-model="capaForm.defectId" :min="0" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="严重程度" prop="severity" required>
                <el-select v-model="capaForm.severity" style="width: 100%">
                  <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="标题" prop="title" required>
            <el-input v-model="capaForm.title" placeholder="CAPA标题" />
          </el-form-item>
          <el-form-item label="描述">
            <el-input v-model="capaForm.description" type="textarea" :rows="3" placeholder="问题描述" />
          </el-form-item>
          <el-divider content-position="left">责任人信息</el-divider>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="负责人" prop="assignedTo" required>
                <el-input v-model="capaForm.assignedTo" placeholder="负责人姓名" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="截止日期" prop="dueDate" required>
                <el-date-picker v-model="capaForm.dueDate" type="date" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveCapa">保存</el-button>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
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

/* 逾期日期文本 — 红色加粗 */
.overdue-text {
  color: var(--el-color-danger, #ff4d4f);
  font-weight: 600;
}
</style>

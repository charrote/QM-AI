<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Document, Plus, Search, Refresh, Setting, Clock, User, Check } from '@element-plus/icons-vue'
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
const processes = ref<Process[]>([])
const equipmentList = ref<Equipment[]>([])
const saving = ref(false)

// Stats
const stats = ref({ total: 0, active: 0, paused: 0 })

const drawerVisible = ref(false)
const drawerTitle = ref('新建巡检计划')
const isEditing = ref(false)
const editingId = ref<number | null>(null)

const form = reactive<CreateIpqcPatrolPlan>({
  processId: 0,
  equipmentIds: [],
  patrolIntervalMin: 120,
  autoGenerate: true,
})

// 设备类型分组
const equipmentTypeOptions = ref([
  { value: '', label: '全部设备', types: [] },
  { value: 'CNC', label: 'CNC加工中心', types: ['CNC'] },
  { value: 'PLC', label: 'PLC设备', types: ['PLC'] },
  { value: '检测设备', label: '检测设备', types: ['检测设备'] },
  { value: '机器人', label: '机器人', types: ['机器人'] },
  { value: '其他', label: '其他', types: ['其他'] },
])
const selectedTypeFilter = ref('')

// 按设备类型分组的设备列表（用于多选下拉）
const equipmentByType = computed(() => {
  const grouped: Record<string, Equipment[]> = {}
  equipmentList.value.forEach(e => {
    const type = e.equipmentType || '其他'
    if (!grouped[type]) grouped[type] = []
    grouped[type].push(e)
  })
  return grouped
})

// 按选中类型筛选设备
const filteredEquipmentByType = computed(() => {
  if (!selectedTypeFilter.value) return []
  const opts = equipmentTypeOptions.value.find(t => t.value === selectedTypeFilter.value)
  if (!opts) return []
  const allDevices: Equipment[] = []
  opts.types.forEach(type => {
    if (equipmentByType.value[type]) {
      allDevices.push(...equipmentByType.value[type])
    }
  })
  return allDevices
})

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
async function loadStats() {
  try {
    const [allRes, activeRes, pausedRes] = await Promise.allSettled([
      patrolPlanApi.list({ page: 1, pageSize: 1, keyword: undefined, status: undefined }),
      patrolPlanApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'active' }),
      patrolPlanApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'paused' }),
    ])
    stats.value = {
      total: allRes.status === 'fulfilled' ? allRes.value.total : 0,
      active: activeRes.status === 'fulfilled' ? activeRes.value.total : 0,
      paused: pausedRes.status === 'fulfilled' ? pausedRes.value.total : 0,
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
    const res = await patrolPlanApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch (e: any) {
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
  isEditing.value = false
  editingId.value = null
  drawerTitle.value = '新建巡检计划'
  form.processId = 0
  form.equipmentIds = []
  form.patrolIntervalMin = 120
  form.autoGenerate = true
  selectedTypeFilter.value = ''
  drawerVisible.value = true
}

async function openEdit(id: number) {
  isEditing.value = true
  editingId.value = id
  drawerTitle.value = '编辑巡检计划'
  try {
    const plan = await patrolPlanApi.get(id)
    if (plan) {
      form.processId = plan.processId
      form.equipmentIds = plan.equipmentIds || []
      form.patrolIntervalMin = plan.patrolIntervalMin
      form.autoGenerate = plan.autoGenerate
      form.inspector = plan.inspector
    }
  } catch {
    ElMessage.error('加载计划失败')
    return
  }
  drawerVisible.value = true
}

async function handleSave() {
  if (!form.processId || !form.equipmentIds || form.equipmentIds.length === 0) {
    ElMessage.warning('请填写完整信息（工序和设备至少各选一个）')
    return
  }
  saving.value = true
  try {
    if (isEditing.value && editingId.value) {
      await patrolPlanApi.update(editingId.value, form as UpdateIpqcPatrolPlan)
      ElMessage.success('已更新')
    } else {
      await patrolPlanApi.create(form)
      ElMessage.success('已创建')
    }
    drawerVisible.value = false
    await loadData()
    await loadStats()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该巡检计划？', '提示', { type: 'warning' })
    await patrolPlanApi.delete(id)
    ElMessage.success('已删除')
    await loadData()
    await loadStats()
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
  await loadStats()
})
</script>

<template>
  <div class="page-container">
    <!-- Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Document /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">巡检计划管理</h2>
          <span class="page-header-banner-subtitle">配置巡检规则，自动生成巡检任务</span>
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
            <div class="stat-label">计划总数</div>
          </div>
        </div>
        <div class="stat-item stat-qualified">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.active }}</div>
            <div class="stat-label">已启用</div>
          </div>
        </div>
        <div class="stat-item stat-pending">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.paused }}</div>
            <div class="stat-label">已暂停</div>
          </div>
        </div>
        <div class="stat-item stat-inspecting">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ items.filter(i => i.autoGenerate).length }}</div>
            <div class="stat-label">自动生成</div>
          </div>
        </div>
      </div>

      <!-- Data Card with Toolbar -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            巡检计划列表
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索计划编号"
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 200px"
              @keyup.enter="loadData"
            />
            <el-select
              v-model="statusFilter"
              clearable
              placeholder="状态筛选"
              size="small"
              style="width: 120px"
              @change="loadData"
            >
              <el-option
                v-for="o in IPQC_PATROL_PLAN_STATUS_OPTIONS"
                :key="o.value"
                :label="o.label"
                :value="o.value"
              />
            </el-select>
            <el-button size="small" @click="loadData">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button type="primary" size="small" @click="openCreate">
              <el-icon><Plus /></el-icon>新建计划
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
          class="plans-table"
        >
          <el-table-column type="index" label="序号" width="55" fixed />
          <el-table-column prop="planNo" label="计划编号" min-width="160" show-overflow-tooltip />
          <el-table-column prop="processName" label="工序" min-width="120" show-overflow-tooltip />
          <el-table-column prop="equipmentNames" label="设备" min-width="160" show-overflow-tooltip>
            <template #default="{ row }">
              <div v-if="row.equipmentNames && row.equipmentNames.length > 0" style="display: flex; flex-wrap: wrap; gap: 4px; align-items: center;">
                <el-tag v-for="(name, idx) in row.equipmentNames" :key="idx" type="info" size="small" effect="plain" round>
                  {{ name }}
                </el-tag>
              </div>
              <span v-else class="text-muted">-</span>
            </template>
          </el-table-column>
          <el-table-column prop="patrolIntervalMin" label="间隔 (分钟)" width="110" align="center" />
          <el-table-column label="自动生成" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.autoGenerate ? 'success' : 'info'" size="small" effect="plain" round>
                {{ row.autoGenerate ? '是' : '否' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="statusTag(row.status)" size="small" effect="plain" round>{{ statusLabel(row.status) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验员" min-width="120" show-overflow-tooltip />
          <el-table-column label="操作" width="260" align="center" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="handleGenerate(row.id)">生成任务</el-button>
              <el-button size="small" type="primary" link @click.stop="openEdit(row.id)">编辑</el-button>
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
    <!-- Drawer: 新建/编辑巡检计划 -->
    <!-- ================================================================== -->
    <el-drawer
      v-model="drawerVisible"
      :title="drawerTitle"
      size="580px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Setting /></el-icon>
          <span class="dialog-section-title">基础配置</span>
        </div>
        <el-form :model="form" label-width="90px">
          <el-form-item label="工序" required>
            <el-select v-model="form.processId" filterable placeholder="选择工序" style="width: 100%">
              <el-option v-for="p in processes" :key="p.id" :label="p.name" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="设备" required>
            <div style="width: 100%">
              <!-- 按类型筛选 -->
              <el-select
                v-model="selectedTypeFilter"
                clearable
                placeholder="按设备类型筛选"
                size="small"
                style="width: 100%; margin-bottom: 8px"
              >
                <el-option
                  v-for="t in equipmentTypeOptions"
                  :key="t.value"
                  :label="t.label"
                  :value="t.value"
                />
              </el-select>
              <!-- 多选设备 -->
              <el-select
                v-model="form.equipmentIds"
                multiple
                filterable
                collapse-tags
                collapse-tags-tooltip
                :max-collapse-tags="3"
                placeholder="选择设备（可多选）"
                style="width: 100%"
              >
                <template v-if="!selectedTypeFilter">
                  <!-- 全部设备，按类型分组 -->
                  <el-option-group
                    v-for="(devices, type) in equipmentByType"
                    :key="type"
                    :label="type"
                  >
                    <el-option
                      v-for="e in devices"
                      :key="e.id"
                      :label="`${e.name}（${e.code}）`"
                      :value="e.id"
                    />
                  </el-option-group>
                </template>
                <template v-else>
                  <!-- 按选中的类型筛选 -->
                  <el-option
                    v-for="e in filteredEquipmentByType"
                    :key="e.id"
                    :label="`${e.name}（${e.code}）`"
                    :value="e.id"
                  />
                </template>
              </el-select>
              <div style="color: var(--el-text-color-secondary); font-size: 12px; margin-top: 4px;">
                已选择 {{ form.equipmentIds?.length || 0 }} 台设备
              </div>
            </div>
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Clock /></el-icon>
          <span class="dialog-section-title">巡检参数</span>
        </div>
        <el-form :model="form" label-width="110px">
          <el-form-item label="巡检间隔 (分钟)" required>
            <el-input-number v-model="form.patrolIntervalMin" :min="10" :max="1440" style="width: 100%" controls-position="right" />
          </el-form-item>
          <el-form-item label="自动生成">
            <el-switch v-model="form.autoGenerate" active-text="开启" inactive-text="关闭" />
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><User /></el-icon>
          <span class="dialog-section-title">人员信息</span>
        </div>
        <el-form :model="form" label-width="90px">
          <el-form-item label="检验员">
            <el-input v-model="form.inspector" placeholder="输入检验员姓名" clearable style="width: 100%" />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="drawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="handleSave" :loading="saving">
            保存
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
.stat-pending .stat-accent { background: var(--el-color-warning); }
.stat-inspecting .stat-accent { background: var(--el-color-info); }
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
.stat-pending .stat-value { color: var(--el-color-warning); }
.stat-inspecting .stat-value { color: var(--el-color-info); }
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

/* ── 巡检计划表格 ── */
.plans-table {
  flex: 1;
  min-height: 0;
  width: 100%;
}

.plans-table :deep(.el-table__cell) {
  white-space: nowrap;
}

.plans-table :deep(.el-table__header-wrapper) {
  flex-shrink: 0;
}

.plans-table :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.plans-table :deep(.el-table__row) {
  height: 32px;
  line-height: 32px;
}

.plans-table :deep(.el-table__header-wrapper .el-table__cell) {
  height: 32px;
  line-height: 32px;
  padding: 0 8px;
}

.plans-table :deep(.el-table__body-wrapper .el-table__cell) {
  padding: 0 8px;
}

.plans-table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.plans-table :deep(.el-button--primary.is-link:hover),
.plans-table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.plans-table :deep(.el-button--primary.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

.plans-table :deep(.el-button--danger.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.plans-table :deep(.el-button--danger.is-link:hover),
.plans-table :deep(.el-button--danger.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.plans-table :deep(.el-button--danger.is-link:focus-visible) {
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
</style>

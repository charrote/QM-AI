<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, List, Check, Clock, Delete, Document, Plus } from '@element-plus/icons-vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import { patrolApi } from '@/api/ipqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
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

// Stats
const stats = ref({ total: 0, scheduled: 0, completed: 0, missed: 0 })

const drawerVisible = ref(false)
const drawerTitle = ref('')
const patrolDetail = ref<IpqcPatrolDetail | null>(null)
const submitForm = ref<SubmitIpqcPatrol>({
  conclusion: 'pending',
  items: [],
})

const submitId = ref<number>(0)

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
async function loadStats() {
  try {
    const [allRes, schedRes, compRes, missRes] = await Promise.allSettled([
      patrolApi.list({ page: 1, pageSize: 1, keyword: undefined, status: undefined }),
      patrolApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'scheduled' }),
      patrolApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'completed' }),
      patrolApi.list({ page: 1, pageSize: 1, keyword: undefined, status: 'missed' }),
    ])
    stats.value = {
      total: allRes.status === 'fulfilled' ? allRes.value.total : 0,
      scheduled: schedRes.status === 'fulfilled' ? schedRes.value.total : 0,
      completed: compRes.status === 'fulfilled' ? compRes.value.total : 0,
      missed: missRes.status === 'fulfilled' ? missRes.value.total : 0,
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
    const res = await patrolApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch (e: any) {
    ElMessage.error('加载巡检记录失败')
  } finally {
    loading.value = false
  }
}

// ─── Patrol Submit ──────────────────────────────
async function openSubmit(id: number) {
  let detail: IpqcPatrolDetail | null = null
  try {
    detail = await patrolApi.get(id)
    if (!detail) {
      ElMessage.warning('未找到巡检记录')
      return
    }
    patrolDetail.value = detail
    drawerTitle.value = `提交巡检 - ${detail.patrolNo}`
    submitForm.value = {
      conclusion: 'pending',
      remarks: detail.remarks || '',
      items: (detail.items || []).map(i => ({
        id: i.id,
        inspectionItemId: i.inspectionItemId,
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
    // Auto-load from plans if no items
    if (!submitForm.value.items || submitForm.value.items.length === 0) {
      try {
        const plans = await inspectionPlanApi.getByContext({
          inspectionType: 'IPQC_PATROL',
          processId: detail.processId,
          equipmentId: detail.equipmentId,
        })
        if (plans.length > 0) {
          const allItems = plans.flatMap(p => p.items)
          const seen = new Set<number>()
          const newItems: IpqcPatrolItemSubmit[] = []
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
            submitForm.value.items = newItems
          }
        }
      } catch (e) {
        console.error('加载检验计划失败', e)
      }
      if (!submitForm.value.items || submitForm.value.items.length === 0) {
        submitForm.value.items = [{ itemName: '外观检查', dataType: 'visual', result: 'pending' }]
      }
    }
    submitId.value = id
    drawerVisible.value = true
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '加载巡检详情失败')
    patrolDetail.value = null
  }
}

function addPatrolItem() {
  submitForm.value.items!.push({ itemName: '', dataType: 'numeric', result: 'pending' })
}

function removePatrolItem(index: number) {
  submitForm.value.items!.splice(index, 1)
}

async function handleSubmit() {
  const id = submitId.value
  if (!id) return
  try {
    await patrolApi.submit(id, {
      conclusion: submitForm.value.conclusion,
      remarks: submitForm.value.remarks,
      items: submitForm.value.items,
    })
    ElMessage.success('巡检已提交')
    drawerVisible.value = false
    await loadData()
    await loadStats()
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
    await loadStats()
  } catch { /* cancelled */ }
}

async function viewDetail(id: number) {
  let detail: IpqcPatrolDetail | null = null
  try {
    detail = await patrolApi.get(id)
    if (!detail) {
      ElMessage.warning('未找到巡检记录')
      return
    }
    patrolDetail.value = detail
    drawerTitle.value = `巡检详情 - ${detail.patrolNo}`
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
    drawerVisible.value = true
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '加载巡检详情失败')
    patrolDetail.value = null
  }
}

onMounted(async () => {
  await loadData()
  await loadStats()
})
</script>

<template>
  <div class="page-container">
    <!-- Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><List /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">巡检记录</h2>
          <span class="page-header-banner-subtitle">查看和管理现场巡检任务执行情况</span>
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
            <div class="stat-label">巡检总数</div>
          </div>
        </div>
        <div class="stat-item stat-pending">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.scheduled }}</div>
            <div class="stat-label">待执行</div>
          </div>
        </div>
        <div class="stat-item stat-qualified">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.completed }}</div>
            <div class="stat-label">已完成</div>
          </div>
        </div>
        <div class="stat-item stat-anomaly">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.missed }}</div>
            <div class="stat-label">已错过</div>
          </div>
        </div>
      </div>

      <!-- Data Card with Toolbar -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            巡检记录列表
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索巡检编号"
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
                v-for="o in IPQC_PATROL_STATUS_OPTIONS"
                :key="o.value"
                :label="o.label"
                :value="o.value"
              />
            </el-select>
            <el-button size="small" @click="loadData">
              <el-icon><Refresh /></el-icon>刷新
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
          class="patrols-table"
        >
          <el-table-column type="index" label="序号" width="55" fixed />
          <el-table-column prop="patrolNo" label="巡检编号" min-width="160" show-overflow-tooltip />
          <el-table-column prop="planNo" label="计划编号" min-width="160" show-overflow-tooltip />
          <el-table-column prop="processName" label="工序" min-width="120" show-overflow-tooltip />
          <el-table-column prop="equipmentName" label="设备" min-width="120" show-overflow-tooltip />
          <el-table-column label="计划时间" min-width="160">
            <template #default="{ row }">
              <div class="time-cell">
                <el-icon><Clock /></el-icon>
                {{ formatDate(row.scheduledTime) }}
              </div>
            </template>
          </el-table-column>
          <el-table-column label="实际时间" min-width="160">
            <template #default="{ row }">
              <div class="time-cell">
                <el-icon><Clock /></el-icon>
                {{ formatDate(row.actualTime) }}
              </div>
            </template>
          </el-table-column>
          <el-table-column prop="totalChecked" label="检验数" width="70" align="center" />
          <el-table-column prop="totalPass" label="合格" width="60" align="center">
            <template #default="{ row }">
              <span class="stat-pass">{{ row.totalPass ?? '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="totalFail" label="不合格" width="70" align="center">
            <template #default="{ row }">
              <span :class="{ 'stat-fail': row.totalFail && row.totalFail > 0 }">{{ row.totalFail ?? '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column label="结论" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="conclusionTag(row.conclusion)" size="small" effect="plain" round>
                {{ row.conclusion === 'qualified' ? '合格' : row.conclusion === 'unqualified' ? '不合格' : '待定' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="statusTag(row.status)" size="small" effect="plain" round>{{ statusLabel(row.status) }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="240" align="center" fixed="right">
            <template #default="{ row }">
              <el-button v-if="row.status === 'scheduled'" size="small" type="primary" link @click.stop="openSubmit(row.id)">
                执行
              </el-button>
              <el-button v-else size="small" type="primary" link @click.stop="viewDetail(row.id)">详情</el-button>
              <el-button v-if="row.status === 'scheduled'" size="small" type="warning" link @click.stop="handleMiss(row.id)">跳过</el-button>
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
    <!-- RightPanel: 巡检提交/详情 -->
    <!-- ================================================================== -->
    <RightPanel v-model:visible="drawerVisible" :title="drawerTitle" :width="580">
      <template #body>
        <template v-if="patrolDetail">
          <!-- Patrol Info -->
          <div class="quick-info">
            <div class="quick-info__item">
              <div class="quick-info__label">设备</div>
              <div class="quick-info__value">{{ patrolDetail.equipmentName || '-' }}</div>
            </div>
            <div class="quick-info__item">
              <div class="quick-info__label">工序</div>
              <div class="quick-info__value">{{ patrolDetail.processName || '-' }}</div>
            </div>
            <div class="quick-info__item">
              <div class="quick-info__label">计划时间</div>
              <div class="quick-info__value" style="font-size: 13px; font-weight: 400">{{ formatDate(patrolDetail.scheduledTime) }}</div>
            </div>
            <div class="quick-info__item">
              <div class="quick-info__label">实际时间</div>
              <div class="quick-info__value" style="font-size: 13px; font-weight: 400">{{ formatDate(patrolDetail.actualTime) }}</div>
            </div>
          </div>

          <!-- Conclusion & Remarks -->
          <div class="dialog-section">
            <div class="dialog-section-header">
              <el-icon class="dialog-section-icon"><Document /></el-icon>
              <span class="dialog-section-title">检验结论</span>
            </div>
            <el-form :model="submitForm" label-width="80px">
              <el-form-item label="结论" required>
                <el-select v-model="submitForm.conclusion" style="width: 100%">
                  <el-option v-for="o in IPQC_PATROL_CONCLUSION_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                </el-select>
              </el-form-item>
              <el-form-item label="备注">
                <el-input v-model="submitForm.remarks" type="textarea" :rows="2" placeholder="备注信息" />
              </el-form-item>
            </el-form>
          </div>

          <!-- Inspection Items -->
          <div class="dialog-section">
            <div class="dialog-section-header">
              <el-icon class="dialog-section-icon"><List /></el-icon>
              <span class="dialog-section-title">检验项明细</span>
            </div>
            <div class="submit-list">
              <div v-for="(item, idx) in submitForm.items" :key="idx" class="submit-row">
                <div class="submit-index">{{ idx + 1 }}</div>
                <el-input
                  v-model="item.itemName"
                  placeholder="项目名称"
                  style="width: 160px"
                  :disabled="!!item.id"
                  size="default"
                />
                <el-select v-model="item.dataType" style="width: 110px" :disabled="!!item.id" size="default">
                  <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                </el-select>
                <el-input-number
                  v-model="item.actualValue"
                  :precision="4"
                  :step="0.1"
                  style="width: 140px"
                  controls-position="right"
                  size="default"
                />
                <el-select v-model="item.result" style="width: 110px" size="default">
                  <el-option v-for="o in INSPECTION_RESULT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
                </el-select>
                <el-button v-if="!item.id" link type="danger" size="small" :icon="Delete" @click="removePatrolItem(idx)" />
              </div>
              <el-button type="primary" link @click="addPatrolItem">
                <el-icon><Plus /></el-icon> 添加项目
              </el-button>
            </div>
          </div>
        </template>

        <!-- Empty State when patrolDetail is null -->
        <div v-else-if="drawerVisible" class="empty-state">
          <p>加载中...</p>
        </div>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="drawerVisible = false">关闭</el-button>
          <el-button size="small" type="primary" @click="handleSubmit" v-if="patrolDetail?.status === 'scheduled'">
            提交
          </el-button>
        </div>
      </template>
    </RightPanel>
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
.stat-pending .stat-accent { background: var(--el-color-warning); }
.stat-qualified .stat-accent { background: var(--el-color-success); }
.stat-anomaly .stat-accent { background: var(--el-color-danger); }
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
.stat-pending .stat-value { color: var(--el-color-warning); }
.stat-qualified .stat-value { color: var(--el-color-success); }
.stat-anomaly .stat-value { color: var(--el-color-danger); }
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

/* ── 巡检表格 ── */
.patrols-table {
  flex: 1;
  min-height: 0;
  width: 100%;
}

.patrols-table :deep(.el-table__cell) {
  white-space: nowrap;
}

.patrols-table :deep(.el-table__header-wrapper) {
  flex-shrink: 0;
}

.patrols-table :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.patrols-table :deep(.el-table__row) {
  height: 32px;
  line-height: 32px;
}

.patrols-table :deep(.el-table__header-wrapper .el-table__cell) {
  height: 32px;
  line-height: 32px;
  padding: 0 8px;
}

.patrols-table :deep(.el-table__body-wrapper .el-table__cell) {
  padding: 0 8px;
}

.patrols-table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.patrols-table :deep(.el-button--primary.is-link:hover),
.patrols-table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.patrols-table :deep(.el-button--primary.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

.patrols-table :deep(.el-button--danger.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.patrols-table :deep(.el-button--danger.is-link:hover),
.patrols-table :deep(.el-button--danger.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.patrols-table :deep(.el-button--danger.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

.time-cell {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
}

.time-cell .el-icon {
  color: var(--el-text-placeholder, #c0c4cc);
  flex-shrink: 0;
}

.stat-pass {
  color: var(--el-color-success, #67c23a);
  font-weight: 600;
}

.stat-fail {
  color: var(--el-color-danger, #f56c6c);
  font-weight: 600;
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

.quick-info {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}

.quick-info__item {
  padding: 12px 14px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  border: 1px solid var(--el-border-color-lighter);
}

.quick-info__label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-bottom: 4px;
}

.quick-info__value {
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
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

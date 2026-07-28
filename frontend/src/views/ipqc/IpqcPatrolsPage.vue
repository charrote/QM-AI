<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Refresh, List, Check, Clock, Delete, Document, Plus } from '@element-plus/icons-vue'
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

const dialogVisible = ref(false)
const dialogTitle = ref('')
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
      // Fallback default item
      if (!submitForm.value.items || submitForm.value.items.length === 0) {
        submitForm.value.items = [{ itemName: '外观检查', dataType: 'visual', result: 'pending' }]
      }
    }
    submitId.value = id
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
  const id = submitId.value
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
  <div class="page-content">
    <!-- Page Header Banner -->
    <div class="page-header-banner page-header-banner--info">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="24"><List /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h1 class="page-header-banner-title">巡检记录</h1>
          <p class="page-header-banner-subtitle">查看和管理现场巡检任务执行情况</p>
        </div>
        <div style="margin-left:auto; display:flex; gap:8px">
          <el-button :icon="Refresh" @click="loadData" :loading="loading">刷新</el-button>
        </div>
      </div>
    </div>

    <!-- Filter Bar -->
    <div class="action-bar">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索巡检编号"
        :prefix-icon="Search"
        clearable
        style="width: 260px"
        @clear="loadData"
        @keyup.enter="loadData"
      />
      <el-select
        v-model="statusFilter"
        placeholder="状态筛选"
        clearable
        style="width: 140px"
        @change="loadData"
      >
        <el-option v-for="o in IPQC_PATROL_STATUS_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
      </el-select>
    </div>

    <!-- Table Card -->
    <div class="data-card">
      <el-table
        :data="items"
        stripe
        v-loading="loading"
        class="data-card__table"
      >
        <el-table-column prop="patrolNo" label="巡检编号" min-width="170" show-overflow-tooltip />
        <el-table-column prop="planNo" label="计划编号" min-width="170" show-overflow-tooltip />
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
        <el-table-column prop="totalChecked" label="检验数" width="80" align="center" />
        <el-table-column prop="totalPass" label="合格" width="70" align="center">
          <template #default="{ row }">
            <span class="stat-pass">{{ row.totalPass ?? '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="totalFail" label="不合格" width="80" align="center">
          <template #default="{ row }">
            <span :class="{ 'stat-fail': row.totalFail && row.totalFail > 0 }">{{ row.totalFail ?? '-' }}</span>
          </template>
        </el-table-column>
        <el-table-column label="结论" width="90" align="center">
          <template #default="{ row }">
            <el-tag :type="conclusionTag(row.conclusion)" size="small" effect="dark" class="status-badge">
              {{ row.conclusion === 'qualified' ? '合格' : row.conclusion === 'unqualified' ? '不合格' : '待定' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusTag(row.status)" size="small" effect="dark" class="status-badge">{{ statusLabel(row.status) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right" align="center">
          <template #default="{ row }">
            <el-button v-if="row.status === 'scheduled'" link size="small" type="primary" @click.stop="openSubmit(row.id)">
              <el-icon><Check /></el-icon>
              执行
            </el-button>
            <el-button v-else link size="small" type="primary" @click.stop="viewDetail(row.id)">详情</el-button>
            <el-button v-if="row.status === 'scheduled'" link size="small" type="warning" @click.stop="handleMiss(row.id)">跳过</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Pagination -->
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          :sizes-layout="'first, prev, pager, next'"
          :pager-count="7"
          layout="total, sizes, prev, pager, next, jumper"
          background
          @current-change="loadData"
        />
      </div>
    </div>

    <!-- Patrol Submit / Detail Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="760px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <div v-if="dialogVisible && patrolDetail">
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
            <div class="quick-info__value" style="font-size:var(--font-base); font-weight:400">{{ formatDate(patrolDetail.scheduledTime) }}</div>
          </div>
          <div class="quick-info__item">
            <div class="quick-info__label">实际时间</div>
            <div class="quick-info__value" style="font-size:var(--font-base); font-weight:400">{{ formatDate(patrolDetail.actualTime) }}</div>
          </div>
        </div>

        <!-- Conclusion & Remarks -->
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><Document /></el-icon>
            检验结论
          </div>
          <el-form :model="submitForm" label-width="80px" size="default">
            <el-form-item label="结论">
              <el-select v-model="submitForm.conclusion" style="width: 220px">
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
            检验项明细
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
                style="width: 160px"
                controls-position="right"
                size="default"
              />
              <el-select v-model="item.result" style="width: 120px" size="default">
                <el-option v-for="o in INSPECTION_RESULT_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
              <el-button v-if="!item.id" link type="danger" size="small" :icon="Delete" @click="removePatrolItem(idx)" />
            </div>
            <el-button type="primary" link @click="addPatrolItem">
              <el-icon><Plus /></el-icon>
              添加项目
            </el-button>
          </div>
        </div>
      </div>

      <template #footer>
        <el-button @click="dialogVisible = false">关闭</el-button>
        <el-button type="primary" @click="handleSubmit" v-if="patrolDetail?.status === 'scheduled'">
          <el-icon><Check /></el-icon>
          提交
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
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
</style>
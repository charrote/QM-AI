<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import {
  Search, Refresh, Box, Plus, DocumentChecked, RefreshRight,
  Setting, Check, CircleCheck, ArrowRight, Clock, WarningFilled, InfoFilled,
} from '@element-plus/icons-vue'
import { batchApi } from '@/api/fqc'
import { productApi } from '@/api/basicData'
import type { ProductBatch, ProductBatchDetail, CreateProductBatch } from '@/types/fqc'
import type { Product, PagedRequest } from '@/types/basicData'
import { BATCH_STATUS_OPTIONS, BATCH_STATUS_MAP } from '@/types/fqc'

defineOptions({ name: 'FqcBatchesPage' })

const loading = ref(false)
const list = ref<ProductBatch[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '', status: '' })
const detailVisible = ref(false)
const detail = ref<ProductBatchDetail | null>(null)
const createVisible = ref(false)
const products = ref<Product[]>([])
const productsLoading = ref(false)
const createForm = reactive<CreateProductBatch>({
  productId: 0,
  quantity: 0,
})

// ─── 批次来源说明 ─────────────────────────────────
const BATCH_SOURCE_DESC: Record<string, { label: string; type: string }> = {
  manual: { label: '手动创建', type: 'info' },
  'ipqc-auto': { label: 'IPQC 自动流转', type: 'success' },
  'work-order': { label: '工单完工', type: 'warning' },
}

const BATCH_STATUS_ICON: Record<string, typeof InfoFilled> = {
  in_progress: Clock,
  inspected: CircleCheck,
  released: DocumentChecked,
  quarantined: WarningFilled,
}

// ─── 统计 ─────────────────────────────────────────
const stats = computed(() => {
  const s = { total: list.value.length, in_progress: 0, inspected: 0, released: 0 }
  for (const item of list.value) {
    if (item.status === 'in_progress') s.in_progress++
    else if (item.status === 'inspected') s.inspected++
    else if (item.status === 'released') s.released++
  }
  return s
})

// ─── 进度流程 ─────────────────────────────────────
type FlowStage = { key: string; label: string; active: boolean; done: boolean }
function buildFlow(detail: ProductBatchDetail): FlowStage[] {
  const s = detail.status
  return [
    { key: 'batch', label: '批次创建', active: true, done: true },
    { key: 'fqc', label: 'FQC 检验', active: s !== 'released' && s !== 'quarantined', done: s === 'released' },
    { key: 'oqc', label: 'OQC 放行', active: s === 'released' && false, done: false },
    { key: 'packaging', label: '包装确认', active: false, done: false },
  ]
}

// ─── 检验通过率 ───────────────────────────────────
function inspectionPassRate(inspections?: FqcInspection[]): number {
  if (!inspections || inspections.length === 0) return 0
  const totalChecked = inspections.reduce((sum, i) => sum + i.totalChecked, 0)
  if (totalChecked === 0) return 0
  const totalPass = inspections.reduce((sum, i) => sum + i.totalPass, 0)
  return Math.round((totalPass / totalChecked) * 100)
}

// ─── 格式化数量 ──────────────────────────────────
function formatQuantity(n: number): string {
  return n.toLocaleString('en-US')
}

// ─── 格式化日期 ──────────────────────────────────
function formatDate(d?: string): string {
  if (!d) return '-'
  return new Date(d).toISOString().slice(0, 10)
}

// ─── 数据获取 ────────────────────────────────────
async function fetchList() {
  loading.value = true
  try {
    const res = await batchApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* */ }
  finally { loading.value = false }
}

async function loadProducts() {
  productsLoading.value = true
  try {
    const res = await productApi.list({ pageSize: 200 })
    products.value = res.items
  } catch { /* */ }
  finally { productsLoading.value = false }
}

async function openDetail(id: number) {
  try {
    detail.value = await batchApi.get(id)
    detailVisible.value = true
  } catch { /* */ }
}

async function handleCreate() {
  if (!createForm.productId) {
    ElMessage.warning('请选择产品')
    return
  }
  if (!createForm.quantity || createForm.quantity <= 0) {
    ElMessage.warning('请输入有效数量')
    return
  }
  try {
    await batchApi.create(createForm)
    ElMessage.success('批次创建成功')
    createVisible.value = false
    await fetchList()
  } catch { /* */ }
}

async function handleGenerateNumber() {
  try {
    const res = await batchApi.generateNumber()
    createForm.batchCode = res.batchCode
  } catch { /* */ }
}

function openCreate() {
  createForm.batchCode = ''
  createForm.productId = 0
  createForm.quantity = 0
  createForm.workOrderId = undefined
  if (products.value.length === 0) loadProducts()
  createVisible.value = true
}

function statusTag(status: string): string {
  const map: Record<string, string> = {
    in_progress: 'primary', inspected: 'warning', released: 'success', quarantined: 'danger',
  }
  return map[status] || 'info'
}
</script>

<template>
  <div class="iqc-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--success">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Box /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">批次管理</h2>
          <span class="page-header-banner-subtitle">管理成品检验批次，从创建到放行全流程追踪</span>
        </div>
      </div>
    </div>

    <!-- Stats Bar -->
    <div class="stats-bar">
      <div class="stat-item stat-total">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.total }}</div>
          <div class="stat-label">总批次数</div>
        </div>
      </div>
      <div class="stat-item stat-primary">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.in_progress }}</div>
          <div class="stat-label">进行中</div>
        </div>
      </div>
      <div class="stat-item stat-warning">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.inspected }}</div>
          <div class="stat-label">已检验</div>
        </div>
      </div>
      <div class="stat-item stat-success">
        <div class="stat-accent"></div>
        <div class="stat-content">
          <div class="stat-value">{{ stats.released }}</div>
          <div class="stat-label">已放行</div>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="data-card">
      <div class="data-card__header">
        <span class="data-card__title">
          批次清单
          <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
        </span>
        <div class="data-card__actions">
          <el-input
            v-model="query.keyword"
            placeholder="搜索批次号/产品"
            :prefix-icon="Search"
            clearable
            size="small"
            style="width: 220px"
            @clear="fetchList"
            @keyup.enter="fetchList"
          />
          <el-select
            v-model="query.status"
            placeholder="批次状态"
            clearable
            size="small"
            style="width: 120px"
            @change="fetchList"
          >
            <el-option v-for="opt in BATCH_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-button size="small" @click="fetchList">
            <el-icon><Refresh /></el-icon>刷新
          </el-button>
          <el-button type="primary" size="small" @click="openCreate">
            <el-icon><Plus /></el-icon>新建批次
          </el-button>
        </div>
      </div>

      <div class="data-card__body">
        <el-table
          :data="list"
          border
          stripe
          size="small"
          class="data-card__table"
          v-loading="loading"
          style="width: 100%"
        >
          <el-table-column prop="batchCode" label="批次号" min-width="160" show-overflow-tooltip>
            <template #default="{ row }">
              <el-button link type="primary" class="data-card__batch-code" @click="openDetail(row.id)">
                {{ row.batchCode }}
              </el-button>
            </template>
          </el-table-column>
          <el-table-column prop="productName" label="产品名称" min-width="150" show-overflow-tooltip />
          <el-table-column prop="quantity" label="数量" width="90" align="center">
            <template #default="{ row }">
              <span class="data-card__quantity">{{ formatQuantity(row.quantity) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="110" align="center">
            <template #default="{ row }">
              <el-tag
                :type="statusTag(row.status)"
                size="small"
                :effect="row.status === 'in_progress' ? 'dark' : 'dark'"
                :class="{ 'data-card__status--pulse': row.status === 'in_progress' }"
              >
                <el-icon class="data-card__status-icon"><component :is="BATCH_STATUS_ICON[row.status] || InfoFilled" /></el-icon>
                {{ BATCH_STATUS_MAP[row.status] || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="来源" width="130" align="center">
            <template #default="{ row }">
              <el-tag size="small" type="info" effect="plain">{{ BATCH_SOURCE_DESC[row.source]?.label || '手动创建' }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="createdAt" label="创建时间" min-width="150" />
          <el-table-column label="操作" width="100" fixed="right" align="center">
            <template #default="{ row }">
              <el-button link size="small" type="primary" @click.stop="openDetail(row.id)">
                <el-icon><DocumentChecked /></el-icon>
                详情
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Empty State -->
        <el-empty
          v-if="!loading && list.length === 0"
          description="暂无批次数据"
          :image-size="100"
          class="data-card__empty"
        >
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>
            新建批次
          </el-button>
        </el-empty>

        <!-- Pagination -->
        <div class="data-card__footer" v-if="total > query.pageSize">
          <div class="data-card__pagination">
            <el-pagination
              v-model:current-page="query.page"
              v-model:page-size="query.pageSize"
              :total="total"
              :page-sizes="[10, 20, 50, 100]"
              :sizes-layout="'first, prev, pager, next'"
              :pager-count="7"
              layout="total, sizes, prev, pager, next, jumper"
              background
              @change="fetchList"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- 详情抽屉 -->
    <el-drawer v-model="detailVisible" title="批次详情" size="680px" :with-header="true" destroy-on-close>
      <template v-if="detail">
        <!-- 进度流程 -->
        <div class="flow-steps" v-if="detail">
          <div
            v-for="(stage, index) in buildFlow(detail)"
            :key="stage.key"
            class="flow-step"
            :class="{
              'flow-step--active': stage.active,
              'flow-step--done': stage.done,
            }"
          >
            <div class="flow-step__indicator">
              <el-icon v-if="stage.done" color="var(--el-color-success)"><CircleCheck /></el-icon>
              <el-icon v-else-if="stage.active" color="var(--el-color-primary)" class="flow-step__indicator--clock"><Clock /></el-icon>
              <span v-else class="flow-step__dot"></span>
            </div>
            <span
              class="flow-step__label"
              :class="{ 'flow-step__label--active': stage.active }"
            >{{ stage.label }}</span>
            <el-icon v-if="index < buildFlow(detail).length - 1" class="flow-step__connector"><ArrowRight /></el-icon>
          </div>
        </div>

        <!-- Detail Header -->
        <div class="drawer-header" style="margin-bottom: 16px;">
          <div class="drawer-header-icon">
            <el-icon :size="18"><Box /></el-icon>
          </div>
          <div class="drawer-header-text">
            <span class="drawer-title">{{ detail.batchCode }}</span>
            <span class="drawer-subtitle">
              <el-tag
                :type="statusTag(detail.status)"
                size="small"
                :effect="detail.status === 'in_progress' ? 'dark' : 'dark'"
                :class="{ 'data-card__status--pulse': detail.status === 'in_progress' }"
                style="margin-right: 8px;"
              >
                <el-icon style="margin-right: 3px; font-size: 12px;"><component :is="BATCH_STATUS_ICON[detail.status] || InfoFilled" /></el-icon>
                {{ BATCH_STATUS_MAP[detail.status] }}
              </el-tag>
              <el-tag v-if="detail.source" size="small" type="info" effect="plain">
                {{ BATCH_SOURCE_DESC[detail.source]?.label || detail.source }}
              </el-tag>
            </span>
          </div>
        </div>

        <el-descriptions :column="2" border style="margin-bottom: 20px;">
          <el-descriptions-item label="产品名称">{{ detail.productName }}</el-descriptions-item>
          <el-descriptions-item label="数量">{{ formatQuantity(detail.quantity) }}</el-descriptions-item>
          <el-descriptions-item label="关联工单">{{ detail.workOrderId || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatDate(detail.createdAt) }}</el-descriptions-item>
        </el-descriptions>

        <!-- 快速操作 -->
        <div v-if="detail.status === 'in_progress'" class="detail-quick-actions">
          <el-button type="primary" size="small" @click="$router.push({ name: 'FqcInspections', query: { batchId: detail.id } })">
            <el-icon><Plus /></el-icon>
            创建 FQC 检验单
          </el-button>
        </div>

        <!-- 检验记录 -->
        <div class="drawer-section-header">
          <el-icon class="drawer-section-icon"><DocumentChecked /></el-icon>
          检验记录
          <el-tag size="small" type="success" effect="plain" v-if="detail.inspections?.length">
            {{ detail.inspections.length }} 条
          </el-tag>
          <el-tag size="small" type="warning" effect="plain" v-if="detail.inspections?.length">
            合格率 {{ inspectionPassRate(detail.inspections) }}%
          </el-tag>
        </div>
        <el-table :data="detail.inspections || []" border size="small" v-if="detail.inspections?.length">
          <el-table-column prop="inspectionNo" label="检验单号" min-width="160" />
          <el-table-column prop="inspectorName" label="检验员" width="100" />
          <el-table-column label="结论" width="90" align="center">
            <template #default="{ row }">
              <el-tag
                :type="row.conclusion === 'qualified' ? 'success' : row.conclusion === 'unqualified' ? 'danger' : 'info'"
                :effect="row.conclusion === 'qualified' ? 'dark' : 'plain'"
                size="small"
              >
                {{ row.conclusion === 'qualified' ? '合格' : row.conclusion === 'unqualified' ? '不合格' : '待检' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="合格/总数" width="100" align="center">
            <template #default="{ row }">
              <span class="detail-table__pass">{{ row.totalPass }}</span>
              <span class="detail-table__sep">/</span>
              <span class="detail-table__total">{{ row.totalChecked }}</span>
            </template>
          </el-table-column>
          <el-table-column prop="totalFail" label="不合格数" width="80" align="center" />
          <el-table-column prop="checkedAt" label="检验时间" min-width="140" />
        </el-table>
        <el-empty v-else description="暂无检验记录" :image-size="50" />

        <!-- 放行记录 -->
        <div class="drawer-section-header">
          <el-icon class="drawer-section-icon"><RefreshRight /></el-icon>
          放行记录
          <el-tag size="small" type="success" effect="plain" v-if="detail.releases?.length">
            {{ detail.releases.length }} 条
          </el-tag>
        </div>
        <el-table :data="detail.releases || []" border size="small" v-if="detail.releases?.length">
          <el-table-column prop="releaseNumber" label="放行单号" min-width="160" />
          <el-table-column prop="customerName" label="客户" min-width="130" />
          <el-table-column label="状态" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="statusTag(row.status)" size="small" effect="dark">
                {{ row.status }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
        <el-empty v-else description="暂无放行记录" :image-size="50" />
      </template>
    </el-drawer>

    <!-- 新建批次对话框 -->
    <el-dialog v-model="createVisible" title="新建成品批次" width="560px" :close-on-click-modal="false" top="6vh">
      <div class="create-dialog__hint" style="margin-bottom: 16px; padding: 8px 12px; background: var(--el-color-info-light-9, #ecf5ff); border-radius: 6px; font-size: 12px; color: var(--el-color-info); display: flex; gap: 6px; align-items: flex-start;">
        <el-icon :size="16" style="flex-shrink: 0; margin-top: 1px;"><InfoFilled /></el-icon>
        <span>手动创建批次为兜底方式。常规流程：<strong>IPQC 关单</strong> 或 <strong>工单完工</strong> → 自动生成批次 → 自动进入 FQC 检验队列。</span>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Box /></el-icon>
          批次信息
        </div>
        <el-form :model="createForm" label-width="90px" label-position="left">
          <el-form-item label="批次号">
            <div style="display: flex; gap: 8px; width: 100%">
              <el-input
                v-model="createForm.batchCode"
                placeholder="留空自动生成 LOT-YYYYMMDD-X"
                style="flex: 1"
                @blur="handleGenerateNumber"
              />
              <el-button @click="handleGenerateNumber">自动生成</el-button>
            </div>
            <div v-if="createForm.batchCode" class="create-dialog__code-preview">
              <el-icon color="var(--el-color-success)"><CircleCheck /></el-icon>
              预览：<strong>{{ createForm.batchCode }}</strong>
            </div>
          </el-form-item>
          <el-form-item label="产品" required>
            <el-select
              v-model="createForm.productId"
              placeholder="请输入关键词搜索产品"
              style="width:100%"
              :loading="productsLoading"
              filterable
              clearable
            >
              <el-option
                v-for="p in products"
                :key="p.id"
                :label="`${p.code} - ${p.name}`"
                :value="p.id"
              />
            </el-select>
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Setting /></el-icon>
          生产信息
        </div>
        <el-form :model="createForm" label-width="90px" label-position="left">
          <el-form-item label="关联工单">
            <el-input-number v-model="createForm.workOrderId" :min="0" :max="99999" placeholder="可选" style="width:100%" />
          </el-form-item>
          <el-form-item label="数量" required>
            <el-input-number v-model="createForm.quantity" :min="1" :max="999999" :precision="0" style="width:100%" />
            <div class="create-dialog__qty-hint">请输入大于 0 的有效数量</div>
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Check /></el-icon>
          创建批次
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.iqc-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.iqc-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  overflow-y: auto;
}

/* ── Stats Bar ── */
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
  border: 1px solid var(--el-border-color-lighter);
  border-radius: var(--radius-lg, 8px);
  padding: 16px 20px;
  gap: 12px;
  transition: all var(--duration-normal, 0.3s) var(--ease-out, ease-out);
  position: relative;
  overflow: hidden;
}

.stat-item::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  border-radius: 4px 0 0 4px;
  background: var(--el-border-color-lighter);
}

.stat-total::before { background: var(--el-color-info); }
.stat-primary::before { background: var(--el-color-primary); }
.stat-warning::before { background: var(--el-color-warning); }
.stat-success::before { background: var(--el-color-success); }

.stat-item:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md, 0 4px 12px rgba(0, 0, 0, 0.08));
}

.stat-accent {
  width: 4px;
  height: 36px;
  border-radius: 2px;
  flex-shrink: 0;
}

.stat-content {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  line-height: 1.2;
}

.stat-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}

/* ── Status Pulse ── */
.data-card__status--pulse {
  animation: status-pulse 2s ease-in-out infinite;
}

@keyframes status-pulse {
  0%, 100% { box-shadow: 0 0 0 0 rgba(64, 158, 255, 0.3); }
  50% { box-shadow: 0 0 0 4px rgba(64, 158, 255, 0.1); }
}

.data-card__empty {
  padding: 40px 0;
}

/* ─── Flow Steps ───────────────────── */
.flow-steps {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 16px 0;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

.flow-step {
  display: flex;
  align-items: center;
  gap: 6px;
}

.flow-step__indicator {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-border-color-lighter, #ebeef5);
  color: var(--el-text-placeholder, #c0c4cc);
  flex-shrink: 0;
  transition: all 0.3s;
}

.flow-step__indicator--clock {
  animation: flow-clock-spin 3s linear infinite;
}

@keyframes flow-clock-spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.flow-step__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--el-border-color-lighter, #ebeef5);
}

.flow-step--done .flow-step__indicator {
  background: var(--el-color-success-light-9, #f0f9eb);
  color: var(--el-color-success, #67c23a);
}

.flow-step--active .flow-step__indicator {
  background: var(--el-color-primary-light-9, #ecf5ff);
  color: var(--el-color-primary, #409eff);
}

.flow-step__label {
  font-size: 13px;
  color: var(--el-text-placeholder, #c0c4cc);
  font-weight: 500;
  white-space: nowrap;
}

.flow-step__label--active {
  color: var(--el-color-primary, #409eff);
  font-weight: 600;
}

.flow-step--done .flow-step__label {
  color: var(--el-text-secondary, #909399);
}

.flow-step__connector {
  color: var(--el-border-color-light, #dcdfe6);
  font-size: 14px;
  flex-shrink: 0;
}

/* ─── Dialog ──────────────────────── */
.create-dialog__code-preview {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-top: 6px;
  font-size: 12px;
  color: var(--el-color-success, #67c23a);
}

.create-dialog__qty-hint {
  font-size: 12px;
  color: var(--el-text-placeholder, #c0c4cc);
  margin-top: 4px;
}

.detail-quick-actions {
  display: flex;
  gap: 8px;
  margin-bottom: 20px;
}

.detail-table__pass {
  color: var(--el-color-success, #67c23a);
  font-weight: 600;
}

.detail-table__sep {
  color: var(--el-text-placeholder, #c0c4cc);
  margin: 0 2px;
}

.detail-table__total {
  color: var(--el-text-regular, #606266);
}
</style>

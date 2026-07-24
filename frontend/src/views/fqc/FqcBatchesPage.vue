<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { Search, Refresh, Box, Plus, DocumentChecked, RefreshRight } from '@element-plus/icons-vue'
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
  // 打开对话框时同步加载产品列表
  if (products.value.length === 0) loadProducts()
  createVisible.value = true
}

function statusTag(status: string): string {
  const map: Record<string, string> = {
    in_progress: 'primary', inspected: 'warning', released: 'success', quarantined: 'danger',
  }
  return map[status] || 'info'
}

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__icon-wrapper">
        <el-icon :size="28"><Box /></el-icon>
      </div>
      <div class="page-header__info">
        <h1 class="page-header__title">批次管理</h1>
        <p class="page-header__subtitle">管理成品检验批次，从创建到放行全流程追踪</p>
      </div>
    </div>

    <!-- Batch Source Alert -->
    <div class="source-banner">
      <el-icon :size="18" color="var(--el-color-info)"><DocumentChecked /></el-icon>
      <span class="source-banner__text">批次来源：</span>
      <el-tag size="small" type="success" effect="plain" class="source-tag">IPQC 关单自动生成</el-tag>
      <el-tag size="small" type="warning" effect="plain" class="source-tag">工单完工自动生成</el-tag>
      <el-tag size="small" type="info" effect="plain" class="source-tag">手动创建（兜底）</el-tag>
      <span class="source-banner__flow">→ FQC 成品检验 → OQC 出货放行</span>
    </div>

    <!-- Toolbar -->
    <div class="action-bar">
      <div class="action-bar__left">
        <el-input
          v-model="query.keyword"
          placeholder="搜索批次号/产品"
          :prefix-icon="Search"
          clearable
          style="width: 260px"
          @clear="fetchList"
          @keyup.enter="fetchList"
        />
        <el-select
          v-model="query.status"
          placeholder="批次状态"
          clearable
          style="width: 140px"
          @change="fetchList"
        >
          <el-option v-for="opt in BATCH_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
      <div class="action-bar__right">
        <el-button type="primary" @click="openCreate">
          <el-icon><Plus /></el-icon>
          新建批次
        </el-button>
      </div>
    </div>

    <!-- Table Card -->
    <div class="data-card">
      <el-table
        :data="list"
        v-loading="loading"
        :row-class-name="() => 'data-card__row'"
        style="width: 100%"
      >
        <el-table-column prop="batchCode" label="批次号" min-width="160" show-overflow-tooltip />
        <el-table-column prop="productName" label="产品名称" min-width="150" show-overflow-tooltip />
        <el-table-column prop="quantity" label="数量" width="80" align="center" />
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusTag(row.status)" size="small" effect="dark">{{ BATCH_STATUS_MAP[row.status] || row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="来源" width="120" align="center">
          <template #default="{ row }">
            <el-tag size="small" type="info" effect="plain">{{ BATCH_SOURCE_DESC[row.batchSource]?.label || '手动创建' }}</el-tag>
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

      <!-- Pagination -->
      <div class="data-card__footer">
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

    <!-- 详情抽屉 -->
    <el-drawer v-model="detailVisible" title="批次详情" size="640px" :with-header="true" destroy-on-close>
      <template v-if="detail">
        <div class="detail-header">
          <el-tag :type="statusTag(detail.status)" size="large" effect="dark">{{ BATCH_STATUS_MAP[detail.status] }}</el-tag>
          <span class="detail-batch-code">{{ detail.batchCode }}</span>
        </div>

        <el-descriptions :column="2" border class="detail-descriptions">
          <el-descriptions-item label="产品名称">{{ detail.productName }}</el-descriptions-item>
          <el-descriptions-item label="数量">{{ detail.quantity }}</el-descriptions-item>
          <el-descriptions-item label="关联工单">{{ detail.workOrderId || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ detail.createdAt }}</el-descriptions-item>
        </el-descriptions>

        <h4 class="detail-section-title">
          <el-icon color="var(--el-color-primary)"><DocumentChecked /></el-icon>
          检验记录
        </h4>
        <el-table :data="detail.inspections || []" border size="small">
          <el-table-column prop="inspectionNo" label="检验单号" min-width="160" />
          <el-table-column label="结论" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="row.conclusion === 'qualified' ? 'success' : row.conclusion === 'unqualified' ? 'danger' : 'info'" size="small">
                {{ row.conclusion === 'qualified' ? '合格' : row.conclusion === 'unqualified' ? '不合格' : '待检' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="totalPass" label="合格数" width="80" align="center" />
          <el-table-column prop="totalFail" label="不合格数" width="90" align="center" />
          <el-table-column prop="checkedAt" label="检验时间" min-width="150" />
        </el-table>

        <h4 class="detail-section-title">
          <el-icon color="var(--el-color-success)"><RefreshRight /></el-icon>
          放行记录
        </h4>
        <el-table :data="detail.releases || []" border size="small">
          <el-table-column prop="releaseNumber" label="放行单号" min-width="160" />
          <el-table-column prop="customerName" label="客户" min-width="130" />
          <el-table-column label="状态" width="80" align="center">
            <template #default="{ row }">
              <el-tag size="small">{{ row.status }}</el-tag>
            </template>
          </el-table-column>
        </el-table>
      </template>
    </el-drawer>

    <!-- 新建批次对话框 -->
    <el-dialog v-model="createVisible" title="新建成品批次" width="560px" :close-on-click-modal="false" top="6vh">
      <div v-if="createVisible">
        <div class="source-banner" style="margin-bottom: 16px">
          <el-icon :size="18" color="var(--el-color-info)"><DocumentChecked /></el-icon>
          <span class="source-banner__text">手动创建批次为兜底方式。</span>
          <span>常规流程：<strong>IPQC 关单</strong> 或 <strong>工单完工</strong> → 自动生成批次 → 自动进入 FQC 检验队列。</span>
        </div>

        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Box /></el-icon>
            <span>批次信息</span>
          </div>
          <el-form :model="createForm" label-width="90px" label-position="left">
            <el-form-item label="批次号">
              <div style="display: flex; gap: 8px; width: 100%">
                <el-input v-model="createForm.batchCode" placeholder="留空自动生成 LOT-YYYYMMDD-X" style="flex: 1" />
                <el-button @click="handleGenerateNumber">自动生成</el-button>
              </div>
            </el-form-item>
            <el-form-item label="产品" required>
              <el-select
                v-model="createForm.productId"
                placeholder="请选择产品"
                style="width:100%"
                :loading="productsLoading"
                filterable
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
          <div class="dialog-section__title">
            <el-icon><Setting /></el-icon>
            <span>生产信息</span>
          </div>
          <el-form :model="createForm" label-width="90px" label-position="left">
            <el-form-item label="关联工单">
              <el-input-number v-model="createForm.workOrderId" :min="0" :max="99999" placeholder="可选" style="width:100%" />
            </el-form-item>
            <el-form-item label="数量" required>
              <el-input-number v-model="createForm.quantity" :min="1" :max="999999" style="width:100%" />
            </el-form-item>
          </el-form>
        </div>
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

<script lang="ts">
import { Setting, Check } from '@element-plus/icons-vue'
</script>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 16px;
}

/* ─── Page Header ──────────────────── */
.page-header {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 0 4px;
}

.page-header__icon-wrapper {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: var(--el-color-success-light-9, #f0f9eb);
  color: var(--el-color-success, #67c23a);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.page-header__info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.page-header__title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  line-height: 1.3;
}

.page-header__subtitle {
  margin: 0;
  font-size: 13px;
  color: var(--text-secondary, #909399);
  line-height: 1.4;
}

/* ─── Source Banner ────────────────── */
.source-banner {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  padding: 10px 16px;
  background: var(--el-color-info-light-9, #ecf5ff);
  border-radius: 8px;
  font-size: 13px;
  color: var(--el-text-secondary, #909399);
}

.source-banner__text {
  font-weight: 500;
  color: var(--el-text-regular, #606266);
}

.source-banner__flow {
  font-weight: 500;
  color: var(--el-color-primary, #409eff);
}

.source-tag {
  margin: 0;
}

/* ─── Action Bar ───────────────────── */
.action-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.action-bar__left {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.action-bar__right {
  display: flex;
  gap: 8px;
}

/* ─── Data Card ────────────────────── */
.data-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: var(--bg-card, #fff);
  border-radius: 8px;
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
  overflow: hidden;
}

.data-card__row {
  transition: background-color 0.2s;
}

.data-card__row:hover {
  background-color: var(--el-fill-color-light, #f5f7fa) !important;
}

.data-card__footer {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
  border-top: 1px solid var(--el-border-color-lighter, #ebeef5);
}

/* ─── Detail Drawer ────────────────── */
.detail-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.detail-batch-code {
  font-size: 18px;
  font-weight: 600;
  color: var(--text-primary, #303133);
}

.detail-descriptions {
  margin-bottom: 24px;
}

.detail-section-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

/* ─── Dialog Sections ──────────────── */
.dialog-section {
  margin-bottom: 20px;
}

.dialog-section__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

.dialog-section__title .el-icon {
  color: var(--el-color-primary, #409eff);
}
</style>
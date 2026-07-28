<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Search, Refresh, Box, Plus, PriceTag, Setting, Check, DocumentChecked,
  ArrowRight,
} from '@element-plus/icons-vue'
import { packagingApi, batchApi } from '@/api/fqc'
import type { PackagingConfirmation, CreatePackagingConfirmation } from '@/types/fqc'
import type { PagedRequest } from '@/types/basicData'
import type { ProductBatch } from '@/types/fqc'
import { useAuthStore } from '@/stores/authStore'

defineOptions({ name: 'FqcPackagingPage' })

const authStore = useAuthStore()

const loading = ref(false)
const list = ref<PackagingConfirmation[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '', status: '' })
const createVisible = ref(false)
const createForm = reactive<CreatePackagingConfirmation>({
  batchId: 0,
  packagingMethod: '',
  labelPrinted: false,
  confirmedBy: authStore.user?.id || 0,
})

// ─── Batch select for create dialog ───
const batchOptions = ref<Array<{ value: number; label: string; batchCode: string; productName: string }>>([])
const batchSearchQuery = ref('')
const batchSelectLoading = ref(false)

const PACKAGING_METHOD_OPTIONS = [
  { value: '纸箱', label: '纸箱 (Cardboard Box)' },
  { value: '木箱', label: '木箱 (Wooden Crate)' },
  { value: '托盘', label: '托盘 (Pallet)' },
  { value: '编织袋', label: '编织袋 (Woven Bag)' },
  { value: '自定义', label: '自定义 (Custom)' },
]

const isCustomMethod = computed(() => createForm.packagingMethod === '自定义')

const totalQty = computed(() => {
  const qty = createForm.qtyPerBox || 0
  const boxes = createForm.totalBoxes || 0
  return qty * boxes
})

async function fetchList() {
  loading.value = true
  try {
    const res = await packagingApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* */ }
  finally { loading.value = false }
}

async function fetchBatchOptions() {
  batchSelectLoading.value = true
  try {
    const res = await batchApi.list({ page: 1, pageSize: 200 })
    batchOptions.value = (res.items as ProductBatch[]).map(b => ({
      value: b.id,
      label: `${b.batchCode || b.id} — ${b.productName || ''}`.trim(),
      batchCode: b.batchCode || String(b.id),
      productName: b.productName || '',
    }))
  } catch { /* */ }
  finally { batchSelectLoading.value = false }
}

async function handleCreate() {
  if (!createForm.batchId || !createForm.packagingMethod) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    await packagingApi.create(createForm)
    ElMessage.success('包装确认成功')
    createVisible.value = false
    await fetchList()
  } catch { /* */ }
}

async function toggleLabelPrinted(row: PackagingConfirmation) {
  try {
    await ElMessageBox.confirm(
      `确认${row.labelPrinted ? '取消' : '标记'}标签打印？`,
      '标签打印操作',
      {
        confirmButtonText: '确认',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
    await packagingApi.updateLabelPrinted(row.id, !row.labelPrinted)
    row.labelPrinted = !row.labelPrinted
    ElMessage.success(row.labelPrinted ? '标签已标记打印' : '标签已取消打印')
  } catch { /* */ }
}

const totalLabelsPrinted = computed(() =>
  list.value.filter(r => r.labelPrinted).length,
)

onMounted(() => {
  fetchList()
  fetchBatchOptions()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__icon-wrapper">
        <el-icon :size="28"><Box /></el-icon>
      </div>
      <div class="page-header__info">
        <h1 class="page-header__title">包装确认</h1>
        <p class="page-header__subtitle">管理成品包装信息，确认包装方式与标签打印</p>
      </div>
    </div>

    <!-- Flow Banner -->
    <div class="flow-banner">
      <span class="flow-banner__step"><el-icon><DocumentChecked /></el-icon> OQC 放行通过</span>
      <el-icon class="flow-banner__arrow"><ArrowRight /></el-icon>
      <span class="flow-banner__step"><el-icon><Box /></el-icon> 包装确认</span>
      <el-icon class="flow-banner__arrow"><ArrowRight /></el-icon>
      <span class="flow-banner__step"><el-icon><PriceTag /></el-icon> 标签打印</span>
      <el-icon class="flow-banner__arrow"><ArrowRight /></el-icon>
      <span class="flow-banner__step"><el-icon><Check /></el-icon> 发货</span>
    </div>

    <!-- Summary Metrics -->
    <div class="metrics-row">
      <div class="metrics-row__item">
        <div class="metrics-row__label">包装记录总数</div>
        <div class="metrics-row__value">{{ total }}</div>
      </div>
      <div class="metrics-row__item metrics-row__item--accent">
        <div class="metrics-row__label">已打印标签</div>
        <div class="metrics-row__value">{{ totalLabelsPrinted }}</div>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="action-bar">
      <div class="action-bar__left">
        <el-input
          v-model="query.keyword"
          placeholder="搜索批次号/包装方式"
          :prefix-icon="Search"
          clearable
          style="width: 260px"
          @clear="fetchList"
          @keyup.enter="fetchList"
        />
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
      <div class="action-bar__right">
        <el-button type="primary" @click="createVisible = true">
          <el-icon><Plus /></el-icon>
          包装确认
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
        <el-table-column prop="packagingMethod" label="包装方式" min-width="140" show-overflow-tooltip />
        <el-table-column prop="qtyPerBox" label="每箱数量" width="90" align="center" />
        <el-table-column prop="totalBoxes" label="总箱数" width="80" align="center" />
        <el-table-column label="总数量" width="90" align="center">
          <template #default="{ row }">
            {{ (row.qtyPerBox || 0) * (row.totalBoxes || 0) }}
          </template>
        </el-table-column>
        <el-table-column label="标签打印" width="130" align="center">
          <template #default="{ row }">
            <div class="label-printed-cell">
              <el-switch
                :model-value="row.labelPrinted"
                inline-prompt
                active-text="已"
                inactive-text="未"
                :before-change="async () => {
                  try {
                    await ElMessageBox.confirm(
                      `确认${row.labelPrinted ? '取消' : '标记'}标签打印？`,
                      '标签打印操作',
                      { confirmButtonText: '确认', cancelButtonText: '取消', type: 'warning' }
                    )
                    return true
                  } catch {
                    return false
                  }
                }"
                @change="() => toggleLabelPrinted(row)"
                style="--el-switch-on-color: #67c23a; --el-switch-off-color: #dcdfe6"
              />
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="confirmedByName" label="确认人" min-width="110" show-overflow-tooltip />
        <el-table-column prop="confirmedAt" label="确认时间" min-width="150" show-overflow-tooltip />
        <el-table-column label="操作" width="120" fixed="right" align="center">
          <template #default="{ row }">
            <el-button
              link
              size="small"
              :type="row.labelPrinted ? 'warning' : 'primary'"
              @click.stop="toggleLabelPrinted(row)"
            >
              <el-icon><PriceTag /></el-icon>
              {{ row.labelPrinted ? '取消打印' : '打印标签' }}
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- Empty State -->
      <div v-if="!loading && list.length === 0" class="empty-state">
        <el-icon :size="48" color="var(--el-text-placeholder, #c0c4cc)"><Box /></el-icon>
        <p class="empty-state__text">暂无包装确认记录</p>
        <el-button type="primary" @click="createVisible = true">
          <el-icon><Plus /></el-icon>
          新建包装确认
        </el-button>
      </div>

      <!-- Pagination -->
      <div v-if="list.length > 0" class="data-card__footer">
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

    <!-- 包装确认对话框 -->
    <el-dialog v-model="createVisible" title="包装确认" width="540px" :close-on-click-modal="false" top="6vh">
      <div v-if="createVisible">
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Box /></el-icon>
            <span>包装信息</span>
          </div>
          <el-form :model="createForm" label-width="90px" label-position="left">
            <el-form-item label="批次" required>
              <el-select
                v-model="createForm.batchId"
                filterable
                clearable
                :loading="batchSelectLoading"
                placeholder="搜索批次号/产品名"
                style="width: 100%"
                @change="batchSearchQuery = ''"
              >
                <el-option
                  v-for="opt in batchOptions"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                >
                  <span>{{ opt.batchCode }}</span>
                  <span style="margin-left: 8px; color: var(--el-text-secondary, #909399);">— {{ opt.productName }}</span>
                </el-option>
              </el-select>
            </el-form-item>
            <el-form-item label="包装方式" required>
              <el-select
                v-model="createForm.packagingMethod"
                placeholder="请选择包装方式"
                style="width: 100%"
              >
                <el-option
                  v-for="opt in PACKAGING_METHOD_OPTIONS"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>
            </el-form-item>
            <el-form-item v-if="isCustomMethod" label="自定义方式">
              <el-input
                v-model="createForm.packagingMethod"
                placeholder="请输入自定义包装方式"
                clearable
                style="width: 100%"
              />
            </el-form-item>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Setting /></el-icon>
            <span>包装参数</span>
          </div>
          <el-form :model="createForm" label-width="90px" label-position="left">
            <el-form-item label="每箱数量">
              <el-input-number v-model="createForm.qtyPerBox" :min="1" style="width: 100%" />
            </el-form-item>
            <el-form-item label="总箱数">
              <el-input-number v-model="createForm.totalBoxes" :min="1" style="width: 100%" />
            </el-form-item>
            <el-form-item label="总数量">
              <div class="total-calc-display">{{ totalQty }}</div>
            </el-form-item>
            <el-form-item label="标签打印">
              <el-switch v-model="createForm.labelPrinted" active-text="开启" inactive-text="关闭" />
            </el-form-item>
          </el-form>
        </div>
      </div>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Check /></el-icon>
          确认
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

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

/* ─── Flow Banner ──────────────────── */
.flow-banner {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 10px 16px;
  background: var(--el-color-success-light-9, #f0f9eb);
  border-radius: 8px;
  font-size: 13px;
}

.flow-banner__step {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--el-text-regular, #606266);
  white-space: nowrap;
}

.flow-banner__arrow {
  color: var(--el-color-info, #909399);
  flex-shrink: 0;
}

/* ─── Metrics Row ──────────────────── */
.metrics-row {
  display: flex;
  align-items: stretch;
  gap: 16px;
}

.metrics-row__item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 12px 16px;
  background: var(--bg-card, #fff);
  border-radius: 8px;
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
  min-width: 140px;
}

.metrics-row__label {
  font-size: 12px;
  color: var(--el-text-secondary, #909399);
  margin-bottom: 4px;
}

.metrics-row__value {
  font-size: 24px;
  font-weight: 700;
  color: var(--el-text-primary, #303133);
  line-height: 1;
}

.metrics-row__item--accent {
  background: var(--el-color-success-light-9, #f0f9eb);
}

.metrics-row__item--accent .metrics-row__value {
  color: var(--el-color-success, #67c23a);
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

/* ─── Label Printed Cell ───────────── */
.label-printed-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

/* ─── Empty State ──────────────────── */
.empty-state {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 24px;
  gap: 12px;
}

.empty-state__text {
  margin: 0;
  font-size: 14px;
  color: var(--el-text-secondary, #909399);
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

/* ─── Total Calculation Display ────── */
.total-calc-display {
  font-size: 18px;
  font-weight: 700;
  color: var(--el-color-success, #67c23a);
  line-height: 32px;
}
</style>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Search, Refresh, Box, Plus, PriceTag, Setting, Check,
} from '@element-plus/icons-vue'
import { packagingApi, batchApi } from '@/api/fqc'
import type { PackagingConfirmation, CreatePackagingConfirmation } from '@/types/fqc'
import type { PagedRequest } from '@/types/basicData'
import type { ProductBatch } from '@/types/fqc'
import { useAuthStore } from '@/stores/authStore'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'

defineOptions({ name: 'FqcPackagingPage' })

const authStore = useAuthStore()

const loading = ref(false)
const list = ref<PackagingConfirmation[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '', status: '' })
const { visible: createVisible, open: openCreatePanel, close: closeCreatePanel } = useRightPanel()
const createForm = reactive<CreatePackagingConfirmation>({
  batchId: 0,
  packagingMethod: '',
  labelPrinted: false,
  confirmedBy: authStore.user?.id || 0,
})

// ─── Batch select for create dialog ───
const batchOptions = ref<Array<{ value: number; label: string; batchCode: string; productName: string }>>([])
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
    closeCreatePanel()
    await fetchList()
  } catch { /* */ }
}

async function toggleLabelPrinted(row: PackagingConfirmation) {
  try {
    await ElMessageBox.confirm(
      `确认${row.labelPrinted ? '重新打印' : '标记'}标签？`,
      '标签打印操作',
      {
        confirmButtonText: '确认',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
    await packagingApi.updateLabelPrinted(row.id, !row.labelPrinted)
    row.labelPrinted = !row.labelPrinted
    ElMessage.success(row.labelPrinted ? '标签已打印' : '标签已取消打印')
  } catch { /* */ }
}

const totalLabelsPrinted = computed(() =>
  list.value.filter(r => r.labelPrinted).length,
)

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

onMounted(() => {
  fetchList()
  fetchBatchOptions()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Box /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">包装确认</h2>
          <span class="page-header-banner-subtitle">管理成品包装信息，确认包装方式与标签打印</span>
        </div>
      </div>
    </div>

    <!-- Summary Metrics -->
    <div class="stat-grid">
      <div class="stat-grid__item stat-grid__item--primary">
        <div class="stat-grid__icon stat-grid__icon--primary">
          <el-icon :size="20"><Box /></el-icon>
        </div>
        <div class="stat-grid__text">
          <div class="stat-grid__label">包装记录总数</div>
          <div class="stat-grid__value">{{ total }}</div>
        </div>
      </div>
      <div class="stat-grid__item stat-grid__item--success">
        <div class="stat-grid__icon stat-grid__icon--success">
          <el-icon :size="20"><PriceTag /></el-icon>
        </div>
        <div class="stat-grid__text">
          <div class="stat-grid__label">已打印标签</div>
          <div class="stat-grid__value">{{ totalLabelsPrinted }}</div>
        </div>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="data-card">
      <div class="data-card__header">
        <span class="data-card__title">包装确认记录</span>
        <div class="data-card__toolbar">
          <el-input
            v-model="query.keyword"
            placeholder="搜索批次号/包装方式"
            clearable
            size="small"
            :prefix-icon="Search"
            style="width: 220px"
            @clear="fetchList"
            @keyup.enter="fetchList"
          />
          <el-button size="small" @click="fetchList">
            <el-icon><Refresh /></el-icon>刷新
          </el-button>
          <el-button size="small" type="primary" @click="createVisible = true">
            <el-icon><Plus /></el-icon>包装确认
          </el-button>
        </div>
      </div>
      <el-table
        :data="list"
        v-loading="loading"
        size="small"
        class="data-card__table"
        @row-click="() => {}"
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
                :before-change="async () => {
                  try {
                    await ElMessageBox.confirm(
                      `确认${row.labelPrinted ? '重新打印' : '标记'}标签？`,
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
        <el-table-column prop="confirmedAt" label="确认时间" min-width="150">
          <template #default="{ row }">{{ formatDate(row.confirmedAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right" align="center">
          <template #default="{ row }">
            <el-button
              link
              size="small"
              :type="row.labelPrinted ? 'warning' : 'primary'"
              @click.stop="toggleLabelPrinted(row)"
            >
              <el-icon><PriceTag /></el-icon>
              {{ row.labelPrinted ? '重新打印' : '打印标签' }}
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
        <div class="data-card__pagination">
          <el-pagination
            v-model:current-page="query.page"
            v-model:page-size="query.pageSize"
            :total="total"
            :page-sizes="[10, 20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            :sizes-layout="'first, prev, pager, next'"
            :pager-count="7"
            background
            @change="fetchList"
          />
        </div>
      </div>
    </div>

    <!-- 包装确认面板 -->
    <RightPanel v-model:visible="createVisible" title="包装确认">
      <template #body>
        <div class="rp-section">
          <div class="rp-section-header">
            <el-icon class="rp-section-icon"><Box /></el-icon>
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

        <div class="rp-section">
          <div class="rp-section-header">
            <el-icon class="rp-section-icon"><Setting /></el-icon>
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
      </template>

      <template #footer>
        <el-button @click="closeCreatePanel">取消</el-button>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Check /></el-icon>
          确认
        </el-button>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 16px;
}

.label-printed-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}

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

.total-calc-display {
  font-size: 18px;
  font-weight: 700;
  color: var(--el-color-success, #67c23a);
  line-height: 32px;
}

/* ── RP Section ─────────────────────── */
.rp-section {
  margin-bottom: 20px;
}

.rp-section:last-of-type {
  margin-bottom: 0;
}

.rp-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
}

.rp-section-icon {
  font-size: 15px;
  color: var(--el-color-primary);
}
</style>

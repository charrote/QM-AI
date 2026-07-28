<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { SupplierScore } from '@/types/iqc'
import type { Supplier } from '@/types/basicData'
import {
  supplierApi
} from '@/api/basicData'
import { supplierScoreApi } from '@/api/iqc'
import { Shop, Search, Refresh, Edit, DataAnalysis, TrendCharts, Star } from '@element-plus/icons-vue'

defineOptions({ name: 'IqcSuppliersPage' })

// ─── Shared State ──────────────────────────────────────
const searchKeyword = ref('')
const gradeFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const loading = ref(false)

// ─── Suppliers ─────────────────────────────────────────
const suppliers = ref<Supplier[]>([])
const scoreMap = ref<Map<number, SupplierScore>>(new Map())
const scoreLoadingMap = ref<Map<number, boolean>>(new Map())

// ─── Edit Drawer ──────────────────────────────────────
const editDrawerVisible = ref(false)
const editingScore = ref<SupplierScore | null>(null)
const editSupplierId = ref<number>(0)
const editLoading = ref(false)

// ─── View Mode ─────────────────────────────────────────
const statusFilter = ref('')

// ─── Stats (computed from scoreMap) ───────────────────
const stats = computed(() => {
  const items = Array.from(scoreMap.value.values())
  return {
    total: suppliers.value.length,
    a: items.filter(s => s.grade === 'A').length,
    b: items.filter(s => s.grade === 'B').length,
    c: items.filter(s => s.grade === 'C').length,
    d: items.filter(s => s.grade === 'D').length,
    noScore: suppliers.value.length - items.length,
  }
})

// ─── Filtered & Paginated List ─────────────────────────
const filteredSuppliers = computed(() => {
  let list = suppliers.value

  if (searchKeyword.value) {
    const kw = searchKeyword.value.toLowerCase()
    list = list.filter(s =>
      s.code.toLowerCase().includes(kw) ||
      s.name.toLowerCase().includes(kw)
    )
  }

  if (gradeFilter.value) {
    const gradeScores = Array.from(scoreMap.value.values()).filter(s => s.grade === gradeFilter.value).map(s => s.supplierId)
    list = list.filter(s => gradeScores.includes(s.id))
  }

  return list
})

const paginatedSuppliers = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filteredSuppliers.value.slice(start, start + pageSize.value)
})

// ─── Helpers ──────────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toISOString().slice(0, 10)
}

function gradeTagType(grade?: string): 'success' | 'primary' | 'warning' | 'danger' | 'info' {
  switch (grade) {
    case 'A': return 'success'
    case 'B': return 'primary'
    case 'C': return 'warning'
    case 'D': return 'danger'
    default: return 'info'
  }
}

function scoreColor(score?: number): string {
  if (score == null) return 'var(--el-text-color-secondary)'
  if (score >= 90) return '#67c23a'
  if (score >= 80) return '#409eff'
  if (score >= 70) return '#e6a23c'
  return '#f56c6c'
}

// ─── CRUD ────────────────────────────────────────────
async function loadSuppliers() {
  loading.value = true
  try {
    const res = await supplierApi.list({ page: 1, pageSize: 500 })
    suppliers.value = res.items.filter((s: any) => s.isActive !== false) as Supplier[]
    // Load scores for all suppliers in parallel
    await Promise.allSettled(
      suppliers.value.map(async (s) => {
        scoreLoadingMap.value.set(s.id, true)
        try {
          const score = await supplierScoreApi.get(s.id)
          scoreMap.value.set(s.id, score)
        } catch {
          // No score yet
        } finally {
          scoreLoadingMap.value.set(s.id, false)
        }
      })
    )
  } catch (e) {
    console.error('Failed to load suppliers', e)
  } finally {
    loading.value = false
  }
}

async function refreshSupplierScore(supplier: Supplier) {
  scoreLoadingMap.value.set(supplier.id, true)
  try {
    const score = await supplierScoreApi.get(supplier.id)
    scoreMap.value.set(supplier.id, score)
  } catch {
    scoreMap.value.delete(supplier.id)
  } finally {
    scoreLoadingMap.value.set(supplier.id, false)
  }
}

function openEditScore(supplier: Supplier) {
  editSupplierId.value = supplier.id
  const existing = scoreMap.value.get(supplier.id)
  if (existing) {
    editingScore.value = { ...existing }
  } else {
    editingScore.value = {
      id: 0,
      supplierId: supplier.id,
      supplierName: supplier.name,
      score: undefined,
      grade: undefined,
      evaluation: '',
      scoreDate: new Date().toISOString().slice(0, 10),
    }
  }
  editDrawerVisible.value = true
}

async function saveEditScore() {
  if (!editSupplierId.value || !editingScore.value) return
  editLoading.value = true
  try {
    const result = await supplierScoreApi.update(editSupplierId.value, {
      score: editingScore.value.score,
      grade: editingScore.value.grade,
      evaluation: editingScore.value.evaluation,
      scoreDate: editingScore.value.scoreDate,
    })
    scoreMap.value.set(editSupplierId.value, result)
    ElMessage.success('供应商评分已更新')
    editDrawerVisible.value = false
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '更新失败')
  } finally {
    editLoading.value = false
  }
}

function onSearch() {
  page.value = 1
}

onMounted(async () => {
  await loadSuppliers()
})
</script>

<template>
  <div class="iqc-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--warning">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Shop /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">供应商评价</h2>
          <span class="page-header-banner-subtitle">来料供应商质量评价、评级管理与趋势追踪</span>
        </div>
      </div>
    </div>

    <!-- Content Area -->
    <div class="iqc-content">
      <!-- Stats Bar -->
      <div class="stats-bar">
        <div class="stat-item stat-total">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.total }}</div>
            <div class="stat-label">供应商总数</div>
          </div>
        </div>
        <div class="stat-item stat-a">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.a }}</div>
            <div class="stat-label">A 级 (优秀)</div>
          </div>
        </div>
        <div class="stat-item stat-b">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.b }}</div>
            <div class="stat-label">B 级 (良好)</div>
          </div>
        </div>
        <div class="stat-item stat-c">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.c }}</div>
            <div class="stat-label">C 级 (合格)</div>
          </div>
        </div>
        <div class="stat-item stat-d">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.d }}</div>
            <div class="stat-label">D 级 (不合格)</div>
          </div>
        </div>
        <div class="stat-item stat-noscore" v-if="stats.noScore > 0">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ stats.noScore }}</div>
            <div class="stat-label">待评价</div>
          </div>
        </div>
      </div>

      <!-- Data Card -->
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            供应商评分清单
            <el-tag v-if="total" type="info" size="small">{{ total }} 家</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索供应商编码/名称..."
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 220px"
              @keyup.enter="onSearch"
              @clear="onSearch"
            />
            <el-select
              v-model="gradeFilter"
              clearable
              placeholder="评级筛选"
              size="small"
              style="width: 120px"
              @change="onSearch"
            >
              <el-option label="A 级" value="A" />
              <el-option label="B 级" value="B" />
              <el-option label="C 级" value="C" />
              <el-option label="D 级" value="D" />
            </el-select>
            <el-button size="small" @click="loadSuppliers">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
          </div>
        </div>

        <el-table
          :data="paginatedSuppliers"
          border
          stripe
          v-loading="loading"
          style="width: 100%"
          size="small"
          class="suppliers-table"
        >
          <el-table-column type="index" label="序号" width="55" />
          <el-table-column prop="code" label="供应商编码" width="130" show-overflow-tooltip />
          <el-table-column prop="name" label="供应商名称" min-width="200" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="supplier-name-cell">{{ row.name }}</span>
              <span v-if="row.contactPerson" class="supplier-contact">
                <el-icon :size="12"><Star /></el-icon>
                {{ row.contactPerson }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="评级" width="90" align="center">
            <template #default="{ row }">
              <el-tag
                :type="gradeTagType(scoreMap.get(row.id)?.grade)"
                size="small"
                effect="dark"
                round
              >
                {{ scoreMap.get(row.id)?.grade ?? '-' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="综合评分" width="100" align="center">
            <template #default="{ row }">
              <span
                class="score-cell"
                :style="{ color: scoreColor(scoreMap.get(row.id)?.score) }"
              >
                {{ scoreMap.get(row.id)?.score != null ? scoreMap.get(row.id)!.score!.toFixed(1) : '-' }}
              </span>
            </template>
          </el-table-column>
          <el-table-column label="评估日期" width="130">
            <template #default="{ row }">
              <span class="date-cell">{{ formatDate(scoreMap.get(row.id)?.scoreDate) }}</span>
            </template>
          </el-table-column>
          <el-table-column label="评估意见" min-width="180" show-overflow-tooltip>
            <template #default="{ row }">
              <span class="eval-cell">{{ scoreMap.get(row.id)?.evaluation || '-' }}</span>
            </template>
          </el-table-column>
          <el-table-column label="操作" width="140" fixed="right" align="center">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="openEditScore(row)">
                <el-icon><Edit /></el-icon>评分
              </el-button>
              <el-button
                size="small"
                type="success"
                link
                @click.stop="refreshSupplierScore(row)"
                :loading="scoreLoadingMap.get(row.id)"
              >
                <el-icon><Refresh /></el-icon>刷新
              </el-button>
            </template>
          </el-table-column>
        </el-table>

        <!-- Pagination -->
        <div class="data-card__pagination" v-if="total > pageSize">
          <el-pagination
            v-model:current-page="page"
            v-model:page-size="pageSize"
            :total="total"
            :page-sizes="[10, 20, 50, 100]"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="onSearch"
            @current-change="onSearch"
          />
        </div>
      </div>
    </div>

    <!-- ================================================================== -->
    <!-- Drawers -->
    <!-- ================================================================== -->

    <!-- Drawer: 编辑供应商评分 -->
    <el-drawer
      v-model="editDrawerVisible"
      title="编辑供应商评分"
      size="580px"
      direction="rtl"
      :close-on-click-modal="false"
    >
      <template #header>
        <div class="drawer-header">
          <div class="drawer-header-icon">
            <el-icon :size="18"><DataAnalysis /></el-icon>
          </div>
          <div class="drawer-header-text">
            <span class="drawer-title">供应商评分</span>
            <span class="drawer-subtitle">{{ editingScore?.supplierName }}</span>
          </div>
          <el-tag
            v-if="editingScore"
            :type="gradeTagType(editingScore.grade)"
            effect="dark"
            round
          >
            {{ editingScore.grade ?? '待评级' }}
          </el-tag>
        </div>
      </template>

      <template v-if="editingScore">
        <!-- 基本信息 -->
        <div class="drawer-section">
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><Shop /></el-icon>
            <span>基本信息</span>
          </div>
          <el-descriptions :column="2" border size="default">
            <el-descriptions-item label="供应商编码">
              {{ suppliers.find(s => s.id === editSupplierId)?.code || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="供应商名称">
              {{ suppliers.find(s => s.id === editSupplierId)?.name || '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="联系人" v-if="suppliers.find(s => s.id === editSupplierId)?.contactPerson">
              {{ suppliers.find(s => s.id === editSupplierId)?.contactPerson }}
            </el-descriptions-item>
            <el-descriptions-item label="联系电话" v-if="suppliers.find(s => s.id === editSupplierId)?.contactPhone">
              {{ suppliers.find(s => s.id === editSupplierId)?.contactPhone }}
            </el-descriptions-item>
          </el-descriptions>
        </div>

        <!-- 评分信息 -->
        <div class="drawer-section">
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><DataAnalysis /></el-icon>
            <span>评分信息</span>
          </div>
          <el-form label-width="100px">
            <el-form-item label="综合评分">
              <el-input-number
                v-model="editingScore.score"
                :min="0"
                :max="100"
                :precision="1"
                style="width: 160px"
                controls-position="right"
              />
              <span class="score-hint">/ 100</span>
            </el-form-item>
            <el-form-item label="评级">
              <el-select v-model="editingScore.grade" style="width: 200px">
                <el-option label="A 级 (优秀)" value="A" />
                <el-option label="B 级 (良好)" value="B" />
                <el-option label="C 级 (合格)" value="C" />
                <el-option label="D 级 (不合格)" value="D" />
              </el-select>
            </el-form-item>
            <el-form-item label="评估日期">
              <el-date-picker
                v-model="editingScore.scoreDate"
                type="date"
                placeholder="选择日期"
                style="width: 200px"
              />
            </el-form-item>
          </el-form>
        </div>

        <!-- 评估意见 -->
        <div class="drawer-section">
          <div class="drawer-section-header">
            <el-icon class="drawer-section-icon"><TrendCharts /></el-icon>
            <span>评估意见</span>
          </div>
          <el-form>
            <el-form-item label="">
              <el-input
                v-model="editingScore.evaluation"
                type="textarea"
                :rows="4"
                placeholder="请输入评估意见..."
              />
            </el-form-item>
          </el-form>
        </div>
      </template>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="editDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="saveEditScore" :loading="editLoading">
            保存评分
          </el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<style scoped>
.iqc-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* ─── 主体布局 ──────────────────────────────────── */
.iqc-content {
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
  grid-template-columns: repeat(6, 1fr);
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
.stat-total .stat-accent { background: var(--el-color-info); }
.stat-a .stat-accent { background: var(--el-color-success); }
.stat-b .stat-accent { background: var(--el-color-primary); }
.stat-c .stat-accent { background: var(--el-color-warning); }
.stat-d .stat-accent { background: var(--el-color-danger); }
.stat-noscore .stat-accent { background: var(--el-text-color-secondary); }
.stat-content {
  display: flex;
  flex-direction: column;
}
.stat-value {
  font-size: 28px;
  font-weight: 700;
  line-height: 1.2;
}
.stat-total .stat-value { color: var(--el-color-info); }
.stat-a .stat-value { color: var(--el-color-success); }
.stat-b .stat-value { color: var(--el-color-primary); }
.stat-c .stat-value { color: var(--el-color-warning); }
.stat-d .stat-value { color: var(--el-color-danger); }
.stat-noscore .stat-value { color: var(--el-text-color-secondary); }
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

/* ── 供应商表格 ── */
.suppliers-table {
  flex: 1;
  min-height: 0;
  width: 100%;
}

.suppliers-table :deep(.el-table__cell) {
  white-space: nowrap;
}

.suppliers-table :deep(.el-table__header-wrapper) {
  flex-shrink: 0;
}

.suppliers-table :deep(.el-table__body-wrapper) {
  overflow-y: auto;
}

.suppliers-table :deep(.el-table__row) {
  height: 32px;
  line-height: 32px;
}

.suppliers-table :deep(.el-table__header-wrapper .el-table__cell) {
  height: 32px;
  line-height: 32px;
  padding: 0 8px;
}

.suppliers-table :deep(.el-table__body-wrapper .el-table__cell) {
  padding: 0 8px;
}

.suppliers-table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.suppliers-table :deep(.el-button--primary.is-link:hover),
.suppliers-table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.suppliers-table :deep(.el-button--success.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.suppliers-table :deep(.el-button--success.is-link:hover),
.suppliers-table :deep(.el-button--success.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

/* ── 单元格样式 ── */
.supplier-name-cell {
  font-weight: 500;
  color: var(--el-text-color-primary);
  display: block;
}
.supplier-contact {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  display: flex;
  align-items: center;
  gap: 2px;
  margin-top: 2px;
}
.score-cell {
  font-weight: 700;
  font-size: 15px;
}
.date-cell {
  font-size: 12px;
  color: var(--el-text-color-regular);
}
.eval-cell {
  font-size: 13px;
  color: var(--el-text-color-regular);
}

/* ── Drawer Sections ── */
.drawer-header {
  display: flex;
  align-items: center;
  gap: 12px;
}
.drawer-header-icon {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-color-warning-light-9);
  border-radius: 8px;
  color: var(--el-color-warning);
  font-size: 18px;
  flex-shrink: 0;
}
.drawer-header-text {
  display: flex;
  flex-direction: column;
}
.drawer-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  line-height: 1.3;
}
.drawer-subtitle {
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.drawer-section {
  margin-bottom: 8px;
}
.drawer-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}
.drawer-section-icon {
  font-size: 15px;
  color: var(--el-color-warning);
}

.score-hint {
  margin-left: 8px;
  font-size: 14px;
  color: var(--el-text-color-secondary);
}

/* ── 响应式 ── */
@media (max-width: 1400px) {
  .stats-bar {
    grid-template-columns: repeat(3, 1fr);
  }
}
@media (max-width: 768px) {
  .stats-bar {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Search, Refresh, Document, EditPen, Check, Plus, Setting, Upload,
  Clock, CircleCheck, Promotion, Close, User,
} from '@element-plus/icons-vue'
import { releaseApi } from '@/api/fqc'
import { batchApi } from '@/api/fqc'
import type { OqcRelease, CreateOqcRelease, SignOqcRelease } from '@/types/fqc'
import type { PagedRequest } from '@/types/basicData'
import { RELEASE_STATUS_OPTIONS, RELEASE_STATUS_MAP } from '@/types/fqc'
import { useAuthStore } from '@/stores/authStore'

defineOptions({ name: 'FqcOqcReleasesPage' })

const authStore = useAuthStore()

const loading = ref(false)
const list = ref<OqcRelease[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '', status: '' })

const createVisible = ref(false)
const signVisible = ref(false)
const confirmLoading = ref(false)
const signId = ref(0)
const activeBatch = ref<OqcRelease | null>(null)

// Create form
const createForm = reactive<CreateOqcRelease>({
  batchId: 0,
  customerId: 0,
  releaseDate: new Date().toISOString().slice(0, 10),
  quantity: 0,
})

// Batch / customer selects
const batchOptions = ref<{ id: number; batchCode: string; quantity: number }[]>([])
const batchSearch = ref('')
const customerOptions = ref<{ id: number; customerName: string }[]>([])
const customerSearch = ref('')

// Sign form
const signForm = reactive<SignOqcRelease>({
  authorizedBy: 0,
  eSignatureUrl: '',
})
const signFileRef = ref<HTMLInputElement | null>(null)

// ─── Computed metrics ──────────────────────────────
const metrics = computed(() => {
  const items = list.value
  return {
    total: items.length,
    pending: items.filter(i => i.status === 'pending').length,
    signed: items.filter(i => i.status === 'signed').length,
    released: items.filter(i => i.status === 'released').length,
  }
})

// ─── Batch / customer data loading ─────────────────
async function loadBatches() {
  try {
    const res = await batchApi.list({ page: 1, pageSize: 500, keyword: batchSearch.value, status: '' })
    batchOptions.value = (res.items ?? []).map(b => ({ id: b.id, batchCode: b.batchCode, quantity: b.quantity }))
  } catch { /* */ }
}

async function loadCustomers() {
  // Placeholder: load from available customers
  // In a real integration this would call a customer API
  try {
    const res = await batchApi.list({ page: 1, pageSize: 500, keyword: customerSearch.value, status: '' })
    const seen = new Set<number>()
    customerOptions.value = []
    for (const b of res.items ?? []) {
      if (b.productName && !seen.has(b.productId)) {
        seen.add(b.productId)
        customerOptions.value.push({ id: b.productId, customerName: b.productName })
      }
    }
  } catch { /* */ }
}

function onBatchSelect(val: number) {
  const batch = batchOptions.value.find(b => b.id === val)
  if (batch) {
    createForm.batchId = batch.id
    createForm.quantity = batch.quantity
  }
}

function onCustomerSelect(val: number) {
  createForm.customerId = val
}

// ─── Helpers ──────────────────────────────────────
function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

// ─── Status helpers ────────────────────────────────
function statusTagType(status: string): string {
  const map: Record<string, string> = {
    pending: 'info', signed: 'warning', released: 'success', cancelled: 'danger',
  }
  return map[status] || 'info'
}

function statusFlowIcon(status: string) {
  switch (status) {
    case 'pending': return { icon: Clock, color: 'var(--el-color-info)' }
    case 'signed': return { icon: CircleCheck, color: 'var(--el-color-warning)' }
    case 'released': return { icon: Promotion, color: 'var(--el-color-success)' }
    case 'cancelled': return { icon: Close, color: 'var(--el-color-danger)' }
    default: return { icon: Clock, color: 'var(--el-color-info)' }
  }
}

// ─── Fetch ─────────────────────────────────────────
async function fetchList() {
  loading.value = true
  try {
    const res = await releaseApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* */ }
  finally { loading.value = false }
}

// ─── Create ────────────────────────────────────────
async function handleCreate() {
  if (!createForm.batchId || !createForm.customerId) {
    ElMessage.warning('请选择批次和客户')
    return
  }
  try {
    await releaseApi.create(createForm)
    ElMessage.success('放行单创建成功')
    createVisible.value = false
    Object.assign(createForm, { batchId: 0, customerId: 0, releaseDate: new Date().toISOString().slice(0, 10), quantity: 0 })
    await fetchList()
  } catch { /* */ }
}

// ─── Sign ──────────────────────────────────────────
function openSign(row: OqcRelease) {
  signId.value = row.id
  activeBatch.value = row
  signForm.authorizedBy = authStore.user?.id || 0
  signForm.eSignatureUrl = row.eSignatureUrl || ''
  signVisible.value = true
}

function triggerFileInput() {
  signFileRef.value?.click()
}

function handleFileChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  if (!file.type.startsWith('image/')) {
    ElMessage.warning('请上传图片文件')
    return
  }
  // Simulate upload URL (in real app, upload to MinIO/object store)
  signForm.eSignatureUrl = URL.createObjectURL(file)
  ElMessage.success('签名图片已选择')
}

async function handleSign() {
  if (!signForm.eSignatureUrl) {
    ElMessage.warning('请上传签名图片')
    return
  }
  try {
    await releaseApi.sign(signId.value, signForm)
    ElMessage.success('签名成功')
    signVisible.value = false
    await fetchList()
  } catch { /* */ }
}

// ─── Confirm ───────────────────────────────────────
async function handleConfirm(row: OqcRelease) {
  try {
    await ElMessageBox.confirm(
      `确认放行批次 <strong>${row.batchCode || '—'}</strong> 给客户 <strong>${row.customerName || '—'}</strong>？数量：${row.quantity}`
        + `<br/><small style="color:#909399">放行单号：${row.releaseNumber}</small>`,
      '确认出货放行',
      {
        confirmButtonText: '确认放行',
        cancelButtonText: '取消',
        type: 'warning',
        dangerouslyUseHTMLString: true,
      }
    )
    confirmLoading.value = true
    try {
      await releaseApi.confirm(row.id)
      ElMessage.success('放行确认成功')
      await fetchList()
    } finally {
      confirmLoading.value = false
    }
  } catch { /* cancelled */ }
}

// ─── Mount ─────────────────────────────────────────
onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header-banner page-header-banner--warning">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Document /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">FQC/OQC 放行管理</h2>
          <span class="page-header-banner-subtitle">管理成品放行流程，从检验合格到出货确认的完整链路</span>
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
            <div class="stat-value">{{ metrics.total }}</div>
            <div class="stat-label">总放行单</div>
          </div>
        </div>
        <div class="stat-item stat-pending">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ metrics.pending }}</div>
            <div class="stat-label">待签名</div>
          </div>
        </div>
        <div class="stat-item stat-signed">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ metrics.signed }}</div>
            <div class="stat-label">已签名</div>
          </div>
        </div>
        <div class="stat-item stat-released">
          <div class="stat-accent"></div>
          <div class="stat-content">
            <div class="stat-value">{{ metrics.released }}</div>
            <div class="stat-label">已放行</div>
          </div>
        </div>
      </div>

    <!-- Data Card with Toolbar -->
    <div class="data-card">
      <div class="data-card__header">
        <span class="data-card__title">
          放行清单
          <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
        </span>
        <div class="data-card__actions">
          <el-input
            v-model="query.keyword"
            placeholder="搜索放行单号/批次/客户"
            clearable
            size="small"
            :prefix-icon="Search"
            style="width: 220px"
            @clear="fetchList"
            @keyup.enter="fetchList"
          />
          <el-select
            v-model="query.status"
            clearable
            placeholder="放行状态"
            size="small"
            style="width: 110px"
            @change="fetchList"
          >
            <el-option v-for="opt in RELEASE_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-button size="small" @click="fetchList">
            <el-icon><Refresh /></el-icon>刷新
          </el-button>
          <el-button type="primary" size="small" @click="createVisible = true">
            <el-icon><Plus /></el-icon>新建放行单
          </el-button>
        </div>
      </div>

      <el-table
        :data="list"
        v-loading="loading"
        border
        stripe
        style="width: 100%"
        size="small"
      >
        <el-table-column label="状态流程" width="150" align="center" fixed>
          <template #default="{ row }">
            <div class="status-flow">
              <el-icon :size="14" :color="statusFlowIcon(row.status).color">
                <component :is="statusFlowIcon(row.status).icon" />
              </el-icon>
              <span class="status-flow__text">{{ RELEASE_STATUS_MAP[row.status] || row.status }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="releaseNumber" label="放行单号" min-width="150" show-overflow-tooltip />
        <el-table-column prop="batchCode" label="批次号" min-width="130" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="info-primary">{{ row.batchCode || '—' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="customerName" label="客户" min-width="130" show-overflow-tooltip>
          <template #default="{ row }">
            <span class="info-primary">{{ row.customerName || '—' }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="quantity" label="数量" width="90" align="center" />
        <el-table-column label="签名状态" width="90" align="center">
          <template #default="{ row }">
            <el-tag
              v-if="row.status === 'signed' && row.signatureTime"
              type="success"
              size="small"
              effect="dark"
            >
              <el-icon style="margin-right: 2px"><Check /></el-icon>
              已签
            </el-tag>
            <el-tag
              v-else-if="row.status === 'pending'"
              type="info"
              size="small"
              effect="dark"
            >
              <el-icon style="margin-right: 2px"><Clock /></el-icon>
              待签
            </el-tag>
            <span v-else class="info-muted">—</span>
          </template>
        </el-table-column>
        <el-table-column prop="releaseDate" label="放行日期" width="120" align="center" />
        <el-table-column label="操作" width="220" fixed="right" align="center">
          <template #default="{ row }">
            <el-button
              v-if="row.status === 'pending'"
              link
              size="small"
              type="warning"
              @click.stop="openSign(row)"
            >
              <el-icon><EditPen /></el-icon>
              签名
            </el-button>
            <el-button
              v-if="row.status === 'signed'"
              link
              size="small"
              type="success"
              :loading="confirmLoading"
              @click.stop="handleConfirm(row)"
            >
              <el-icon><Check /></el-icon>
              确认放行
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="data-card__pagination">
        <el-pagination
          v-model:current-page="query.page"
          v-model:page-size="query.pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @change="fetchList"
        />
      </div>
    </div>

    <!-- Create Dialog -->
    <el-dialog
      v-model="createVisible"
      title="新建出货放行单"
      width="560px"
      :close-on-click-modal="false"
      top="6vh"
    >
      <!-- 放行信息 -->
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Document /></el-icon>
          <span>放行信息</span>
        </div>
        <el-form :model="createForm" label-width="90px" label-position="left">
          <el-form-item label="批次" required>
            <el-select
              v-model="createForm.batchId"
              filterable
              clearable
              placeholder="搜索或选择批次"
              style="width: 100%"
              @change="onBatchSelect"
              @visible-change="loadBatches"
              @clear="createForm.batchId = 0"
            >
              <el-option
                v-for="b in batchOptions"
                :key="b.id"
                :label="`${b.batchCode}`"
                :value="b.id"
              >
                <span>{{ b.batchCode }}</span>
                <span style="float:right;color:#909399;font-size:12px">数量: {{ b.quantity }}</span>
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="客户" required>
            <el-select
              v-model="createForm.customerId"
              filterable
              clearable
              placeholder="搜索或选择客户"
              style="width: 100%"
              @change="onCustomerSelect"
              @visible-change="loadCustomers"
              @clear="createForm.customerId = 0"
            >
              <el-option
                v-for="c in customerOptions"
                :key="c.id"
                :label="c.customerName"
                :value="c.id"
              />
            </el-select>
          </el-form-item>
        </el-form>
      </div>

      <!-- 放行参数 -->
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Setting /></el-icon>
          <span>放行参数</span>
        </div>
        <el-form :model="createForm" label-width="90px" label-position="left">
          <el-form-item label="放行数量">
            <el-input-number
              v-model="createForm.quantity"
              :min="1"
              style="width: 100%"
              controls-position="right"
            />
          </el-form-item>
          <el-form-item label="放行日期">
            <el-date-picker
              v-model="createForm.releaseDate"
              type="date"
              value-format="YYYY-MM-DD"
              style="width: 100%"
            />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Check /></el-icon>
          创建
        </el-button>
      </template>
    </el-dialog>

    <!-- Sign Dialog -->
    <el-dialog
      v-model="signVisible"
      title="电子签名确认"
      width="540px"
      :close-on-click-modal="false"
      top="6vh"
    >
      <!-- 放行信息摘要 -->
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Document /></el-icon>
          <span>放行信息</span>
        </div>
        <div v-if="activeBatch" class="release-summary">
          <div class="release-summary__row">
            <span class="release-summary__label">放行单号</span>
            <span class="release-summary__value">{{ activeBatch.releaseNumber }}</span>
          </div>
          <div class="release-summary__row">
            <span class="release-summary__label">批次号</span>
            <span class="release-summary__value">{{ activeBatch.batchCode || '—' }}</span>
          </div>
          <div class="release-summary__row">
            <span class="release-summary__label">客户</span>
            <span class="release-summary__value">{{ activeBatch.customerName || '—' }}</span>
          </div>
          <div class="release-summary__row">
            <span class="release-summary__label">数量</span>
            <span class="release-summary__value">{{ activeBatch.quantity }}</span>
          </div>
        </div>
      </div>

      <!-- 签名区域 -->
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><EditPen /></el-icon>
          <span>电子签名</span>
        </div>
        <el-form :model="signForm" label-width="90px" label-position="left">
          <el-form-item label="签名者">
            <span class="info-primary">
              <el-icon style="margin-right: 4px"><User /></el-icon>
              {{ authStore.user?.name || authStore.user?.username || '当前用户' }}
            </span>
          </el-form-item>
          <el-form-item label="签名图片" required>
            <div class="sig-upload" @click="triggerFileInput">
              <el-icon :size="32" color="var(--el-color-info)"><Upload /></el-icon>
              <div class="sig-upload__text">点击或拖拽上传签名图片</div>
              <div class="sig-upload__hint">支持 JPG、PNG 格式</div>
              <input
                ref="signFileRef"
                type="file"
                accept="image/jpeg,image/png,image/gif,image/webp"
                style="display:none"
                @change="handleFileChange"
              />
            </div>
          </el-form-item>
          <el-form-item label="签名预览" v-if="signForm.eSignatureUrl">
            <div class="sig-preview">
              <img :src="signForm.eSignatureUrl" alt="签名预览" />
            </div>
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <el-button @click="signVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSign">
          <el-icon><Check /></el-icon>
          确认签名
        </el-button>
      </template>
    </el-dialog>
  </div>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
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
.stat-total .stat-accent { background: var(--el-color-info); }
.stat-pending .stat-accent { background: var(--el-color-info); }
.stat-signed .stat-accent { background: var(--el-color-warning); }
.stat-released .stat-accent { background: var(--el-color-success); }
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
.stat-pending .stat-value { color: var(--el-color-info); }
.stat-signed .stat-value { color: var(--el-color-warning); }
.stat-released .stat-value { color: var(--el-color-success); }
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

.data-card__row {
  transition: background-color 0.2s;
}

.data-card__row:hover {
  background-color: var(--el-fill-color-light, #f5f7fa) !important;
}

/* ── 状态流程 ── */
.status-flow {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
}

.status-flow__text {
  font-size: 12px;
}

/* ── 单元格样式 ── */
.info-primary {
  color: var(--el-text-color-primary);
  font-weight: 500;
}

.info-muted {
  color: var(--el-text-placeholder, #c0c4cc);
}

/* ── Release Summary ──────────────────── */
.release-summary {
  padding: 12px 16px;
  background: var(--el-fill-color-lighter, #f2f6fc);
  border-radius: 8px;
}

.release-summary__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 0;
}

.release-summary__row:not(:last-child) {
  border-bottom: 1px dashed var(--el-border-color-light, #dcdfe6);
}

.release-summary__label {
  font-size: 13px;
  color: var(--el-text-secondary, #909399);
}

.release-summary__value {
  font-size: 13px;
  color: var(--el-text-primary, #303133);
  font-weight: 500;
}

/* ── 对话框 Sections ── */
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
  color: var(--el-color-warning);
}
.dialog-section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}

/* ── 签名上传 ─────────────── */
.sig-upload {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 24px 16px;
  background: var(--el-fill-color-lighter, #f2f6fc);
  border: 1px dashed var(--el-border-color, #dcdfe6);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.sig-upload:hover {
  border-color: var(--el-color-primary, #409eff);
  background: var(--el-color-primary-light-9, #ecf5ff);
}

.sig-upload__text {
  font-size: 14px;
  color: var(--el-text-regular, #606266);
}

.sig-upload__hint {
  font-size: 12px;
  color: var(--el-text-placeholder, #c0c4cc);
}

/* ── 签名预览 ────────────── */
.sig-preview {
  padding: 12px;
  background: var(--el-fill-color-lighter, #f2f6fc);
  border-radius: 8px;
  text-align: center;
}

.sig-preview img {
  max-width: 100%;
  max-height: 150px;
  border: 1px solid var(--el-border-color-light, #dcdfe6);
  border-radius: 4px;
}
</style>

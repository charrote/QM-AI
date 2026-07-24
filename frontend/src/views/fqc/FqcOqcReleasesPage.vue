<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { Search, Refresh, Document, EditPen, Check, Upload } from '@element-plus/icons-vue'
import { releaseApi } from '@/api/fqc'
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
const signId = ref(0)
const createForm = reactive<CreateOqcRelease>({
  batchId: 0,
  customerId: 0,
  releaseDate: new Date().toISOString().slice(0, 10),
  quantity: 0,
})
const signForm = reactive<SignOqcRelease>({
  authorizedBy: 1,
  eSignatureUrl: '',
})

async function fetchList() {
  loading.value = true
  try {
    const res = await releaseApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* */ }
  finally { loading.value = false }
}

async function handleCreate() {
  if (!createForm.batchId || !createForm.customerId) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    await releaseApi.create(createForm)
    ElMessage.success('放行单创建成功')
    createVisible.value = false
    await fetchList()
  } catch { /* */ }
}

function openSign(id: number) {
  signId.value = id
  signForm.authorizedBy = authStore.user?.id || 0
  signForm.eSignatureUrl = ''
  signVisible.value = true
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

async function handleConfirm(id: number) {
  try {
    await releaseApi.confirm(id)
    ElMessage.success('放行确认成功')
    await fetchList()
  } catch { /* */ }
}

function statusTag(status: string): string {
  const map: Record<string, string> = {
    pending: 'info', signed: 'warning', released: 'success', cancelled: 'danger',
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
        <el-icon :size="28"><Document /></el-icon>
      </div>
      <div class="page-header__info">
        <h1 class="page-header__title">OQC 出货放行</h1>
        <p class="page-header__subtitle">管理出货放行单，执行电子签名与确认流程</p>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="action-bar">
      <div class="action-bar__left">
        <el-input
          v-model="query.keyword"
          placeholder="搜索放行单号/批次"
          :prefix-icon="Search"
          clearable
          style="width: 260px"
          @clear="fetchList"
          @keyup.enter="fetchList"
        />
        <el-select
          v-model="query.status"
          placeholder="放行状态"
          clearable
          style="width: 140px"
          @change="fetchList"
        >
          <el-option v-for="opt in RELEASE_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-button @click="fetchList">
          <el-icon><Refresh /></el-icon>
          刷新
        </el-button>
      </div>
      <div class="action-bar__right">
        <el-button type="primary" @click="createVisible = true">
          <el-icon><Plus /></el-icon>
          新建放行单
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
        <el-table-column prop="releaseNumber" label="放行单号" min-width="160" show-overflow-tooltip />
        <el-table-column prop="batchCode" label="批次号" min-width="130" show-overflow-tooltip />
        <el-table-column prop="customerName" label="客户" min-width="130" show-overflow-tooltip />
        <el-table-column prop="quantity" label="数量" width="80" align="center" />
        <el-table-column label="状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="statusTag(row.status)" size="small" effect="dark">{{ RELEASE_STATUS_MAP[row.status] || row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="releaseDate" label="放行日期" width="110" align="center" />
        <el-table-column label="操作" width="220" fixed="right" align="center">
          <template #default="{ row }">
            <el-button v-if="row.status === 'pending'" link size="small" type="warning" @click.stop="openSign(row.id)">
              <el-icon><EditPen /></el-icon>
              签名
            </el-button>
            <el-button v-if="row.status === 'signed'" link size="small" type="success" @click.stop="handleConfirm(row.id)">
              <el-icon><Check /></el-icon>
              确认放行
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

    <!-- 新建放行单对话框 -->
    <el-dialog v-model="createVisible" title="新建出货放行单" width="540px" :close-on-click-modal="false" top="6vh">
      <div v-if="createVisible">
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Document /></el-icon>
            <span>放行信息</span>
          </div>
          <el-form :model="createForm" label-width="90px" label-position="left">
            <el-form-item label="批次 ID" required>
              <el-input-number v-model="createForm.batchId" :min="1" style="width:100%" />
            </el-form-item>
            <el-form-item label="客户 ID" required>
              <el-input-number v-model="createForm.customerId" :min="1" style="width:100%" />
            </el-form-item>
          </el-form>
        </div>

        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><Setting /></el-icon>
            <span>放行参数</span>
          </div>
          <el-form :model="createForm" label-width="90px" label-position="left">
            <el-form-item label="放行数量">
              <el-input-number v-model="createForm.quantity" :min="1" style="width:100%" />
            </el-form-item>
            <el-form-item label="放行日期">
              <el-date-picker v-model="createForm.releaseDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
            </el-form-item>
          </el-form>
        </div>
      </div>

      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">
          <el-icon><Check /></el-icon>
          创建
        </el-button>
      </template>
    </el-dialog>

    <!-- 电子签名对话框 -->
    <el-dialog v-model="signVisible" title="电子签名" width="500px" :close-on-click-modal="false" top="8vh">
      <div v-if="signVisible">
        <div class="dialog-section">
          <div class="dialog-section__title">
            <el-icon><EditPen /></el-icon>
            <span>签名信息</span>
          </div>
          <el-form :model="signForm" label-width="110px" label-position="left">
            <el-form-item label="签名图片 URL" required>
              <el-input v-model="signForm.eSignatureUrl" placeholder="输入 MinIO 签名图片地址" clearable />
            </el-form-item>
            <el-form-item label="签名预览" v-if="signForm.eSignatureUrl">
              <div class="sig-preview">
                <img :src="signForm.eSignatureUrl" alt="签名预览" />
              </div>
            </el-form-item>
          </el-form>
        </div>
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
</template>

<script lang="ts">
import { Plus, Setting } from '@element-plus/icons-vue'
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
  background: var(--el-color-warning-light-9, #fdf6ec);
  color: var(--el-color-warning, #e6a23c);
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

/* ─── Signature Preview ────────────── */
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
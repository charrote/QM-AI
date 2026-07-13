<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage } from 'element-plus'
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
    <div class="toolbar-row">
      <el-form :inline="true" :model="query" >
        <el-form-item>
          <el-input v-model="query.keyword" placeholder="搜索放行单号/批次" clearable @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item>
          <el-select v-model="query.status" placeholder="放行状态" clearable @change="fetchList">
            <el-option v-for="opt in RELEASE_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-button type="primary" @click="createVisible = true">新建放行单</el-button>
      </div>
    </div>

    <el-table :data="list" v-loading="loading" stripe border style="width: 100%">
      <el-table-column prop="releaseNumber" label="放行单号" min-width="160" />
      <el-table-column prop="batchCode" label="批次号" min-width="130" />
      <el-table-column prop="customerName" label="客户" min-width="130" />
      <el-table-column prop="quantity" label="数量" width="70" align="center" />
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ RELEASE_STATUS_MAP[row.status] || row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="releaseDate" label="放行日期" width="110" />
      <el-table-column label="操作" width="180" fixed="right">
        <template #default="{ row }">
          <el-button size="small" v-if="row.status === 'pending'" type="warning" @click="openSign(row.id)">签名</el-button>
          <el-button size="small" v-if="row.status === 'signed'" type="success" @click="handleConfirm(row.id)">确认放行</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-row">
      <el-pagination
        v-model:current-page="query.page"
        v-model:page-size="query.pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, prev, pager, next, sizes"
        @change="fetchList"
      />
    </div>

    <!-- 新建放行单对话框 -->
    <el-dialog v-model="createVisible" title="新建出货放行单" width="500px">
      <el-form :model="createForm" label-width="100px" >
        <el-form-item label="批次 ID">
          <el-input-number v-model="createForm.batchId" :min="1" />
        </el-form-item>
        <el-form-item label="客户 ID">
          <el-input-number v-model="createForm.customerId" :min="1" />
        </el-form-item>
        <el-form-item label="放行数量">
          <el-input-number v-model="createForm.quantity" :min="1" />
        </el-form-item>
        <el-form-item label="放行日期">
          <el-date-picker v-model="createForm.releaseDate" type="date" value-format="YYYY-MM-DD" style="width:100%" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- 电子签名对话框 -->
    <el-dialog v-model="signVisible" title="电子签名" width="450px">
      <el-form :model="signForm" label-width="100px" >
        <el-form-item label="签名图片 URL">
          <el-input v-model="signForm.eSignatureUrl" placeholder="输入 MinIO 签名图片地址" />
        </el-form-item>
        <el-form-item label="签名预览" v-if="signForm.eSignatureUrl">
          <img :src="signForm.eSignatureUrl" style="max-width:300px; max-height:120px; border:1px solid #ddd" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="signVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSign">确认签名</el-button>
      </template>
    </el-dialog>
  </div>
</template>

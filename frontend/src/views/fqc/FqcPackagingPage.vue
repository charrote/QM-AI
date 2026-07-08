<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { packagingApi } from '@/api/fqc'
import type { PackagingConfirmation, CreatePackagingConfirmation } from '@/types/fqc'
import type { PagedRequest } from '@/types/basicData'
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

async function fetchList() {
  loading.value = true
  try {
    const res = await packagingApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* */ }
  finally { loading.value = false }
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
    await packagingApi.updateLabelPrinted(row.id, !row.labelPrinted)
    row.labelPrinted = !row.labelPrinted
    ElMessage.success(row.labelPrinted ? '标签已标记打印' : '标签已取消打印')
  } catch { /* */ }
}

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-form :inline="true" :model="query" size="small">
        <el-form-item>
          <el-input v-model="query.keyword" placeholder="搜索批次号/包装方式" clearable @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-button type="primary" @click="createVisible = true">包装确认</el-button>
      </div>
    </div>

    <el-table :data="list" v-loading="loading" stripe border style="width: 100%">
      <el-table-column prop="batchCode" label="批次号" min-width="150" />
      <el-table-column prop="packagingMethod" label="包装方式" min-width="130" />
      <el-table-column prop="qtyPerBox" label="每箱数量" width="90" align="center" />
      <el-table-column prop="totalBoxes" label="总箱数" width="70" align="center" />
      <el-table-column label="标签打印" width="90" align="center">
        <template #default="{ row }">
          <el-switch :model-value="row.labelPrinted" @change="toggleLabelPrinted(row)" />
        </template>
      </el-table-column>
      <el-table-column prop="confirmedAt" label="确认时间" min-width="150" />
      <el-table-column label="操作" width="80" fixed="right">
        <template #default="{ row }">
          <el-button size="small" @click="toggleLabelPrinted(row)">
            {{ row.labelPrinted ? '取消打印' : '打印标签' }}
          </el-button>
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

    <!-- 包装确认对话框 -->
    <el-dialog v-model="createVisible" title="包装确认" width="500px">
      <el-form :model="createForm" label-width="100px" size="small">
        <el-form-item label="批次 ID">
          <el-input-number v-model="createForm.batchId" :min="1" />
        </el-form-item>
        <el-form-item label="包装方式">
          <el-input v-model="createForm.packagingMethod" placeholder="如：纸箱、木箱、托盘" />
        </el-form-item>
        <el-form-item label="每箱数量">
          <el-input-number v-model="createForm.qtyPerBox" :min="1" />
        </el-form-item>
        <el-form-item label="总箱数">
          <el-input-number v-model="createForm.totalBoxes" :min="1" />
        </el-form-item>
        <el-form-item label="标签打印">
          <el-switch v-model="createForm.labelPrinted" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">确认</el-button>
      </template>
    </el-dialog>
  </div>
</template>

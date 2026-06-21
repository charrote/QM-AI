<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage } from 'element-plus'
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
    <!-- 批次来源说明横幅 -->
    <el-alert
      title="批次来源说明"
      type="info"
      :closable="false"
      show-icon
      style="margin-bottom: 12px"
    >
      <template #default>
        <span>批次是成品检验的基本单元，来源包括：</span>
        <el-tag size="small" type="success" style="margin:0 4px">IPQC 关单自动生成</el-tag>
        <el-tag size="small" type="warning" style="margin:0 4px">工单完工自动生成</el-tag>
        <el-tag size="small" type="info" style="margin:0 4px">手动创建（兜底）</el-tag>
        <span style="margin-left:4px">→ 进行 FQC 成品检验 → OQC 出货放行</span>
      </template>
    </el-alert>

    <div class="toolbar-row">
      <el-form :inline="true" :model="query" size="small">
        <el-form-item>
          <el-input v-model="query.keyword" placeholder="搜索批次号/产品" clearable @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item>
          <el-select v-model="query.status" placeholder="批次状态" clearable @change="fetchList">
            <el-option v-for="opt in BATCH_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-button type="primary" @click="openCreate">新建批次（手动）</el-button>
      </div>
    </div>

    <el-table :data="list" v-loading="loading" stripe border style="width: 100%">
      <el-table-column prop="batchCode" label="批次号" min-width="160" />
      <el-table-column prop="productName" label="产品名称" min-width="150" />
      <el-table-column prop="quantity" label="数量" width="80" align="center" />
      <el-table-column label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="statusTag(row.status)" size="small">{{ BATCH_STATUS_MAP[row.status] || row.status }}</el-tag>
        </template>
      </el-table-column>
      <el-table-column label="来源" width="110">
        <template #default="{ row }">
          <el-tag size="small" type="info">手动创建</el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" min-width="150" />
      <el-table-column label="操作" width="100" fixed="right">
        <template #default="{ row }">
          <el-button size="small" @click="openDetail(row.id)">详情</el-button>
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

    <!-- 详情抽屉 -->
    <el-drawer v-model="detailVisible" title="批次详情" size="600px">
      <template v-if="detail">
        <el-descriptions :column="2" border size="small">
          <el-descriptions-item label="批次号">{{ detail.batchCode }}</el-descriptions-item>
          <el-descriptions-item label="产品">{{ detail.productName }}</el-descriptions-item>
          <el-descriptions-item label="数量">{{ detail.quantity }}</el-descriptions-item>
          <el-descriptions-item label="关联工单">{{ detail.workOrderId || '-' }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="statusTag(detail.status)">{{ BATCH_STATUS_MAP[detail.status] }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ detail.createdAt }}</el-descriptions-item>
        </el-descriptions>

        <h4 style="margin-top: 16px">检验记录</h4>
        <el-table :data="detail.inspections || []" border size="small">
          <el-table-column prop="inspectionNo" label="检验单号" />
          <el-table-column label="结论" width="80">
            <template #default="{ row }">
              <el-tag :type="row.conclusion === 'qualified' ? 'success' : row.conclusion === 'unqualified' ? 'danger' : 'info'" size="small">
                {{ row.conclusion === 'qualified' ? '合格' : row.conclusion === 'unqualified' ? '不合格' : '待检' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="totalPass" label="合格数" width="70" align="center" />
          <el-table-column prop="totalFail" label="不合格数" width="80" align="center" />
          <el-table-column prop="checkedAt" label="检验时间" />
        </el-table>

        <h4 style="margin-top: 16px">放行记录</h4>
        <el-table :data="detail.releases || []" border size="small">
          <el-table-column prop="releaseNumber" label="放行单号" />
          <el-table-column prop="customerName" label="客户" />
          <el-table-column label="状态" width="80">
            <template #default="{ row }">
              <el-tag size="small">{{ row.status }}</el-tag>
            </template>
          </el-table-column>
        </el-table>
      </template>
    </el-drawer>

    <!-- 新建批次对话框 -->
    <el-dialog v-model="createVisible" title="新建成品批次" width="520px">
      <el-alert
        title="批次来源说明"
        type="info"
        :closable="false"
        show-icon
        style="margin-bottom:12px"
      >
        手动创建批次为兜底方式。<br>
        常规流程：<strong>IPQC 关单</strong> 或 <strong>工单完工</strong> → 自动生成批次 → 自动进入 FQC 检验队列。
      </el-alert>
      <el-form :model="createForm" label-width="100px" size="small">
        <el-form-item label="批次号">
          <el-input v-model="createForm.batchCode" placeholder="留空自动生成 LOT-YYYYMMDD-X">
            <template #append>
              <el-button @click="handleGenerateNumber">自动生成</el-button>
            </template>
          </el-input>
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
        <el-form-item label="关联工单">
          <el-input-number v-model="createForm.workOrderId" :min="0" :max="99999" placeholder="可选" />
        </el-form-item>
        <el-form-item label="数量" required>
          <el-input-number v-model="createForm.quantity" :min="1" :max="999999" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">创建批次</el-button>
      </template>
    </el-dialog>
  </div>
</template>

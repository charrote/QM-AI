<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { inspectionApi, batchApi } from '@/api/fqc'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import type { FqcInspection, FqcInspectionDetail, CreateFqcInspection, SubmitFqcInspection, FqcInspectionItemSubmit } from '@/types/fqc'
import type { ProductBatch } from '@/types/fqc'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import { FQC_CONCLUSION_OPTIONS, FQC_INSPECTION_TYPE_OPTIONS } from '@/types/fqc'

defineOptions({ name: 'FqcInspectionsPage' })

// ─── 状态 ─────────────────────────────────────────
const loading = ref(false)
const list = ref<FqcInspection[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '', status: '' })
const detailVisible = ref(false)
const detail = ref<FqcInspectionDetail | null>(null)
const createVisible = ref(false)
const submitVisible = ref(false)
const submitId = ref(0)
const batches = ref<ProductBatch[]>([])

// ─── 创建表单 ─────────────────────────────────────
const createForm = reactive<CreateFqcInspection>({
  batchId: 0,
  inspectionType: 'full',
  sampleSize: 0,
  ac: 0,
  re: 0,
})

// ─── 提交表单 ─────────────────────────────────────
const submitForm = reactive({
  totalChecked: 0,
  totalPass: 0,
  totalFail: 0,
  items: [] as FqcInspectionItemSubmit[],
})

// ─── 获取列表 ─────────────────────────────────────
async function fetchList() {
  loading.value = true
  try {
    const res = await inspectionApi.list({ ...query })
    list.value = res.items
    total.value = res.total
  } catch { /* handled by interceptor */ }
  finally { loading.value = false }
}

// ─── 查看详情 ─────────────────────────────────────
async function openDetail(id: number) {
  try {
    detail.value = await inspectionApi.get(id)
    detailVisible.value = true
  } catch { /* */ }
}

// ─── 新建检验单 ─────────────────────────────────
async function openCreate() {
  createForm.batchId = 0
  createForm.inspectionType = 'full'
  createForm.sampleSize = 0
  createForm.ac = 0
  createForm.re = 0
  // 加载批次列表
  try {
    const res = await batchApi.list({ page: 1, pageSize: 100 })
    batches.value = res.items
  } catch { /* */ }
  createVisible.value = true
}

async function handleCreate() {
  if (!createForm.batchId) {
    ElMessage.warning('请选择批次')
    return
  }
  try {
    await inspectionApi.create(createForm)
    ElMessage.success('检验单创建成功')
    createVisible.value = false
    await fetchList()
  } catch { /* */ }
}

// ─── 提交检验结果 ─────────────────────────────────
async function openSubmit(id: number) {
  submitId.value = id
  try {
    const res = await inspectionApi.get(id)
    submitForm.totalChecked = res.totalChecked || res.sampleSize
    submitForm.totalPass = res.totalPass
    submitForm.totalFail = res.totalFail
    submitForm.items = (res.items || []).map(it => ({
      id: it.id,
      inspectionItemId: it.inspectionItemId,
      itemName: it.itemName,
      itemCode: it.itemCode,
      usl: it.usl,
      lsl: it.lsl,
      dataType: it.dataType,
      actualValue: it.actualValue,
      result: it.result,
      imageUrls: it.imageUrls,
    }))

    // Auto-load from plans if no items exist yet
    if (!submitForm.items || submitForm.items.length === 0) {
      try {
        const plans = await inspectionPlanApi.getByContext({
          inspectionType: 'FQC',
          productId: res.productId,
        })
        if (plans.length > 0) {
          const allItems = plans.flatMap(p => p.items)
          const seen = new Set<number>()
          const newItems: FqcInspectionItemSubmit[] = []
          for (const item of allItems) {
            if (!seen.has(item.inspectionItemId)) {
              seen.add(item.inspectionItemId)
              newItems.push({
                inspectionItemId: item.inspectionItemId,
                itemName: item.inspectionItemName,
                dataType: item.dataType,
                usl: item.usl ?? undefined,
                lsl: item.lsl ?? undefined,
                result: 'pending',
              })
            }
          }
          if (newItems.length > 0) {
            submitForm.items = newItems
          }
        }
      } catch (e) {
        console.error('加载检验计划失败', e)
      }
      // Fallback default item
      if (!submitForm.items || submitForm.items.length === 0) {
        submitForm.items.push({
          itemName: '外观检查',
          dataType: 'visual',
          result: 'pending',
        })
      }
    }
  } catch { return }
  submitVisible.value = true
}

async function handleSubmit() {
  try {
    await inspectionApi.submit(submitId.value, {
      totalChecked: submitForm.totalChecked,
      totalPass: submitForm.totalPass,
      totalFail: submitForm.totalFail,
      items: submitForm.items,
    })
    ElMessage.success('检验结果提交成功')
    submitVisible.value = false
    await fetchList()
  } catch { /* */ }
}

function addItem() {
  submitForm.items.push({
    itemName: '',
    dataType: 'numeric',
    result: 'pending',
  })
}

function removeItem(index: number) {
  submitForm.items.splice(index, 1)
}

// ─── 结论标签 ─────────────────────────────────────
function conclusionTag(type: string): string {
  const map: Record<string, string> = { pending: 'info', qualified: 'success', unqualified: 'danger' }
  return map[type] || 'info'
}

function conclusionLabel(type: string): string {
  const map: Record<string, string> = { pending: '待检验', qualified: '合格', unqualified: '不合格' }
  return map[type] || type
}

function inspectionTypeTag(type: string): string {
  return type === 'full' ? 'primary' : 'warning'
}

function inspectionTypeLabel(type: string): string {
  return type === 'full' ? '全检' : '抽检'
}

onMounted(fetchList)
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-form :inline="true" :model="query" size="small">
        <el-form-item>
          <el-input v-model="query.keyword" placeholder="搜索检验单号/批次号" clearable @keyup.enter="fetchList" />
        </el-form-item>
        <el-form-item>
          <el-select v-model="query.status" placeholder="检验结论" clearable @change="fetchList">
            <el-option v-for="opt in FQC_CONCLUSION_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetchList">查询</el-button>
        </el-form-item>
      </el-form>
      <div>
        <el-button type="primary" @click="openCreate">新建检验单</el-button>
      </div>
    </div>

    <el-table :data="list" v-loading="loading" stripe border style="width: 100%">
      <el-table-column prop="inspectionNo" label="检验单号" min-width="160" />
      <el-table-column prop="batchCode" label="批次号" min-width="150" />
      <el-table-column label="检验方式" width="80">
        <template #default="{ row }">
          <el-tag :type="inspectionTypeTag(row.inspectionType)" size="small">
            {{ inspectionTypeLabel(row.inspectionType) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="sampleSize" label="样本量" width="70" align="center" />
      <el-table-column prop="totalChecked" label="已检" width="60" align="center" />
      <el-table-column prop="totalPass" label="合格" width="60" align="center" />
      <el-table-column prop="totalFail" label="不合格" width="70" align="center" />
      <el-table-column label="结论" width="80">
        <template #default="{ row }">
          <el-tag :type="conclusionTag(row.conclusion)" size="small">
            {{ conclusionLabel(row.conclusion) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="checkedAt" label="检验时间" min-width="150" />
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button size="small" @click="openDetail(row.id)">详情</el-button>
          <el-button size="small" type="primary" v-if="row.conclusion === 'pending'" @click="openSubmit(row.id)">提交</el-button>
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
    <el-drawer v-model="detailVisible" title="检验详情" size="600px">
      <template v-if="detail">
        <el-descriptions :column="2" border size="small">
          <el-descriptions-item label="检验单号">{{ detail.inspectionNo }}</el-descriptions-item>
          <el-descriptions-item label="批次号">{{ detail.batchCode }}</el-descriptions-item>
          <el-descriptions-item label="产品名称">{{ detail.productName }}</el-descriptions-item>
          <el-descriptions-item label="检验方式">{{ inspectionTypeLabel(detail.inspectionType) }}</el-descriptions-item>
          <el-descriptions-item label="样本量/总数量">{{ detail.sampleSize }}/{{ detail.batchQuantity }}</el-descriptions-item>
          <el-descriptions-item label="检验结论">
            <el-tag :type="conclusionTag(detail.conclusion)">{{ conclusionLabel(detail.conclusion) }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="合格/不合格">{{ detail.totalPass }}/{{ detail.totalFail }}</el-descriptions-item>
          <el-descriptions-item label="检验时间">{{ detail.checkedAt || '-' }}</el-descriptions-item>
        </el-descriptions>
        <h4 style="margin-top: 16px">检验明细</h4>
        <el-table :data="detail.items || []" border size="small">
          <el-table-column prop="itemName" label="项目名称" />
          <el-table-column prop="usl" label="规格上限" width="90" />
          <el-table-column prop="lsl" label="规格下限" width="90" />
          <el-table-column prop="actualValue" label="实测值" width="90" />
          <el-table-column label="结果" width="70">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : row.result === 'fail' ? 'danger' : 'info'" size="small">
                {{ row.result === 'pass' ? '合格' : row.result === 'fail' ? '不合格' : '待检' }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </template>
    </el-drawer>

    <!-- 新建检验单对话框 -->
    <el-dialog v-model="createVisible" title="新建成品检验单" width="500px">
      <el-form :model="createForm" label-width="100px" size="small">
        <el-form-item label="批次">
          <el-select v-model="createForm.batchId" placeholder="请选择批次" style="width:100%">
            <el-option v-for="b in batches" :key="b.id" :label="`${b.batchCode} - ${b.productName}`" :value="b.id" />
          </el-select>
        </el-form-item>
        <el-form-item label="检验方式">
          <el-radio-group v-model="createForm.inspectionType">
            <el-radio value="full">全检</el-radio>
            <el-radio value="sampling">抽检</el-radio>
          </el-radio-group>
        </el-form-item>
        <template v-if="createForm.inspectionType === 'sampling'">
          <el-form-item label="样本量">
            <el-input-number v-model="createForm.sampleSize" :min="1" />
          </el-form-item>
          <el-form-item label="合格判定 Ac">
            <el-input-number v-model="createForm.ac" :min="0" />
          </el-form-item>
          <el-form-item label="不合格判定 Re">
            <el-input-number v-model="createForm.re" :min="1" />
          </el-form-item>
        </template>
      </el-form>
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- 提交检验结果对话框 -->
    <el-dialog v-model="submitVisible" title="提交检验结果" width="650px">
      <el-form :model="submitForm" label-width="100px" size="small">
        <el-form-item label="已检数量">
          <el-input-number v-model="submitForm.totalChecked" :min="0" />
        </el-form-item>
        <el-form-item label="合格数量">
          <el-input-number v-model="submitForm.totalPass" :min="0" />
        </el-form-item>
        <el-form-item label="不合格数量">
          <el-input-number v-model="submitForm.totalFail" :min="0" />
        </el-form-item>
      </el-form>

      <h4>检验明细</h4>
      <div v-for="(item, idx) in submitForm.items" :key="idx" style="border:1px solid #eee; padding:8px; margin-bottom:8px; border-radius:4px">
        <el-row :gutter="8">
          <el-col :span="8">
            <el-input v-model="item.itemName" placeholder="项目名称" size="small" />
          </el-col>
          <el-col :span="4">
            <el-input v-model="item.usl" placeholder="USL" size="small" type="number" />
          </el-col>
          <el-col :span="4">
            <el-input v-model="item.lsl" placeholder="LSL" size="small" type="number" />
          </el-col>
          <el-col :span="4">
            <el-input v-model="item.actualValue" placeholder="实测值" size="small" type="number" />
          </el-col>
          <el-col :span="4">
            <el-select v-model="item.result" size="small">
              <el-option label="合格" value="pass" />
              <el-option label="不合格" value="fail" />
            </el-select>
          </el-col>
        </el-row>
      </div>
      <el-button size="small" @click="addItem">+ 添加项目</el-button>

      <template #footer>
        <el-button @click="submitVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit">提交</el-button>
      </template>
    </el-dialog>
  </div>
</template>

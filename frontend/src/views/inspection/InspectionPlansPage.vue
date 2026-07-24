<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import { inspectionPlanApi } from '@/api/inspectionPlan'
import { inspectionItemApi } from '@/api/inspectionItem'
import type { InspectionPlan, InspectionPlanDetail, InspectionPlanItem, CreateInspectionPlan, CreateInspectionPlanItem, UpdateInspectionPlan } from '@/types/inspectionItem'
import type { InspectionItem } from '@/types/inspectionItem'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'InspectionPlansPage' })

const INSPECTION_TYPE_OPTIONS = [
  { value: 'IQC', label: 'IQC来料检验' },
  { value: 'IPQC', label: 'IPQC过程检验' },
  { value: 'FQC', label: 'FQC成品检验' },
  { value: 'OQC', label: 'OQC出货检验' },
]

const loading = ref(false)
const items = ref<InspectionPlan[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref<number | null>(null)

const inspectionItemOptions = ref<InspectionItem[]>([])

// ─── Form ──────────────────────────────────────────────────────

const form = reactive({
  planCode: '',
  planName: '',
  inspectionType: 'IQC' as string,
  description: '',
  productId: undefined as number | undefined,
  materialId: undefined as number | undefined,
  supplierId: undefined as number | undefined,
  customerId: undefined as number | undefined,
  processId: undefined as number | undefined,
  equipmentId: undefined as number | undefined,
})

const planItems = ref<CreateInspectionPlanItem[]>([])

const formRules = {
  planCode: [{ required: true, message: '请输入计划编码', trigger: 'blur' }],
  planName: [{ required: true, message: '请输入计划名称', trigger: 'blur' }],
  inspectionType: [{ required: true, message: '请选择检验类型', trigger: 'change' }],
}

// ─── Methods ───────────────────────────────────────────────────

async function loadInspectionItems() {
  try {
    inspectionItemOptions.value = await inspectionItemApi.getSelectList()
  } catch {
    // ignore
  }
}

async function loadData() {
  loading.value = true
  try {
    const res = await inspectionPlanApi.list(query)
    items.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  query.page = 1
  loadData()
}

function handlePageChange(page: number) {
  query.page = page
  loadData()
}

function openCreate() {
  dialogTitle.value = '新增检验计划'
  isEdit.value = false
  editingId.value = null
  form.planCode = ''
  form.planName = ''
  form.inspectionType = 'IQC'
  form.description = ''
  form.productId = undefined
  form.materialId = undefined
  form.supplierId = undefined
  form.customerId = undefined
  form.processId = undefined
  form.equipmentId = undefined
  planItems.value = []
  loadInspectionItems()
  dialogVisible.value = true
}

async function openEdit(id: number) {
  dialogTitle.value = '编辑检验计划'
  isEdit.value = true
  editingId.value = id
  try {
    const detail = await inspectionPlanApi.get(id)
    form.planCode = detail.planCode
    form.planName = detail.planName
    form.inspectionType = detail.inspectionType
    form.description = detail.description ?? ''
    form.productId = detail.productId ?? undefined
    form.materialId = detail.materialId ?? undefined
    form.supplierId = detail.supplierId ?? undefined
    form.customerId = detail.customerId ?? undefined
    form.processId = detail.processId ?? undefined
    form.equipmentId = detail.equipmentId ?? undefined
    planItems.value = detail.items.map(i => ({
      inspectionItemId: i.inspectionItemId,
      sortOrder: i.sortOrder,
      usl: i.usl,
      lsl: i.lsl,
      targetValue: i.targetValue,
      ucl: i.ucl,
      lcl: i.lcl,
      sampleSize: i.sampleSize,
      isRequired: i.isRequired,
    }))
    await loadInspectionItems()
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载检验计划详情失败')
  }
}

function addPlanItem() {
  planItems.value.push({
    inspectionItemId: 0,
    sortOrder: planItems.value.length + 1,
    usl: undefined,
    lsl: undefined,
    targetValue: undefined,
    ucl: undefined,
    lcl: undefined,
    sampleSize: undefined,
    isRequired: true,
  })
}

function removePlanItem(index: number) {
  planItems.value.splice(index, 1)
}

function onItemSelectChange(index: number) {
  const selectedId = planItems.value[index].inspectionItemId
  const item = inspectionItemOptions.value.find(opt => opt.id === selectedId)
  if (item) {
    planItems.value[index].usl = planItems.value[index].usl ?? item.usl
    planItems.value[index].lsl = planItems.value[index].lsl ?? item.lsl
    planItems.value[index].targetValue = planItems.value[index].targetValue ?? item.targetValue
  }
}

function getItemLabel(id: number): string {
  const item = inspectionItemOptions.value.find(opt => opt.id === id)
  return item ? `${item.itemCode} - ${item.itemName}` : ''
}

async function handleSave() {
  try {
    if (isEdit.value && editingId.value) {
      const updateDto: UpdateInspectionPlan = {
        planName: form.planName,
        description: form.description || undefined,
        productId: form.productId,
        materialId: form.materialId,
        supplierId: form.supplierId,
        customerId: form.customerId,
        processId: form.processId,
        equipmentId: form.equipmentId,
        isActive: true,
        items: planItems.value.map((pi, idx) => ({ ...pi, sortOrder: idx + 1 })),
      }
      await inspectionPlanApi.update(editingId.value, updateDto)
      ElMessage.success('更新成功')
    } else {
      const createDto: CreateInspectionPlan = {
        planCode: form.planCode,
        planName: form.planName,
        inspectionType: form.inspectionType,
        description: form.description || undefined,
        productId: form.productId,
        materialId: form.materialId,
        supplierId: form.supplierId,
        customerId: form.customerId,
        processId: form.processId,
        equipmentId: form.equipmentId,
        items: planItems.value.map((pi, idx) => ({ ...pi, sortOrder: idx + 1 })),
      }
      await inspectionPlanApi.create(createDto)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (err: any) {
    ElMessage.error(err?.response?.data?.message || '操作失败')
  }
}

async function handleDelete(id: number, name: string) {
  try {
    await ElMessageBox.confirm(`确定删除检验计划「${name}」？`, '确认删除', {
      type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消',
    })
    await inspectionPlanApi.delete(id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // cancelled
  }
}

function getTypeLabel(val: string) {
  const opt = INSPECTION_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <div class="inspection-plans-page">
    <div class="page-header">
      <h2>检验计划管理</h2>
      <p class="text-gray-400 text-sm">定义"什么维度组合→检验哪些项目"，贯通检验项目主数据与业务执行</p>
    </div>

    <el-card shadow="never" class="search-card">
      <div class="search-bar">
        <el-input v-model="query.keyword" placeholder="搜索计划编码/名称" clearable style="width: 240px" @keyup.enter="handleSearch">
          <template #prefix><el-icon><Search /></el-icon></template>
        </el-input>
        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="query.keyword = ''; handleSearch()">重置</el-button>
        <div class="search-spacer" />
        <el-button type="success" @click="openCreate">+ 新增检验计划</el-button>
      </div>
    </el-card>

    <el-card shadow="never" class="table-card">
      <el-table :data="items" v-loading="loading" stripe border style="width: 100%">
        <el-table-column prop="planCode" label="计划编码" width="130" />
        <el-table-column prop="planName" label="计划名称" min-width="150" />
        <el-table-column label="检验类型" width="130">
          <template #default="{ row }"><el-tag size="small" effect="plain">{{ getTypeLabel(row.inspectionType) }}</el-tag></template>
        </el-table-column>
        <el-table-column prop="productName" label="产品" min-width="120">
          <template #default="{ row }">{{ row.productName || '-' }}</template>
        </el-table-column>
        <el-table-column prop="supplierName" label="供应商" min-width="120">
          <template #default="{ row }">{{ row.supplierName || '-' }}</template>
        </el-table-column>
        <el-table-column prop="processName" label="工序" min-width="120">
          <template #default="{ row }">{{ row.processName || '-' }}</template>
        </el-table-column>
        <el-table-column label="检验项目数" width="100">
          <template #default="{ row }">{{ row.itemCount }}</template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small" effect="dark">{{ row.isActive ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="openEdit(row.id)">编辑</el-button>
            <el-button link size="small" type="danger" @click="handleDelete(row.id, row.planName)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-row">
        <el-pagination
          v-model:current-page="query.page"
          :page-size="query.pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadData"
          @current-change="handlePageChange"
        />
      </div>
    </el-card>

    <!-- 新增/编辑对话框 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="860px" destroy-on-close>
      <el-form :model="form" :rules="formRules" label-width="120px" label-position="top">
        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="计划编码" prop="planCode">
              <el-input v-model="form.planCode" placeholder="如 PLAN-IQC-001" :disabled="isEdit" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="计划名称" prop="planName">
              <el-input v-model="form.planName" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="检验类型" prop="inspectionType">
              <el-select v-model="form.inspectionType" style="width: 100%">
                <el-option v-for="o in INSPECTION_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">业务维度（设置后系统自动匹配）</el-divider>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="产品">
              <el-input v-model="form.productId" placeholder="产品ID" type="number" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="供应商">
              <el-input v-model="form.supplierId" placeholder="供应商ID" type="number" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="客户">
              <el-input v-model="form.customerId" placeholder="客户ID" type="number" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="工序">
              <el-input v-model="form.processId" placeholder="工序ID" type="number" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="设备">
              <el-input v-model="form.equipmentId" placeholder="设备ID" type="number" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="材料">
              <el-input v-model="form.materialId" placeholder="材料ID" type="number" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">检验项目清单</el-divider>

        <div class="plan-items-section">
          <div v-for="(pi, idx) in planItems" :key="idx" class="plan-item-row">
            <el-row :gutter="12" align="middle">
              <el-col :span="8">
                <el-form-item :label="`项目 ${idx + 1}`" :prop="`items.${idx}.inspectionItemId`"
                  :rules="[{ required: true, message: '请选择检验项目', trigger: 'change' }]">
                  <el-select v-model="pi.inspectionItemId" filterable style="width: 100%"
                    @change="onItemSelectChange(idx)">
                    <el-option v-for="opt in inspectionItemOptions" :key="opt.id"
                      :label="`${opt.itemCode} - ${opt.itemName}`" :value="opt.id" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="3">
                <el-form-item label="USL">
                  <el-input-number v-model="pi.usl" :precision="4" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="3">
                <el-form-item label="LSL">
                  <el-input-number v-model="pi.lsl" :precision="4" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="3">
                <el-form-item label="Target">
                  <el-input-number v-model="pi.targetValue" :precision="4" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="3">
                <el-form-item label="样本数">
                  <el-input-number v-model="pi.sampleSize" :min="1" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="2">
                <el-form-item label="必检">
                  <el-switch v-model="pi.isRequired" />
                </el-form-item>
              </el-col>
              <el-col :span="2" class="text-right">
                <el-button type="danger" :icon="'Delete'" circle size="small" @click="removePlanItem(idx)" />
              </el-col>
            </el-row>
          </div>

          <el-button type="primary" plain @click="addPlanItem" class="mt-2">
            + 添加检验项目
          </el-button>
        </div>

        <el-form-item label="描述" class="mt-4">
          <el-input v-model="form.description" type="textarea" :rows="2" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.inspection-plans-page { padding: 16px; }
.page-header { margin-bottom: 16px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; background: #e6f7ff; border-radius: 10px; }
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.text-gray-400 { color: #909399; }
.text-right { text-align: right; }
.search-card { margin-bottom: 12px; }
.search-bar { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.search-spacer { flex: 1; }
.table-card { flex: 1; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 8px; border-top: 1px solid #f0f0f0; }
.mt-2 { margin-top: 8px; }
.mt-4 { margin-top: 16px; }
.plan-item-row {
  padding: 12px;
  margin-bottom: 8px;
  background: #fafafa;
  border: 1px solid #e4e7ed;
  border-radius: 6px;
}
.plan-items-section { padding: 8px 0; }
</style>

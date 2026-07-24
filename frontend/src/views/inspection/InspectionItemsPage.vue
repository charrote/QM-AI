<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Checked, Search } from '@element-plus/icons-vue'
import { inspectionItemApi } from '@/api/inspectionItem'
import type { InspectionItem, InspectionItemDetail, CreateInspectionItem, UpdateInspectionItem } from '@/types/inspectionItem'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'InspectionItemsPage' })

const DATA_TYPE_OPTIONS = [
  { value: 'numeric', label: '数值型' },
  { value: 'visual', label: '外观型' },
  { value: 'attribute', label: '属性型' },
]

const CHART_TYPE_OPTIONS = [
  { value: '', label: '不使用控制图' },
  { value: 'Xbar_R', label: 'Xbar-R (均值-极差)' },
  { value: 'Xbar_S', label: 'Xbar-S (均值-标准差)' },
  { value: 'I_MR', label: 'I-MR (单值-移动极差)' },
  { value: 'P', label: 'p图 (不合格品率)' },
  { value: 'U', label: 'u图 (单位缺陷数)' },
  { value: 'C', label: 'c图 (缺陷数)' },
]

const loading = ref(false)
const items = ref<InspectionItem[]>([])
const total = ref(0)
const query = reactive<PagedRequest>({ page: 1, pageSize: 20, keyword: '' })

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref<number | null>(null)

const form = reactive<CreateInspectionItem>({
  itemCode: '',
  itemName: '',
  description: '',
  dataType: 'numeric',
  unit: '',
  usl: undefined,
  lsl: undefined,
  targetValue: undefined,
  ucl: undefined,
  lcl: undefined,
  dataCollectionParamCode: '',
  chartType: '',
  subgroupSize: undefined,
  inspectionMethod: '',
  sampleSize: undefined,
})

const formRules = {
  itemCode: [{ required: true, message: '请输入检验项目编码', trigger: 'blur' }],
  itemName: [{ required: true, message: '请输入检验项目名称', trigger: 'blur' }],
}

async function loadData() {
  loading.value = true
  try {
    const res = await inspectionItemApi.list(query)
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
  dialogTitle.value = '新增检验项目'
  isEdit.value = false
  editingId.value = null
  Object.assign(form, {
    itemCode: '', itemName: '', description: '', dataType: 'numeric',
    unit: '', usl: undefined, lsl: undefined, targetValue: undefined,
    ucl: undefined, lcl: undefined, dataCollectionParamCode: '',
    chartType: '', subgroupSize: undefined, inspectionMethod: '', sampleSize: undefined,
  })
  dialogVisible.value = true
}

async function openEdit(id: number) {
  dialogTitle.value = '编辑检验项目'
  isEdit.value = true
  editingId.value = id
  try {
    const detail = await inspectionItemApi.get(id)
    form.itemCode = detail.itemCode
    form.itemName = detail.itemName
    form.description = detail.description ?? ''
    form.dataType = detail.dataType
    form.unit = detail.unit ?? ''
    form.usl = detail.usl ?? undefined
    form.lsl = detail.lsl ?? undefined
    form.targetValue = detail.targetValue ?? undefined
    form.ucl = detail.ucl ?? undefined
    form.lcl = detail.lcl ?? undefined
    form.dataCollectionParamCode = detail.dataCollectionParamCode ?? ''
    form.chartType = detail.chartType ?? ''
    form.subgroupSize = detail.subgroupSize ?? undefined
    form.inspectionMethod = detail.inspectionMethod ?? ''
    form.sampleSize = detail.sampleSize ?? undefined
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载检验项目详情失败')
  }
}

async function handleSave() {
  try {
    if (isEdit.value && editingId.value) {
      await inspectionItemApi.update(editingId.value, form as UpdateInspectionItem)
      ElMessage.success('更新成功')
    } else {
      await inspectionItemApi.create(form)
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
    await ElMessageBox.confirm(`确定删除检验项目「${name}」？`, '确认删除', {
      type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消',
    })
    await inspectionItemApi.delete(id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // cancelled
  }
}

function getDataTypeLabel(val: string) {
  const opt = DATA_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

function getChartTypeLabel(val?: string) {
  if (!val) return '-'
  const opt = CHART_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

onMounted(loadData)
</script>

<template>
  <div class="inspection-items-page">
    <!-- 头部 -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon"><el-icon :size="28"><Checked /></el-icon></div>
        <div class="page-header-text">
          <h2>检验项目管理</h2>
          <p>品质部统一管理的检验项目主数据，贯通IQC/IPQC/FQC/SPC</p>
        </div>
      </div>
    </div>

    <!-- 查询栏 -->
    <el-card shadow="never" class="search-card">
      <div class="search-bar">
        <el-input v-model="query.keyword" placeholder="搜索项目编码/名称" clearable style="width: 240px" @keyup.enter="handleSearch">
          <template #prefix><el-icon><Search /></el-icon></template>
        </el-input>
        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="query.keyword = ''; handleSearch()">重置</el-button>
        <div class="search-spacer" />
        <el-button type="success" @click="openCreate">+ 新增检验项目</el-button>
      </div>
    </el-card>

    <!-- 数据表格 -->
    <el-card shadow="never" class="table-card">
      <el-table :data="items" v-loading="loading" stripe border style="width: 100%">
        <el-table-column prop="itemCode" label="项目编码" width="120" />
        <el-table-column prop="itemName" label="项目名称" min-width="150" />
        <el-table-column label="数据类型" width="100">
          <template #default="{ row }"><el-tag size="small" effect="plain">{{ getDataTypeLabel(row.dataType) }}</el-tag></template>
        </el-table-column>
        <el-table-column prop="unit" label="单位" width="70" />
        <el-table-column label="规格范围" width="180">
          <template #default="{ row }">
            <span v-if="row.lsl != null || row.usl != null" class="spec-range">{{ row.lsl ?? '-' }} ~ {{ row.usl ?? '-' }}</span>
            <span v-else class="text-gray-400">-</span>
          </template>
        </el-table-column>
        <el-table-column label="目标值" width="100">
          <template #default="{ row }">{{ row.targetValue ?? '-' }}</template>
        </el-table-column>
        <el-table-column label="控制图" width="130">
          <template #default="{ row }">{{ getChartTypeLabel(row.chartType) }}</template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.isActive ? 'success' : 'danger'" size="small" effect="dark">{{ row.isActive ? '启用' : '停用' }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="170">
          <template #default="{ row }">{{ row.createdAt }}</template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="openEdit(row.id)">编辑</el-button>
            <el-button link size="small" type="danger" @click="handleDelete(row.id, row.itemName)">删除</el-button>
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
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="780px" destroy-on-close>
      <el-form :model="form" :rules="formRules" label-width="120px" label-position="top">
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="项目编码" prop="itemCode">
              <el-input v-model="form.itemCode" placeholder="如 DIM-001" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="项目名称" prop="itemName">
              <el-input v-model="form.itemName" placeholder="如 直径测量" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="数据类型">
              <el-select v-model="form.dataType" style="width: 100%">
                <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="单位">
              <el-input v-model="form.unit" placeholder="如 mm" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="检验方法">
              <el-input v-model="form.inspectionMethod" placeholder="如 游标卡尺" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">规格限 & 目标</el-divider>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="规格上限 (USL)">
              <el-input-number v-model="form.usl" :min="undefined" :max="undefined" :precision="4" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="规格下限 (LSL)">
              <el-input-number v-model="form.lsl" :min="undefined" :max="undefined" :precision="4" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="目标值 (Target)">
              <el-input-number v-model="form.targetValue" :min="undefined" :max="undefined" :precision="4" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">管理限 (SPC控制图用)</el-divider>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="管理上限 (UCL)">
              <el-input-number v-model="form.ucl" :min="undefined" :max="undefined" :precision="4" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="管理下限 (LCL)">
              <el-input-number v-model="form.lcl" :min="undefined" :max="undefined" :precision="4" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="默认抽样数量">
              <el-input-number v-model="form.sampleSize" :min="1" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-divider content-position="left">数采 & SPC配置</el-divider>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="数采参数编码">
              <el-input v-model="form.dataCollectionParamCode" placeholder="关联 dynamic_params.code" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="控制图类型">
              <el-select v-model="form.chartType" style="width: 100%" clearable>
                <el-option v-for="o in CHART_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="子组大小">
              <el-input-number v-model="form.subgroupSize" :min="1" :max="20" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="描述">
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
.inspection-items-page { padding: 16px; }
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
.spec-range { font-family: monospace; font-size: 13px; color: #409eff; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 8px; border-top: 1px solid #f0f0f0; }
</style>

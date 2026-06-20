<script setup lang="ts">
defineOptions({ name: 'BasicData' })

import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  productApi, bomApi, processApi, routingApi,
  inspectionStandardApi, defectCodeApi, equipmentApi,
  toolApi, supplierApi, customerApi,
} from '@/api/basicData'
import type {
  Product, Process, Equipment, Tool, Supplier, Customer,
  Bom, Routing, InspectionStandard, DefectCode,
  CreateProduct, CreateProcess, CreateEquipment, CreateTool,
  CreateSupplier, CreateCustomer, CreateBom, CreateRouting,
  CreateInspectionStandard, CreateDefectCode,
} from '@/types/basicData'

// ─── 实体配置 ──────────────────────────────────────────
interface EntityConfig<T, C> {
  key: string
  label: string
  api: {
    list: (params?: any) => Promise<any>
    get: (id: number) => Promise<T>
    create: (data: C) => Promise<T>
    update: (id: number, data: any) => Promise<T>
    delete: (id: number) => Promise<void>
  }
  columns: { prop: string; label: string; width?: string; formatter?: (row: T) => string }[]
  defaultCreate: () => C
  icon?: string
  keywordSearch?: string[]
}

type EntityMap = {
  product: Product
  process: Process
  routing: Routing
  bom: Bom
  standard: InspectionStandard
  defect: DefectCode
  equipment: Equipment
  tool: Tool
  supplier: Supplier
  customer: Customer
}

type EntityName = keyof EntityMap

const activeEntity = ref<EntityName>('product')

const entityConfigs: Record<EntityName, EntityConfig<any, any>> = {
  product: {
    key: 'product', label: '产品管理',
    api: productApi,
    columns: [
      { prop: 'code', label: '产品编码', width: '120' },
      { prop: 'name', label: '产品名称' },
      { prop: 'category', label: '产品类别', width: '120' },
      { prop: 'unit', label: '单位', width: '80' },
      { prop: 'defaultInspectionLevel', label: '检验水平', width: '100' },
      { prop: 'defaultAql', label: 'AQL', width: '80' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: Product) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateProduct => ({
      code: '', name: '', description: '', unit: '', category: '',
      defaultInspectionLevel: 'II', defaultAql: 1.0,
    }),
  },
  process: {
    key: 'process', label: '工序管理',
    api: processApi,
    columns: [
      { prop: 'code', label: '工序编码', width: '120' },
      { prop: 'name', label: '工序名称' },
      { prop: 'processType', label: '工序类型', width: '100' },
      { prop: 'department', label: '所属部门', width: '120' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: Process) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateProcess => ({
      code: '', name: '', description: '', processType: '', department: '',
    }),
  },
  routing: {
    key: 'routing', label: '工艺路线',
    api: routingApi,
    columns: [
      { prop: 'productName', label: '产品名称' },
      { prop: 'stepOrder', label: '步骤', width: '60' },
      { prop: 'processName', label: '工序名称' },
      { prop: 'code', label: '路线编号', width: '120' },
      { prop: 'standardTimeMinutes', label: '标准工时(分)', width: '120' },
    ],
    defaultCreate: (): CreateRouting => ({
      productId: 0, code: '', description: '', stepOrder: 1, processId: 0, standardTimeMinutes: undefined,
    }),
  },
  bom: {
    key: 'bom', label: 'BOM清单',
    api: bomApi,
    columns: [
      { prop: 'productName', label: '所属产品' },
      { prop: 'materialCode', label: '物料编码', width: '120' },
      { prop: 'materialName', label: '物料名称' },
      { prop: 'quantity', label: '用量', width: '80' },
      { prop: 'unit', label: '单位', width: '60' },
      { prop: 'level', label: '层级', width: '60' },
    ],
    defaultCreate: (): CreateBom => ({
      productId: 0, materialCode: '', materialName: '', quantity: 1, unit: '', level: 0, remark: '',
    }),
  },
  standard: {
    key: 'standard', label: '检验标准',
    api: inspectionStandardApi,
    columns: [
      { prop: 'code', label: '标准编码', width: '120' },
      { prop: 'name', label: '标准名称' },
      { prop: 'inspectionType', label: '检验类型', width: '100' },
      { prop: 'itemName', label: '检验项目' },
      { prop: 'productName', label: '关联产品' },
      { prop: 'usl', label: 'USL', width: '80' },
      { prop: 'lsl', label: 'LSL', width: '80' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: InspectionStandard) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateInspectionStandard => ({
      code: '', name: '', description: '', inspectionType: 'IQC',
      productId: undefined, processId: undefined, itemName: '',
      usl: undefined, lsl: undefined, target: undefined, unit: '',
      inspectionMethod: '', samplingFrequency: '',
    }),
  },
  defect: {
    key: 'defect', label: '不良代码',
    api: defectCodeApi,
    columns: [
      { prop: 'code', label: '代码', width: '100' },
      { prop: 'name', label: '名称' },
      { prop: 'defectType', label: '不良类别', width: '100' },
      { prop: 'severity', label: '严重等级', width: '100' },
      {
        prop: 'isReworkable', label: '可返工', width: '80',
        formatter: (r: DefectCode) => r.isReworkable ? '是' : '否',
      },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: DefectCode) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateDefectCode => ({
      code: '', name: '', description: '', defectType: '', severity: 'MI', isReworkable: false,
    }),
  },
  equipment: {
    key: 'equipment', label: '设备管理',
    api: equipmentApi,
    columns: [
      { prop: 'code', label: '设备编码', width: '120' },
      { prop: 'name', label: '设备名称' },
      { prop: 'model', label: '型号', width: '120' },
      { prop: 'productionLine', label: '产线', width: '100' },
      { prop: 'workshop', label: '车间', width: '100' },
      { prop: 'status', label: '状态', width: '80' },
      { prop: 'equipmentType', label: '类型', width: '100' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: Equipment) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateEquipment => ({
      code: '', name: '', model: '', productionLine: '', workshop: '',
      equipmentType: '', hasMqttConnection: false, mqttTopicPrefix: '',
    }),
  },
  tool: {
    key: 'tool', label: '刀具管理',
    api: toolApi,
    columns: [
      { prop: 'code', label: '刀具编码', width: '120' },
      { prop: 'name', label: '刀具名称' },
      { prop: 'toolType', label: '刀具类型', width: '100' },
      { prop: 'designLife', label: '设计寿命', width: '100' },
      { prop: 'lifeUnit', label: '寿命单位', width: '80' },
      { prop: 'currentLife', label: '当前寿命', width: '100' },
      { prop: 'supplier', label: '供应商' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: Tool) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateTool => ({
      code: '', name: '', model: '', toolType: '', designLife: undefined,
      lifeUnit: 'cycles', currentLife: 0, supplier: '',
    }),
  },
  supplier: {
    key: 'supplier', label: '供应商管理',
    api: supplierApi,
    columns: [
      { prop: 'code', label: '供应商编码', width: '120' },
      { prop: 'name', label: '供应商名称' },
      { prop: 'contactPerson', label: '联系人', width: '100' },
      { prop: 'contactPhone', label: '联系电话', width: '130' },
      { prop: 'grade', label: '等级', width: '60' },
      { prop: 'supplyCategory', label: '供应类别' },
      { prop: 'score', label: '评分', width: '70' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: Supplier) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateSupplier => ({
      code: '', name: '', address: '', contactPerson: '', contactPhone: '',
      email: '', grade: 'B', supplyCategory: '',
    }),
  },
  customer: {
    key: 'customer', label: '客户管理',
    api: customerApi,
    columns: [
      { prop: 'code', label: '客户编码', width: '120' },
      { prop: 'name', label: '客户名称' },
      { prop: 'contactPerson', label: '联系人', width: '100' },
      { prop: 'contactPhone', label: '联系电话', width: '130' },
      {
        prop: 'isActive', label: '状态', width: '80',
        formatter: (r: Customer) => r.isActive ? '启用' : '停用',
      },
    ],
    defaultCreate: (): CreateCustomer => ({
      code: '', name: '', address: '', contactPerson: '', contactPhone: '', email: '',
    }),
  },
}

const route = useRoute()

// 路由路径 → 实体 key 映射
const pathToEntity: Record<string, EntityName> = {
  '/basic-data/product': 'product',
  '/basic-data/process': 'process',
  '/basic-data/routing': 'routing',
  '/basic-data/bom': 'bom',
  '/basic-data/standard': 'standard',
  '/basic-data/defect': 'defect',
  '/basic-data/equipment': 'equipment',
  '/basic-data/tool': 'tool',
  '/basic-data/supplier': 'supplier',
  '/basic-data/customer': 'customer',
}

// 根据当前路由设置激活的实体
function syncEntityFromRoute() {
  const entity = pathToEntity[route.path]
  if (entity && entity !== activeEntity.value) {
    activeEntity.value = entity
  }
}

// 监听路由变化
watch(() => route.path, () => {
  syncEntityFromRoute()
  loadData()
})

const config = computed(() => entityConfigs[activeEntity.value])

// ─── 数据状态 ──────────────────────────────────────────
const tableData = ref<any[]>([])
const loading = ref(false)
const total = ref(0)
const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref(0)
const formRef = ref()
const formData = reactive<any>({})

const lookupProducts = ref<Product[]>([])
const lookupProcesses = ref<Process[]>([])

// ─── 数据加载 ──────────────────────────────────────────
async function loadData() {
  loading.value = true
  try {
    const res = await config.value.api.list({
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
    })
    tableData.value = res.items
    total.value = res.total
  } finally {
    loading.value = false
  }
}

async function loadLookups() {
  try {
    const [prodRes, procRes] = await Promise.all([
      productApi.list({ page: 1, pageSize: 999 }),
      processApi.list({ page: 1, pageSize: 999 }),
    ])
    lookupProducts.value = prodRes.items
    lookupProcesses.value = procRes.items
  } catch { /* ignore */ }
}

// ─── 搜索 ──────────────────────────────────────────────
function handleSearch() {
  page.value = 1
  loadData()
}

function handleReset() {
  searchKeyword.value = ''
  page.value = 1
  loadData()
}

// ─── 新增/编辑 ─────────────────────────────────────────
function openCreate() {
  isEdit.value = false
  editingId.value = 0
  dialogTitle.value = `新增${config.value.label}`
  const defaults = config.value.defaultCreate()
  Object.assign(formData, defaults)
  dialogVisible.value = true
}

async function openEdit(row: any) {
  isEdit.value = true
  editingId.value = row.id
  dialogTitle.value = `编辑${config.value.label}`
  try {
    const detail = await config.value.api.get(row.id)
    Object.assign(formData, detail)
  } catch {
    Object.assign(formData, row)
  }
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  try {
    if (isEdit.value) {
      await config.value.api.update(editingId.value, formData)
      ElMessage.success('更新成功')
    } else {
      await config.value.api.create(formData)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch { /* error handled by interceptor */ }
}

// ─── 删除 ──────────────────────────────────────────────
async function handleDelete(row: any) {
  try {
    await ElMessageBox.confirm(`确认删除 "${row.name || row.code}"？此操作不可恢复。`, '确认删除', {
      type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消',
    })
    await config.value.api.delete(row.id)
    ElMessage.success('删除成功')
    loadData()
  } catch { /* cancelled or error */ }
}

// ─── 分页 ──────────────────────────────────────────────
function handlePageChange(p: number) {
  page.value = p
  loadData()
}

// ─── 初始化 ────────────────────────────────────────────
onMounted(() => {
  syncEntityFromRoute()
  loadData()
  loadLookups()
})
</script>

<template>
  <div class="basic-data">
    <!-- 工具栏 -->
    <div class="toolbar">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索编码/名称..."
        clearable
        style="width: 260px"
        @keyup.enter="handleSearch"
      />
      <el-button type="primary" @click="handleSearch">搜索</el-button>
      <el-button @click="handleReset">重置</el-button>
      <el-button type="primary" class="btn-add" @click="openCreate">
        + 新增{{ config.label }}
      </el-button>
    </div>

    <!-- 数据表格 -->
    <el-table
      :data="tableData"
      v-loading="loading"
      border
      stripe
      style="width: 100%"
      size="small"
      empty-text="暂无数据"
    >
      <el-table-column type="index" label="#" width="50" fixed />
      <el-table-column
        v-for="col in config.columns"
        :key="col.prop"
        :prop="col.prop"
        :label="col.label"
        :width="col.width"
        :formatter="col.formatter"
        show-overflow-tooltip
      />
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button size="small" type="primary" link @click="openEdit(row)">编辑</el-button>
          <el-button size="small" type="danger" link @click="handleDelete(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
    <div class="pagination-wrap">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next"
        @current-change="handlePageChange"
      />
    </div>

    <!-- 新增/编辑 Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="740px"
      :close-on-click-modal="false"
      destroy-on-close
      class="data-dialog"
    >
      <el-form
        ref="formRef"
        :model="formData"
        label-width="100px"
        size="small"
        class="dialog-form"
      >
        <!-- 动态渲染不同实体的表单字段 -->
        <template v-if="activeEntity === 'product'">
          <el-form-item label="产品编码" prop="code" :rules="[{ required: true, message: '请输入产品编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="产品名称" prop="name" :rules="[{ required: true, message: '请输入产品名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="产品类别" prop="category">
            <el-input v-model="formData.category" />
          </el-form-item>
          <el-form-item label="单位" prop="unit">
            <el-input v-model="formData.unit" />
          </el-form-item>
          <el-form-item label="描述" prop="description">
            <el-input v-model="formData.description" type="textarea" :rows="2" />
          </el-form-item>
          <el-form-item label="检验水平" prop="defaultInspectionLevel">
            <el-select v-model="formData.defaultInspectionLevel" style="width: 100%">
              <el-option label="I (一般水平I)" value="I" />
              <el-option label="II (一般水平II)" value="II" />
              <el-option label="III (一般水平III)" value="III" />
              <el-option label="S-1 (特殊水平)" value="S-1" />
              <el-option label="S-2 (特殊水平)" value="S-2" />
              <el-option label="S-3 (特殊水平)" value="S-3" />
              <el-option label="S-4 (特殊水平)" value="S-4" />
            </el-select>
          </el-form-item>
          <el-form-item label="AQL值" prop="defaultAql">
            <el-input-number v-model="formData.defaultAql" :min="0" :max="100" :step="0.01" style="width: 100%" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'process'">
          <el-form-item label="工序编码" prop="code" :rules="[{ required: true, message: '请输入工序编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="工序名称" prop="name" :rules="[{ required: true, message: '请输入工序名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="工序类型" prop="processType">
            <el-select v-model="formData.processType" style="width: 100%">
              <el-option label="加工" value="加工" />
              <el-option label="检验" value="检验" />
              <el-option label="装配" value="装配" />
              <el-option label="包装" value="包装" />
            </el-select>
          </el-form-item>
          <el-form-item label="所属部门" prop="department">
            <el-input v-model="formData.department" />
          </el-form-item>
          <el-form-item label="描述" prop="description">
            <el-input v-model="formData.description" type="textarea" :rows="2" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'routing'">
          <el-form-item label="产品" prop="productId" :rules="[{ required: true, message: '请选择产品' }]">
            <el-select v-model="formData.productId" filterable style="width: 100%">
              <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="工序" prop="processId" :rules="[{ required: true, message: '请选择工序' }]">
            <el-select v-model="formData.processId" filterable style="width: 100%">
              <el-option v-for="p in lookupProcesses" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="路线编号" prop="code">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="步骤顺序" prop="stepOrder">
            <el-input-number v-model="formData.stepOrder" :min="1" style="width: 100%" />
          </el-form-item>
          <el-form-item label="标准工时(分)" prop="standardTimeMinutes">
            <el-input-number v-model="formData.standardTimeMinutes" :min="0" :step="0.5" style="width: 100%" />
          </el-form-item>
          <el-form-item label="描述" prop="description">
            <el-input v-model="formData.description" type="textarea" :rows="2" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'bom'">
          <el-form-item label="所属产品" prop="productId" :rules="[{ required: true, message: '请选择产品' }]">
            <el-select v-model="formData.productId" filterable style="width: 100%">
              <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="物料编码" prop="materialCode" :rules="[{ required: true, message: '请输入物料编码' }]">
            <el-input v-model="formData.materialCode" />
          </el-form-item>
          <el-form-item label="物料名称" prop="materialName" :rules="[{ required: true, message: '请输入物料名称' }]">
            <el-input v-model="formData.materialName" />
          </el-form-item>
          <el-form-item label="用量" prop="quantity">
            <el-input-number v-model="formData.quantity" :min="0.01" :step="0.1" style="width: 100%" />
          </el-form-item>
          <el-form-item label="单位" prop="unit">
            <el-input v-model="formData.unit" />
          </el-form-item>
          <el-form-item label="层级" prop="level">
            <el-input-number v-model="formData.level" :min="0" style="width: 100%" />
          </el-form-item>
          <el-form-item label="备注" prop="remark">
            <el-input v-model="formData.remark" type="textarea" :rows="2" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'standard'">
          <el-form-item label="标准编码" prop="code" :rules="[{ required: true, message: '请输入标准编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="标准名称" prop="name" :rules="[{ required: true, message: '请输入标准名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="检验类型" prop="inspectionType">
            <el-select v-model="formData.inspectionType" style="width: 100%">
              <el-option label="IQC来料检验" value="IQC" />
              <el-option label="IPQC过程检验" value="IPQC" />
              <el-option label="FQC成品检验" value="FQC" />
              <el-option label="OQC出货检验" value="OQC" />
            </el-select>
          </el-form-item>
          <el-form-item label="检验项目" prop="itemName">
            <el-input v-model="formData.itemName" />
          </el-form-item>
          <el-form-item label="关联产品" prop="productId">
            <el-select v-model="formData.productId" filterable clearable style="width: 100%">
              <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="关联工序" prop="processId">
            <el-select v-model="formData.processId" filterable clearable style="width: 100%">
              <el-option v-for="p in lookupProcesses" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
            </el-select>
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="USL" prop="usl">
                <el-input-number v-model="formData.usl" :min="0" :step="0.01" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="LSL" prop="lsl">
                <el-input-number v-model="formData.lsl" :min="0" :step="0.01" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Target" prop="target">
                <el-input-number v-model="formData.target" :step="0.01" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="单位" prop="unit">
            <el-input v-model="formData.unit" />
          </el-form-item>
          <el-form-item label="检验方法" prop="inspectionMethod">
            <el-input v-model="formData.inspectionMethod" />
          </el-form-item>
          <el-form-item label="抽样频率" prop="samplingFrequency">
            <el-input v-model="formData.samplingFrequency" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'defect'">
          <el-form-item label="不良代码" prop="code" :rules="[{ required: true, message: '请输入不良代码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="名称" prop="name" :rules="[{ required: true, message: '请输入名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="不良类别" prop="defectType">
            <el-select v-model="formData.defectType" style="width: 100%">
              <el-option label="外观" value="外观" />
              <el-option label="尺寸" value="尺寸" />
              <el-option label="功能" value="功能" />
              <el-option label="材料" value="材料" />
              <el-option label="其他" value="其他" />
            </el-select>
          </el-form-item>
          <el-form-item label="严重等级" prop="severity">
            <el-select v-model="formData.severity" style="width: 100%">
              <el-option label="CR (严重)" value="CR" />
              <el-option label="MA (主要)" value="MA" />
              <el-option label="MI (次要)" value="MI" />
            </el-select>
          </el-form-item>
          <el-form-item label="可返工" prop="isReworkable" class="full-width">
            <el-switch v-model="formData.isReworkable" />
          </el-form-item>
          <el-form-item label="描述" prop="description">
            <el-input v-model="formData.description" type="textarea" :rows="2" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'equipment'">
          <el-form-item label="设备编码" prop="code" :rules="[{ required: true, message: '请输入设备编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="设备名称" prop="name" :rules="[{ required: true, message: '请输入设备名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="型号" prop="model">
            <el-input v-model="formData.model" />
          </el-form-item>
          <el-form-item label="设备类型" prop="equipmentType">
            <el-select v-model="formData.equipmentType" style="width: 100%">
              <el-option label="CNC" value="CNC" />
              <el-option label="PLC" value="PLC" />
              <el-option label="检测设备" value="检测设备" />
              <el-option label="其他" value="其他" />
            </el-select>
          </el-form-item>
          <el-form-item label="产线" prop="productionLine">
            <el-input v-model="formData.productionLine" />
          </el-form-item>
          <el-form-item label="车间" prop="workshop">
            <el-input v-model="formData.workshop" />
          </el-form-item>
          <el-form-item label="MQTT连接" prop="hasMqttConnection" class="full-width">
            <el-switch v-model="formData.hasMqttConnection" />
          </el-form-item>
          <el-form-item label="MQTT Topic" prop="mqttTopicPrefix" v-if="formData.hasMqttConnection">
            <el-input v-model="formData.mqttTopicPrefix" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'tool'">
          <el-form-item label="刀具编码" prop="code" :rules="[{ required: true, message: '请输入刀具编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="刀具名称" prop="name" :rules="[{ required: true, message: '请输入刀具名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="型号" prop="model">
            <el-input v-model="formData.model" />
          </el-form-item>
          <el-form-item label="刀具类型" prop="toolType">
            <el-select v-model="formData.toolType" style="width: 100%">
              <el-option label="车刀" value="车刀" />
              <el-option label="铣刀" value="铣刀" />
              <el-option label="钻头" value="钻头" />
              <el-option label="磨具" value="磨具" />
              <el-option label="其他" value="其他" />
            </el-select>
          </el-form-item>
          <el-form-item label="设计寿命" prop="designLife">
            <el-input-number v-model="formData.designLife" :min="0" style="width: 100%" />
          </el-form-item>
          <el-form-item label="寿命单位" prop="lifeUnit">
            <el-select v-model="formData.lifeUnit" style="width: 100%">
              <el-option label="次数 (cycles)" value="cycles" />
              <el-option label="小时 (hours)" value="hours" />
            </el-select>
          </el-form-item>
          <el-form-item label="当前寿命" prop="currentLife">
            <el-input-number v-model="formData.currentLife" :min="0" style="width: 100%" />
          </el-form-item>
          <el-form-item label="供应商" prop="supplier">
            <el-input v-model="formData.supplier" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'supplier'">
          <el-form-item label="供应商编码" prop="code" :rules="[{ required: true, message: '请输入供应商编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="供应商名称" prop="name" :rules="[{ required: true, message: '请输入供应商名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="联系人" prop="contactPerson">
            <el-input v-model="formData.contactPerson" />
          </el-form-item>
          <el-form-item label="联系电话" prop="contactPhone">
            <el-input v-model="formData.contactPhone" />
          </el-form-item>
          <el-form-item label="邮箱" prop="email">
            <el-input v-model="formData.email" />
          </el-form-item>
          <el-form-item label="地址" prop="address">
            <el-input v-model="formData.address" type="textarea" :rows="2" />
          </el-form-item>
          <el-form-item label="供应商等级" prop="grade">
            <el-select v-model="formData.grade" style="width: 100%">
              <el-option label="A级" value="A" />
              <el-option label="B级" value="B" />
              <el-option label="C级" value="C" />
              <el-option label="D级" value="D" />
            </el-select>
          </el-form-item>
          <el-form-item label="供应类别" prop="supplyCategory">
            <el-input v-model="formData.supplyCategory" />
          </el-form-item>
          <el-form-item label="评分" prop="score" v-if="isEdit">
            <el-input-number v-model="formData.score" :min="0" :max="100" style="width: 100%" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>

        <template v-if="activeEntity === 'customer'">
          <el-form-item label="客户编码" prop="code" :rules="[{ required: true, message: '请输入客户编码' }]">
            <el-input v-model="formData.code" />
          </el-form-item>
          <el-form-item label="客户名称" prop="name" :rules="[{ required: true, message: '请输入客户名称' }]">
            <el-input v-model="formData.name" />
          </el-form-item>
          <el-form-item label="联系人" prop="contactPerson">
            <el-input v-model="formData.contactPerson" />
          </el-form-item>
          <el-form-item label="联系电话" prop="contactPhone">
            <el-input v-model="formData.contactPhone" />
          </el-form-item>
          <el-form-item label="邮箱" prop="email">
            <el-input v-model="formData.email" />
          </el-form-item>
          <el-form-item label="地址" prop="address">
            <el-input v-model="formData.address" type="textarea" :rows="2" />
          </el-form-item>
          <el-form-item label="启用" prop="isActive" class="full-width" v-if="isEdit">
            <el-switch v-model="formData.isActive" />
          </el-form-item>
        </template>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.basic-data {
  background: var(--bg-white, #fff);
  border-radius: 8px;
  padding: 16px;
}

.toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}

.btn-add {
  margin-left: auto;
}

.pagination-wrap {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}

.dialog-form {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0 12px;
  max-height: 58vh;
  overflow-y: auto;
  padding: 4px 0;
}

/* 全宽字段：textarea、单行开关、分组行 */
.dialog-form :deep(.full-width) {
  grid-column: 1 / -1;
}

/* Textarea 在网格中占全宽 */
.dialog-form :deep(.el-textarea) {
  grid-column: 1 / -1;
}

/* 双列行（USL/LSL/Target 内部分三列）不再受网格约束 */
.dialog-form :deep(.el-row) {
  grid-column: 1 / -1;
  width: 100%;
}

/* 表单内边距微调 */
.dialog-form :deep(.el-form-item) {
  margin-bottom: 14px;
}

.dialog-form :deep(.el-form-item__label) {
  font-weight: 500;
  color: var(--text-primary, #303133);
}

/* Dialog 头部样式 */
.data-dialog :deep(.el-dialog__header) {
  padding: 16px 24px;
  border-bottom: 1px solid var(--border-color, #e4e7ed);
  margin: 0;
}

.data-dialog :deep(.el-dialog__title) {
  font-size: 16px;
  font-weight: 600;
}

.data-dialog :deep(.el-dialog__body) {
  padding: 20px 24px 0;
}

.data-dialog :deep(.el-dialog__footer) {
  padding: 12px 24px 16px;
  border-top: 1px solid var(--border-color, #e4e7ed);
}
</style>

<script setup lang="ts">
defineOptions({ name: 'BasicData' })

import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Setting, Box, Monitor, Document, Upload,
  WarningFilled, Tools, Wrench, Shop, User,
  Search, List, Refresh, Plus, Check,
} from '@element-plus/icons-vue'
import {
  productApi, bomApi, processApi, routingApi,
  inspectionStandardApi, defectCodeApi, equipmentApi,
  toolApi, supplierApi, customerApi,
} from '@/api/basicData'
import { sysDictApi } from '@/api/sysDict'
import { organizationApi } from '@/api/organization'
import type { SysDictItem } from '@/types/sysDict'
import type { OrganizationTreeNode } from '@/types/organization'
import { LEVEL_CONFIG } from '@/types/organization'
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
const router = useRouter()

// 实体图标映射
const entityIcons: Record<EntityName, any> = {
  product: Box,
  process: Monitor,
  routing: Document,
  bom: Upload,
  standard: Document,
  defect: WarningFilled,
  equipment: Tools,
  tool: Wrench,
  supplier: Shop,
  customer: User,
}

// 实体路由路径映射
const entityRoutePath: Record<EntityName, string> = {
  product: '/basic-data/product',
  process: '/basic-data/process',
  routing: '/basic-data/routing',
  bom: '/basic-data/bom',
  standard: '/basic-data/standard',
  defect: '/basic-data/defect',
  equipment: '/basic-data/equipment',
  tool: '/basic-data/tool',
  supplier: '/basic-data/supplier',
  customer: '/basic-data/customer',
}

function switchEntity(key: EntityName) {
  activeEntity.value = key
  router.replace(entityRoutePath[key])
  loadData()
}

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

// ─── 字典下拉数据 ──────────────────────────────────────
const dictData = ref<Record<string, SysDictItem[]>>({})
const orgTree = ref<OrganizationTreeNode[]>([])
const orgWorkshops = ref<{ id: number; name: string; code: string }[]>([])
const orgLines = ref<{ id: number; name: string; code: string }[]>([])

// 组织层级选项（用于树下拉选择）
const orgOptions = computed(() => {
  const result: { id: number; name: string; level: string; levelLabel: string; padding: number }[] = []
  function walk(nodes: OrganizationTreeNode[], depth: number) {
    for (const n of nodes) {
      result.push({
        id: n.id,
        name: n.name,
        level: n.level,
        levelLabel: LEVEL_CONFIG[n.level]?.label || n.level,
        padding: depth * 20,
      })
      if (n.children?.length) walk(n.children, depth + 1)
    }
  }
  walk(orgTree.value, 0)
  return result
})

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
    const [prodRes, procRes, dictRes, orgTreeRes] = await Promise.all([
      productApi.list({ page: 1, pageSize: 999 }),
      processApi.list({ page: 1, pageSize: 999 }),
      sysDictApi.getBatch([
        'equipment_type', 'process_type', 'defect_category', 'severity',
        'inspection_type', 'product_category', 'tool_type', 'supply_category',
        'material_unit',
      ]),
      organizationApi.tree(),
    ])
    lookupProducts.value = prodRes.items
    lookupProcesses.value = procRes.items
    dictData.value = dictRes
    orgTree.value = orgTreeRes

    // 提取车间和产线
    function collectByLevel(nodes: OrganizationTreeNode[], level: string) {
      const result: { id: number; name: string; code: string }[] = []
      for (const n of nodes) {
        if (n.level === level) result.push({ id: n.id, name: n.name, code: n.code })
        if (n.children?.length) result.push(...collectByLevel(n.children, level))
      }
      return result
    }
    orgWorkshops.value = collectByLevel(orgTreeRes, 'workshop')
    orgLines.value = collectByLevel(orgTreeRes, 'line')
  } catch { /* ignore */ }
}

// ─── 字典辅助函数 ──────────────────────────────────────
function getDictOptions(typeCode: string): SysDictItem[] {
  return dictData.value[typeCode] || []
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
    // 如果获取详情失败，手动赋值已知字段而非浅拷贝可能不完整的 row
    formData.code = row.code
    formData.name = row.name
    formData.description = row.description
    formData.isActive = row.isActive !== undefined ? row.isActive : true
    if (row.unit !== undefined) formData.unit = row.unit
    if (row.category !== undefined) formData.category = row.category
    if (row.defaultInspectionLevel !== undefined) formData.defaultInspectionLevel = row.defaultInspectionLevel
    if (row.defaultAql !== undefined) formData.defaultAql = row.defaultAql
    if (row.equipmentType !== undefined) formData.equipmentType = row.equipmentType
    if (row.workshop !== undefined) formData.workshop = row.workshop
    if (row.productionLine !== undefined) formData.productionLine = row.productionLine
    if (row.hasMqttConnection !== undefined) formData.hasMqttConnection = row.hasMqttConnection
    if (row.mqttTopicPrefix !== undefined) formData.mqttTopicPrefix = row.mqttTopicPrefix
    if (row.toolType !== undefined) formData.toolType = row.toolType
    if (row.designLife !== undefined) formData.designLife = row.designLife
    if (row.lifeUnit !== undefined) formData.lifeUnit = row.lifeUnit
    if (row.currentLife !== undefined) formData.currentLife = row.currentLife
    if (row.supplier !== undefined) formData.supplier = row.supplier
    if (row.grade !== undefined) formData.grade = row.grade
    if (row.score !== undefined) formData.score = row.score
    if (row.supplyCategory !== undefined) formData.supplyCategory = row.supplyCategory
    if (row.contactPerson !== undefined) formData.contactPerson = row.contactPerson
    if (row.contactPhone !== undefined) formData.contactPhone = row.contactPhone
    if (row.email !== undefined) formData.email = row.email
    if (row.address !== undefined) formData.address = row.address
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
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="page-header__main">
        <el-icon class="page-header__icon"><Setting /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">基础数据管理</h2>
          <p class="page-header__subtitle">管理产品、工序、供应商等基础数据</p>
        </div>
      </div>
      <div class="page-header__actions">
        <el-input
          v-model="searchKeyword"
          placeholder="搜索编码/名称..."
          clearable
          class="header-search"
          @keyup.enter="handleSearch"
        >
          <template #prefix>
            <el-icon><Search /></el-icon>
          </template>
        </el-input>
      </div>
    </div>

    <!-- 实体导航 -->
    <div class="entity-nav">
      <div
        v-for="item in entityTabs"
        :key="item.key"
        class="entity-nav__item"
        :class="{ 'entity-nav__item--active': activeEntity === item.key }"
        @click="switchEntity(item.key)"
      >
        <el-icon class="entity-nav__icon">
          <component :is="item.icon" />
        </el-icon>
        <span class="entity-nav__label">{{ item.label }}</span>
      </div>
    </div>

    <!-- 数据卡片 -->
    <div class="data-card">
      <div class="data-card__header">
        <div class="data-card__title">
          <el-icon><List /></el-icon>
          <span>{{ config.label }}列表</span>
          <el-tag v-if="total" type="info" size="small" class="data-card__count">
            共 {{ total }} 条
          </el-tag>
        </div>
        <div class="data-card__toolbar">
          <el-button @click="handleReset" text>
            <el-icon><Refresh /></el-icon>重置
          </el-button>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>新增{{ config.label }}
          </el-button>
        </div>
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
        class="data-card__table"
        @row-click="openEdit"
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
        >
          <template v-if="col.prop === 'isActive'" #default="{ row }">
            <el-tag
              :type="row.isActive ? 'success' : 'danger'"
              size="small"
              effect="light"
            >
              {{ row.isActive ? '启用' : '停用' }}
            </el-tag>
          </template>
          <template v-if="col.prop === 'isReworkable'" #default="{ row }">
            <el-tag
              :type="row.isReworkable ? 'success' : 'info'"
              size="small"
              effect="light"
            >
              {{ row.isReworkable ? '是' : '否' }}
            </el-tag>
          </template>
        </template>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" link @click.stop="openEdit(row)">编辑</el-button>
            <el-button size="small" type="danger" link @click.stop="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          :small="true"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          class="data-card__pagination"
        />
      </div>
    </div>

    <!-- 新增/编辑 Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="760px"
      :close-on-click-modal="false"
      destroy-on-close
      class="data-dialog"
    >
      <div class="dialog-body-wrap">
        <el-form
          ref="formRef"
          :model="formData"
          label-width="100px"
          label-position="top"
          size="default"
          class="dialog-form"
        >
          <!-- 产品 -->
          <template v-if="activeEntity === 'product'">
            <div class="form-section">
              <div class="form-section__title">基本信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="产品编码" prop="code" :rules="[{ required: true, message: '请输入产品编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入产品编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="产品名称" prop="name" :rules="[{ required: true, message: '请输入产品名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入产品名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="产品类别" prop="category">
                    <el-select v-model="formData.category" filterable clearable placeholder="请选择" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('product_category')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="单位" prop="unit">
                    <el-select v-model="formData.unit" filterable clearable placeholder="请选择" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('material_unit')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <div class="form-section">
              <div class="form-section__title">检验参数</div>
              <el-form-item label="描述" prop="description">
                <el-input v-model="formData.description" type="textarea" :rows="2" placeholder="请输入产品描述" />
              </el-form-item>
              <el-row :gutter="16">
                <el-col :span="12">
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
                </el-col>
                <el-col :span="12">
                  <el-form-item label="AQL值" prop="defaultAql">
                    <el-input-number v-model="formData.defaultAql" :min="0" :max="100" :step="0.01" :precision="2" style="width: 100%" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="所属组织" prop="orgId">
                <el-select v-model="formData.orgId" filterable clearable placeholder="选择组织" style="width: 100%">
                  <el-option
                    v-for="org in orgOptions"
                    :key="org.id"
                    :label="`${'  '.repeat(org.padding / 20)}[${org.levelLabel}] ${org.name}`"
                    :value="org.id"
                  />
                </el-select>
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 工序 -->
          <template v-if="activeEntity === 'process'">
            <div class="form-section">
              <div class="form-section__title">基本信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="工序编码" prop="code" :rules="[{ required: true, message: '请输入工序编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入工序编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="工序名称" prop="name" :rules="[{ required: true, message: '请输入工序名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入工序名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="工序类型" prop="processType">
                <el-select v-model="formData.processType" filterable clearable placeholder="请选择" style="width: 100%">
                  <el-option
                    v-for="opt in getDictOptions('process_type')"
                    :key="opt.itemValue"
                    :label="opt.itemLabel"
                    :value="opt.itemValue"
                  />
                </el-select>
              </el-form-item>
            </div>
            <div class="form-section">
              <div class="form-section__title">归属信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="所属车间" prop="orgId">
                    <el-select v-model="formData.orgId" filterable clearable placeholder="选择车间" style="width: 100%">
                      <el-option
                        v-for="ws in orgWorkshops"
                        :key="ws.id"
                        :label="`[${ws.code}] ${ws.name}`"
                        :value="ws.id"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="所属部门" prop="department">
                    <el-input v-model="formData.department" placeholder="请输入部门" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="描述" prop="description">
                <el-input v-model="formData.description" type="textarea" :rows="2" placeholder="请输入描述" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 工艺路线 -->
          <template v-if="activeEntity === 'routing'">
            <div class="form-section">
              <div class="form-section__title">路线配置</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="产品" prop="productId" :rules="[{ required: true, message: '请选择产品' }]">
                    <el-select v-model="formData.productId" filterable placeholder="请选择产品" style="width: 100%">
                      <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="工序" prop="processId" :rules="[{ required: true, message: '请选择工序' }]">
                    <el-select v-model="formData.processId" filterable placeholder="请选择工序" style="width: 100%">
                      <el-option v-for="p in lookupProcesses" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="路线编号" prop="code">
                    <el-input v-model="formData.code" placeholder="请输入路线编号" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="步骤顺序" prop="stepOrder">
                    <el-input-number v-model="formData.stepOrder" :min="1" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="标准工时(分)" prop="standardTimeMinutes">
                <el-input-number v-model="formData.standardTimeMinutes" :min="0" :step="0.5" :precision="1" :style="{ width: '100%' }" />
              </el-form-item>
              <el-form-item label="描述" prop="description">
                <el-input v-model="formData.description" type="textarea" :rows="2" placeholder="请输入描述" />
              </el-form-item>
            </div>
          </template>

          <!-- BOM -->
          <template v-if="activeEntity === 'bom'">
            <div class="form-section">
              <div class="form-section__title">物料信息</div>
              <el-form-item label="所属产品" prop="productId" :rules="[{ required: true, message: '请选择产品' }]">
                <el-select v-model="formData.productId" filterable placeholder="请选择产品" style="width: 100%">
                  <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                </el-select>
              </el-form-item>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="物料编码" prop="materialCode" :rules="[{ required: true, message: '请输入物料编码' }]">
                    <el-input v-model="formData.materialCode" placeholder="请输入物料编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="物料名称" prop="materialName" :rules="[{ required: true, message: '请输入物料名称' }]">
                    <el-input v-model="formData.materialName" placeholder="请输入物料名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="8">
                  <el-form-item label="用量" prop="quantity">
                    <el-input-number v-model="formData.quantity" :min="0.01" :step="0.1" :precision="2" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="单位" prop="unit">
                    <el-select v-model="formData.unit" filterable clearable placeholder="请选择" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('material_unit')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="层级" prop="level">
                    <el-input-number v-model="formData.level" :min="0" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="备注" prop="remark">
                <el-input v-model="formData.remark" type="textarea" :rows="2" placeholder="请输入备注" />
              </el-form-item>
            </div>
          </template>

          <!-- 检验标准 -->
          <template v-if="activeEntity === 'standard'">
            <div class="form-section">
              <div class="form-section__title">标准信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="标准编码" prop="code" :rules="[{ required: true, message: '请输入标准编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入标准编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="标准名称" prop="name" :rules="[{ required: true, message: '请输入标准名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入标准名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="检验类型" prop="inspectionType">
                    <el-select v-model="formData.inspectionType" style="width: 100%">
                      <el-option label="IQC来料检验" value="IQC" />
                      <el-option label="IPQC过程检验" value="IPQC" />
                      <el-option label="FQC成品检验" value="FQC" />
                      <el-option label="OQC出货检验" value="OQC" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="检验项目" prop="itemName">
                    <el-input v-model="formData.itemName" placeholder="请输入检验项目" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="关联产品" prop="productId">
                    <el-select v-model="formData.productId" filterable clearable placeholder="选择产品" style="width: 100%">
                      <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="关联工序" prop="processId">
                    <el-select v-model="formData.processId" filterable clearable placeholder="选择工序" style="width: 100%">
                      <el-option v-for="p in lookupProcesses" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <div class="form-section">
              <div class="form-section__title">公差参数</div>
              <el-row :gutter="12">
                <el-col :span="8">
                  <el-form-item label="USL" prop="usl">
                    <el-input-number v-model="formData.usl" :min="0" :step="0.01" :precision="4" :style="{ width: '100%' }" placeholder="上限" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="LSL" prop="lsl">
                    <el-input-number v-model="formData.lsl" :min="0" :step="0.01" :precision="4" :style="{ width: '100%' }" placeholder="下限" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="Target" prop="target">
                    <el-input-number v-model="formData.target" :step="0.01" :precision="4" :style="{ width: '100%' }" placeholder="目标值" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="单位" prop="unit">
                    <el-input v-model="formData.unit" placeholder="请输入单位" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="抽样频率" prop="samplingFrequency">
                    <el-input v-model="formData.samplingFrequency" placeholder="如: 100%" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="检验方法" prop="inspectionMethod">
                <el-input v-model="formData.inspectionMethod" type="textarea" :rows="2" placeholder="请输入检验方法" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 不良代码 -->
          <template v-if="activeEntity === 'defect'">
            <div class="form-section">
              <div class="form-section__title">不良信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="不良代码" prop="code" :rules="[{ required: true, message: '请输入不良代码' }]">
                    <el-input v-model="formData.code" placeholder="请输入不良代码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="名称" prop="name" :rules="[{ required: true, message: '请输入名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="不良类别" prop="defectType">
                    <el-select v-model="formData.defectType" filterable clearable placeholder="请选择" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('defect_category')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="严重等级" prop="severity">
                    <el-select v-model="formData.severity" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('severity')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <div class="form-section">
              <div class="form-section__title">其他设置</div>
              <el-form-item class="form-actions">
                <el-switch v-model="formData.isReworkable" active-text="可返工" inactive-text="不可返工" />
              </el-form-item>
              <el-form-item label="描述" prop="description">
                <el-input v-model="formData.description" type="textarea" :rows="2" placeholder="请输入描述" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 设备 -->
          <template v-if="activeEntity === 'equipment'">
            <div class="form-section">
              <div class="form-section__title">设备信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="设备编码" prop="code" :rules="[{ required: true, message: '请输入设备编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入设备编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="设备名称" prop="name" :rules="[{ required: true, message: '请输入设备名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入设备名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="型号" prop="model">
                    <el-input v-model="formData.model" placeholder="请输入型号" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="设备类型" prop="equipmentType">
                    <el-select v-model="formData.equipmentType" filterable clearable placeholder="请选择" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('equipment_type')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <div class="form-section">
              <div class="form-section__title">归属信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="车间" prop="workshopId">
                    <el-select v-model="formData.workshopId" filterable clearable placeholder="选择车间" style="width: 100%"
                      @change="(val: number) => { formData.lineId = undefined; orgLines.filter(l => l.id === val) }">
                      <el-option
                        v-for="ws in orgWorkshops"
                        :key="ws.id"
                        :label="`[${ws.code}] ${ws.name}`"
                        :value="ws.id"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="产线" prop="lineId">
                    <el-select v-model="formData.lineId" filterable clearable placeholder="选择产线" style="width: 100%">
                      <el-option
                        v-for="line in orgLines"
                        :key="line.id"
                        :label="`[${line.code}] ${line.name}`"
                        :value="line.id"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="所属组织" prop="orgId">
                <el-select v-model="formData.orgId" filterable clearable placeholder="选择组织" style="width: 100%">
                  <el-option
                    v-for="org in orgOptions"
                    :key="org.id"
                    :label="`${'  '.repeat(org.padding / 20)}[${org.levelLabel}] ${org.name}`"
                    :value="org.id"
                  />
                </el-select>
              </el-form-item>
            </div>
            <div class="form-section">
              <div class="form-section__title">MQTT 连接</div>
              <el-form-item class="form-actions">
                <el-switch v-model="formData.hasMqttConnection" active-text="已连接" inactive-text="未连接" />
              </el-form-item>
              <el-form-item v-if="formData.hasMqttConnection" label="MQTT Topic" prop="mqttTopicPrefix">
                <el-input v-model="formData.mqttTopicPrefix" placeholder="请输入 MQTT Topic 前缀" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 刀具 -->
          <template v-if="activeEntity === 'tool'">
            <div class="form-section">
              <div class="form-section__title">刀具信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="刀具编码" prop="code" :rules="[{ required: true, message: '请输入刀具编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入刀具编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="刀具名称" prop="name" :rules="[{ required: true, message: '请输入刀具名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入刀具名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="型号" prop="model">
                    <el-input v-model="formData.model" placeholder="请输入型号" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="刀具类型" prop="toolType">
                    <el-select v-model="formData.toolType" filterable clearable placeholder="请选择" style="width: 100%">
                      <el-option
                        v-for="opt in getDictOptions('tool_type')"
                        :key="opt.itemValue"
                        :label="opt.itemLabel"
                        :value="opt.itemValue"
                      />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <div class="form-section">
              <div class="form-section__title">寿命参数</div>
              <el-row :gutter="16">
                <el-col :span="8">
                  <el-form-item label="设计寿命" prop="designLife">
                    <el-input-number v-model="formData.designLife" :min="0" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="寿命单位" prop="lifeUnit">
                    <el-select v-model="formData.lifeUnit" style="width: 100%">
                      <el-option label="次数 (cycles)" value="cycles" />
                      <el-option label="小时 (hours)" value="hours" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="当前寿命" prop="currentLife">
                    <el-input-number v-model="formData.currentLife" :min="0" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="供应商" prop="supplier">
                    <el-input v-model="formData.supplier" placeholder="请输入供应商" />
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 供应商 -->
          <template v-if="activeEntity === 'supplier'">
            <div class="form-section">
              <div class="form-section__title">基本信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="供应商编码" prop="code" :rules="[{ required: true, message: '请输入供应商编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入供应商编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="供应商名称" prop="name" :rules="[{ required: true, message: '请输入供应商名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入供应商名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="联系人" prop="contactPerson">
                    <el-input v-model="formData.contactPerson" placeholder="请输入联系人" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="联系电话" prop="contactPhone">
                    <el-input v-model="formData.contactPhone" placeholder="请输入联系电话" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="邮箱" prop="email">
                    <el-input v-model="formData.email" placeholder="请输入邮箱" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="供应商等级" prop="grade">
                    <el-select v-model="formData.grade" style="width: 100%">
                      <el-option label="A级" value="A" />
                      <el-option label="B级" value="B" />
                      <el-option label="C级" value="C" />
                      <el-option label="D级" value="D" />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <div class="form-section">
              <div class="form-section__title">其他信息</div>
              <el-form-item label="供应类别" prop="supplyCategory">
                <el-select v-model="formData.supplyCategory" filterable clearable placeholder="请选择" style="width: 100%">
                  <el-option
                    v-for="opt in getDictOptions('supply_category')"
                    :key="opt.itemValue"
                    :label="opt.itemLabel"
                    :value="opt.itemValue"
                  />
                </el-select>
              </el-form-item>
              <el-form-item label="地址" prop="address">
                <el-input v-model="formData.address" type="textarea" :rows="2" placeholder="请输入地址" />
              </el-form-item>
              <el-form-item v-if="isEdit" label="评分" prop="score">
                <el-input-number v-model="formData.score" :min="0" :max="100" :style="{ width: '100%' }" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 客户 -->
          <template v-if="activeEntity === 'customer'">
            <div class="form-section">
              <div class="form-section__title">客户信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="客户编码" prop="code" :rules="[{ required: true, message: '请输入客户编码' }]">
                    <el-input v-model="formData.code" placeholder="请输入客户编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="客户名称" prop="name" :rules="[{ required: true, message: '请输入客户名称' }]">
                    <el-input v-model="formData.name" placeholder="请输入客户名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="联系人" prop="contactPerson">
                    <el-input v-model="formData.contactPerson" placeholder="请输入联系人" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="联系电话" prop="contactPhone">
                    <el-input v-model="formData.contactPhone" placeholder="请输入联系电话" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="邮箱" prop="email">
                <el-input v-model="formData.email" placeholder="请输入邮箱" />
              </el-form-item>
              <el-form-item label="地址" prop="address">
                <el-input v-model="formData.address" type="textarea" :rows="2" placeholder="请输入地址" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>
        </el-form>
      </div>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSave" :loading="saving">
            <el-icon><Check /></el-icon>保存
          </el-button>
        </div>
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

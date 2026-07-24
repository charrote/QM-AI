<script setup lang="ts">
defineOptions({ name: 'BasicData' })

import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Setting, Box, Monitor, Document, Upload,
  WarningFilled, Tools, Shop, User,
  Search, List, Refresh, Plus, Check,
} from '@element-plus/icons-vue'
import {
  productApi, bomApi, processApi,
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
  Bom, InspectionStandard, DefectCode,
  CreateProduct, CreateProcess, CreateEquipment, CreateTool,
  CreateSupplier, CreateCustomer, CreateBom,
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
      defaultInspectionLevel: 'II', defaultAql: 1.0, orgId: undefined,
    }),
  },
  process: {
    key: 'process', label: '工序管理',
    api: processApi,
    columns: [
      { prop: 'code', label: '工序编码', width: '120' },
      { prop: 'name', label: '工序名称', align: 'left', minWidth: '150' },
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
  bom: Upload,
  standard: Document,
  defect: WarningFilled,
  equipment: Tools,
  tool: Tools,
  supplier: Shop,
  customer: User,
}

// 实体路由路径映射
const entityRoutePath: Record<EntityName, string> = {
  product: '/basic-data/product',
  process: '/basic-data/process',
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

const entityTabs = computed(() =>
  Object.entries(entityConfigs).map(([key, cfg]) => ({
    key: key as EntityName,
    label: cfg.label,
    icon: entityIcons[key as EntityName],
  }))
)

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
const saving = ref(false)

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
  isEdit.value = true
  editingId.value = 0
  dialogTitle.value = `新增${config.value.label}`
  const defaults = config.value.defaultCreate()
  Object.assign(formData, defaults)
  dialogVisible.value = true
}

async function openDetail(row: any) {
  isEdit.value = false
  editingId.value = row.id
  dialogTitle.value = `${config.value.label}详情`
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
  saving.value = true
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
  } catch { /* error handled by interceptor */ } finally {
    saving.value = false
  }
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
        @row-click="openDetail"
      >
        <el-table-column type="index" label="#" width="50" fixed class-name="index-cell" />
        <el-table-column
          v-for="col in config.columns"
          :key="col.prop"
          :prop="col.prop"
          :label="col.label"
          :width="col.width"
          :align="col.align || 'center'"
          :formatter="col.formatter"
          show-overflow-tooltip
          :class-name="col.prop === 'isActive' || col.prop === 'isReworkable' ? 'status-cell' : ''"
        />
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

    <!-- 新增/编辑 Drawer -->
    <el-drawer
      v-model="dialogVisible"
      :title="dialogTitle"
      size="600px"
      direction="rtl"
      :close-on-click-modal="true"
      destroy-on-close
      class="data-drawer"
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
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入产品编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="产品名称" prop="name" :rules="[{ required: true, message: '请输入产品名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入产品名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="产品类别" prop="category">
                    <el-select v-model="formData.category" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                    <el-select v-model="formData.unit" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                <el-input v-model="formData.description" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入产品描述" />
              </el-form-item>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="检验水平" prop="defaultInspectionLevel">
                    <el-select v-model="formData.defaultInspectionLevel" :disabled="!isEdit" style="width: 100%">
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
                    <el-input-number v-model="formData.defaultAql" :disabled="!isEdit" :min="0" :max="100" :step="0.01" :precision="2" style="width: 100%" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="所属组织" prop="orgId">
                <el-select v-model="formData.orgId" :disabled="!isEdit" filterable clearable placeholder="选择组织" style="width: 100%">
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
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 工序 -->
          <template v-if="activeEntity === 'process'">
            <div class="form-section">
              <div class="form-section__title">基本信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="工序编码" prop="code" :rules="[{ required: true, message: '请输入工序编码' }]">
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入工序编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="工序名称" prop="name" :rules="[{ required: true, message: '请输入工序名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入工序名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="工序类型" prop="processType">
                <el-select v-model="formData.processType" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                    <el-select v-model="formData.orgId" :disabled="!isEdit" filterable clearable placeholder="选择车间" style="width: 100%">
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
                    <el-input v-model="formData.department" :disabled="!isEdit" placeholder="请输入部门" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="描述" prop="description">
                <el-input v-model="formData.description" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入描述" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- BOM -->
          <template v-if="activeEntity === 'bom'">
            <div class="form-section">
              <div class="form-section__title">物料信息</div>
              <el-form-item label="所属产品" prop="productId" :rules="[{ required: true, message: '请选择产品' }]">
                <el-select v-model="formData.productId" :disabled="!isEdit" filterable placeholder="请选择产品" style="width: 100%">
                  <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                </el-select>
              </el-form-item>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="物料编码" prop="materialCode" :rules="[{ required: true, message: '请输入物料编码' }]">
                    <el-input v-model="formData.materialCode" :disabled="!isEdit" placeholder="请输入物料编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="物料名称" prop="materialName" :rules="[{ required: true, message: '请输入物料名称' }]">
                    <el-input v-model="formData.materialName" :disabled="!isEdit" placeholder="请输入物料名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="8">
                  <el-form-item label="用量" prop="quantity">
                    <el-input-number v-model="formData.quantity" :disabled="!isEdit" :min="0.01" :step="0.1" :precision="2" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="单位" prop="unit">
                    <el-select v-model="formData.unit" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                    <el-input-number v-model="formData.level" :disabled="!isEdit" :min="0" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="备注" prop="remark">
                <el-input v-model="formData.remark" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入备注" />
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
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入标准编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="标准名称" prop="name" :rules="[{ required: true, message: '请输入标准名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入标准名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="检验类型" prop="inspectionType">
                    <el-select v-model="formData.inspectionType" :disabled="!isEdit" style="width: 100%">
                      <el-option label="IQC来料检验" value="IQC" />
                      <el-option label="IPQC过程检验" value="IPQC" />
                      <el-option label="FQC成品检验" value="FQC" />
                      <el-option label="OQC出货检验" value="OQC" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="检验项目" prop="itemName">
                    <el-input v-model="formData.itemName" :disabled="!isEdit" placeholder="请输入检验项目" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="关联产品" prop="productId">
                    <el-select v-model="formData.productId" :disabled="!isEdit" filterable clearable placeholder="选择产品" style="width: 100%">
                      <el-option v-for="p in lookupProducts" :key="p.id" :label="`[${p.code}] ${p.name}`" :value="p.id" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="关联工序" prop="processId">
                    <el-select v-model="formData.processId" :disabled="!isEdit" filterable clearable placeholder="选择工序" style="width: 100%">
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
                    <el-input-number v-model="formData.usl" :disabled="!isEdit" :min="0" :step="0.01" :precision="4" :style="{ width: '100%' }" placeholder="上限" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="LSL" prop="lsl">
                    <el-input-number v-model="formData.lsl" :disabled="!isEdit" :min="0" :step="0.01" :precision="4" :style="{ width: '100%' }" placeholder="下限" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="Target" prop="target">
                    <el-input-number v-model="formData.target" :disabled="!isEdit" :step="0.01" :precision="4" :style="{ width: '100%' }" placeholder="目标值" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="单位" prop="unit">
                    <el-input v-model="formData.unit" :disabled="!isEdit" placeholder="请输入单位" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="抽样频率" prop="samplingFrequency">
                    <el-input v-model="formData.samplingFrequency" :disabled="!isEdit" placeholder="如: 100%" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="检验方法" prop="inspectionMethod">
                <el-input v-model="formData.inspectionMethod" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入检验方法" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 不良代码 -->
          <template v-if="activeEntity === 'defect'">
            <div class="form-section">
              <div class="form-section__title">不良信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="不良代码" prop="code" :rules="[{ required: true, message: '请输入不良代码' }]">
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入不良代码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="名称" prop="name" :rules="[{ required: true, message: '请输入名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="不良类别" prop="defectType">
                    <el-select v-model="formData.defectType" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                    <el-select v-model="formData.severity" :disabled="!isEdit" style="width: 100%">
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
                <el-switch v-model="formData.isReworkable" :disabled="!isEdit" active-text="可返工" inactive-text="不可返工" />
              </el-form-item>
              <el-form-item label="描述" prop="description">
                <el-input v-model="formData.description" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入描述" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 设备 -->
          <template v-if="activeEntity === 'equipment'">
            <div class="form-section">
              <div class="form-section__title">设备信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="设备编码" prop="code" :rules="[{ required: true, message: '请输入设备编码' }]">
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入设备编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="设备名称" prop="name" :rules="[{ required: true, message: '请输入设备名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入设备名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="型号" prop="model">
                    <el-input v-model="formData.model" :disabled="!isEdit" placeholder="请输入型号" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="设备类型" prop="equipmentType">
                    <el-select v-model="formData.equipmentType" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                    <el-select v-model="formData.workshopId" :disabled="!isEdit" filterable clearable placeholder="选择车间" style="width: 100%"
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
                    <el-select v-model="formData.lineId" :disabled="!isEdit" filterable clearable placeholder="选择产线" style="width: 100%">
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
                <el-select v-model="formData.orgId" :disabled="!isEdit" filterable clearable placeholder="选择组织" style="width: 100%">
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
                <el-switch v-model="formData.hasMqttConnection" :disabled="!isEdit" active-text="已连接" inactive-text="未连接" />
              </el-form-item>
              <el-form-item v-if="formData.hasMqttConnection" label="MQTT Topic" prop="mqttTopicPrefix">
                <el-input v-model="formData.mqttTopicPrefix" :disabled="!isEdit" placeholder="请输入 MQTT Topic 前缀" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 刀具 -->
          <template v-if="activeEntity === 'tool'">
            <div class="form-section">
              <div class="form-section__title">刀具信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="刀具编码" prop="code" :rules="[{ required: true, message: '请输入刀具编码' }]">
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入刀具编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="刀具名称" prop="name" :rules="[{ required: true, message: '请输入刀具名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入刀具名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="型号" prop="model">
                    <el-input v-model="formData.model" :disabled="!isEdit" placeholder="请输入型号" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="刀具类型" prop="toolType">
                    <el-select v-model="formData.toolType" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
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
                    <el-input-number v-model="formData.designLife" :disabled="!isEdit" :min="0" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="寿命单位" prop="lifeUnit">
                    <el-select v-model="formData.lifeUnit" :disabled="!isEdit" style="width: 100%">
                      <el-option label="次数 (cycles)" value="cycles" />
                      <el-option label="小时 (hours)" value="hours" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="当前寿命" prop="currentLife">
                    <el-input-number v-model="formData.currentLife" :disabled="!isEdit" :min="0" :style="{ width: '100%' }" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="供应商" prop="supplier">
                    <el-input v-model="formData.supplier" :disabled="!isEdit" placeholder="请输入供应商" />
                  </el-form-item>
                </el-col>
              </el-row>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 供应商 -->
          <template v-if="activeEntity === 'supplier'">
            <div class="form-section">
              <div class="form-section__title">基本信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="供应商编码" prop="code" :rules="[{ required: true, message: '请输入供应商编码' }]">
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入供应商编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="供应商名称" prop="name" :rules="[{ required: true, message: '请输入供应商名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入供应商名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="联系人" prop="contactPerson">
                    <el-input v-model="formData.contactPerson" :disabled="!isEdit" placeholder="请输入联系人" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="联系电话" prop="contactPhone">
                    <el-input v-model="formData.contactPhone" :disabled="!isEdit" placeholder="请输入联系电话" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="邮箱" prop="email">
                    <el-input v-model="formData.email" :disabled="!isEdit" placeholder="请输入邮箱" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="供应商等级" prop="grade">
                    <el-select v-model="formData.grade" :disabled="!isEdit" style="width: 100%">
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
                <el-select v-model="formData.supplyCategory" :disabled="!isEdit" filterable clearable placeholder="请选择" style="width: 100%">
                  <el-option
                    v-for="opt in getDictOptions('supply_category')"
                    :key="opt.itemValue"
                    :label="opt.itemLabel"
                    :value="opt.itemValue"
                  />
                </el-select>
              </el-form-item>
              <el-form-item label="地址" prop="address">
                <el-input v-model="formData.address" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入地址" />
              </el-form-item>
              <el-form-item v-if="isEdit" label="评分" prop="score">
                <el-input-number v-model="formData.score" :disabled="!isEdit" :min="0" :max="100" :style="{ width: '100%' }" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>

          <!-- 客户 -->
          <template v-if="activeEntity === 'customer'">
            <div class="form-section">
              <div class="form-section__title">客户信息</div>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="客户编码" prop="code" :rules="[{ required: true, message: '请输入客户编码' }]">
                    <el-input v-model="formData.code" :disabled="!isEdit" placeholder="请输入客户编码" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="客户名称" prop="name" :rules="[{ required: true, message: '请输入客户名称' }]">
                    <el-input v-model="formData.name" :disabled="!isEdit" placeholder="请输入客户名称" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="联系人" prop="contactPerson">
                    <el-input v-model="formData.contactPerson" :disabled="!isEdit" placeholder="请输入联系人" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="联系电话" prop="contactPhone">
                    <el-input v-model="formData.contactPhone" :disabled="!isEdit" placeholder="请输入联系电话" />
                  </el-form-item>
                </el-col>
              </el-row>
              <el-form-item label="邮箱" prop="email">
                <el-input v-model="formData.email" :disabled="!isEdit" placeholder="请输入邮箱" />
              </el-form-item>
              <el-form-item label="地址" prop="address">
                <el-input v-model="formData.address" :disabled="!isEdit" type="textarea" :rows="2" placeholder="请输入地址" />
              </el-form-item>
            </div>
            <el-form-item v-if="isEdit" class="form-actions">
              <el-switch v-model="formData.isActive" :disabled="!isEdit" active-text="启用" inactive-text="停用" />
            </el-form-item>
          </template>
        </el-form>
      </div>

      <template #footer>
        <div class="dialog-footer">
          <el-button v-if="isEdit" @click="dialogVisible = false">取消</el-button>
          <el-button v-if="isEdit" type="primary" @click="handleSave" :loading="saving">
            <el-icon><Check /></el-icon>保存
          </el-button>
        </div>
      </template>
    </el-drawer>
  </div>
</template>

<style scoped>
/* ─── Layout ──────────────────────────────── */
.basic-data {
  display: flex;
  flex-direction: column;
  gap: var(--space-4, 16px);
}

/* ─── Page Header ─────────────────────────── */
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-4, 16px);
}

.page-header__main {
  display: flex;
  align-items: center;
  gap: var(--space-3, 12px);
}

.page-header__icon {
  font-size: 28px;
  color: var(--primary, #1677ff);
  background: var(--primary-bg, #f0f7ff);
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg, 8px);
  display: flex;
  align-items: center;
  justify-content: center;
}

.page-header__title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary, #1a1a1a);
  line-height: 1.3;
}

.page-header__subtitle {
  margin: 2px 0 0;
  font-size: 13px;
  color: var(--text-secondary, #8c8c8c);
}

.header-search {
  width: 260px;
}

.header-search :deep(.el-input__wrapper) {
  border-radius: var(--radius-md, 6px);
  box-shadow: 0 0 0 1px var(--border-color, #dcdfe6) inset;
}

/* ─── Entity Navigation ───────────────────── */
.entity-nav {
  display: flex;
  gap: 4px;
  padding: 4px;
  background: var(--bg-white, #fff);
  border-radius: var(--radius-lg, 8px);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  overflow-x: auto;
  scrollbar-width: none;
}

.entity-nav::-webkit-scrollbar {
  display: none;
}

.entity-nav__item {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: var(--radius-md, 6px);
  cursor: pointer;
  white-space: nowrap;
  font-size: 13px;
  color: var(--text-secondary, #606266);
  transition: all 0.2s ease;
  user-select: none;
  flex-shrink: 0;
}

.entity-nav__item:hover {
  background: var(--primary-bg, #f0f7ff);
  color: var(--primary, #1677ff);
}

.entity-nav__item--active {
  background: var(--primary, #1677ff);
  color: #fff;
  font-weight: 500;
  box-shadow: 0 2px 8px rgba(22, 119, 255, 0.25);
}

.entity-nav__icon {
  font-size: 16px;
  flex-shrink: 0;
}

/* ─── Data Card ───────────────────────────── */
.data-card {
  background: var(--bg-white, #fff);
  border-radius: var(--radius-lg, 8px);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.data-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-4, 16px) var(--space-5, 20px);
  border-bottom: 1px solid var(--border-color, #e4e7ed);
  background: var(--bg-white, #fff);
}

.data-card__title {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary, #1a1a1a);
}

.data-card__title .el-icon {
  color: var(--primary, #1677ff);
}

.data-card__count {
  font-weight: 400;
}

.data-card__toolbar {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
}

/* ─── Table Index Column ──────────────────── */
.data-card__table :deep(.el-table__row > .el-table__cell.index-cell),
.data-card__table :deep(.index-cell) {
  white-space: nowrap !important;
}

.data-card__table :deep(.index-cell .el-table__cell) {
  white-space: nowrap !important;
}

.data-card__table :deep(.index-cell .cell) {
  white-space: nowrap !important;
  display: inline-block;
  min-width: 100%;
}

/* ─── Table ───────────────────────────────── */
.data-card__table {
  border-radius: 0;
}

.data-card__table :deep(.el-table__row) {
  transition: background-color 0.15s ease;
}

.data-card__table :deep(.el-table__row:hover) {
  background-color: var(--primary-light, #e6f4ff);
}

.data-card__table :deep(.el-table__row--striped) {
  --el-table-tr-bg-color: transparent;
}

.data-card__table :deep(.el-table th.el-table__cell) {
  background: var(--bg-gray, #fafafa) !important;
  color: var(--text-primary, #1a1a1a);
  font-weight: 600;
  font-size: 12px;
}

/* ─── Pagination ──────────────────────────── */
.data-card__footer {
  display: flex;
  justify-content: flex-end;
  padding: var(--space-3, 12px) var(--space-5, 20px);
  border-top: 1px solid var(--border-color, #e4e7ed);
  background: var(--bg-white, #fff);
}

.data-card__pagination {
  display: flex;
  align-items: center;
}

/* ─── Dialog ──────────────────────────────── */
.dialog-body-wrap {
  max-height: 60vh;
  overflow-y: auto;
  padding-right: 8px;
}

.dialog-body-wrap::-webkit-scrollbar {
  width: 5px;
}

.dialog-body-wrap::-webkit-scrollbar-thumb {
  background: var(--border-color, #dcdfe6);
  border-radius: 3px;
}

.dialog-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-2, 8px);
}

.dialog-form :deep(.el-form-item) {
  margin-bottom: 0;
}

.dialog-form :deep(.el-form-item__label) {
  font-weight: 500;
  color: var(--text-primary, #303133);
  font-size: 13px;
}

/* Form sections */
.form-section {
  padding: var(--space-4, 16px) 0;
  border-bottom: 1px dashed var(--border-color-light, #ebeef5);
}

.form-section:last-child {
  border-bottom: none;
}

.form-section__title {
  display: flex;
  align-items: center;
  gap: var(--space-2, 8px);
  font-size: 13px;
  font-weight: 600;
  color: var(--primary, #1677ff);
  margin-bottom: var(--space-3, 12px);
  padding-bottom: var(--space-2, 8px);
  border-bottom: 1px solid var(--primary-light-5, #8bc5ff);
}

.form-section__title::before {
  content: '';
  width: 3px;
  height: 14px;
  background: var(--primary, #1677ff);
  border-radius: 2px;
  flex-shrink: 0;
}

.form-actions {
  display: flex;
  align-items: center;
}

/* Drawer styles */
.data-drawer :deep(.el-drawer) {
  border-radius: var(--radius-lg, 8px) 0 0 var(--radius-lg, 8px);
  overflow: hidden;
}

.data-drawer :deep(.el-drawer__header) {
  padding: 18px 24px;
  border-bottom: 1px solid var(--border-color, #e4e7ed);
  margin: 0;
  background: var(--bg-white, #fff);
  margin-bottom: 0;
}

.data-drawer :deep(.el-drawer__close-btn) {
  top: 18px;
  right: 24px;
}

.data-drawer :deep(.el-drawer__title) {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-primary, #1a1a1a);
}

.data-drawer :deep(.el-drawer__body) {
  padding: 0;
  background: var(--bg-white, #fff);
}

/* ─── Dialog Footer Buttons ──────────────── */
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3, 12px);
}

/* ─── Action Buttons ──────────────────────── */
.data-card__table :deep(.el-button--primary.is-link) {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
}

.data-card__table :deep(.el-button--primary.is-link:hover),
.data-card__table :deep(.el-button--primary.is-link:focus) {
  border: none !important;
  box-shadow: none !important;
  outline: none;
}

.data-card__table :deep(.el-button--primary.is-link:focus-visible) {
  outline: none;
  box-shadow: none;
}

/* ─── Status Tags ─────────────────────────── */
.data-card__table :deep(.el-tag) {
  border-radius: var(--radius-sm, 4px);
  font-weight: 500;
}

/* ─── Responsive ──────────────────────────── */
@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .entity-nav {
    flex-wrap: nowrap;
  }

  .data-card__header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--space-3, 12px);
  }
}
</style>

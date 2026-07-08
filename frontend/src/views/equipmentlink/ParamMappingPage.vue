<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { equipmentLinkApi } from '@/api/equipmentLink'
import { equipmentApi } from '@/api/basicData'
import type { EquipmentParamMapping, CreateParamMapping } from '@/types/equipmentLink'
import { DATA_TYPE_OPTIONS } from '@/types/equipmentLink'
import type { Equipment } from '@/types/basicData'

defineOptions({ name: 'ParamMappingPage' })

const loading = ref(false)
const mappings = ref<EquipmentParamMapping[]>([])
const total = ref(0)
const query = reactive({ page: 1, pageSize: 20, equipmentId: '' as string | '', keyword: '' })

const equipments = ref<Equipment[]>([])

const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref<number | null>(null)

const form = reactive<CreateParamMapping>({
  equipmentId: 0,
  mqttTopic: '',
  systemParamCode: '',
  paramGroupId: undefined,
  dataType: 'numeric',
  unit: '',
})

const formRules = {
  equipmentId: [{ required: true, message: '请选择设备', trigger: 'change' }],
  mqttTopic: [{ required: true, message: '请输入MQTT Topic', trigger: 'blur' }],
  systemParamCode: [{ required: true, message: '请输入系统参数代码', trigger: 'blur' }],
  dataType: [{ required: true, message: '请选择数据类型', trigger: 'change' }],
}

const formRef = ref()

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadEquipments() {
  try {
    const res = await equipmentApi.list({ pageSize: 1000 })
    equipments.value = res.items
  } catch (e) {
    console.error('Failed to load equipments', e)
  }
}

async function loadData() {
  loading.value = true
  try {
    const params: any = { page: query.page, pageSize: query.pageSize }
    if (query.equipmentId) params.equipmentId = Number(query.equipmentId)
    if (query.keyword) params.keyword = query.keyword
    const res = await equipmentLinkApi.mappings(params)
    mappings.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load mappings', e)
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
  dialogTitle.value = '新增参数映射'
  isEdit.value = false
  editingId.value = null
  Object.assign(form, {
    equipmentId: 0, mqttTopic: '', systemParamCode: '', paramGroupId: undefined, dataType: 'numeric', unit: ''
  })
  dialogVisible.value = true
}

async function openEdit(id: number) {
  dialogTitle.value = '编辑参数映射'
  isEdit.value = true
  editingId.value = id
  try {
    const detail = await equipmentLinkApi.mappingById(id)
    form.equipmentId = detail.equipmentId
    form.mqttTopic = detail.mqttTopic
    form.systemParamCode = detail.systemParamCode
    form.paramGroupId = detail.paramGroupId
    form.dataType = detail.dataType
    form.unit = detail.unit ?? ''
    dialogVisible.value = true
  } catch {
    ElMessage.error('加载映射详情失败')
  }
}

async function handleSave() {
  try {
    await formRef.value.validate()
    if (isEdit.value && editingId.value) {
      await equipmentLinkApi.createMapping(form)
      ElMessage.success('更新成功')
    } else {
      await equipmentLinkApi.createMapping(form)
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    loadData()
  } catch (err: any) {
    if (err?.response?.data?.message) {
      ElMessage.error(err.response.data.message)
    } else if (err?.message) {
      ElMessage.error(err.message)
    } else {
      ElMessage.error('操作失败')
    }
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确定删除该参数映射？', '确认删除', {
      type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消',
    })
    equipmentLinkApi.removeMapping(id)
    ElMessage.success('删除成功')
    loadData()
  } catch {
    // cancelled
  }
}

function getEquipmentName(id: number): string {
  const eq = equipments.value.find(e => e.id === id)
  return eq ? `${eq.name} (${eq.code})` : `设备${id}`
}

function getDataTypeLabel(val: string) {
  const opt = DATA_TYPE_OPTIONS.find(o => o.value === val)
  return opt?.label ?? val
}

onMounted(() => {
  loadEquipments()
  loadData()
})
</script>
<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-select v-model="query.equipmentId" placeholder="选择设备" clearable style="width: 180px" size="small">
        <el-option v-for="eq in equipments" :key="eq.id" :label="`${eq.name} (${eq.code})`" :value="eq.id" />
      </el-select>
      <el-input v-model="query.keyword" placeholder="搜索设备代码/参数代码" clearable style="width: 220px" size="small" @keyup.enter="handleSearch" />
      <el-button @click="handleSearch" size="small">查询</el-button>
      <el-button @click="loadData" size="small">刷新</el-button>
      <div style="flex:1"></div>
      <el-button type="primary" @click="openCreate" size="small">+ 新增映射</el-button>
    </div>

    <el-table :data="mappings" v-loading="loading" stripe border style="width: 100%" size="small">
      <el-table-column prop="equipmentId" label="设备ID" width="70" />
      <el-table-column label="设备名称" min-width="150">
        <template #default="{ row }">{{ getEquipmentName(row.equipmentId) }}</template>
      </el-table-column>
      <el-table-column prop="mqttTopic" label="MQTT Topic" min-width="180" show-overflow-tooltip />
      <el-table-column prop="systemParamCode" label="系统参数代码" width="140" />
      <el-table-column prop="paramGroupId" label="参数组ID" width="90" />
      <el-table-column label="数据类型" width="90">
        <template #default="{ row }">{{ getDataTypeLabel(row.dataType) }}</template>
      </el-table-column>
      <el-table-column prop="unit" label="单位" width="70" />
      <el-table-column prop="createdAt" label="创建时间" width="160">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="160" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="openEdit(row.id)">编辑</el-button>
          <el-button link size="small" type="danger" @click="handleDelete(row.id)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-row">
      <el-pagination
        v-model:current-page="query.page"
        :page-size="query.pageSize"
        :total="total"
        layout="total, prev, pager, next"
        size="small"
        @current-change="loadData"
      />
    </div>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="560px" :close-on-click-modal="false">
      <el-ref ref="formRef">
        <el-form :model="form" :rules="formRules" label-width="100px" size="small">
          <el-form-item label="设备" prop="equipmentId">
            <el-select v-model="form.equipmentId" placeholder="选择设备" style="width: 100%">
              <el-option v-for="eq in equipments" :key="eq.id" :label="`${eq.name} (${eq.code})`" :value="eq.id" />
            </el-select>
          </el-form-item>
          <el-form-item label="MQTT Topic" prop="mqttTopic">
            <el-input v-model="form.mqttTopic" placeholder="如 machine/001/status" />
          </el-form-item>
          <el-form-item label="系统参数代码" prop="systemParamCode">
            <el-input v-model="form.systemParamCode" placeholder="如 SPEED_SET" />
          </el-form-item>
          <el-form-item label="参数组ID">
            <el-input-number v-model="form.paramGroupId" :min="0" placeholder="可选" style="width: 100%" />
          </el-form-item>
          <el-form-item label="数据类型" prop="dataType">
            <el-select v-model="form.dataType" style="width: 100%">
              <el-option v-for="o in DATA_TYPE_OPTIONS" :key="o.value" :label="o.label" :value="o.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="单位">
            <el-input v-model="form.unit" placeholder="如 mm, RPM" />
          </el-form-item>
        </el-form>
      </el-ref>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>
<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

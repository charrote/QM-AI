<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { defectApi } from '@/api/defect'
import type { Defect, CreateDefect } from '@/types/defect'
import {
  SEVERITY_OPTIONS, SEVERITY_MAP, DEFECT_STATUS_OPTIONS, DEFECT_STATUS_MAP,
  SOURCE_TYPE_OPTIONS, SOURCE_TYPE_MAP,
} from '@/types/defect'

defineOptions({ name: 'DefectsPage' })

const router = useRouter()

const searchKeyword = ref('')
const sourceTypeFilter = ref('')
const severityFilter = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const defects = ref<Defect[]>([])

const dialogVisible = ref(false)
const isEditing = ref(false)
const currentDefectId = ref<number | null>(null)
const defectForm = reactive<CreateDefect>({
  defectCode: '', severity: 'minor', sourceType: 'IQC', sourceId: 0,
  productId: 0, batchId: 0, equipmentId: 0, quantity: 0,
  description: '', discoveredBy: '', discoveredAt: new Date().toISOString().slice(0, 10),
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadDefects() {
  try {
    const res = await defectApi.getAllDefects({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value,
      sourceType: sourceTypeFilter.value || undefined,
      severity: severityFilter.value || undefined,
      status: statusFilter.value || undefined,
    })
    defects.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load defects', e)
  }
}

function openCreate() {
  isEditing.value = false
  currentDefectId.value = null
  defectForm.defectCode = ''
  defectForm.severity = 'minor'
  defectForm.sourceType = 'IQC'
  defectForm.sourceId = 0
  defectForm.productId = 0
  defectForm.batchId = 0
  defectForm.equipmentId = 0
  defectForm.quantity = 0
  defectForm.description = ''
  defectForm.discoveredBy = ''
  defectForm.discoveredAt = new Date().toISOString().slice(0, 10)
  dialogVisible.value = true
}

function openEdit(row: Defect) {
  isEditing.value = true
  currentDefectId.value = row.id
  defectForm.defectCode = row.defectCode
  defectForm.severity = row.severity
  defectForm.sourceType = row.sourceType
  defectForm.sourceId = row.sourceId
  defectForm.productId = row.productId
  defectForm.batchId = row.batchId
  defectForm.equipmentId = row.equipmentId
  defectForm.quantity = row.quantity
  defectForm.description = row.description
  defectForm.discoveredBy = row.discoveredBy
  defectForm.discoveredAt = row.discoveredAt?.slice(0, 10) || ''
  dialogVisible.value = true
}

async function saveDefect() {
  if (!defectForm.defectCode || !defectForm.description) {
    ElMessage.warning('请填写缺陷代码和描述')
    return
  }
  try {
    if (isEditing.value && currentDefectId.value) {
      await defectApi.updateDefect(currentDefectId.value, {
        defectCode: defectForm.defectCode,
        severity: defectForm.severity,
        sourceType: defectForm.sourceType,
        sourceId: defectForm.sourceId,
        productId: defectForm.productId,
        batchId: defectForm.batchId,
        equipmentId: defectForm.equipmentId,
        quantity: defectForm.quantity,
        description: defectForm.description,
        discoveredBy: defectForm.discoveredBy,
        discoveredAt: defectForm.discoveredAt,
      })
      ElMessage.success('缺陷记录已更新')
    } else {
      await defectApi.createDefect(defectForm)
      ElMessage.success('缺陷记录已创建')
    }
    dialogVisible.value = false
    await loadDefects()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteDefect(row: Defect) {
  try {
    await ElMessageBox.confirm(`确定删除缺陷「${row.defectCode}」吗？`, '确认', { type: 'warning' })
    await defectApi.deleteDefect(row.id)
    ElMessage.success('已删除')
    await loadDefects()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

function goToCapa(row: Defect) {
  router.push({ name: 'Capa', query: { defectId: String(row.id) } })
}

function goToScrapRework(row: Defect) {
  router.push({ name: 'ScrapRework', query: { defectId: String(row.id) } })
}

onMounted(loadDefects)
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索缺陷代码/描述..."
        clearable
        style="width: 240px"
        @keyup.enter="loadDefects"
      />
      <el-select v-model="sourceTypeFilter" placeholder="来源类型" clearable style="width: 140px" @change="loadDefects">
        <el-option v-for="opt in SOURCE_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="severityFilter" placeholder="严重程度" clearable style="width: 110px" @change="loadDefects">
        <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 110px" @change="loadDefects">
        <el-option v-for="opt in DEFECT_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
      </el-select>
      <el-button type="primary" @click="openCreate">+ 新建缺陷</el-button>
      <el-button @click="loadDefects">刷新</el-button>
    </div>

    <el-table :data="defects" stripe style="width: 100%" size="small">
      <el-table-column prop="defectCode" label="缺陷代码" width="140" />
      <el-table-column label="严重程度" width="80">
        <template #default="{ row }">
          <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small" effect="plain">
            {{ SEVERITY_MAP[row.severity] || row.severity }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="来源" width="110">
        <template #default="{ row }">
          {{ SOURCE_TYPE_MAP[row.sourceType] || row.sourceType }}
        </template>
      </el-table-column>
      <el-table-column prop="quantity" label="不良数" width="70" align="right" />
      <el-table-column prop="description" label="描述" min-width="180" show-overflow-tooltip />
      <el-table-column prop="discoveredBy" label="发现人" width="80" />
      <el-table-column label="发现时间" width="150">
        <template #default="{ row }">{{ formatDate(row.discoveredAt) }}</template>
      </el-table-column>
      <el-table-column label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="DEFECT_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
            {{ DEFECT_STATUS_MAP[row.status] || row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="240" fixed="right">
        <template #default="{ row }">
          <el-button link size="small" type="primary" @click="openEdit(row)">编辑</el-button>
          <el-button link size="small" type="warning" @click="goToCapa(row)">CAPA</el-button>
          <el-button link size="small" type="success" @click="goToScrapRework(row)">报废/返工</el-button>
          <el-button link size="small" type="danger" @click="deleteDefect(row)">删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <div class="pagination-row">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next"
        size="small"
        @current-change="loadDefects"
      />
    </div>

    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? '编辑缺陷记录' : '新建缺陷记录'"
      width="640px"
      :close-on-click-modal="false"
    >
      <el-form :model="defectForm" label-width="100px" size="small">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="缺陷代码" required>
              <el-input v-model="defectForm.defectCode" placeholder="如: DEF-001" :disabled="isEditing" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="严重程度" required>
              <el-select v-model="defectForm.severity" style="width: 100%">
                <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="来源类型" required>
              <el-select v-model="defectForm.sourceType" style="width: 100%">
                <el-option v-for="opt in SOURCE_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="不良数量" required>
              <el-input-number v-model="defectForm.quantity" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item label="产品ID">
              <el-input-number v-model="defectForm.productId" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="批次ID">
              <el-input-number v-model="defectForm.batchId" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="设备ID">
              <el-input-number v-model="defectForm.equipmentId" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="描述" required>
          <el-input v-model="defectForm.description" type="textarea" :rows="3" placeholder="描述不良现象" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="发现人">
              <el-input v-model="defectForm.discoveredBy" placeholder="发现人姓名" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="发现时间">
              <el-date-picker v-model="defectForm.discoveredAt" type="date" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveDefect">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { anomalyApi } from '@/api/iqc'
import type { IqcAnomaly, CreateIqcAnomaly, ResolveIqcAnomaly } from '@/types/iqc'
import {
  IQC_ANOMALY_TYPE_OPTIONS, IQC_SEVERITY_OPTIONS, IQC_ANOMALY_STATUS_OPTIONS,
} from '@/types/iqc'

defineOptions({ name: 'IqcAnomaliesPage' })

const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const anomalies = ref<IqcAnomaly[]>([])
const anomalyDialogVisible = ref(false)
const anomalyForm = reactive<CreateIqcAnomaly>({
  receiptId: 0, anomalyType: 'quality', severity: 'major', description: ''
})
const resolveAnomalyVisible = ref(false)
const resolveForm = reactive<ResolveIqcAnomaly>({ resolution: '' })
const resolveAnomalyId = ref<number>(0)

function calculateResultColor(result: string) {
  switch (result) {
    case 'pass': return 'success'
    case 'fail': return 'danger'
    case 'qualified': return 'success'
    case 'unqualified': return 'danger'
    case 'anomaly': return 'warning'
    default: return 'info'
  }
}

const statusLabel = (status: string, options: any[]) => {
  const opt = options.find((o: any) => o.value === status)
  return opt?.label || status
}

async function loadAnomalies() {
  try {
    const res = await anomalyApi.list({
      page: page.value, pageSize: pageSize.value, keyword: searchKeyword.value
    })
    anomalies.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load anomalies', e)
  }
}

function openCreateAnomaly(receiptId?: number) {
  anomalyForm.receiptId = receiptId || 0
  anomalyForm.inspectionId = undefined
  anomalyForm.anomalyType = 'quality'
  anomalyForm.severity = 'major'
  anomalyForm.description = ''
  anomalyForm.handler = ''
  anomalyDialogVisible.value = true
}

async function saveAnomaly() {
  if (!anomalyForm.receiptId) {
    ElMessage.warning('请选择来料登记')
    return
  }
  try {
    await anomalyApi.create(anomalyForm)
    ElMessage.success('异常单已创建')
    anomalyDialogVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
}

function openResolveAnomaly(row: IqcAnomaly) {
  resolveAnomalyId.value = row.id
  resolveForm.resolution = ''
  resolveForm.handler = ''
  resolveAnomalyVisible.value = true
}

async function resolveAnomaly() {
  if (!resolveForm.resolution) {
    ElMessage.warning('请填写解决方案')
    return
  }
  try {
    await anomalyApi.resolve(resolveAnomalyId.value, resolveForm)
    ElMessage.success('异常单已解决')
    resolveAnomalyVisible.value = false
    await loadAnomalies()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

onMounted(async () => {
  await loadAnomalies()
})
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-input
        v-model="searchKeyword"
        placeholder="搜索异常单号..."
        clearable
        style="width: 300px"
        @keyup.enter="loadAnomalies"
      />
      <el-button type="primary" @click="openCreateAnomaly()">+ 新建异常单</el-button>
      <el-button @click="loadAnomalies">刷新</el-button>
    </div>

    <el-table :data="anomalies" stripe style="width: 100%" size="small">
      <el-table-column prop="anomalyNo" label="异常单号" width="180" />
      <el-table-column prop="receiptNo" label="来料单号" width="150" />
      <el-table-column label="类型" width="90">
        <template #default="{ row }">{{ statusLabel(row.anomalyType, IQC_ANOMALY_TYPE_OPTIONS) }}</template>
      </el-table-column>
      <el-table-column label="严重程度" width="80">
        <template #default="{ row }">
          <el-tag :type="row.severity === 'critical' ? 'danger' : row.severity === 'major' ? 'warning' : 'info'" size="small">
            {{ statusLabel(row.severity, IQC_SEVERITY_OPTIONS) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
      <el-table-column label="状态" width="90">
        <template #default="{ row }">
          <el-tag :type="calculateResultColor(row.status)" size="small" effect="plain">
            {{ statusLabel(row.status, IQC_ANOMALY_STATUS_OPTIONS) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="handler" label="处理人" width="90" />
      <el-table-column label="操作" width="120" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="row.status === 'open' || row.status === 'processing'"
            link size="small" type="success"
            @click="openResolveAnomaly(row)"
          >解决</el-button>
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
        @current-change="loadAnomalies"
      />
    </div>

    <!-- Dialog: 新建异常单 -->
    <el-dialog
      v-model="anomalyDialogVisible"
      title="新建异常单"
      width="520px"
      :close-on-click-modal="false"
    >
      <el-form :model="anomalyForm" label-width="100px" size="small">
        <el-form-item label="来料登记ID" required>
          <el-input-number v-model="anomalyForm.receiptId" :min="1" style="width: 100%" />
        </el-form-item>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="异常类型">
              <el-select v-model="anomalyForm.anomalyType" style="width: 100%">
                <el-option v-for="opt in IQC_ANOMALY_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="严重程度">
              <el-select v-model="anomalyForm.severity" style="width: 100%">
                <el-option v-for="opt in IQC_SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="描述">
          <el-input v-model="anomalyForm.description" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="处理人">
          <el-input v-model="anomalyForm.handler" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="anomalyDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveAnomaly">创建</el-button>
      </template>
    </el-dialog>

    <!-- Dialog: 解决异常单 -->
    <el-dialog
      v-model="resolveAnomalyVisible"
      title="解决异常单"
      width="480px"
      :close-on-click-modal="false"
    >
      <el-form :model="resolveForm" label-width="100px" size="small">
        <el-form-item label="解决方案" required>
          <el-input v-model="resolveForm.resolution" type="textarea" :rows="4" placeholder="请描述解决方案..." />
        </el-form-item>
        <el-form-item label="处理人">
          <el-input v-model="resolveForm.handler" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="resolveAnomalyVisible = false">取消</el-button>
        <el-button type="primary" @click="resolveAnomaly">确认解决</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
</style>

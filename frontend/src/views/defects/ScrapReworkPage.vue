<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Warning } from '@element-plus/icons-vue'
import { defectApi } from '@/api/defect'
import type { ScrapReworkRecord, CreateScrapRework, UpdateReworkResult } from '@/types/defect'
import {
  REWORK_TYPE_OPTIONS, REWORK_TYPE_MAP, REWORK_INSPECTION_RESULT_OPTIONS,
} from '@/types/defect'

defineOptions({ name: 'ScrapReworkPage' })

const route = useRoute()
const defectIdFilter = ref(route.query.defectId ? Number(route.query.defectId) : '')

const records = ref<ScrapReworkRecord[]>([])
const dialogVisible = ref(false)
const scrapForm = reactive<CreateScrapRework>({
  type: 'scrap', defectId: 0, batchId: 0, quantity: 0,
  reason: '', authorizedBy: '',
})

const reworkResultVisible = ref(false)
const reworkId = ref<number | null>(null)
const reworkResultForm = reactive<UpdateReworkResult>({
  reworkInspectionResult: 'pass', reworkSteps: '',
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadRecords() {
  try {
    records.value = await defectApi.getScrapReworkRecords(
      defectIdFilter.value !== '' ? Number(defectIdFilter.value) : undefined
    )
  } catch (e) {
    console.error('Failed to load scrap/rework records', e)
  }
}

function openCreate() {
  scrapForm.type = 'scrap'
  scrapForm.defectId = defectIdFilter.value !== '' ? Number(defectIdFilter.value) : 0
  scrapForm.batchId = 0
  scrapForm.quantity = 0
  scrapForm.reason = ''
  scrapForm.authorizedBy = ''
  dialogVisible.value = true
}

async function saveRecord() {
  if (!scrapForm.defectId || !scrapForm.reason || !scrapForm.authorizedBy) {
    ElMessage.warning('请填写必填信息')
    return
  }
  try {
    await defectApi.createScrapRework(scrapForm)
    ElMessage.success(scrapForm.type === 'scrap' ? '报废记录已创建' : '返工记录已创建')
    dialogVisible.value = false
    await loadRecords()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

function openReworkResult(row: ScrapReworkRecord) {
  if (row.type !== 'rework') {
    ElMessage.warning('仅返工记录需要检验结果')
    return
  }
  if (row.reworkInspectionResult) {
    ElMessage.warning('该记录已有检验结果')
    return
  }
  reworkId.value = row.id
  reworkResultForm.reworkInspectionResult = 'pass'
  reworkResultForm.reworkSteps = row.reworkSteps || ''
  reworkResultVisible.value = true
}

async function saveReworkResult() {
  if (reworkId.value === null) return
  try {
    await defectApi.updateReworkResult(reworkId.value, { ...reworkResultForm })
    ElMessage.success('返工检验结果已更新')
    reworkResultVisible.value = false
    await loadRecords()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

const scrapFormRef = ref()

const formRules = {
  defectId: [{ required: true, message: '请输入缺陷ID', trigger: 'blur' }],
  reason: [{ required: true, message: '请输入原因', trigger: 'blur' }],
  authorizedBy: [{ required: true, message: '请输入审批人', trigger: 'blur' }],
}

const scrapCount = computed(() => records.value.filter(r => r.type === 'scrap').length)
const reworkCount = computed(() => records.value.filter(r => r.type === 'rework').length)
const pendingInspection = computed(() => records.value.filter(r => r.type === 'rework' && r.reworkInspectionRequired && !r.reworkInspectionResult).length)
const totalCount = computed(() => records.value.length)

function rowClassName({ row }: { row: ScrapReworkRecord }) {
  return row.type === 'rework' ? 'rework-row' : ''
}

onMounted(loadRecords)
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Warning /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>报废/返工管理</h2>
          <p>不良品报废与返工处置的记录与跟踪</p>
        </div>
      </div>
    </div>

    <!-- Stat Summary Cards -->
    <el-row :gutter="12" class="stat-row">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-danger">{{ scrapCount }}</div>
          <div class="stat-label">报废总数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-warning">{{ reworkCount }}</div>
          <div class="stat-label">返工总数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-info">{{ pendingInspection }}</div>
          <div class="stat-label">待检验返工</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value text-primary">{{ totalCount }}</div>
          <div class="stat-label">总记录数</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Toolbar -->
    <el-card shadow="never" class="search-card">
      <div class="toolbar-row">
        <el-input-number
          v-model="defectIdFilter"
          placeholder="缺陷ID筛选"
          :min="0"
          size="small"
          style="width: 160px"
          @change="loadRecords"
        />
        <el-button @click="loadRecords">刷新</el-button>
        <div class="search-spacer" />
        <el-button type="primary" @click="openCreate">+ 新建记录</el-button>
      </div>
    </el-card>

    <!-- Table -->
    <el-card shadow="never" class="table-card">
      <el-table :data="records" stripe style="width: 100%" :row-class-name="rowClassName">
        <el-table-column label="类型" width="80">
          <template #default="{ row }">
            <el-tag :type="REWORK_TYPE_OPTIONS.find(o => o.value === row.type)?.type || 'info'" size="small" effect="plain">
              {{ REWORK_TYPE_MAP[row.type] || row.type }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="defectId" label="缺陷ID" width="85" />
        <el-table-column prop="batchId" label="批次ID" width="85" />
        <el-table-column prop="quantity" label="数量" width="80" align="right" />
        <el-table-column prop="reason" label="原因" min-width="180" show-overflow-tooltip />
        <el-table-column prop="reworkSteps" label="返工步骤" min-width="150" show-overflow-tooltip />
        <el-table-column label="返工检验" width="90">
          <template #default="{ row }">
            <template v-if="row.reworkInspectionRequired">
              <template v-if="row.reworkInspectionResult">
                <el-tag :type="REWORK_INSPECTION_RESULT_OPTIONS.find(o => o.value === row.reworkInspectionResult)?.type || 'info'" size="small">
                  {{ REWORK_INSPECTION_RESULT_OPTIONS.find(o => o.value === row.reworkInspectionResult)?.label || row.reworkInspectionResult }}
                </el-tag>
              </template>
              <template v-else>
                <el-tag type="warning" size="small" effect="dark">待检验</el-tag>
              </template>
            </template>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column prop="authorizedBy" label="审批人" width="85" />
        <el-table-column label="审批时间" width="165">
          <template #default="{ row }">{{ formatDate(row.authorizedAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="130" fixed="right">
          <template #default="{ row }">
            <el-button
              v-if="row.type === 'rework' && !row.reworkInspectionResult"
              link size="small" type="success"
              @click="openReworkResult(row)"
            >填写检验结果</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="scrapForm.type === 'scrap' ? '新建报废记录' : '新建返工记录'"
      width="580px"
      :close-on-click-modal="false"
    >
      <el-form :model="scrapForm" label-width="100px" :rules="formRules" ref="scrapFormRef">
        <el-divider content-position="left">基本信息</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="类型" prop="type" required>
              <el-select v-model="scrapForm.type" style="width: 100%">
                <el-option v-for="opt in REWORK_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="缺陷ID" prop="defectId" required>
              <el-input-number v-model="scrapForm.defectId" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="批次ID" required>
              <el-input-number v-model="scrapForm.batchId" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="数量" prop="quantity" required>
              <el-input-number v-model="scrapForm.quantity" :min="0" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-divider content-position="left">处置详情</el-divider>
        <el-form-item label="原因" prop="reason" required>
          <el-input v-model="scrapForm.reason" type="textarea" :rows="2" placeholder="报废/返工原因" />
        </el-form-item>
        <el-form-item label="返工步骤" v-if="scrapForm.type === 'rework'">
          <el-input v-model="scrapForm.reworkSteps" type="textarea" :rows="2" placeholder="返工步骤描述" />
        </el-form-item>
        <el-divider content-position="left">审批信息</el-divider>
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="审批人" prop="authorizedBy" required>
              <el-input v-model="scrapForm.authorizedBy" placeholder="审批人姓名" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="需要检验" v-if="scrapForm.type === 'rework'">
              <el-switch v-model="scrapForm.reworkInspectionRequired" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveRecord">保存</el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="reworkResultVisible"
      title="填写返工检验结果"
      width="480px"
      :close-on-click-modal="false"
    >
      <el-form :model="reworkResultForm" label-width="100px">
        <el-form-item label="返工步骤">
          <el-input v-model="reworkResultForm.reworkSteps" type="textarea" :rows="2" placeholder="返工步骤" />
        </el-form-item>
        <el-form-item label="检验结果" required>
          <el-select v-model="reworkResultForm.reworkInspectionResult" style="width: 100%">
            <el-option v-for="opt in REWORK_INSPECTION_RESULT_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="reworkResultVisible = false">取消</el-button>
        <el-button type="primary" @click="saveReworkResult">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.page-header { margin-bottom: 16px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon {
  width: 44px; height: 44px;
  display: flex; align-items: center; justify-content: center;
  background: #fef0f0; border-radius: 10px;
}
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.stat-row { margin-bottom: 12px; }
.stat-card { text-align: center; border-radius: 8px; }
.stat-card:hover { transform: translateY(-2px); transition: all 0.2s; }
.stat-value { font-size: 28px; font-weight: 700; }
.stat-label { font-size: 13px; color: #909399; margin-top: 4px; }
.text-danger { color: #f56c6c; }
.text-warning { color: #e6a23c; }
.text-info { color: #409eff; }
.text-primary { color: #409eff; }
.search-card { margin-bottom: 12px; }
.toolbar-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.search-spacer { flex: 1; }
.table-card { flex: 1; }
.rework-row { background-color: #fdf6ec; }
</style>

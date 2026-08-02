<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Warning, Refresh, Plus } from '@element-plus/icons-vue'
import { defectApi } from '@/api/defect'
import RightPanel from '@/components/layout/RightPanel.vue'
import type { ScrapReworkRecord, CreateScrapRework, UpdateReworkResult } from '@/types/defect'
import {
  REWORK_TYPE_OPTIONS, REWORK_TYPE_MAP, REWORK_INSPECTION_RESULT_OPTIONS,
} from '@/types/defect'

defineOptions({ name: 'ScrapReworkPage' })

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
    records.value = await defectApi.getScrapReworkRecords()
  } catch (e) {
    console.error('Failed to load scrap/rework records', e)
  }
}

function openCreate() {
  scrapForm.type = 'scrap'
  scrapForm.defectId = 0
  scrapForm.batchId = 0
  scrapForm.quantity = 0
  scrapForm.reason = ''
  scrapForm.authorizedBy = ''
  dialogVisible.value = true
}

async function saveRecord() {
  if (!scrapForm.defectId || scrapForm.defectId === 0 || !scrapForm.reason || !scrapForm.authorizedBy) {
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
    <!-- Page Header Banner -->
    <div class="m07-header-banner m07-header-banner--warning">
      <div class="m07-header-banner-main">
        <div class="m07-header-banner-left">
          <div class="m07-header-banner-icon">
            <el-icon :size="24"><Warning /></el-icon>
          </div>
          <div class="m07-header-banner-text">
            <div class="m07-header-banner-title">报废/返工管理</div>
            <div class="m07-header-banner-subtitle">不良品报废与返工处置的记录与跟踪</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stats Bar -->
    <div v-if="totalCount > 0" class="m07-stat-grid">
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--primary">
          <el-icon :size="22"><Warning /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">记录总数</div>
          <div class="m07-stat-value m07-stat-value--primary">{{ totalCount }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--danger">
          <el-icon :size="22"><Warning /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">报废</div>
          <div class="m07-stat-value m07-stat-value--danger">{{ scrapCount }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--warning">
          <el-icon :size="22"><Warning /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">返工</div>
          <div class="m07-stat-value" style="color: #ca8a04">{{ reworkCount }}</div>
        </div>
      </div>
      <div class="m07-stat-card">
        <div class="m07-stat-icon m07-stat-icon--info">
          <el-icon :size="22"><Warning /></el-icon>
        </div>
        <div class="m07-stat-content">
          <div class="m07-stat-label">待检验</div>
          <div class="m07-stat-value" style="color: #6b7280">{{ pendingInspection }}</div>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="m07-table-card">
      <div class="m07-table-card__header">
        <div class="m07-table-card__title">
          <el-icon><Warning /></el-icon>
          <span>报废/返工记录列表</span>
          <el-tag v-if="totalCount" type="info" size="small" class="m07-table-card__count">
            共 {{ totalCount }} 条
          </el-tag>
        </div>
        <div class="m07-table-card__toolbar">
          <div class="m07-filter-bar">
            <el-button @click="loadRecords" text>
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
          </div>
          <el-button type="primary" @click="openCreate">
            <el-icon><Plus /></el-icon>新建记录
          </el-button>
        </div>
      </div>

      <div class="m07-table-card__body">
        <el-table
          :data="records"
          border
          stripe
          style="width: 100%"
          size="small"
          empty-text="暂无数据"
          class="m07-table"
          :row-class-name="rowClassName"
        >
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
              <template v-if="row.type === 'rework' && !row.reworkInspectionResult">
                <el-button size="small" type="success" link @click.stop="openReworkResult(row)">填写检验结果</el-button>
              </template>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <!-- Create/Edit RightPanel -->
    <RightPanel
      v-model:visible="dialogVisible"
      :title="scrapForm.type === 'scrap' ? '新建报废记录' : '新建返工记录'"
      :width="580"
    >
      <template #body>
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
      </template>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveRecord">保存</el-button>
      </template>
    </RightPanel>

    <RightPanel
      v-model:visible="reworkResultVisible"
      title="填写返工检验结果"
      :width="480"
    >
      <template #body>
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
      </template>
      <template #footer>
        <el-button @click="reworkResultVisible = false">取消</el-button>
        <el-button type="primary" @click="saveReworkResult">保存</el-button>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
/* ─── Table Link Buttons ──────────────────── */
.m07-table .el-button.is-link {
  padding: 0 4px;
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
}

.m07-table .el-button.is-link:hover,
.m07-table .el-button.is-link:focus,
.m07-table .el-button.is-link:focus-visible,
.m07-table .el-button.is-link:active {
  border: none !important;
  box-shadow: none !important;
  background: transparent !important;
  outline: none;
}
</style>

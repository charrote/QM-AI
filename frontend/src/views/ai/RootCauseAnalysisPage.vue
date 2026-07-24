<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { aiApi } from '@/api/ai'

defineOptions({ name: 'RootCauseAnalysisPage' })

const form = ref({
  productId: '' as string | number,
  defectCode: '',
  startDate: '',
  endDate: '',
  processId: '' as string | number,
})

const loading = ref(false)
const results = ref<Array<{
  cause: string
  confidence: number
  evidence: string
  recommendation: string
}>>([])
const summary = ref('')

async function runAnalysis() {
  const params: Record<string, any> = {}
  if (form.value.productId) params.productId = Number(form.value.productId)
  if (form.value.defectCode) params.defectCode = form.value.defectCode
  if (form.value.startDate) params.startDate = form.value.startDate
  if (form.value.endDate) params.endDate = form.value.endDate
  if (form.value.processId) params.processId = Number(form.value.processId)

  if (Object.keys(params).length === 0) {
    ElMessage.warning('请至少填写一个查询条件')
    return
  }

  loading.value = true
  results.value = []
  summary.value = ''
  try {
    const res = await aiApi.runRootCauseAnalysis(params)
    results.value = res.findings
    summary.value = res.summary
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '分析失败')
  } finally {
    loading.value = false
  }
}

function formatConfidence(c: number) {
  return (c * 100).toFixed(1) + '%'
}

function resetForm() {
  form.value.productId = ''
  form.value.defectCode = ''
  form.value.startDate = ''
  form.value.endDate = ''
  form.value.processId = ''
  results.value = []
  summary.value = ''
}

function onDateRangeChange(val: string[] | null) {
  if (val) {
    form.value.startDate = val[0] || ''
    form.value.endDate = val[1] || ''
  } else {
    form.value.startDate = ''
    form.value.endDate = ''
  }
}
</script>

<template>
  <div class="page-container">
    <!-- Analysis Form -->
    <el-card shadow="never" class="analysis-form-card">
      <template #header>
        <span style="font-weight: 600;">分析条件</span>
      </template>
      <el-row :gutter="16">
        <el-col :span="6">
          <el-form-item label="产品ID" style="margin-bottom: 0;">
            <el-input v-model.number="form.productId" placeholder="如: 1" type="number" />
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="缺陷代码" style="margin-bottom: 0;">
            <el-input v-model="form.defectCode" placeholder="如: DEF-001" />
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="日期范围" style="margin-bottom: 0;">
            <el-date-picker
              :model-value="[form.startDate, form.endDate]"
              type="daterange"
              range-separator="至"
              start-placeholder="开始日期"
              end-placeholder="结束日期"
              value-format="YYYY-MM-DD"
              style="width: 100%"
              @update:model-value="onDateRangeChange"
            />
          </el-form-item>
        </el-col>
        <el-col :span="6">
          <el-form-item label="工序ID" style="margin-bottom: 0;">
            <el-input v-model.number="form.processId" placeholder="如: 5" type="number" />
          </el-form-item>
        </el-col>
      </el-row>
      <div style="display: flex; gap: 8px;">
        <el-button type="primary" :loading="loading" @click="runAnalysis">
          <el-icon><Search /></el-icon>
          运行分析
        </el-button>
        <el-button @click="resetForm">
          重置
        </el-button>
      </div>
    </el-card>

    <!-- Summary -->
    <el-card shadow="never" v-if="summary" class="summary-card">
      <template #header>
        <span style="font-weight: 600;">AI 分析摘要</span>
      </template>
      <div class="summary-content">{{ summary }}</div>
    </el-card>

    <!-- Findings -->
    <el-card shadow="never" v-if="results.length > 0" class="findings-card">
      <template #header>
        <span style="font-weight: 600;">分析结果（{{ results.length }} 条发现）</span>
      </template>
      <el-table :data="results" stripe style="width: 100%" >
        <el-table-column label="序号" width="60" align="center">
          <template #default="{ $index }">{{ $index + 1 }}</template>
        </el-table-column>
        <el-table-column prop="cause" label="原因" min-width="200" show-overflow-tooltip />
        <el-table-column label="置信度" width="120" align="center">
          <template #default="{ row }">
            <el-progress
              :percentage="(row.confidence * 100).toFixed(0)"
              :stroke-width="12"
              :color="row.confidence >= 0.7 ? '#67C23A' : row.confidence >= 0.4 ? '#E6A23C' : '#F56C6C'"
            />
            <span style="font-size: 12px; margin-top: 2px; display: block;">{{ formatConfidence(row.confidence) }}</span>
          </template>
        </el-table-column>
        <el-table-column prop="evidence" label="证据" min-width="200" show-overflow-tooltip />
        <el-table-column prop="recommendation" label="建议" min-width="200" show-overflow-tooltip />
      </el-table>
    </el-card>

    <!-- Empty state -->
    <el-empty v-if="!loading && results.length === 0 && !summary" description="请设置分析条件后点击「运行分析」" />
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.analysis-form-card { margin-bottom: 16px; }
.summary-card { margin-bottom: 16px; }
.findings-card { flex: 1; }
.summary-content { line-height: 1.8; white-space: pre-wrap; }
:deep(.el-form-item) { margin-bottom: 0; }
</style>

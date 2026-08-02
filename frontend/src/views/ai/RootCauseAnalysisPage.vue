<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { Search, Cpu, List } from '@element-plus/icons-vue'
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

function getConfidenceClass(c: number) {
  if (c >= 0.7) return 'confidence-bar__fill--high'
  if (c >= 0.4) return 'confidence-bar__fill--med'
  return 'confidence-bar__fill--low'
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
    <!-- Page Header Banner -->
    <div class="ai-header-banner">
      <div class="ai-header-banner-main">
        <div class="ai-header-banner-icon">
          <el-icon :size="24"><Cpu /></el-icon>
        </div>
        <div class="ai-header-banner-text">
          <div class="ai-header-banner-title">AI 根因分析</div>
          <div class="ai-header-banner-subtitle">基于历史数据的智能根因分析与诊断建议</div>
        </div>
      </div>
    </div>

    <!-- Search Form -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
        <el-form :model="form" label-width="70px" style="width: 100%;">
          <el-row :gutter="16" align="middle">
            <el-col :span="4">
              <el-form-item label="产品ID" style="margin-bottom: 0;">
                <el-input v-model.number="form.productId" placeholder="如: 1" type="number" />
              </el-form-item>
            </el-col>
            <el-col :span="4">
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
            <el-col :span="4">
              <el-form-item label="工序ID" style="margin-bottom: 0;">
                <el-input v-model.number="form.processId" placeholder="如: 5" type="number" />
              </el-form-item>
            </el-col>
            <el-col :span="6" class="ai-filter-actions">
              <el-button type="primary" :loading="loading" @click="runAnalysis" size="default">
                <el-icon><Search /></el-icon>
                运行分析
              </el-button>
              <el-button @click="resetForm">重置</el-button>
            </el-col>
          </el-row>
        </el-form>
      </div>
    </div>

    <!-- Summary -->
    <div v-if="summary" class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><Cpu /></el-icon>
          AI 分析摘要
        </div>
        <el-tag size="small" type="primary">AI 智能诊断</el-tag>
      </div>
      <div class="data-card">
        <div class="data-card__body">
          <div class="summary-content">{{ summary }}</div>
        </div>
      </div>
    </div>

    <!-- Findings -->
    <div v-if="results.length > 0" class="section">
      <div class="section__header">
        <div class="section__title">
          <el-icon><List /></el-icon>
          分析结果
        </div>
        <el-tag size="small" type="info">{{ results.length }} 条发现</el-tag>
      </div>
      <div class="table-card">
        <div class="table-card__body">
          <el-table :data="results" stripe style="width: 100%">
            <el-table-column label="序号" width="60" align="center">
              <template #default="{ $index }">{{ $index + 1 }}</template>
            </el-table-column>
            <el-table-column prop="cause" label="根因" min-width="200" show-overflow-tooltip />
            <el-table-column label="置信度" width="200" align="center">
              <template #default="{ row }">
                <div class="confidence-bar">
                  <div class="confidence-bar__track">
                    <div :class="['confidence-bar__fill', getConfidenceClass(row.confidence)]"
                         :style="{ width: (row.confidence * 100) + '%' }"></div>
                  </div>
                  <span class="confidence-bar__value">{{ formatConfidence(row.confidence) }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="evidence" label="证据" min-width="200" show-overflow-tooltip />
            <el-table-column prop="recommendation" label="建议" min-width="200" show-overflow-tooltip />
          </el-table>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="!loading && results.length === 0 && !summary" class="empty-state">
      <div class="empty-state__icon"><el-icon :size="48"><Cpu /></el-icon></div>
      <div class="empty-state__title">尚未进行分析</div>
      <div class="empty-state__desc">请设置分析条件后点击「运行分析」开始智能诊断</div>
    </div>
  </div>
</template>

<style scoped>
.summary-content {
  line-height: var(--leading-relaxed);
  white-space: pre-wrap;
  color: var(--el-text-color-regular);
}
</style>

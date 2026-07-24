<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { reportsApi } from '@/api/reports'
import { REPORT_TYPE_OPTIONS, REPORT_MODULE_OPTIONS, REPORT_FORMAT_OPTIONS, REPORT_TYPE_MAP } from '@/types/reports'
import { EditPen, Document, Download } from '@element-plus/icons-vue'

defineOptions({ name: 'ReportBuilderPage' })

// ─── Form ────────────────────────────────────────────
const generating = ref(false)
const generateResult = ref<{ jobId: string } | null>(null)

const form = reactive({
  reportType: 'quality_overview',
  startDate: '',
  endDate: '',
  module: 'all',
  format: 'xlsx' as 'csv' | 'xlsx' | 'pdf',
})

function getDefaultDates() {
  const now = new Date()
  const monthAgo = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
  form.startDate = monthAgo.toISOString().slice(0, 10)
  form.endDate = now.toISOString().slice(0, 10)
}

async function handleGenerate() {
  if (!form.startDate || !form.endDate) {
    ElMessage.warning('请选择日期范围')
    return
  }
  if (form.startDate > form.endDate) {
    ElMessage.warning('开始日期不能晚于结束日期')
    return
  }
  generating.value = true
  generateResult.value = null
  try {
    generateResult.value = await reportsApi.generateReport({
      reportType: form.reportType,
      startDate: form.startDate,
      endDate: form.endDate,
      module: form.module,
      format: form.format,
    })
    ElMessage.success('报表生成任务已提交')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '生成失败')
  } finally {
    generating.value = false
  }
}

function resetForm() {
  getDefaultDates()
  form.reportType = 'quality_overview'
  form.module = 'all'
  form.format = 'xlsx'
  generateResult.value = null
}

onMounted(() => {
  getDefaultDates()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Document /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>报表定制</h2>
          <p>按需定制质量报表，支持多种格式导出</p>
        </div>
      </div>
    </div>

    <el-row :gutter="24">
      <!-- Form Card -->
      <el-col :span="14">
        <el-card shadow="hover" class="form-card">
          <template #header>
            <div class="card-header">
              <span>报表配置</span>
              <el-button size="small" @click="resetForm">重置</el-button>
            </div>
          </template>
          <el-form :model="form" label-width="100px" label-position="left">
            <el-divider content-position="left">基本设置</el-divider>
            <el-row :gutter="16">
              <el-col :span="16">
                <el-form-item label="报表类型" required>
                  <el-select v-model="form.reportType" placeholder="选择报表类型" style="width: 100%">
                    <el-option v-for="opt in REPORT_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="8">
                <el-form-item label="导出格式" required>
                  <el-select v-model="form.format" placeholder="选择格式" style="width: 100%">
                    <el-option v-for="opt in REPORT_FORMAT_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>

            <el-divider content-position="left">时间范围</el-divider>
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="开始日期" required>
                  <el-date-picker v-model="form.startDate" type="date" placeholder="开始日期" style="width: 100%" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="结束日期" required>
                  <el-date-picker v-model="form.endDate" type="date" placeholder="结束日期" style="width: 100%" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-divider content-position="left">高级设置</el-divider>
            <el-form-item label="模块">
              <el-select v-model="form.module" placeholder="选择模块" style="width: 100%">
                <el-option v-for="opt in REPORT_MODULE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
              </el-select>
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="handleGenerate" :loading="generating" :icon="EditPen">
                生成报表
              </el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <!-- Result Card -->
      <el-col :span="10">
        <el-card shadow="hover" class="result-card">
          <template #header>
            <div class="card-header">
              <span><el-icon><Download /></el-icon> 生成结果</span>
            </div>
          </template>
          <div v-if="generating" class="result-loading">
            <el-icon class="is-loading" :size="32" color="#409eff"><Loading /></el-icon>
            <p>正在生成报表，请稍候...</p>
          </div>
          <div v-else-if="generateResult" class="result-success">
            <div class="result-icon-wrap">
              <el-icon :size="48" color="#67c23a"><SuccessFilled /></el-icon>
            </div>
            <p class="result-title">报表已提交</p>
            <div class="result-detail-list">
              <div class="result-detail-item">
                <span class="detail-label">报表类型</span>
                <span class="detail-value">{{ REPORT_TYPE_MAP[form.reportType] || form.reportType }}</span>
              </div>
              <div class="result-detail-item">
                <span class="detail-label">格式</span>
                <span class="detail-value">{{ REPORT_FORMAT_OPTIONS.find(o => o.value === form.format)?.label }}</span>
              </div>
              <div class="result-detail-item">
                <span class="detail-label">时间范围</span>
                <span class="detail-value">{{ form.startDate }} ~ {{ form.endDate }}</span>
              </div>
              <div class="result-detail-item">
                <span class="detail-label">任务 ID</span>
                <span class="detail-value"><code>{{ generateResult.jobId }}</code></span>
              </div>
            </div>
          </div>
          <div v-else class="result-empty">
            <el-icon :size="48" color="#c0c4cc"><Document /></el-icon>
            <p>填写表单后点击"生成报表"</p>
          </div>
        </el-card>

        <!-- Tips Card -->
        <el-card shadow="hover" class="tips-card">
          <template #header>
            <span><el-icon><InfoFilled /></el-icon> 生成说明</span>
          </template>
          <ul class="tips-list">
            <li>报表生成可能需要数分钟，请前往导出中心查看进度</li>
            <li>大数据量报表（如全量数据）可能耗时较长</li>
            <li>导出格式支持 CSV、Excel 和 PDF</li>
            <li>历史报表可在导出中心查看和下载</li>
          </ul>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; overflow-y: auto; padding: 4px 0; }
.page-header { margin-bottom: 16px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; background: #ecf5ff; border-radius: 10px; }
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.form-card { margin-bottom: 16px; }
.card-header { display: flex; align-items: center; justify-content: space-between; }
.result-card, .tips-card { margin-bottom: 16px; }
.result-loading, .result-empty { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 40px 0; color: var(--el-text-color-secondary); gap: 12px; }
.result-success { display: flex; flex-direction: column; align-items: center; padding: 16px 0; gap: 8px; }
.result-icon-wrap { margin-bottom: 4px; }
.result-title { font-size: 18px; font-weight: 600; color: var(--el-text-color-primary); margin: 0; }
.result-detail-list { width: 100%; padding: 0 8px; }
.result-detail-item { display: flex; justify-content: space-between; padding: 6px 0; border-bottom: 1px solid #f0f0f0; font-size: 13px; }
.result-detail-item:last-child { border-bottom: none; }
.detail-label { color: var(--el-text-color-secondary); }
.detail-value { color: var(--el-text-color-primary); font-weight: 500; }
.tips-list { margin: 0; padding-left: 20px; font-size: 13px; line-height: 2; color: var(--el-text-color-secondary); }
</style>

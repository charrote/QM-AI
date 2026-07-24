<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Connection, Box, DataAnalysis } from '@element-plus/icons-vue'
import { traceApi } from '@/api/trace'
import type { NgDiffusionResult } from '@/types/trace'
import { RISK_LEVEL_CONFIG } from '@/types/trace'

defineOptions({ name: 'NgDiffusionPage' })

const route = useRoute()
const batchCode = ref(route.query.batchCode as string || '')
const loading = ref(false)
const result = ref<NgDiffusionResult | null>(null)
const error = ref('')

async function analyze() {
  loading.value = true
  result.value = null
  error.value = ''
  try {
    if (!batchCode.value.trim()) { ElMessage.warning('请输入批次号'); return }
    result.value = await traceApi.ngDiffusion(batchCode.value.trim())
  } catch (e: any) {
    error.value = e?.response?.data?.message || 'NG 扩散分析失败'
    ElMessage.error(error.value)
  } finally {
    loading.value = false
  }
}

function riskLabel(code: string) {
  return RISK_LEVEL_CONFIG[code]?.label || code
}

function riskType(code: string) {
  return RISK_LEVEL_CONFIG[code]?.type || 'info'
}

onMounted(() => {})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Connection /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>NG 扩散分析</h2>
          <p>4 维度扩散分析：同设备 / 同刀具 / 同供应商 / 同工艺参数</p>
        </div>
      </div>
    </div>

    <!-- Search -->
    <el-card shadow="never" class="search-card">
      <div class="search-card-header">
        <span class="search-card-title">批次号查询</span>
        <el-tag size="small" type="warning">4 维度分析</el-tag>
      </div>
      <div class="search-input-row">
        <el-input v-model="batchCode" placeholder="请输入 NG 批次号" size="large" clearable @keyup.enter="analyze" style="max-width: 420px">
          <template #prefix><el-icon><Box /></el-icon></template>
        </el-input>
        <el-button type="primary" size="large" @click="analyze" :loading="loading">
          <el-icon><DataAnalysis /></el-icon> 分析扩散
        </el-button>
      </div>
      <div class="search-hints">
        <el-text type="info">分析该 NG 批次通过同设备、同刀具、同供应商、同工艺参数影响的其它批次</el-text>
      </div>
      <div v-if="error" class="error-msg">
        <el-alert :title="error" type="error" :closable="false" show-icon />
      </div>
    </el-card>

    <div v-if="result && !loading" class="result-container">
      <!-- Risk summary card -->
      <el-card shadow="never" class="info-card">
        <div class="risk-summary">
          <div class="risk-item">
            <span class="risk-label">NG 批次</span>
            <span class="risk-value">{{ result.causeBatch }}</span>
          </div>
          <div class="risk-item">
            <span class="risk-label">风险等级</span>
            <el-tag :type="riskType(result.riskLevel)" size="large" effect="dark">{{ riskLabel(result.riskLevel) }}</el-tag>
          </div>
          <div class="risk-item">
            <span class="risk-label">受影响批次总数</span>
            <span class="risk-value highlight">{{ result.totalAffectedCount }}</span>
          </div>
        </div>
      </el-card>

      <el-card v-if="result.affectedBatches?.length" shadow="never">
        <template #header>
          <span>受影响批次列表</span>
          <el-tag size="small" type="info">{{ result.affectedBatches.length }} 个批次</el-tag>
        </template>
        <el-table :data="result.affectedBatches" stripe>
          <el-table-column prop="batchCode" label="批次号" width="140" />
          <el-table-column prop="productName" label="产品名称" min-width="150" />
          <el-table-column label="数量" width="100" align="right">
            <template #default="{ row }">{{ row.quantity }} pcs</template>
          </el-table-column>
          <el-table-column label="不良率" width="100" align="right">
            <template #default="{ row }">{{ row.defectRate != null ? (row.defectRate + '%') : '-' }}</template>
          </el-table-column>
          <el-table-column prop="affectedStage" label="影响阶段" width="130">
            <template #default="{ row }">
              <el-tag size="small" effect="plain">{{ row.affectedStage }}</el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>

      <el-empty v-else description="暂无受影响批次" :image-size="100" />
    </div>

    <el-card v-else class="empty-card" shadow="never">
      <el-empty description="请输入 NG 批次号进行扩散分析">
        <el-text type="info">4 维度扩散分析：同设备 / 同刀具 / 同供应商 / 同工艺参数</el-text>
      </el-empty>
    </el-card>
  </div>
</template>

<style scoped>
.page-container {
  display: flex; flex-direction: column; height: 100%; overflow-y: auto;
  gap: 12px; padding: 8px 16px;
}
.page-header { margin-bottom: 4px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon {
  width: 44px; height: 44px; display: flex; align-items: center; justify-content: center;
  background: #fdf6ec; border-radius: 10px;
}
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.search-card { flex-shrink: 0; }
.search-card-header { display: flex; align-items: center; justify-content: space-between; }
.search-card-title { font-weight: 600; font-size: 15px; }
.search-input-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.search-hints { margin-top: 12px; }
.error-msg { margin-top: 12px; }
.result-container { display: flex; flex-direction: column; gap: 12px; }
.info-card { flex-shrink: 0; }
.risk-summary { display: flex; align-items: center; gap: 32px; flex-wrap: wrap; }
.risk-item { display: flex; flex-direction: column; gap: 6px; }
.risk-label { font-size: 13px; color: #909399; }
.risk-value { font-size: 16px; font-weight: 600; color: #303133; }
.risk-value.highlight { color: #f56c6c; font-size: 20px; }
.empty-card { flex: 1; display: flex; align-items: center; justify-content: center; }
</style>

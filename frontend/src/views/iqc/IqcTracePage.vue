<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { traceApi } from '@/api/iqc'
import { Box, Document, WarningFilled, DataAnalysis } from '@element-plus/icons-vue'
import type { BatchTrace } from '@/types/iqc'
import { IQC_RECEIPT_STATUS_OPTIONS, IQC_ANOMALY_TYPE_OPTIONS, IQC_SEVERITY_OPTIONS, IQC_ANOMALY_STATUS_OPTIONS } from '@/types/iqc'

defineOptions({ name: 'IqcTracePage' })

const traceBatchNo = ref('')
const traceResult = ref<BatchTrace | null>(null)
const traceLoading = ref(false)

const statusLabel = (status: string, options: any[]) => {
  const opt = options.find((o: any) => o.value === status)
  return opt?.label || status
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

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

async function searchTrace() {
  if (!traceBatchNo.value.trim()) {
    ElMessage.warning('请输入批次号')
    return
  }
  traceLoading.value = true
  traceResult.value = null
  try {
    traceResult.value = await traceApi.byBatch(traceBatchNo.value.trim())
  } catch {
    ElMessage.warning('未找到该批次记录')
  } finally {
    traceLoading.value = false
  }
}
</script>

<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-input
        v-model="traceBatchNo"
        placeholder="输入批次号..."
        style="width: 300px"
        @keyup.enter="searchTrace"
      />
      <el-button type="primary" :loading="traceLoading" @click="searchTrace">追溯查询</el-button>
    </div>

    <div v-if="traceResult" class="trace-result">
      <el-card v-if="traceResult.receipt" class="trace-card">
        <template #header>
          <el-icon style="vertical-align: middle"><Box /></el-icon>
          <span style="vertical-align: middle">来料信息</span>
        </template>
        <el-descriptions :column="3" size="small" border>
          <el-descriptions-item label="单号">{{ traceResult.receipt.receiptNo }}</el-descriptions-item>
          <el-descriptions-item label="供应商">{{ traceResult.receipt.supplierName }}</el-descriptions-item>
          <el-descriptions-item label="物料">{{ traceResult.receipt.productName }}</el-descriptions-item>
          <el-descriptions-item label="批次号">{{ traceResult.receipt.batchNo }}</el-descriptions-item>
          <el-descriptions-item label="数量">{{ traceResult.receipt.quantity }} {{ traceResult.receipt.unit }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="calculateResultColor(traceResult.receipt.status)" size="small">
              {{ statusLabel(traceResult.receipt.status, IQC_RECEIPT_STATUS_OPTIONS) }}
            </el-tag>
          </el-descriptions-item>
        </el-descriptions>
      </el-card>

      <el-card v-if="traceResult.inspections.length > 0" class="trace-card">
        <template #header>
          <el-icon style="vertical-align: middle"><Document /></el-icon>
          <span style="vertical-align: middle">检验记录 ({{ traceResult.inspections.length }})</span>
        </template>
        <el-table :data="traceResult.inspections" size="small" stripe>
          <el-table-column prop="inspectionNo" label="检验单号" width="180" />
          <el-table-column prop="sampleSize" label="样本量" width="70" />
          <el-table-column label="Ac/Re" width="70">
            <template #default="{ row }">{{ row.ac }}/{{ row.re }}</template>
          </el-table-column>
          <el-table-column prop="defectQty" label="不合格" width="70" />
          <el-table-column label="结果" width="80">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">
                {{ row.result === 'pass' ? '合格' : '不合格' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验员" width="90" />
          <el-table-column prop="inspectedAt" label="时间" width="160">
            <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
          </el-table-column>
        </el-table>
      </el-card>

      <el-card v-if="traceResult.anomalies.length > 0" class="trace-card">
        <template #header>
          <el-icon style="vertical-align: middle"><WarningFilled /></el-icon>
          <span style="vertical-align: middle">异常记录 ({{ traceResult.anomalies.length }})</span>
        </template>
        <el-table :data="traceResult.anomalies" size="small" stripe>
          <el-table-column prop="anomalyNo" label="异常单号" width="180" />
          <el-table-column label="类型" width="80">
            <template #default="{ row }">{{ statusLabel(row.anomalyType, IQC_ANOMALY_TYPE_OPTIONS) }}</template>
          </el-table-column>
          <el-table-column label="严重程度" width="80">
            <template #default="{ row }">
              <el-tag :type="row.severity === 'critical' ? 'danger' : 'warning'" size="small">
                {{ statusLabel(row.severity, IQC_SEVERITY_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column label="状态" width="80">
            <template #default="{ row }">
              <el-tag :type="row.status === 'resolved' || row.status === 'closed' ? 'success' : 'danger'" size="small">
                {{ statusLabel(row.status, IQC_ANOMALY_STATUS_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>

      <el-card v-if="traceResult.supplierScore" class="trace-card">
        <template #header>
          <el-icon style="vertical-align: middle"><DataAnalysis /></el-icon>
          <span style="vertical-align: middle">供应商评分</span>
        </template>
        <el-descriptions :column="3" size="small" border>
          <el-descriptions-item label="评分">{{ traceResult.supplierScore.score }}</el-descriptions-item>
          <el-descriptions-item label="评级">{{ traceResult.supplierScore.grade }} 级</el-descriptions-item>
          <el-descriptions-item label="评估日期">{{ formatDate(traceResult.supplierScore.scoreDate) }}</el-descriptions-item>
        </el-descriptions>
      </el-card>
    </div>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.trace-result { display: flex; flex-direction: column; gap: 12px; }
.trace-card { margin-bottom: 0; }
</style>

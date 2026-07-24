<script setup lang="ts">
import { ref } from 'vue'
import { ElMessage } from 'element-plus'
import { traceApi } from '@/api/iqc'
import { Box, Document, WarningFilled, DataAnalysis, Search, Refresh } from '@element-plus/icons-vue'
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
  } catch (e: any) {
    if (e?.response?.status === 404) {
      ElMessage.warning('未找到该批次记录')
    } else {
      ElMessage.error(e?.response?.data?.message || '追溯查询失败')
    }
  } finally {
    traceLoading.value = false
  }
}
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-left">
        <el-icon class="page-header-icon"><Search /></el-icon>
        <div class="page-header-text">
          <h1>来料追溯</h1>
          <p>来料批次全流程追溯查询</p>
        </div>
      </div>
      <div class="page-header-right">
        <el-button :icon="Refresh" circle @click="traceResult = null; traceBatchNo = ''" title="重置" />
      </div>
    </div>

    <!-- Toolbar / Search Bar -->
    <div class="search-bar">
      <el-input
        v-model="traceBatchNo"
        placeholder="输入批次号进行追溯查询..."
        clearable
        :prefix-icon="Search"
        style="width: 400px"
        size="large"
        @keyup.enter="searchTrace"
        @clear="traceResult = null"
      />
      <el-button type="primary" size="large" :loading="traceLoading" @click="searchTrace" :icon="Search">
        追溯查询
      </el-button>
    </div>

    <!-- Trace Result -->
    <div v-if="traceResult" class="trace-result">
      <div class="data-card" v-if="traceResult.receipt">
        <div class="data-card-header">
          <span class="data-card-title">
            <el-icon><Box /></el-icon>
            来料信息
          </span>
          <el-tag :type="calculateResultColor(traceResult.receipt.status)" size="small" effect="dark">
            {{ statusLabel(traceResult.receipt.status, IQC_RECEIPT_STATUS_OPTIONS) }}
          </el-tag>
        </div>
        <el-descriptions :column="3" border size="small" style="padding: 16px 20px">
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
      </div>

      <div class="data-card" v-if="traceResult.inspections.length > 0">
        <div class="data-card-header">
          <span class="data-card-title">
            <el-icon><Document /></el-icon>
            检验记录
          </span>
          <el-tag type="info" size="small" effect="plain">{{ traceResult.inspections.length }} 条</el-tag>
        </div>
        <el-table :data="traceResult.inspections" stripe size="small">
          <el-table-column prop="inspectionNo" label="检验单号" width="180" />
          <el-table-column prop="sampleSize" label="样本量" width="70" />
          <el-table-column label="Ac/Re" width="70">
            <template #default="{ row }">{{ row.ac }}/{{ row.re }}</template>
          </el-table-column>
          <el-table-column prop="defectQty" label="不合格" width="70" />
          <el-table-column label="结果" width="80">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small" effect="dark">
                {{ row.result === 'pass' ? '合格' : '不合格' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验员" width="90" />
          <el-table-column prop="inspectedAt" label="时间" width="160">
            <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
          </el-table-column>
        </el-table>
      </div>

      <div class="data-card" v-if="traceResult.anomalies.length > 0">
        <div class="data-card-header">
          <span class="data-card-title">
            <el-icon><WarningFilled /></el-icon>
            异常记录
          </span>
          <el-tag type="danger" size="small" effect="plain">{{ traceResult.anomalies.length }} 条</el-tag>
        </div>
        <el-table :data="traceResult.anomalies" stripe size="small">
          <el-table-column prop="anomalyNo" label="异常单号" width="180" />
          <el-table-column label="类型" width="80">
            <template #default="{ row }">{{ statusLabel(row.anomalyType, IQC_ANOMALY_TYPE_OPTIONS) }}</template>
          </el-table-column>
          <el-table-column label="严重程度" width="90">
            <template #default="{ row }">
              <el-tag :type="row.severity === 'critical' ? 'danger' : 'warning'" size="small" effect="dark">
                {{ statusLabel(row.severity, IQC_SEVERITY_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="row.status === 'resolved' || row.status === 'closed' ? 'success' : 'danger'" size="small" effect="dark">
                {{ statusLabel(row.status, IQC_ANOMALY_STATUS_OPTIONS) }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <div class="data-card" v-if="traceResult.supplierScore">
        <div class="data-card-header">
          <span class="data-card-title">
            <el-icon><DataAnalysis /></el-icon>
            供应商评分
          </span>
        </div>
        <el-descriptions :column="3" border size="small" style="padding: 16px 20px">
          <el-descriptions-item label="评分">{{ traceResult.supplierScore.score }}</el-descriptions-item>
          <el-descriptions-item label="评级">{{ traceResult.supplierScore.grade }} 级</el-descriptions-item>
          <el-descriptions-item label="评估日期">{{ formatDate(traceResult.supplierScore.scoreDate) }}</el-descriptions-item>
        </el-descriptions>
      </div>
    </div>

    <!-- Empty / Initial State -->
    <div v-else class="empty-state">
      <el-icon :size="48" style="color: var(--el-text-color-placeholder); margin-bottom: 12px"><Search /></el-icon>
      <p>请输入批次号进行来料追溯查询</p>
    </div>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }

/* ─── Page Header ─────────────────────────────── */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}
.page-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}
.page-header-icon {
  width: 44px;
  height: 44px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-color-info-light-9);
  border-radius: 10px;
  color: var(--el-color-info);
  font-size: 22px;
}
.page-header-text h1 {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  line-height: 1.3;
}
.page-header-text p {
  margin: 2px 0 0;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}
.page-header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

/* ─── Search Bar ──────────────────────────────── */
.search-bar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
  padding: 20px 24px;
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}
.search-bar :deep(.el-input__wrapper) {
  border-radius: 8px;
}

/* ─── Trace Result ────────────────────────────── */
.trace-result {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.data-card {
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  overflow: hidden;
}
.data-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 14px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
}
.data-card-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

/* ─── Empty State ─────────────────────────────── */
.empty-state {
  text-align: center;
  padding: 80px 20px;
  color: var(--el-text-color-secondary);
  background: #fff;
  border-radius: 10px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
}
.empty-state p {
  margin: 8px 0 0;
  font-size: 14px;
}
</style>

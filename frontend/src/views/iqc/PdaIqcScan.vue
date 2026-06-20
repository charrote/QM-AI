<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { receiptApi, inspectionApi, aiRiskApi } from '@/api/iqc'
import { Iphone, Camera, EditPen, Document, Cpu, Search } from '@element-plus/icons-vue'
import type { IqcReceipt, IqcReceiptDetail, AiRiskScore } from '@/types/iqc'
import { IQC_RECEIPT_STATUS_OPTIONS } from '@/types/iqc'

defineOptions({ name: 'PdaIqcScan' })

// ─── State ──────────────────────────────────────────
const scanInput = ref('')
const scanMode = ref<'scan' | 'result' | 'detail'>('scan')
const currentReceipt = ref<IqcReceipt | null>(null)
const receiptDetail = ref<IqcReceiptDetail | null>(null)
const aiRisk = ref<AiRiskScore | null>(null)
const loading = ref(false)
const scanHistory = ref<IqcReceipt[]>([])
const isManualEntry = ref(false)

// 手动录入表单
const manualForm = ref({
  receiptNo: '',
  supplierName: '',
  productName: '',
  batchNo: '',
  quantity: 0,
})

// ─── Auto-focus scan input ──────────────────────────
onMounted(() => {
  focusScanInput()
})

function focusScanInput() {
  nextTick(() => {
    const el = document.querySelector('.scan-input-field') as HTMLInputElement
    if (el) el.focus()
  })
}

// ─── Scan handler ───────────────────────────────────
async function handleScan() {
  const code = scanInput.value.trim()
  if (!code) return

  loading.value = true
  scanInput.value = ''

  try {
    // 尝试按批次号查询
    const { traceApi } = await import('@/api/iqc')
    const trace = await traceApi.byBatch(code)
    if (trace.receipt) {
      currentReceipt.value = trace.receipt
      receiptDetail.value = await receiptApi.get(trace.receipt.id)
      await loadAiRisk(trace.receipt.id)
      scanMode.value = 'result'
      ElMessage.success(`已找到批次: ${code}`)
      addToHistory(trace.receipt)
      return
    }
  } catch {
    // 批次号未找到，尝试按收货单号查询
  }

  try {
    // 尝试按收货单号查询
    const receipts = await receiptApi.list({ keyword: code, page: 1, pageSize: 10 })
    if (receipts.items.length > 0) {
      currentReceipt.value = receipts.items[0]
      receiptDetail.value = await receiptApi.get(receipts.items[0].id)
      await loadAiRisk(receipts.items[0].id)
      scanMode.value = 'result'
      ElMessage.success(`已找到单据: ${code}`)
      addToHistory(receipts.items[0])
      return
    }
  } catch {
    // ignore
  }

  // 未找到，提示新建
  ElMessage.info(`未找到 "${code}"，请手动录入或重新扫码`)
  isManualEntry.value = true
  manualForm.value.receiptNo = code
}

async function loadAiRisk(receiptId: number) {
  try {
    aiRisk.value = await aiRiskApi.analyze(receiptId)
  } catch {
    aiRisk.value = null
  }
}

async function handleScanFromHistory(item: IqcReceipt) {
  loading.value = true
  try {
    currentReceipt.value = item
    receiptDetail.value = await receiptApi.get(item.id)
    await loadAiRisk(item.id)
    scanMode.value = 'result'
  } catch {
    ElMessage.error('加载失败')
  } finally {
    loading.value = false
  }
}

function addToHistory(receipt: IqcReceipt) {
  // 去重加入历史
  const existing = scanHistory.value.findIndex(h => h.id === receipt.id)
  if (existing >= 0) {
    scanHistory.value[existing] = receipt
  } else {
    scanHistory.value.unshift(receipt)
    if (scanHistory.value.length > 20) scanHistory.value.pop()
  }
}

// ─── Manual entry ───────────────────────────────────
async function submitManualEntry() {
  if (!manualForm.value.receiptNo || !manualForm.value.supplierName || !manualForm.value.productName) {
    ElMessage.warning('请填写完整信息')
    return
  }

  loading.value = true
  try {
    const result = await receiptApi.create({
      receiptNo: manualForm.value.receiptNo,
      supplierId: 0, // PDA 简化模式
      productId: 0,
      batchNo: manualForm.value.batchNo,
      quantity: manualForm.value.quantity || 1,
      receiptDate: new Date().toISOString(),
    })
    currentReceipt.value = result
    receiptDetail.value = result
    ElMessage.success('来料登记已创建')
    scanMode.value = 'result'
    isManualEntry.value = false
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  } finally {
    loading.value = false
  }
}

// ─── Start inspection ──────────────────────────────
async function startInspection() {
  if (!currentReceipt.value) return
  try {
    // 触发检验（后端自动创建检验单）
    await receiptApi.get(currentReceipt.value.id)
    ElMessage.success('检验单已生成')
    scanMode.value = 'scan'
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

// ─── Reset ──────────────────────────────────────────
function resetScan() {
  scanMode.value = 'scan'
  currentReceipt.value = null
  receiptDetail.value = null
  aiRisk.value = null
  isManualEntry.value = false
  focusScanInput()
}

// ─── Status helpers ────────────────────────────────
const statusLabel = (status: string) => {
  const opt = IQC_RECEIPT_STATUS_OPTIONS.find(o => o.value === status)
  return opt?.label || status
}

const statusColor = (status: string) => {
  switch (status) {
    case 'qualified': return '#67c23a'
    case 'unqualified':
    case 'anomaly': return '#f56c6c'
    case 'inspecting': return '#e6a23c'
    default: return '#909399'
  }
}
</script>

<template>
  <div class="pda-container">
    <!-- Header -->
    <div class="pda-header">
      <div class="pda-title">
        <el-icon style="vertical-align: middle"><Iphone /></el-icon>
        <span style="vertical-align: middle">PDA 扫码录入</span>
      </div>
      <div class="pda-subtitle">IQC 来料检验 · 移动端</div>
    </div>

    <!-- ══════════════════════════════════════════════ -->
    <!-- SCAN MODE -->
    <!-- ══════════════════════════════════════════════ -->
    <div v-if="scanMode === 'scan'" class="pda-content">
      <!-- Scan Input -->
      <div class="scan-box" @click="focusScanInput">
        <div class="scan-icon">
          <el-icon :size="32"><Camera /></el-icon>
        </div>
        <input
          ref="scanInputRef"
          v-model="scanInput"
          class="scan-input-field"
          placeholder="点击此处，扫码枪扫描条码..."
          @keyup.enter="handleScan"
        />
        <div class="scan-hint">扫码自动查询 | 按 Enter 提交</div>
      </div>

      <!-- Manual Entry Toggle -->
      <div class="toggle-manual" @click="isManualEntry = !isManualEntry">
        <template v-if="isManualEntry">
          ← 返回扫码
        </template>
        <template v-else>
          <el-icon style="vertical-align: middle"><EditPen /></el-icon>
          <span style="vertical-align: middle">手动录入</span>
        </template>
      </div>

      <!-- Manual Entry Form -->
      <div v-if="isManualEntry" class="manual-form">
        <div class="form-field">
          <label>收货单号 *</label>
          <input v-model="manualForm.receiptNo" placeholder="如: REC-001" />
        </div>
        <div class="form-field">
          <label>供应商 *</label>
          <input v-model="manualForm.supplierName" placeholder="供应商名称" />
        </div>
        <div class="form-field">
          <label>物料 *</label>
          <input v-model="manualForm.productName" placeholder="物料名称" />
        </div>
        <div class="form-row">
          <div class="form-field half">
            <label>批次号</label>
            <input v-model="manualForm.batchNo" placeholder="批次号" />
          </div>
          <div class="form-field half">
            <label>数量</label>
            <input v-model.number="manualForm.quantity" type="number" placeholder="数量" />
          </div>
        </div>
        <button class="pda-btn primary" :disabled="loading" @click="submitManualEntry">
          {{ loading ? '提交中...' : '提交登记' }}
        </button>
      </div>

      <!-- Scan History -->
      <div v-if="scanHistory.length > 0" class="scan-history">
        <div class="history-title">
          <el-icon style="vertical-align: middle"><Document /></el-icon>
          <span style="vertical-align: middle">扫码记录</span>
        </div>
        <div
          v-for="item in scanHistory"
          :key="item.id"
          class="history-item"
          @click="handleScanFromHistory(item)"
        >
          <div class="history-left">
            <div class="history-no">{{ item.receiptNo }}</div>
            <div class="history-meta">{{ item.supplierName }} · {{ item.productName }}</div>
          </div>
          <div class="history-right">
            <span :style="{ color: statusColor(item.status) }" class="history-status">
              {{ statusLabel(item.status) }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════ -->
    <!-- RESULT MODE -->
    <!-- ══════════════════════════════════════════════ -->
    <div v-else-if="scanMode === 'result' && currentReceipt" class="pda-content">
      <div class="result-card">
        <div class="result-header">
          <span class="result-label">来料登记</span>
          <span :style="{ color: statusColor(currentReceipt.status) }" class="result-status">
            {{ statusLabel(currentReceipt.status) }}
          </span>
        </div>
        <div class="result-body">
          <div class="result-row">
            <span class="result-key">单号</span>
            <span class="result-val">{{ currentReceipt.receiptNo }}</span>
          </div>
          <div class="result-row">
            <span class="result-key">供应商</span>
            <span class="result-val">{{ currentReceipt.supplierName }}</span>
          </div>
          <div class="result-row">
            <span class="result-key">物料</span>
            <span class="result-val">{{ currentReceipt.productName }}</span>
          </div>
          <div class="result-row">
            <span class="result-key">批次号</span>
            <span class="result-val">{{ currentReceipt.batchNo || '-' }}</span>
          </div>
          <div class="result-row">
            <span class="result-key">数量</span>
            <span class="result-val">{{ currentReceipt.quantity }} {{ currentReceipt.unit }}</span>
          </div>
        </div>

        <!-- AI Risk -->
        <div v-if="aiRisk" class="ai-risk-section">
          <div class="risk-header">
            <span>
              <el-icon style="vertical-align: middle"><Cpu /></el-icon>
              <span style="vertical-align: middle">AI 风险评分</span>
            </span>
            <span :class="['risk-level', aiRisk.level]">
              {{ aiRisk.level === 'high' ? '高风险' : aiRisk.level === 'warning' ? '预警' : '低风险' }}
            </span>
          </div>
          <div class="risk-score-bar">
            <div
              class="risk-score-fill"
              :style="{ width: aiRisk.score + '%', background: aiRisk.score >= 70 ? '#f56c6c' : aiRisk.score >= 40 ? '#e6a23c' : '#67c23a' }"
            />
            <span class="risk-score-text">{{ aiRisk.score }}/100</span>
          </div>
          <div v-if="aiRisk.recommendations.length > 0" class="risk-recs">
            <div v-for="(rec, i) in aiRisk.recommendations" :key="i" class="risk-rec">• {{ rec }}</div>
          </div>
        </div>

        <!-- Actions -->
        <div class="result-actions">
          <button class="pda-btn primary" @click="startInspection">
            <el-icon style="vertical-align: middle"><Search /></el-icon>
            <span style="vertical-align: middle">开始检验</span>
          </button>
          <button class="pda-btn outline" @click="resetScan">
            ← 返回扫码
          </button>
        </div>
      </div>
    </div>

    <!-- Footer -->
    <div class="pda-footer">
      QM-AI PDA · v1.0
    </div>
  </div>
</template>

<style scoped>
.pda-container {
  max-width: 480px;
  margin: 0 auto;
  min-height: 100vh;
  background: #f5f7fa;
  display: flex;
  flex-direction: column;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
}

.pda-header {
  background: linear-gradient(135deg, #1a73e8, #0d47a1);
  color: white;
  padding: 20px 16px 16px;
  text-align: center;
}

.pda-title {
  font-size: 20px;
  font-weight: 700;
}

.pda-subtitle {
  font-size: 12px;
  opacity: 0.8;
  margin-top: 4px;
}

.pda-content {
  flex: 1;
  padding: 16px;
  overflow-y: auto;
}

/* Scan Box */
.scan-box {
  background: white;
  border: 2px dashed #d9d9d9;
  border-radius: 12px;
  padding: 32px 16px;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s;
}

.scan-box:focus-within {
  border-color: #1a73e8;
  background: #f0f7ff;
}

.scan-icon {
  font-size: 48px;
  margin-bottom: 8px;
}

.scan-input-field {
  width: 100%;
  border: none;
  outline: none;
  text-align: center;
  font-size: 16px;
  padding: 8px;
  background: transparent;
  color: #333;
}

.scan-input-field::placeholder {
  color: #bbb;
}

.scan-hint {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}

/* Manual Entry */
.toggle-manual {
  text-align: center;
  color: #1a73e8;
  padding: 12px;
  font-size: 14px;
  cursor: pointer;
}

.manual-form {
  background: white;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 12px;
}

.form-field {
  margin-bottom: 12px;
}

.form-field label {
  display: block;
  font-size: 13px;
  color: #666;
  margin-bottom: 4px;
  font-weight: 500;
}

.form-field input {
  width: 100%;
  border: 1px solid #d9d9d9;
  border-radius: 6px;
  padding: 10px 12px;
  font-size: 14px;
  outline: none;
  box-sizing: border-box;
}

.form-field input:focus {
  border-color: #1a73e8;
}

.form-row {
  display: flex;
  gap: 12px;
}

.form-field.half {
  flex: 1;
}

/* Buttons */
.pda-btn {
  display: block;
  width: 100%;
  padding: 14px;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  border: none;
  cursor: pointer;
  text-align: center;
  margin-bottom: 8px;
}

.pda-btn.primary {
  background: #1a73e8;
  color: white;
}

.pda-btn.primary:disabled {
  background: #a0c4ff;
}

.pda-btn.outline {
  background: white;
  color: #1a73e8;
  border: 1px solid #1a73e8;
}

/* History */
.scan-history {
  margin-top: 16px;
}

.history-title {
  font-size: 14px;
  font-weight: 600;
  color: #333;
  margin-bottom: 8px;
}

.history-item {
  background: white;
  border-radius: 8px;
  padding: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
  cursor: pointer;
}

.history-no {
  font-size: 14px;
  font-weight: 600;
  color: #333;
}

.history-meta {
  font-size: 12px;
  color: #999;
  margin-top: 2px;
}

.history-status {
  font-size: 13px;
  font-weight: 500;
}

/* Result Card */
.result-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 14px 16px;
  background: #f0f7ff;
  border-bottom: 1px solid #e8e8e8;
}

.result-label {
  font-weight: 600;
  font-size: 15px;
}

.result-status {
  font-weight: 600;
  font-size: 14px;
}

.result-body {
  padding: 12px 16px;
}

.result-row {
  display: flex;
  justify-content: space-between;
  padding: 6px 0;
  border-bottom: 1px solid #f5f5f5;
}

.result-key {
  font-size: 13px;
  color: #666;
}

.result-val {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

/* AI Risk */
.ai-risk-section {
  padding: 12px 16px;
  border-top: 1px solid #e8e8e8;
}

.risk-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
  font-size: 14px;
  font-weight: 500;
}

.risk-level {
  font-size: 12px;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 600;
}

.risk-level.high { background: #fef0f0; color: #f56c6c; }
.risk-level.warning { background: #fdf6ec; color: #e6a23c; }
.risk-level.low { background: #f0f9eb; color: #67c23a; }

.risk-score-bar {
  height: 12px;
  background: #f0f0f0;
  border-radius: 6px;
  position: relative;
  overflow: hidden;
}

.risk-score-fill {
  height: 100%;
  border-radius: 6px;
  transition: width 0.5s;
}

.risk-score-text {
  font-size: 11px;
  color: #999;
  margin-left: 4px;
}

.risk-recs {
  margin-top: 8px;
}

.risk-rec {
  font-size: 12px;
  color: #666;
  padding: 2px 0;
}

/* Actions */
.result-actions {
  padding: 12px 16px;
  border-top: 1px solid #e8e8e8;
}

/* Footer */
.pda-footer {
  text-align: center;
  padding: 12px;
  font-size: 12px;
  color: #999;
  background: #f5f7fa;
}

/* Loading */
.pda-content:empty::after {
  content: '加载中...';
  display: block;
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>

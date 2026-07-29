<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Search, Check, Close, Document, List, ArrowLeft, Clock } from '@element-plus/icons-vue'
import { patrolApi } from '@/api/ipqc'
import type { IpqcPatrolDetail, IpqcPatrolItemSubmit } from '@/types/ipqc'
import { IPQC_PATROL_CONCLUSION_OPTIONS, INSPECTION_RESULT_OPTIONS } from '@/types/ipqc'

defineOptions({ name: 'PdaIpqcScan' })

const router = useRouter()
const route = useRoute()

// ─── State ──────────────────────────────────────
const scanInput = ref('')
const patrol = ref<IpqcPatrolDetail | null>(null)
const loading = ref(false)
const submitting = ref(false)
const items = ref<IpqcPatrolItemSubmit[]>([])

const patrolConclusion = ref('pending')
const patrolRemarks = ref('')

// ─── Scan & Load ───────────────────────────────
async function handleScan() {
  const patrolId = scanInput.value.trim()
  if (!patrolId) {
    ElMessage.warning('请输入或扫描巡检编号')
    return
  }

  loading.value = true
  try {
    // Support both ID and patrolNo
    const id = parseInt(patrolId)
    const detail = isNaN(id)
      ? null  // Would need a findByPatrolNo endpoint
      : await patrolApi.get(id)

    if (!detail) {
      ElMessage.error('未找到此巡检记录')
      loading.value = false
      return
    }

    patrol.value = detail
    patrolConclusion.value = detail.conclusion || 'pending'
    patrolRemarks.value = detail.remarks || ''

    // Initialize items from existing or create defaults
    items.value = (detail.items || []).map(i => ({
      id: i.id,
      itemName: i.itemName,
      itemCode: i.itemCode,
      usl: i.usl,
      lsl: i.lsl,
      dataType: i.dataType,
      actualValue: i.actualValue,
      result: i.result || 'pending',
      imageUrls: i.imageUrls,
    }))

    if (items.value.length === 0) {
      items.value = [
        { itemName: '外观检查', dataType: 'visual', result: 'pending' },
        { itemName: '尺寸测量', dataType: 'numeric', result: 'pending' },
        { itemName: '功能检查', dataType: 'attribute', result: 'pending' },
      ]
    }

    scanInput.value = ''
    ElMessage.success(`已加载巡检 #${detail.patrolNo}`)
  } catch {
    ElMessage.error('加载巡检记录失败')
  } finally {
    loading.value = false
  }
}

// ─── Submit ────────────────────────────────────
async function handleSubmit() {
  if (!patrol.value) return

  submitting.value = true
  try {
    await patrolApi.submit(patrol.value.id, {
      conclusion: patrolConclusion.value,
      remarks: patrolRemarks.value,
      items: items.value,
    })
    ElMessage.success('巡检已提交')
    // Reset
    patrol.value = null
    items.value = []
    patrolConclusion.value = 'pending'
    patrolRemarks.value = ''
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '提交失败')
  } finally {
    submitting.value = false
  }
}

function goBack() {
  router.back()
}

function resultTag(r: string) {
  return r === 'pass' ? 'success' : r === 'fail' ? 'danger' : 'info'
}
</script>

<template>
  <div class="pda-container">
    <!-- Header -->
    <div class="pda-header">
      <button class="back-btn" @click="goBack">
        <el-icon :size="22"><ArrowLeft /></el-icon>
        返回
      </button>
      <h2>
        <el-icon :size="22" color="var(--el-color-primary)"><Document /></el-icon>
        IPQC 巡检
      </h2>
    </div>

    <!-- Scan Input -->
    <div class="scan-section" v-if="!patrol">
      <div class="scan-header">
        <el-icon :size="36" color="var(--el-color-primary)"><Search /></el-icon>
        <span class="scan-tip">扫描或输入巡检编号</span>
      </div>
      <div class="scan-input-row">
        <input
          v-model="scanInput"
          type="text"
          class="pda-input scan-input"
          placeholder="扫码或输入编号..."
          @keyup.enter="handleScan"
          autofocus
        />
        <button class="pda-btn pda-btn--primary scan-btn" @click="handleScan" :disabled="loading">
          <el-icon :size="20"><Search /></el-icon>
          {{ loading ? '加载中...' : '查询' }}
        </button>
      </div>
    </div>

    <!-- Patrol Form -->
    <div class="patrol-form" v-if="patrol">
      <!-- Patrol Info -->
      <div class="patrol-info">
        <div class="patrol-info__header">
          <el-icon :size="20" color="var(--el-color-primary)"><Document /></el-icon>
          <span class="patrol-info__title">巡检信息</span>
        </div>
        <div class="info-row"><span class="label">巡检编号</span><span class="value">{{ patrol.patrolNo }}</span></div>
        <div class="info-row"><span class="label">设备</span><span class="value">{{ patrol.equipmentName || '-' }}</span></div>
        <div class="info-row"><span class="label">工序</span><span class="value">{{ patrol.processName || '-' }}</span></div>
        <div class="info-row"><span class="label">计划时间</span><span class="value">{{ new Date(patrol.scheduledTime).toLocaleString('zh-CN') }}</span></div>
      </div>

      <!-- Inspection Items -->
      <div class="items-section">
        <div class="section-header">
          <el-icon color="var(--el-color-primary)"><List /></el-icon>
          <span>检验项目</span>
        </div>
        <div v-for="(item, idx) in items" :key="idx" class="inspect-item">
          <div class="item-header">
            <span class="item-name">{{ item.itemName }}</span>
            <div class="item-result">
              <button
                :class="['result-btn', { active: item.result === 'pass' }]"
                @click="item.result = 'pass'"
              >
                <el-icon :size="16"><Check /></el-icon>
                合格
              </button>
              <button
                :class="['result-btn', 'result-btn--fail', { active: item.result === 'fail' }]"
                @click="item.result = 'fail'"
              >
                <el-icon :size="16"><Close /></el-icon>
                不合格
              </button>
            </div>
          </div>

          <div class="item-value" v-if="item.dataType === 'numeric'">
            <input
              v-model.number="item.actualValue"
              type="number"
              :step="0.01"
              class="pda-input value-input"
              :placeholder="`实测值 (${item.lsl ?? '-'} ~ ${item.usl ?? '-'})`"
            />
            <span v-if="item.usl != null && item.actualValue != null" class="value-hint">
              {{ item.actualValue >= (item.lsl ?? -Infinity) && item.actualValue <= (item.usl ?? Infinity) ? '✅ 合格' : '❌ 超差' }}
            </span>
          </div>
        </div>
      </div>

      <!-- Remarks -->
      <div class="remarks-section">
        <div class="section-header">
          <el-icon color="var(--el-color-primary)"><Document /></el-icon>
          <span>备注</span>
        </div>
        <textarea v-model="patrolRemarks" class="pda-textarea" rows="3" placeholder="备注信息..." />
      </div>

      <!-- Conclusion -->
      <div class="conclusion-section">
        <div class="section-header">
          <el-icon color="var(--el-color-primary)"><Check /></el-icon>
          <span>检验结论</span>
        </div>
        <div class="conclusion-btns">
          <button
            v-for="opt in IPQC_PATROL_CONCLUSION_OPTIONS"
            :key="opt.value"
            :class="['conclusion-btn', { active: patrolConclusion === opt.value }]"
            @click="patrolConclusion = opt.value"
          >
            {{ opt.label }}
          </button>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="action-buttons">
        <button class="pda-btn pda-btn--secondary" @click="patrol = null" :disabled="submitting">
          取消
        </button>
        <button class="pda-btn pda-btn--primary" @click="handleSubmit" :disabled="submitting">
          <el-icon :size="20"><Check /></el-icon>
          {{ submitting ? '提交中...' : '提交巡检' }}
        </button>
      </div>
    </div>

    <!-- Scan History (placeholder) -->
    <div class="history-section" v-if="!patrol">
      <div class="section-header">
        <el-icon color="var(--el-text-secondary)"><Clock /></el-icon>
        <span>最近扫描</span>
      </div>
      <div class="history-empty">暂无记录</div>
    </div>
  </div>
</template>



<style scoped>
.pda-container {
  max-width: 480px;
  margin: 0 auto;
  padding: 12px 16px;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  min-height: 100vh;
  background: var(--bg-base, #f5f7fa);
}

/* ─── Header ───────────────────────── */
.pda-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 20px;
  position: sticky;
  top: 0;
  background: var(--bg-base, #f5f7fa);
  padding: 8px 0;
  z-index: 10;
}

.back-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  background: none;
  border: none;
  font-size: 16px;
  color: var(--primary, #1677ff);
  cursor: pointer;
  padding: 8px 12px;
  border-radius: var(--radius-lg, 8px);
  min-height: 44px;
}

.pda-header h2 {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 18px;
  margin: 0;
  color: var(--el-text-color-primary, #303133);
  font-weight: 600;
}

/* ─── Scan Section ─────────────────── */
.scan-section {
  background: var(--el-bg-color, #fff);
  border-radius: var(--radius-xl, 12px);
  padding: 28px 20px;
  margin-bottom: 16px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.scan-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  margin-bottom: 20px;
}

.scan-tip {
  font-size: 15px;
  color: var(--el-text-color-regular, #606266);
  text-align: center;
  font-weight: 500;
}

.scan-input-row {
  display: flex;
  gap: 8px;
}

.pda-input {
  flex: 1;
  padding: 14px 16px;
  border: 2px solid var(--el-border-color, #dcdfe6);
  border-radius: var(--radius-lg, 10px);
  font-size: 17px;
  outline: none;
  transition: border-color 0.2s;
  background: var(--el-bg-color, #fff);
  min-height: 48px;
}

.pda-input:focus {
  border-color: var(--primary, #1677ff);
}

.scan-btn {
  min-width: 100px;
}

/* ─── Buttons ──────────────────────── */
.pda-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 14px 24px;
  border: none;
  border-radius: var(--radius-lg, 10px);
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.2s;
  white-space: nowrap;
  min-height: 52px;
}

.pda-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.pda-btn--primary {
  background: var(--primary, #409eff);
  color: #fff;
}

.pda-btn--secondary {
  background: var(--el-fill-color-light, #f0f2f5);
  color: var(--el-text-color-regular, #606266);
}

/* ─── Patrol Form ──────────────────── */
.patrol-form {
  background: var(--el-bg-color, #fff);
  border-radius: var(--radius-xl, 12px);
  padding: 16px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.patrol-info {
  background: var(--primary-light-9, #f0f9ff);
  border-radius: var(--radius-lg, 10px);
  padding: 14px 16px;
  margin-bottom: 20px;
}

.patrol-info__header {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 10px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 6px 0;
}

.info-row .label {
  color: var(--el-text-color-secondary, #909399);
  font-size: 14px;
}

.info-row .value {
  color: var(--el-text-color-primary, #303133);
  font-weight: 500;
  font-size: 14px;
}

/* ─── Section Headers ──────────────── */
.section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary, #303133);
  margin-bottom: 12px;
}

/* ─── Items ────────────────────────── */
.items-section {
  margin-bottom: 20px;
}

.inspect-item {
  background: var(--el-fill-color-lighter, #fafafa);
  border-radius: var(--radius-lg, 10px);
  padding: 14px;
  margin-bottom: 10px;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.item-name {
  font-weight: 500;
  font-size: 15px;
  color: var(--el-text-color-primary, #303133);
}

.item-result {
  display: flex;
  gap: 8px;
}

.result-btn {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 10px 16px;
  border: 2px solid var(--success, #67c23a);
  border-radius: 24px;
  background: #fff;
  color: var(--success, #67c23a);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  min-height: 44px;
}

.result-btn--fail {
  border-color: var(--danger, #f56c6c);
  color: var(--danger, #f56c6c);
}

.result-btn.active {
  background: var(--success, #67c23a);
  color: #fff;
}

.result-btn--fail.active {
  background: var(--danger, #f56c6c);
  color: #fff;
}

.item-value {
  display: flex;
  align-items: center;
  gap: 8px;
}

.value-input {
  flex: 1;
  padding: 12px 14px;
  font-size: 15px;
  border: 2px solid var(--el-border-color, #dcdfe6);
  border-radius: var(--radius-md, 8px);
  outline: none;
  min-height: 44px;
}

.value-hint {
  font-size: 13px;
  white-space: nowrap;
}

/* ─── Remarks ──────────────────────── */
.remarks-section {
  margin-bottom: 20px;
}

.pda-textarea {
  width: 100%;
  padding: 12px 14px;
  border: 2px solid var(--el-border-color, #dcdfe6);
  border-radius: var(--radius-lg, 10px);
  font-size: 15px;
  outline: none;
  resize: vertical;
  box-sizing: border-box;
  font-family: inherit;
  min-height: 80px;
  background: var(--el-bg-color, #fff);
  color: var(--el-text-color-primary, #303133);
}

.pda-textarea:focus {
  border-color: var(--primary, #1677ff);
}

/* ─── Conclusion ───────────────────── */
.conclusion-section {
  margin-bottom: 24px;
}

.conclusion-btns {
  display: flex;
  gap: 8px;
}

.conclusion-btn {
  flex: 1;
  padding: 14px 12px;
  border: 2px solid var(--el-border-color, #dcdfe6);
  border-radius: var(--radius-lg, 10px);
  background: var(--el-bg-color, #fff);
  color: var(--el-text-color-regular, #606266);
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  text-align: center;
  min-height: 52px;
  transition: all 0.2s;
}

.conclusion-btn.active {
  border-color: var(--primary, #1677ff);
  background: var(--primary-light-9, #ecf5ff);
  color: var(--primary, #1677ff);
  font-weight: 600;
}

/* ─── Action Buttons ───────────────── */
.action-buttons {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

.action-buttons .pda-btn {
  flex: 1;
  text-align: center;
}

/* ─── History ──────────────────────── */
.history-section {
  background: var(--el-bg-color, #fff);
  border-radius: var(--radius-xl, 12px);
  padding: 16px;
  margin-top: 16px;
}

.history-empty {
  text-align: center;
  color: var(--el-text-color-placeholder, #c0c4cc);
  font-size: 14px;
  padding: 24px 0;
}
</style>
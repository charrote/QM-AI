<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
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
      <button class="back-btn" @click="goBack">← 返回</button>
      <h2>IPQC 巡检</h2>
    </div>

    <!-- Scan Input -->
    <div class="scan-section" v-if="!patrol">
      <div class="scan-tip">扫描或输入巡检编号</div>
      <div class="scan-input-row">
        <input
          v-model="scanInput"
          type="text"
          class="pda-input scan-input"
          placeholder="扫码或输入编号..."
          @keyup.enter="handleScan"
          autofocus
        />
        <button class="pda-btn primary" @click="handleScan" :disabled="loading">
          {{ loading ? '加载中...' : '查询' }}
        </button>
      </div>
    </div>

    <!-- Patrol Form -->
    <div class="patrol-form" v-if="patrol">
      <div class="patrol-info">
        <div class="info-row"><span class="label">巡检编号</span><span class="value">{{ patrol.patrolNo }}</span></div>
        <div class="info-row"><span class="label">设备</span><span class="value">{{ patrol.equipmentName || '-' }}</span></div>
        <div class="info-row"><span class="label">工序</span><span class="value">{{ patrol.processName || '-' }}</span></div>
        <div class="info-row"><span class="label">计划时间</span><span class="value">{{ new Date(patrol.scheduledTime).toLocaleString('zh-CN') }}</span></div>
      </div>

      <!-- Inspection Items -->
      <div class="items-section">
        <h3>检验项目</h3>
        <div v-for="(item, idx) in items" :key="idx" class="inspect-item">
          <div class="item-header">
            <span class="item-name">{{ item.itemName }}</span>
            <div class="item-result">
              <button
                :class="['result-btn', { active: item.result === 'pass' }]"
                @click="item.result = 'pass'"
              >✓ 合格</button>
              <button
                :class="['result-btn', 'fail', { active: item.result === 'fail' }]"
                @click="item.result = 'fail'"
              >✕ 不合格</button>
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

      <div class="remarks-section">
        <h3>备注</h3>
        <textarea v-model="patrolRemarks" class="pda-textarea" rows="2" placeholder="备注信息..." />
      </div>

      <div class="conclusion-section">
        <h3>检验结论</h3>
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

      <div class="action-buttons">
        <button class="pda-btn secondary" @click="patrol = null">取消</button>
        <button class="pda-btn primary" @click="handleSubmit" :disabled="submitting">
          {{ submitting ? '提交中...' : '✅ 提交巡检' }}
        </button>
      </div>
    </div>

    <!-- Scan History (placeholder) -->
    <div class="history-section" v-if="!patrol">
      <h3>最近扫描</h3>
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
  background: #f5f7fa;
}

.pda-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  position: sticky;
  top: 0;
  background: #f5f7fa;
  padding: 8px 0;
  z-index: 10;
}

.back-btn {
  background: none;
  border: none;
  font-size: 16px;
  color: #409eff;
  cursor: pointer;
  padding: 4px 8px;
}

.pda-header h2 {
  font-size: 18px;
  margin: 0;
  color: #303133;
}

.scan-section {
  background: #fff;
  border-radius: 12px;
  padding: 24px 16px;
  margin-bottom: 16px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.scan-tip {
  font-size: 14px;
  color: #606266;
  margin-bottom: 12px;
  text-align: center;
}

.scan-input-row {
  display: flex;
  gap: 8px;
}

.pda-input {
  flex: 1;
  padding: 12px 16px;
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  font-size: 16px;
  outline: none;
  transition: border-color 0.2s;
  background: #fff;
}

.pda-input:focus {
  border-color: #409eff;
}

.pda-btn {
  padding: 12px 24px;
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 500;
  cursor: pointer;
  transition: opacity 0.2s;
  white-space: nowrap;
}

.pda-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.pda-btn.primary {
  background: #409eff;
  color: #fff;
}

.pda-btn.secondary {
  background: #f0f2f5;
  color: #606266;
}

.patrol-form {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.06);
}

.patrol-info {
  background: #f0f9ff;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 16px;
  font-size: 14px;
}

.info-row {
  display: flex;
  justify-content: space-between;
  padding: 4px 0;
}

.info-row .label {
  color: #909399;
}

.info-row .value {
  color: #303133;
  font-weight: 500;
}

.items-section h3,
.remarks-section h3,
.conclusion-section h3 {
  font-size: 14px;
  color: #303133;
  margin: 0 0 8px 0;
}

.inspect-item {
  background: #fafafa;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 8px;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.item-name {
  font-weight: 500;
  font-size: 14px;
}

.item-result {
  display: flex;
  gap: 6px;
}

.result-btn {
  padding: 4px 12px;
  border: 1.5px solid #67c23a;
  border-radius: 16px;
  background: #fff;
  color: #67c23a;
  font-size: 13px;
  cursor: pointer;
}

.result-btn.fail {
  border-color: #f56c6c;
  color: #f56c6c;
}

.result-btn.active {
  background: #67c23a;
  color: #fff;
}

.result-btn.fail.active {
  background: #f56c6c;
  color: #fff;
}

.item-value {
  display: flex;
  align-items: center;
  gap: 8px;
}

.value-input {
  flex: 1;
  padding: 8px 12px;
  font-size: 14px;
}

.value-hint {
  font-size: 13px;
  white-space: nowrap;
}

.pda-textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1.5px solid #dcdfe6;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  resize: vertical;
  box-sizing: border-box;
  font-family: inherit;
}

.pda-textarea:focus {
  border-color: #409eff;
}

.conclusion-btns {
  display: flex;
  gap: 8px;
}

.conclusion-btn {
  flex: 1;
  padding: 10px;
  border: 1.5px solid #dcdfe6;
  border-radius: 8px;
  background: #fff;
  color: #606266;
  font-size: 14px;
  cursor: pointer;
  text-align: center;
}

.conclusion-btn.active {
  border-color: #409eff;
  background: #ecf5ff;
  color: #409eff;
  font-weight: 600;
}

.action-buttons {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.action-buttons .pda-btn {
  flex: 1;
  text-align: center;
}

.history-section {
  background: #fff;
  border-radius: 12px;
  padding: 16px;
  margin-top: 16px;
}

.history-section h3 {
  font-size: 14px;
  margin: 0 0 8px 0;
  color: #303133;
}

.history-empty {
  text-align: center;
  color: #c0c4cc;
  font-size: 13px;
  padding: 20px 0;
}
</style>

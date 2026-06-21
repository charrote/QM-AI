<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { riskScoreApi } from '@/api/ipqc'
import { equipmentApi, processApi } from '@/api/basicData'
import type { IpqcAiRiskScore, RiskFactor } from '@/types/ipqc'
import { IPQC_RISK_LEVEL_OPTIONS, IPQC_TREND_OPTIONS } from '@/types/ipqc'
import type { Equipment, Process } from '@/types/basicData'

defineOptions({ name: 'IpqcRiskDashboard' })

// ─── State ──────────────────────────────────────
const equipmentList = ref<Equipment[]>([])
const processList = ref<Process[]>([])
const selectedEquipmentId = ref(0)
const selectedProcessId = ref(0)

const riskScore = ref<IpqcAiRiskScore | null>(null)
const scoreHistory = ref<IpqcAiRiskScore[]>([])
const loading = ref(false)
const historyLoading = ref(false)

// ─── Load Data ─────────────────────────────────
async function loadEquipment() {
  try {
    const res = await equipmentApi.list({ pageSize: 100 })
    equipmentList.value = res.items
    if (res.items.length > 0) selectedEquipmentId.value = res.items[0].id
  } catch { /* ignore */ }
}

async function loadProcesses() {
  try {
    const res = await processApi.list({ pageSize: 100 })
    processList.value = res.items
    if (res.items.length > 0) selectedProcessId.value = res.items[0].id
  } catch { /* ignore */ }
}

async function analyzeRisk() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) {
    ElMessage.warning('请选择设备和工序')
    return
  }
  loading.value = true
  try {
    const result = await riskScoreApi.analyze(selectedEquipmentId.value, selectedProcessId.value)
    riskScore.value = result
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '分析失败')
  } finally {
    loading.value = false
  }
}

async function loadHistory() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) return
  historyLoading.value = true
  try {
    const history = await riskScoreApi.history(selectedEquipmentId.value, selectedProcessId.value, 24)
    scoreHistory.value = history
  } catch {
    // silent
  } finally {
    historyLoading.value = false
  }
}

async function loadLatest() {
  if (!selectedEquipmentId.value || !selectedProcessId.value) return
  try {
    const latest = await riskScoreApi.latest(selectedEquipmentId.value, selectedProcessId.value)
    if (latest) riskScore.value = latest
  } catch {
    // no data yet
  }
}

function levelTag(level: string) {
  const opt = IPQC_RISK_LEVEL_OPTIONS.find(o => o.value === level)
  return opt?.type || 'info'
}

function levelLabel(level: string) {
  const opt = IPQC_RISK_LEVEL_OPTIONS.find(o => o.value === level)
  return opt?.label || level
}

function trendIcon(trend?: string) {
  if (trend === 'rising') return '🔼'
  if (trend === 'falling') return '🔽'
  return '➡️'
}

function trendColor(trend?: string) {
  if (trend === 'rising') return '#f56c6c'
  if (trend === 'falling') return '#67c23a'
  return '#909399'
}

onMounted(async () => {
  await Promise.all([loadEquipment(), loadProcesses()])
})
</script>

<template>
  <div class="page-container">
    <!-- Filters -->
    <div class="toolbar-row">
      <el-select v-model="selectedEquipmentId" filterable placeholder="选择设备" style="width:220px">
        <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
      </el-select>
      <el-select v-model="selectedProcessId" filterable placeholder="选择工序" style="width:220px">
        <el-option v-for="p in processList" :key="p.id" :label="p.name" :value="p.id" />
      </el-select>
      <el-button type="primary" @click="analyzeRisk" :loading="loading">分析风险</el-button>
      <el-button @click="loadLatest">最新评分</el-button>
    </div>

    <!-- Risk Score Card -->
    <div v-if="riskScore" class="risk-card">
      <div class="risk-gauge">
        <div class="score-circle" :style="{ borderColor: riskScore.score >= 70 ? '#f56c6c' : riskScore.score >= 40 ? '#e6a23c' : '#67c23a' }">
          <span class="score-value">{{ riskScore.score }}</span>
          <span class="score-label">/ 100</span>
        </div>
        <div class="score-info">
          <div class="score-level">
            <el-tag :type="levelTag(riskScore.level)" size="large">{{ levelLabel(riskScore.level) }}</el-tag>
            <span class="trend-badge" :style="{ color: trendColor(riskScore.trend) }">
              {{ trendIcon(riskScore.trend) }}
              {{ riskScore.trend === 'rising' ? '上升' : riskScore.trend === 'falling' ? '下降' : '稳定' }}
            </span>
          </div>
          <div class="score-time" v-if="riskScore.lastUpdated">
            更新于 {{ new Date(riskScore.lastUpdated).toLocaleString('zh-CN') }}
          </div>
        </div>
      </div>

      <!-- Recommendations -->
      <div class="section" v-if="riskScore.recommendations && riskScore.recommendations.length > 0">
        <h4>建议措施</h4>
        <el-timeline>
          <el-timeline-item
            v-for="(rec, idx) in riskScore.recommendations"
            :key="idx"
            :timestamp="'建议 ' + (idx + 1)"
            size="small"
            placement="top"
          >
            {{ rec }}
          </el-timeline-item>
        </el-timeline>
      </div>

      <!-- Risk Factors -->
      <div class="section" v-if="riskScore.factors && riskScore.factors.length > 0">
        <h4>风险因素</h4>
        <el-table :data="riskScore.factors" size="small" stripe>
          <el-table-column prop="name" label="因素" min-width="140" />
          <el-table-column prop="description" label="描述" min-width="200" />
          <el-table-column label="当前值" width="100">
            <template #default="{ row }">
              <span v-if="row.currentValue != null">{{ row.currentValue.toFixed(1) }}</span>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="目标值" width="100">
            <template #default="{ row }">
              <span v-if="row.targetValue != null">{{ row.targetValue.toFixed(1) }}</span>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="影响" width="80">
            <template #default="{ row }">
              <el-tag :type="row.impact > 15 ? 'danger' : row.impact > 5 ? 'warning' : 'info'" size="small">
                +{{ row.impact }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <el-icon :size="48" color="#c0c4cc"><DataAnalysis /></el-icon>
      <p>请选择设备和工序后点击"分析风险"</p>
    </div>

    <!-- Score History -->
    <div v-if="scoreHistory.length > 0" class="section" style="margin-top:16px">
      <h4>评分历史（近 24 小时）</h4>
      <el-table :data="scoreHistory" size="small" stripe v-loading="historyLoading" max-height="300">
        <el-table-column label="评分" width="80">
          <template #default="{ row }">
            <el-tag :type="levelTag(row.level)" size="small">{{ row.score }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="等级" width="80">
          <template #default="{ row }">
            <el-tag :type="levelTag(row.level)" size="small">{{ levelLabel(row.level) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="趋势" width="80">
          <template #default="{ row }">
            <span :style="{ color: trendColor(row.trend) }">{{ trendIcon(row.trend) }}</span>
          </template>
        </el-table-column>
        <el-table-column label="时间" width="170">
          <template #default="{ row }">{{ new Date(row.lastUpdated!).toLocaleString('zh-CN') }}</template>
        </el-table-column>
      </el-table>
    </div>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; gap: 16px; }
.toolbar-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }

.risk-card {
  background: var(--bg-card, #fff);
  border-radius: 8px;
  padding: 24px;
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
}

.risk-gauge {
  display: flex;
  align-items: center;
  gap: 24px;
  margin-bottom: 20px;
}

.score-circle {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  border: 6px solid #67c23a;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: var(--bg-card, #fff);
}

.score-value {
  font-size: 36px;
  font-weight: 700;
  line-height: 1;
}

.score-label {
  font-size: 13px;
  color: #909399;
  margin-top: 2px;
}

.score-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.score-level {
  display: flex;
  align-items: center;
  gap: 12px;
}

.trend-badge {
  font-size: 14px;
  font-weight: 500;
}

.score-time {
  font-size: 12px;
  color: #909399;
}

.section h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  color: var(--text-primary, #303133);
  font-weight: 600;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 0;
  color: #909399;
  gap: 12px;
}
</style>

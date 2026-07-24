<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import {
  DataAnalysis,
  WarningFilled,
  CircleCheckFilled,
  TrendCharts,
  Monitor,
  Setting,
  Refresh,
  Document,
} from '@element-plus/icons-vue'
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
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__icon-wrapper">
        <el-icon :size="28"><DataAnalysis /></el-icon>
      </div>
      <div class="page-header__info">
        <h1 class="page-header__title">AI 风险驾驶舱</h1>
        <p class="page-header__subtitle">基于实时数据智能评估生产质量风险等级</p>
      </div>
    </div>

    <!-- Filters -->
    <div class="action-bar">
      <div class="action-bar__left">
        <el-select
          v-model="selectedEquipmentId"
          filterable
          placeholder="选择设备"
          :prefix-icon="Monitor"
          style="width: 220px"
        >
          <el-option v-for="e in equipmentList" :key="e.id" :label="e.name" :value="e.id" />
        </el-select>
        <el-select
          v-model="selectedProcessId"
          filterable
          placeholder="选择工序"
          :prefix-icon="Setting"
          style="width: 220px"
        >
          <el-option v-for="p in processList" :key="p.id" :label="p.name" :value="p.id" />
        </el-select>
        <el-button type="primary" @click="analyzeRisk" :loading="loading" size="default">
          <el-icon><DataAnalysis /></el-icon>
          分析风险
        </el-button>
        <el-button @click="loadLatest" size="default">
          <el-icon><Refresh /></el-icon>
          最新评分
        </el-button>
      </div>
    </div>

    <!-- Risk Score Card -->
    <div v-if="riskScore" class="risk-card">
      <!-- Stat Cards Row -->
      <div class="stat-row">
        <div class="stat-card stat-card--score">
          <div class="stat-card__icon" :style="{ background: riskScore.score >= 70 ? 'var(--el-color-danger-light-9)' : riskScore.score >= 40 ? 'var(--el-color-warning-light-9)' : 'var(--el-color-success-light-9)' }">
            <el-icon :size="22" :style="{ color: riskScore.score >= 70 ? 'var(--el-color-danger)' : riskScore.score >= 40 ? 'var(--el-color-warning)' : 'var(--el-color-success)' }">
              <WarningFilled v-if="riskScore.score >= 70" />
              <WarningFilled v-else-if="riskScore.score >= 40" />
              <CircleCheckFilled v-else />
            </el-icon>
          </div>
          <div class="stat-card__content">
            <div class="stat-card__number">{{ riskScore.score }}</div>
            <div class="stat-card__label">风险评分</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-card__content">
            <div class="stat-card__number">
              <el-tag :type="levelTag(riskScore.level)" size="large" effect="dark">{{ levelLabel(riskScore.level) }}</el-tag>
            </div>
            <div class="stat-card__label">风险等级</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-card__content">
            <div class="stat-card__trend" :style="{ color: trendColor(riskScore.trend) }">
              <el-icon :size="20">
                <TrendCharts v-if="riskScore.trend === 'falling'" />
                <WarningFilled v-else-if="riskScore.trend === 'rising'" />
                <Setting v-else />
              </el-icon>
              {{ riskScore.trend === 'rising' ? '上升' : riskScore.trend === 'falling' ? '下降' : '稳定' }}
            </div>
            <div class="stat-card__label">趋势走向</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-card__content">
            <div class="stat-card__number stat-card__time" v-if="riskScore.lastUpdated">
              {{ new Date(riskScore.lastUpdated).toLocaleString('zh-CN') }}
            </div>
            <div class="stat-card__label">最后更新</div>
          </div>
        </div>
      </div>

      <!-- Recommendations -->
      <div class="section" v-if="riskScore.recommendations && riskScore.recommendations.length > 0">
        <div class="section__title">
          <el-icon color="var(--el-color-primary)"><Document /></el-icon>
          <span>建议措施</span>
        </div>
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
        <div class="section__title">
          <el-icon color="var(--el-color-warning)"><WarningFilled /></el-icon>
          <span>风险因素</span>
        </div>
        <el-table :data="riskScore.factors" stripe>
          <el-table-column prop="name" label="因素" min-width="140" show-overflow-tooltip />
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column label="当前值" width="100" align="center">
            <template #default="{ row }">
              <span v-if="row.currentValue != null">{{ row.currentValue.toFixed(1) }}</span>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="目标值" width="100" align="center">
            <template #default="{ row }">
              <span v-if="row.targetValue != null">{{ row.targetValue.toFixed(1) }}</span>
              <span v-else>-</span>
            </template>
          </el-table-column>
          <el-table-column label="影响" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="row.impact > 15 ? 'danger' : row.impact > 5 ? 'warning' : 'info'" size="small" effect="dark">
                +{{ row.impact }}
              </el-tag>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else class="empty-state">
      <el-icon :size="64" color="#c0c4cc"><DataAnalysis /></el-icon>
      <p class="empty-state__text">请选择设备和工序后点击"分析风险"</p>
    </div>

    <!-- Score History -->
    <div v-if="scoreHistory.length > 0" class="section" style="margin-top:16px">
      <div class="section__title">
        <el-icon color="var(--el-color-info)"><TrendCharts /></el-icon>
        <span>评分历史（近 24 小时）</span>
      </div>
      <el-table :data="scoreHistory" stripe v-loading="historyLoading" max-height="300">
        <el-table-column label="评分" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="levelTag(row.level)" size="small" effect="dark">{{ row.score }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="等级" width="80" align="center">
          <template #default="{ row }">
            <el-tag :type="levelTag(row.level)" size="small" effect="dark">{{ levelLabel(row.level) }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="趋势" width="80" align="center">
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
.page-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* ─── Page Header ──────────────────── */
.page-header {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 0 4px;
}

.page-header__icon-wrapper {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: var(--el-color-warning-light-9, #fdf6ec);
  color: var(--el-color-warning, #e6a23c);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.page-header__info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.page-header__title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  line-height: 1.3;
}

.page-header__subtitle {
  margin: 0;
  font-size: 13px;
  color: var(--text-secondary, #909399);
  line-height: 1.4;
}

/* ─── Action Bar ───────────────────── */
.action-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.action-bar__left {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

/* ─── Risk Card ────────────────────── */
.risk-card {
  background: var(--bg-card, #fff);
  border-radius: 8px;
  padding: 24px;
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
}

/* ─── Stat Cards ───────────────────── */
.stat-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  background: var(--el-fill-color-blank, #fff);
  border: 1px solid var(--el-border-color-lighter, #ebeef5);
  border-radius: 8px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 14px;
  transition: box-shadow 0.2s;
}

.stat-card:hover {
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
}

.stat-card--score {
  border-color: var(--el-border-color-light, #dcdfe6);
}

.stat-card__icon {
  width: 44px;
  height: 44px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-card__content {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-card__number {
  font-size: 22px;
  font-weight: 700;
  color: var(--text-primary, #303133);
  line-height: 1.2;
}

.stat-card__label {
  font-size: 12px;
  color: var(--el-text-secondary, #909399);
}

.stat-card__time {
  font-size: 12px;
  font-weight: 500;
}

.stat-card__trend {
  font-size: 15px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 6px;
}

/* ─── Sections ─────────────────────── */
.section {
  margin-top: 8px;
}

.section__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--text-primary, #303133);
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
}

/* ─── Empty State ──────────────────── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 0;
  gap: 12px;
}

.empty-state__text {
  margin: 0;
  color: var(--el-text-secondary, #909399);
  font-size: 14px;
}
</style>
<script setup lang="ts">
defineOptions({ name: 'Dashboard' })

import { ref, onMounted, computed } from 'vue'
import {
  DataAnalysis, TrendCharts, Warning, CircleCheck,
  Bell, ArrowRight, Box, Monitor, Checked, Service,
  Search, Calendar
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

const formatDate = (date: Date) => {
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    weekday: 'long',
  })
}

const currentTime = ref(new Date())

// Time-based greeting
const greeting = computed(() => {
  const hour = currentTime.value.getHours()
  if (hour < 6) return '夜深了'
  if (hour < 9) return '早上好'
  if (hour < 12) return '上午好'
  if (hour < 14) return '中午好'
  if (hour < 18) return '下午好'
  if (hour < 22) return '晚上好'
  return '夜深了'
})

// Stats data
const stats = ref([
  {
    label: '今日检验数',
    value: 0,
    unit: '',
    icon: DataAnalysis,
    color: '#1677ff',
    trend: '+12%',
    trendType: 'up' as const,
  },
  {
    label: '合格率',
    value: 0,
    unit: '%',
    icon: CircleCheck,
    color: '#52c41a',
    trend: '+2.1%',
    trendType: 'up' as const,
  },
  {
    label: '异常待处理',
    value: 0,
    unit: '',
    icon: Warning,
    color: '#faad14',
    trend: '-3',
    trendType: 'down' as const,
  },
  {
    label: '预警通知',
    value: 0,
    unit: '',
    icon: Bell,
    color: '#ff4d4f',
    trend: '+5',
    trendType: 'up' as const,
  },
])

// Recent activities
const recentActivities = ref([
  { time: '10:32', content: '供应商A来料检验完成，结果合格', type: 'success' },
  { time: '10:15', content: '产线B3巡检发现尺寸偏差，已标记异常', type: 'warning' },
  { time: '09:48', content: '产品P001首件检验通过', type: 'success' },
  { time: '09:30', content: 'SPC控制图检测到C7参数趋势异常', type: 'danger' },
  { time: '09:12', content: '客户投诉CT-2024-003已分配处理人', type: 'info' },
  { time: '09:05', content: '产线A2完成批量生产，等待FQC检验', type: 'info' },
])

// Quick access modules
const moduleCards = ref([
  { name: 'IQC来料检验', path: '/iqc/receipts', icon: Box, desc: '供应商来料登记与检验', color: '#1677ff' },
  { name: 'IPQC过程检验', path: '/ipqc/first-pieces', icon: Monitor, desc: '产线首件与巡检管理', color: '#52c41a' },
  { name: 'FQC/OQC成品检验', path: '/fqc/inspections', icon: Checked, desc: '成品出厂检验与放行', color: '#faad14' },
  { name: 'SPC统计分析', path: '/spc', icon: TrendCharts, desc: '统计过程控制与分析', color: '#8c8c8c' },
  { name: '缺陷与CAPA', path: '/defects', icon: Warning, desc: '缺陷管理与纠正预防', color: '#ff4d4f' },
  { name: '客诉与8D', path: '/complaints/list', icon: Service, desc: '客户投诉与8D报告', color: '#722ed1' },
  { name: '追溯查询', path: '/trace', icon: Search, desc: '批次全流程追溯', color: '#13c2c2' },
  { name: '质量仪表盘', path: '/reports/dashboard', icon: DataAnalysis, desc: '质量数据可视化', color: '#eb2f96' },
])

function getActivityColor(type: string): string {
  const colors: Record<string, string> = {
    success: '#52c41a',
    warning: '#faad14',
    danger: '#ff4d4f',
    info: '#8c8c8c',
  }
  return colors[type] || '#8c8c8c'
}

// Simulate loading data
onMounted(() => {
  const animateValue = (el: HTMLElement, target: number) => {
    let current = 0
    const step = Math.ceil(target / 30)
    const timer = setInterval(() => {
      current += step
      if (current >= target) {
        current = target
        clearInterval(timer)
      }
      el.textContent = current.toString()
    }, 20)
  }

  setTimeout(() => {
    const statValues = document.querySelectorAll('.stat-value-number')
    const targets = [156, 97, 8, 12]
    statValues.forEach((el, i) => {
      if (el && targets[i] !== undefined) {
        animateValue(el as HTMLElement, targets[i])
      }
    })
  }, 300)

  // Update time every minute
  setInterval(() => {
    currentTime.value = new Date()
  }, 60000)
})

function navigateTo(path: string) {
  window.location.href = path
}
</script>

<template>
  <div class="dashboard-page">
    <!-- Decorative gradient bar -->
    <div class="dashboard-gradient-bar"></div>

    <!-- Welcome Section -->
    <div class="dashboard-header">
      <div class="header-left">
        <div class="greeting-group">
          <span class="greeting-emoji">👋</span>
          <h2 class="welcome-title">{{ greeting }}</h2>
        </div>
        <p class="welcome-subtitle">质量概览 · 实时掌握全厂质量动态</p>
      </div>
      <div class="header-right">
        <div class="date-pill">
          <el-icon><Calendar /></el-icon>
          <span>{{ formatDate(currentTime) }}</span>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <el-row :gutter="16" class="stats-row">
      <el-col :xs="12" :sm="6" v-for="stat in stats" :key="stat.label">
        <el-card shadow="never" class="stat-card" :class="'stat-card--' + stat.color.replace('#', '')">
          <div class="stat-content">
            <div class="stat-icon" :style="{ background: stat.color + '15', color: stat.color }">
              <el-icon :size="22">
                <component :is="stat.icon" />
              </el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">
                <span class="stat-value-number">0</span>
                <span class="stat-unit" v-if="stat.unit">{{ stat.unit }}</span>
              </div>
              <div class="stat-label">{{ stat.label }}</div>
            </div>
            <div class="stat-trend" :class="'trend--' + stat.trendType">
              {{ stat.trend }}
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Main Content -->
    <el-row :gutter="16">
      <!-- Recent Activities -->
      <el-col :xs="24" :lg="10">
        <el-card shadow="never" class="activity-card">
          <template #header>
            <div class="card-header">
              <span>最近活动</span>
              <el-link type="primary" :underline="false">查看全部</el-link>
            </div>
          </template>
          <div class="activity-list">
            <div
              v-for="(item, index) in recentActivities"
              :key="index"
              class="activity-item"
            >
              <div class="activity-dot" :style="{ background: getActivityColor(item.type) }"></div>
              <div class="activity-content">
                <div class="activity-text">{{ item.content }}</div>
                <div class="activity-time">{{ item.time }}</div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- Quick Access -->
      <el-col :xs="24" :lg="14">
        <el-card shadow="never" class="modules-card">
          <template #header>
            <div class="card-header">
              <span>快捷入口</span>
            </div>
          </template>
          <div class="modules-grid">
            <div
              v-for="mod in moduleCards"
              :key="mod.name"
              class="module-item"
              @click="navigateTo(mod.path)"
            >
              <div class="module-icon" :style="{ background: mod.color + '15', color: mod.color }">
                <el-icon :size="18">
                  <component :is="mod.icon" />
                </el-icon>
              </div>
              <div class="module-info">
                <div class="module-name">{{ mod.name }}</div>
                <div class="module-desc">{{ mod.desc }}</div>
              </div>
              <el-icon :size="14" color="#bfbfbf"><ArrowRight /></el-icon>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<style scoped>
.dashboard-page {
  padding: 0;
  overflow: hidden;
}

/* Decorative gradient bar */
.dashboard-gradient-bar {
  height: 4px;
  background: linear-gradient(90deg, #1677ff 0%, #52c41a 33%, #faad14 66%, #ff4d4f 100%);
  border-radius: 0 0 8px 8px;
  margin: -20px -20px 20px;
}

/* ═══ Header ═══ */
.dashboard-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-5);
  flex-wrap: wrap;
  gap: var(--space-3);
}

.header-left {
  display: flex;
  align-items: baseline;
  gap: var(--space-3);
}

.greeting-group {
  display: flex;
  align-items: baseline;
  gap: 8px;
}

.greeting-emoji {
  font-size: 24px;
  line-height: 1;
}

.welcome-title {
  font-size: var(--font-3xl);
  font-weight: var(--font-bold);
  color: var(--text-primary, #1a1a1a);
  margin: 0;
  line-height: 1.2;
}

.welcome-subtitle {
  font-size: var(--font-sm);
  color: var(--text-secondary, #8c8c8c);
  margin: var(--space-1) 0 0;
}

.header-right {
  display: flex;
  align-items: center;
}

.date-pill {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: var(--font-sm);
  color: var(--text-regular, #4a4a4a);
  background: var(--el-bg-color);
  padding: 8px 14px;
  border-radius: var(--radius-full);
  border: 1px solid var(--el-border-color-lighter);
  box-shadow: var(--shadow-xs);
}

/* ═══ Stats Cards ═══ */
.stats-row {
  margin-bottom: var(--space-5);
}

.stat-card {
  margin-bottom: 0;
  border-radius: var(--radius-lg);
  border: 1px solid var(--el-border-color-lighter);
  transition: all 0.25s ease;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  border-radius: 4px 0 0 4px;
}

.stat-card--1677ff::before { background: #1677ff; }
.stat-card--52c41a::before { background: #52c41a; }
.stat-card--faad14::before { background: #faad14; }
.stat-card--ff4d4f::before { background: #ff4d4f; }

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
  border-color: transparent;
}

.stat-card :deep(.el-card__body) {
  padding: var(--space-5);
}

.stat-content {
  display: flex;
  align-items: center;
  gap: var(--space-4);
}

.stat-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-info {
  flex: 1;
  min-width: 0;
}

.stat-value {
  font-size: var(--font-3xl);
  font-weight: var(--font-bold);
  color: var(--text-primary, #1a1a1a);
  line-height: 1.2;
  display: flex;
  align-items: baseline;
  gap: 2px;
}

.stat-unit {
  font-size: var(--font-md);
  font-weight: var(--font-medium);
  color: var(--text-secondary, #8c8c8c);
}

.stat-label {
  font-size: var(--font-sm);
  color: var(--text-secondary, #8c8c8c);
  margin-top: 2px;
}

.stat-trend {
  font-size: var(--font-xs);
  font-weight: var(--font-medium);
  padding: 2px 8px;
  border-radius: var(--radius-full);
  white-space: nowrap;
}

.trend--up {
  color: #52c41a;
  background: #f6ffed;
}

.trend--down {
  color: #ff4d4f;
  background: #fff2f0;
}

/* ═══ Activity Card ═══ */
.activity-card,
.modules-card {
  margin-bottom: 0;
  border-radius: var(--radius-lg);
  border: 1px solid var(--el-border-color-lighter);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: var(--font-semibold);
}

.activity-list {
  max-height: 320px;
  overflow-y: auto;
}

.activity-item {
  display: flex;
  gap: var(--space-3);
  padding: var(--space-3) 0;
  border-bottom: 1px solid var(--el-border-color-lighter);
  position: relative;
}

.activity-item:last-child {
  border-bottom: none;
}

.activity-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  margin-top: 6px;
  flex-shrink: 0;
  box-shadow: 0 0 0 3px rgba(255, 255, 255, 0.8);
}

.activity-content {
  flex: 1;
  min-width: 0;
}

.activity-text {
  font-size: var(--font-sm);
  color: var(--text-regular, #4a4a4a);
  line-height: 1.5;
}

.activity-time {
  font-size: 11px;
  color: var(--text-secondary, #8c8c8c);
  margin-top: 3px;
}

/* ═══ Module Cards ═══ */
.modules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: var(--space-3);
}

.module-item {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  border: 1px solid var(--el-border-color-lighter);
  cursor: pointer;
  transition: all 0.25s ease;
}

.module-item:hover {
  border-color: transparent;
  background: var(--primary-bg, #f0f7ff);
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.module-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.module-info {
  flex: 1;
  min-width: 0;
}

.module-name {
  font-size: var(--font-sm);
  font-weight: var(--font-medium);
  color: var(--text-primary, #1a1a1a);
}

.module-desc {
  font-size: 11px;
  color: var(--text-secondary, #8c8c8c);
  margin-top: 1px;
}

/* ═══ Dark mode ═══ */
html.dark .trend--up,
html.dark .trend--down {
  background: rgba(82, 196, 26, 0.15);
}

html.dark .stat-card {
  border-color: var(--el-border-color);
}

html.dark .activity-item {
  border-color: var(--el-border-color-light);
}

html.dark .module-item {
  border-color: var(--el-border-color-light);
}

html.dark .module-item:hover {
  background: rgba(22, 119, 255, 0.1);
}

html.dark .date-pill {
  background: var(--el-bg-color);
  color: var(--text-regular, #d4d4d9);
  border-color: var(--el-border-color);
}
</style>

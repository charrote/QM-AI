<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Bell, WarningFilled, InfoFilled } from '@element-plus/icons-vue'
import { aiApi } from '@/api/ai'
import { ALERT_LEVEL_OPTIONS, ALERT_LEVEL_MAP } from '@/types/ai'
import type { PagedResult } from '@/types/basicData'

defineOptions({ name: 'AlertCenterPage' })

const searchLevel = ref('')
const resolvedFilter = ref<boolean | undefined>(undefined)
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const alerts = ref<Array<{
  id: number
  level: string
  title: string
  description: string
  source: string
  resolved: boolean
  createdAt: string
  resolvedAt?: string
}>>([])

const stats = reactive({
  totalAlerts: 0,
  unresolvedAlerts: 0,
  byLevel: {} as Record<string, number>,
  bySource: {} as Record<string, number>,
})

async function loadAlerts() {
  try {
    const res: PagedResult<any> = await aiApi.alerts({
      page: page.value,
      pageSize: pageSize.value,
      level: searchLevel.value || undefined,
      resolved: resolvedFilter.value,
    })
    alerts.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('[AlertCenterPage] Failed to load alerts:', e)
  }
}

async function loadStats() {
  try {
    const res = await aiApi.alertStats()
    Object.assign(stats, res)
  } catch (e) {
    console.error('[AlertCenterPage] Failed to load stats:', e)
  }
}

async function resolveAlert(row: any) {
  try {
    await aiApi.resolveAlert(row.id)
    ElMessage.success('已标记为已解决')
    await Promise.all([loadAlerts(), loadStats()])
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

const urgentAlerts = computed(() => alerts.value.filter(a => a.level === 'critical' && !a.resolved))

function getLevelColor(level: string): string {
  const colors: Record<string, string> = {
    critical: '#dc2626', high: '#ea580c', medium: '#2563eb', low: '#6b7280',
  }
  return colors[level] || '#6b7280'
}

function getLevelLabel(level: string): string {
  return ALERT_LEVEL_MAP[level] || level
}

function getLevelIcon(level: string) {
  const icons: Record<string, any> = { critical: WarningFilled, high: WarningFilled, medium: InfoFilled, low: InfoFilled }
  return icons[level] || InfoFilled
}

onMounted(async () => {
  await Promise.all([loadAlerts(), loadStats()])
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="ai-header-banner">
      <div class="ai-header-banner-main">
        <div class="ai-header-banner-icon">
          <el-icon :size="24"><Bell /></el-icon>
        </div>
        <div class="ai-header-banner-text">
          <div class="ai-header-banner-title">预警中心</div>
          <div class="ai-header-banner-subtitle">AI 驱动的实时质量预警与告警管理</div>
        </div>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="ai-stat-grid">
      <div class="ai-stat-card ai-stat-card--blue">
        <div class="ai-stat-icon ai-stat-icon--blue">
          <el-icon :size="22"><Bell /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">预警总数</div>
          <div class="ai-stat-value">{{ stats.totalAlerts }}</div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--red">
        <div class="ai-stat-icon ai-stat-icon--red">
          <el-icon :size="22"><WarningFilled /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">未解决</div>
          <div class="ai-stat-value ai-stat-value--danger">{{ stats.unresolvedAlerts }}</div>
        </div>
      </div>
      <div v-for="item in ALERT_LEVEL_OPTIONS" :key="item.value"
           :class="['ai-stat-card', `ai-stat-card--${item.value === 'critical' ? 'red' : item.value === 'high' ? 'orange' : item.value === 'medium' ? 'blue' : 'gray'}`]">
        <div :class="['ai-stat-icon', `ai-stat-icon--${item.value === 'critical' ? 'red' : item.value === 'high' ? 'orange' : item.value === 'medium' ? 'blue' : 'gray'}`]">
          <el-icon :size="22"><component :is="getLevelIcon(item.value)" /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">{{ item.label }}</div>
          <div class="ai-stat-value" :class="`ai-stat-value--${item.value === 'critical' ? 'danger' : item.value === 'high' ? 'warning' : item.value === 'medium' ? 'primary' : ''}`">
            {{ stats.byLevel[item.value] || 0 }}
          </div>
        </div>
      </div>
    </div>

    <!-- Urgent Alerts -->
    <div v-if="urgentAlerts.length > 0" class="section">
      <div class="section__header">
        <div class="section__title">需要立即关注</div>
        <el-tag type="danger" size="small" effect="dark">{{ urgentAlerts.length }} 条 {{ getLevelLabel('critical') }}预警</el-tag>
      </div>
      <el-row :gutter="12">
        <el-col :span="8" v-for="alert in urgentAlerts" :key="alert.id">
          <div class="alert-card alert-card--critical" style="margin-bottom: 12px;">
            <div class="alert-card__header">
              <el-tag type="danger" size="small" effect="dark">{{ getLevelLabel(alert.level) }}</el-tag>
              <el-tag :type="alert.resolved ? 'success' : 'warning'" size="small" effect="plain">
                {{ alert.resolved ? '已解决' : '未解决' }}
              </el-tag>
            </div>
            <p class="alert-card__title">{{ alert.title }}</p>
            <p class="alert-card__desc">{{ alert.description }}</p>
            <div class="alert-card__footer">
              <span>{{ formatDate(alert.createdAt) }}</span>
              <el-button v-if="!alert.resolved" type="primary" size="small" @click="resolveAlert(alert)">标记已解决</el-button>
            </div>
          </div>
        </el-col>
      </el-row>
    </div>

    <!-- Toolbar -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
        <el-select v-model="searchLevel" placeholder="预警级别" clearable style="width: 130px" @change="loadAlerts">
          <el-option v-for="opt in ALERT_LEVEL_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-select v-model="resolvedFilter" placeholder="已解决" clearable style="width: 130px" @change="loadAlerts">
          <el-option label="未解决" :value="false" />
          <el-option label="已解决" :value="true" />
        </el-select>
        <div class="ai-filter-actions">
          <el-button type="primary" @click="loadAlerts">查询</el-button>
          <el-button @click="loadAlerts">刷新</el-button>
        </div>
      </div>
    </div>

    <!-- Table -->
    <div class="table-card">
      <div class="table-card__body">
        <el-table :data="alerts" stripe style="width: 100%" :row-class-name="({ row }) => {
          if (row.level === 'critical' && !row.resolved) return 'alert-row-critical'
          if (row.level === 'high' && !row.resolved) return 'alert-row-high'
          return ''
        }">
          <el-table-column label="级别" width="90" align="center">
            <template #default="{ row }">
              <el-tag :type="ALERT_LEVEL_OPTIONS.find(o => o.value === row.level)?.type || 'info'" size="small" effect="dark">
                {{ ALERT_LEVEL_MAP[row.level] || row.level }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="title" label="标题" min-width="200" show-overflow-tooltip />
          <el-table-column prop="source" label="来源" width="120" show-overflow-tooltip />
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column label="状态" width="85" align="center">
            <template #default="{ row }">
              <el-tag :type="row.resolved ? 'success' : 'danger'" size="small" effect="dark">
                {{ row.resolved ? '已解决' : '未解决' }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="创建时间" width="160" align="center">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="110" align="center" fixed="right">
            <template #default="{ row }">
              <el-button v-if="!row.resolved" link size="small" type="success" @click="resolveAlert(row)">标记已解决</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
      <div class="table-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadAlerts"
          @current-change="loadAlerts"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.alert-row-critical { background-color: rgba(220, 38, 38, 0.04); }
.alert-row-high { background-color: rgba(234, 88, 12, 0.04); }
</style>

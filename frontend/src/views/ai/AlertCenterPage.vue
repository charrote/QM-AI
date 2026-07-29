<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Bell } from '@element-plus/icons-vue'
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
    console.error('Failed to load alerts', e)
  }
}

async function loadStats() {
  try {
    const res = await aiApi.alertStats()
    Object.assign(stats, res)
  } catch (e) {
    console.error('Failed to load stats', e)
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
    critical: '#f56c6c', high: '#e6a23c', medium: '#409eff', low: '#909399',
  }
  return colors[level] || '#909399'
}

function getLevelLabel(level: string): string {
  return ALERT_LEVEL_MAP[level] || level
}

function alertRowClass({ row }: { row: any }) {
  if (row.level === 'critical' && !row.resolved) return 'alert-row-critical'
  if (row.level === 'high' && !row.resolved) return 'alert-row-high'
  return ''
}

onMounted(async () => {
  await Promise.all([loadAlerts(), loadStats()])
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Bell /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>预警中心</h2>
          <p>AI 驱动的实时质量预警与告警管理</p>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <el-row :gutter="12" class="stats-row">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.totalAlerts }}</div>
          <div class="stat-label">预警总数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card stat-unresolved">
          <div class="stat-value stat-value-danger">{{ stats.unresolvedAlerts }}</div>
          <div class="stat-label">未解决</div>
        </el-card>
      </el-col>
      <el-col :span="6" v-for="item in ALERT_LEVEL_OPTIONS" :key="item.value">
        <el-card shadow="hover" class="stat-card stat-level" :class="`stat-level-${item.value}`">
          <div class="stat-value" :style="{ color: getLevelColor(item.value) }">{{ stats.byLevel[item.value] || 0 }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Priority Alert Cards -->
    <div v-if="urgentAlerts.length > 0" class="urgent-alerts">
      <el-alert title="需要立即关注" type="error" :closable="false" show-icon style="margin-bottom: 16px">
        <template #default>
          <div class="urgent-count">{{ urgentAlerts.length }} 条 {{ getLevelLabel('critical') }}预警</div>
        </template>
      </el-alert>
      <el-row :gutter="12">
        <el-col :span="8" v-for="alert in urgentAlerts" :key="alert.id">
          <el-card shadow="hover" class="alert-card alert-card-critical">
            <div class="alert-card-header">
              <el-tag type="danger" size="small" effect="dark">{{ getLevelLabel(alert.level) }}</el-tag>
              <el-tag :type="alert.resolved ? 'success' : 'warning'" size="small">{{ alert.resolved ? '已解决' : '未解决' }}</el-tag>
            </div>
            <p class="alert-card-title">{{ alert.title }}</p>
            <p class="alert-card-desc">{{ alert.description }}</p>
            <div class="alert-card-footer">
              <span>{{ formatDate(alert.createdAt) }}</span>
              <el-button v-if="!alert.resolved" type="primary" size="small" @click="resolveAlert(alert)">标记已解决</el-button>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>

    <!-- Toolbar -->
    <el-card shadow="never" class="toolbar-card">
      <div class="toolbar-row">
        <el-select v-model="searchLevel" placeholder="预警级别" clearable style="width: 130px" @change="loadAlerts">
          <el-option v-for="opt in ALERT_LEVEL_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-select v-model="resolvedFilter" placeholder="已解决" clearable style="width: 130px" @change="loadAlerts">
          <el-option label="未解决" :value="false" />
          <el-option label="已解决" :value="true" />
        </el-select>
        <el-button type="primary" @click="loadAlerts">查询</el-button>
        <el-button @click="loadAlerts">刷新</el-button>
      </div>
    </el-card>

    <!-- Table -->
    <el-card shadow="never" class="table-card">
      <el-table :data="alerts" stripe style="width: 100%" :row-class-name="alertRowClass">
        <el-table-column label="级别" width="90">
          <template #default="{ row }">
            <el-tag :type="ALERT_LEVEL_OPTIONS.find(o => o.value === row.level)?.type || 'info'" size="small" effect="dark">
              {{ ALERT_LEVEL_MAP[row.level] || row.level }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="title" label="标题" min-width="200" show-overflow-tooltip />
        <el-table-column prop="source" label="来源" width="120" show-overflow-tooltip />
        <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
        <el-table-column prop="resolved" label="状态" width="85">
          <template #default="{ row }">
            <el-tag :type="row.resolved ? 'success' : 'danger'" size="small" effect="dark">
              {{ row.resolved ? '已解决' : '未解决' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="165">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="110" fixed="right">
          <template #default="{ row }">
            <el-button v-if="!row.resolved" link size="small" type="success" @click="resolveAlert(row)">标记已解决</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-row">
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
    </el-card>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.page-header { margin-bottom: 16px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; background: #fef0f0; border-radius: 10px; }
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.stats-row { margin-bottom: 12px; }
.stat-card { text-align: center; border-radius: 8px; transition: all 0.2s; }
.stat-card:hover { transform: translateY(-2px); }
.stat-value { font-size: 28px; font-weight: 700; }
.stat-value-danger { color: #f56c6c; }
.stat-label { font-size: 13px; color: #909399; margin-top: 4px; }
.stat-unresolved { border-left: 3px solid #f56c6c; }
.stat-level-critical { border-left: 3px solid #f56c6c; }
.stat-level-high { border-left: 3px solid #e6a23c; }
.stat-level-medium { border-left: 3px solid #409eff; }
.stat-level-low { border-left: 3px solid #909399; }
.urgent-alerts { margin-bottom: 16px; }
.urgent-count { font-weight: 600; margin-top: 4px; }
.alert-card { border-radius: 8px; }
.alert-card-critical { border-top: 3px solid #f56c6c; }
.alert-card-header { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
.alert-card-title { margin: 0 0 4px; font-size: 14px; font-weight: 600; color: #303133; }
.alert-card-desc { margin: 0 0 8px; font-size: 12px; color: #909399; line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
.alert-card-footer { display: flex; justify-content: space-between; align-items: center; font-size: 12px; color: #909399; }
.toolbar-card { margin-bottom: 12px; }
.toolbar-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.table-card { flex: 1; display: flex; flex-direction: column; }
.table-card :deep(.el-card__body) { flex: 1; display: flex; flex-direction: column; padding: 0; }
.table-card :deep(.el-table) { flex: 1; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 8px; border-top: 1px solid #f0f0f0; }
.alert-row-critical { background-color: #fef0f0; }
.alert-row-high { background-color: #fdf6ec; }
</style>

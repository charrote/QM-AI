<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
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

onMounted(async () => {
  await Promise.all([loadAlerts(), loadStats()])
})
</script>

<template>
  <div class="page-container">
    <!-- Stats Cards -->
    <el-row :gutter="16" class="stats-row">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ stats.totalAlerts }}</div>
          <div class="stat-label">预警总数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card" style="border-left: 3px solid #f56c6c;">
          <div class="stat-value" style="color: #f56c6c;">{{ stats.unresolvedAlerts }}</div>
          <div class="stat-label">未解决</div>
        </el-card>
      </el-col>
      <el-col :span="6" v-for="item in ALERT_LEVEL_OPTIONS" :key="item.value">
        <el-card shadow="hover" class="stat-card" :class="`stat-level-${item.value}`">
          <div class="stat-value">{{ stats.byLevel[item.value] || 0 }}</div>
          <div class="stat-label">{{ item.label }}</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Toolbar -->
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

    <!-- Table -->
    <el-table :data="alerts" stripe style="width: 100%" >
      <el-table-column label="级别" width="80">
        <template #default="{ row }">
          <el-tag :type="ALERT_LEVEL_OPTIONS.find(o => o.value === row.level)?.type || 'info'" size="small" effect="plain">
            {{ ALERT_LEVEL_MAP[row.level] || row.level }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="title" label="标题" min-width="200" show-overflow-tooltip />
      <el-table-column prop="source" label="来源" width="120" show-overflow-tooltip />
      <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
      <el-table-column prop="resolved" label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.resolved ? 'success' : 'danger'" size="small" effect="plain">
            {{ row.resolved ? '已解决' : '未解决' }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="创建时间" width="160">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
      <el-table-column label="操作" width="100" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="!row.resolved"
            link size="small" type="success"
            @click="resolveAlert(row)"
          >标记已解决</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- Pagination -->
    <div class="pagination-row">
      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next"
        size="small"
        @current-change="loadAlerts"
      />
    </div>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin: 12px 0; flex-wrap: wrap; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 0; }
.stats-row { margin-bottom: 8px; }
.stat-card { text-align: center; }
.stat-value { font-size: 28px; font-weight: 700; color: var(--el-text-color-primary); }
.stat-label { font-size: 13px; color: var(--el-text-color-secondary); margin-top: 4px; }
</style>

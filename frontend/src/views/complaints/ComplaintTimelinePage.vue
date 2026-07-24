<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { List, Back, Clock } from '@element-plus/icons-vue'
import { complaintApi } from '@/api/complaint'
import { EVENT_TYPE_MAP } from '@/types/complaint'
import type { ComplaintEvent } from '@/types/complaint'

defineOptions({ name: 'ComplaintTimelinePage' })

const router = useRouter()
const route = useRoute()
const events = ref<ComplaintEvent[]>([])
const complaintId = ref<number>(Number(route.query.complaintId) || 0)
const loading = ref(false)

const eventTypeColors: Record<string, string> = {
  created: 'primary',
  acknowledged: 'success',
  status_change: 'warning',
  d8_update: 'info',
  verify: '',
  close: '',
}

function formatDate(d: string) {
  return new Date(d).toLocaleString('zh-CN')
}

function getEventTypeLabel(type: string) {
  return EVENT_TYPE_MAP[type] || type
}

async function loadEvents() {
  if (!complaintId.value) return
  loading.value = true
  try {
    events.value = await complaintApi.timeline(complaintId.value)
    events.value.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  } catch (e) {
    ElMessage.error('加载事件失败')
  } finally {
    loading.value = false
  }
}

function goToList() {
  router.push({ name: 'ComplaintList' })
}

onMounted(() => {
  if (!complaintId.value && route.query.complaintId) {
    complaintId.value = Number(route.query.complaintId)
  }
  loadEvents()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header__main">
        <el-icon class="page-header__icon" :size="28"><List /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">投诉时间线</h2>
          <p class="page-header__subtitle">客户投诉处理全过程追踪</p>
        </div>
      </div>
      <div class="page-header__actions">
        <el-button :icon="Back" @click="goToList">返回列表</el-button>
      </div>
    </div>

    <!-- Timeline Card -->
    <el-card v-loading="loading" shadow="never" class="timeline-card">
      <template v-if="events.length === 0 && !loading">
        <div class="empty-state">
          <el-icon :size="48" color="#909399"><Clock /></el-icon>
          <p>暂无事件记录</p>
        </div>
      </template>

      <el-timeline v-else>
        <el-timeline-item
          v-for="event in events"
          :key="event.id"
          :timestamp="formatDate(event.createdAt)"
          :type="eventTypeColors[event.eventType] || 'info'"
          :size="event.eventType === 'created' ? 'large' : 'normal'"
          :placement="event.eventType === 'created' ? 'top' : 'bottom'"
        >
          <el-card shadow="never" class="event-card">
            <div class="event-header">
              <el-tag :type="eventTypeColors[event.eventType] || 'info'" size="small" effect="plain">
                {{ getEventTypeLabel(event.eventType) }}
              </el-tag>
              <span class="event-user">操作人: {{ event.createdBy }}</span>
            </div>
            <div class="event-body" v-if="event.eventData">
              {{ event.eventData }}
            </div>
          </el-card>
        </el-timeline-item>
      </el-timeline>
    </el-card>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; gap: 16px; }

/* Page Header */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: var(--el-bg-color);
  border-radius: var(--radius-lg, 8px);
  padding: 16px 20px;
  border: 1px solid var(--el-border-color-lighter);
}
.page-header__main { display: flex; align-items: center; gap: 12px; }
.page-header__icon { color: var(--el-color-primary); flex-shrink: 0; }
.page-header__title { margin: 0; font-size: 20px; font-weight: 600; color: var(--el-text-color-primary); line-height: 1.2; }
.page-header__subtitle { margin: 4px 0 0; font-size: 13px; color: var(--el-text-color-secondary); }
.page-header__actions { display: flex; gap: 8px; }

/* Timeline */
.timeline-card { margin: 0; }
.empty-state { text-align: center; padding: 60px 0; color: var(--el-text-color-secondary); }
.event-card { margin-bottom: 0; }
.event-header { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
.event-user { font-size: 12px; color: var(--el-text-color-secondary); }
.event-body { font-size: 13px; line-height: 1.6; white-space: pre-wrap; color: var(--el-text-color-regular); }
</style>
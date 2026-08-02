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
  <div class="qmc-container">
    <!-- Page Header Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Clock /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">投诉时间线</h2>
          <span class="page-header-banner-subtitle">客户投诉处理全过程事件追踪</span>
        </div>
      </div>
    </div>

    <!-- Timeline Card -->
    <div class="data-card">
      <div class="data-card__header">
        <span class="data-card__title">
          <el-icon style="color: var(--primary)"><Clock /></el-icon>
          事件记录
          <el-tag v-if="events.length" type="info" size="small">{{ events.length }} 条</el-tag>
        </span>
        <div class="data-card__toolbar">
          <el-button size="small" :icon="Back" @click="goToList">返回列表</el-button>
        </div>
      </div>
      <div class="data-card__body">
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
      </div>
    </div>
  </div>
</template>

<style scoped>
.qmc-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.content-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 8px 8px 0;
  overflow-y: auto;
}
.empty-state { text-align: center; padding: 60px 0; color: var(--el-text-color-secondary); }
.event-card { margin-bottom: 0; }
.event-header { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
.event-user { font-size: 12px; color: var(--el-text-color-secondary); }
.event-body { font-size: 13px; line-height: 1.6; white-space: pre-wrap; color: var(--el-text-color-regular); }
</style>
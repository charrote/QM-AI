<template>
  <div
    class="route-step-card"
    :class="{
      'route-step-card--dragging': isDragging,
      'route-step-card--drag-over': isDragOver,
      'route-step-card--placeholder': isPlaceholder,
    }"
    :draggable="draggable && !isPlaceholder"
    @dragstart="handleDragStart"
    @dragend="handleDragEnd"
    @dragover="handleDragOver"
    @dragleave="handleDragLeave"
    @drop="handleDrop"
    @dblclick.stop="emit('dblclick', step)"
  >
    <!-- 占位卡片 -->
    <template v-if="isPlaceholder">
      <div class="route-step-card__placeholder-text">
        <el-icon><Plus /></el-icon>
        <span>插入到此</span>
      </div>
    </template>

    <!-- 步骤卡片 -->
    <template v-else>
      <!-- 顶部：步骤序号 + 删除按钮 -->
      <div class="route-step-card__top">
        <span class="route-step-card__step-num">步骤 {{ step.stepOrder }}</span>
        <el-popconfirm
          title="确认删除此步骤？"
          confirm-button-text="删除"
          cancel-button-text="取消"
          @confirm="emit('delete', step.id)"
        >
          <template #reference>
            <el-button size="small" text type="danger" class="route-step-card__delete-btn">
              <el-icon><Delete /></el-icon>
            </el-button>
          </template>
        </el-popconfirm>
      </div>

      <!-- 中间：工序名称 -->
      <div class="route-step-card__body">
        <div class="route-step-card__process-name">{{ step.processName }}</div>
        <div v-if="step.processCode" class="route-step-card__process-code">{{ step.processCode }}</div>
      </div>

      <!-- 底部：时间信息 -->
      <div class="route-step-card__time">
        <div v-if="step.preWaitTimeMinutes" class="route-step-card__time-item">
          <span class="route-step-card__time-label">前置</span>
          <span class="route-step-card__time-value">{{ step.preWaitTimeMinutes }}min</span>
        </div>
        <div class="route-step-card__time-item">
          <span class="route-step-card__time-label">标准</span>
          <span class="route-step-card__time-value">{{ formatTime(step.standardTimeMinutes) }}</span>
        </div>
        <div v-if="step.postWaitTimeMinutes" class="route-step-card__time-item">
          <span class="route-step-card__time-label">后置</span>
          <span class="route-step-card__time-value">{{ step.postWaitTimeMinutes }}min</span>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ProductRouteStepDto } from '@/types/routing'
import { Delete, Plus } from '@element-plus/icons-vue'

defineOptions({ name: 'RouteStepCard' })

const props = defineProps<{
  step: ProductRouteStepDto & { _isPlaceholder?: boolean }
  draggable?: boolean
  isDragging?: boolean
  isDragOver?: boolean
  routingHeaderId?: number
}>()

const emit = defineEmits<{
  delete: [id: number]
  dragStart: [e: DragEvent, id: number]
  dragEnd: [e: DragEvent]
  dragOver: [e: DragEvent, id: number]
  dragLeave: [e: DragEvent]
  drop: [e: DragEvent, id: number]
  dblclick: [step: ProductRouteStepDto]
}>()

const isPlaceholder = computed(() => props.step._isPlaceholder === true)

function handleDragStart(e: DragEvent) {
  if (!props.draggable) return
  // 设置拖拽数据
  e.dataTransfer?.setData('text/plain', String(props.step.id))
  e.dataTransfer!.effectAllowed = 'move'
  // 拖拽时轻微透明效果
  setTimeout(() => {
    emit('dragStart', e, props.step.id)
  }, 0)
}

function handleDragEnd(e: DragEvent) {
  emit('dragEnd', e)
}

function handleDragOver(e: DragEvent) {
  if (!props.draggable) return
  e.preventDefault()
  e.dataTransfer!.dropEffect = 'move'
  // 不阻止默认行为，让浏览器正常处理
  emit('dragOver', e, props.step.id)
}

function handleDragLeave(e: DragEvent) {
  emit('dragLeave', e)
}

function handleDrop(e: DragEvent) {
  if (!props.draggable) return
  e.preventDefault()
  e.stopPropagation()
  emit('drop', e, props.step.id)
}

function formatTime(minutes?: number): string {
  if (minutes == null) return '—'
  if (minutes < 60) return `${minutes} min`
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  return m > 0 ? `${h}h ${m}min` : `${h}h`
}
</script>

<style scoped>
.route-step-card {
  display: flex;
  flex-direction: column;
  width: 180px;
  min-height: 140px;
  background: var(--el-bg-color);
  border: 1.5px solid var(--el-border-color-light);
  border-radius: 10px;
  padding: 12px;
  cursor: grab;
  transition: all 0.2s ease;
  flex-shrink: 0;
  position: relative;
  overflow: visible;
  user-select: none;
  -webkit-user-select: none;
}

.route-step-card:hover {
  border-color: var(--el-color-primary);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  transform: translateY(-2px);
}

.route-step-card--dragging {
  transform: scale(0.98);
  cursor: grabbing;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  opacity: 1 !important;
  visibility: visible !important;
}

.route-step-card--drag-over {
  position: relative;
}

/* 插入指示器 */
.route-step-card--drag-over.insert-top::before {
  content: '';
  position: absolute;
  left: -24px;
  top: 10%;
  width: 4px;
  height: 80%;
  background: var(--el-color-primary);
  border-radius: 2px;
  z-index: 100;
  box-shadow: 0 0 8px rgba(64, 158, 255, 0.6);
  animation: insertPulse 1s ease-in-out infinite;
}

.route-step-card--drag-over.insert-bottom::after {
  content: '';
  position: absolute;
  right: -24px;
  top: 10%;
  width: 4px;
  height: 80%;
  background: var(--el-color-primary);
  border-radius: 2px;
  z-index: 100;
  box-shadow: 0 0 8px rgba(64, 158, 255, 0.6);
  animation: insertPulse 1s ease-in-out infinite;
}

@keyframes insertPulse {
  0%, 100% { opacity: 0.9; }
  50% { opacity: 0.4; }
}

/* 占位卡片 */
.route-step-card--placeholder {
  background: var(--el-fill-color-light) !important;
  border: 2px dashed var(--el-border-color-dark) !important;
  opacity: 0.6;
  cursor: default;
}

.route-step-card__placeholder-text {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  height: 100%;
  min-height: 80px;
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

.route-step-card__placeholder-text .el-icon {
  font-size: 24px;
  color: var(--el-text-color-placeholder);
}

/* 顶部：步骤序号 + 删除按钮 */
.route-step-card__top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}

.route-step-card__step-num {
  font-size: 11px;
  font-weight: 600;
  color: var(--el-color-primary);
  background: var(--el-color-primary-light-9);
  padding: 2px 10px;
  border-radius: 10px;
}

.route-step-card__delete-btn {
  padding: 4px;
  opacity: 1;
  transition: opacity 0.2s;
}

.route-step-card:hover .route-step-card__delete-btn {
  opacity: 1;
}

/* 中间：工序名称 */
.route-step-card__body {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  margin-bottom: 12px;
  min-height: 0;
}

.route-step-card__process-name {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  margin-bottom: 4px;
}

.route-step-card__process-code {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 底部：时间信息 */
.route-step-card__time {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  padding-top: 10px;
  border-top: 1px solid var(--el-border-color-lighter);
}

.route-step-card__time-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  flex: 1;
}

.route-step-card__time-label {
  font-size: 10px;
  color: var(--el-text-color-placeholder);
  line-height: 1;
}

.route-step-card__time-value {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
  line-height: 1.2;
}

/* 拖拽容器中的插入位置指示 */
.flow-container {
  position: relative;
}

.flow-container .insert-indicator {
  position: absolute;
  height: 3px;
  background: var(--el-color-primary);
  border-radius: 2px;
  z-index: 20;
  pointer-events: none;
  box-shadow: 0 0 8px rgba(64, 158, 255, 0.6);
  animation: pulse 1s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.6; }
}
</style>
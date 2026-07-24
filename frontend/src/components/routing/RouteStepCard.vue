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
    @dblclick="emit('edit')"
  >
    <!-- 拖拽手柄 -->
    <div class="route-step-card__handle" @mousedown.prevent>
      <el-icon class="handle-icon"><Rank /></el-icon>
    </div>

    <!-- 步骤信息 -->
    <div class="route-step-card__body">
      <template v-if="isPlaceholder">
        <div class="route-step-card__placeholder-text">
          <el-icon><Plus /></el-icon>
          <span>插入到此</span>
        </div>
      </template>
      <template v-else>
        <div class="route-step-card__header">
          <span class="route-step-card__step-num">步骤 {{ step.stepOrder }}</span>
          <el-icon v-if="draggable" class="route-step-card__drag-hint"><Operation /></el-icon>
        </div>
        <div class="route-step-card__process-name">{{ step.processName }}</div>
        <div class="route-step-card__process-code">{{ step.processCode }}</div>
        <div class="route-step-card__meta">
          <el-icon><Timer /></el-icon>
          <span>{{ formatTime(step.standardTimeMinutes) }}</span>
        </div>
      </template>
    </div>

    <!-- 操作按钮 -->
    <div class="route-step-card__actions">
      <el-button size="small" text @click.stop="emit('edit')">
        <el-icon><EditPen /></el-icon>
      </el-button>
      <el-popconfirm
        title="确认删除此步骤？"
        confirm-button-text="删除"
        cancel-button-text="取消"
        @confirm="emit('delete', step.id)"
      >
        <template #reference>
          <el-button size="small" text type="danger" @click.stop>
            <el-icon><Delete /></el-icon>
          </el-button>
        </template>
      </el-popconfirm>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ProductRouteStepDto } from '@/types/routing'
import { Rank, Operation, Timer, EditPen, Delete, Plus } from '@element-plus/icons-vue'

defineOptions({ name: 'RouteStepCard' })

const props = defineProps<{
  step: ProductRouteStepDto & { _isPlaceholder?: boolean }
  draggable?: boolean
  isDragging?: boolean
  isDragOver?: boolean
  routingHeaderId?: number
}>()

const emit = defineEmits<{
  edit: []
  delete: [id: number]
  dragStart: [e: DragEvent, id: number]
  dragEnd: [e: DragEvent]
  dragOver: [e: DragEvent, id: number]
  dragLeave: [e: DragEvent]
  drop: [e: DragEvent, id: number]
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
  align-items: center;
  width: 180px;
  min-height: 120px;
  background: var(--el-bg-color);
  border: 1.5px solid var(--el-border-color-light);
  border-radius: 10px;
  padding: 0;
  cursor: grab;
  transition: all 0.2s ease;
  flex-shrink: 0;
  position: relative;
  overflow: visible;
  margin-right: 0;
  /* 防止拖拽时选中文字 */
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
  /* 拖拽时保持原样，不改变颜色 */
  opacity: 1 !important;
  visibility: visible !important;
}

.route-step-card--drag-over {
  position: relative;
}

/* 插入指示器：完全在卡片之间的空隙中，不触及卡片 */
/* 插入到卡片左侧（之前）→ 指示器在左侧空隙中 */
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

/* 插入到卡片右侧（之后）→ 指示器在右侧空隙中 */
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

/* ─── 占位卡片（拖拽实时预览）──────────────────────────────── */
.route-step-card--placeholder {
  background: var(--el-fill-color-light) !important;
  border: 2px dashed var(--el-border-color-dark) !important;
  opacity: 0.6;
  cursor: default;
}

.route-step-card--placeholder .route-step-card__handle {
  opacity: 0 !important;
}

.route-step-card--placeholder .route-step-card__actions {
  opacity: 0 !important;
}

.route-step-card--placeholder .route-step-card__process-name {
  color: var(--el-text-color-secondary);
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

/* 拖拽容器中的插入位置指示 */
.flow-container {
  position: relative;
}

/* 插入位置虚线指示器 */
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

.route-step-card__handle {
  width: 32px;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-fill-color-light);
  border-right: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
  opacity: 0;
  transition: opacity 0.2s;
  cursor: grab;
}

.route-step-card:hover .route-step-card__handle {
  opacity: 1;
}

.handle-icon {
  color: var(--el-text-color-placeholder);
  font-size: 14px;
}

.route-step-card__body {
  flex: 1;
  padding: 12px 14px;
  min-width: 0;
}

.route-step-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.route-step-card__step-num {
  font-size: 11px;
  font-weight: 600;
  color: var(--el-color-primary);
  background: var(--el-color-primary-light-9);
  padding: 1px 8px;
  border-radius: 10px;
}

.route-step-card__drag-hint {
  color: var(--el-text-color-secondary);
  font-size: 12px;
}

.route-step-card__process-name {
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  line-height: 1.3;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.route-step-card__process-code {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-top: 2px;
}

.route-step-card__meta {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-top: 8px;
  font-size: 12px;
  color: var(--el-text-color-regular);
}

.route-step-card__meta .el-icon {
  font-size: 14px;
}

.route-step-card__actions {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding-right: 8px;
  opacity: 0;
  transition: opacity 0.2s;
}

.route-step-card:hover .route-step-card__actions {
  opacity: 1;
}
</style>
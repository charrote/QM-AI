<script setup lang="ts">
/**
 * RightPanel — 通用右侧滑入面板
 *
 * 用法：
 *   <RightPanel v-model:visible="showPanel" title="新建XXX">
 *     <template #body>
 *       <!-- 表单内容 -->
 *     </template>
 *     <template #footer>
 *       <el-button @click="cancel">取消</el-button>
 *       <el-button type="primary" @click="submit">确定</el-button>
 *     </template>
 *   </RightPanel>
 */
import { computed } from 'vue'
import { Close } from '@element-plus/icons-vue'
defineOptions({ name: 'RightPanel' })

const props = withDefaults(defineProps<{
  /** 面板可见性（v-model），支持 boolean 或 ref<boolean> */
  visible: boolean | { value: boolean }
  /** 面板标题 */
  title: string
  /** 面板宽度（默认 460px） */
  width?: string | number
  /** 是否显示关闭按钮（默认 true） */
  showClose?: boolean
  /** 是否显示遮罩层（默认 false） */
  mask?: boolean
}>(), {
  width: '460px',
  showClose: true,
  mask: false,
})

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

// 支持 boolean 和 ref<boolean> 两种传参方式
const visible = computed(() => {
  const v = props.visible
  return typeof v === 'object' && v !== null ? v.value : v
})

function close() {
  emit('update:visible', false)
}
</script>

<template>
  <Transition name="slide-right">
    <div v-if="visible" class="rp-root" :style="{ width: typeof width === 'number' ? `${width}px` : width }">
      <!-- 遮罩 -->
      <div v-if="mask" class="rp-mask" @click="close" />

      <div class="rp-panel">
        <!-- 头部 -->
        <div class="rp-header">
          <div class="rp-title">
            <slot name="title-icon" />
            <span>{{ title }}</span>
          </div>
          <el-button
            v-if="showClose"
            class="rp-close"
            circle
            size="small"
            @click="close"
          >
            <el-icon><Close /></el-icon>
          </el-button>
        </div>

        <!-- 主体 -->
        <div class="rp-body">
          <slot name="body" />
        </div>

        <!-- 底部 -->
        <div class="rp-footer">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
/* ── Root ── */
.rp-root {
  position: fixed;
  top: 0;
  right: 0;
  height: 100vh;
  background: var(--el-bg-color);
  border-left: 1px solid var(--el-border-color-light, #dcdfe6);
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.08);
  display: flex;
  flex-direction: column;
  z-index: 2000;
}

/* ── Mask ── */
.rp-mask {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.2);
  z-index: -1;
}

/* ── Panel ── */
.rp-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-width: 0;
}

/* ── Header ── */
.rp-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter, #ebeef5);
  flex-shrink: 0;
}

.rp-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.rp-close {
  color: var(--el-text-placeholder);
  transition: all 0.2s;
}

.rp-close:hover {
  color: var(--el-text-regular);
  background: var(--el-fill-color-light);
}

/* ── Body ── */
.rp-body {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
}

/* ── Footer ── */
.rp-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 14px 20px;
  border-top: 1px solid var(--el-border-color-lighter, #ebeef5);
  flex-shrink: 0;
  background: var(--el-bg-color);
}

/* ── Transition ── */
.slide-right-enter-active,
.slide-right-leave-active {
  transition: all 0.3s var(--ease-out, ease-out);
}

.slide-right-enter-from,
.slide-right-leave-to {
  transform: translateX(100%);
  opacity: 0;
}

.slide-right-enter-to,
.slide-right-leave-from {
  transform: translateX(0);
  opacity: 1;
}
</style>

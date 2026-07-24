<script setup lang="ts">
import { Check, InfoFilled, Warning, CircleClose, Close } from '@element-plus/icons-vue'

interface Props {
  type?: 'success' | 'warning' | 'error' | 'info'
  title?: string
  description?: string
  closable?: boolean
  showIcon?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  type: 'info',
  title: '',
  description: '',
  closable: true,
  showIcon: true,
})

const emit = defineEmits<{
  close: []
}>()

function handleClose() {
  emit('close')
}
</script>

<template>
  <div :class="['prompt-message', `prompt-message--${type}`]">
    <el-icon v-if="showIcon" class="prompt-message__icon" :size="16">
      <Check v-if="type === 'success'" />
      <Warning v-if="type === 'warning'" />
      <CircleClose v-if="type === 'error'" />
      <InfoFilled v-if="type === 'info'" />
    </el-icon>
    <div class="prompt-message__content">
      <div v-if="title" class="prompt-message__title">{{ title }}</div>
      <div v-if="description" class="prompt-message__desc">{{ description }}</div>
    </div>
    <el-icon v-if="closable" class="prompt-message__close" @click="handleClose">
      <Close />
    </el-icon>
  </div>
</template>

<style scoped>
.prompt-message {
  display: flex;
  align-items: flex-start;
  gap: var(--space-3);
  padding: var(--space-3) var(--space-4);
  border-radius: var(--radius-md);
  font-size: var(--font-sm);
  line-height: 1.5;
}

.prompt-message--success {
  background: #f6ffed;
  border: 1px solid #b7eb8f;
  color: #389e0d;
}

.prompt-message--warning {
  background: #fffbe6;
  border: 1px solid #ffe58f;
  color: #d48806;
}

.prompt-message--error {
  background: #fff2f0;
  border: 1px solid #ffccc7;
  color: #cf1322;
}

.prompt-message--info {
  background: #e6f4ff;
  border: 1px solid #91caff;
  color: #0958d9;
}

.prompt-message__icon {
  flex-shrink: 0;
  margin-top: 1px;
}

.prompt-message__content {
  flex: 1;
  min-width: 0;
}

.prompt-message__title {
  font-weight: var(--font-medium);
}

.prompt-message__desc {
  margin-top: 2px;
  opacity: 0.85;
}

.prompt-message__close {
  flex-shrink: 0;
  cursor: pointer;
  opacity: 0.6;
  transition: opacity 0.2s ease;
}

.prompt-message__close:hover {
  opacity: 1;
}

html.dark .prompt-message--success {
  background: rgba(82, 196, 26, 0.15);
  border-color: rgba(82, 196, 26, 0.3);
}

html.dark .prompt-message--warning {
  background: rgba(250, 173, 20, 0.15);
  border-color: rgba(250, 173, 20, 0.3);
}

html.dark .prompt-message--error {
  background: rgba(255, 77, 79, 0.15);
  border-color: rgba(255, 77, 79, 0.3);
}

html.dark .prompt-message--info {
  background: rgba(22, 119, 255, 0.15);
  border-color: rgba(22, 119, 255, 0.3);
}
</style>

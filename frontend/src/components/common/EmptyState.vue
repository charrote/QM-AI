<script setup lang="ts">
interface Props {
  icon?: string
  title?: string
  description?: string
  buttonText?: string
  buttonType?: 'primary' | 'default'
  showButton?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  icon: 'Document',
  title: '暂无数据',
  description: '',
  buttonText: '返回',
  buttonType: 'primary',
  showButton: false,
})

const emit = defineEmits<{
  click: []
}>()

function handleClick() {
  emit('click')
}
</script>

<template>
  <div class="empty-state">
    <el-icon class="empty-state__icon" :size="48">
      <component :is="icon" />
    </el-icon>
    <h3 class="empty-state__title">{{ title }}</h3>
    <p class="empty-state__desc" v-if="description">{{ description }}</p>
    <el-button
      v-if="showButton"
      :type="buttonType"
      @click="handleClick"
    >
      {{ buttonText }}
    </el-button>
  </div>
</template>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-12) 0;
  text-align: center;
}

.empty-state__icon {
  margin-bottom: var(--space-4);
  opacity: 0.5;
  color: var(--el-text-color-placeholder);
}

.empty-state__title {
  font-size: var(--font-md);
  font-weight: var(--font-medium);
  color: var(--el-text-color-regular);
  margin: 0 0 var(--space-2);
}

.empty-state__desc {
  font-size: var(--font-sm);
  color: var(--el-text-color-secondary);
  margin: 0 0 var(--space-4);
}
</style>

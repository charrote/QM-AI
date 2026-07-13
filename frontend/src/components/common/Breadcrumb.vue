<script setup lang="ts">
import { ArrowRight } from '@element-plus/icons-vue'

interface Props {
  items: Array<{ label: string; path?: string }>
}

const props = withDefaults(defineProps<Props>(), {
  items: () => [],
})

const emit = defineEmits<{
  click: [index: number, item: { label: string; path?: string }]
}>()

function handleClick(index: number, item: { label: string; path?: string }) {
  emit('click', index, item)
}
</script>

<template>
  <nav class="enterprise-breadcrumb">
    <div
      v-for="(item, index) in items"
      :key="index"
      class="breadcrumb-item"
      :class="{ clickable: item.path }"
      @click="item.path && handleClick(index, item)"
    >
      <span class="breadcrumb-text">{{ item.label }}</span>
      <el-icon v-if="index < items.length - 1" class="breadcrumb-separator" :size="10">
        <ArrowRight />
      </el-icon>
    </div>
  </nav>
</template>

<style scoped>
.enterprise-breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-size: var(--font-sm);
  color: var(--el-text-color-secondary);
  padding: var(--space-2) 0;
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.breadcrumb-item.clickable {
  cursor: pointer;
  transition: color 0.2s ease;
}

.breadcrumb-item.clickable:hover {
  color: var(--primary, #1677ff);
}

.breadcrumb-text {
  white-space: nowrap;
}

.breadcrumb-item:last-child .breadcrumb-text {
  color: var(--el-text-color-regular);
  font-weight: var(--font-medium);
}

.breadcrumb-separator {
  color: var(--el-text-color-placeholder);
  flex-shrink: 0;
}
</style>

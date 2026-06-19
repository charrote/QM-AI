<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps<{
  visible: boolean
  x: number
  y: number
  tabId: string
  closable: boolean
}>()

const emit = defineEmits<{
  close: []
  closeOthers: [tabId: string]
  closeAll: []
  refresh: []
}>()

const menuRef = ref<HTMLElement>()

function handleClickOutside(e: MouseEvent) {
  if (menuRef.value && !menuRef.value.contains(e.target as HTMLElement)) {
    emit('close')
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <Teleport to="body">
    <div
      v-if="visible"
      ref="menuRef"
      class="context-menu"
      :style="{ left: `${x}px`, top: `${y}px` }"
    >
      <template v-if="closable">
        <div class="context-menu-item" @click="emit('close')">
          <span>关闭</span>
        </div>
        <div class="context-menu-item" @click="emit('closeOthers', tabId)">
          <span>关闭其他</span>
        </div>
        <div class="context-menu-item" @click="emit('closeAll')">
          <span>关闭全部</span>
        </div>
        <div class="context-menu-divider" />
      </template>
      <div class="context-menu-item" @click="emit('refresh')">
        <span>刷新</span>
      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.context-menu {
  position: fixed;
  z-index: 9999;
  background: var(--bg-white, #ffffff);
  border: 1px solid var(--border-color, #e4e7ed);
  border-radius: 6px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.12);
  padding: 4px 0;
  min-width: 120px;
}

.context-menu-item {
  padding: 8px 16px;
  cursor: pointer;
  font-size: 13px;
  color: var(--text-primary, #303133);
  transition: background var(--transition-fast, 0.2s ease);
}

.context-menu-item:hover {
  background: var(--tab-item-hover-bg, #ecf5ff);
  color: var(--tab-item-active-color, #409eff);
}

.context-menu-divider {
  height: 1px;
  margin: 4px 0;
  background: var(--border-color, #e4e7ed);
}
</style>

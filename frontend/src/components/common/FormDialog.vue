<script setup lang="ts" generic="T extends Record<string, any>">
import { ref, computed } from 'vue'

const props = defineProps<{
  visible: boolean
  title: string
  width?: string
  confirmText?: string
  cancelText?: string
  loading?: boolean
  modelValue?: T
  closeOnClickOverlay?: boolean
  destroyOnClose?: boolean
  showClose?: boolean
  center?: boolean
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  submit: [data: T]
  cancel: []
  open: []
  close: []
}>()

const dialogVisible = computed({
  get: () => props.visible,
  set: (val) => emit('update:visible', val),
})

function handleClose() {
  dialogVisible.value = false
}

function handleSubmit() {
  emit('submit', props.modelValue as T)
}

function handleCancel() {
  emit('cancel')
  dialogVisible.value = false
}

function handleOpen() {
  emit('open')
}

function handleCloseEvent() {
  emit('close')
}
</script>

<template>
  <el-dialog
    :model-value="dialogVisible"
    :title="title"
    :width="width || '560px'"
    :close-on-click-modal="closeOnClickOverlay ?? false"
    :destroy-on-close="destroyOnClose ?? true"
    :show-close="showClose ?? true"
    :center="center"
    align-center
    append-to-body
    @close="handleCloseEvent"
    @open="handleOpen"
  >
    <slot :model-value="modelValue" />
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleCancel">{{ cancelText || '取消' }}</el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{ confirmText || '确定' }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<style scoped>
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
}
</style>

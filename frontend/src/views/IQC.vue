<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'

defineOptions({ name: 'IQC' })

const router = useRouter()

onMounted(() => {
  router.replace('/iqc/params')
})
</script>

<template>
  <div class="redirect-hint">
    <p>正在跳转到动态参数配置...</p>
  </div>
</template>

<style scoped>
.redirect-hint {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: var(--el-text-color-secondary);
  font-size: 14px;
}
</style>

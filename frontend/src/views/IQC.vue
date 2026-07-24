<script setup lang=ts>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'

defineOptions({ name: 'IQC' })

const router = useRouter()

onMounted(() => {
  router.replace('/iqc/params')
})
</script>

<template>
  <div class=redirect-page>
    <el-icon :size=32 color=#409eff class=redirect-icon><Loading /></el-icon>
    <p class=redirect-text>正在跳转到动态参数配置...</p>
  </div>
</template>

<script lang=ts>
import { Loading } from '@element-plus/icons-vue'
export default { components: { Loading } }
</script>

<style scoped>
.redirect-page {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 300px;
  gap: 12px;
}
.redirect-icon {
  animation: spin 1s linear infinite;
}
@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
.redirect-text {
  margin: 0;
  font-size: 14px;
  color: #909399;
}
</style>

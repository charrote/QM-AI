<script setup lang="ts">
import { WarningFilled } from '@element-plus/icons-vue'
defineOptions({ name: 'NotFound' })
</script>

<template>
  <div class="empty-state-page">
    <div class="empty-state-page__icon empty-state-page__icon--danger">
      <el-icon :size="72" color="#f56c6c"><WarningFilled /></el-icon>
    </div>
    <h3 class="empty-state-page__title">页面未找到</h3>
    <p class="empty-state-page__desc">您访问的页面不存在或已被移除，请检查URL或返回首页。</p>
    <el-button type="primary" @click="$router.push('/dashboard')">返回首页</el-button>
  </div>
</template>

<style scoped>
.empty-state-page {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  gap: 16px;
}
.empty-state-page__icon {
  width: 96px;
  height: 96px;
  border-radius: 50%;
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
}
.empty-state-page__icon--danger {
  background: #fef0f0;
}
.empty-state-page__title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #303133;
}
.empty-state-page__desc {
  margin: 0;
  font-size: 14px;
  color: #909399;
  max-width: 320px;
  text-align: center;
}
</style>

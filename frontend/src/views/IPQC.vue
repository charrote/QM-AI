<script setup lang="ts">
import { Monitor } from '@element-plus/icons-vue'
defineOptions({ name: 'IPQC' })
</script>

<template>
  <div class="empty-state-page">
    <div class="empty-state-page__icon">
      <el-icon :size="72" color="#67c23a"><Monitor /></el-icon>
    </div>
    <h3 class="empty-state-page__title">IPQC过程检验</h3>
    <p class="empty-state-page__desc">过程检验管理功能即将上线，支持首件检验、巡检与异常处理。</p>
    <el-button type="primary" @click="$router.push('/ipqc/first-pieces')">前往首件检验</el-button>
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

<script setup lang="ts">
defineOptions({ name: 'NotFound' })
</script>

<template>
  <div class="placeholder-view">
    <el-icon class="placeholder-icon" :size="80" color="#f56c6c"><WarningFilled /></el-icon>
    <h1>404</h1>
    <p>页面未找到 - Page Not Found</p>
    <el-button type="primary" @click="$router.push('/dashboard')">返回首页</el-button>
  </div>
</template>

<script setup lang="ts">
import { Clock } from '@element-plus/icons-vue'
defineOptions({ name: 'OperationLogPage' })
</script>

<template>
  <div class="page-container" style="height:100%;display:flex;flex-direction:column;overflow:hidden;">
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Clock /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">操作日志</h2>
          <span class="page-header-banner-subtitle">系统操作记录与审计追踪</span>
        </div>
      </div>
    </div>
    <div style="flex:1;display:flex;align-items:center;justify-content:center;color:var(--el-text-color-secondary);">
      <div style="text-align:center;">
        <el-icon :size="48" style="color:var(--el-color-info);"><Clock /></el-icon>
        <p style="margin-top:12px;font-size:14px;">功能开发中</p>
      </div>
    </div>
  </div>
</template>

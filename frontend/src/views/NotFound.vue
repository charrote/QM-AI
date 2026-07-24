<script setup lang="ts">
import { WarningFilled } from '@element-plus/icons-vue'
defineOptions({ name: 'NotFound' })
</script>

<template>
  <div class="not-found-page">
    <div class="not-found-bg">
      <div class="not-found-circle not-found-circle--1"></div>
      <div class="not-found-circle not-found-circle--2"></div>
      <div class="not-found-circle not-found-circle--3"></div>
    </div>

    <div class="not-found-content">
      <div class="not-found-icon-wrap">
        <el-icon class="not-found-icon" :size="80"><WarningFilled /></el-icon>
        <div class="not-found-404">404</div>
      </div>
      <h2 class="not-found-title">页面未找到</h2>
      <p class="not-found-desc">您访问的页面不存在或已被移除，请检查 URL 或返回首页。</p>
      <el-button type="primary" size="large" round @click="$router.push('/dashboard')">
        返回首页
      </el-button>
    </div>
  </div>
</template>

<style scoped>
.not-found-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 50%, #d5dce8 100%);
  position: relative;
  overflow: hidden;
}

.not-found-bg {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.not-found-circle {
  position: absolute;
  border-radius: 50%;
  opacity: 0.08;
}

.not-found-circle--1 {
  width: 400px;
  height: 400px;
  background: #1a73e8;
  top: -120px;
  right: -100px;
}

.not-found-circle--2 {
  width: 300px;
  height: 300px;
  background: #f56c6c;
  bottom: -80px;
  left: -60px;
}

.not-found-circle--3 {
  width: 200px;
  height: 200px;
  background: #e6a23c;
  top: 50%;
  left: 60%;
}

.not-found-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  z-index: 1;
  padding: 24px;
}

.not-found-icon-wrap {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
}

.not-found-icon {
  color: #f56c6c;
  background: white;
  border-radius: 50%;
  width: 100px;
  height: 100px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 30px rgba(245, 108, 108, 0.15);
}

.not-found-404 {
  position: absolute;
  top: -8px;
  right: -16px;
  font-size: 18px;
  font-weight: 800;
  color: white;
  background: linear-gradient(135deg, #1a73e8, #0d47a1);
  width: 40px;
  height: 40px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(26, 115, 232, 0.3);
}

.not-found-title {
  margin: 0;
  font-size: 22px;
  font-weight: 700;
  color: #303133;
}

.not-found-desc {
  margin: 0;
  font-size: 14px;
  color: #909399;
  max-width: 340px;
  text-align: center;
  line-height: 1.6;
}
</style>

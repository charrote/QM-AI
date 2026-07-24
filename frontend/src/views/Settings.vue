<script setup lang="ts">
import { Setting } from '@element-plus/icons-vue'
defineOptions({ name: 'Settings' })
</script>

<template>
  <div class="settings-page">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="page-header-main">
        <h2>系统管理</h2>
        <p>系统配置、权限管理与操作日志</p>
      </div>
    </div>

    <!-- 功能卡片网格 -->
    <div class="card-grid">
      <div
        v-for="item in featureCards"
        :key="item.key"
        class="feature-card"
      >
        <div class="card-icon" :style="{ background: item.bg }">
          <el-icon :size="32" :color="item.color"><component :is="item.icon" /></el-icon>
        </div>
        <div class="card-body">
          <div class="card-top">
            <h3>{{ item.title }}</h3>
            <el-tag size="small" effect="plain" class="coming-soon-tag">
              <span class="coming-dot"></span>
              即将上线
            </el-tag>
          </div>
          <p class="card-desc">{{ item.desc }}</p>
        </div>
        <div class="card-footer">
          <span class="coming-soon-label">功能开发中</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Setting, Lock, Document, Collection } from '@element-plus/icons-vue'

interface FeatureCard {
  key: string
  icon: typeof Setting
  title: string
  desc: string
  bg: string
  color: string
}

const featureCards: FeatureCard[] = [
  {
    key: 'system-config',
    icon: Setting,
    title: '系统配置',
    desc: '系统参数配置与开关管理',
    bg: 'linear-gradient(135deg, #ecf5ff 0%, #d9ecff 100%)',
    color: '#409eff',
  },
  {
    key: 'permission',
    icon: Lock,
    title: '权限管理',
    desc: '用户角色与功能权限配置',
    bg: 'linear-gradient(135deg, #fdf2f8 0%, #fce7f3 100%)',
    color: '#f56c6c',
  },
  {
    key: 'audit-log',
    icon: Document,
    title: '操作日志',
    desc: '系统操作记录与审计追踪',
    bg: 'linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%)',
    color: '#0ea5e9',
  },
  {
    key: 'data-dict',
    icon: Collection,
    title: '数据字典',
    desc: '系统字典数据管理',
    bg: 'linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%)',
    color: '#22c55e',
  },
]
</script>

<style scoped>
.settings-page {
  display: flex;
  flex-direction: column;
  gap: 24px;
  min-height: calc(100vh - 120px);
  padding: 4px 0;
}

/* ── 页面标题 ── */
.page-header {
  flex-shrink: 0;
  padding: 0 4px;
}
.page-header-main h2 {
  margin: 0;
  font-size: 22px;
  font-weight: 600;
  color: #303133;
  letter-spacing: -0.02em;
}
.page-header-main p {
  margin: 6px 0 0;
  font-size: 14px;
  color: #909399;
}

/* ── 卡片网格 ── */
.card-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
  padding: 0 4px;
}

.feature-card {
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
  transition: transform 0.2s, box-shadow 0.2s;
  border: 1px solid #f0f2f5;
}
.feature-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.06);
}

.card-icon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.card-body {
  flex: 1;
}

.card-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}
.card-top h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}

.coming-soon-tag {
  display: flex;
  align-items: center;
  gap: 4px;
  font-weight: 500;
}
.coming-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #909399;
  flex-shrink: 0;
}

.card-desc {
  margin: 0;
  font-size: 13px;
  color: #909399;
  line-height: 1.6;
}

.card-footer {
  padding-top: 12px;
  border-top: 1px solid #f0f2f5;
}
.coming-soon-label {
  font-size: 12px;
  color: #c0c4cc;
  letter-spacing: 0.02em;
}
</style>
<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { User, Lock } from '@element-plus/icons-vue'
import type { FormInstance, FormRules } from 'element-plus'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const formRef = ref<FormInstance>()
const loading = ref(false)
const errorMsg = ref('')
const rememberMe = ref(localStorage.getItem('qm-ai-remembered-user') !== null)

const form = reactive({
  username: rememberMe.value ? localStorage.getItem('qm-ai-remembered-user') || '' : '',
  password: '',
})

const rules: FormRules = {
  username: [
    { required: true, message: '请输入用户名', trigger: 'blur' },
    { min: 2, message: '用户名至少2位', trigger: 'blur' },
  ],
  password: [
    { required: true, message: '请输入密码', trigger: 'blur' },
    { min: 4, message: '密码至少4位', trigger: 'blur' },
  ],
}

async function handleLogin() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  errorMsg.value = ''
  try {
    await authStore.login(form.username, form.password)
    if (rememberMe.value) {
      localStorage.setItem('qm-ai-remembered-user', form.username)
    } else {
      localStorage.removeItem('qm-ai-remembered-user')
    }
    const redirect = (route.query.redirect as string) || '/dashboard'
    router.push(redirect)
  } catch (err: unknown) {
    const e = err as { response?: { data?: { message?: string } }; message?: string }
    errorMsg.value = e.response?.data?.message || e.message || '登录失败，请检查用户名和密码'
  } finally {
    loading.value = false
  }
}

// 系统状态展示数据
const systemStats = [
  { label: '业务模块', value: '14+' },
  { label: '数据表', value: '45+' },
  { label: 'AI 引擎', value: '智能' },
]

// 功能模块展示
const featureCards = [
  { icon: 'Box', title: 'IQC 来料检验', desc: '供应商来料登记与检验' },
  { icon: 'Monitor', title: 'IPQC 过程检验', desc: '产线首件与巡检管理' },
  { icon: 'Checked', title: 'FQC/OQC 成品检验', desc: '成品出厂检验与放行' },
  { icon: 'DataAnalysis', title: 'SPC 统计分析', desc: '统计过程控制与分析' },
  { icon: 'Warning', title: '缺陷与 CAPA', desc: '缺陷管理与纠正预防' },
  { icon: 'ChatLineSquare', title: '客诉与 8D', desc: '客户投诉与 8D 报告' },
]

const currentYear = computed(() => new Date().getFullYear())
</script>

<template>
  <div class="login-page">
    <div class="login-layout">
      <!-- Left: Branding Panel -->
      <div class="brand-panel">
        <div class="brand-bg">
          <div class="bg-gradient-1"></div>
          <div class="bg-gradient-2"></div>
          <div class="bg-gradient-3"></div>
          <div class="bg-grid"></div>
          <div class="bg-particles">
            <span v-for="i in 12" :key="i" class="particle" :style="{ left: (i * 8.3) + '%', top: (i * 7.7) + '%' }"></span>
          </div>
        </div>
        
        <div class="brand-content">
          <div class="brand-logo">
            <div class="logo-badge">
              <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M9 12L11 14L15 10M21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C16.9706 3 21 7.02944 21 12Z" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>
            <span class="logo-text">QM-AI</span>
          </div>
          
          <h1 class="brand-title">工业 AI 质量决策平台</h1>
          <p class="brand-subtitle">Industrial AI Quality Decision Platform</p>
          
          <div class="brand-stats">
            <div v-for="stat in systemStats" :key="stat.label" class="stat-item">
              <div class="stat-value">{{ stat.value }}</div>
              <div class="stat-label">{{ stat.label }}</div>
            </div>
          </div>
          
          <div class="brand-features">
            <div v-for="feat in featureCards" :key="feat.title" class="feature-item">
              <el-icon :size="14" class="feature-icon">
                <component :is="feat.icon" />
              </el-icon>
              <span>{{ feat.title }}</span>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Right: Login Form Panel -->
      <div class="form-panel">
        <div class="form-container">
          <div class="form-header">
            <h2 class="form-title">欢迎登录</h2>
            <p class="form-desc">请输入您的账号信息以继续</p>
          </div>
          
          <el-form
            ref="formRef"
            :model="form"
            :rules="rules"
            size="large"
            @keyup.enter="handleLogin"
          >
            <el-form-item prop="username">
              <el-input
                v-model="form.username"
                placeholder="用户名"
                :prefix-icon="User"
                autocomplete="username"
                @focus="errorMsg = ''"
              />
            </el-form-item>
            
            <el-form-item prop="password">
              <el-input
                v-model="form.password"
                type="password"
                placeholder="密码"
                show-password
                :prefix-icon="Lock"
                autocomplete="current-password"
                @focus="errorMsg = ''"
              />
            </el-form-item>
            
            <div class="form-options">
              <el-checkbox v-model="rememberMe">记住我</el-checkbox>
            </div>
            
            <el-alert
              v-if="errorMsg"
              :title="errorMsg"
              type="error"
              show-icon
              closable
              class="login-error"
              @close="errorMsg = ''"
            />
            
            <el-button
              type="primary"
              :loading="loading"
              class="login-btn"
              @click="handleLogin"
            >
              {{ loading ? '登录中...' : '登 录' }}
            </el-button>
          </el-form>
          
          <div class="form-footer">
            <span>© {{ currentYear }} QM-AI · 工业 AI 质量决策平台</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import {
  Box, Monitor, Checked, DataAnalysis,
  Warning, ChatLineSquare
} from '@element-plus/icons-vue'

export default {
  components: {
    Box, Monitor, Checked, DataAnalysis,
    Warning, ChatLineSquare
  }
}
</script>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: #f0f2f5;
}

.login-layout {
  display: flex;
  width: 100%;
  min-height: 100vh;
}

/* ═══ Brand Panel ═══ */
.brand-panel {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
  padding: 60px;
  background: linear-gradient(135deg, #001529 0%, #002140 40%, #003a6e 100%);
}

/* Background Effects */
.brand-bg {
  position: absolute;
  inset: 0;
  overflow: hidden;
}

.bg-gradient-1 {
  position: absolute;
  width: 600px;
  height: 600px;
  top: -200px;
  right: -100px;
  background: radial-gradient(circle, rgba(22, 119, 255, 0.18) 0%, transparent 70%);
  pointer-events: none;
}

.bg-gradient-2 {
  position: absolute;
  width: 400px;
  height: 400px;
  bottom: -100px;
  left: -50px;
  background: radial-gradient(circle, rgba(82, 196, 26, 0.12) 0%, transparent 70%);
  pointer-events: none;
}

.bg-gradient-3 {
  position: absolute;
  width: 500px;
  height: 500px;
  top: 40%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: radial-gradient(circle, rgba(64, 150, 255, 0.06) 0%, transparent 60%);
  pointer-events: none;
}

.bg-grid {
  position: absolute;
  inset: 0;
  background-image: 
    linear-gradient(rgba(255, 255, 255, 0.025) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.025) 1px, transparent 1px);
  background-size: 40px 40px;
  pointer-events: none;
}

.bg-particles {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.particle {
  position: absolute;
  width: 3px;
  height: 3px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 50%;
  box-shadow: 0 0 6px rgba(64, 150, 255, 0.2);
  animation: particleFloat 6s ease-in-out infinite;
}

.particle:nth-child(odd) {
  width: 2px;
  height: 2px;
  background: rgba(255, 255, 255, 0.1);
  animation-duration: 8s;
  animation-delay: -3s;
}

.particle:nth-child(3n) {
  width: 4px;
  height: 4px;
  background: rgba(64, 150, 255, 0.12);
  box-shadow: 0 0 8px rgba(64, 150, 255, 0.3);
  animation-duration: 7s;
  animation-delay: -1s;
}

@keyframes particleFloat {
  0%, 100% { transform: translateY(0) translateX(0); opacity: 0.4; }
  25% { transform: translateY(-12px) translateX(4px); opacity: 0.8; }
  50% { transform: translateY(-6px) translateX(-3px); opacity: 0.5; }
  75% { transform: translateY(-18px) translateX(6px); opacity: 0.7; }
}

.brand-content {
  position: relative;
  z-index: 1;
  text-align: center;
  color: #fff;
  max-width: 420px;
}

/* Logo */
.brand-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 14px;
  margin-bottom: 32px;
}

.logo-badge {
  width: 44px;
  height: 44px;
  color: #fff;
  background: rgba(64, 150, 255, 0.15);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(64, 150, 255, 0.25);
  box-shadow: 0 0 16px rgba(64, 150, 255, 0.12);
}

.logo-badge svg {
  width: 26px;
  height: 26px;
}

.logo-text {
  font-size: 28px;
  font-weight: 700;
  letter-spacing: 1px;
}

.brand-title {
  font-size: 22px;
  font-weight: 600;
  margin: 0 0 8px;
  color: #fff;
  letter-spacing: 0.5px;
}

.brand-subtitle {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.5);
  margin: 0 0 36px;
  letter-spacing: 0.3px;
}

/* Stats — Glass Morphism Cards */
.brand-stats {
  display: flex;
  gap: 16px;
  justify-content: center;
  margin-bottom: 40px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  padding: 18px 32px;
  background: rgba(255, 255, 255, 0.06);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 
    0 4px 24px rgba(0, 0, 0, 0.15),
    0 0 0 1px rgba(255, 255, 255, 0.04) inset;
  min-width: 110px;
}

.stat-value {
  font-size: 30px;
  font-weight: 700;
  color: #4096ff;
  text-shadow: 0 0 20px rgba(64, 150, 255, 0.25);
  line-height: 1.2;
}

.stat-label {
  font-size: 12px;
  color: rgba(255, 255, 255, 0.78);
  letter-spacing: 0.3px;
}

/* Feature Cards — Glass Morphism */
.brand-features {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 10px;
  text-align: left;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  background: rgba(255, 255, 255, 0.06);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  border-radius: 10px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  font-size: 13px;
  color: rgba(255, 255, 255, 0.82);
}

.feature-icon {
  color: rgba(255, 255, 255, 0.65);
  flex-shrink: 0;
}

/* ═══ Form Panel ═══ */
.form-panel {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px;
  background: #fff;
}

.form-container {
  width: 100%;
  max-width: 380px;
}

.form-header {
  margin-bottom: 36px;
}

.form-title {
  font-size: 24px;
  font-weight: 600;
  color: #1a1a1a;
  margin: 0 0 6px;
}

.form-desc {
  font-size: 13px;
  color: #8c8c8c;
  margin: 0;
}

.form-options {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.login-error {
  margin-bottom: 20px;
}

.login-btn {
  width: 100%;
  height: 44px;
  font-size: 15px;
  font-weight: 500;
  border-radius: 8px;
}

.form-footer {
  text-align: center;
  margin-top: 32px;
  font-size: 12px;
  color: #bfbfbf;
}

/* ═══ Responsive ═══ */
@media (max-width: 1024px) {
  .brand-panel {
    display: none;
  }
  
  .form-panel {
    flex: 1;
  }
}

/* Dark mode */
html.dark .login-page {
  background: #111114;
}

html.dark .form-panel {
  background: #18181b;
}

html.dark .form-title {
  color: #f4f4f5;
}

html.dark .form-desc {
  color: #71717a;
}

html.dark .form-footer {
  color: #52525b;
}
</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { RefreshLeft, Box } from '@element-plus/icons-vue'
import { traceApi } from '@/api/trace'
import type { RecallSimulationResult } from '@/types/trace'

defineOptions({ name: 'RecallSimulationPage' })

const route = useRoute()
const batchCode = ref(route.query.batchCode as string || '')
const loading = ref(false)
const result = ref<RecallSimulationResult | null>(null)
const error = ref('')

async function simulate() {
  loading.value = true
  result.value = null
  error.value = ''
  try {
    if (!batchCode.value.trim()) { ElMessage.warning('请输入批次号'); return }
    result.value = await traceApi.recallSimulation(batchCode.value.trim())
  } catch (e: any) {
    error.value = e?.response?.data?.message || '召回模拟失败'
    ElMessage.error(error.value)
  } finally {
    loading.value = false
  }
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

onMounted(() => {})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="page-header-banner">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="22"><RefreshLeft /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <span class="page-header-banner-title">召回模拟</span>
          <span class="page-header-banner-subtitle">模拟批次产品召回的影响范围，辅助决策</span>
        </div>
      </div>
    </div>

    <el-card shadow="never" class="search-card">
      <div class="search-card-header">
        <span class="search-card-title">批次号查询</span>
        <el-tag size="small" type="danger">风险评估</el-tag>
      </div>
      <div class="search-input-row">
        <el-input v-model="batchCode" placeholder="请输入批次号" size="large" clearable @keyup.enter="simulate" style="max-width: 420px">
          <template #prefix><el-icon><Box /></el-icon></template>
        </el-input>
        <el-button type="danger" size="large" @click="simulate" :loading="loading">
          <el-icon><RefreshLeft /></el-icon> 模拟召回
        </el-button>
      </div>
      <div class="search-hints">
        <el-text type="info">模拟该批次产品召回的影响范围：受影响客户、召回数量、预计损失</el-text>
      </div>
      <div v-if="error" class="error-msg">
        <el-alert :title="error" type="error" :closable="false" show-icon />
      </div>
    </el-card>

    <div v-if="result && !loading" class="result-container">
      <el-card shadow="never" class="info-card">
        <template #header>
          <span>召回情景</span>
          <el-tag v-if="result.estimatedRecallCost" size="small" type="danger">预计成本 ¥{{ result.estimatedRecallCost?.toLocaleString() }}</el-tag>
        </template>
        <div class="info-row">
          <div class="info-item">
            <span class="info-label">产品</span>
            <span class="info-value">{{ result.productName || '-' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">批次</span>
            <span class="info-value">{{ result.batchCode || '-' }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">批次数量</span>
            <span class="info-value">{{ result.batchQuantity }} pcs</span>
          </div>
          <div class="info-item">
            <span class="info-label">预计召回成本</span>
            <span class="info-value risk-cost">{{ result.estimatedRecallCost?.toLocaleString() ?? '-' }}</span>
          </div>
        </div>
      </el-card>

      <el-card v-if="result.affectedCustomers?.length" shadow="never">
        <template #header>
          <span>受影响客户列表</span>
          <el-tag size="small" type="info">{{ result.affectedCustomers.length }} 个客户</el-tag>
        </template>
        <el-table :data="result.affectedCustomers" stripe>
          <el-table-column prop="name" label="客户名称" min-width="150" />
          <el-table-column prop="region" label="地区" width="120" />
          <el-table-column label="数量" width="100" align="right">
            <template #default="{ row }">{{ row.quantity }} pcs</template>
          </el-table-column>
          <el-table-column label="预计损失" width="130" align="right">
            <template #default="{ row }"><span class="loss-value">{{ row.estimatedLoss?.toLocaleString() ?? '-' }}</span></template>
          </el-table-column>
        </el-table>
      </el-card>

      <el-empty v-else description="暂无受影响客户" :image-size="100" />
    </div>

    <el-card v-else class="empty-card" shadow="never">
      <el-empty description="请输入批次号进行召回模拟">
        <el-text type="info">模拟该批次产品流向及召回影响范围，辅助决策</el-text>
      </el-empty>
    </el-card>
  </div>
</template>

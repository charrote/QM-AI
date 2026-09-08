<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Box, Search } from '@element-plus/icons-vue'
import { TRACE_METHOD_OPTIONS } from '@/types/trace'

defineOptions({ name: 'TracePage' })

const method = ref<'sn' | 'batch' | 'equipment'>('sn')
const snInput = ref('')
const batchInput = ref('')
const equipmentInput = ref('')

function trace() {
  ElMessage.warning('需要与MES系统对接')
}

onMounted(() => {})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="page-header-banner">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="22"><Search /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <span class="page-header-banner-title">质量追溯</span>
          <span class="page-header-banner-subtitle">6 阶段全流程追溯 — 来料 → 领料 → 加工 → 检验 → 批次 → 出货</span>
        </div>
      </div>
    </div>

    <!-- Search Panel -->
    <el-card shadow="never" class="search-card">
      <div class="search-card-header">
        <span class="search-card-title">追溯查询</span>
        <el-tag size="small" type="info">6 阶段全流程追溯</el-tag>
      </div>
      <div class="search-methods">
        <el-radio-group v-model="method" size="large">
          <el-radio-button v-for="opt in TRACE_METHOD_OPTIONS" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </el-radio-button>
        </el-radio-group>
      </div>
      <div class="search-input-row">
        <template v-if="method === 'sn'">
          <el-input v-model="snInput" placeholder="请输入 SN 编码" size="large" clearable style="max-width: 420px">
            <template #prefix><el-icon><Box /></el-icon></template>
          </el-input>
        </template>
        <template v-else-if="method === 'batch'">
          <el-input v-model="batchInput" placeholder="请输入批次号" size="large" clearable style="max-width: 420px">
            <template #prefix><el-icon><Box /></el-icon></template>
          </el-input>
        </template>
        <template v-else>
          <el-input v-model="equipmentInput" placeholder="请输入设备 ID 或编码" size="large" clearable style="max-width: 420px">
            <template #prefix><el-icon><Box /></el-icon></template>
          </el-input>
        </template>
        <el-button type="primary" size="large" @click="trace">
          <el-icon><Box /></el-icon> 追溯
        </el-button>
      </div>
      <div class="search-hints">
        <el-text type="info">输入 SN / 批次号 / 设备编码进行 6 阶段质量追溯</el-text>
      </div>
    </el-card>

    <!-- Empty state -->
    <el-card class="empty-card" shadow="never">
      <el-empty description="请输入 SN 编码、批次号或设备编码开始追溯">
        <template #extra>
          <el-text type="warning" :line-height="2" class="mes-hint">
            <el-icon :size="18" style="vertical-align: middle"><Box /></el-icon>
            MES对接同步后提供
          </el-text>
        </template>
      </el-empty>
    </el-card>
  </div>
</template>

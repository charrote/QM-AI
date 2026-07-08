<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { aiApi } from '@/api/ai'
import { ALERT_LEVEL_OPTIONS, ALERT_LEVEL_MAP } from '@/types/ai'
import type { PagedResult } from '@/types/basicData'

defineOptions({ name: 'AI' })
const activeTab = ref('alerts')
</script>
<template>
  <div style="height:100%;display:flex;flex-direction:column">
    <el-tabs v-model="activeTab" type="border-card">
      <el-tab-pane name="alerts"><AlertCenterPage /></el-tab-pane>
      <el-tab-pane name="root-cause"><RootCauseAnalysisPage /></el-tab-pane>
      <el-tab-pane name="models"><ModelManagementPage /></el-tab-pane>
    </el-tabs>
  </div>
</template>

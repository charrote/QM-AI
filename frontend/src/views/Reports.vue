<script setup lang="ts">
import { ref } from 'vue'
defineOptions({ name: 'Reports' })
const activeTab = ref('dashboard')
</script>
<template>
  <div style="height:100%;display:flex;flex-direction:column">
    <el-tabs v-model="activeTab" type="border-card" style="flex:1;display:flex;flex-direction:column">
      <el-tab-pane name="dashboard"><QualityDashboardPage /></el-tab-pane>
      <el-tab-pane name="builder"><ReportBuilderPage /></el-tab-pane>
      <el-tab-pane name="export"><ExportCenterPage /></el-tab-pane>
    </el-tabs>
  </div>
</template>

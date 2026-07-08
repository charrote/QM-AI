<script setup lang="ts">
import { ref } from 'vue'
import ComplaintListPage from './complaints/ComplaintListPage.vue'
import D8ReportPage from './complaints/D8ReportPage.vue'
import ComplaintTimelinePage from './complaints/ComplaintTimelinePage.vue'

defineOptions({ name: 'Complaints' })
const activeTab = ref('list')
</script>

<template>
  <div style="height:100%;display:flex;flex-direction:column">
    <el-tabs v-model="activeTab" type="border-card">
      <el-tab-pane name="list"><ComplaintListPage /></el-tab-pane>
      <el-tab-pane name="d8"><D8ReportPage /></el-tab-pane>
      <el-tab-pane name="timeline"><ComplaintTimelinePage /></el-tab-pane>
    </el-tabs>
  </div>
</template>

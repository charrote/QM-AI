<script setup lang="ts">
import { ref } from 'vue'
defineOptions({ name: 'Audits' })
const activeTab = ref('list')
</script>
<template>
  <div style="height:100%;display:flex;flex-direction:column">
    <el-tabs v-model="activeTab" type="border-card">
      <el-tab-pane name="list"><AuditListPage /></el-tab-pane>
      <el-tab-pane name="finding"><FindingPage /></el-tab-pane>
    </el-tabs>
  </div>
</template>

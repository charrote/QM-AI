<script setup lang="ts">
import { ref } from 'vue'
import DocumentListPage from './documents/DocumentListPage.vue'
import VersionHistoryPage from './documents/VersionHistoryPage.vue'

defineOptions({ name: 'Documents' })
const activeTab = ref('list')
</script>

<template>
  <div style="height:100%;display:flex;flex-direction:column">
    <el-tabs v-model="activeTab" type="border-card">
      <el-tab-pane name="list"><DocumentListPage /></el-tab-pane>
      <el-tab-pane name="versions"><VersionHistoryPage /></el-tab-pane>
    </el-tabs>
  </div>
</template>

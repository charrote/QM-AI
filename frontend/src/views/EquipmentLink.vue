<script setup lang="ts">
import { ref } from 'vue'
import ParamMappingPage from '@/views/equipmentlink/ParamMappingPage.vue'
import StatusHistoryPage from '@/views/equipmentlink/StatusHistoryPage.vue'
import QualityCorrelationPage from '@/views/equipmentlink/QualityCorrelationPage.vue'

defineOptions({ name: 'EquipmentLink' })
const activeTab = ref('mapping')
</script>
<template>
  <div style="height:100%;display:flex;flex-direction:column">
    <el-tabs v-model="activeTab" type="border-card">
      <el-tab-pane name="mapping">
        <template #label>
          <span>参数映射</span>
        </template>
        <ParamMappingPage />
      </el-tab-pane>
      <el-tab-pane name="status">
        <template #label>
          <span>状态历史</span>
        </template>
        <StatusHistoryPage />
      </el-tab-pane>
      <el-tab-pane name="correlation">
        <template #label>
          <span>质量关联</span>
        </template>
        <QualityCorrelationPage />
      </el-tab-pane>
    </el-tabs>
  </div>
</template>

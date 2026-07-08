<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { equipmentLinkApi } from '@/api/equipmentLink'
import { equipmentApi } from '@/api/basicData'
import { SIGNAL_OPTIONS } from '@/types/equipmentLink'
import type { EquipmentStatusHistory } from '@/types/equipmentLink'
import type { Equipment } from '@/types/basicData'

defineOptions({ name: 'StatusHistoryPage' })

const loading = ref(false)
const records = ref<EquipmentStatusHistory[]>([])
const equipments = ref<Equipment[]>([])

const query = reactive({
  equipmentId: '' as string | '',
  dateStart: '',
  dateEnd: '',
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function getSignalLabel(signal: string) {
  const opt = SIGNAL_OPTIONS.find((o: any) => o.value === signal)
  return opt?.label ?? signal
}

function getSignalType(signal: string) {
  const opt = SIGNAL_OPTIONS.find((o: any) => o.value === signal)
  return opt?.type ?? 'info'
}

function getEquipmentName(id: number): string {
  const eq = equipments.value.find(e => e.id === id)
  return eq ? `${eq.name} (${eq.code})` : `设备${id}`
}

async function loadEquipments() {
  try {
    const res = await equipmentApi.list({ pageSize: 1000 })
    equipments.value = res.items
  } catch (e) {
    console.error('Failed to load equipments', e)
  }
}

async function loadRecords() {
  if (!query.equipmentId) {
    records.value = []
    return
  }
  loading.value = true
  try {
    // TODO: Implement status history by equipmentId
    // Current backend only supports recent-status/{mappingId}
    records.value = []
    ElMessage.warning('状态历史功能待实现')
  } catch (e) {
    console.error('Failed to load statuses', e)
    ElMessage.error('加载状态历史失败')
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  loadRecords()
}

function resetQuery() {
  query.equipmentId = ''
  query.dateStart = ''
  query.dateEnd = ''
  records.value = []
}

onMounted(() => {
  loadEquipments()
})
</script>
<template>
  <div class="page-container">
    <div class="toolbar-row">
      <el-select v-model="query.equipmentId" placeholder="选择设备" clearable style="width: 200px" size="small" @change="handleSearch">
        <el-option v-for="eq in equipments" :key="eq.id" :label="`${eq.name} (${eq.code})`" :value="eq.id" />
      </el-select>
      <el-date-picker
        v-model="query.dateStart"
        type="date"
        placeholder="开始日期"
        size="small"
        value-format="YYYY-MM-DD"
        style="width: 150px"
      />
      <span style="line-height: 28px">至</span>
      <el-date-picker
        v-model="query.dateEnd"
        type="date"
        placeholder="结束日期"
        size="small"
        value-format="YYYY-MM-DD"
        style="width: 150px"
      />
      <el-button @click="handleSearch" size="small">查询</el-button>
      <el-button @click="resetQuery" size="small">重置</el-button>
    </div>

    <el-table :data="records" v-loading="loading" stripe border style="width: 100%" size="small">
      <el-table-column prop="equipmentId" label="设备ID" width="70" />
      <el-table-column label="设备名称" min-width="150">
        <template #default="{ row }">{{ getEquipmentName(row.equipmentId) }}</template>
      </el-table-column>
      <el-table-column label="信号" width="80">
        <template #default="{ row }">
          <el-tag :type="getSignalType(row.signal)" size="small" effect="plain">
            {{ getSignalLabel(row.signal) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="signalData" label="信号数据" min-width="160" show-overflow-tooltip />
      <el-table-column prop="recordedAt" label="记录时间" min-width="170">
        <template #default="{ row }">{{ formatDate(row.recordedAt) }}</template>
      </el-table-column>
    </el-table>

    <div v-if="!loading && records.length === 0 && query.equipmentId" class="empty-hint">
      暂无状态记录
    </div>
  </div>
</template>
<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.empty-hint { text-align: center; padding: 40px; color: #909399; font-size: 14px; }
</style>

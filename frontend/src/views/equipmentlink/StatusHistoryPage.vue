<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Monitor } from '@element-plus/icons-vue'
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
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon"><el-icon :size="28"><Monitor /></el-icon></div>
        <div class="page-header-text">
          <h2>设备状态历史</h2>
          <p>设备运行信号的历史记录查询与分析</p>
        </div>
      </div>
    </div>

    <!-- Toolbar -->
    <el-card shadow="never" class="search-card">
      <div class="search-bar">
        <el-select v-model="query.equipmentId" placeholder="选择设备" clearable style="width: 220px" @change="handleSearch">
          <el-option v-for="eq in equipments" :key="eq.id" :label="`${eq.name} (${eq.code})`" :value="eq.id" />
        </el-select>
        <el-date-picker
          v-model="query.dateStart"
          type="date"
          placeholder="开始日期"
          value-format="YYYY-MM-DD"
          style="width: 160px"
        />
        <span class="date-separator">至</span>
        <el-date-picker
          v-model="query.dateEnd"
          type="date"
          placeholder="结束日期"
          value-format="YYYY-MM-DD"
          style="width: 160px"
        />
        <el-button type="primary" @click="handleSearch">查询</el-button>
        <el-button @click="resetQuery">重置</el-button>
      </div>
    </el-card>

    <!-- Table -->
    <el-card shadow="never" class="table-card">
      <el-table :data="records" v-loading="loading" stripe border style="width: 100%">
        <el-table-column label="设备" min-width="160">
          <template #default="{ row }"><span class="equipment-name">{{ getEquipmentName(row.equipmentId) }}</span></template>
        </el-table-column>
        <el-table-column label="信号" width="90">
          <template #default="{ row }">
            <el-tag :type="getSignalType(row.signal)" size="small" effect="dark">
              {{ getSignalLabel(row.signal) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="signalData" label="信号数据" min-width="160" show-overflow-tooltip />
        <el-table-column label="记录时间" min-width="175">
          <template #default="{ row }">{{ formatDate(row.recordedAt) }}</template>
        </el-table-column>
      </el-table>

      <div v-if="!loading && records.length === 0 && query.equipmentId" class="empty-hint">
        暂无状态记录
      </div>
    </el-card>
  </div>
</template>
<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.page-header { margin-bottom: 16px; }
.page-header-main { display: flex; align-items: center; gap: 14px; }
.page-header-icon { width: 44px; height: 44px; display: flex; align-items: center; justify-content: center; background: #e6f7ff; border-radius: 10px; }
.page-header-text h2 { margin: 0; font-size: 20px; font-weight: 600; color: #303133; }
.page-header-text p { margin: 2px 0 0; font-size: 13px; color: #909399; }
.search-card { margin-bottom: 12px; }
.search-bar { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.date-separator { color: #909399; line-height: 36px; }
.table-card { flex: 1; }
.empty-hint { text-align: center; padding: 40px; color: #909399; font-size: 14px; }
.equipment-name { font-weight: 500; color: #303133; }
</style>

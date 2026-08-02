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
    const data = await equipmentLinkApi.statusHistory(Number(query.equipmentId), 100)
    records.value = data
  } catch (e) {
    console.error('Failed to load statuses', e)
    ElMessage.error('加载状态历史失败，请稍后重试')
    records.value = []
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
    <!-- Page Header Banner (Primary) -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="24"><Monitor /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <div class="page-header-banner-title">设备状态历史</div>
          <div class="page-header-banner-subtitle">设备运行信号的历史记录查询与分析</div>
        </div>
      </div>
    </div>

    <!-- Filter Card -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
        <el-select v-model="query.equipmentId" placeholder="选择设备" clearable style="width: 220px" @change="handleSearch">
          <el-option v-for="eq in equipments" :key="eq.id" :label="`${eq.name} (${eq.code})`" :value="eq.id" />
        </el-select>
        <el-date-picker
          v-model="query.dateStart" type="date" placeholder="开始日期" value-format="YYYY-MM-DD" style="width: 160px"
        />
        <span class="date-sep">至</span>
        <el-date-picker
          v-model="query.dateEnd" type="date" placeholder="结束日期" value-format="YYYY-MM-DD" style="width: 160px"
        />
        <div class="ai-filter-actions">
          <el-button type="primary" @click="handleSearch">查询</el-button>
          <el-button @click="resetQuery">重置</el-button>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="data-card">
      <el-table :data="records" v-loading="loading" stripe style="width: 100%">
        <el-table-column label="设备" min-width="160">
          <template #default="{ row }"><span class="eq-name">{{ getEquipmentName(row.equipmentId) }}</span></template>
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
    </div>
  </div>
</template>

<style scoped>
.eq-name {
  font-weight: 500;
  color: var(--el-text-color-primary);
}
.date-sep {
  color: var(--el-text-color-secondary);
  line-height: 36px;
}
.empty-hint {
  text-align: center;
  padding: var(--space-10) 0;
  color: var(--el-text-color-secondary);
  font-size: var(--font-md);
}
</style>

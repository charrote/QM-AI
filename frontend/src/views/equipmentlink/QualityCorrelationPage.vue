<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { equipmentLinkApi } from '@/api/equipmentLink'
import { equipmentApi } from '@/api/basicData'
import { View } from '@element-plus/icons-vue'
import type { EquipmentQualityCorrelation } from '@/types/equipmentLink'
import type { Equipment } from '@/types/basicData'

defineOptions({ name: 'QualityCorrelationPage' })

const loading = ref(false)
const correlations = ref<EquipmentQualityCorrelation[]>([])
const equipments = ref<Equipment[]>([])
const detailVisible = ref(false)
const detailData = ref('')

const query = reactive({
  dateStart: '',
  dateEnd: '',
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function getEquipmentName(id: number): string {
  const eq = equipments.value.find(e => e.id === id)
  return eq ? `${eq.name} (${eq.code})` : `设备${id}`
}

function formatCorrelationData(data: string): string {
  try {
    const parsed = JSON.parse(data)
    return JSON.stringify(parsed, null, 2)
  } catch {
    return data
  }
}

function viewCorrelation(row: EquipmentQualityCorrelation) {
  detailData.value = formatCorrelationData(row.correlationData)
  detailVisible.value = true
}

async function loadEquipments() {
  try {
    const res = await equipmentApi.list({ pageSize: 1000 })
    equipments.value = res.items
  } catch (e) {
    console.error('Failed to load equipments', e)
  }
}

function handleSearch() {
  // loadCorrelations() - removed, backend not available
}

function resetQuery() { query.dateStart = ''; query.dateEnd = '' }

onMounted(() => {
  loadEquipments()
})
</script>
<template>
  <div class="page-container">
    <div class="toolbar-row">
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

    <el-table :data="correlations" v-loading="loading" stripe border style="width: 100%" size="small" row-key="id">
      <el-table-column prop="equipmentId" label="设备ID" width="70" />
      <el-table-column label="设备名称" min-width="140">
        <template #default="{ row }">{{ getEquipmentName(row.equipmentId) }}</template>
      </el-table-column>
      <el-table-column prop="analysisDate" label="分析日期" width="120">
        <template #default="{ row }">{{ formatDate(row.analysisDate) }}</template>
      </el-table-column>
      <el-table-column label="关联数据" min-width="200">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="viewCorrelation(row)">
            <el-icon><View /></el-icon> 查看
          </el-button>
          <span v-if="!row.correlationData" class="text-gray-400">无数据</span>
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="160">
        <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
      </el-table-column>
    </el-table>

    <div v-if="!loading && correlations.length === 0" class="empty-hint">
      暂无质量关联数据
    </div>

    <el-dialog v-model="detailVisible" title="关联数据详情" width="700px">
      <pre class="detail-data">{{ detailData }}</pre>
      <template #footer>
        <el-button @click="detailVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>
<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.empty-hint { text-align: center; padding: 40px; color: #909399; font-size: 14px; }
.detail-data {
  margin: 4px 0 0;
  padding: 8px;
  background: #f5f7fa;
  border-radius: 4px;
  font-size: 12px;
  line-height: 1.5;
  color: #303133;
  max-height: 200px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-all;
}
.text-gray-400 { color: #909399; }
</style>

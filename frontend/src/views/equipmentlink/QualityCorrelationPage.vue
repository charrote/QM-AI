<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Connection } from '@element-plus/icons-vue'
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
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon"><el-icon :size="28"><Connection /></el-icon></div>
        <div class="page-header-text">
          <h2>质量关联分析</h2>
          <p>设备参数与质量数据的关联关系分析</p>
        </div>
      </div>
    </div>

    <!-- Toolbar -->
    <el-card shadow="never" class="search-card">
      <div class="search-bar">
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
      <el-table :data="correlations" v-loading="loading" stripe border style="width: 100%" row-key="id">
        <el-table-column label="设备" min-width="150">
          <template #default="{ row }"><span class="equipment-name">{{ getEquipmentName(row.equipmentId) }}</span></template>
        </el-table-column>
        <el-table-column label="分析日期" width="170">
          <template #default="{ row }">{{ formatDate(row.analysisDate) }}</template>
        </el-table-column>
        <el-table-column label="关联数据" min-width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click="viewCorrelation(row)">
              <el-icon><View /></el-icon> 查看详情
            </el-button>
            <span v-if="!row.correlationData" class="text-gray-400">无数据</span>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
      </el-table>

      <div v-if="!loading && correlations.length === 0" class="empty-hint">
        暂无质量关联数据
      </div>
    </el-card>

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

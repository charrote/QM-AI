<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { Connection, View } from '@element-plus/icons-vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'
import { equipmentLinkApi } from '@/api/equipmentLink'
import { equipmentApi } from '@/api/basicData'
import type { EquipmentQualityCorrelation } from '@/types/equipmentLink'
import type { Equipment } from '@/types/basicData'

defineOptions({ name: 'QualityCorrelationPage' })

const loading = ref(false)
const correlations = ref<EquipmentQualityCorrelation[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const equipments = ref<Equipment[]>([])
const detailPanel = useRightPanel()
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
  detailPanel.open()
}

async function loadEquipments() {
  try {
    const res = await equipmentApi.list({ pageSize: 1000 })
    equipments.value = res.items
  } catch (e) {
    console.error('Failed to load equipments', e)
  }
}

async function loadCorrelations() {
  loading.value = true
  try {
    const params: any = { page: page.value, pageSize: pageSize.value }
    if (query.dateStart) params.dateFrom = query.dateStart
    if (query.dateEnd) params.dateTo = query.dateEnd
    const res = await equipmentLinkApi.correlations(params)
    correlations.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load correlations', e)
    ElMessage.error('加载关联数据失败')
    correlations.value = []
  } finally {
    loading.value = false
  }
}

function handleSearch() {
  page.value = 1
  loadCorrelations()
}

function handlePageChange(p: number) {
  page.value = p
  loadCorrelations()
}

function handleSizeChange(s: number) {
  pageSize.value = s
  page.value = 1
  loadCorrelations()
}

function resetQuery() {
  query.dateStart = ''
  query.dateEnd = ''
}

onMounted(() => {
  loadEquipments()
  loadCorrelations()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner (Primary) -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="24"><Connection /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <div class="page-header-banner-title">质量关联分析</div>
          <div class="page-header-banner-subtitle">设备参数与质量数据的关联关系分析</div>
        </div>
      </div>
    </div>

    <!-- Filter Card -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
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
      <el-table :data="correlations" v-loading="loading" stripe style="width: 100%" row-key="id">
        <el-table-column label="设备" min-width="150">
          <template #default="{ row }"><span class="eq-name">{{ getEquipmentName(row.equipmentId) }}</span></template>
        </el-table-column>
        <el-table-column label="分析日期" width="170">
          <template #default="{ row }">{{ formatDate(row.analysisDate) }}</template>
        </el-table-column>
        <el-table-column label="关联数据" min-width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click="viewCorrelation(row)">
              <el-icon><View /></el-icon> 查看详情
            </el-button>
            <span v-if="!row.correlationData" class="text-secondary">无数据</span>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="170">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
      </el-table>
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handleSizeChange"
          @current-change="handlePageChange"
        />
      </div>
      <div v-if="!loading && correlations.length === 0" class="empty-hint">
        暂无质量关联数据
      </div>
    </div>

    <!-- Panel: Detail -->
    <RightPanel v-model:visible="detailPanel.visible" title="关联数据详情" :width="700">
      <template #body>
        <pre class="detail-data">{{ detailData }}</pre>
      </template>
      <template #footer>
        <el-button @click="detailPanel.close()">关闭</el-button>
      </template>
    </RightPanel>
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
.text-secondary {
  color: var(--el-text-color-secondary);
}
.empty-hint {
  text-align: center;
  padding: var(--space-10) 0;
  color: var(--el-text-color-secondary);
  font-size: var(--font-md);
}
.detail-data {
  margin: 4px 0 0;
  padding: 12px;
  background: var(--el-fill-color-lighter);
  border-radius: var(--radius-md);
  font-size: var(--font-sm);
  line-height: var(--leading-relaxed);
  color: var(--el-text-color-primary);
  max-height: 300px;
  overflow: auto;
  white-space: pre-wrap;
  word-break: break-all;
}
</style>

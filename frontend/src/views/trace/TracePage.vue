<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { traceApi } from '@/api/trace'
import type { TraceResult } from '@/types/trace'
import { TRACE_METHOD_OPTIONS } from '@/types/trace'

defineOptions({ name: 'TracePage' })

const router = useRouter()
const method = ref<'sn' | 'batch' | 'equipment'>('sn')
const snInput = ref('')
const batchInput = ref('')
const equipmentInput = ref('')
const loading = ref(false)
const result = ref<TraceResult | null>(null)
const error = ref('')

async function trace() {
  loading.value = true
  result.value = null
  error.value = ''
  try {
    if (method.value === 'sn') {
      if (!snInput.value.trim()) { ElMessage.warning('请输入SN编码'); return }
      result.value = await traceApi.traceBySn(snInput.value.trim())
    } else if (method.value === 'batch') {
      if (!batchInput.value.trim()) { ElMessage.warning('请输入批次号'); return }
      result.value = await traceApi.traceByBatch(batchInput.value.trim())
    } else {
      if (!equipmentInput.value.trim()) { ElMessage.warning('请输入设备ID或编码'); return }
      result.value = await traceApi.traceByEquipment(equipmentInput.value.trim())
    }
  } catch (e: any) {
    error.value = e?.response?.data?.message || '追溯失败，请检查输入'
    ElMessage.error(error.value)
  } finally {
    loading.value = false
  }
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function goToDiffusion() {
  if (result.value?.batch?.code) {
    router.push({ name: 'NgDiffusion', query: { batchCode: result.value.batch.code } })
  } else {
    router.push({ name: 'NgDiffusion' })
  }
}

function goToRecall() {
  if (result.value?.batch?.code) {
    router.push({ name: 'RecallSimulation', query: { batchCode: result.value.batch.code } })
  } else {
    router.push({ name: 'RecallSimulation' })
  }
}

onMounted(() => {})
</script>

<template>
  <div class="page-container">
    <!-- Search Panel -->
    <el-card class="search-card" shadow="never">
      <template #header>
        <div class="card-header">
          <span>质量追溯查询</span>
          <el-tag size="small" type="info">6 阶段全流程追溯</el-tag>
        </div>
      </template>
      <div class="search-methods">
        <el-radio-group v-model="method" size="large">
          <el-radio-button v-for="opt in TRACE_METHOD_OPTIONS" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </el-radio-button>
        </el-radio-group>
      </div>
      <div class="search-input-row">
        <template v-if="method === 'sn'">
          <el-input v-model="snInput" placeholder="请输入 SN 编码" size="large" clearable @keyup.enter="trace" style="max-width: 400px">
            <template #prefix><el-icon><Search /></el-icon></template>
          </el-input>
        </template>
        <template v-else-if="method === 'batch'">
          <el-input v-model="batchInput" placeholder="请输入批次号" size="large" clearable @keyup.enter="trace" style="max-width: 400px">
            <template #prefix><el-icon><Box /></el-icon></template>
          </el-input>
        </template>
        <template v-else>
          <el-input v-model="equipmentInput" placeholder="请输入设备 ID 或编码" size="large" clearable @keyup.enter="trace" style="max-width: 400px">
            <template #prefix><el-icon><Monitor /></el-icon></template>
          </el-input>
        </template>
        <el-button type="primary" size="large" @click="trace" :loading="loading">
          <el-icon><Search /></el-icon> 追溯
        </el-button>
      </div>
      <div class="search-hints">
        <el-text type="info" size="small">💡 输入 SN / 批次号 / 设备编码进行 6 阶段质量追溯：来料→领料→加工→检验→批次→出货</el-text>
      </div>
      <div v-if="error" class="error-msg">
        <el-alert :title="error" type="error" :closable="false" show-icon />
      </div>
    </el-card>

    <!-- Results -->
    <div v-if="result" class="result-container">
      <!-- Product Info Card -->
      <el-card class="info-card" shadow="never">
        <div class="info-row">
          <div class="info-item">
            <span class="info-label">产品</span>
            <span class="info-value">{{ result.product?.name || '-' }} ({{ result.product?.code || '-' }})</span>
          </div>
          <div class="info-item">
            <span class="info-label">规格</span>
            <span class="info-value">{{ result.product?.specification || '-' }}</span>
          </div>
          <div class="info-item" v-if="result.batch">
            <span class="info-label">批次</span>
            <span class="info-value">{{ result.batch.code }} — {{ result.batch.quantity }} pcs</span>
          </div>
          <div class="info-item" v-if="result.equipment">
            <span class="info-label">设备</span>
            <span class="info-value">{{ result.equipment.name }} ({{ result.equipment.code }})</span>
          </div>
        </div>
      </el-card>

      <!-- Material Chain -->
      <el-card v-if="result.materialChain?.length" class="timeline-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span>阶段一：来料追溯</span>
            <el-tag size="small" type="primary">物料链</el-tag>
          </div>
        </template>
        <el-table :data="result.materialChain" stripe size="small">
          <el-table-column prop="materialCode" label="物料代码" width="120" />
          <el-table-column prop="materialName" label="物料名称" min-width="150" />
          <el-table-column prop="batchCode" label="批次号" width="120" />
          <el-table-column prop="supplier" label="供应商" width="120" />
          <el-table-column label="数量" width="80" align="right">
            <template #default="{ row }">{{ row.quantity ?? '-' }}</template>
          </el-table-column>
          <el-table-column label="接收日期" width="150">
            <template #default="{ row }">{{ formatDate(row.receivedDate) }}</template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- First Piece -->
      <el-card v-if="result.firstPieces?.length" class="timeline-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span>阶段二：领料 / 首件检验</span>
            <el-tag size="small" type="warning">首件</el-tag>
          </div>
        </template>
        <el-table :data="result.firstPieces" stripe size="small">
          <el-table-column prop="serialNumber" label="SN" width="140" />
          <el-table-column prop="productCode" label="产品代码" width="120" />
          <el-table-column prop="productName" label="产品名称" min-width="120" />
          <el-table-column label="检验时间" width="150">
            <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
          </el-table-column>
          <el-table-column label="结果" width="80">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">{{ row.result }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验人" width="80" />
          <el-table-column prop="items" label="检验项目" min-width="150">
            <template #default="{ row }">
              <el-tag v-for="item in row.items" :key="item" size="small" class="item-tag">{{ item }}</el-tag>
            </template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- Patrol Records -->
      <el-card v-if="result.patrols?.length" class="timeline-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span>阶段三：加工 / 巡检</span>
            <el-tag size="small" type="warning">加工</el-tag>
          </div>
        </template>
        <el-table :data="result.patrols" stripe size="small">
          <el-table-column prop="patrolNo" label="巡检单号" width="140" />
          <el-table-column prop="processName" label="工序" width="120" />
          <el-table-column prop="equipmentName" label="设备" width="120" />
          <el-table-column label="检验时间" width="150">
            <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
          </el-table-column>
          <el-table-column label="结果" width="80">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">{{ row.result }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验人" width="80" />
          <el-table-column label="不良数" width="80" align="right">
            <template #default="{ row }">{{ row.defectCount ?? '-' }}</template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- FQC -->
      <el-card v-if="result.fqcInspections?.length" class="timeline-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span>阶段四：FQC 成品检验</span>
            <el-tag size="small" type="danger">成品</el-tag>
          </div>
        </template>
        <el-table :data="result.fqcInspections" stripe size="small">
          <el-table-column prop="inspectionNo" label="检验单号" width="140" />
          <el-table-column prop="productName" label="产品名称" min-width="120" />
          <el-table-column label="检验时间" width="150">
            <template #default="{ row }">{{ formatDate(row.inspectedAt) }}</template>
          </el-table-column>
          <el-table-column label="结果" width="80">
            <template #default="{ row }">
              <el-tag :type="row.result === 'pass' ? 'success' : 'danger'" size="small">{{ row.result }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="inspector" label="检验人" width="80" />
          <el-table-column label="不良数" width="80" align="right">
            <template #default="{ row }">{{ row.defectCount ?? '-' }}</template>
          </el-table-column>
        </el-table>
      </el-card>

      <!-- OQC -->
      <el-card v-if="result.oqcReleases?.length" class="timeline-card" shadow="never">
        <template #header>
          <div class="card-header">
            <span>阶段六：OQC 出货放行</span>
            <el-tag size="small" type="success">出货</el-tag>
          </div>
        </template>
        <el-table :data="result.oqcReleases" stripe size="small">
          <el-table-column prop="releaseNo" label="放行单号" width="140" />
          <el-table-column prop="productName" label="产品名称" min-width="120" />
          <el-table-column prop="customer" label="客户" width="120" />
          <el-table-column label="放行时间" width="150">
            <template #default="{ row }">{{ formatDate(row.releasedAt) }}</template>
          </el-table-column>
          <el-table-column label="数量" width="80" align="right">
            <template #default="{ row }">{{ row.quantity }} pcs</template>
          </el-table-column>
          <el-table-column prop="releasedBy" label="放行人" width="80" />
        </el-table>
      </el-card>

      <!-- Empty state -->
      <el-empty v-if="
        !result.materialChain?.length && !result.firstPieces?.length &&
        !result.patrols?.length && !result.fqcInspections?.length && !result.oqcReleases?.length
      " description="暂无追溯数据" :image-size="100" />
    </div>

    <!-- Quick Actions -->
    <el-card v-if="result" class="actions-card" shadow="never">
      <template #header>
        <span>追溯分析工具</span>
      </template>
      <div class="actions-row">
        <el-button type="primary" @click="goToDiffusion">
          <el-icon><Connection /></el-icon> NG 扩散分析
        </el-button>
        <el-button type="warning" @click="goToRecall">
          <el-icon><RefreshLeft /></el-icon> 召回模拟
        </el-button>
      </div>
    </el-card>

    <!-- Empty state for no result -->
    <el-card v-else class="empty-card" shadow="never">
      <el-empty description="请输入 SN 编码、批次号或设备编码开始追溯">
        <el-text type="info" size="small">支持 6 阶段全流程追溯：来料 → 领料 → 加工 → 检验 → 批次 → 出货</el-text>
      </el-empty>
    </el-card>
  </div>
</template>

<style scoped>
.page-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow-y: auto;
  gap: 12px;
  padding: 8px 16px;
}
.search-card {
  flex-shrink: 0;
}
.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.search-methods {
  margin-bottom: 16px;
  display: flex;
  justify-content: center;
}
.search-input-row {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.search-hints {
  margin-top: 12px;
}
.error-msg {
  margin-top: 12px;
}
.result-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.info-card {
  flex-shrink: 0;
}
.info-row {
  display: flex;
  flex-wrap: wrap;
  gap: 24px;
}
.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.info-label {
  font-size: 12px;
  color: #909399;
}
.info-value {
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}
.timeline-card {
  flex-shrink: 0;
}
.item-tag {
  margin: 2px 4px 2px 0;
}
.actions-card {
  flex-shrink: 0;
}
.actions-row {
  display: flex;
  gap: 12px;
}
.empty-card {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>

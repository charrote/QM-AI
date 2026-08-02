<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Cpu, CircleCheck, Loading, Warning, Plus } from '@element-plus/icons-vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'
import { aiApi } from '@/api/ai'
import { MODEL_TYPE_OPTIONS, MODEL_TYPE_MAP, MODEL_STATUS_OPTIONS, MODEL_STATUS_MAP } from '@/types/ai'
import type { PagedResult } from '@/types/basicData'

defineOptions({ name: 'ModelManagementPage' })

const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const models = ref<Array<{
  id: number
  name: string
  type: string
  status: string
  accuracy?: number
  trainedAt?: string
  createdAt: string
}>>([])

// Create panel
const createPanel = useRightPanel()
const createForm = reactive({
  name: '',
  type: 'classification',
  trainingData: '',
})

// Detail panel
const detailPanel = useRightPanel()
const currentModel = ref<{
  id: number
  name: string
  type: string
  status: string
  accuracy?: number
  parameters: Record<string, any>
  metrics?: Record<string, number>
  trainedAt?: string
  createdAt: string
} | null>(null)

async function loadModels() {
  try {
    const res: PagedResult<any> = await aiApi.models({
      page: page.value,
      pageSize: pageSize.value,
      status: statusFilter.value || undefined,
    })
    models.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('[ModelManagementPage] Failed to load models:', e)
  }
}

async function handleCreate() {
  if (!createForm.name) {
    ElMessage.warning('请填写模型名称')
    return
  }
  try {
    await aiApi.trainModel({
      name: createForm.name,
      type: createForm.type,
      trainingData: createForm.trainingData || undefined,
    })
    ElMessage.success('模型训练已启动')
    createPanel.close()
    createForm.name = ''
    createForm.type = 'classification'
    createForm.trainingData = ''
    await loadModels()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '创建失败')
  }
}

async function viewDetail(id: number) {
  try {
    currentModel.value = await aiApi.getModel(id)
    detailPanel.open()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '获取详情失败')
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确定删除该模型吗？', '确认', { type: 'warning' })
    await aiApi.deleteModel(id)
    ElMessage.success('已删除')
    await loadModels()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

function getAccuracyClass(accuracy?: number) {
  if (accuracy == null) return ''
  if (accuracy >= 0.9) return 'ai-stat-value--success'
  if (accuracy >= 0.7) return 'ai-stat-value--warning'
  return 'ai-stat-value--danger'
}

function getAccuracyPct(accuracy?: number) {
  if (accuracy == null) return 0
  return Math.round(accuracy * 100)
}

const createFormRef = ref()
const formRules = {
  name: [{ required: true, message: '请输入模型名称', trigger: 'blur' }],
  type: [{ required: true, message: '请选择模型类型', trigger: 'change' }],
}

onMounted(() => {
  loadModels()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="ai-header-banner">
      <div class="ai-header-banner-main">
        <div class="ai-header-banner-icon">
          <el-icon :size="24"><Cpu /></el-icon>
        </div>
        <div class="ai-header-banner-text">
          <div class="ai-header-banner-title">模型管理</div>
          <div class="ai-header-banner-subtitle">AI 模型训练、部署与性能监控</div>
        </div>
      </div>
    </div>

    <!-- Stats Grid -->
    <div class="ai-stat-grid">
      <div class="ai-stat-card ai-stat-card--blue">
        <div class="ai-stat-icon ai-stat-icon--blue">
          <el-icon :size="22"><Cpu /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">模型总数</div>
          <div class="ai-stat-value">{{ models.length }}</div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--green">
        <div class="ai-stat-icon ai-stat-icon--green">
          <el-icon :size="22"><CircleCheck /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">已激活</div>
          <div class="ai-stat-value ai-stat-value--success">
            {{ models.filter(m => m.status === 'active' || m.status === 'trained').length }}
          </div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--blue">
        <div class="ai-stat-icon ai-stat-icon--blue">
          <el-icon :size="22"><Loading /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">训练中</div>
          <div class="ai-stat-value ai-stat-value--primary">
            {{ models.filter(m => m.status === 'training' || m.status === 'pending').length }}
          </div>
        </div>
      </div>
      <div class="ai-stat-card ai-stat-card--gray">
        <div class="ai-stat-icon ai-stat-icon--gray">
          <el-icon :size="22"><Warning /></el-icon>
        </div>
        <div class="ai-stat-content">
          <div class="ai-stat-label">停用/失败</div>
          <div class="ai-stat-value">
            {{ models.filter(m => !['active', 'trained', 'training', 'pending'].includes(m.status)).length }}
          </div>
        </div>
      </div>
    </div>

    <!-- Toolbar -->
    <div class="ai-filter-card">
      <div class="ai-filter-row">
        <el-select v-model="statusFilter" placeholder="模型状态" clearable style="width: 130px" @change="loadModels">
          <el-option v-for="opt in MODEL_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <div class="ai-filter-actions">
          <el-button type="primary" @click="createPanel.open()">
            <el-icon><Plus /></el-icon> 新建模型
          </el-button>
          <el-button @click="loadModels">刷新</el-button>
        </div>
      </div>
    </div>

    <!-- Table -->
    <div class="table-card">
      <div class="table-card__body">
        <el-table :data="models" stripe style="width: 100%">
          <el-table-column prop="name" label="模型名称" min-width="150" show-overflow-tooltip />
          <el-table-column label="类型" width="130" align="center">
            <template #default="{ row }">
              <el-tag size="small" effect="plain">{{ MODEL_TYPE_MAP[row.type] || row.type }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column label="状态" width="120" align="center">
            <template #default="{ row }">
              <div class="model-status">
                <span :class="['model-status__dot', `model-status__dot--${row.status}`]"></span>
                <el-tag :type="MODEL_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="dark">
                  {{ MODEL_STATUS_MAP[row.status] || row.status }}
                </el-tag>
              </div>
            </template>
          </el-table-column>
          <el-table-column label="准确率" width="140" align="center">
            <template #default="{ row }">
              <div v-if="row.accuracy != null" class="confidence-bar">
                <div class="confidence-bar__track">
                  <div :class="[
                    'confidence-bar__fill',
                    row.accuracy >= 0.9 ? 'confidence-bar__fill--high' :
                    row.accuracy >= 0.7 ? 'confidence-bar__fill--med' : 'confidence-bar__fill--low'
                  ]" :style="{ width: (row.accuracy * 100) + '%' }"></div>
                </div>
                <span class="confidence-bar__value">{{ getAccuracyPct(row.accuracy) }}%</span>
              </div>
              <span v-else class="text-secondary">-</span>
            </template>
          </el-table-column>
          <el-table-column label="训练时间" width="160" align="center">
            <template #default="{ row }">{{ formatDate(row.trainedAt) }}</template>
          </el-table-column>
          <el-table-column label="创建时间" width="160" align="center">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="150" align="center" fixed="right">
            <template #default="{ row }">
              <el-button link size="small" type="primary" @click="viewDetail(row.id)">详情</el-button>
              <el-button link size="small" type="danger" @click="handleDelete(row.id)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
      <div class="table-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="loadModels"
          @current-change="loadModels"
        />
      </div>
    </div>

    <!-- Panel: Create -->
    <RightPanel v-model:visible="createPanel.visible" title="新建模型" :width="520">
      <template #body>
        <el-form :model="createForm" label-width="100px" :rules="formRules" ref="createFormRef">
          <el-divider content-position="left">基本信息</el-divider>
          <el-form-item label="模型名称" prop="name" required>
            <el-input v-model="createForm.name" placeholder="如: 缺陷分类模型-v2" />
          </el-form-item>
          <el-form-item label="模型类型" prop="type" required>
            <el-select v-model="createForm.type" style="width: 100%">
              <el-option v-for="opt in MODEL_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
          <el-divider content-position="left">训练配置</el-divider>
          <el-form-item label="训练数据">
            <el-input v-model="createForm.trainingData" type="textarea" :rows="3" placeholder="训练数据集描述或路径（可选）" />
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="createPanel.close()">取消</el-button>
        <el-button type="primary" @click="handleCreate">创建</el-button>
      </template>
    </RightPanel>

    <!-- Panel: Detail -->
    <RightPanel v-model:visible="detailPanel.visible" :title="currentModel?.name || '模型详情'" :width="640">
      <template #body>
        <template v-if="currentModel">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="模型名称">{{ currentModel.name }}</el-descriptions-item>
            <el-descriptions-item label="类型">{{ MODEL_TYPE_MAP[currentModel.type] || currentModel.type }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <div class="model-status">
                <span :class="['model-status__dot', `model-status__dot--${(currentModel as any).status}`]"></span>
                <el-tag :type="(currentModel as any)?.status ? (MODEL_STATUS_OPTIONS.find(o => o.value === (currentModel as any).status)?.type || 'info') : 'info'" size="small">
                  {{ MODEL_STATUS_MAP[(currentModel as any).status] || (currentModel as any).status }}
                </el-tag>
              </div>
            </el-descriptions-item>
            <el-descriptions-item label="准确率">
              {{ currentModel.accuracy != null ? (currentModel.accuracy * 100).toFixed(1) + '%' : '-' }}
            </el-descriptions-item>
            <el-descriptions-item label="训练时间">{{ formatDate(currentModel.trainedAt) }}</el-descriptions-item>
            <el-descriptions-item label="创建时间">{{ formatDate(currentModel.createdAt) }}</el-descriptions-item>
          </el-descriptions>

          <el-divider content-position="left">参数</el-divider>
          <div v-if="Object.keys(currentModel.parameters).length > 0" class="quick-info">
            <div v-for="(val, key) in currentModel.parameters" :key="key" class="quick-info__item">
              <div class="quick-info__label">{{ key }}</div>
              <div class="quick-info__value">{{ typeof val === 'object' ? JSON.stringify(val) : String(val) }}</div>
            </div>
          </div>
          <el-empty v-else description="无参数" :image-size="40" />

          <el-divider content-position="left">指标</el-divider>
          <div v-if="currentModel.metrics && Object.keys(currentModel.metrics).length > 0" class="quick-info">
            <div v-for="(val, key) in currentModel.metrics" :key="key" class="quick-info__item">
              <div class="quick-info__label">{{ key }}</div>
              <div class="quick-info__value">{{ typeof val === 'number' ? val.toFixed(4) : String(val) }}</div>
            </div>
          </div>
          <el-empty v-else description="无指标" :image-size="40" />
        </template>
      </template>
      <template #footer>
        <el-button @click="detailPanel.close()">关闭</el-button>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.text-secondary {
  color: var(--el-text-color-secondary);
}
</style>

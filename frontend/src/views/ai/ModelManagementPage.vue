<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Cpu } from '@element-plus/icons-vue'
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

// Create dialog
const createVisible = ref(false)
const createForm = reactive({
  name: '',
  type: 'classification',
  trainingData: '',
})

// Detail dialog
const detailVisible = ref(false)
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
    console.error('Failed to load models', e)
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
    createVisible.value = false
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
    detailVisible.value = true
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
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Cpu /></el-icon>
        </div>
        <div class="page-header-text">
          <h2>模型管理</h2>
          <p>AI 模型训练、部署与性能监控</p>
        </div>
      </div>
    </div>

    <!-- Model Status Indicators -->
    <el-row :gutter="12" class="stat-row">
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value">{{ models.length }}</div>
          <div class="stat-label">模型总数</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value stat-active">{{ models.filter(m => m.status === 'active' || m.status === 'trained').length }}</div>
          <div class="stat-label">已激活</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value stat-training">{{ models.filter(m => m.status === 'training' || m.status === 'pending').length }}</div>
          <div class="stat-label">训练中</div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-value stat-inactive">{{ models.filter(m => m.status !== 'active' && m.status !== 'trained' && m.status !== 'training' && m.status !== 'pending').length }}</div>
          <div class="stat-label">停用/失败</div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Toolbar -->
    <el-card shadow="never" class="toolbar-card">
      <div class="toolbar-row">
        <el-select v-model="statusFilter" placeholder="模型状态" clearable style="width: 130px" @change="loadModels">
          <el-option v-for="opt in MODEL_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
        </el-select>
        <el-button type="primary" @click="createVisible = true">+ 新建模型</el-button>
        <el-button @click="loadModels">刷新</el-button>
      </div>
    </el-card>

    <!-- Table -->
    <el-card shadow="never" class="table-card">
      <el-table :data="models" stripe style="width: 100%">
        <el-table-column prop="name" label="模型名称" min-width="150" show-overflow-tooltip />
        <el-table-column label="类型" width="130">
          <template #default="{ row }">
            <el-tag size="small" effect="plain">{{ MODEL_TYPE_MAP[row.type] || row.type }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="110">
          <template #default="{ row }">
            <div class="model-status">
              <span class="status-dot" :class="`dot-${row.status}`"></span>
              <el-tag :type="MODEL_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="dark">
                {{ MODEL_STATUS_MAP[row.status] || row.status }}
              </el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="准确率" width="120" align="center">
          <template #default="{ row }">
            <div v-if="row.accuracy != null" class="accuracy-cell">
              <el-progress
                :percentage="(row.accuracy * 100).toFixed(0)"
                :stroke-width="8"
                :color="row.accuracy >= 0.9 ? '#67C23A' : row.accuracy >= 0.7 ? '#E6A23C' : '#F56C6C'"
              />
              <span class="accuracy-text">{{ (row.accuracy * 100).toFixed(1) }}%</span>
            </div>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="训练时间" width="160">
          <template #default="{ row }">{{ formatDate(row.trainedAt) }}</template>
        </el-table-column>
        <el-table-column label="创建时间" width="160">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="viewDetail(row.id)">详情</el-button>
            <el-button link size="small" type="danger" @click="handleDelete(row.id)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>

      <div class="pagination-row">
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
    </el-card>

    <!-- Create Dialog -->
    <el-dialog v-model="createVisible" title="新建模型" width="520px" :close-on-click-modal="false">
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
      <template #footer>
        <el-button @click="createVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCreate">创建</el-button>
      </template>
    </el-dialog>

    <!-- Detail Dialog -->
    <el-dialog v-model="detailVisible" :title="currentModel?.name || '模型详情'" width="640px">
      <template v-if="currentModel">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="模型名称">{{ currentModel.name }}</el-descriptions-item>
          <el-descriptions-item label="类型">{{ MODEL_TYPE_MAP[currentModel.type] || currentModel.type }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <div class="model-status">
              <span class="status-dot" :class="`dot-${(currentModel as any).status}`"></span>
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
        <div v-if="Object.keys(currentModel.parameters).length > 0" class="params-grid">
          <div v-for="(val, key) in currentModel.parameters" :key="key" class="param-item">
            <span class="param-key">{{ key }}</span>
            <span class="param-value">{{ typeof val === 'object' ? JSON.stringify(val) : String(val) }}</span>
          </div>
        </div>
        <el-empty v-else description="无参数" :image-size="40" />

        <el-divider content-position="left">指标</el-divider>
        <div v-if="currentModel.metrics && Object.keys(currentModel.metrics).length > 0" class="params-grid">
          <div v-for="(val, key) in currentModel.metrics" :key="key" class="param-item">
            <span class="param-key">{{ key }}</span>
            <span class="param-value">{{ typeof val === 'number' ? val.toFixed(4) : String(val) }}</span>
          </div>
        </div>
        <el-empty v-else description="无指标" :image-size="40" />
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
.stat-row { margin-bottom: 12px; }
.stat-card { text-align: center; border-radius: 8px; transition: all 0.2s; }
.stat-card:hover { transform: translateY(-2px); }
.stat-value { font-size: 28px; font-weight: 700; }
.stat-active { color: #67c23a; }
.stat-training { color: #409eff; }
.stat-inactive { color: #909399; }
.stat-label { font-size: 13px; color: #909399; margin-top: 4px; }
.toolbar-card { margin-bottom: 12px; }
.toolbar-row { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.table-card { flex: 1; display: flex; flex-direction: column; }
.table-card >>> .el-card__body { flex: 1; display: flex; flex-direction: column; padding: 0; }
.table-card >>> .el-table { flex: 1; }
.pagination-row { display: flex; justify-content: flex-end; padding: 12px 8px; border-top: 1px solid #f0f0f0; }
.model-status { display: flex; align-items: center; gap: 6px; }
.status-dot { width: 8px; height: 8px; border-radius: 50%; }
.dot-active { background: #67c23a; }
.dot-trained { background: #67c23a; }
.dot-training { background: #409eff; animation: pulse 1.5s infinite; }
.dot-pending { background: #409eff; animation: pulse 1.5s infinite; }
.dot-inactive { background: #909399; }
.dot-failed { background: #f56c6c; }
.accuracy-cell { display: flex; align-items: center; gap: 6px; }
.accuracy-text { font-size: 12px; color: #909399; white-space: nowrap; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.3; } }
.params-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 8px; }
.param-item { display: flex; flex-direction: column; padding: 8px; background: var(--el-fill-color-light); border-radius: 4px; }
.param-key { font-size: 12px; color: var(--el-text-color-secondary); margin-bottom: 4px; }
.param-value { font-size: 13px; color: var(--el-text-color-primary); word-break: break-all; }
</style>

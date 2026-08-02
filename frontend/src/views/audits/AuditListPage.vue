<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Edit, Plus, ArrowRight } from '@element-plus/icons-vue'
import { auditApi } from '@/api/audit'
import type { PagedResult } from '@/types/basicData'
import type { Audit, AuditFinding } from '@/types/audit'
import {
  AUDIT_TYPE_OPTIONS, AUDIT_STATUS_OPTIONS,
  AUDIT_STATUS_MAP, FINDING_TYPE_OPTIONS, FINDING_STATUS_OPTIONS,
  FINDING_TYPE_MAP, FINDING_STATUS_MAP, SEVERITY_OPTIONS, SEVERITY_MAP,
} from '@/types/audit'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'

defineOptions({ name: 'AuditListPage' })

const router = useRouter()

const searchKeyword = ref('')
const auditTypeFilter = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const audits = ref<Audit[]>([])

// ─── 新建/编辑面板 ─────────────────────────────────
const { visible: panelVisible, open: openPanel, close: closePanel } = useRightPanel()
const isEditing = ref(false)
const currentId = ref<number | null>(null)
const panelTitle = computed(() => isEditing.value ? '编辑审核' : '新建审核')

const form = reactive({
  auditCode: '',
  auditType: 'internal',
  title: '',
  description: '',
  startDate: '',
  endDate: '',
  auditorId: 0,
  scope: '',
  status: 'planned',
})

// ─── 详情面板 ──────────────────────────────────────
const { visible: detailVisible, open: openDetail, close: closeDetail } = useRightPanel()
const currentDetailAudit = ref<Audit | null>(null)
const detailFindings = ref<AuditFinding[]>([])
const detailLoading = ref(false)

const detailTitle = computed(() => currentDetailAudit.value?.auditCode || '审核详情')

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadAudits() {
  try {
    const res = await auditApi.list({
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
      auditType: auditTypeFilter.value || undefined,
      status: statusFilter.value || undefined,
    })
    audits.value = res.items
    total.value = res.total
  } catch (e) {
    console.error('Failed to load audits', e)
  }
}

function openCreate() {
  isEditing.value = false
  currentId.value = null
  form.auditCode = ''
  form.auditType = 'internal'
  form.title = ''
  form.description = ''
  form.startDate = ''
  form.endDate = ''
  form.auditorId = 0
  form.scope = ''
  form.status = 'planned'
  openPanel()
}

function openEdit(row: Audit) {
  isEditing.value = true
  currentId.value = row.id
  form.auditCode = row.auditCode
  form.auditType = row.auditType
  form.title = row.title
  form.description = row.description || ''
  form.startDate = row.startDate?.slice(0, 10) || ''
  form.endDate = row.endDate?.slice(0, 10) || ''
  form.auditorId = row.auditorId
  form.scope = row.scope || ''
  openPanel()
}

async function saveAudit() {
  if (!form.auditCode || !form.title) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    if (isEditing.value && currentId.value) {
      await auditApi.update(currentId.value, {
        auditType: form.auditType,
        title: form.title,
        description: form.description,
        startDate: form.startDate,
        endDate: form.endDate,
        auditorId: form.auditorId,
        scope: form.scope,
      })
      ElMessage.success('审核已更新')
    } else {
      await auditApi.create({
        auditCode: form.auditCode,
        auditType: form.auditType,
        title: form.title,
        description: form.description,
        startDate: form.startDate,
        endDate: form.endDate,
        auditorId: form.auditorId,
        scope: form.scope,
      })
      ElMessage.success('审核已创建')
    }
    closePanel()
    await loadAudits()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteAudit(row: Audit) {
  try {
    await ElMessageBox.confirm(`确定删除审核「${row.auditCode}」吗？`, '确认', { type: 'warning' })
    await auditApi.remove(row.id)
    ElMessage.success('已删除')
    await loadAudits()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

async function viewDetail(row: Audit) {
  detailLoading.value = true
  try {
    const audit = await auditApi.getById(row.id)
    currentDetailAudit.value = audit
    detailFindings.value = await auditApi.findings(row.id)
    openDetail()
  } catch (e: any) {
    console.error('Failed to load audit detail', e)
    ElMessage.error(e?.response?.data?.message || '加载审核详情失败')
  } finally {
    detailLoading.value = false
  }
}

function goToFindings() {
  closeDetail()
  router.push({ name: 'Finding', query: { auditId: String(currentDetailAudit.value!.id) } })
}

onMounted(async () => {
  await loadAudits()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="24"><Edit /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">审核管理</h2>
          <p class="page-header-banner-subtitle">质量审核计划与记录管理</p>
        </div>
      </div>
      <div>
        <el-button :icon="Plus" type="primary" @click="openCreate">新建审核</el-button>
      </div>
    </div>

    <!-- Data Card -->
    <div class="data-card">
      <!-- Card Header -->
      <div class="data-card__header">
        <div class="data-card__title">
          审核列表
          <el-tag size="small" effect="plain">{{ total }} 条</el-tag>
        </div>
        <div class="data-card__extra">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索代码/标题/范围..."
            clearable
            style="width: 220px"
            @keyup.enter="loadAudits"
          />
          <el-select v-model="auditTypeFilter" placeholder="审核类型" clearable style="width: 120px" @change="loadAudits">
            <el-option v-for="opt in AUDIT_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 120px" @change="loadAudits">
            <el-option v-for="opt in AUDIT_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-button @click="loadAudits">刷新</el-button>
        </div>
      </div>

      <!-- Card Body -->
      <div class="data-card__body">
        <el-table :data="audits" stripe class="data-card__table" size="small">
          <el-table-column prop="auditCode" label="审核代码" width="150" />
          <el-table-column label="审核类型" width="110" align="center">
            <template #default="{ row }">
              <el-tag size="small" effect="plain">{{ AUDIT_TYPE_OPTIONS.find(o => o.value === row.auditType)?.label || row.auditType }}</el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="title" label="标题" min-width="220" show-overflow-tooltip />
          <el-table-column label="审核人ID" width="90" align="center">
            <template #default="{ row }">{{ row.auditorId || '-' }}</template>
          </el-table-column>
          <el-table-column prop="scope" label="范围" min-width="160" show-overflow-tooltip />
          <el-table-column label="状态" width="110" align="center">
            <template #default="{ row }">
              <el-tag :type="AUDIT_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
                {{ AUDIT_STATUS_MAP[row.status] || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="开始日期" width="120" align="center">
            <template #default="{ row }">{{ row.startDate?.slice(0, 10) || '-' }}</template>
          </el-table-column>
          <el-table-column label="结束日期" width="120" align="center">
            <template #default="{ row }">{{ row.endDate?.slice(0, 10) || '-' }}</template>
          </el-table-column>
          <el-table-column label="创建时间" width="180">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="200" align="center" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="openEdit(row)">编辑</el-button>
              <el-button size="small" type="primary" link @click.stop="viewDetail(row)">详情</el-button>
              <el-popconfirm title="确认删除此审核？" confirm-button-text="删除" cancel-button-text="取消" @confirm="deleteAudit(row)">
                <template #reference><el-button size="small" type="danger" link @click.stop>删除</el-button></template>
              </el-popconfirm>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <!-- Card Footer -->
      <div class="data-card__footer">
        <el-pagination
          v-model:current-page="page"
          v-model:page-size="pageSize"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          size="small"
          @size-change="loadAudits"
          @current-change="loadAudits"
        />
      </div>
    </div>

    <!-- 新建/编辑面板 -->
    <RightPanel v-model:visible="panelVisible" :title="panelTitle">
      <template #body>
        <el-form :model="form" label-width="100px">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="审核代码" required>
                <el-input v-model="form.auditCode" placeholder="如: AUD-2026-001" :disabled="isEditing" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="审核类型" required>
                <el-select v-model="form.auditType" style="width: 100%">
                  <el-option v-for="opt in AUDIT_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="标题" required>
            <el-input v-model="form.title" placeholder="审核标题" />
          </el-form-item>
          <el-form-item label="描述">
            <el-input v-model="form.description" type="textarea" :rows="3" placeholder="审核描述" />
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="开始日期">
                <el-date-picker v-model="form.startDate" type="date" style="width: 100%" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="结束日期">
                <el-date-picker v-model="form.endDate" type="date" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="审核人ID">
                <el-input v-model="form.auditorId" placeholder="审核人ID" type="number" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="范围">
                <el-input v-model="form.scope" placeholder="审核范围" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closePanel">取消</el-button>
        <el-button type="primary" @click="saveAudit">保存</el-button>
      </template>
    </RightPanel>

    <!-- 审核详情面板 -->
    <RightPanel v-model:visible="detailVisible" :title="detailTitle">
      <template #body>
        <div v-loading="detailLoading" style="display: flex; flex-direction: column; gap: 16px;">
          <!-- 审核信息 -->
          <el-card v-if="currentDetailAudit" shadow="never" class="detail-card">
            <template #header>
              <div class="card-header">
                <span>审核信息</span>
                <el-tag :type="AUDIT_STATUS_OPTIONS.find(o => o.value === currentDetailAudit!.status)?.type || 'info'" size="default">
                  {{ AUDIT_STATUS_MAP[currentDetailAudit!.status] || currentDetailAudit!.status }}
                </el-tag>
              </div>
            </template>
            <el-descriptions :column="1" border size="small">
              <el-descriptions-item label="审核代码">{{ currentDetailAudit.auditCode }}</el-descriptions-item>
              <el-descriptions-item label="审核类型">
                {{ AUDIT_TYPE_OPTIONS.find(o => o.value === currentDetailAudit.auditType)?.label || currentDetailAudit.auditType }}
              </el-descriptions-item>
              <el-descriptions-item label="标题">{{ currentDetailAudit.title }}</el-descriptions-item>
              <el-descriptions-item label="描述">{{ currentDetailAudit.description || '-' }}</el-descriptions-item>
              <el-descriptions-item label="开始日期">{{ currentDetailAudit.startDate?.slice(0, 10) || '-' }}</el-descriptions-item>
              <el-descriptions-item label="结束日期">{{ currentDetailAudit.endDate?.slice(0, 10) || '-' }}</el-descriptions-item>
              <el-descriptions-item label="审核人ID">{{ currentDetailAudit.auditorId || '-' }}</el-descriptions-item>
              <el-descriptions-item label="范围">{{ currentDetailAudit.scope || '-' }}</el-descriptions-item>
              <el-descriptions-item label="创建时间">{{ formatDate(currentDetailAudit.createdAt) }}</el-descriptions-item>
              <el-descriptions-item label="更新时间">{{ formatDate(currentDetailAudit.updatedAt) }}</el-descriptions-item>
            </el-descriptions>
          </el-card>

          <!-- 不符合项 -->
          <el-card v-if="currentDetailAudit" shadow="never" class="detail-card">
            <template #header>
              <div class="card-header">
                <span>不符合项 ({{ detailFindings.length }})</span>
                <el-button size="small" type="primary" text @click="goToFindings">
                  管理不符合项 <el-icon><ArrowRight /></el-icon>
                </el-button>
              </div>
            </template>
            <div class="data-card-inner">
              <el-table :data="detailFindings" stripe size="small">
                <el-table-column prop="id" label="ID" width="50" align="center" />
                <el-table-column label="类型" width="80" align="center">
                  <template #default="{ row }">
                    <el-tag :type="FINDING_TYPE_OPTIONS.find(o => o.value === row.findingType)?.type || 'info'" size="small">
                      {{ FINDING_TYPE_MAP[row.findingType] || row.findingType }}
                    </el-tag>
                  </template>
                </el-table-column>
                <el-table-column label="严重程度" width="80" align="center">
                  <template #default="{ row }">
                    <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small">
                      {{ SEVERITY_MAP[row.severity] || row.severity }}
                    </el-tag>
                  </template>
                </el-table-column>
                <el-table-column prop="description" label="描述" min-width="160" show-overflow-tooltip />
                <el-table-column label="状态" width="80" align="center">
                  <template #default="{ row }">
                    <el-tag :type="FINDING_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small">
                      {{ FINDING_STATUS_MAP[row.status] || row.status }}
                    </el-tag>
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </el-card>
        </div>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; gap: var(--space-5); }

/* Detail card */
.detail-card { background: var(--el-bg-color); border-radius: var(--radius-lg); border: 1px solid var(--el-border-color-lighter); }
.card-header { display: flex; align-items: center; justify-content: space-between; }
.data-card-inner { background: var(--el-bg-color); border-radius: var(--radius-lg); overflow: hidden; }
.data-card-inner :deep(.el-table th.el-table__cell) { background: var(--el-fill-color-light) !important; }
</style>

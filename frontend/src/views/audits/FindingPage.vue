<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { WarningFilled, Plus } from '@element-plus/icons-vue'
import { auditApi } from '@/api/audit'
import type { PagedResult } from '@/types/basicData'
import type { AuditFinding } from '@/types/audit'
import {
  FINDING_TYPE_OPTIONS, FINDING_STATUS_OPTIONS,
  FINDING_STATUS_MAP, FINDING_TYPE_MAP, SEVERITY_OPTIONS, SEVERITY_MAP,
} from '@/types/audit'
import RightPanel from '@/components/layout/RightPanel.vue'
import { useRightPanel } from '@/composables/useRightPanel'

defineOptions({ name: 'FindingPage' })

const route = useRoute()
const router = useRouter()

const routeAuditId = ref<number | undefined>(route.query.auditId ? Number(route.query.auditId) : undefined)

const searchAuditId = ref('')
const findingTypeFilter = ref('')
const statusFilter = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)

const findings = ref<AuditFinding[]>([])

// ─── 新建面板 ─────────────────────────────────────
const { visible: createVisible, open: openCreatePanel, close: closeCreatePanel } = useRightPanel()
const createForm = reactive({
  auditId: 0,
  findingType: 'non_conformity',
  severity: 'major',
  description: '',
  evidence: '',
  requirementRef: '',
})

// ─── 状态更新面板 ─────────────────────────────────
const { visible: statusVisible, open: openStatusPanel, close: closeStatusPanel } = useRightPanel()
const currentFinding = ref<AuditFinding | null>(null)
const statusForm = reactive({
  status: 'corrected',
  rectificationPlan: '',
  responsibleUserId: 0,
  rectificationDueDate: '',
})

// ─── 验证面板 ─────────────────────────────────────
const { visible: verifyVisible, open: openVerifyPanel, close: closeVerifyPanel } = useRightPanel()
const verifyForm = reactive({
  verifiedBy: 0,
})

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadFindings() {
  try {
    if (routeAuditId.value && routeAuditId.value > 0) {
      // 有 auditId：加载该审核下的不符合项
      const res = await auditApi.findingsList(routeAuditId.value, {
        findingType: findingTypeFilter.value || undefined,
        status: statusFilter.value || undefined,
      })
      findings.value = res
      total.value = res.length
    } else {
      // 无 auditId：加载所有不符合项（可按审核ID搜索）
      const searchId = searchAuditId.value ? Number(searchAuditId.value) : undefined
      const all = await auditApi.allFindings({
        findingType: findingTypeFilter.value || undefined,
        status: statusFilter.value || undefined,
      })
      findings.value = searchId ? all.filter(f => f.auditId === searchId) : all
      total.value = findings.value.length
    }
  } catch (e) {
    console.error('Failed to load findings', e)
  }
}

function openCreate() {
  createForm.auditId = routeAuditId.value || 0
  createForm.findingType = 'non_conformity'
  createForm.severity = 'major'
  createForm.description = ''
  createForm.evidence = ''
  createForm.requirementRef = ''
  openCreatePanel()
}

function openUpdateStatus(row: AuditFinding) {
  currentFinding.value = row
  statusForm.status = row.status || 'open'
  statusForm.rectificationPlan = row.rectificationPlan || ''
  statusForm.responsibleUserId = row.responsibleUserId || 0
  statusForm.rectificationDueDate = row.rectificationDueDate?.slice(0, 10) || ''
  openStatusPanel()
}

function openVerify(row: AuditFinding) {
  currentFinding.value = row
  verifyForm.verifiedBy = 0
  openVerifyPanel()
}

async function createFinding() {
  if (!createForm.auditId || !createForm.description) {
    ElMessage.warning('请填写完整信息')
    return
  }
  try {
    await auditApi.createFinding(createForm.auditId, {
      auditId: createForm.auditId,
      findingType: createForm.findingType,
      severity: createForm.severity,
      description: createForm.description,
      evidence: createForm.evidence,
      requirementRef: createForm.requirementRef,
    })
    ElMessage.success('不符合项已创建')
    closeCreatePanel()
    await loadFindings()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function updateStatus() {
  if (!currentFinding.value) return
  try {
    await auditApi.updateFindingStatus(currentFinding.value.id, {
      status: statusForm.status,
      rectificationPlan: statusForm.rectificationPlan,
      responsibleUserId: statusForm.responsibleUserId || undefined,
      rectificationDueDate: statusForm.rectificationDueDate || undefined,
    })
    ElMessage.success('状态已更新')
    closeStatusPanel()
    await loadFindings()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function verifyFinding() {
  if (!currentFinding.value) return
  try {
    await auditApi.verifyFinding(currentFinding.value.id, {
      verifierId: String(verifyForm.verifiedBy),
      passed: true,
    })
    ElMessage.success('已验证')
    closeVerifyPanel()
    await loadFindings()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteFinding(row: AuditFinding) {
  try {
    await ElMessageBox.confirm(`确定删除该不符合项吗？`, '确认', { type: 'warning' })
    await auditApi.removeFinding(row.id)
    ElMessage.success('已删除')
    await loadFindings()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

onMounted(async () => {
  await loadFindings()
})
</script>

<template>
  <div class="page-container">
    <!-- Page Header Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="24"><WarningFilled /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">不符合项</h2>
          <p class="page-header-banner-subtitle">审核不符合项管理与跟踪</p>
        </div>
      </div>
    </div>

    <!-- Data Card -->
    <div class="data-card">
      <!-- Card Header -->
      <div class="data-card__header">
        <div class="data-card__title">
          不符合项列表
          <el-tag size="small" effect="plain">{{ total }} 条</el-tag>
        </div>
        <div class="data-card__extra">
          <el-input
            v-model="searchAuditId"
            placeholder="按审核ID搜索..."
            clearable
            style="width: 180px"
            @clear="loadFindings"
            @keyup.enter="loadFindings"
          />
          <el-select v-model="findingTypeFilter" placeholder="发现类型" clearable style="width: 120px" @change="loadFindings">
            <el-option v-for="opt in FINDING_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-select v-model="statusFilter" placeholder="状态" clearable style="width: 120px" @change="loadFindings">
            <el-option v-for="opt in FINDING_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
          </el-select>
          <el-button @click="loadFindings">刷新</el-button>
          <div style="flex: 1" />
          <el-button :icon="Plus" type="primary" @click="openCreate">新建不符合项</el-button>
        </div>
      </div>

      <!-- Card Body -->
      <div class="data-card__body">
        <el-table :data="findings" stripe class="data-card__table" size="small">
          <el-table-column prop="id" label="ID" width="60" align="center" />
          <el-table-column prop="auditId" label="审核ID" width="90" align="center" />
          <el-table-column label="发现类型" width="120" align="center">
            <template #default="{ row }">
              <el-tag :type="(FINDING_TYPE_OPTIONS.find(o => o.value === row.findingType)?.type) || 'info'" size="small" effect="plain">
                {{ FINDING_TYPE_MAP[row.findingType] || row.findingType }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="严重程度" width="110" align="center">
            <template #default="{ row }">
              <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small" effect="plain">
                {{ SEVERITY_MAP[row.severity] || row.severity }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="描述" min-width="240" show-overflow-tooltip />
          <el-table-column label="状态" width="110" align="center">
            <template #default="{ row }">
              <el-tag :type="FINDING_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
                {{ FINDING_STATUS_MAP[row.status] || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="责任人" width="90" align="center">
            <template #default="{ row }">{{ row.responsibleUserId || '-' }}</template>
          </el-table-column>
          <el-table-column label="截止日期" width="120" align="center">
            <template #default="{ row }">{{ row.rectificationDueDate?.slice(0, 10) || '-' }}</template>
          </el-table-column>
          <el-table-column label="创建时间" width="170">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
          <el-table-column label="操作" width="240" align="center" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="openUpdateStatus(row)">更新状态</el-button>
              <el-button size="small" type="success" link @click.stop="openVerify(row)">验证</el-button>
              <el-popconfirm title="确认删除该不符合项？" confirm-button-text="删除" cancel-button-text="取消" @confirm="deleteFinding(row)">
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
          @current-change="loadFindings"
        />
      </div>
    </div>

    <!-- 新建不符合项面板 -->
    <RightPanel v-model:visible="createVisible" title="新建不符合项">
      <template #body>
        <el-form :model="createForm" label-width="100px">
          <el-form-item label="审核ID" required>
            <el-input v-model="createForm.auditId" placeholder="审核ID" type="number" />
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="发现类型" required>
                <el-select v-model="createForm.findingType" style="width: 100%">
                  <el-option v-for="opt in FINDING_TYPE_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="严重程度">
                <el-select v-model="createForm.severity" style="width: 100%">
                  <el-option v-for="opt in SEVERITY_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>
          <el-form-item label="描述" required>
            <el-input v-model="createForm.description" type="textarea" :rows="3" placeholder="不符合项描述" />
          </el-form-item>
          <el-form-item label="证据">
            <el-input v-model="createForm.evidence" type="textarea" :rows="2" placeholder="相关证据" />
          </el-form-item>
          <el-form-item label="要求引用">
            <el-input v-model="createForm.requirementRef" placeholder="相关标准要求" />
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closeCreatePanel">取消</el-button>
        <el-button type="primary" @click="createFinding">保存</el-button>
      </template>
    </RightPanel>

    <!-- 更新状态面板 -->
    <RightPanel v-model:visible="statusVisible" title="更新状态">
      <template #body>
        <el-form :model="statusForm" label-width="110px">
          <el-form-item label="状态" required>
            <el-select v-model="statusForm.status" style="width: 100%">
              <el-option v-for="opt in FINDING_STATUS_OPTIONS" :key="opt.value" :label="opt.label" :value="opt.value" />
            </el-select>
          </el-form-item>
          <el-form-item label="整改计划">
            <el-input v-model="statusForm.rectificationPlan" type="textarea" :rows="3" placeholder="整改措施" />
          </el-form-item>
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="责任人">
                <el-input v-model="statusForm.responsibleUserId" placeholder="责任人ID" type="number" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="截止日期">
                <el-date-picker v-model="statusForm.rectificationDueDate" type="date" style="width: 100%" />
              </el-form-item>
            </el-col>
          </el-row>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closeStatusPanel">取消</el-button>
        <el-button type="primary" @click="updateStatus">保存</el-button>
      </template>
    </RightPanel>

    <!-- 验证面板 -->
    <RightPanel v-model:visible="verifyVisible" title="验证不符合项">
      <template #body>
        <el-form :model="verifyForm" label-width="80px">
          <el-form-item label="验证人ID" required>
            <el-input v-model="verifyForm.verifiedBy" placeholder="验证人ID" type="number" />
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <el-button @click="closeVerifyPanel">取消</el-button>
        <el-button type="primary" @click="verifyFinding">验证</el-button>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; gap: var(--space-5); }
</style>

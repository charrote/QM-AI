<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { auditApi } from '@/api/audit'
import type { Audit, AuditFinding } from '@/types/audit'
import {
  AUDIT_TYPE_OPTIONS, AUDIT_STATUS_OPTIONS,
  AUDIT_STATUS_MAP, FINDING_TYPE_OPTIONS, FINDING_STATUS_OPTIONS,
  FINDING_TYPE_MAP, FINDING_STATUS_MAP, SEVERITY_OPTIONS, SEVERITY_MAP,
} from '@/types/audit'

defineOptions({ name: 'AuditDetailPage' })

const route = useRoute()
const router = useRouter()
const auditId = Number(route.query.id)

const audit = ref<Audit | null>(null)
const findings = ref<AuditFinding[]>([])
const loading = ref(false)

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadAudit() {
  if (!auditId) return
  loading.value = true
  try {
    const data = await auditApi.getById(auditId)
    audit.value = data
  } catch (e) {
    console.error('Failed to load audit', e)
    ElMessage.error('加载审核详情失败')
  } finally {
    loading.value = false
  }
}

async function loadFindings() {
  if (!auditId) return
  try {
    findings.value = await auditApi.findings(auditId)
  } catch (e) {
    console.error('Failed to load findings', e)
  }
}

onMounted(async () => {
  await Promise.all([loadAudit(), loadFindings()])
})
</script>

<template>
  <div class="page-container" v-loading="loading">
    <template v-if="audit">
      <!-- Audit Info -->
      <el-card shadow="never" class="detail-card">
        <template #header>
          <div class="card-header">
            <span>审核信息</span>
            <el-tag :type="AUDIT_STATUS_OPTIONS.find(o => o.value === audit!.status)?.type || 'info'">
              {{ AUDIT_STATUS_MAP[audit!.status] || audit!.status }}
            </el-tag>
          </div>
        </template>
        <el-descriptions :column="2" border size="small">
          <el-descriptions-item label="审核代码">{{ audit!.auditCode }}</el-descriptions-item>
          <el-descriptions-item label="审核类型">
            {{ AUDIT_TYPE_OPTIONS.find(o => o.value === audit!.auditType)?.label || audit!.auditType }}
          </el-descriptions-item>
          <el-descriptions-item label="标题" :span="2">{{ audit!.title }}</el-descriptions-item>
          <el-descriptions-item label="描述" :span="2">{{ audit!.description || '-' }}</el-descriptions-item>
          <el-descriptions-item label="开始日期">{{ formatDate(audit!.startDate) }}</el-descriptions-item>
          <el-descriptions-item label="结束日期">{{ formatDate(audit!.endDate) }}</el-descriptions-item>
          <el-descriptions-item label="审核人ID">{{ audit!.auditorId || '-' }}</el-descriptions-item>
          <el-descriptions-item label="范围">{{ audit!.scope || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatDate(audit!.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="更新时间">{{ formatDate(audit!.updatedAt) }}</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <!-- Findings -->
      <el-card shadow="never" class="detail-card">
        <template #header>
          <div class="card-header">
            <span>不符合项 ({{ findings.length }})</span>
            <el-button size="small" type="primary" @click="router.push({ name: 'Finding', query: { auditId: String(auditId) } })">
              管理不符合项
            </el-button>
          </div>
        </template>
        <el-table :data="findings" stripe size="small">
          <el-table-column prop="id" label="ID" width="60" />
          <el-table-column label="类型" width="100">
            <template #default="{ row }">
              <el-tag :type="FINDING_TYPE_OPTIONS.find(o => o.value === row.findingType)?.type || 'info'" size="small">
                {{ FINDING_TYPE_MAP[row.findingType] || row.findingType }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="严重程度" width="90">
            <template #default="{ row }">
              <el-tag :type="SEVERITY_OPTIONS.find(o => o.value === row.severity)?.type || 'info'" size="small">
                {{ SEVERITY_MAP[row.severity] || row.severity }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column prop="evidence" label="证据" min-width="150" show-overflow-tooltip />
          <el-table-column label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="FINDING_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small">
                {{ FINDING_STATUS_MAP[row.status] || row.status }}
              </el-tag>
            </template>
          </el-table-column>
          <el-table-column label="责任人" width="80">
            <template #default="{ row }">{{ row.responsibleUserId || '-' }}</template>
          </el-table-column>
          <el-table-column label="截止日期" width="110">
            <template #default="{ row }">{{ row.rectificationDueDate?.slice(0, 10) || '-' }}</template>
          </el-table-column>
          <el-table-column label="创建时间" width="150">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
        </el-table>
      </el-card>
    </template>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.detail-card { margin-bottom: 16px; }
.card-header { display: flex; align-items: center; justify-content: space-between; }
</style>

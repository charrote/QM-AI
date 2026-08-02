<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { CircleCheck, InfoFilled, AlarmClock, Search, Tools, Monitor, Select, ArrowRight } from '@element-plus/icons-vue'
import { defectApi } from '@/api/defect'
import type { Capa, CapaRootCause, CapaCorrectiveAction, CapaPreventiveAction, CapaVerification } from '@/types/defect'
import {
  CAPA_PHASE_LABELS, CAPA_PHASE_MAP, SEVERITY_MAP, CAPA_STATUS_MAP,
  ACTION_STATUS_OPTIONS, ACTION_STATUS_MAP,
} from '@/types/defect'

defineOptions({ name: 'CapaDetailPage' })

const router = useRouter()
const route = useRoute()
const capaId = ref<number | null>(route.params.id ? Number(route.params.id) : null)
const capa = ref<Capa | null>(null)

if (capaId.value === null) {
  router.push({ name: 'Capa' })
}

const tempMeasureForm = ref({ description: '', executedBy: '', executedAt: new Date().toISOString().slice(0, 10) })
const rootCauseForm = ref({ analysisMethod: '5Why', content: '', rootCauseSummary: '' })
const correctiveForm = ref({ actionDescription: '', responsiblePerson: '', dueDate: '' })
const preventiveForm = ref({ actionDescription: '', responsiblePerson: '', dueDate: '' })
const verificationForm = ref({ verifierId: 0, verificationDate: new Date().toISOString().slice(0, 10), conclusion: '', evidence: '', remarks: '' })

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

const phaseLabel = (phase: number) => CAPA_PHASE_MAP[phase] || String(phase)

// ─── 流程状态计算 ─────────────────────────────────────
const phaseStatus = computed(() => {
  if (!capa.value) return []
  return CAPA_PHASE_LABELS.map(p => ({
    ...p,
    status: p.phase < capa.value.currentPhase ? 'completed' : 
            p.phase === capa.value.currentPhase ? 'active' : 'pending',
  }))
})

const currentPhase = computed(() => phaseStatus.value.find(p => p.status === 'active') || phaseStatus.value[0])
const nextPhase = computed(() => phaseStatus.value.find(p => p.status === 'pending'))

async function loadCapaDetail() {
  if (capaId.value === null) return
  try {
    capa.value = await defectApi.getCapaById(capaId.value)
  } catch (e) {
    ElMessage.error('加载 CAPA 详情失败')
  }
}

async function addTemporaryMeasure() {
  if (!tempMeasureForm.value.description || !tempMeasureForm.value.executedBy) {
    ElMessage.warning('请填写措施描述和执行人')
    return
  }
  try {
    await defectApi.addTemporaryMeasure(capaId.value, { ...tempMeasureForm.value })
    ElMessage.success('临时措施已添加')
    tempMeasureForm.value = { description: '', executedBy: '', executedAt: new Date().toISOString().slice(0, 10) }
    await loadCapaDetail()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '添加失败')
  }
}

async function addRootCause() {
  if (!rootCauseForm.value.content || !rootCauseForm.value.rootCauseSummary) {
    ElMessage.warning('请填写分析内容和根因总结')
    return
  }
  try {
    await defectApi.addRootCause(capaId.value, { ...rootCauseForm.value })
    ElMessage.success('根本原因分析已添加')
    rootCauseForm.value = { analysisMethod: '5Why', content: '', rootCauseSummary: '' }
    await loadCapaDetail()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '添加失败')
  }
}

async function addCorrectiveAction() {
  if (!correctiveForm.value.actionDescription || !correctiveForm.value.responsiblePerson) {
    ElMessage.warning('请填写措施描述和负责人')
    return
  }
  try {
    await defectApi.addCorrectiveAction(capaId.value, { ...correctiveForm.value })
    ElMessage.success('纠正措施已添加')
    correctiveForm.value = { actionDescription: '', responsiblePerson: '', dueDate: '' }
    await loadCapaDetail()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '添加失败')
  }
}

async function addPreventiveAction() {
  if (!preventiveForm.value.actionDescription || !preventiveForm.value.responsiblePerson) {
    ElMessage.warning('请填写措施描述和负责人')
    return
  }
  try {
    await defectApi.addPreventiveAction(capaId.value, { ...preventiveForm.value })
    ElMessage.success('预防措施已添加')
    preventiveForm.value = { actionDescription: '', responsiblePerson: '', dueDate: '' }
    await loadCapaDetail()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '添加失败')
  }
}

async function updateActionStatus(actionId: number, status: string, isCorrective: boolean) {
  try {
    if (isCorrective) {
      await defectApi.updateCorrectiveActionStatus(actionId, status)
    } else {
      await defectApi.updatePreventiveActionStatus(actionId, status)
    }
    ElMessage.success('状态已更新')
    await loadCapaDetail()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function addVerification() {
  if (!verificationForm.value.conclusion) {
    ElMessage.warning('请填写验证结论')
    return
  }
  try {
    await defectApi.addVerification(capaId.value, { ...verificationForm.value })
    ElMessage.success('效果验证已添加')
    verificationForm.value = { verifierId: 0, verificationDate: new Date().toISOString().slice(0, 10), conclusion: '', evidence: '', remarks: '' }
    await loadCapaDetail()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '添加失败')
  }
}

onMounted(loadCapaDetail)
</script>

<template>
  <div class="page-container" v-if="capa">
    <!-- Page Header -->
    <div class="m07-header-banner m07-header-banner--primary">
      <div class="m07-header-banner-main">
        <div class="m07-header-banner-left">
          <div class="m07-header-banner-icon">
            <el-icon :size="24"><CircleCheck /></el-icon>
          </div>
          <div class="m07-header-banner-text">
            <div class="m07-header-banner-title">CAPA 详情</div>
            <div class="m07-header-banner-subtitle">
              <div class="capa-meta">
                <el-tag type="info" size="small">{{ capa.capaCode }}</el-tag>
                <el-tag :type="capa.severity === 'critical' ? 'danger' : capa.severity === 'major' ? 'warning' : 'info'" size="small">{{ SEVERITY_MAP[capa.severity] }}</el-tag>
                <el-tag :type="capa.status === 'open' ? 'danger' : capa.status === 'in_progress' ? 'primary' : 'success'" size="small">{{ CAPA_STATUS_MAP[capa.status] }}</el-tag>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 流程可视化 -->
    <el-card shadow="never" class="m07-flow-card">
      <div class="m07-flow-card__header">
        <span class="m07-flow-card__title"><el-icon><CircleCheck /></el-icon> CAPA 处理流程</span>
        <el-tag :type="capa.status === 'closed' ? 'success' : capa.status === 'in_progress' ? 'primary' : 'info'" size="small">
          {{ CAPA_STATUS_MAP[capa.status] }}
        </el-tag>
      </div>
      <div class="m07-flow-card__body">
        <!-- 流程节点 -->
        <div class="m07-flow-steps">
          <div
            v-for="(phase, index) in phaseStatus"
            :key="phase.phase"
            class="m07-flow-step"
            :class="`m07-flow-step--${phase.status}`"
          >
            <!-- 步骤图标 -->
            <div class="m07-flow-step__icon">
              <el-icon :size="20">
                <CircleCheck v-if="phase.status === 'completed'" />
                <AlarmClock v-else-if="phase.status === 'active'" />
                <Select v-else />
              </el-icon>
            </div>
            <!-- 步骤内容 -->
            <div class="m07-flow-step__content">
              <div class="m07-flow-step__title">{{ phase.label }}</div>
              <div class="m07-flow-step__desc" v-if="phase.status === 'completed'">已完成</div>
              <div class="m07-flow-step__desc" v-else-if="phase.status === 'active'">进行中</div>
              <div class="m07-flow-step__desc" v-else>待进行</div>
            </div>
            <!-- 连接箭头 -->
            <el-icon v-if="index < phaseStatus.length - 1" class="m07-flow-step__arrow" :size="16">
              <ArrowRight />
            </el-icon>
          </div>
        </div>

        <!-- 下一步指引 -->
        <div v-if="nextPhase" class="m07-flow-card__next">
          <el-icon><ArrowRight /></el-icon>
          <span>下一步：<strong>{{ nextPhase.label }}</strong></span>
        </div>
        <div v-else class="m07-flow-card__next m07-flow-card__next--done">
          <el-icon><CircleCheck /></el-icon>
          <span>所有阶段已完成</span>
        </div>
      </div>
    </el-card>

    <!-- Basic Info -->
    <el-card shadow="never" class="m07-info-card">
      <div class="m07-info-card__header">
        <span class="m07-info-card__title"><el-icon><InfoFilled /></el-icon> 基本信息</span>
      </div>
      <div class="m07-info-card__body">
        <el-descriptions :column="3" border>
          <el-descriptions-item label="缺陷ID">{{ capa.defectId }}</el-descriptions-item>
          <el-descriptions-item label="负责人">{{ capa.assignedTo }}</el-descriptions-item>
          <el-descriptions-item label="截止日期">{{ capa.dueDate?.slice(0, 10) || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建人">{{ capa.createdBy }}</el-descriptions-item>
          <el-descriptions-item label="创建时间">{{ formatDate(capa.createdAt) }}</el-descriptions-item>
          <el-descriptions-item label="关闭时间" v-if="capa.closedAt">{{ formatDate(capa.closedAt) }}</el-descriptions-item>
        </el-descriptions>
        <div class="m07-description-block">
          <strong>问题描述：</strong>
          <span>{{ capa.description || '无' }}</span>
        </div>
      </div>
    </el-card>

    <!-- Phase 1: 临时措施 -->
    <el-card shadow="never" class="m07-phase-card" :class="{ 'm07-phase-card--active': capa.currentPhase >= 1 && capa.currentPhase < 2 }">
      <div class="m07-phase-card__header">
        <span class="m07-phase-card__title"><el-icon><AlarmClock /></el-icon> 阶段一：临时措施</span>
        <el-tag :type="capa.currentPhase >= 1 ? 'success' : 'info'" size="small" effect="dark">{{ capa.currentPhase >= 1 ? '已完成' : '待进行' }}</el-tag>
      </div>
      <div class="m07-phase-card__body">
        <div v-if="capa.temporaryMeasures && capa.temporaryMeasures.length > 0">
          <el-table :data="capa.temporaryMeasures" stripe border size="small" class="data-card__table">
            <el-table-column prop="description" label="措施描述" min-width="200" />
            <el-table-column prop="executedBy" label="执行人" width="90" />
            <el-table-column label="执行时间" width="165">
              <template #default="{ row }">{{ formatDate(row.executedAt) }}</template>
            </el-table-column>
          </el-table>
        </div>
        <el-empty v-else description="暂无临时措施" :image-size="60" />
        <div v-if="capa.currentPhase >= 1 && capa.currentPhase < 2" class="m07-phase-form">
          <el-form :model="tempMeasureForm" inline>
            <el-form-item label="措施描述">
              <el-input v-model="tempMeasureForm.description" placeholder="措施描述" class="m07-phase-input" />
            </el-form-item>
            <el-form-item label="执行人">
              <el-input v-model="tempMeasureForm.executedBy" placeholder="执行人" class="m07-phase-input--sm" />
            </el-form-item>
            <el-form-item label="执行日期">
              <el-date-picker v-model="tempMeasureForm.executedAt" type="date" class="m07-phase-input--md" />
            </el-form-item>
            <el-button type="primary" @click="addTemporaryMeasure">添加</el-button>
          </el-form>
        </div>
      </div>
    </el-card>

    <!-- Phase 2: 根本原因分析 -->
    <el-card shadow="never" class="m07-phase-card" :class="{ 'm07-phase-card--active': capa.currentPhase >= 2 && capa.currentPhase < 3 }">
      <div class="m07-phase-card__header">
        <span class="m07-phase-card__title"><el-icon><Search /></el-icon> 阶段二：根本原因分析 / 5Why</span>
        <el-tag :type="capa.currentPhase >= 2 ? 'success' : 'info'" size="small" effect="dark">{{ capa.currentPhase >= 2 ? '已完成' : '待进行' }}</el-tag>
      </div>
      <div class="m07-phase-card__body">
        <div v-if="capa.rootCauses && capa.rootCauses.length > 0">
          <el-table :data="capa.rootCauses" stripe border size="small" class="data-card__table">
            <el-table-column prop="analysisMethod" label="分析方法" width="100" />
            <el-table-column prop="content" label="分析内容" min-width="250" show-overflow-tooltip />
            <el-table-column prop="rootCauseSummary" label="根因总结" min-width="180" show-overflow-tooltip />
            <el-table-column prop="createdBy" label="分析人" width="80" />
            <el-table-column label="时间" width="165">
              <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
            </el-table-column>
          </el-table>
        </div>
        <el-empty v-else description="暂无根本原因分析" :image-size="60" />
        <div v-if="capa.currentPhase >= 2 && capa.currentPhase < 3" class="m07-phase-form">
          <el-form :model="rootCauseForm" inline>
            <el-form-item label="分析方法">
              <el-select v-model="rootCauseForm.analysisMethod" class="m07-phase-input--sm">
                <el-option label="5Why" value="5Why" />
                <el-option label="鱼骨图" value="fishbone" />
                <el-option label="FMEA" value="FMEA" />
                <el-option label="其他" value="other" />
              </el-select>
            </el-form-item>
            <el-form-item label="分析内容">
              <el-input v-model="rootCauseForm.content" placeholder="分析内容" class="m07-phase-input" />
            </el-form-item>
            <el-form-item label="根因总结">
              <el-input v-model="rootCauseForm.rootCauseSummary" placeholder="根因总结" class="m07-phase-input--md" />
            </el-form-item>
            <el-button type="primary" @click="addRootCause">添加分析</el-button>
          </el-form>
        </div>
      </div>
    </el-card>

    <!-- Phase 3: 纠正措施 -->
    <el-card shadow="never" class="m07-phase-card" :class="{ 'm07-phase-card--active': capa.currentPhase >= 3 && capa.currentPhase < 4 }">
      <div class="m07-phase-card__header">
        <span class="m07-phase-card__title"><el-icon><Tools /></el-icon> 阶段三：纠正措施</span>
        <el-tag :type="capa.currentPhase >= 3 ? 'success' : 'info'" size="small" effect="dark">{{ capa.currentPhase >= 3 ? '已完成' : '待进行' }}</el-tag>
      </div>
      <div class="m07-phase-card__body">
        <div v-if="capa.correctiveActions && capa.correctiveActions.length > 0">
          <el-table :data="capa.correctiveActions" stripe border size="small" class="data-card__table">
            <el-table-column prop="actionDescription" label="措施描述" min-width="200" />
            <el-table-column prop="responsiblePerson" label="负责人" width="90" />
            <el-table-column label="截止日期" width="115">
              <template #default="{ row }">{{ row.dueDate?.slice(0, 10) || '-' }}</template>
            </el-table-column>
            <el-table-column label="状态" width="95">
              <template #default="{ row }">
                <el-tag :type="ACTION_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="dark">
                  {{ ACTION_STATUS_MAP[row.status] || row.status }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="remarks" label="备注" show-overflow-tooltip />
            <el-table-column label="操作" width="120" v-if="capa.currentPhase >= 3 && capa.currentPhase < 4">
              <template #default="{ row }">
                <el-dropdown size="small" @command="(cmd: string) => updateActionStatus(row.id, cmd, true)">
                  <el-button link size="small" type="primary">更新状态</el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item v-for="opt in ACTION_STATUS_OPTIONS" :key="opt.value" :command="opt.value">
                        {{ opt.label }}
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </template>
            </el-table-column>
          </el-table>
        </div>
        <el-empty v-else description="暂无纠正措施" :image-size="60" />
        <div v-if="capa.currentPhase >= 3 && capa.currentPhase < 4" class="m07-phase-form">
          <el-form :model="correctiveForm" inline>
            <el-form-item label="措施描述">
              <el-input v-model="correctiveForm.actionDescription" placeholder="措施描述" class="m07-phase-input" />
            </el-form-item>
            <el-form-item label="负责人">
              <el-input v-model="correctiveForm.responsiblePerson" placeholder="负责人" class="m07-phase-input--sm" />
            </el-form-item>
            <el-form-item label="截止日期">
              <el-date-picker v-model="correctiveForm.dueDate" type="date" class="m07-phase-input--md" />
            </el-form-item>
            <el-button type="primary" @click="addCorrectiveAction">添加</el-button>
          </el-form>
        </div>
      </div>
    </el-card>

    <!-- Phase 4: 预防措施 -->
    <el-card shadow="never" class="m07-phase-card" :class="{ 'm07-phase-card--active': capa.currentPhase >= 4 && capa.currentPhase < 5 }">
      <div class="m07-phase-card__header">
        <span class="m07-phase-card__title"><el-icon><Monitor /></el-icon> 阶段四：预防措施</span>
        <el-tag :type="capa.currentPhase >= 4 ? 'success' : 'info'" size="small" effect="dark">{{ capa.currentPhase >= 4 ? '已完成' : '待进行' }}</el-tag>
      </div>
      <div class="m07-phase-card__body">
        <div v-if="capa.preventiveActions && capa.preventiveActions.length > 0">
          <el-table :data="capa.preventiveActions" stripe border size="small" class="data-card__table">
            <el-table-column prop="actionDescription" label="措施描述" min-width="200" />
            <el-table-column prop="responsiblePerson" label="负责人" width="90" />
            <el-table-column label="截止日期" width="115">
              <template #default="{ row }">{{ row.dueDate?.slice(0, 10) || '-' }}</template>
            </el-table-column>
            <el-table-column label="状态" width="95">
              <template #default="{ row }">
                <el-tag :type="ACTION_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="dark">
                  {{ ACTION_STATUS_MAP[row.status] || row.status }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="remarks" label="备注" show-overflow-tooltip />
            <el-table-column label="操作" width="120" v-if="capa.currentPhase >= 4 && capa.currentPhase < 5">
              <template #default="{ row }">
                <el-dropdown size="small" @command="(cmd: string) => updateActionStatus(row.id, cmd, false)">
                  <el-button link size="small" type="primary">更新状态</el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item v-for="opt in ACTION_STATUS_OPTIONS" :key="opt.value" :command="opt.value">
                        {{ opt.label }}
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </template>
            </el-table-column>
          </el-table>
        </div>
        <el-empty v-else description="暂无预防措施" :image-size="60" />
        <div v-if="capa.currentPhase >= 4 && capa.currentPhase < 5" class="m07-phase-form">
          <el-form :model="preventiveForm" inline>
            <el-form-item label="措施描述">
              <el-input v-model="preventiveForm.actionDescription" placeholder="措施描述" class="m07-phase-input" />
            </el-form-item>
            <el-form-item label="负责人">
              <el-input v-model="preventiveForm.responsiblePerson" placeholder="负责人" class="m07-phase-input--sm" />
            </el-form-item>
            <el-form-item label="截止日期">
              <el-date-picker v-model="preventiveForm.dueDate" type="date" class="m07-phase-input--md" />
            </el-form-item>
            <el-button type="primary" @click="addPreventiveAction">添加</el-button>
          </el-form>
        </div>
      </div>
    </el-card>

    <!-- Phase 5: 效果验证 -->
    <el-card shadow="never" class="m07-phase-card" :class="{ 'm07-phase-card--active': capa.currentPhase >= 5 && capa.currentPhase < 6 }">
      <div class="m07-phase-card__header">
        <span class="m07-phase-card__title"><el-icon><Select /></el-icon> 阶段五：效果验证</span>
        <el-tag :type="capa.currentPhase >= 5 ? 'success' : 'info'" size="small" effect="dark">{{ capa.currentPhase >= 5 ? '已完成' : '待进行' }}</el-tag>
      </div>
      <div class="m07-phase-card__body">
        <div v-if="capa.verifications && capa.verifications.length > 0">
          <el-table :data="capa.verifications" stripe border size="small" class="data-card__table">
            <el-table-column prop="verifierId" label="验证人ID" width="90" />
            <el-table-column label="验证日期" width="165">
              <template #default="{ row }">{{ formatDate(row.verificationDate) }}</template>
            </el-table-column>
            <el-table-column prop="conclusion" label="结论" min-width="150" />
            <el-table-column prop="evidence" label="证据" min-width="150" show-overflow-tooltip />
            <el-table-column prop="remarks" label="备注" show-overflow-tooltip />
          </el-table>
        </div>
        <el-empty v-else description="暂无验证记录" :image-size="60" />
        <div v-if="capa.currentPhase >= 5 && capa.currentPhase < 6" class="m07-phase-form">
          <el-form :model="verificationForm" inline>
            <el-form-item label="验证人ID">
              <el-input-number v-model="verificationForm.verifierId" :min="0" class="m07-phase-input--sm" />
            </el-form-item>
            <el-form-item label="验证日期">
              <el-date-picker v-model="verificationForm.verificationDate" type="date" class="m07-phase-input--md" />
            </el-form-item>
            <el-form-item label="结论">
              <el-input v-model="verificationForm.conclusion" placeholder="结论" class="m07-phase-input--md" />
            </el-form-item>
            <el-form-item label="证据">
              <el-input v-model="verificationForm.evidence" placeholder="证据" class="m07-phase-input--md" />
            </el-form-item>
            <el-form-item label="备注">
              <el-input v-model="verificationForm.remarks" placeholder="备注" class="m07-phase-input--sm" />
            </el-form-item>
            <el-button type="primary" @click="addVerification">添加</el-button>
          </el-form>
        </div>
      </div>
    </el-card>
  </div>
  <el-empty v-else description="CAPA不存在" />
</template>

<style scoped>
.capa-meta {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

/* ─── Phase Form Inputs (统一宽度) ─── */
.m07-phase-input {
  width: 200px;
}

.m07-phase-input--sm {
  width: 120px;
}

.m07-phase-input--md {
  width: 150px;
}

/* ─── Phase Card Enhancement ─── */
.m07-phase-card {
  overflow: visible;
}

/* 已完成阶段 — 轻微淡化但保持可读 */
.m07-phase-card--completed {
  opacity: 0.8;
}

/* 当前活跃阶段 — 强调左侧色条 */
.m07-phase-card--active {
  border-left: 3px solid var(--primary, #1677ff);
  box-shadow: var(--shadow-sm);
}

/* ─── Phase Card Table ─── */
.m07-phase-card__body .data-card__table {
  border-radius: var(--radius-md);
}

.m07-phase-card__body .data-card__table th.el-table__cell {
  background: var(--bg-gray, #fafafa) !important;
  color: var(--text-primary, #1a1a1a);
  font-weight: 600;
  font-size: 12px;
  height: 38px;
  padding: 0 10px;
}

.m07-phase-card__body .data-card__table td.el-table__cell {
  padding: 8px 10px;
  height: 38px;
  font-size: var(--font-sm);
}

/* Phase card table row hover */
.m07-phase-card__body .data-card__table .el-table__row:hover {
  background-color: var(--primary-light, #e6f4ff);
}

/* ─── Header Meta Tags ─── */
.capa-meta .el-tag {
  font-weight: var(--font-medium);
}

/* ─── Flow Card ─────────────────────────── */
.m07-flow-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: var(--radius-lg);
  overflow: hidden;
  margin-bottom: var(--space-4);
}

.m07-flow-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-3) var(--space-4);
  border-bottom: 1px solid var(--el-border-color-lighter);
  gap: var(--space-3);
}

.m07-flow-card__title {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-size: var(--font-md);
  font-weight: var(--font-semibold);
  color: var(--el-text-color-primary);
}

.m07-flow-card__body {
  padding: var(--space-5);
}

/* 流程步骤容器 */
.m07-flow-steps {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-2);
  flex-wrap: wrap;
}

/* 单步 */
.m07-flow-step {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-3) var(--space-4);
  background: var(--el-fill-color-lighter);
  border-radius: var(--radius-lg);
  border: 2px solid transparent;
  transition: all var(--duration-normal) var(--ease-out);
  flex: 1;
  min-width: 140px;
}

.m07-flow-step--completed {
  background: var(--success-bg, #f6ffed);
  border-color: var(--success, #52c41a);
}

.m07-flow-step--active {
  background: var(--primary-light, #e6f4ff);
  border-color: var(--primary, #1677ff);
  box-shadow: 0 2px 8px rgba(22, 119, 255, 0.15);
}

.m07-flow-step--pending {
  background: var(--el-fill-color-lighter);
  border-color: var(--el-border-color-light);
  opacity: 0.6;
}

.m07-flow-step__icon {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  background: rgba(255, 255, 255, 0.8);
}

.m07-flow-step--completed .m07-flow-step__icon {
  color: var(--success, #52c41a);
}

.m07-flow-step--active .m07-flow-step__icon {
  color: var(--primary, #1677ff);
  animation: m07-flow-pulse 2s infinite;
}

.m07-flow-step--pending .m07-flow-step__icon {
  color: var(--el-text-color-placeholder);
}

@keyframes m07-flow-pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); }
}

.m07-flow-step__content {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.m07-flow-step__title {
  font-size: var(--font-md);
  font-weight: var(--font-semibold);
  color: var(--el-text-color-primary);
  white-space: nowrap;
}

.m07-flow-step--completed .m07-flow-step__title {
  color: var(--success, #52c41a);
}

.m07-flow-step--active .m07-flow-step__title {
  color: var(--primary, #1677ff);
}

.m07-flow-step__desc {
  font-size: var(--font-xs);
  color: var(--el-text-color-secondary);
}

.m07-flow-step__arrow {
  color: var(--el-border-color);
  flex-shrink: 0;
}

.m07-flow-step--pending + .m07-flow-step__arrow,
.m07-flow-step--completed + .m07-flow-step__arrow {
  color: var(--el-border-color);
}

/* 下一步指引 */
.m07-flow-card__next {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  margin-top: var(--space-4);
  padding: var(--space-3) var(--space-4);
  background: var(--primary-light, #e6f4ff);
  border-radius: var(--radius-md);
  color: var(--primary, #1677ff);
  font-size: var(--font-sm);
}

.m07-flow-card__next--done {
  background: var(--success-bg, #f6ffed);
  color: var(--success, #52c41a);
}
</style>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { defectApi } from '@/api/defect'
import type { Capa, CapaRootCause, CapaCorrectiveAction, CapaPreventiveAction, CapaVerification } from '@/types/defect'
import {
  CAPA_PHASE_LABELS, CAPA_PHASE_MAP, SEVERITY_MAP, CAPA_STATUS_MAP,
  ACTION_STATUS_OPTIONS, ACTION_STATUS_MAP,
} from '@/types/defect'

defineOptions({ name: 'CapaDetailPage' })

const route = useRoute()
const capaId = Number(route.params.id)
const capa = ref<Capa | null>(null)

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

async function loadCapaDetail() {
  try {
    capa.value = await defectApi.getCapaById(capaId)
  } catch (e) {
    ElMessage.error('加载CAPA详情失败')
  }
}

async function addTemporaryMeasure() {
  if (!tempMeasureForm.value.description || !tempMeasureForm.value.executedBy) {
    ElMessage.warning('请填写措施描述和执行人')
    return
  }
  try {
    await defectApi.addTemporaryMeasure(capaId, { ...tempMeasureForm.value })
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
    await defectApi.addRootCause(capaId, { ...rootCauseForm.value })
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
    await defectApi.addCorrectiveAction(capaId, { ...correctiveForm.value })
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
    await defectApi.addPreventiveAction(capaId, { ...preventiveForm.value })
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
    await defectApi.addVerification(capaId, { ...verificationForm.value })
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
    <div class="header-section">
      <h2 class="capa-title">{{ capa.title }}</h2>
      <div class="capa-meta">
        <el-tag type="info" size="small">{{ capa.capaCode }}</el-tag>
        <el-tag :type="capa.severity === 'critical' ? 'danger' : capa.severity === 'major' ? 'warning' : 'info'" size="small">
          {{ SEVERITY_MAP[capa.severity] }}
        </el-tag>
        <el-tag :type="capa.status === 'active' ? 'primary' : capa.status === 'completed' ? 'success' : 'info'" size="small">
          {{ CAPA_STATUS_MAP[capa.status] }}
        </el-tag>
      </div>
    </div>

    <el-steps :active="capa.currentPhase" finish-status="success" class="capa-stepper">
      <el-step
        v-for="step in CAPA_PHASE_LABELS"
        :key="step.phase"
        :title="step.label"
        :description="step.phase <= capa.currentPhase ? '已完成' : '待进行'"
      />
    </el-steps>

    <el-descriptions :column="3" border class="detail-descriptions">
      <el-descriptions-item label="缺陷ID">{{ capa.defectId }}</el-descriptions-item>
      <el-descriptions-item label="负责人">{{ capa.assignedTo }}</el-descriptions-item>
      <el-descriptions-item label="截止日期">{{ capa.dueDate?.slice(0, 10) || '-' }}</el-descriptions-item>
      <el-descriptions-item label="创建人">{{ capa.createdBy }}</el-descriptions-item>
      <el-descriptions-item label="创建时间">{{ formatDate(capa.createdAt) }}</el-descriptions-item>
      <el-descriptions-item label="关闭时间" v-if="capa.closedAt">{{ formatDate(capa.closedAt) }}</el-descriptions-item>
    </el-descriptions>

    <el-divider />
    <p class="section-desc"><strong>问题描述：</strong>{{ capa.description || '无' }}</p>
    <el-divider />

    <!-- Phase 1: 临时措施 -->
    <el-card class="phase-card">
      <template #header>
        <div class="card-header">
          <span><el-icon><AlarmClock /></el-icon> 临时措施 (Phase 1)</span>
          <el-tag :type="capa.currentPhase >= 1 ? 'success' : 'info'" size="small">
            {{ capa.currentPhase >= 1 ? '已完成' : '待进行' }}
          </el-tag>
        </div>
      </template>
      <div v-if="capa.temporaryMeasures && capa.temporaryMeasures.length > 0">
        <el-table :data="capa.temporaryMeasures"  stripe>
          <el-table-column prop="description" label="措施描述" min-width="200" />
          <el-table-column prop="executedBy" label="执行人" width="90" />
          <el-table-column label="执行时间" width="160">
            <template #default="{ row }">{{ formatDate(row.executedAt) }}</template>
          </el-table-column>
        </el-table>
      </div>
      <el-empty v-else description="暂无临时措施" :image-size="60" />
      <el-row :gutter="12" class="phase-form" v-if="capa.currentPhase >= 1 && capa.currentPhase < 2">
        <el-col :span="12">
          <el-input v-model="tempMeasureForm.description" placeholder="措施描述"  />
        </el-col>
        <el-col :span="4">
          <el-input v-model="tempMeasureForm.executedBy" placeholder="执行人"  />
        </el-col>
        <el-col :span="4">
          <el-date-picker v-model="tempMeasureForm.executedAt" type="date" style="width: 100%" />
        </el-col>
        <el-col :span="2">
          <el-button type="primary" size="small" @click="addTemporaryMeasure">添加</el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Phase 2: 根本原因分析 -->
    <el-card class="phase-card">
      <template #header>
        <div class="card-header">
          <span><el-icon><Search /></el-icon> 根本原因分析 / 5Why (Phase 2)</span>
          <el-tag :type="capa.currentPhase >= 2 ? 'success' : 'info'" size="small">
            {{ capa.currentPhase >= 2 ? '已完成' : '待进行' }}
          </el-tag>
        </div>
      </template>
      <div v-if="capa.rootCauses && capa.rootCauses.length > 0">
        <el-table :data="capa.rootCauses"  stripe>
          <el-table-column prop="analysisMethod" label="分析方法" width="100" />
          <el-table-column prop="content" label="分析内容" min-width="250" show-overflow-tooltip />
          <el-table-column prop="rootCauseSummary" label="根因总结" min-width="180" show-overflow-tooltip />
          <el-table-column prop="createdBy" label="分析人" width="80" />
          <el-table-column label="时间" width="160">
            <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
          </el-table-column>
        </el-table>
      </div>
      <el-empty v-else description="暂无根本原因分析" :image-size="60" />
      <el-row :gutter="12" class="phase-form" v-if="capa.currentPhase >= 2 && capa.currentPhase < 3">
        <el-col :span="3">
          <el-select v-model="rootCauseForm.analysisMethod"  style="width: 100%">
            <el-option label="5Why" value="5Why" />
            <el-option label="鱼骨图" value="fishbone" />
            <el-option label="FMEA" value="FMEA" />
            <el-option label="其他" value="other" />
          </el-select>
        </el-col>
        <el-col :span="8">
          <el-input v-model="rootCauseForm.content" placeholder="分析内容"  />
        </el-col>
        <el-col :span="7">
          <el-input v-model="rootCauseForm.rootCauseSummary" placeholder="根因总结"  />
        </el-col>
        <el-col :span="6">
          <el-button type="primary" size="small" @click="addRootCause">添加分析</el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Phase 3: 纠正措施 -->
    <el-card class="phase-card">
      <template #header>
        <div class="card-header">
          <span><el-icon><Tools /></el-icon> 纠正措施 (Phase 3)</span>
          <el-tag :type="capa.currentPhase >= 3 ? 'success' : 'info'" size="small">
            {{ capa.currentPhase >= 3 ? '已完成' : '待进行' }}
          </el-tag>
        </div>
      </template>
      <div v-if="capa.correctiveActions && capa.correctiveActions.length > 0">
        <el-table :data="capa.correctiveActions"  stripe>
          <el-table-column prop="actionDescription" label="措施描述" min-width="200" />
          <el-table-column prop="responsiblePerson" label="负责人" width="90" />
          <el-table-column label="截止日期" width="110">
            <template #default="{ row }">{{ row.dueDate?.slice(0, 10) || '-' }}</template>
          </el-table-column>
          <el-table-column label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="ACTION_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small">
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
      <el-row :gutter="12" class="phase-form" v-if="capa.currentPhase >= 3 && capa.currentPhase < 4">
        <el-col :span="8">
          <el-input v-model="correctiveForm.actionDescription" placeholder="措施描述"  />
        </el-col>
        <el-col :span="5">
          <el-input v-model="correctiveForm.responsiblePerson" placeholder="负责人"  />
        </el-col>
        <el-col :span="5">
          <el-date-picker v-model="correctiveForm.dueDate" type="date" style="width: 100%" />
        </el-col>
        <el-col :span="6">
          <el-button type="primary" size="small" @click="addCorrectiveAction">添加</el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Phase 4: 预防措施 -->
    <el-card class="phase-card">
      <template #header>
        <div class="card-header">
          <span><el-icon><Shield /></el-icon> 预防措施 (Phase 4)</span>
          <el-tag :type="capa.currentPhase >= 4 ? 'success' : 'info'" size="small">
            {{ capa.currentPhase >= 4 ? '已完成' : '待进行' }}
          </el-tag>
        </div>
      </template>
      <div v-if="capa.preventiveActions && capa.preventiveActions.length > 0">
        <el-table :data="capa.preventiveActions"  stripe>
          <el-table-column prop="actionDescription" label="措施描述" min-width="200" />
          <el-table-column prop="responsiblePerson" label="负责人" width="90" />
          <el-table-column label="截止日期" width="110">
            <template #default="{ row }">{{ row.dueDate?.slice(0, 10) || '-' }}</template>
          </el-table-column>
          <el-table-column label="状态" width="90">
            <template #default="{ row }">
              <el-tag :type="ACTION_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small">
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
      <el-row :gutter="12" class="phase-form" v-if="capa.currentPhase >= 4 && capa.currentPhase < 5">
        <el-col :span="8">
          <el-input v-model="preventiveForm.actionDescription" placeholder="措施描述"  />
        </el-col>
        <el-col :span="5">
          <el-input v-model="preventiveForm.responsiblePerson" placeholder="负责人"  />
        </el-col>
        <el-col :span="5">
          <el-date-picker v-model="preventiveForm.dueDate" type="date" style="width: 100%" />
        </el-col>
        <el-col :span="6">
          <el-button type="primary" size="small" @click="addPreventiveAction">添加</el-button>
        </el-col>
      </el-row>
    </el-card>

    <!-- Phase 5: 效果验证 -->
    <el-card class="phase-card">
      <template #header>
        <div class="card-header">
          <span><el-icon><Select /></el-icon> 效果验证 (Phase 5)</span>
          <el-tag :type="capa.currentPhase >= 5 ? 'success' : 'info'" size="small">
            {{ capa.currentPhase >= 5 ? '已完成' : '待进行' }}
          </el-tag>
        </div>
      </template>
      <div v-if="capa.verifications && capa.verifications.length > 0">
        <el-table :data="capa.verifications"  stripe>
          <el-table-column prop="verifierId" label="验证人ID" width="90" />
          <el-table-column label="验证日期" width="160">
            <template #default="{ row }">{{ formatDate(row.verificationDate) }}</template>
          </el-table-column>
          <el-table-column prop="conclusion" label="结论" min-width="150" />
          <el-table-column prop="evidence" label="证据" min-width="150" show-overflow-tooltip />
          <el-table-column prop="remarks" label="备注" show-overflow-tooltip />
        </el-table>
      </div>
      <el-empty v-else description="暂无验证记录" :image-size="60" />
      <el-row :gutter="12" class="phase-form" v-if="capa.currentPhase >= 5 && capa.currentPhase < 6">
        <el-col :span="3">
          <el-input-number v-model="verificationForm.verifierId" :min="0" placeholder="验证人ID"  style="width: 100%" />
        </el-col>
        <el-col :span="4">
          <el-date-picker v-model="verificationForm.verificationDate" type="date" style="width: 100%" />
        </el-col>
        <el-col :span="5">
          <el-input v-model="verificationForm.conclusion" placeholder="结论"  />
        </el-col>
        <el-col :span="5">
          <el-input v-model="verificationForm.evidence" placeholder="证据"  />
        </el-col>
        <el-col :span="4">
          <el-input v-model="verificationForm.remarks" placeholder="备注"  />
        </el-col>
        <el-col :span="3">
          <el-button type="primary" size="small" @click="addVerification">添加</el-button>
        </el-col>
      </el-row>
    </el-card>
  </div>
  <el-empty v-else description="CAPA不存在" />
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; gap: 8px; }
.header-section { display: flex; align-items: center; gap: 16px; margin-bottom: 8px; }
.capa-title { margin: 0; font-size: 18px; }
.capa-meta { display: flex; gap: 6px; }
.capa-stepper { padding: 12px 0; }
.detail-descriptions { margin-bottom: 0; }
.section-desc { margin: 0; font-size: 14px; }
.phase-card { margin-bottom: 8px; }
.card-header { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.phase-form { margin-top: 12px; }
</style>

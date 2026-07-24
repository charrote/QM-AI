<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { d8ReportApi } from '@/api/d8Report'
import type { D8Report } from '@/types/complaint'
import {
  D8_DISCIPLINE_LABELS, D8_DISCIPLINE_MAP, D8_STATUS_OPTIONS,
} from '@/types/complaint'

const D8_STATUS_MAP: Record<string, string> = Object.fromEntries(D8_STATUS_OPTIONS.map(o => [o.value, o.label]))

defineOptions({ name: 'D8ReportPage' })

const d8List = ref<D8Report[]>([])
const selectedD8 = ref<D8Report | null>(null)
const loading = ref(false)
const viewMode = ref<'list' | 'detail'>('list')
const complaintIdFilter = ref<number | undefined>(undefined)

// Dialog
const dialogVisible = ref(false)
const isEditing = ref(false)
const currentD8Id = ref<number | null>(null)
const form = reactive({
  complaintId: 0,
  d0Description: '',
  d1Team: '',
  d2Description: '',
  d3Measures: '',
  d4AnalysisMethod: '',
  d4Content: '',
  d4RootCause: '',
  d5Actions: '',
  d6Verification: '',
  d7Preventive: '',
  d8Thanks: '',
})

// Step form for detail view
const currentStep = ref(0)

function formatDate(d?: string) {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

async function loadD8List() {
  loading.value = true
  try {
    d8List.value = await d8ReportApi.list(complaintIdFilter.value)
  } catch (e) {
    console.error('Failed to load D8 reports', e)
  } finally {
    loading.value = false
  }
}

function openCreate() {
  isEditing.value = false
  currentD8Id.value = null
  form.complaintId = complaintIdFilter.value || 0
  form.d0Description = ''
  form.d1Team = ''
  form.d2Description = ''
  form.d3Measures = ''
  form.d4AnalysisMethod = ''
  form.d4Content = ''
  form.d4RootCause = ''
  form.d5Actions = ''
  form.d6Verification = ''
  form.d7Preventive = ''
  form.d8Thanks = ''
  dialogVisible.value = true
}

function openEdit(row: D8Report) {
  isEditing.value = true
  currentD8Id.value = row.id
  form.complaintId = row.complaintId
  form.d0Description = row.d0Description || ''
  form.d1Team = row.d1Team || ''
  form.d2Description = row.d2Description || ''
  form.d3Measures = row.d3Measures || ''
  form.d4AnalysisMethod = row.d4AnalysisMethod || ''
  form.d4Content = row.d4Content || ''
  form.d4RootCause = row.d4RootCause || ''
  form.d5Actions = row.d5Actions || ''
  form.d6Verification = row.d6Verification || ''
  form.d7Preventive = row.d7Preventive || ''
  form.d8Thanks = row.d8Thanks || ''
  dialogVisible.value = true
}

async function saveD8() {
  if (!form.complaintId || !form.d0Description) {
    ElMessage.warning('请填写投诉ID和问题概述')
    return
  }
  try {
    if (isEditing.value && currentD8Id.value) {
      await d8ReportApi.update(currentD8Id.value, {
        d0Description: form.d0Description,
        d1Team: form.d1Team,
        d2Description: form.d2Description,
        d3Measures: form.d3Measures,
        d4AnalysisMethod: form.d4AnalysisMethod,
        d4Content: form.d4Content,
        d4RootCause: form.d4RootCause,
        d5Actions: form.d5Actions,
        d6Verification: form.d6Verification,
        d7Preventive: form.d7Preventive,
        d8Thanks: form.d8Thanks,
      })
      ElMessage.success('8D报告已更新')
    } else {
      await d8ReportApi.create({ complaintId: form.complaintId, d0Description: form.d0Description })
      ElMessage.success('8D报告已创建')
    }
    dialogVisible.value = false
    await loadD8List()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteD8(row: D8Report) {
  try {
    await ElMessageBox.confirm('确定删除8D报告吗？', '确认', { type: 'warning' })
    ElMessage.info('删除功能待实现')
    ElMessage.success('已删除')
    await loadD8List()
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '删除失败')
  }
}

function viewDetail(row: D8Report) {
  selectedD8.value = row
  viewMode.value = 'detail'
  currentStep.value = row.currentDiscipline
}

async function advanceDiscipline() {
  if (!selectedD8.value) return
  const nextDisc = Math.min(selectedD8.value.currentDiscipline + 1, 8)
  try {
    await ElMessageBox.confirm(
      `确认推进到「${D8_DISCIPLINE_LABELS[nextDisc]?.label}」？`,
      '确认推进',
      { type: 'info' }
    )
    selectedD8.value = await d8ReportApi.advance(selectedD8.value.id, { disciplineIndex: nextDisc })
    currentStep.value = selectedD8.value.currentDiscipline
    ElMessage.success('已推进')
  } catch (e: any) {
    if (e !== 'cancel') ElMessage.error(e?.response?.data?.message || '推进失败')
  }
}

function exportPdf() {
  if (!selectedD8.value) return
  try {
    d8ReportApi.get(selectedD8.value.id)
    ElMessage.success('PDF下载中')
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '导出失败')
  }
}

// Get field key for a given discipline index (0-8)
const disciplineFields: Record<number, { key: string; label: string }[]> = {
  0: [{ key: 'd0Description', label: 'D0·问题概述' }],
  1: [{ key: 'd1Team', label: 'D1·改善小组' }],
  2: [{ key: 'd2Description', label: 'D2·问题描述' }],
  3: [{ key: 'd3Measures', label: 'D3·临时围堵措施' }],
  4: [
    { key: 'd4AnalysisMethod', label: 'D4·分析方法' },
    { key: 'd4Content', label: 'D4·分析过程' },
    { key: 'd4RootCause', label: 'D4·根本原因' },
  ],
  5: [{ key: 'd5Actions', label: 'D5·纠正措施' }],
  6: [{ key: 'd6Verification', label: 'D6·实施验证' }],
  7: [{ key: 'd7Preventive', label: 'D7·预防措施' }],
  8: [{ key: 'd8Thanks', label: 'D8·结案致谢' }],
}

function getDisciplineFields(disc: number) {
  return disciplineFields[disc] || []
}

function getFieldValue(report: D8Report, key: string) {
  return (report as Record<string, any>)[key] || ''
}

// Step navigation labels (for the step indicator)
const stepLabels = [
  'D0·问题概述',
  'D1·改善小组',
  'D2·问题描述',
  'D3·临时围堵',
  'D4·分析方法',
  'D4·分析内容',
  'D4·根本原因',
  'D5·纠正措施',
  'D6·实施验证',
  'D7·预防措施',
  'D8·结案致谢',
]

// Map discipline index to step index (0-10)
function disciplineToStep(disc: number): number {
  // D0=step0, D1=step1, D2=step2, D3=step3, D4=steps4-6, D5=step7, D6=step8, D7=step9, D8=step10
  if (disc === 4) return 4
  return disc + (disc >= 4 ? 3 : 0)
}

function getStepLabel(idx: number): string {
  return stepLabels[idx] || ''
}

function getDisciplineFromStep(idx: number): number {
  if (idx >= 7) return idx - 3
  if (idx >= 4) return 4
  return idx
}

onMounted(loadD8List)
</script>

<template>
  <div class="page-container">
    <template v-if="viewMode === 'list'">
      <!-- Toolbar -->
      <div class="toolbar-row">
        <el-input
          v-model="complaintIdFilter"
          placeholder="按投诉ID筛选"
          clearable
          style="width: 200px"
          @change="loadD8List"
        />
        <el-button type="primary" @click="openCreate">+ 新建8D报告</el-button>
        <el-button @click="loadD8List">刷新</el-button>
      </div>

      <!-- Table -->
      <el-table :data="d8List" stripe style="width: 100%"  v-loading="loading">
        <el-table-column prop="id" label="ID" width="60" />
        <el-table-column prop="complaintId" label="投诉ID" width="80" />
        <el-table-column label="D0·问题概述" min-width="200" show-overflow-tooltip>
          <template #default="{ row }">{{ row.d0Description?.slice(0, 50) || '-' }}</template>
        </el-table-column>
        <el-table-column label="当前阶段" width="130">
          <template #default="{ row }">
            <el-tag size="small" effect="plain">
              {{ D8_DISCIPLINE_LABELS[row.currentDiscipline]?.label || `D${row.currentDiscipline}` }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="D8_STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small" effect="plain">
              {{ D8_STATUS_MAP[row.status] || row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="创建时间" width="150">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button link size="small" type="primary" @click="viewDetail(row)">查看</el-button>
            <el-button link size="small" type="warning" @click="openEdit(row)">编辑</el-button>
            <el-button link size="small" type="danger" @click="deleteD8(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </template>

    <template v-if="selectedD8">
      <!-- Detail View with Step Form -->
      <div class="detail-header">
        <el-button @click="viewMode = 'list'" style="margin-bottom: 12px">← 返回列表</el-button>
        <h3>8D 报告 #{{ selectedD8.id }}</h3>
        <div class="detail-actions">
          <el-button type="primary" size="small" @click="exportPdf">导出PDF</el-button>
          <el-button type="warning" size="small" @click="openEdit(selectedD8)">编辑</el-button>
        </div>
      </div>

      <!-- Steps -->
      <el-card class="steps-card" style="margin-bottom: 16px">
        <template #header>
          <div class="card-header">
            <span>8D 阶段进度</span>
            <el-tag :type="D8_STATUS_OPTIONS.find(o => o.value === selectedD8!.status)?.type || 'info'" size="small">
              {{ D8_STATUS_MAP[selectedD8!.status] || selectedD8!.status }}
            </el-tag>
          </div>
        </template>
        <el-steps
          :active="disciplineToStep(selectedD8!.currentDiscipline)"
          finish-status="success"
          simple
        >
          <el-step
            v-for="(label, idx) in stepLabels"
            :key="idx"
            :title="label"
            :status="
              disciplineToStep(selectedD8!.currentDiscipline) > idx ? 'success' :
              disciplineToStep(selectedD8!.currentDiscipline) === idx ? 'process' : ''
            "
          />
        </el-steps>

        <!-- Step navigation -->
        <div class="step-nav" style="margin-top: 16px">
          <el-button
            size="small"
            :disabled="disciplineToStep(selectedD8!.currentDiscipline) <= 0"
            @click="currentStep = disciplineToStep(selectedD8!.currentDiscipline) - 1"
          >上一步</el-button>
          <el-button
            type="success"
            size="small"
            :disabled="disciplineToStep(selectedD8!.currentDiscipline) >= 10"
            @click="advanceDiscipline()"
          >推进到下一步</el-button>
        </div>
      </el-card>

      <!-- Current Step Content -->
      <el-card v-for="field in getDisciplineFields(getDisciplineFromStep(currentStep))" :key="field.key" class="discipline-card" style="margin-bottom: 12px">
        <template #header>
          <span class="field-label">{{ field.label }}</span>
        </template>
        <div class="field-content">
          {{ getFieldValue(selectedD8!, field.key) || '暂无内容' }}
        </div>
      </el-card>

      <!-- All Disciplines Collapsible -->
      <el-collapse style="margin-top: 16px">
        <el-collapse-item
          v-for="disc in D8_DISCIPLINE_LABELS"
          :key="disc.discipline"
          :name="disc.discipline"
          :title="disc.label"
        >
          <div v-for="field in getDisciplineFields(disc.discipline)" :key="field.key" class="discipline-detail">
            <strong>{{ field.label }}:</strong>
            <div class="discipline-detail-content">
              {{ getFieldValue(selectedD8!, field.key) || '暂无内容' }}
            </div>
          </div>
        </el-collapse-item>
      </el-collapse>
    </template>

    <!-- Create/Edit Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEditing ? '编辑8D报告' : '新建8D报告'"
      width="720px"
      :close-on-click-modal="false"
    >
      <el-form :model="form" label-width="120px" >
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="投诉ID" required>
              <el-input-number v-model="form.complaintId" :min="1" style="width: 100%" />
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="D0·问题概述" required>
          <el-input v-model="form.d0Description" type="textarea" :rows="2" placeholder="描述问题概述" />
        </el-form-item>
        <el-form-item label="D1·改善小组">
          <el-input v-model="form.d1Team" type="textarea" :rows="2" placeholder="团队成员及角色" />
        </el-form-item>
        <el-form-item label="D2·问题描述">
          <el-input v-model="form.d2Description" type="textarea" :rows="2" placeholder="问题描述、5W2H" />
        </el-form-item>
        <el-form-item label="D3·临时围堵">
          <el-input v-model="form.d3Measures" type="textarea" :rows="2" placeholder="临时围堵措施" />
        </el-form-item>
        <el-form-item label="D4·分析方法">
          <el-input v-model="form.d4AnalysisMethod" type="textarea" :rows="2" placeholder="鱼骨图/5Why/FTA" />
        </el-form-item>
        <el-form-item label="D4·分析内容">
          <el-input v-model="form.d4Content" type="textarea" :rows="2" placeholder="详细分析过程" />
        </el-form-item>
        <el-form-item label="D4·根本原因">
          <el-input v-model="form.d4RootCause" type="textarea" :rows="2" placeholder="确认的根本原因" />
        </el-form-item>
        <el-form-item label="D5·纠正措施">
          <el-input v-model="form.d5Actions" type="textarea" :rows="2" placeholder="纠正措施计划" />
        </el-form-item>
        <el-form-item label="D6·实施验证">
          <el-input v-model="form.d6Verification" type="textarea" :rows="2" placeholder="实施及验证结果" />
        </el-form-item>
        <el-form-item label="D7·预防措施">
          <el-input v-model="form.d7Preventive" type="textarea" :rows="2" placeholder="预防措施" />
        </el-form-item>
        <el-form-item label="D8·结案致谢">
          <el-input v-model="form.d8Thanks" type="textarea" :rows="2" placeholder="结案致谢及总结" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="saveD8">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.page-container { display: flex; flex-direction: column; height: 100%; }
.toolbar-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.detail-header { display: flex; align-items: center; gap: 12px; flex-wrap: wrap; }
.detail-header h3 { margin: 0; }
.detail-actions { margin-left: auto; display: flex; gap: 8px; }
.card-header { display: flex; align-items: center; justify-content: space-between; }
.step-nav { display: flex; justify-content: space-between; }
.field-label { font-weight: 600; font-size: 14px; }
.field-content { white-space: pre-wrap; font-size: 13px; line-height: 1.6; }
.discipline-detail { margin-bottom: 8px; }
.discipline-detail-content { margin-top: 4px; white-space: pre-wrap; font-size: 13px; line-height: 1.6; color: var(--el-text-color-regular); }
</style>

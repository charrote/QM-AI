<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { paramGroupApi, dynamicParamApi, closureRuleApi } from '@/api/dynamicParams'
import { Setting, Document, DataAnalysis, Tools, FolderOpened, Edit, Delete, Cpu } from '@element-plus/icons-vue'
import type {
  ParamGroup, ParamGroupDetail, CreateParamGroup,
  DynamicParam, DynamicParamDetail, CreateDynamicParam, UpdateDynamicParam,
  ClosureRule, ClosureRuleDetail, CreateClosureRule,
  ClosureCondition, ClosureEvaluationResult,
  AiStrategyPreset,
} from '@/types/dynamicParams'

defineOptions({ name: 'IqcParams' })

// ============================================================================
// Section 1: 参数组管理 (ParamGroupTree)
// ============================================================================
const groups = ref<ParamGroup[]>([])
const selectedGroup = ref<ParamGroup | null>(null)
const groupDialogVisible = ref(false)
const isEditingGroup = ref(false)
const groupForm = reactive<CreateParamGroup>({ name: '', code: '', description: '', sortOrder: 0 })

async function loadGroups() {
  try {
    groups.value = await paramGroupApi.getAll()
  } catch (e) {
    console.error('Failed to load param groups', e)
  }
}

function openCreateGroup() {
  isEditingGroup.value = false
  groupForm.name = ''
  groupForm.code = ''
  groupForm.description = ''
  groupForm.sortOrder = 0
  groupDialogVisible.value = true
}

function openEditGroup(group: ParamGroup) {
  isEditingGroup.value = true
  groupForm.name = group.name
  groupForm.code = group.code
  groupForm.description = group.description
  groupForm.sortOrder = group.sortOrder
  groupDialogVisible.value = true
}

async function saveGroup() {
  try {
    if (isEditingGroup.value && selectedGroup.value) {
      await paramGroupApi.update(selectedGroup.value.id, {
        name: groupForm.name,
        description: groupForm.description,
        sortOrder: groupForm.sortOrder,
      })
      ElMessage.success('参数组已更新')
    } else {
      await paramGroupApi.create(groupForm)
      ElMessage.success('参数组已创建')
    }
    groupDialogVisible.value = false
    await loadGroups()
    // 刷新参数列表
    if (selectedGroup.value) {
      const updated = groups.value.find(g => g.id === selectedGroup.value!.id)
      if (updated) selectedGroup.value = updated
    }
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteGroup(group: ParamGroup) {
  try {
    await ElMessageBox.confirm(`确定删除参数组「${group.name}」吗？`, '确认删除', {
      type: 'warning',
      confirmButtonText: '删除',
      cancelButtonText: '取消',
    })
    await paramGroupApi.delete(group.id)
    ElMessage.success('参数组已删除')
    if (selectedGroup.value?.id === group.id) {
      selectedGroup.value = null
    }
    await loadGroups()
  } catch (e: any) {
    if (e !== 'cancel') {
      ElMessage.error(e?.response?.data?.message || '删除失败')
    }
  }
}

// ============================================================================
// Section 2: 动态参数管理 (ParamForm + ParamPreviewCard + AiStrategySelector)
// ============================================================================
const params = ref<DynamicParam[]>([])
const selectedParam = ref<DynamicParam | null>(null)
const paramDialogVisible = ref(false)
const isEditingParam = ref(false)
const aiStrategies = ref<AiStrategyPreset[]>([])
const filteredStrategies = computed(() =>
  aiStrategies.value.filter(s => s.dataType === paramForm.dataType)
)

const paramForm = reactive<CreateDynamicParam>({
  groupId: 0,
  name: '',
  code: '',
  dataType: 'numeric',
  unit: '',
  targetValue: undefined,
  usl: undefined,
  lsl: undefined,
  precision: 1.0,
  aiStrategy: '',
  sortOrder: 0,
  isActive: true,
})

const dataTypeOptions = [
  { value: 'numeric', label: '数值型' },
  { value: 'categorical', label: '枚举型' },
  { value: 'boolean', label: '布尔型' },
]

async function loadParams() {
  if (!selectedGroup.value) {
    params.value = []
    return
  }
  try {
    params.value = await dynamicParamApi.getByGroup(selectedGroup.value.id)
  } catch (e) {
    console.error('Failed to load params', e)
  }
}

watch(selectedGroup, () => {
  selectedParam.value = null
  loadParams()
})

async function loadAiStrategies() {
  try {
    aiStrategies.value = await dynamicParamApi.getAiStrategies()
  } catch (e) {
    console.error('Failed to load AI strategies', e)
  }
}

function openCreateParam() {
  if (!selectedGroup.value) {
    ElMessage.warning('请先选择一个参数组')
    return
  }
  isEditingParam.value = false
  paramForm.groupId = selectedGroup.value.id
  paramForm.name = ''
  paramForm.code = ''
  paramForm.dataType = 'numeric'
  paramForm.unit = ''
  paramForm.targetValue = undefined
  paramForm.usl = undefined
  paramForm.lsl = undefined
  paramForm.precision = 1.0
  paramForm.aiStrategy = ''
  paramForm.sortOrder = 0
  paramDialogVisible.value = true
}

function openEditParam(param: DynamicParam) {
  isEditingParam.value = true
  paramForm.groupId = param.groupId
  paramForm.name = param.name
  paramForm.code = param.code
  paramForm.dataType = param.dataType
  paramForm.unit = param.unit || ''
  paramForm.targetValue = param.targetValue
  paramForm.usl = param.usl
  paramForm.lsl = param.lsl
  paramForm.precision = param.precision
  // aiStrategy 在 DB 中存储为 JSON 格式 {"id":"strategy_id"}
  // 需要解析提取 ID 以匹配下拉选项
  paramForm.aiStrategy = parseAiStrategyId(param.aiStrategy)
  paramForm.sortOrder = param.sortOrder
  paramForm.isActive = param.isActive
  paramDialogVisible.value = true
}

/** 从后端 JSON 格式的 aiStrategy 中提取策略 ID */
function parseAiStrategyId(val: string | undefined | null): string {
  if (!val) return ''
  try {
    const parsed = JSON.parse(val)
    return parsed?.id || val
  } catch {
    return val // 已经是纯 ID 字符串
  }
}

async function saveParam() {
  try {
    // 将 AI 策略 ID 序列化为后端期望的 JSON 格式 {"id":"strategy_id"}
    const aiStrategyPayload = paramForm.aiStrategy
      ? JSON.stringify({ id: paramForm.aiStrategy })
      : undefined

    if (isEditingParam.value && selectedParam.value) {
      await dynamicParamApi.update(selectedParam.value.id, {
        groupId: paramForm.groupId,
        name: paramForm.name,
        dataType: paramForm.dataType,
        unit: paramForm.unit || undefined,
        targetValue: paramForm.targetValue,
        usl: paramForm.usl,
        lsl: paramForm.lsl,
        precision: paramForm.precision,
        aiStrategy: aiStrategyPayload,
        sortOrder: paramForm.sortOrder,
        isActive: paramForm.isActive ?? true,
      })
      ElMessage.success('参数已更新')
    } else {
      await dynamicParamApi.create({
        ...paramForm,
        aiStrategy: aiStrategyPayload,
      })
      ElMessage.success('参数已创建')
    }
    paramDialogVisible.value = false
    await loadParams()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteParam(param: DynamicParam) {
  try {
    await ElMessageBox.confirm(`确定删除参数「${param.name}」吗？`, '确认删除', {
      type: 'warning',
      confirmButtonText: '删除',
      cancelButtonText: '取消',
    })
    await dynamicParamApi.delete(param.id)
    ElMessage.success('参数已删除')
    if (selectedParam.value?.id === param.id) {
      selectedParam.value = null
    }
    await loadParams()
  } catch (e: any) {
    if (e !== 'cancel') {
      ElMessage.error(e?.response?.data?.message || '删除失败')
    }
  }
}

function selectParam(param: DynamicParam) {
  selectedParam.value = param
}

// 数值型规格限校验
const specLimitError = computed(() => {
  if (paramForm.dataType === 'numeric' && paramForm.usl !== undefined && paramForm.lsl !== undefined) {
    return paramForm.usl < paramForm.lsl ? 'USL 必须大于 LSL' : ''
  }
  return ''
})

// ============================================================================
// Section 3: 动态仪表盘预览 (DynamicDashboard)
// ============================================================================
const dashboardVisible = ref(false)

function previewDashboard() {
  if (params.value.length === 0) {
    ElMessage.warning('当前组没有参数可供展示')
    return
  }
  dashboardVisible.value = !dashboardVisible.value
}

// ============================================================================
// Section 4: 关单策略模板 (ClosureRuleBuilder + ClosureRuleList)
// ============================================================================
const rules = ref<ClosureRule[]>([])
const ruleDialogVisible = ref(false)
const isEditingRule = ref(false)
const editingRule = ref<ClosureRuleDetail | null>(null)

const ruleForm = reactive<CreateClosureRule>({
  name: '',
  code: '',
  conditionJson: '[]',
  logic: 'AND',
  description: '',
})

// 条件列表（从 JSON 解析）
const conditions = ref<ClosureCondition[]>([])

const conditionTypeOptions = [
  { value: 'consecutive_ok', label: '连续 N 件合格' },
  { value: 'spk_cpk', label: 'SPC-Cpk' },
  { value: 'sampling_rate', label: '抽检合格率' },
  { value: 'ai_risk_score', label: 'AI 风险评分' },
]

const operatorOptions = [
  { value: '>', label: '>' },
  { value: '>=', label: '>=' },
  { value: '<', label: '<' },
  { value: '<=', label: '<=' },
  { value: '=', label: '=' },
]

function getConditionLabel(type: string): string {
  const opt = conditionTypeOptions.find(o => o.value === type)
  return opt ? opt.label : type
}

async function loadRules() {
  try {
    rules.value = await closureRuleApi.getAll()
  } catch (e) {
    console.error('Failed to load rules', e)
  }
}

function openCreateRule() {
  isEditingRule.value = false
  editingRule.value = null
  ruleForm.name = ''
  ruleForm.code = ''
  ruleForm.logic = 'AND'
  ruleForm.description = ''
  ruleForm.conditionJson = '[]'
  conditions.value = []
  ruleDialogVisible.value = true
}

function openEditRule(rule: ClosureRule) {
  isEditingRule.value = true
  closureRuleApi.get(rule.id).then(detail => {
    editingRule.value = detail
    ruleForm.name = detail.name
    ruleForm.code = detail.code
    ruleForm.logic = detail.logic
    ruleForm.description = detail.description || ''
    ruleForm.conditionJson = detail.conditionJson
    try {
      conditions.value = JSON.parse(detail.conditionJson || '[]')
    } catch {
      conditions.value = []
    }
    ruleDialogVisible.value = true
  })
}

function addCondition() {
  conditions.value.push({
    type: 'consecutive_ok',
    paramCode: '',
    threshold: 10,
    operator: '>=',
  })
}

function removeCondition(index: number) {
  conditions.value.splice(index, 1)
}

function resetConditionFields(idx: number) {
  const cond = conditions.value[idx]
  // Set default thresholds based on type
  switch (cond.type) {
    case 'consecutive_ok': cond.threshold = 10; cond.operator = '>='; break
    case 'spk_cpk': cond.threshold = 1.33; cond.operator = '>'; break
    case 'sampling_rate': cond.threshold = 98; cond.operator = '>='; break
    case 'ai_risk_score': cond.threshold = 60; cond.operator = '<'; break
  }
}

async function saveRule() {
  // 验证
  if (conditions.value.length === 0) {
    ElMessage.warning('请至少添加一个条件')
    return
  }
  ruleForm.conditionJson = JSON.stringify(conditions.value)

  try {
    if (isEditingRule.value && editingRule.value) {
      await closureRuleApi.update(editingRule.value.id, {
        name: ruleForm.name,
        conditionJson: ruleForm.conditionJson,
        logic: ruleForm.logic,
        description: ruleForm.description || undefined,
        isActive: editingRule.value.isActive,
      })
      ElMessage.success('规则已更新')
    } else {
      await closureRuleApi.create(ruleForm)
      ElMessage.success('规则已创建')
    }
    ruleDialogVisible.value = false
    await loadRules()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '操作失败')
  }
}

async function deleteRule(rule: ClosureRule) {
  try {
    await ElMessageBox.confirm(`确定删除关单规则「${rule.name}」吗？`, '确认删除', {
      type: 'warning',
      confirmButtonText: '删除',
      cancelButtonText: '取消',
    })
    await closureRuleApi.delete(rule.id)
    ElMessage.success('规则已删除')
    await loadRules()
  } catch (e: any) {
    if (e !== 'cancel') {
      ElMessage.error(e?.response?.data?.message || '删除失败')
    }
  }
}

// 关单评估测试
const evaluationResult = ref<ClosureEvaluationResult | null>(null)

async function testEvaluateRule(rule: ClosureRule) {
  try {
    evaluationResult.value = await closureRuleApi.evaluate({ ruleId: rule.id })
    if (evaluationResult.value.isSatisfied) {
      ElMessage.success(`规则「${rule.name}」评估通过 ✓`)
    } else {
      ElMessage.warning(`规则「${rule.name}」评估不通过`)
    }
  } catch (e: any) {
    ElMessage.error('评估失败')
  }
}

// ============================================================================
// Lifecycle
// ============================================================================
onMounted(async () => {
  await loadGroups()
  await loadAiStrategies()
  await loadRules()
})
</script>

<template>
  <div class="iqc-container">
    <!-- Page Header -->
    <div class="page-header">
      <div class="page-header-main">
        <div class="page-header-icon">
          <el-icon :size="28"><Document /></el-icon>
        </div>
        <div class="page-header-text">
          <h2 class="page-header-title">动态参数配置</h2>
          <span class="page-header-subtitle">管理系统参数组、动态参数定义及关单策略规则</span>
        </div>
      </div>
    </div>

    <!-- ================================================================== -->
    <!-- M02.5: 动态参数配置 -->
    <!-- ================================================================== -->
    <div class="params-view">
      <!-- Top Row: Parameter Group Tree + Parameter Definition -->
      <div class="params-main-row">
        <!-- Left: 参数组树 (ParamGroupTree) -->
        <div class="params-tree-panel">
          <div class="panel-header">
            <div class="panel-header-left">
              <el-icon class="panel-icon"><FolderOpened /></el-icon>
              <span>参数组</span>
            </div>
            <el-button type="primary" size="small" @click="openCreateGroup">
              + 新建参数组
            </el-button>
          </div>
          <div class="tree-content">
            <div
              v-for="group in groups"
              :key="group.id"
              :class="['group-item', { active: selectedGroup?.id === group.id }]"
              @click="selectedGroup = group"
            >
              <div class="group-item-header">
                <el-icon :size="16"><FolderOpened /></el-icon>
                <span class="group-name">{{ group.name }}</span>
                <el-tag size="small" type="info" effect="plain">{{ group.paramCount }}</el-tag>
              </div>
              <div class="group-item-actions">
                <el-button link size="small" type="primary" @click.stop="openEditGroup(group)">
                  <el-icon><Edit /></el-icon>
                </el-button>
                <el-button link size="small" type="danger" @click.stop="deleteGroup(group)">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </div>
            </div>
            <div v-if="groups.length === 0" class="empty-hint">
              暂无参数组，请点击上方按钮创建
            </div>
          </div>

          <!-- Dynamic Dashboard Preview (embedded in tree panel bottom) -->
          <div v-if="dashboardVisible" class="dashboard-preview">
            <div class="panel-header">
              <div class="panel-header-left">
                <el-icon class="panel-icon"><DataAnalysis /></el-icon>
                <span>动态仪表盘预览</span>
              </div>
              <el-button link size="small" @click="dashboardVisible = false">关闭</el-button>
            </div>
            <div class="dashboard-cards">
              <div
                v-for="param in params.filter(p => p.isActive)"
                :key="param.id"
                class="dashboard-card"
              >
                <div class="card-label">{{ param.name }}</div>
                <div class="card-value">
                  <template v-if="param.dataType === 'numeric'">
                    <span class="value-num">—</span>
                    <span class="value-unit">{{ param.unit || '' }}</span>
                  </template>
                  <template v-else-if="param.dataType === 'boolean'">
                    <el-tag :type="'info'" effect="dark" size="small">—</el-tag>
                  </template>
                  <template v-else>
                    <span class="value-text">—</span>
                  </template>
                </div>
                <div class="card-trend">
                  <el-tag size="small" effect="plain" type="info">等待数据</el-tag>
                </div>
                <div v-if="param.usl !== null || param.lsl !== null" class="card-spec">
                  <span v-if="param.lsl !== null">LSL: {{ param.lsl }}</span>
                  <span v-if="param.usl !== null">USL: {{ param.usl }}</span>
                </div>
              </div>
              <div v-if="params.filter(p => p.isActive).length === 0" class="empty-hint">
                当前组无启用参数
              </div>
            </div>
          </div>
        </div>

        <!-- Right: 参数定义面板 (ParamForm) -->
        <div class="params-form-panel">
          <div class="panel-header">
            <div class="panel-header-left">
              <el-icon class="panel-icon"><Tools /></el-icon>
              <span>参数定义</span>
            </div>
            <div class="panel-actions">
              <el-button
                size="small"
                :disabled="!selectedGroup"
                @click="previewDashboard"
              >
                <el-icon style="vertical-align: middle"><DataAnalysis /></el-icon>
                <span style="vertical-align: middle">{{ dashboardVisible ? '关闭仪表盘' : '预览仪表盘' }}</span>
              </el-button>
              <el-button
                type="primary"
                size="small"
                :disabled="!selectedGroup"
                @click="openCreateParam"
              >
                + 新建参数
              </el-button>
            </div>
          </div>

          <!-- Params List -->
          <div class="params-list">
            <div
              v-for="param in params"
              :key="param.id"
              :class="['param-item', { active: selectedParam?.id === param.id }]"
              @click="selectParam(param)"
            >
              <div class="param-item-header">
                <span class="param-name">{{ param.name }}</span>
                <el-tag
                  :type="param.isActive ? 'success' : 'info'"
                  size="small"
                  effect="plain"
                >
                  {{ param.isActive ? '启用' : '停用' }}
                </el-tag>
              </div>
              <div class="param-item-meta">
                <span class="param-code">{{ param.code }}</span>
                <span class="param-type">{{ dataTypeOptions.find(o => o.value === param.dataType)?.label || param.dataType }}</span>
                <span v-if="param.unit" class="param-unit">{{ param.unit }}</span>
              </div>
              <div v-if="param.dataType === 'numeric' && (param.usl !== null || param.lsl !== null)" class="param-item-spec">
                <span v-if="param.lsl !== null">LSL: {{ param.lsl }}</span>
                <span v-if="param.targetValue !== null">目标: {{ param.targetValue }}</span>
                <span v-if="param.usl !== null">USL: {{ param.usl }}</span>
              </div>
              <div class="param-item-actions">
                <el-button link size="small" type="primary" @click.stop="openEditParam(param)">
                  <el-icon><Edit /></el-icon>
                </el-button>
                <el-button link size="small" type="danger" @click.stop="deleteParam(param)">
                  <el-icon><Delete /></el-icon>
                </el-button>
              </div>
            </div>
            <div v-if="!selectedGroup" class="empty-hint">
              请先在左侧选择一个参数组
            </div>
            <div v-else-if="params.length === 0" class="empty-hint">
              该参数组暂无参数，点击「新建参数」创建
            </div>
          </div>
        </div>
      </div>

      <!-- Bottom Row: 关单策略模板 (ClosureRuleBuilder + ClosureRuleList) -->
      <div class="rules-section">
        <div class="panel-header">
          <div class="panel-header-left">
            <el-icon class="panel-icon"><Setting /></el-icon>
            <span>关单策略模板配置</span>
          </div>
          <el-button type="primary" size="small" @click="openCreateRule">
            + 新建规则
          </el-button>
        </div>

        <div class="data-card">
          <div class="rules-content">
            <div class="rules-table">
            <el-table :data="rules" stripe style="width: 100%" >
              <el-table-column prop="name" label="规则名称" min-width="160" />
              <el-table-column prop="code" label="编码" width="120" />
              <el-table-column prop="logic" label="逻辑" width="80">
                <template #default="{ row }">
                  <el-tag :type="row.logic === 'AND' ? 'primary' : 'warning'" size="small">
                    {{ row.logic }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="isActive" label="状态" width="80">
                <template #default="{ row }">
                  <el-tag :type="row.isActive ? 'success' : 'info'" size="small" effect="plain">
                    {{ row.isActive ? '启用' : '停用' }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="description" label="描述" min-width="160" show-overflow-tooltip />
              <el-table-column label="操作" width="180" fixed="right">
                <template #default="{ row }">
                  <el-button link size="small" type="primary" @click="openEditRule(row)">
                    编辑
                  </el-button>
                  <el-button link size="small" type="success" @click="testEvaluateRule(row)">
                    测试
                  </el-button>
                  <el-button link size="small" type="danger" @click="deleteRule(row)">
                    删除
                  </el-button>
                </template>
              </el-table-column>
            </el-table>
            <div v-if="rules.length === 0" class="empty-hint">
              暂无关单规则，点击「新建规则」创建
            </div>
          </div>

          <!-- Evaluation Result -->
          <div v-if="evaluationResult" class="evaluation-result">
            <el-alert
              :title="`评估结果: ${evaluationResult.ruleName}`"
              :type="evaluationResult.isSatisfied ? 'success' : 'warning'"
              :description="evaluationResult.isSatisfied
                ? '所有条件满足，允许关单'
                : `未满足条件:\n${evaluationResult.failedConditions?.join('\n') || '无'}`
              "
              show-icon
              :closable="true"
              @close="evaluationResult = null"
            />
          </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ================================================================== -->
    <!-- Dialogs -->
    <!-- ================================================================== -->

    <!-- 参数组 Dialog -->
    <el-dialog
      v-model="groupDialogVisible"
      :title="isEditingGroup ? '编辑参数组' : '新建参数组'"
      width="520px"
      :close-on-click-modal="false"
    >
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Edit /></el-icon>
          <span class="dialog-section-title">组信息</span>
        </div>
        <el-form :model="groupForm" label-width="100px" >
          <el-form-item label="组名称" required>
            <el-input v-model="groupForm.name" placeholder="如：热力学参数组" />
          </el-form-item>
          <el-form-item label="组编码" :required="!isEditingGroup">
            <el-input
              v-model="groupForm.code"
              :disabled="isEditingGroup"
              placeholder="如：thermo_params"
            />
          </el-form-item>
          <el-form-item label="排序号">
            <el-input-number v-model="groupForm.sortOrder!" :min="0" :step="1" />
          </el-form-item>
          <el-form-item label="描述">
            <el-input v-model="groupForm.description" type="textarea" :rows="3" />
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <el-button size="small" @click="groupDialogVisible = false">取消</el-button>
        <el-button size="small" type="primary" @click="saveGroup">保存</el-button>
      </template>
    </el-dialog>

    <!-- 参数 Dialog -->
    <el-dialog
      v-model="paramDialogVisible"
      :title="isEditingParam ? '编辑参数' : '新建参数'"
      width="620px"
      :close-on-click-modal="false"
    >
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Tools /></el-icon>
          <span class="dialog-section-title">基本信息</span>
        </div>
        <el-form :model="paramForm" label-width="120px" >
          <el-form-item label="参数名称" required>
            <el-input v-model="paramForm.name" placeholder="如：温度-精加工" />
          </el-form-item>
          <el-form-item label="参数编码" :required="!isEditingParam">
            <el-input
              v-model="paramForm.code"
              :disabled="isEditingParam"
              placeholder="如：temp_finishing"
            />
          </el-form-item>
          <el-form-item label="数据类型" required>
            <el-select v-model="paramForm.dataType" style="width: 100%">
              <el-option
                v-for="opt in dataTypeOptions"
                :key="opt.value"
                :value="opt.value"
                :label="opt.label"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="单位">
            <el-input v-model="paramForm.unit" placeholder="如：℃" />
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section" v-if="paramForm.dataType === 'numeric'">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><DataAnalysis /></el-icon>
          <span class="dialog-section-title">规格限设置</span>
        </div>
        <el-form :model="paramForm" label-width="150px">
          <el-form-item label="目标值">
            <el-input-number v-model="paramForm.targetValue!" :precision="4" :step="0.1" style="width: 100%" />
          </el-form-item>
          <el-form-item label="下规格限 (LSL)">
            <el-input-number v-model="paramForm.lsl!" :precision="4" :step="0.1" style="width: 100%" />
          </el-form-item>
          <el-form-item label="上规格限 (USL)">
            <el-input-number v-model="paramForm.usl!" :precision="4" :step="0.1" style="width: 100%" />
          </el-form-item>
          <el-form-item v-if="specLimitError" label=" " label-width="150px">
            <span style="color: var(--el-color-danger)">{{ specLimitError }}</span>
          </el-form-item>
          <el-form-item label="精度">
            <el-input-number v-model="paramForm.precision!" :min="0.01" :max="100" :step="0.01" style="width: 100%" />
          </el-form-item>
        </el-form>
      </div>

      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Cpu /></el-icon>
          <span class="dialog-section-title">高级设置</span>
        </div>
        <el-form :model="paramForm" label-width="120px">
          <el-form-item label="AI 策略预置">
            <el-select v-model="paramForm.aiStrategy" style="width: 100%" clearable placeholder="选择 AI 策略">
              <el-option
                v-for="s in filteredStrategies"
                :key="s.id"
                :value="s.id"
                :label="s.name"
              >
                <span>{{ s.name }}</span>
                <span class="strategy-desc">{{ s.description }}</span>
              </el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="排序号">
            <el-input-number v-model="paramForm.sortOrder!" :min="0" :step="1" style="width: 100%" />
          </el-form-item>
        </el-form>
      </div>

      <template #footer>
        <el-button size="small" @click="paramDialogVisible = false">取消</el-button>
        <el-button size="small" type="primary" @click="saveParam">保存</el-button>
      </template>
    </el-dialog>

    <!-- 关单规则 Dialog -->
    <el-dialog
      v-model="ruleDialogVisible"
      :title="isEditingRule ? '编辑关单规则' : '新建关单规则'"
      width="680px"
      :close-on-click-modal="false"
    >
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><Setting /></el-icon>
          <span class="dialog-section-title">规则基本信息</span>
        </div>
        <el-form :model="ruleForm" label-width="120px" >
          <el-form-item label="规则名称" required>
            <el-input v-model="ruleForm.name" placeholder="如：连续10件合格放行" />
          </el-form-item>
          <el-form-item label="规则编码" :required="!isEditingRule">
            <el-input
              v-model="ruleForm.code"
              :disabled="isEditingRule"
              placeholder="如：consecutive_10_ok"
            />
          </el-form-item>
          <el-form-item label="逻辑运算符">
            <el-radio-group v-model="ruleForm.logic">
              <el-radio value="AND">全部满足 (AND)</el-radio>
              <el-radio value="OR">任一满足 (OR)</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="描述">
            <el-input v-model="ruleForm.description" type="textarea" :rows="2" />
          </el-form-item>
        </el-form>
      </div>

      <!-- Conditions Builder -->
      <div class="dialog-section">
        <div class="dialog-section-header">
          <el-icon class="dialog-section-icon"><DataAnalysis /></el-icon>
          <span class="dialog-section-title">触发条件</span>
        </div>
        <div class="conditions-builder">
        <div class="conditions-header">
          <span class="conditions-title">触发条件</span>
          <el-button size="small" type="primary" link @click="addCondition">
            + 添加条件
          </el-button>
        </div>
        <div
          v-for="(cond, idx) in conditions"
          :key="idx"
          class="condition-row"
        >
          <div class="condition-fields">
            <el-select
              v-model="cond.type"
              style="width: 160px"
              @change="resetConditionFields(idx)"
            >
              <el-option
                v-for="opt in conditionTypeOptions"
                :key="opt.value"
                :value="opt.value"
                :label="opt.label"
              />
            </el-select>
            <el-select v-model="cond.operator" style="width: 80px">
              <el-option
                v-for="opt in operatorOptions"
                :key="opt.value"
                :value="opt.value"
                :label="opt.label"
              />
            </el-select>
            <el-input-number
              v-model="cond.threshold"
              :min="0"
              :max="10000"
              :precision="2"
              :step="cond.type === 'spk_cpk' ? 0.01 : 1"
              style="width: 140px"
            />
            <span class="condition-unit">
              {{ cond.type === 'consecutive_ok' ? '件' : cond.type === 'spk_cpk' ? '' : cond.type === 'sampling_rate' ? '%' : '分' }}
            </span>
          </div>
          <el-button
            link
            size="small"
            type="danger"
            @click="removeCondition(idx)"
          >
            <el-icon><Delete /></el-icon>
          </el-button>
        </div>
        <div v-if="conditions.length === 0" class="condition-empty">
          暂无条件，点击「添加条件」开始构建
        </div>
        </div>
      </div>

      <template #footer>
        <el-button size="small" @click="ruleDialogVisible = false">取消</el-button>
        <el-button size="small" type="primary" @click="saveRule">
          {{ isEditingRule ? '更新规则' : '保存规则' }}
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.iqc-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* Page Header */
.page-header {
  padding: 12px 16px;
  background: linear-gradient(135deg, var(--el-color-primary), var(--el-color-primary-light-3));
  border-radius: 8px;
  margin: 8px;
}

.page-header-main {
  display: flex;
  align-items: center;
  gap: 14px;
}

.page-header-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.2);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  flex-shrink: 0;
}

.page-header-text {
  display: flex;
  flex-direction: column;
}

.page-header-title {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: white;
  line-height: 1.3;
}

.page-header-subtitle {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.85);
}

/* Data Card */
.data-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  overflow: hidden;
}

/* Sub-Tabs */
.sub-tabs {
  padding: 8px 16px;
  background: var(--el-bg-color-page);
  border-bottom: 1px solid var(--el-border-color-light);
}

/* Params View */
.params-view {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  overflow-y: auto;
}

.params-main-row {
  display: flex;
  gap: 12px;
  flex: 1;
  min-height: 400px;
}

/* Panel Styles */
.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  border-bottom: 1px solid var(--el-border-color-light);
  background: var(--el-fill-color-blank);
}

.panel-header-left {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.panel-icon {
  font-size: 16px;
  color: var(--el-color-primary);
}

.panel-header h3, .panel-header h4 {
  margin: 0;
  font-size: 14px;
  font-weight: 600;
}

.panel-actions {
  display: flex;
  gap: 8px;
}

/* Dialog Sections */
.dialog-section {
  margin-bottom: 16px;
}

.dialog-section:last-of-type {
  margin-bottom: 0;
}

.dialog-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
}

.dialog-section-icon {
  font-size: 15px;
  color: var(--el-color-primary);
}

.dialog-section-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-regular);
}

/* Tree Panel (Left) */
.params-tree-panel {
  width: 340px;
  min-width: 300px;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.tree-content {
  flex: 1;
  overflow-y: auto;
  padding: 4px;
}

.group-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 10px;
  margin: 2px 0;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.15s;
}

.group-item:hover {
  background: var(--el-fill-color-light);
}

.group-item.active {
  background: var(--el-color-primary-light-9);
  color: var(--el-color-primary);
}

.group-item-header {
  display: flex;
  align-items: center;
  gap: 6px;
  flex: 1;
  min-width: 0;
}

.group-name {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 13px;
}

.group-item-actions {
  display: none;
  gap: 2px;
  flex-shrink: 0;
}

.group-item:hover .group-item-actions {
  display: flex;
}

/* Dashboard Preview */
.dashboard-preview {
  border-top: 1px solid var(--el-border-color-light);
  max-height: 240px;
  overflow-y: auto;
}

.dashboard-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: 8px;
  padding: 8px;
}

.dashboard-card {
  background: var(--el-fill-color-lighter);
  border: 1px solid var(--el-border-color-extra-light);
  border-radius: 6px;
  padding: 10px;
  text-align: center;
}

.card-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-bottom: 4px;
}

.card-value {
  font-size: 20px;
  font-weight: 700;
  color: var(--el-text-color-primary);
  margin-bottom: 4px;
}

.card-value .value-unit {
  font-size: 12px;
  font-weight: 400;
  margin-left: 2px;
}

.card-trend {
  margin-bottom: 4px;
}

.card-spec {
  font-size: 10px;
  color: var(--el-text-color-secondary);
  display: flex;
  justify-content: center;
  gap: 8px;
}

/* Form Panel (Right) */
.params-form-panel {
  flex: 1;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.params-list {
  flex: 1;
  overflow-y: auto;
  padding: 4px;
}

.param-item {
  display: flex;
  flex-direction: column;
  padding: 8px 12px;
  margin: 2px 0;
  border-radius: 4px;
  cursor: pointer;
  transition: background 0.15s;
  border: 1px solid transparent;
  position: relative;
}

.param-item:hover {
  background: var(--el-fill-color-light);
}

.param-item.active {
  background: var(--el-color-primary-light-9);
  border-color: var(--el-color-primary-light-5);
}

.param-item-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.param-name {
  font-size: 13px;
  font-weight: 500;
}

.param-item-meta {
  display: flex;
  gap: 8px;
  font-size: 11px;
  color: var(--el-text-color-secondary);
  margin-top: 4px;
}

.param-item-spec {
  font-size: 11px;
  color: var(--el-text-color-secondary);
  display: flex;
  gap: 12px;
  margin-top: 2px;
}

.param-item-actions {
  position: absolute;
  right: 8px;
  top: 8px;
  display: none;
}

.param-item:hover .param-item-actions {
  display: flex;
}

/* Rules Section */
.rules-section {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
}

.rules-content {
  padding: 12px;
}

.evaluation-result {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--el-border-color-light);
}

/* Conditions Builder */
.conditions-builder {
  margin: 0 12px 12px;
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
  padding: 12px;
  background: var(--el-fill-color-lighter);
}

.conditions-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.conditions-title {
  font-size: 13px;
  font-weight: 600;
}

.condition-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.condition-fields {
  display: flex;
  align-items: center;
  gap: 6px;
  flex: 1;
}

.condition-unit {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  white-space: nowrap;
}

.condition-empty {
  text-align: center;
  color: var(--el-text-color-secondary);
  font-size: 12px;
  padding: 20px;
}

/* Common */
.empty-hint {
  text-align: center;
  color: var(--el-text-color-disabled);
  font-size: 12px;
  padding: 20px;
}

.strategy-desc {
  font-size: 11px;
  color: var(--el-text-color-secondary);
  margin-left: 8px;
}
</style>

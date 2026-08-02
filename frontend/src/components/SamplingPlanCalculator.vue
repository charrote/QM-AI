<template>
  <!-- ===================== Drawer 模式 ===================== -->
  <RightPanel
    v-if="mode === 'drawer'"
    v-model:visible="visible"
    title="GB/T 2828.1 抽样方案计算器"
    :width="480"
  >
    <template #body>
      <div class="sampling-drawer-content">
        <el-form label-width="80px" label-position="left">
          <el-form-item label="批量">
            <el-input-number
              v-model="form.lotSize"
              :min="1"
              :max="500000"
              style="width: 100%"
            />
          </el-form-item>
          <el-form-item label="检验水平">
            <el-select v-model="form.samplingLevel" style="width: 100%">
              <el-option
                v-for="opt in SAMPLING_LEVEL_OPTIONS"
                :key="opt.value"
                :label="opt.label"
                :value="opt.value"
              />
            </el-select>
          </el-form-item>
          <el-form-item label="AQL 值">
            <el-input-number
              v-model="form.aqlValue"
              :min="0.01"
              :step="0.1"
              :precision="2"
              style="width: 100%"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" size="small" @click="handleCalculate" style="width: 100%">
              计算抽样方案
            </el-button>
          </el-form-item>
        </el-form>

        <div v-if="result" class="sampling-result-card">
          <div class="result-title">计算结果</div>
          <div class="result-grid">
            <div class="result-item">
              <div class="result-label">字母代码</div>
              <div class="result-value">{{ result.sampleCode }}</div>
            </div>
            <div class="result-item">
              <div class="result-label">样本量</div>
              <div class="result-value result-success">{{ result.sampleSize }}</div>
            </div>
            <div class="result-item">
              <div class="result-label">Ac</div>
              <div class="result-value result-warning">{{ result.ac }}</div>
            </div>
            <div class="result-item">
              <div class="result-label">Re</div>
              <div class="result-value result-danger">{{ result.re }}</div>
            </div>
          </div>
        </div>
      </div>
    </template>
    <template #footer>
      <div style="display: flex; gap: 8px; justify-content: flex-end;">
        <el-button size="small" @click="handleReset">重置</el-button>
        <el-button size="small" type="primary" @click="handleCalculate">重新计算</el-button>
      </div>
    </template>
  </RightPanel>

  <!-- ===================== Panel 模式 (内联折叠面板) ===================== -->
  <div v-else-if="mode === 'panel'" class="sampling-panel">
    <el-collapse v-model="activeNames">
      <el-collapse-item name="sampling">
        <template #title>
          <el-icon style="vertical-align: middle"><ScaleToOriginal /></el-icon>
          <span style="vertical-align: middle">GB/T 2828.1 抽样方案计算器</span>
        </template>
        <el-row :gutter="16" style="margin-bottom: 8px">
          <el-col :span="6">
            <el-form-item label="批量">
              <el-input-number
                v-model="form.lotSize"
                :min="1"
                :max="500000"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="检验水平">
              <el-select v-model="form.samplingLevel" style="width: 100%">
                <el-option
                  v-for="opt in SAMPLING_LEVEL_OPTIONS"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="AQL 值">
              <el-input-number
                v-model="form.aqlValue"
                :min="0.01"
                :step="0.1"
                :precision="2"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6" style="display: flex; align-items: flex-start; padding-top: 2px">
            <el-button type="primary" size="small" @click="handleCalculate">计算</el-button>
          </el-col>
        </el-row>
        <div v-if="result" class="sampling-result">
          <el-tag>字母代码: {{ result.sampleCode }}</el-tag>
          <el-tag type="success">样本量: {{ result.sampleSize }}</el-tag>
          <el-tag type="warning">Ac: {{ result.ac }}</el-tag>
          <el-tag type="danger">Re: {{ result.re }}</el-tag>
        </div>
      </el-collapse-item>
    </el-collapse>
  </div>

  <!-- ===================== Inline 模式 (可折叠内联) ===================== -->
  <el-collapse-transition v-else>
    <div v-show="showInline" class="sampling-section">
      <div class="sampling-header" @click="toggleInline">
        <span>
          <el-icon><ScaleToOriginal /></el-icon>
          GB/T 2828.1 抽样方案计算器
        </span>
        <el-icon class="collapse-arrow" :class="{ collapsed: !showInline }">
          <CaretBottom />
        </el-icon>
      </div>
      <div v-show="showInline" class="sampling-body">
        <el-row :gutter="16">
          <el-col :span="6">
            <el-form-item label="批量">
              <el-input-number
                v-model="form.lotSize"
                :min="1"
                :max="500000"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="检验水平">
              <el-select v-model="form.samplingLevel" style="width: 100%">
                <el-option
                  v-for="opt in SAMPLING_LEVEL_OPTIONS"
                  :key="opt.value"
                  :label="opt.label"
                  :value="opt.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="6">
            <el-form-item label="AQL 值">
              <el-input-number
                v-model="form.aqlValue"
                :min="0.01"
                :step="0.1"
                :precision="2"
                style="width: 100%"
              />
            </el-form-item>
          </el-col>
          <el-col :span="6" style="display: flex; align-items: flex-start; padding-top: 2px">
            <el-button type="primary" size="small" @click="handleCalculate">计算</el-button>
          </el-col>
        </el-row>
        <div v-if="result" class="sampling-result-inline">
          <div class="result-tag">
            <el-icon><Document /></el-icon> 字母代码: {{ result.sampleCode }}
          </div>
          <div class="result-tag result-success">
            <el-icon><CircleCheck /></el-icon> 样本量: {{ result.sampleSize }}
          </div>
          <div class="result-tag result-warning">
            <el-icon><WarningFilled /></el-icon> Ac: {{ result.ac }}
          </div>
          <div class="result-tag result-danger">
            <el-icon><CircleClose /></el-icon> Re: {{ result.re }}
          </div>
        </div>
      </div>
    </div>
  </el-collapse-transition>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { ScaleToOriginal, Document, WarningFilled, CircleCheck, CircleClose, CaretBottom } from '@element-plus/icons-vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import { SAMPLING_LEVEL_OPTIONS } from '@/types/iqc'
import { useSamplingPlan } from '@/composables/useSamplingPlan'

/** 组件模式 */
type CalculatorMode = 'drawer' | 'panel' | 'inline'

defineOptions({ name: 'SamplingPlanCalculator' })

const props = withDefaults(defineProps<{
  /** 显示模式 */
  mode?: CalculatorMode
  /** 初始是否自动计算 */
  autoCalculate?: boolean
  /** Drawer 模式下的可见性 (v-model) */
  modelValue?: boolean
}>(), {
  mode: 'panel',
  autoCalculate: true,
  modelValue: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
}>()

// ─── 计算逻辑 (composable) ─────────────────────────────
const { form, result, loading, calculate, reset } = useSamplingPlan()

// ─── Drawer 模式 ──────────────────────────────────────
const visible = computed({
  get: () => props.modelValue,
  set: (val: boolean) => emit('update:modelValue', val),
})

// ─── Panel 模式 ───────────────────────────────────────
const activeNames = ref<string[]>(['sampling'])

// ─── Inline 模式 ──────────────────────────────────────
const showInline = ref(true)
function toggleInline() {
  showInline.value = !showInline.value
}

// ─── 统一操作 ─────────────────────────────────────────
function handleCalculate() {
  calculate()
}

function handleReset() {
  reset()
}

// ─── 生命周期 ─────────────────────────────────────────
if (props.autoCalculate) {
  onMounted(() => {
    calculate()
  })
}

// 组件卸载前自动关闭 drawer，防止干扰路由过渡
if (props.mode === 'drawer') {
  onBeforeUnmount(() => {
    emit('update:modelValue', false)
  })
}
</script>

<style scoped>
/* ─── Drawer 模式 ────────────────────────────────────── */
.sampling-drawer-content {
  padding: 0 20px;
}

.sampling-result-card {
  margin-top: 20px;
  padding: 16px;
  background: var(--el-fill-color-blank);
  border: 1px solid var(--el-border-color-light);
  border-radius: 6px;
}

.result-title {
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin-bottom: 12px;
}

.result-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.result-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.result-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.result-value {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.result-success { color: var(--el-color-success); }
.result-warning { color: var(--el-color-warning); }
.result-danger { color: var(--el-color-danger); }

/* ─── Panel 模式 ─────────────────────────────────────── */
.sampling-panel {
  margin-top: 8px;
}

.sampling-result {
  display: flex;
  gap: 8px;
  align-items: center;
  flex-wrap: wrap;
}

/* ─── Inline 模式 ────────────────────────────────────── */
.sampling-section {
  border-top: 1px solid var(--el-border-color-light);
  background: var(--el-fill-color-blank);
}

.sampling-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  font-size: 14px;
  font-weight: 500;
  color: var(--el-text-color-regular);
  cursor: pointer;
  user-select: none;
  transition: background 0.2s;
}

.sampling-header:hover {
  background: var(--el-fill-color-light);
}

.sampling-header .el-icon {
  margin-right: 6px;
  vertical-align: middle;
}

.collapse-arrow {
  transition: transform 0.3s;
}

.collapse-arrow.collapsed {
  transform: rotate(-90deg);
}

.sampling-body {
  padding: 12px 14px 16px;
}

.sampling-result-inline {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px dashed var(--el-border-color-light);
}

.result-tag {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 14px;
  background: var(--el-fill-color-lighter);
  border: 1px solid var(--el-border-color-extra-light);
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  color: var(--el-text-color-regular);
}

.result-success {
  background: var(--el-color-success-light-9);
  color: var(--el-color-success);
}

.result-warning {
  background: var(--el-color-warning-light-9);
  color: var(--el-color-warning);
}

.result-danger {
  background: var(--el-color-danger-light-9);
  color: var(--el-color-danger);
}
</style>

<template>
  <RightPanel v-model:visible="visible" title="克隆工艺路线" :width="600">
    <template #body>
      <div class="clone-drawer">
        <!-- 源路线 -->
        <div class="clone-section">
          <div class="clone-section__title">
            <el-icon><CopyDocument /></el-icon>
            <span>源路线</span>
          </div>
          <div class="clone-source-row">
            <el-select
              v-model="sourceProductId"
              filterable
              placeholder="选择源产品"
              style="flex: 1"
              @change="onSourceProductChange"
            >
              <el-option
                v-for="p in products"
                :key="p.id"
                :label="`${p.code} - ${p.name}`"
                :value="p.id"
              />
            </el-select>
            <el-select
              v-model="sourceHeaderId"
              filterable
              placeholder="选择源路线"
              style="flex: 1; margin-left: 10px"
              :disabled="!sourceProductId"
              @change="onSourceHeaderChange"
            >
              <el-option
                v-for="route in sourceRoutes"
                :key="route.id"
                :label="`${route.routeCode} - ${route.routeName}`"
                :value="route.id"
              />
            </el-select>
          </div>
        </div>

        <!-- 源路线步骤预览 -->
        <div v-if="sourceSteps.length > 0" class="clone-preview">
          <div class="clone-preview__header">
            <el-icon><List /></el-icon>
            <span>源路线包含 {{ sourceSteps.length }} 个工序步骤</span>
          </div>
          <div class="clone-preview__list">
            <div
              v-for="(step, idx) in sourceSteps"
              :key="step.id"
              class="clone-preview__item"
            >
              <span class="clone-preview__step-num">{{ idx + 1 }}</span>
              <span class="clone-preview__step-name">{{ step.processName }}</span>
              <span class="clone-preview__step-code">{{ step.processCode }}</span>
              <span v-if="step.standardTimeMinutes" class="clone-preview__step-time">
                {{ step.standardTimeMinutes }} min
              </span>
            </div>
          </div>
        </div>

        <el-divider v-if="sourceSteps.length > 0" />

        <!-- 目标路线信息 -->
        <div class="clone-section">
          <div class="clone-section__title">
            <el-icon><DocumentChecked /></el-icon>
            <span>目标路线信息</span>
          </div>
          <el-form label-width="80px" size="default">
            <el-form-item label="目标产品">
              <el-input
                :value="defaultProductName"
                readonly
                placeholder="默认当前选中产品"
                style="width: 100%"
              >
                <template #prefix>
                  <el-icon><Goods /></el-icon>
                </template>
              </el-input>
            </el-form-item>
            <el-form-item label="路线编号">
              <el-input
                v-model="targetRouteCode"
                placeholder="如 ALT-001"
                maxlength="50"
              />
            </el-form-item>
            <el-form-item label="路线名称">
              <el-input
                v-model="targetRouteName"
                placeholder="如 替代路线"
                maxlength="200"
              />
            </el-form-item>
            <el-form-item label="路线类型">
              <el-radio-group v-model="targetRouteType">
                <el-radio
                  v-for="opt in routeTypeOptions"
                  :key="opt.value"
                  :value="opt.value"
                >
                  {{ opt.label }}
                </el-radio>
              </el-radio-group>
            </el-form-item>
          </el-form>
        </div>

        <!-- 风险提示 -->
        <el-alert
          v-if="sourceSteps.length > 0 && targetProductId && targetRouteCode"
          title="确认克隆"
          :description="`将新建一条「${targetRouteName || targetRouteCode}」路线（含 ${sourceSteps.length} 个工序步骤）至「${targetProduct?.name}」。`"
          type="warning"
          :closable="false"
          show-icon
        />
      </div>
    </template>
    <template #footer>
      <div class="drawer-footer">
        <el-button @click="visible = false">取消</el-button>
        <el-button
          type="primary"
          :disabled="!canClone"
          :loading="cloning"
          @click="handleClone"
        >
          确认克隆
        </el-button>
      </div>
    </template>
  </RightPanel>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Goods } from '@element-plus/icons-vue'
import RightPanel from '@/components/layout/RightPanel.vue'
import type { Product } from '@/types/basicData'
import type { RouteHeaderDto, ProductRouteStepDto, RouteType, RouteDetailDto } from '@/types/routing'
import { ROUTE_TYPE_OPTIONS } from '@/types/routing'
import { productApi } from '@/api/basicData'
import { getRouteHeaders, getRouteDetail } from '@/api/routing'
import { cloneRouteHeader } from '@/api/routing'

defineOptions({ name: 'CloneDrawer' })

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  cloned: []
}>()

const props = defineProps<{
  modelValue: boolean
  defaultProductId?: number
}>()

const visible = ref(props.modelValue)
const cloning = ref(false)
const products = ref<Product[]>([])
const sourceProductId = ref<number | undefined>()
const sourceHeaderId = ref<number | undefined>()
const targetProductId = ref<number | undefined>()
const targetRouteCode = ref('')
const targetRouteName = ref('')
const targetRouteType = ref<RouteType>('ALT')
const sourceRoutes = ref<RouteHeaderDto[]>([])
const sourceSteps = ref<ProductRouteStepDto[]>([])
const routeTypeOptions = ROUTE_TYPE_OPTIONS
// 追踪是否刚刚执行了克隆操作（用于区分"取消关闭"和"克隆成功关闭"）
let justCloned = false

const targetProduct = computed(() => products.value.find(p => p.id === targetProductId.value))

const defaultProductName = computed(() => {
  if (!props.defaultProductId) return ''
  const product = products.value.find(p => p.id === props.defaultProductId)
  return product ? `${product.code} - ${product.name}` : ''
})

const canClone = computed(() =>
  !!sourceHeaderId.value &&
  !!targetProductId.value &&
  !!targetRouteCode.value &&
  sourceSteps.value.length > 0
)

watch(() => props.modelValue, (val) => {
  // 同步父组件传过来的 modelValue 值（处理外部修改，如刷新等）
  if (visible.value !== val) {
    visible.value = val
  }
  if (val) init()
}, { immediate: true })

// 当 drawer 关闭时，通知父组件更新 v-model
watch(visible, (val) => {
  if (!val && !justCloned) {
    emit('update:modelValue', false)
  }
  justCloned = false
})

async function init() {
  sourceProductId.value = undefined
  sourceHeaderId.value = undefined
  targetProductId.value = props.defaultProductId || undefined
  targetRouteCode.value = ''
  targetRouteName.value = ''
  targetRouteType.value = 'ALT'
  sourceRoutes.value = []
  sourceSteps.value = []

  try {
    const res = await productApi.list({ page: 1, pageSize: 999 })
    products.value = res.items
  } catch { /* ignore */ }
}

async function onSourceProductChange() {
  sourceHeaderId.value = undefined
  sourceRoutes.value = []
  sourceSteps.value = []

  if (!sourceProductId.value) {
    // 目标产品保持不变（锁定为当前选中产品）
    return
  }

  // 目标产品始终为当前选中产品，不随源产品变化
  // targetProductId.value 已在 init() 中设置为 props.defaultProductId

  try {
    const result = await getRouteHeaders(sourceProductId.value)
    sourceRoutes.value = result.routes || []
  } catch {
    sourceRoutes.value = []
  }
}

async function onSourceHeaderChange() {
  if (!sourceHeaderId.value) {
    sourceSteps.value = []
    return
  }

  try {
    const detail: RouteDetailDto = await getRouteDetail(sourceHeaderId.value)
    sourceSteps.value = detail.steps
  } catch {
    sourceSteps.value = []
  }
}

async function handleClone() {
  if (!sourceHeaderId.value || !targetProductId.value || !targetRouteCode.value) {
    if (!targetProductId.value) {
      ElMessage.warning('请先选择目标产品')
    } else if (!sourceHeaderId.value) {
      ElMessage.warning('请先选择源路线')
    } else {
      ElMessage.warning('请填写路线编号')
    }
    return
  }

  cloning.value = true
  try {
    await cloneRouteHeader({
      sourceHeaderId: sourceHeaderId.value,
      targetProductId: targetProductId.value,
      targetRouteCode: targetRouteCode.value,
      targetRouteName: targetRouteName.value || targetRouteCode.value,
      targetRouteType: targetRouteType.value,
    })
    ElMessage.success('路线克隆成功')
    justCloned = true
    visible.value = false
  } catch { /* error handled by interceptor */ }
  finally {
    cloning.value = false
  }
}
</script>

<style scoped>
.clone-drawer {
  padding: 10px 10px 0;
}

.clone-section {
  margin-bottom: 20px;
}

.clone-section__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin-bottom: 10px;
}

.clone-section__title .el-icon {
  font-size: 16px;
}

.clone-source-row {
  display: flex;
  align-items: center;
}

.clone-preview {
  margin-bottom: 20px;
}

.clone-preview__header {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 500;
  color: var(--el-text-color-regular);
  margin-bottom: 10px;
}

.clone-preview__header .el-icon {
  font-size: 15px;
}

.clone-preview__list {
  max-height: 260px;
  overflow-y: auto;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 8px;
  padding: 4px 0;
}

.clone-preview__item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 14px;
  font-size: 13px;
  color: var(--el-text-color-regular);
  transition: background 0.15s;
}

.clone-preview__item:hover {
  background: var(--el-fill-color-light);
}

.clone-preview__step-num {
  width: 22px;
  height: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--el-color-primary-light-9);
  color: var(--el-color-primary);
  font-size: 11px;
  font-weight: 600;
  border-radius: 50%;
  flex-shrink: 0;
}

.clone-preview__step-name {
  font-weight: 500;
  flex: 1;
}

.clone-preview__step-code {
  color: var(--el-text-color-secondary);
  font-size: 12px;
}

.clone-preview__step-time {
  color: var(--el-text-color-regular);
  font-size: 12px;
  white-space: nowrap;
}

.drawer-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}
</style>

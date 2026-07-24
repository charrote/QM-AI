<template>
  <el-drawer
    v-model="visible"
    :title="isEdit ? '编辑工序步骤' : '新增工序步骤'"
    size="520px"
    direction="rtl"
    :close-on-click-modal="false"
    :show-close="false"
    :destroy-on-close="false"
    :append-to-body="true"
    @keydown.esc.prevent
  >
    <el-form
      ref="formRef"
      :model="formData"
      label-width="100px"
      label-position="top"
      size="default"
      :rules="rules"
      class="edit-form"
    >
      <el-form-item label="工序" prop="processId">
        <el-select
          v-model="formData.processId"
          filterable
          remote
          :remote-method="searchProcess"
          :loading="processLoading"
          placeholder="搜索工序名称或编码"
          style="width: 100%"
          @change="onProcessChange"
        >
          <el-option
            v-for="p in processOptions"
            :key="p.id"
            :label="`${p.code} - ${p.name}`"
            :value="p.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="标准工时(min)" prop="standardTimeMinutes">
        <el-input-number
          v-model="formData.standardTimeMinutes"
          :min="0"
          :max="9999"
          :step="0.5"
          :precision="1"
          style="width: 100%"
          placeholder="请输入标准工时"
        />
      </el-form-item>

      <el-form-item label="备注" prop="description">
        <el-input
          v-model="formData.description"
          type="textarea"
          :rows="3"
          placeholder="请输入备注（可选）"
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="drawer-footer">
        <el-button @click="visible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">
          {{ isEdit ? '保存' : '添加' }}
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue'
import { ElMessage } from 'element-plus'
import type { Process } from '@/types/basicData'
import { processApi } from '@/api/basicData'
import { updateStep, batchCreateSteps } from '@/api/routing'

defineOptions({ name: 'RouteEditDrawer' })

const emit = defineEmits<{
  saved: []
}>()

const props = defineProps<{
  modelValue: boolean
  productId: number
  stepId?: number // 编辑模式时有值
  step?: import('@/types/routing').ProductRouteStepDto | null // 编辑模式时有值
}>()

const visible = ref(props.modelValue)
const saving = ref(false)
const formRef = ref()
const processLoading = ref(false)
const processOptions = ref<Process[]>([])

const isEdit = ref(!!props.stepId)
const formData = reactive({
  processId: 0,
  standardTimeMinutes: undefined as number | undefined,
  description: '',
})

const rules = {
  processId: [{ required: true, message: '请选择工序', trigger: 'change' }],
}

watch(() => props.modelValue, (val) => {
  visible.value = val
  if (val) initForm()
})

watch(visible, (val) => {
  if (!val) emit('saved')
})

async function initForm() {
  // 加载工序列表
  try {
    const res = await processApi.list({ page: 1, pageSize: 999 })
    processOptions.value = res.items
  } catch { /* ignore */ }

  if (props.step) {
    // 编辑模式
    isEdit.value = true
    formData.processId = props.step.processId
    formData.standardTimeMinutes = props.step.standardTimeMinutes
    formData.description = props.step.description || ''
  } else {
    // 新增模式
    isEdit.value = false
    formData.processId = 0
    formData.standardTimeMinutes = undefined
    formData.description = ''
  }
}

function searchProcess(query: string) {
  if (!query) {
    processOptions.value = []
    return
  }
  processLoading.value = true
  setTimeout(async () => {
    try {
      const res = await processApi.list({ page: 1, pageSize: 50, keyword: query })
      processOptions.value = res.items
    } catch { /* ignore */ }
    processLoading.value = false
  }, 300)
}

function onProcessChange() {
  // 工序变化后的额外逻辑（如自动填充工时等）
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    if (isEdit.value && props.stepId) {
      await updateStep(props.stepId, {
        processId: formData.processId,
        standardTimeMinutes: formData.standardTimeMinutes,
        description: formData.description,
      })
      ElMessage.success('更新成功')
    } else {
      await batchCreateSteps([{
        productId: props.productId,
        processId: formData.processId,
        standardTimeMinutes: formData.standardTimeMinutes,
        description: formData.description,
      }])
      ElMessage.success('添加成功')
    }
    visible.value = false
  } catch { /* error handled by interceptor */ }
  finally {
    saving.value = false
  }
}
</script>

<style scoped>
.edit-form {
  padding: 10px 20px 0;
}

.drawer-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}
</style>
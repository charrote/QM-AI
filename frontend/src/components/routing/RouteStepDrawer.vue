<template>
  <el-drawer
    :model-value="modelValue"
    @update:model-value="handleUpdate"
    :title="drawerTitle"
    size="520px"
    direction="rtl"
    :close-on-click-modal="false"
    :show-close="false"
    :destroy-on-close="true"
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
          placeholder="请选择工序"
          style="width: 100%"
          clearable
        >
          <el-option
            v-for="p in processOptions"
            :key="p.id"
            :label="`${p.code} - ${p.name}`"
            :value="p.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="标准工时 (min)" prop="standardTimeMinutes">
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
        <el-button @click="handleCancel">取消</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">
          {{ isEdit ? '保存' : '添加' }}
        </el-button>
      </div>
    </template>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import type { Process } from '@/types/basicData'
import type { ProductRouteStepDto } from '@/types/routing'
import { processApi } from '@/api/basicData'
import { addRouteStep, updateRouteStep } from '@/api/routing'

defineOptions({ name: 'RouteStepDrawer' })

const emit = defineEmits<{
  saved: []
}>()

const props = defineProps<{
  modelValue: boolean
  productId: number
  headerId?: number
  stepId?: number
  step?: ProductRouteStepDto | null
}>()

const saving = ref(false)
const formRef = ref()
const processOptions = ref<Process[]>([])

const isEdit = computed(() => !!props.stepId)
const drawerTitle = computed(() => isEdit.value ? '编辑工序步骤' : '新增工序步骤')

const formData = reactive({
  processId: 0,
  standardTimeMinutes: undefined as number | undefined,
  description: '',
})

const rules = {
  processId: [{ required: true, message: '请选择工序', trigger: 'change' }],
}

// 完全由父组件 modelValue 控制，不再维护内部 visible ref
watch(() => props.modelValue, (val) => {
  if (val) initForm()
})

function handleUpdate(val: boolean) {
  emit('update:modelValue', val)
  if (!val) emit('saved')
}

async function initForm() {
  try {
    const res = await processApi.list({ page: 1, pageSize: 999 })
    processOptions.value = res.items
  } catch { /* ignore */ }

  if (props.step) {
    formData.processId = props.step.processId
    formData.standardTimeMinutes = props.step.standardTimeMinutes
    formData.description = props.step.description || ''
  } else {
    formData.processId = 0
    formData.standardTimeMinutes = undefined
    formData.description = ''
  }
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    if (isEdit.value && props.stepId && props.headerId) {
      await updateRouteStep(props.headerId, props.stepId, {
        processId: formData.processId,
        standardTimeMinutes: formData.standardTimeMinutes,
        description: formData.description,
      })
      ElMessage.success('更新成功')
    } else if (props.headerId) {
      await addRouteStep(props.headerId, {
        processId: formData.processId,
        standardTimeMinutes: formData.standardTimeMinutes,
        description: formData.description,
      })
      ElMessage.success('添加成功')
    }
    handleUpdate(false)
  } catch { /* error handled by interceptor */ }
  finally {
    saving.value = false
  }
}

function handleCancel() {
  handleUpdate(false)
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
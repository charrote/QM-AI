<template>
  <RightPanel v-model:visible="visible" :title="isEdit ? '编辑工艺路线' : '新建工艺路线'" :width="520">
    <template #body>
      <el-form
        ref="formRef"
        :model="formData"
        :rules="rules"
        label-width="80px"
        label-position="top"
        size="default"
        class="header-form"
      >
        <el-form-item label="路线编号" prop="routeCode">
          <el-input
            v-model="formData.routeCode"
            :disabled="isEdit"
            placeholder="如 STD-001"
            maxlength="50"
          />
        </el-form-item>

        <el-form-item label="路线名称" prop="routeName">
          <el-input v-model="formData.routeName" placeholder="如 标准路线" maxlength="200" />
        </el-form-item>

        <el-form-item label="路线类型" prop="routeType">
          <el-radio-group v-model="formData.routeType">
            <el-radio
              v-for="opt in routeTypeOptions"
              :key="opt.value"
              :value="opt.value"
            >
              {{ opt.label }}
            </el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="描述">
          <el-input
            v-model="formData.description"
            type="textarea"
            :rows="3"
            placeholder="工艺路线描述（可选）"
            maxlength="500"
          />
        </el-form-item>

        <el-form-item label="设为默认">
          <el-switch v-model="formData.isDefault" />
        </el-form-item>
      </el-form>
    </template>
    <template #footer>
      <div class="drawer-footer">
        <el-button @click="visible = false">取消</el-button>
        <el-button type="primary" :loading="saving" @click="handleSave">
          {{ isEdit ? '保存' : '创建' }}
        </el-button>
      </div>
    </template>
  </RightPanel>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch } from 'vue'
import { ElMessage } from 'element-plus'
import RightPanel from '@/components/layout/RightPanel.vue'
import type { RouteHeaderDto, CreateRouteHeaderDto, RouteType } from '@/types/routing'
import { ROUTE_TYPE_OPTIONS } from '@/types/routing'
import { createRouteHeader, updateRouteHeader } from '@/api/routing'

defineOptions({ name: 'RouteHeaderDrawer' })

const emit = defineEmits<{
  saved: [headerId: number]
  closed: []
}>()

const props = defineProps<{
  modelValue: boolean
  productId: number
  headerId?: number
  header?: RouteHeaderDto | null
}>()

const visible = ref(props.modelValue)
const saving = ref(false)
const formRef = ref()
const routeTypeOptions = ROUTE_TYPE_OPTIONS

const isEdit = computed(() => !!props.headerId)

const formData = reactive({
  routeCode: '',
  routeName: '',
  routeType: 'STD' as RouteType,
  description: '',
  isDefault: false,
})

const rules = {
  routeCode: [{ required: true, message: '请输入路线编号', trigger: 'blur' }],
  routeName: [{ required: true, message: '请输入路线名称', trigger: 'blur' }],
  routeType: [{ required: true, message: '请选择路线类型', trigger: 'change' }],
}

watch(() => props.modelValue, (val) => {
  visible.value = val
  if (val) initForm()
})

watch(visible, (val) => {
  if (!val) {
    emit('closed')
  }
})

function initForm() {
  if (props.header) {
    formData.routeCode = props.header.routeCode
    formData.routeName = props.header.routeName
    formData.routeType = props.header.routeType
    formData.description = props.header.description || ''
    formData.isDefault = props.header.isDefault
  } else {
    formData.routeCode = ''
    formData.routeName = ''
    formData.routeType = 'STD' as RouteType
    formData.description = ''
    formData.isDefault = false
  }
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  saving.value = true
  try {
    if (isEdit.value && props.headerId) {
      await updateRouteHeader(props.headerId, {
        routeCode: formData.routeCode,
        routeName: formData.routeName,
        routeType: formData.routeType,
        description: formData.description || undefined,
        isDefault: formData.isDefault,
      })
      ElMessage.success('更新成功')
    } else {
      const result = await createRouteHeader({
        productId: props.productId,
        routeCode: formData.routeCode,
        routeName: formData.routeName,
        routeType: formData.routeType,
        description: formData.description || undefined,
        isDefault: formData.isDefault,
      })
      ElMessage.success('创建成功')
      emit('saved', result.id)
    }
    visible.value = false
  } catch { /* error handled by interceptor */ }
  finally {
    saving.value = false
  }
}
</script>

<style scoped>
.header-form {
  padding: 10px 20px 0;
}

.drawer-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}
</style>
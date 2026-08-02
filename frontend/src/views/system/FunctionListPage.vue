<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import RightPanel from '@/components/layout/RightPanel.vue'
import { List, Plus, Search, Refresh } from '@element-plus/icons-vue'
import { permissionApi } from '@/api/system'
import type { Permission as PermType, CreatePermission, UpdatePermission } from '@/types/system'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'FunctionListPage' })

const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(50)
const total = ref(0)
const items = ref<PermType[]>([])
const loading = ref(false)
const saving = ref(false)

const drawerVisible = ref(false)
const drawerTitle = ref('新建功能')
const isEditing = ref(false)
const editingId = ref<number | null>(null)

const form = reactive<CreatePermission>({
  name: '',
  code: '',
  module: '',
})

async function loadData() {
  loading.value = true
  try {
    const params: PagedRequest = {
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
    }
    const res = await permissionApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch {
    ElMessage.error('加载功能列表失败')
  } finally {
    loading.value = false
  }
}

function openCreate() {
  isEditing.value = false
  editingId.value = null
  drawerTitle.value = '新建功能'
  form.name = ''
  form.code = ''
  form.module = ''
  drawerVisible.value = true
}

async function openEdit(id: number) {
  isEditing.value = true
  editingId.value = id
  drawerTitle.value = '编辑功能'
  try {
    const perm = await permissionApi.get(id)
    form.name = perm.name
    form.code = perm.code
    form.module = perm.module || ''
  } catch {
    ElMessage.error('加载功能失败')
    return
  }
  drawerVisible.value = true
}

async function handleSave() {
  if (!form.name || !form.code) {
    ElMessage.warning('请填写功能名称和编码')
    return
  }
  saving.value = true
  try {
    if (isEditing.value && editingId.value) {
      await permissionApi.update(editingId.value, form as UpdatePermission)
      ElMessage.success('已更新')
    } else {
      await permissionApi.create(form)
      ElMessage.success('已创建')
    }
    drawerVisible.value = false
    await loadData()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function handleDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该功能？', '提示', { type: 'warning' })
    await permissionApi.delete(id)
    ElMessage.success('已删除')
    await loadData()
  } catch { /* cancelled */ }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="page-container">
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><List /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">功能列表</h2>
          <span class="page-header-banner-subtitle">定义系统功能点与权限编码</span>
        </div>
      </div>
    </div>

    <div class="ipqc-content">
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            功能列表
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索功能名称/编码"
              clearable
              size="small"
              :prefix-icon="Search"
              style="width: 200px"
              @keyup.enter="loadData"
            />
            <el-button size="small" @click="loadData">
              <el-icon><Refresh /></el-icon>刷新
            </el-button>
            <el-button type="primary" size="small" @click="openCreate">
              <el-icon><Plus /></el-icon>新建功能
            </el-button>
          </div>
        </div>

        <el-table
          :data="items"
          border
          stripe
          v-loading="loading"
          style="width: 100%"
          size="small"
        >
          <el-table-column type="index" label="序号" width="55" fixed />
          <el-table-column prop="name" label="功能名称" min-width="150" show-overflow-tooltip />
          <el-table-column prop="code" label="功能编码" min-width="200" show-overflow-tooltip />
          <el-table-column prop="module" label="所属模块" width="150" align="center" />
          <el-table-column label="操作" width="160" align="center" fixed="right">
            <template #default="{ row }">
              <el-button size="small" type="primary" link @click.stop="openEdit(row.id)">编辑</el-button>
              <el-button size="small" type="danger" link @click.stop="handleDelete(row.id)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>

        <div class="data-card__pagination">
          <el-pagination
            v-model:current-page="page"
            v-model:page-size="pageSize"
            :total="total"
            :page-sizes="[20, 50, 100, 200]"
            layout="total, sizes, prev, pager, next, jumper"
            @size-change="loadData"
            @current-change="loadData"
          />
        </div>
      </div>
    </div>

    <!-- Panel -->
    <RightPanel
      v-model:visible="drawerVisible"
      :title="drawerTitle"
      :width="520"
      :show-close="true"
    >
      <template #body>
        <div class="dialog-section">
          <div class="dialog-section-header">
            <el-icon class="dialog-section-icon"><List /></el-icon>
            <span class="dialog-section-title">功能信息</span>
          </div>
          <el-form :model="form" label-width="80px">
            <el-form-item label="功能名称" required>
              <el-input v-model="form.name" placeholder="请输入功能名称" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item label="功能编码" required>
              <el-input v-model="form.code" placeholder="如: ipqc:plan:create" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item label="所属模块">
              <el-input v-model="form.module" placeholder="如: IPQC" clearable style="width: 100%" />
            </el-form-item>
          </el-form>
        </div>
      </template>

      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="drawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="handleSave" :loading="saving">保存</el-button>
        </div>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.page-container { height: 100%; display: flex; flex-direction: column; overflow: hidden; }
.ipqc-content { flex: 1; display: flex; flex-direction: column; gap: 12px; padding: 12px; overflow-y: auto; }
.data-card { background: #fff; border-radius: 8px; box-shadow: 0 1px 4px rgba(0,0,0,0.04); overflow: hidden; flex: 1; display: flex; flex-direction: column; min-height: 0; }
.data-card__header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; border-bottom: 1px solid var(--el-border-color-lighter); background: var(--el-fill-color-blank); flex-shrink: 0; }
.data-card__title { display: flex; align-items: center; gap: 6px; font-size: 15px; font-weight: 600; color: var(--el-text-color-primary); }
.data-card__actions { display: flex; align-items: center; gap: 8px; flex-wrap: wrap; }
.data-card__pagination { display: flex; justify-content: flex-end; padding: 12px 16px; border-top: 1px solid var(--el-border-color-lighter); flex-shrink: 0; }
.dialog-section { margin-bottom: 16px; }
.dialog-section:last-of-type { margin-bottom: 0; }
.dialog-section-header { display: flex; align-items: center; gap: 6px; padding: 8px 12px; background: var(--el-fill-color-light); border-radius: 6px; margin-bottom: 12px; }
.dialog-section-icon { font-size: 15px; color: var(--el-color-primary); }
.dialog-section-title { font-size: 13px; font-weight: 600; color: var(--el-text-color-regular); }
</style>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import RightPanel from '@/components/layout/RightPanel.vue'
import { Setting, Plus, Search, Refresh } from '@element-plus/icons-vue'
import { roleApi } from '@/api/system'
import type { Role as RoleType, CreateRole, UpdateRole } from '@/types/system'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'RoleManagementPage' })

const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const items = ref<RoleType[]>([])
const loading = ref(false)
const saving = ref(false)

const drawerVisible = ref(false)
const drawerTitle = ref('新建角色')
const isEditing = ref(false)
const editingId = ref<number | null>(null)

const form = reactive<CreateRole>({
  name: '',
  description: '',
})

async function loadData() {
  loading.value = true
  try {
    const params: PagedRequest = {
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
    }
    const res = await roleApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch {
    ElMessage.error('加载角色列表失败')
  } finally {
    loading.value = false
  }
}

function openCreate() {
  isEditing.value = false
  editingId.value = null
  drawerTitle.value = '新建角色'
  form.name = ''
  form.description = ''
  drawerVisible.value = true
}

async function openEdit(id: number) {
  isEditing.value = true
  editingId.value = id
  drawerTitle.value = '编辑角色'
  try {
    const role = await roleApi.get(id)
    form.name = role.name
    form.description = role.description || ''
  } catch {
    ElMessage.error('加载角色失败')
    return
  }
  drawerVisible.value = true
}

async function handleSave() {
  if (!form.name) {
    ElMessage.warning('请填写角色名称')
    return
  }
  saving.value = true
  try {
    if (isEditing.value && editingId.value) {
      await roleApi.update(editingId.value, form as UpdateRole)
      ElMessage.success('已更新')
    } else {
      await roleApi.create(form)
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
    await ElMessageBox.confirm('确认删除该角色？', '提示', { type: 'warning' })
    await roleApi.delete(id)
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
          <el-icon :size="28"><Setting /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">角色管理</h2>
          <span class="page-header-banner-subtitle">定义系统角色与权限组</span>
        </div>
      </div>
    </div>

    <div class="ipqc-content">
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            角色列表
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索角色名称"
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
              <el-icon><Plus /></el-icon>新建角色
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
          <el-table-column prop="name" label="角色名称" min-width="150" show-overflow-tooltip />
          <el-table-column prop="description" label="描述" min-width="200" show-overflow-tooltip />
          <el-table-column prop="userCount" label="用户数" width="80" align="center" />
          <el-table-column prop="createdAt" label="创建时间" width="170" />
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
            :page-sizes="[10, 20, 50, 100]"
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
            <el-icon class="dialog-section-icon"><Setting /></el-icon>
            <span class="dialog-section-title">角色信息</span>
          </div>
          <el-form :model="form" label-width="80px">
            <el-form-item label="角色名称" required>
              <el-input v-model="form.name" placeholder="请输入角色名称" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item label="描述">
              <el-input v-model="form.description" type="textarea" :rows="3" placeholder="请输入角色描述" style="width: 100%" />
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
.page-container {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.ipqc-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 12px;
  overflow-y: auto;
}
.data-card {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}
.data-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
  flex-shrink: 0;
}
.data-card__title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}
.data-card__actions {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.data-card__pagination {
  display: flex;
  justify-content: flex-end;
  padding: 12px 16px;
  border-top: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
}
.dialog-section {
  margin-bottom: 16px;
}
.dialog-section:last-of-type { margin-bottom: 0; }
.dialog-section-header {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  margin-bottom: 12px;
}
.dialog-section-icon { font-size: 15px; color: var(--el-color-primary); }
.dialog-section-title { font-size: 13px; font-weight: 600; color: var(--el-text-color-regular); }
</style>

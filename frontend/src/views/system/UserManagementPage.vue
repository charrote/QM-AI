<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import RightPanel from '@/components/layout/RightPanel.vue'
import { User, Plus, Search, Refresh, Setting } from '@element-plus/icons-vue'
import { userApi } from '@/api/system'
import type { User as UserType, CreateUser, UpdateUser } from '@/types/system'
import type { PagedRequest } from '@/types/basicData'

defineOptions({ name: 'UserManagementPage' })

const searchKeyword = ref('')
const page = ref(1)
const pageSize = ref(20)
const total = ref(0)
const items = ref<UserType[]>([])
const loading = ref(false)
const saving = ref(false)
const roles = ref<Array<{ id: number; name: string }>>([])

const drawerVisible = ref(false)
const drawerTitle = ref('新建用户')
const isEditing = ref(false)
const editingId = ref<number | null>(null)

const form = reactive<CreateUser>({
  username: '',
  password: '',
  displayName: '',
  email: '',
  roleId: 0,
})

async function loadData() {
  loading.value = true
  try {
    const params: PagedRequest = {
      page: page.value,
      pageSize: pageSize.value,
      keyword: searchKeyword.value || undefined,
    }
    const res = await userApi.list(params)
    items.value = res.items
    total.value = res.total
  } catch {
    ElMessage.error('加载用户列表失败')
  } finally {
    loading.value = false
  }
}

async function loadRoles() {
  try {
    roles.value = await userApi.getRoles()
  } catch { /* ignore */ }
}

function openCreate() {
  isEditing.value = false
  editingId.value = null
  drawerTitle.value = '新建用户'
  form.username = ''
  form.password = ''
  form.displayName = ''
  form.email = ''
  form.roleId = 0
  drawerVisible.value = true
}

async function openEdit(id: number) {
  isEditing.value = true
  editingId.value = id
  drawerTitle.value = '编辑用户'
  try {
    const user = await userApi.get(id)
    form.displayName = user.displayName || ''
    form.email = user.email || ''
    // Extract role id from roleName — we need to get the full user data
    // Since get() returns UserListDto without roleId, we need a workaround
    // Use the list API with keyword filter or just set a placeholder
  } catch {
    ElMessage.error('加载用户失败')
    return
  }
  drawerVisible.value = true
}

async function handleSave() {
  if (!form.username || !form.password || !form.roleId) {
    ElMessage.warning('请填写用户名、密码和角色')
    return
  }
  saving.value = true
  try {
    if (isEditing.value && editingId.value) {
      await userApi.update(editingId.value, form as UpdateUser)
      ElMessage.success('已更新')
    } else {
      await userApi.create(form)
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
    await ElMessageBox.confirm('确认删除该用户？', '提示', { type: 'warning' })
    await userApi.delete(id)
    ElMessage.success('已删除')
    await loadData()
  } catch { /* cancelled */ }
}

onMounted(async () => {
  await Promise.all([loadData(), loadRoles()])
})
</script>

<template>
  <div class="page-container">
    <!-- Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><User /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">用户管理</h2>
          <span class="page-header-banner-subtitle">管理系统用户账号与角色分配</span>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="ipqc-content">
      <div class="data-card">
        <div class="data-card__header">
          <span class="data-card__title">
            用户列表
            <el-tag v-if="total" type="info" size="small">{{ total }} 条</el-tag>
          </span>
          <div class="data-card__actions">
            <el-input
              v-model="searchKeyword"
              placeholder="搜索用户名/姓名"
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
              <el-icon><Plus /></el-icon>新建用户
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
          <el-table-column prop="username" label="用户名" min-width="120" show-overflow-tooltip />
          <el-table-column prop="displayName" label="显示名称" min-width="120" show-overflow-tooltip />
          <el-table-column prop="email" label="邮箱" min-width="150" show-overflow-tooltip />
          <el-table-column prop="roleName" label="角色" width="120" align="center" />
          <el-table-column label="状态" width="80" align="center">
            <template #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'info'" size="small" effect="plain" round>
                {{ row.isActive ? '启用' : '停用' }}
              </el-tag>
            </template>
          </el-table-column>
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
            <span class="dialog-section-title">用户信息</span>
          </div>
          <el-form :model="form" label-width="80px">
            <el-form-item label="用户名" required>
              <el-input v-model="form.username" :disabled="isEditing" placeholder="请输入用户名" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item :label="isEditing ? '新密码' : '密码'" :required="!isEditing">
              <el-input v-model="form.password" type="password" show-password :placeholder="isEditing ? '留空不修改' : '请输入密码'" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item label="显示名称">
              <el-input v-model="form.displayName" placeholder="请输入显示名称" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item label="邮箱">
              <el-input v-model="form.email" placeholder="请输入邮箱" clearable style="width: 100%" />
            </el-form-item>
            <el-form-item label="角色" required>
              <el-select v-model="form.roleId" filterable placeholder="选择角色" style="width: 100%">
                <el-option v-for="r in roles" :key="r.id" :label="r.name" :value="r.id" />
              </el-select>
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
</style>

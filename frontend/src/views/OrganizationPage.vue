<script setup lang="ts">
defineOptions({ name: 'OrganizationPage' })

import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  Folder, FolderAdd, Search, SetUp, Edit, Delete, Plus, Connection,
} from '@element-plus/icons-vue'
import { organizationApi } from '@/api/organization'
import { useOrgStore } from '@/stores/orgStore'
import type { OrganizationTreeNode, OrganizationDetail, CreateOrganization } from '@/types/organization'
import { LEVEL_CONFIG, NEXT_LEVEL } from '@/types/organization'

const orgStore = useOrgStore()

// ─── 状态 ────────────────────────────────────────────────
const treeData = ref<OrganizationTreeNode[]>([])
const loading = ref(false)
const selectedNode = ref<OrganizationDetail | null>(null)
const selectedNodeId = ref<number | null>(null)
const searchKeyword = ref('')

// Dialog
const dialogVisible = ref(false)
const dialogTitle = ref('')
const isEdit = ref(false)
const editingId = ref<number | null>(null)
const parentNode = ref<OrganizationTreeNode | null>(null)
const formRef = ref()
const formData = reactive({
  code: '',
  name: '',
  level: '',
  parentId: null as number | null,
  sortOrder: 0,
  location: '',
  description: '',
})

// ─── 计算属性 ────────────────────────────────────────────
const levelLabel = computed(() => {
  if (!selectedNode.value) return ''
  const cfg = LEVEL_CONFIG[selectedNode.value.level]
  return cfg ? cfg.label : selectedNode.value.level
})

const levelColor = computed(() => {
  if (!selectedNode.value) return ''
  const cfg = LEVEL_CONFIG[selectedNode.value.level]
  return cfg ? cfg.color : '#909399'
})

/** 搜索过滤后的扁平节点列表 */
const filteredNodes = computed(() => {
  if (!searchKeyword.value) return []
  const result: { id: number; name: string; level: string; path: string }[] = []
  function walk(nodes: OrganizationTreeNode[], parentPath: string) {
    for (const n of nodes) {
      const path = parentPath ? `${parentPath} / ${n.name}` : n.name
      if (n.name.includes(searchKeyword.value) || n.code.includes(searchKeyword.value)) {
        result.push({ id: n.id, name: n.name, level: n.level, path })
      }
      if (n.children?.length) {
        walk(n.children, path)
      }
    }
  }
  walk(treeData.value, '')
  return result
})

/** 根节点是否存在 */
const hasRoot = computed(() => treeData.value.length > 0)

// ─── 数据加载 ────────────────────────────────────────────
async function loadTree() {
  loading.value = true
  try {
    treeData.value = await organizationApi.tree()
    // orgStore.loadOrgTree() is called separately if needed,
    // as it re-fetches the same API and may not be necessary here
  } finally {
    loading.value = false
  }
}

async function loadNodeDetail(id: number) {
  try {
    selectedNode.value = await organizationApi.get(id)
    selectedNodeId.value = id
  } catch {
    selectedNode.value = null
    selectedNodeId.value = null
  }
}

// ─── 节点选择 ────────────────────────────────────────────
function handleNodeClick(node: OrganizationTreeNode) {
  selectedNodeId.value = node.id
  loadNodeDetail(node.id)
}

function handleSearchSelect(id: number) {
  loadNodeDetail(id)
  searchKeyword.value = ''
}

// ─── 新增/编辑 ───────────────────────────────────────────
function openCreateRoot() {
  isEdit.value = false
  editingId.value = null
  parentNode.value = null
  dialogTitle.value = '新增集团'
  Object.assign(formData, {
    code: '', name: '', level: 'group', parentId: null,
    sortOrder: 0, location: '', description: '',
  })
  dialogVisible.value = true
}

function openCreateChild(parent: OrganizationTreeNode) {
  isEdit.value = false
  editingId.value = null
  parentNode.value = parent
  const nextLevel = NEXT_LEVEL[parent.level]
  if (!nextLevel) {
    ElMessage.warning('该层级下无法再添加子节点')
    return
  }
  dialogTitle.value = `新增子节点 - ${parent.name}`
  Object.assign(formData, {
    code: '', name: '', level: nextLevel, parentId: parent.id,
    sortOrder: 0, location: '', description: '',
  })
  dialogVisible.value = true
}

async function openEdit() {
  if (!selectedNode.value) return
  isEdit.value = true
  editingId.value = selectedNode.value.id
  dialogTitle.value = `编辑 - ${selectedNode.value.name}`
  Object.assign(formData, {
    code: selectedNode.value.code,
    name: selectedNode.value.name,
    level: selectedNode.value.level,
    parentId: selectedNode.value.parentId,
    sortOrder: selectedNode.value.sortOrder,
    location: selectedNode.value.location || '',
    description: selectedNode.value.description || '',
  })
  dialogVisible.value = true
}

async function handleSave() {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  try {
    if (isEdit.value && editingId.value) {
      await organizationApi.update(editingId.value, {
        code: formData.code,
        name: formData.name,
        parentId: formData.parentId,
        sortOrder: formData.sortOrder,
        location: formData.location || undefined,
        description: formData.description || undefined,
      })
      ElMessage.success('更新成功')
    } else {
      await organizationApi.create({
        code: formData.code,
        name: formData.name,
        level: formData.level,
        parentId: formData.parentId,
        sortOrder: formData.sortOrder,
        location: formData.location || undefined,
        description: formData.description || undefined,
      })
      ElMessage.success('创建成功')
    }
    dialogVisible.value = false
    await loadTree()
    if (selectedNodeId.value) {
      await loadNodeDetail(selectedNodeId.value)
    }
  } catch { /* error handled by interceptor */ }
}

// ─── 删除 ────────────────────────────────────────────────
async function handleDelete(node?: OrganizationTreeNode) {
  const target = node || selectedNode.value
  if (!target) return

  try {
    await ElMessageBox.confirm(
      `确认删除 "${target.name}"？\n此操作不可恢复。`,
      '确认删除',
      { type: 'warning', confirmButtonText: '删除', cancelButtonText: '取消' },
    )
    await organizationApi.delete(target.id)
    ElMessage.success('删除成功')
    selectedNode.value = null
    selectedNodeId.value = null
    await loadTree()
  } catch { /* cancelled or error */ }
}

// ─── 树节点渲染辅助 ────────────────────────────────────
function getLevelTag(level: string) {
  const cfg = LEVEL_CONFIG[level]
  return cfg ? cfg.label : level
}

function getLevelColor(level: string) {
  const cfg = LEVEL_CONFIG[level]
  return cfg ? cfg.color : '#909399'
}

// ─── 初始化 ──────────────────────────────────────────────
onMounted(() => {
  loadTree()
})
</script>

<template>
  <div class="org-page">
    <!-- 页面标题 -->
    <div class="page-header">
      <div class="page-header-main">
        <h2>企业组织管理</h2>
        <p>管理集团、工厂、车间、产线的层级结构</p>
      </div>
    </div>

    <!-- 主体：左侧树 + 右侧详情 -->
    <div class="org-body">
      <!-- 左侧：组织树 -->
      <div class="org-tree-panel">
        <div class="panel-header">
          <div class="panel-title">
            <el-icon><Folder /></el-icon>
            <span>组织架构</span>
          </div>
          <el-button
            v-if="!hasRoot"
            type="primary"
            size="small"
            @click="openCreateRoot"
          >
            <el-icon><Plus /></el-icon>
            创建集团
          </el-button>
        </div>

        <!-- 搜索 -->
        <div class="search-wrapper">
          <el-input
            v-model="searchKeyword"
            placeholder="搜索组织名称或编码"
            clearable
            size="default"
          >
            <template #prefix>
              <el-icon><Search /></el-icon>
            </template>
          </el-input>
        </div>

        <!-- 搜索结果 -->
        <div v-if="searchKeyword" class="search-results">
          <div class="search-results-header">
            <span>搜索结果</span>
            <el-tag size="small" type="info">{{ filteredNodes.length }}</el-tag>
          </div>
          <div
            v-for="item in filteredNodes"
            :key="item.id"
            class="search-item"
            @click="handleSearchSelect(item.id)"
          >
            <el-tag :color="getLevelColor(item.level)" size="small" effect="dark" class="level-tag">
              {{ getLevelTag(item.level) }}
            </el-tag>
            <span class="search-path">{{ item.path }}</span>
          </div>
          <div v-if="filteredNodes.length === 0" class="search-empty">
            <el-icon :size="32" color="#c0c4cc"><Search /></el-icon>
            <p>未找到匹配的组织</p>
          </div>
        </div>

        <!-- 树视图 -->
        <div v-else class="tree-wrapper" v-loading="loading">
          <template v-if="treeData.length === 0">
            <div class="tree-empty">
              <div class="tree-empty-icon">
                <el-icon :size="64" color="#c0c4cc"><FolderAdd /></el-icon>
              </div>
              <p class="tree-empty-title">暂无组织数据</p>
              <p class="tree-empty-hint">点击上方按钮创建第一个集团节点</p>
            </div>
          </template>
          <el-tree
            v-else
            :data="treeData"
            :props="{ label: 'name', children: 'children' }"
            node-key="id"
            default-expand-all
            highlight-current
            :current-node-key="selectedNodeId"
            @node-click="handleNodeClick"
            class="org-tree"
          >
            <template #default="{ node, data }">
              <div class="custom-tree-node" :class="'level-' + data.level">
                <span class="level-indicator" :style="{ borderColor: getLevelColor(data.level) }"></span>
                <el-tag :color="getLevelColor(data.level)" size="small" effect="dark" class="level-tag">
                  {{ getLevelTag(data.level) }}
                </el-tag>
                <span class="node-name">{{ data.name }}</span>
                <span class="node-code">{{ data.code }}</span>
                <span v-if="data.childCount > 0" class="child-count" :style="{ background: getLevelColor(data.level) + '18', color: getLevelColor(data.level) }">
                  <el-icon :size="12"><Connection /></el-icon>
                  {{ data.childCount }}
                </span>
              </div>
            </template>
          </el-tree>
        </div>
      </div>

      <!-- 右侧：详情 -->
      <div class="org-detail-panel">
        <template v-if="selectedNode">
          <div class="detail-header">
            <div class="detail-title">
              <el-tag :color="levelColor" effect="dark" size="small" class="level-badge">{{ levelLabel }}</el-tag>
              <h3>{{ selectedNode.name }}</h3>
            </div>
            <div class="detail-actions">
              <el-button size="small" type="primary" @click="openEdit">
                <el-icon><Edit /></el-icon> 编辑
              </el-button>
              <el-button
                v-if="NEXT_LEVEL[selectedNode.level]"
                size="small"
                type="success"
                @click="openCreateChild({ ...selectedNode, children: [], childCount: 0 } as OrganizationTreeNode)"
              >
                <el-icon><Plus /></el-icon> 添加子节点
              </el-button>
              <el-button size="small" type="danger" @click="handleDelete()">
                <el-icon><Delete /></el-icon> 删除
              </el-button>
            </div>
          </div>

          <div class="detail-card">
            <el-descriptions :column="2" border class="detail-descriptions">
              <el-descriptions-item label="编码" :span="1">
                <span class="detail-value code">{{ selectedNode.code }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="层级" :span="1">
                <el-tag :color="levelColor" effect="dark" size="small">{{ levelLabel }}</el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="父级组织" :span="1">
                <span class="detail-value">{{ selectedNode.parentName || '-' }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="排序号" :span="1">
                <span class="detail-value">{{ selectedNode.sortOrder }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="状态" :span="1">
                <el-tag :type="selectedNode.isActive ? 'success' : 'danger'" size="small">
                  {{ selectedNode.isActive ? '启用' : '停用' }}
                </el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="位置" :span="1">
                <span class="detail-value">{{ selectedNode.location || '-' }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="描述" :span="2">
                <span class="detail-value">{{ selectedNode.description || '-' }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="创建时间" :span="1">
                <span class="detail-value time">{{ selectedNode.createdAt }}</span>
              </el-descriptions-item>
              <el-descriptions-item label="更新时间" :span="1">
                <span class="detail-value time">{{ selectedNode.updatedAt }}</span>
              </el-descriptions-item>
            </el-descriptions>
          </div>
        </template>

        <template v-else>
          <div class="detail-empty">
            <div class="detail-empty-icon">
              <el-icon :size="80" color="#dcdfe6"><SetUp /></el-icon>
            </div>
            <p class="detail-empty-title">选择一个组织节点</p>
            <p class="detail-empty-desc">从左侧树结构中点击任意节点查看详情</p>
          </div>
        </template>
      </div>
    </div>

    <!-- 新增/编辑 Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="560px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form
        ref="formRef"
        :model="formData"
        label-width="100px"
        size="default"
      >
        <el-divider content-position="left">基本信息</el-divider>
        <el-form-item label="层级" prop="level">
          <el-tag :color="getLevelColor(formData.level)" effect="dark">
            {{ getLevelTag(formData.level) }}
          </el-tag>
          <span class="form-hint" v-if="parentNode">
            父级: {{ parentNode.name }} ({{ getLevelTag(parentNode.level) }})
          </span>
        </el-form-item>
        <el-form-item
          label="组织编码"
          prop="code"
          :rules="[{ required: true, message: '请输入组织编码' }]"
        >
          <el-input v-model="formData.code" placeholder="唯一编码，如 HQ, FACTORY_1" />
        </el-form-item>
        <el-form-item
          label="组织名称"
          prop="name"
          :rules="[{ required: true, message: '请输入组织名称' }]"
        >
          <el-input v-model="formData.name" placeholder="如 集团总部, 第一工厂" />
        </el-form-item>

        <el-divider content-position="left">其他信息</el-divider>
        <el-form-item label="排序号" prop="sortOrder">
          <el-input-number v-model="formData.sortOrder" :min="0" style="width: 100%" />
        </el-form-item>
        <el-form-item label="位置" prop="location">
          <el-input v-model="formData.location" placeholder="可选" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input v-model="formData.description" type="textarea" :rows="2" placeholder="可选" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<style scoped>
.org-page {
  display: flex;
  flex-direction: column;
  gap: 16px;
  height: 100%;
  min-height: calc(100vh - 120px);
  padding: 4px 0;
}

/* ── 页面标题 ── */
.page-header {
  flex-shrink: 0;
  padding: 0 4px;
}
.page-header-main h2 {
  margin: 0;
  font-size: 22px;
  font-weight: 600;
  color: #303133;
  letter-spacing: -0.02em;
}
.page-header-main p {
  margin: 6px 0 0;
  font-size: 14px;
  color: #909399;
}

/* ── 主体区域 ── */
.org-body {
  display: flex;
  gap: 16px;
  flex: 1;
  min-height: 0;
}

/* ── 左侧树面板 ── */
.org-tree-panel {
  width: 380px;
  flex-shrink: 0;
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.panel-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: 600;
  color: #303133;
}
.panel-title .el-icon {
  color: #409eff;
  font-size: 18px;
}

.search-wrapper {
  margin-bottom: 12px;
}
.search-wrapper :deep(.el-input__wrapper) {
  border-radius: 8px;
}

.search-results {
  flex: 1;
  overflow-y: auto;
  border: 1px solid #ebeef5;
  border-radius: 8px;
  padding: 0;
  background: #fafbfc;
}

.search-results-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 14px;
  font-size: 13px;
  color: #909399;
  border-bottom: 1px solid #ebeef5;
  background: #fff;
  position: sticky;
  top: 0;
}

.search-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px;
  cursor: pointer;
  border-bottom: 1px solid #f2f3f5;
  transition: background 0.15s;
}
.search-item:last-child {
  border-bottom: none;
}
.search-item:hover {
  background: #ecf5ff;
}

.search-path {
  font-size: 13px;
  color: #303133;
  line-height: 1.4;
}

.search-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 32px 20px;
  gap: 8px;
}
.search-empty p {
  margin: 0;
  font-size: 13px;
  color: #909399;
}

.tree-wrapper {
  flex: 1;
  overflow-y: auto;
}

.tree-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  gap: 4px;
}
.tree-empty-icon {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: #f5f7fa;
  display: flex;
  align-items: center;
  justify-content: center;
}
.tree-empty-title {
  margin: 12px 0 0;
  font-size: 15px;
  font-weight: 500;
  color: #606266;
}
.tree-empty-hint {
  font-size: 13px !important;
  color: #c0c4cc !important;
  margin: 4px 0 0 !important;
}

/* 树节点 */
.custom-tree-node {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  padding: 6px 8px;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.15s;
  border-left: 3px solid transparent;
  width: 100%;
}
.custom-tree-node:hover {
  background: #f5f7fa;
}
.custom-tree-node.level-group { border-left-color: #722ed1; }
.custom-tree-node.level-company { border-left-color: #2f54eb; }
.custom-tree-node.level-workshop { border-left-color: #f5a623; }
.custom-tree-node.level-line { border-left-color: #2f9e6b; }

.level-indicator {
  width: 4px;
  height: 4px;
  border-radius: 50%;
  border: 1.5px solid #dcdfe6;
  flex-shrink: 0;
}

.level-tag {
  flex-shrink: 0;
}

.node-name {
  font-weight: 500;
  color: #303133;
}

.node-code {
  font-size: 11px;
  color: #909399;
}

.child-count {
  display: flex;
  align-items: center;
  gap: 2px;
  font-size: 11px;
  padding: 0 8px;
  border-radius: 8px;
  line-height: 20px;
  margin-left: auto;
}

/* ── 右侧详情面板 ── */
.org-detail-panel {
  flex: 1;
  background: #fff;
  border-radius: 12px;
  padding: 24px;
  overflow-y: auto;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.detail-title {
  display: flex;
  align-items: center;
  gap: 10px;
}
.level-badge {
  font-weight: 600;
}
.detail-title h3 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
  color: #303133;
}

.detail-actions {
  display: flex;
  gap: 8px;
}

.detail-card {
  background: #fafbfc;
  border-radius: 10px;
  padding: 4px;
}

.detail-descriptions :deep(.el-descriptions__label) {
  font-weight: 500;
  width: 100px;
}

.detail-value {
  color: #303133;
}
.detail-value.code {
  font-family: 'SF Mono', 'Fira Code', monospace;
  background: #f0f2f5;
  padding: 1px 8px;
  border-radius: 4px;
  font-size: 13px;
}
.detail-value.time {
  font-size: 13px;
  color: #606266;
}

.detail-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  gap: 4px;
}
.detail-empty-icon {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e7ed 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}
.detail-empty-title {
  margin: 0;
  font-size: 16px;
  font-weight: 500;
  color: #606266;
}
.detail-empty-desc {
  margin: 6px 0 0;
  font-size: 13px;
  color: #c0c4cc;
}

/* ── Dialog 表单 ── */
.form-hint {
  margin-left: 12px;
  font-size: 12px;
  color: #909399;
}

:deep(.el-dialog) {
  border-radius: 12px;
}
:deep(.el-dialog__header) {
  margin-right: 0;
  padding: 20px 24px 16px;
}
:deep(.el-dialog__body) {
  padding: 16px 24px 24px;
}
:deep(.el-divider__text) {
  font-weight: 600;
  color: #303133;
  font-size: 14px;
}
</style>

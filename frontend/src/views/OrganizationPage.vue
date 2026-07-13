<script setup lang="ts">
defineOptions({ name: 'OrganizationPage' })

import { ref, reactive, computed, onMounted, nextTick } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
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
    await orgStore.loadOrgTree()
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
    <!-- 左侧：组织树 -->
    <div class="org-tree-panel">
      <div class="panel-header">
        <h3>组织架构</h3>
        <el-button
          v-if="!hasRoot"
          type="primary"
          size="small"
          @click="openCreateRoot"
        >
          创建集团
        </el-button>
      </div>

      <!-- 搜索 -->
      <el-input
        v-model="searchKeyword"
        placeholder="搜索组织名称/编码..."
        clearable
        size="small"
        class="search-input"
      />

      <!-- 搜索结果 -->
      <div v-if="searchKeyword" class="search-results">
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
        <div v-if="filteredNodes.length === 0" class="search-empty">未找到匹配的组织</div>
      </div>

      <!-- 树视图 -->
      <div v-else class="tree-wrapper" v-loading="loading">
        <template v-if="treeData.length === 0">
          <div class="tree-empty">
            <el-icon :size="48" color="#c0c4cc"><Folder /></el-icon>
            <p>暂无组织数据</p>
            <p class="tree-empty-hint">请先创建集团节点</p>
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
        >
          <template #default="{ node, data }">
            <div class="custom-tree-node">
              <el-tag
                :color="getLevelColor(data.level)"
                size="small"
                effect="dark"
                class="level-tag"
              >
                {{ getLevelTag(data.level) }}
              </el-tag>
              <span class="node-name">{{ data.name }}</span>
              <span class="node-code">{{ data.code }}</span>
              <span v-if="data.childCount > 0" class="child-count">{{ data.childCount }}</span>
            </div>
          </template>
        </el-tree>
      </div>
    </div>

    <!-- 右侧：详情 -->
    <div class="org-detail-panel">
      <template v-if="selectedNode">
        <div class="detail-header">
          <h3>{{ selectedNode.name }}</h3>
          <div class="detail-actions">
            <el-button size="small" type="primary" @click="openEdit">编辑</el-button>
            <el-button
              v-if="NEXT_LEVEL[selectedNode.level]"
              size="small"
              type="success"
              @click="openCreateChild({ ...selectedNode, children: [], childCount: 0 } as OrganizationTreeNode)"
            >
              添加子节点
            </el-button>
            <el-button size="small" type="danger" @click="handleDelete()">删除</el-button>
          </div>
        </div>

        <el-descriptions :column="2" border class="detail-descriptions">
          <el-descriptions-item label="编码" :span="1">{{ selectedNode.code }}</el-descriptions-item>
          <el-descriptions-item label="层级" :span="1">
            <el-tag :color="levelColor" effect="dark" size="small">{{ levelLabel }}</el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="父级组织" :span="1">
            {{ selectedNode.parentName || '-' }}
          </el-descriptions-item>
          <el-descriptions-item label="排序号" :span="1">{{ selectedNode.sortOrder }}</el-descriptions-item>
          <el-descriptions-item label="状态" :span="1">
            <el-tag :type="selectedNode.isActive ? 'success' : 'danger'" size="small">
              {{ selectedNode.isActive ? '启用' : '停用' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="位置" :span="1">{{ selectedNode.location || '-' }}</el-descriptions-item>
          <el-descriptions-item label="描述" :span="2">{{ selectedNode.description || '-' }}</el-descriptions-item>
          <el-descriptions-item label="创建时间" :span="1">{{ selectedNode.createdAt }}</el-descriptions-item>
          <el-descriptions-item label="更新时间" :span="1">{{ selectedNode.updatedAt }}</el-descriptions-item>
        </el-descriptions>
      </template>

      <template v-else>
        <div class="detail-empty">
          <el-icon :size="48" color="#c0c4cc"><SetUp /></el-icon>
          <p>请从左侧选择一个组织节点</p>
        </div>
      </template>
    </div>

    <!-- 新增/编辑 Dialog -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="540px"
      :close-on-click-modal="false"
      destroy-on-close
    >
      <el-form
        ref="formRef"
        :model="formData"
        label-width="100px"
        size="small"
      >
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
  gap: 16px;
  height: 100%;
  min-height: calc(100vh - 120px);
}

/* ── 左侧树面板 ── */
.org-tree-panel {
  width: 360px;
  flex-shrink: 0;
  background: #fff;
  border-radius: 8px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.panel-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 600;
}

.search-input {
  margin-bottom: 8px;
}

.search-results {
  flex: 1;
  overflow-y: auto;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
  padding: 8px;
}

.search-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 8px;
  cursor: pointer;
  border-radius: 4px;
  transition: background 0.2s;
}

.search-item:hover {
  background: #ecf5ff;
}

.search-path {
  font-size: 13px;
  color: #606266;
}

.search-empty {
  text-align: center;
  color: #909399;
  padding: 20px;
  font-size: 13px;
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
  color: #909399;
}

.tree-empty p {
  margin: 8px 0 0;
  font-size: 14px;
}

.tree-empty-hint {
  font-size: 12px !important;
  color: #c0c4cc !important;
}

.custom-tree-node {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  padding: 2px 0;
  width: 100%;
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
  margin-left: auto;
  background: #e6e8eb;
  color: #606266;
  font-size: 11px;
  padding: 0 6px;
  border-radius: 8px;
  line-height: 16px;
}

/* ── 右侧详情面板 ── */
.org-detail-panel {
  flex: 1;
  background: #fff;
  border-radius: 8px;
  padding: 24px;
  overflow-y: auto;
}

.detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;
}

.detail-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.detail-actions {
  display: flex;
  gap: 8px;
}

.detail-descriptions {
  margin-top: 8px;
}

.detail-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 100px 20px;
  color: #909399;
}

.detail-empty p {
  margin-top: 12px;
  font-size: 14px;
}

.form-hint {
  margin-left: 12px;
  font-size: 12px;
  color: #909399;
}
</style>

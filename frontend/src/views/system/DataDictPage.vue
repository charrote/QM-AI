<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import RightPanel from '@/components/layout/RightPanel.vue'
import { Collection, Plus, Search, Refresh, Edit, Delete, Close, ArrowDown, Check } from '@element-plus/icons-vue'
import { sysDictApi } from '@/api/sysDict'
import type { SysDictType, SysDictItem } from '@/types/sysDict'

defineOptions({ name: 'DataDictPage' })

// ─── 状态 ───────────────────────────────
const types = ref<SysDictType[]>([])
const selectedType = ref<string>('')
const items = ref<SysDictItem[]>([])
const loading = ref(false)
const saving = ref(false)
const searchKeyword = ref('')

// ─── 类型编辑抽屉 ─────────────────────
const typeDrawerVisible = ref(false)
const typeForm = reactive<{ id?: number; typeCode: string; typeName: string; remark: string }>({
  typeCode: '', typeName: '', remark: '',
})

// ─── 字典项编辑抽屉 ───────────────────
const itemDrawerVisible = ref(false)
const itemForm = reactive<{ id?: number; itemLabel: string; itemValue: string; sortOrder: number; color: string }>({
  itemLabel: '', itemValue: '', sortOrder: 0, color: '',
})

// ─── 预设颜色 (32 种) ─────────────────
const colorOptions = [
  '#1677ff', '#4096ff', '#69b1ff', '#90ccff', '#b3e0ff', '#cceaff', '#e6f4ff',
  '#ff7d00', '#ffa940', '#ffc069', '#ffd59b', '#ffe8c2', '#fff2e8',
  '#52c41a', '#73d13d', '#95de64', '#b7eb8f', '#d9f7be', '#f6ffed',
  '#faad14', '#ffc53d', '#ffd666', '#ffe599', '#fff1b8', '#fff7d6',
  '#f5222d', '#ff4d4f', '#ff7875', '#ffa39e', '#ffccc7', '#fff1f0',
  '#722ed1', '#9254de', '#b37feb', '#d3adf7', '#e9d5ff', '#f9f0ff',
  '#13c2c2', '#36cfc9', '#5cdbd3', '#87e8de', '#b5f5ec', '#e6fffb',
  '#2f54eb', '#597ef7', '#85a5ff', '#adc6ff', '#d6e4ff', '#e8f3ff',
  '#eb2f96', '#f759ab', '#ff87cb', '#ffc2e8', '#ffd6f3', '#fff0f6',
  '#8c8c8c', '#595959', '#434343', '#262626', '#141414', '#000000',
]

const filteredItems = computed(() => {
  if (!searchKeyword.value) return items.value
  const kw = searchKeyword.value.toLowerCase()
  return items.value.filter(i => i.itemLabel.toLowerCase().includes(kw) || i.itemValue.toLowerCase().includes(kw))
})

// ─── 加载数据 ─────────────────────────
async function loadTypes() {
  try {
    types.value = await sysDictApi.getTypes()
  } catch { /* ignore */ }
}

async function loadItems(typeCode: string) {
  selectedType.value = typeCode
  loading.value = true
  try {
    items.value = await sysDictApi.getItems(typeCode)
    searchKeyword.value = ''
  } catch {
    ElMessage.error('加载字典项失败')
  } finally {
    loading.value = false
  }
}

// ─── 类型 CRUD ────────────────────────
function openTypeCreate() {
  typeForm.id = undefined
  typeForm.typeCode = ''
  typeForm.typeName = ''
  typeForm.remark = ''
  typeDrawerVisible.value = true
}

function openTypeEdit(row: SysDictType) {
  typeForm.id = row.id
  typeForm.typeCode = row.typeCode
  typeForm.typeName = row.typeName
  typeForm.remark = row.remark || ''
  typeDrawerVisible.value = true
}

async function handleTypeSave() {
  if (!typeForm.typeCode || !typeForm.typeName) {
    ElMessage.warning('请填写类型编码和名称')
    return
  }
  saving.value = true
  try {
    const body = { typeCode: typeForm.typeCode, typeName: typeForm.typeName, remark: typeForm.remark }
    if (typeForm.id) {
      await sysDictApi.updateType(typeForm.id, body)
      ElMessage.success('已更新')
    } else {
      await sysDictApi.createType(body)
      ElMessage.success('已创建')
    }
    typeDrawerVisible.value = false
    await loadTypes()
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function handleTypeDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该字典类型？', '提示', { type: 'warning' })
    await sysDictApi.deleteType(id)
    ElMessage.success('已删除')
    await loadTypes()
  } catch { /* cancelled */ }
}

// ─── 字典项 CRUD ──────────────────────
function openItemCreate() {
  itemForm.id = undefined
  itemForm.itemLabel = ''
  itemForm.itemValue = ''
  itemForm.sortOrder = items.value.length + 1
  itemForm.color = ''
  itemDrawerVisible.value = true
}

function openItemEdit(row: SysDictItem) {
  itemForm.id = row.id
  itemForm.itemLabel = row.itemLabel
  itemForm.itemValue = row.itemValue
  itemForm.sortOrder = row.sortOrder
  itemForm.color = row.color || ''
  itemDrawerVisible.value = true
}

async function handleItemSave() {
  if (!itemForm.itemLabel || !itemForm.itemValue) {
    ElMessage.warning('请填写显示标签和选项值')
    return
  }
  saving.value = true
  try {
    const body = { typeCode: selectedType.value, itemLabel: itemForm.itemLabel, itemValue: itemForm.itemValue, sortOrder: itemForm.sortOrder, color: itemForm.color }
    if (itemForm.id) {
      await sysDictApi.updateItem(itemForm.id, body)
      ElMessage.success('已更新')
    } else {
      await sysDictApi.createItem(body)
      ElMessage.success('已创建')
    }
    itemDrawerVisible.value = false
    await loadItems(selectedType.value)
  } catch (e: any) {
    ElMessage.error(e?.response?.data?.message || '保存失败')
  } finally {
    saving.value = false
  }
}

async function handleItemDelete(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该字典项？', '提示', { type: 'warning' })
    await sysDictApi.deleteItem(id)
    ElMessage.success('已删除')
    await loadItems(selectedType.value)
  } catch { /* cancelled */ }
}

// ─── 初始化 ───────────────────────────
onMounted(async () => {
  await loadTypes()
  if (types.value.length > 0) {
    await loadItems(types.value[0].typeCode)
  }
})
</script>

<template>
  <div class="page-container">
    <!-- Banner -->
    <div class="page-header-banner page-header-banner--primary">
      <div class="page-header-banner-main">
        <div class="page-header-banner-icon">
          <el-icon :size="28"><Collection /></el-icon>
        </div>
        <div class="page-header-banner-text">
          <h2 class="page-header-banner-title">数据字典</h2>
          <span class="page-header-banner-subtitle">管理系统下拉选项与分类数据</span>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="page-body">
      <!-- Left: Type list -->
      <div class="type-panel">
        <div class="panel-header">
          <span>字典类型</span>
          <el-button size="small" type="primary" :icon="Plus" @click="openTypeCreate">新增</el-button>
        </div>
        <div class="panel-body">
          <el-table
            :data="types"
            border
            size="small"
            highlight-current-row
            @current-change="(row: SysDictType) => row && loadItems(row.typeCode)"
            style="width: 100%"
          >
            <el-table-column prop="typeCode" label="类型编码" min-width="120" show-overflow-tooltip />
            <el-table-column prop="typeName" label="类型名称" min-width="120" show-overflow-tooltip />
            <el-table-column prop="isSystem" label="系统" width="60" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isSystem ? 'warning' : 'info'" size="small" effect="plain" round>
                  {{ row.isSystem ? '是' : '否' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="130" align="center">
              <template #default="{ row }">
                <el-button size="small" type="primary" link @click.stop="openTypeEdit(row)">编辑</el-button>
                <el-button v-if="!row.isSystem" size="small" type="danger" link @click.stop="handleTypeDelete(row.id)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>

      <!-- Right: Items -->
      <div class="items-panel">
        <div class="panel-header">
          <div style="display: flex; align-items: center; gap: 8px;">
            <span>字典项</span>
            <el-tag v-if="selectedType" type="info" size="small">{{ selectedType }}</el-tag>
            <el-tag type="info" size="small" effect="plain">{{ items.length }} 项</el-tag>
          </div>
          <div style="display: flex; gap: 8px; align-items: center;">
            <el-input v-model="searchKeyword" placeholder="搜索" size="small" clearable :prefix-icon="Search" style="width: 140px" />
            <el-button size="small" :icon="Refresh" @click="loadItems(selectedType)">刷新</el-button>
            <el-button size="small" type="primary" :icon="Plus" @click="openItemCreate">新增项</el-button>
          </div>
        </div>
        <div class="panel-body">
          <el-table :data="filteredItems" border v-loading="loading" size="small" style="width: 100%">
            <el-table-column type="index" label="#" width="45" />
            <el-table-column prop="itemLabel" label="显示标签" min-width="150" show-overflow-tooltip />
            <el-table-column prop="itemValue" label="选项值" min-width="120" show-overflow-tooltip />
            <el-table-column prop="sortOrder" label="排序" width="60" align="center" />
            <el-table-column prop="color" label="颜色" width="60" align="center">
              <template #default="{ row }">
                <div v-if="row.color" class="color-indicator" :style="{ backgroundColor: row.color }"></div>
                <span v-else>-</span>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="130" align="center">
              <template #default="{ row }">
                <el-button size="small" type="primary" link @click.stop="openItemEdit(row)">编辑</el-button>
                <el-button size="small" type="danger" link @click.stop="handleItemDelete(row.id)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </div>

    <!-- Type Panel -->
    <RightPanel v-model:visible="typeDrawerVisible" title="字典类型" :width="480" :show-close="true">
      <template #body>
        <el-form :model="typeForm" label-width="80px">
          <el-form-item label="类型编码" required>
            <el-input v-model="typeForm.typeCode" placeholder="如: equipment_type" :disabled="!!typeForm.id" clearable />
          </el-form-item>
          <el-form-item label="类型名称" required>
            <el-input v-model="typeForm.typeName" placeholder="如: 设备类型" clearable />
          </el-form-item>
          <el-form-item label="备注">
            <el-input v-model="typeForm.remark" type="textarea" :rows="2" clearable />
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="typeDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="handleTypeSave" :loading="saving">保存</el-button>
        </div>
      </template>
    </RightPanel>

    <!-- Item Panel -->
    <RightPanel v-model:visible="itemDrawerVisible" title="字典项" :width="480" :show-close="true">
      <template #body>
        <el-form :model="itemForm" label-width="80px">
          <el-form-item label="显示标签" required>
            <el-input v-model="itemForm.itemLabel" placeholder="如: CNC 加工中心" clearable />
          </el-form-item>
          <el-form-item label="选项值" required>
            <el-input v-model="itemForm.itemValue" placeholder="如: CNC" clearable />
          </el-form-item>
          <el-form-item label="排序">
            <el-input-number v-model="itemForm.sortOrder" :min="0" controls-position="right" style="width: 100%" />
          </el-form-item>
          <el-form-item label="颜色">
            <el-popover placement="bottom" :width="340" trigger="click">
              <template #reference>
                <div class="color-picker-trigger">
                  <div v-if="itemForm.color" class="color-swatch-preview" :style="{ backgroundColor: itemForm.color }"></div>
                  <span v-else class="color-placeholder">请选择颜色</span>
                  <span>{{ itemForm.color || '选择颜色' }}</span>
                  <el-icon><ArrowDown /></el-icon>
                </div>
              </template>
              <div class="color-grid">
                <div
                  v-for="c in colorOptions"
                  :key="c"
                  class="color-grid-item"
                  :class="{ active: itemForm.color === c }"
                  :style="{ backgroundColor: c }"
                  @click="itemForm.color = c"
                >
                  <el-icon v-if="itemForm.color === c" class="check-icon"><Check /></el-icon>
                </div>
              </div>
            </el-popover>
          </el-form-item>
        </el-form>
      </template>
      <template #footer>
        <div style="display: flex; gap: 8px; justify-content: flex-end;">
          <el-button size="small" @click="itemDrawerVisible = false">取消</el-button>
          <el-button size="small" type="primary" @click="handleItemSave" :loading="saving">保存</el-button>
        </div>
      </template>
    </RightPanel>
  </div>
</template>

<style scoped>
.page-container { height: 100%; display: flex; flex-direction: column; overflow: hidden; }
.page-body { flex: 1; display: flex; gap: 12px; padding: 12px; overflow: hidden; }

/* Panels */
.type-panel, .items-panel {
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.04);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}
.type-panel { flex: 1; min-width: 0; }
.items-panel { flex: 1; min-width: 0; }

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-4);
  border-bottom: 1px solid var(--el-border-color-lighter);
  background: var(--el-fill-color-blank);
  flex-shrink: 0;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  height: var(--toolbar-height, 40px);
}
.panel-body { flex: 1; overflow-y: auto; padding: 0; }
.panel-body .el-table { width: 100%; }

/* Table row height */
.type-panel :deep(.el-table__row),
.items-panel :deep(.el-table__row) {
  height: var(--table-row-height, 36px);
  line-height: var(--table-row-height, 36px);
}
.type-panel :deep(.el-table__header-wrapper .el-table__cell),
.items-panel :deep(.el-table__header-wrapper .el-table__cell) {
  height: var(--table-row-height, 36px);
  line-height: var(--table-row-height, 36px);
  padding: 0 var(--space-2);
}
.type-panel :deep(.el-table__body-wrapper .el-table__cell),
.items-panel :deep(.el-table__body-wrapper .el-table__cell) {
  padding: 0 var(--space-2);
}

/* Button styles */
.type-panel :deep(.el-button--primary.is-link),
.items-panel :deep(.el-button--primary.is-link) { padding: 0 4px; border: none !important; box-shadow: none !important; }
.type-panel :deep(.el-button--primary.is-link:hover),
.type-panel :deep(.el-button--primary.is-link:focus),
.items-panel :deep(.el-button--primary.is-link:hover),
.items-panel :deep(.el-button--primary.is-link:focus) { border: none !important; box-shadow: none !important; outline: none; }
.type-panel :deep(.el-button--danger.is-link),
.items-panel :deep(.el-button--danger.is-link) { padding: 0 4px; border: none !important; box-shadow: none !important; }
.type-panel :deep(.el-button--danger.is-link:hover),
.type-panel :deep(.el-button--danger.is-link:focus),
.items-panel :deep(.el-button--danger.is-link:hover),
.items-panel :deep(.el-button--danger.is-link:focus) { border: none !important; box-shadow: none !important; outline: none; }

/* Color indicator */
.color-indicator {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  display: inline-block;
  border: 1px solid var(--el-border-color-light);
}

/* Color swatch in dropdown */
.color-swatch {
  width: 14px;
  height: 14px;
  border-radius: 3px;
  flex-shrink: 0;
  border: 1px solid rgba(0, 0, 0, 0.1);
}

/* Color picker grid */
.color-picker-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 8px;
  border: 1px solid var(--el-border-color);
  border-radius: 4px;
  cursor: pointer;
  background: var(--el-bg-color);
  min-width: 200px;
}
.color-swatch-preview {
  width: 16px;
  height: 16px;
  border-radius: 3px;
  flex-shrink: 0;
  border: 1px solid rgba(0, 0, 0, 0.1);
}
.color-placeholder {
  color: var(--el-text-color-placeholder);
}
.color-grid {
  display: grid;
  grid-template-columns: repeat(8, 1fr);
  gap: 6px;
  padding: 8px;
}
.color-grid-item {
  width: 32px;
  height: 24px;
  border-radius: 3px;
  cursor: pointer;
  border: 2px solid transparent;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}
.color-grid-item:hover {
  transform: scale(1.1);
  border-color: var(--el-border-color);
}
.color-grid-item.active {
  border-color: #fff;
  box-shadow: 0 0 0 2px var(--el-color-primary);
}
.check-icon {
  color: #fff;
  font-size: 14px;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.3);
}
</style>

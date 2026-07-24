<script setup lang="ts">
defineOptions({ name: 'Routing' })

import { ref, reactive, computed, onMounted, watch } from 'vue'
import { ElMessage } from 'element-plus'
import {
  Document, Search, Plus, Refresh,
  CopyDocument, Loading,
} from '@element-plus/icons-vue'
import { productApi } from '@/api/basicData'
import {
  getRouteHeaders, getRouteDetail, reorderRouteSteps,
  deleteRouteStep,
} from '@/api/routing'
import type { Product } from '@/types/basicData'
import type { RouteHeaderDto, RouteDetailDto, ProductRouteStepDto, RouteType } from '@/types/routing'
import RouteStepCard from '@/components/routing/RouteStepCard.vue'
import RouteStepDrawer from '@/components/routing/RouteStepDrawer.vue'
import RouteSelector from '@/components/routing/RouteSelector.vue'
import RouteHeaderCard from '@/components/routing/RouteHeaderCard.vue'
import RouteHeaderDrawer from '@/components/routing/RouteHeaderDrawer.vue'
import CloneDialog from '@/components/routing/CloneDialog.vue'

// ─── 数据状态 ──────────────────────────────────────────
const products = ref<Product[]>([])
const filteredProducts = ref<Product[]>([])
const searchKeyword = ref('')
const selectedProductId = ref<number | null>(null)

const routeHeaders = ref<RouteHeaderDto[]>([])
const activeRouteId = ref<number | null>(null)
const activeRouteDetail = ref<RouteDetailDto | null>(null)
const routeLoading = ref(false)

// ─── 拖拽状态 ──────────────────────────────────────────
let dragStepId: number | null = null
let dragIndex: number | null = null
let isDraggingActive = false
let originalSteps: ProductRouteStepDto[] = []
let lastPreviewIds = ''

// ─── 弹窗控制 ──────────────────────────────────────────
const stepDrawerVisible = ref(false)
const stepDrawerStep = ref<ProductRouteStepDto | null>(null)
const stepDrawerStepId = ref<number | null>(null)

const headerDrawerVisible = ref(false)
const editingHeader = ref<RouteHeaderDto | null>(null)

const cloneDialogVisible = ref(false)

// ─── 产品列表搜索 ──────────────────────────────────────
watch(searchKeyword, (val) => {
  const keyword = val.toLowerCase().trim()
  if (!keyword) {
    filteredProducts.value = products.value
  } else {
    filteredProducts.value = products.value.filter(p =>
      p.code.toLowerCase().includes(keyword) ||
      p.name.toLowerCase().includes(keyword)
    )
  }
})

// ─── 选中产品切换 ──────────────────────────────────────
async function selectProduct(product: Product) {
  selectedProductId.value = product.id
  activeRouteId.value = null
  routeHeaders.value = []
  activeRouteDetail.value = null

  routeLoading.value = true
  try {
    routeHeaders.value = await getRouteHeaders(product.id)
    if (routeHeaders.value.length > 0) {
      const activeRoute = routeHeaders.value.find(r => r.isActive) || routeHeaders.value[0]
      selectRoute(activeRoute.id)
    }
  } catch {
    ElMessage.error('加载工艺路线失败')
  } finally {
    routeLoading.value = false
  }
}

// ─── 选中路线切换 ──────────────────────────────────────
async function selectRoute(headerId: number) {
  if (!selectedProductId.value) return
  activeRouteId.value = headerId
  routeLoading.value = true

  try {
    activeRouteDetail.value = await getRouteDetail(headerId)
  } catch {
    ElMessage.error('加载路线详情失败')
    activeRouteDetail.value = null
  } finally {
    routeLoading.value = false
  }
}

// ─── 路线操作 ──────────────────────────────────────────
function openCreateRoute() {
  editingHeader.value = null
  headerDrawerVisible.value = true
}

function openEditRoute(header: RouteHeaderDto) {
  editingHeader.value = { ...header }
  headerDrawerVisible.value = true
}

async function handleRouteDeleted() {
  if (!selectedProductId.value) return
  routeHeaders.value = await getRouteHeaders(selectedProductId.value)
  if (routeHeaders.value.length > 0) {
    const activeRoute = routeHeaders.value.find(r => r.isActive) || routeHeaders.value[0]
    selectRoute(activeRoute.id)
  } else {
    activeRouteId.value = null
    activeRouteDetail.value = null
  }
}

async function handleRouteCreated(headerId: number) {
  if (!selectedProductId.value) return
  routeHeaders.value = await getRouteHeaders(selectedProductId.value)
  selectRoute(headerId)
}

// ─── 实时预览拖拽 ──────────────────────────────────────
let dropOccurred = false

function handleCardDragStart(e: DragEvent, stepId: number, index: number) {
  dragStepId = stepId
  dragIndex = index
  isDraggingActive = true
  dropOccurred = false
  e.dataTransfer?.setData('text/plain', String(stepId))
  e.dataTransfer!.effectAllowed = 'move'

  if (activeRouteDetail.value) {
    originalSteps = [...activeRouteDetail.value.steps]
    const steps = activeRouteDetail.value.steps.map((s, i) =>
      i === index ? { ...s, stepOrder: s.stepOrder, _isPlaceholder: true } : s
    )
    activeRouteDetail.value = { ...activeRouteDetail.value, steps }
    lastPreviewIds = steps.map(s => s.id).join(',')
  }
}

function handleCardDragEnd(e: DragEvent) {
  if (!dropOccurred && isDraggingActive && activeRouteDetail.value) {
    const steps = originalSteps.map(s => ({ ...s, _isPlaceholder: undefined }))
    activeRouteDetail.value = { ...activeRouteDetail.value, steps }
  }
  dropOccurred = false
  dragStepId = null
  dragIndex = null
  isDraggingActive = false
  lastPreviewIds = ''
}

function handleCardDragOver(e: DragEvent, stepId: number, index: number) {
  e.preventDefault()
  e.dataTransfer!.dropEffect = 'move'
  if (!isDraggingActive || !activeRouteDetail.value || dragIndex == null) return

  let insertAt: number
  const targetCard = (e.target as HTMLElement)?.closest('.route-step-card')
  if (targetCard) {
    const rect = targetCard.getBoundingClientRect()
    const isLeft = e.clientX < rect.left + rect.width * 0.4
    insertAt = isLeft ? index : index + 1
  } else {
    insertAt = index
  }

  const draggedItem = originalSteps[dragIndex]
  const preview = originalSteps.filter((_, i) => i !== dragIndex)
  const placeholder = { ...draggedItem, _isPlaceholder: true }
  const newPreview = [
    ...preview.slice(0, insertAt),
    placeholder,
    ...preview.slice(insertAt),
  ]
  const updated = newPreview.map((s, i) => ({ ...s, stepOrder: i + 1 }))

  const newIds = updated.map(s => s.id).join(',')
  if (newIds !== lastPreviewIds) {
    lastPreviewIds = newIds
    activeRouteDetail.value = { ...activeRouteDetail.value, steps: updated }
  }
}

function handleCardDragLeave(e: DragEvent) {
  // 不需要处理
}

function handleContainerDrop(e: DragEvent) {
  handleCardDrop(e, 0)
}

async function handleCardDrop(e: DragEvent, targetIndex: number) {
  e.preventDefault()
  dropOccurred = true
  isDraggingActive = false

  if (dragStepId == null || dragIndex == null) return
  if (!activeRouteDetail.value) return

  const finalSteps = activeRouteDetail.value.steps
    .map((s, i) => ({ ...s, stepOrder: i + 1, _isPlaceholder: undefined }))
  activeRouteDetail.value = { ...activeRouteDetail.value, steps: finalSteps }

  try {
    await reorderRouteSteps(activeRouteDetail.value.id, finalSteps.map(s => s.id))
  } catch {
    activeRouteDetail.value = { ...activeRouteDetail.value, steps: originalSteps }
    ElMessage.error('排序保存失败')
  }

  dragStepId = null
  dragIndex = null
  originalSteps = []
  lastPreviewIds = ''
}

// ─── 步骤操作 ──────────────────────────────────────────
function openAddStep() {
  if (!selectedProductId.value || !activeRouteId.value) {
    ElMessage.warning('请先选择产品并创建路线')
    return
  }
  stepDrawerStep.value = null
  stepDrawerStepId.value = null
  stepDrawerVisible.value = true
}

function openEditStep(step: ProductRouteStepDto) {
  stepDrawerStep.value = step
  stepDrawerStepId.value = step.id
  stepDrawerVisible.value = true
}

async function handleDeleteStep(stepId: number) {
  if (!selectedProductId.value || !activeRouteId.value) return

  try {
    await deleteRouteStep(activeRouteId.value, stepId)
    ElMessage.success('删除成功')
    activeRouteDetail.value = await getRouteDetail(activeRouteId.value)
  } catch { /* error handled by interceptor */ }
}

// ─── 克隆路线 ──────────────────────────────────────────
function openCloneDialog() {
  cloneDialogVisible.value = true
}

// ─── 刷新数据 ──────────────────────────────────────────
async function refreshRoute() {
  if (!selectedProductId.value) return
  routeLoading.value = true
  try {
    if (activeRouteId.value) {
      activeRouteDetail.value = await getRouteDetail(activeRouteId.value)
    }
    routeHeaders.value = await getRouteHeaders(selectedProductId.value)
  } catch {
    ElMessage.error('刷新失败')
  } finally {
    routeLoading.value = false
  }
}

// ─── 计算属性 ──────────────────────────────────────────
const currentProductName = computed(() => {
  if (!selectedProductId.value) return ''
  return products.value.find(p => p.id === selectedProductId.value)?.name || ''
})

const currentProductCode = computed(() => {
  if (!selectedProductId.value) return ''
  return products.value.find(p => p.id === selectedProductId.value)?.code || ''
})

const totalSteps = computed(() => activeRouteDetail.value?.totalSteps || 0)
const total工时 = computed(() => activeRouteDetail.value?.totalStandardTimeMinutes || 0)
const currentRoute = computed(() => routeHeaders.value.find(r => r.id === activeRouteId.value) || null)

// ─── 初始化 ──────────────────────────────────────────
onMounted(async () => {
  try {
    const res = await productApi.list({ page: 1, pageSize: 999 })
    products.value = res.items
    filteredProducts.value = res.items

    if (products.value.length > 0) {
      selectProduct(products.value[0])
    }
  } catch { /* ignore */ }
})
</script>

<template>
  <div class="routing-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="page-header__main">
        <el-icon class="page-header__icon"><Document /></el-icon>
        <div class="page-header__text">
          <h2 class="page-header__title">产品工艺路线</h2>
          <p class="page-header__subtitle">管理产品的工序流程与步骤排序</p>
        </div>
      </div>
      <div class="page-header__actions">
        <el-button @click="refreshRoute" text>
          <el-icon><Refresh /></el-icon>刷新
        </el-button>
        <el-button @click="openCloneDialog">
          <el-icon><CopyDocument /></el-icon>克隆路线
        </el-button>
        <el-button type="primary" @click="openCreateRoute">
          <el-icon><Plus /></el-icon>新增路线
        </el-button>
      </div>
    </div>

    <div class="routing-body">
      <!-- 左侧产品列表 -->
      <aside class="product-sidebar">
        <div class="sidebar-header">
          <h3 class="sidebar-title">产品列表</h3>
          <el-input
            v-model="searchKeyword"
            placeholder="搜索产品..."
            clearable
            size="small"
            :prefix-icon="Search"
          />
        </div>
        <div class="sidebar-list">
          <div
            v-for="product in filteredProducts"
            :key="product.id"
            class="sidebar-item"
            :class="{ 'sidebar-item--active': selectedProductId === product.id }"
            @click="selectProduct(product)"
          >
            <div class="sidebar-item__main">
              <span class="sidebar-item__name">{{ product.name }}</span>
              <span class="sidebar-item__code">{{ product.code }}</span>
            </div>
          </div>
          <div v-if="filteredProducts.length === 0" class="sidebar-empty">
            暂无匹配产品
          </div>
        </div>
      </aside>

      <!-- 右侧工艺路线 -->
      <main class="routing-main">
        <!-- 路线选择器 -->
        <div v-if="selectedProductId" class="route-selector-wrap">
          <RouteSelector
            :product-id="selectedProductId"
            :routes="routeHeaders"
            :active-route-id="activeRouteId"
            @route-selected="selectRoute"
            @create-route="openCreateRoute"
            @route-deleted="handleRouteDeleted"
            @route-created="handleRouteCreated"
          />
        </div>

        <!-- 路线信息卡片 -->
        <RouteHeaderCard
          v-if="currentRoute"
          :route="currentRoute"
          @edit="openEditRoute(currentRoute)"
          @toggle="handleRouteDeleted"
          @delete="handleRouteDeleted"
        />

        <!-- 加载状态 -->
        <div v-if="routeLoading" class="route-content route-content--loading">
          <el-icon class="is-loading" :size="32"><Loading /></el-icon>
          <span>加载中...</span>
        </div>

        <!-- 空状态 -->
        <div
          v-else-if="selectedProductId && !activeRouteDetail && routeHeaders.length === 0"
          class="route-content route-content--empty"
        >
          <el-empty description="该产品暂无工艺路线">
            <el-button type="primary" @click="openCreateRoute">
              <el-icon><Plus /></el-icon>创建第一条工艺路线
            </el-button>
          </el-empty>
        </div>

        <!-- 空状态 - 没有选中路线 -->
        <div
          v-else-if="selectedProductId && !activeRouteDetail && routeHeaders.length > 0"
          class="route-content route-content--empty"
        >
          <el-empty description="请选择一条路线" :image-size="80" />
        </div>

        <!-- 步骤卡片流 -->
        <div
          v-else-if="activeRouteDetail && activeRouteDetail.steps.length > 0"
          class="route-content route-content--flow"
        >
          <div class="flow-container" @drop.prevent="handleContainerDrop">
            <template v-for="(step, index) in activeRouteDetail.steps" :key="step.id">
              <RouteStepCard
                :step="step"
                :draggable="true"
                :is-dragging="dragIndex === index"
                :is-drag-over="false"
                @drag-start="(e: DragEvent, id: number) => handleCardDragStart(e, id, index)"
                @drag-end="handleCardDragEnd"
                @drag-over="(e: DragEvent, id: number) => handleCardDragOver(e, id, index)"
                @drag-leave="handleCardDragLeave"
                @drop="(e: DragEvent, id: number) => handleCardDrop(e, id, index)"
                @edit="openEditStep(step)"
                @delete="handleDeleteStep"
              />
              <span
                v-if="index < activeRouteDetail.steps.length - 1"
                class="flow-arrow"
                :class="{ 'flow-arrow--fade': step._isPlaceholder }"
              >→</span>
            </template>

            <!-- 添加步骤占位 -->
            <div class="flow-add-placeholder" @click="openAddStep">
              <el-icon class="add-icon"><Plus /></el-icon>
              <span>添加步骤</span>
            </div>
          </div>
        </div>

        <!-- 未选择产品 -->
        <div v-else class="route-content route-content--idle">
          <el-empty description="请从左侧选择产品" :image-size="120" />
        </div>
      </main>
    </div>

    <!-- 步骤编辑抽屉 -->
    <RouteStepDrawer
      v-model="stepDrawerVisible"
      :product-id="selectedProductId!"
      :header-id="activeRouteId!"
      :step-id="stepDrawerStepId"
      :step="stepDrawerStep"
      @saved="refreshRoute"
    />

    <!-- 路线头编辑抽屉 -->
    <RouteHeaderDrawer
      v-model="headerDrawerVisible"
      :product-id="selectedProductId!"
      :header-id="editingHeader?.id"
      :header="editingHeader"
      @saved="handleRouteCreated"
      @closed="editingHeader = null"
    />

    <!-- 克隆路线对话框 -->
    <CloneDialog
      v-model="cloneDialogVisible"
      @cloned="refreshRoute"
    />
  </div>
</template>

<style scoped>
.routing-page {
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--el-bg-color-page);
}

/* ─── 页面头部 ──────────────────────────────────── */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  background: var(--el-bg-color);
  border-bottom: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
}

.page-header__main {
  display: flex;
  align-items: center;
  gap: 12px;
}

.page-header__icon {
  font-size: 24px;
  color: var(--el-color-primary);
}

.page-header__text {
  display: flex;
  flex-direction: column;
}

.page-header__title {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.page-header__subtitle {
  margin: 2px 0 0;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.page-header__actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* ─── 主体布局 ──────────────────────────────────── */
.routing-body {
  flex: 1;
  display: flex;
  overflow: hidden;
}

/* ─── 左侧产品列表 ──────────────────────────────── */
.product-sidebar {
  width: 240px;
  min-width: 240px;
  background: var(--el-bg-color);
  border-right: 1px solid var(--el-border-color-lighter);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.sidebar-header {
  padding: 16px;
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.sidebar-title {
  margin: 0 0 10px;
  font-size: 14px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.sidebar-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.sidebar-item {
  padding: 10px 12px;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
  margin-bottom: 2px;
}

.sidebar-item:hover {
  background: var(--el-fill-color-light);
}

.sidebar-item--active {
  background: var(--el-color-primary-light-9);
  border: 1px solid var(--el-color-primary-light-5);
}

.sidebar-item--active .sidebar-item__name {
  color: var(--el-color-primary);
  font-weight: 600;
}

.sidebar-item__main {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.sidebar-item__name {
  font-size: 13px;
  color: var(--el-text-color-primary);
  line-height: 1.3;
}

.sidebar-item__code {
  font-size: 11px;
  color: var(--el-text-color-secondary);
}

.sidebar-empty {
  padding: 20px;
  text-align: center;
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

/* ─── 右侧工艺路线 ──────────────────────────────── */
.routing-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: var(--el-bg-color-page);
}

.route-selector-wrap {
  padding: 12px 20px 0;
  flex-shrink: 0;
}

.route-content {
  flex: 1;
  padding: 20px;
  overflow-x: auto;
  overflow-y: auto;
  display: flex;
  align-items: center;
  justify-content: center;
}

.route-content--loading {
  flex-direction: column;
  gap: 12px;
  color: var(--el-text-color-secondary);
}

.route-content--empty {
  flex-direction: column;
  gap: 12px;
}

.route-content--idle {
  flex-direction: column;
}

/* ─── 步骤卡片流 ────────────────────────────────── */
.flow-container {
  display: flex;
  align-items: center;
  gap: 0;
  padding: 16px 20px;
  overflow-x: auto;
  min-height: 160px;
  position: relative;
}

.flow-arrow {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  width: 20px;
  height: 24px;
  font-size: 16px;
  color: var(--el-color-primary);
  font-weight: bold;
  opacity: 0.5;
  margin: 0 14px;
  z-index: 1;
  transition: opacity 0.2s;
}

.flow-arrow--fade {
  opacity: 0.15;
}

.flow-add-placeholder {
  width: 140px;
  min-height: 120px;
  border: 2px dashed var(--el-border-color-light);
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
  color: var(--el-text-color-secondary);
  font-size: 13px;
  margin-left: 8px;
}

.flow-add-placeholder:hover {
  border-color: var(--el-color-primary);
  color: var(--el-color-primary);
  background: var(--el-color-primary-light-9);
}

.flow-add-placeholder .add-icon {
  font-size: 22px;
}

:deep(.el-icon.is-loading) {
  animation: loading-rotate 1s linear infinite;
}

@keyframes loading-rotate {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.flow-container::-webkit-scrollbar {
  height: 6px;
}

.flow-container::-webkit-scrollbar-track {
  background: var(--el-fill-color-lighter);
  border-radius: 3px;
}

.flow-container::-webkit-scrollbar-thumb {
  background: var(--el-border-color);
  border-radius: 3px;
}

.flow-container::-webkit-scrollbar-thumb:hover {
  background: var(--el-border-color-dark);
}

.product-sidebar::-webkit-scrollbar {
  width: 4px;
}

.product-sidebar::-webkit-scrollbar-thumb {
  background: var(--el-border-color);
  border-radius: 2px;
}
</style>
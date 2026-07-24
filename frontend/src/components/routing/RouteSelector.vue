<template>
  <div class="route-selector">
    <div class="route-selector__header">
      <el-tabs
        v-model="activeRouteId"
        type="card"
        @tab-change="handleTabChange"
        class="route-tabs"
      >
        <el-tab-pane
          v-for="route in routes"
          :key="route.id"
          :name="route.id"
          :label="route.routeName"
          :closable="routes.length > 1"
          @close="handleCloseTab(route.id)"
        >
          <template #label>
            <div class="tab-label">
              <RouteTypeTag :type="route.routeType" />
              <span>{{ route.routeName }}</span>
              <el-tag v-if="route.isDefault" type="primary" size="small" effect="plain" class="default-tag">默认</el-tag>
              <el-tag v-if="!route.isActive" type="info" size="small">已禁用</el-tag>
              <el-button
                size="small"
                text
                class="toggle-btn"
                @click.stop="handleToggle(route)"
              >
                {{ route.isActive ? '禁用' : '启用' }}
              </el-button>
            </div>
          </template>
        </el-tab-pane>
      </el-tabs>
      <el-button text @click="emit('create-route')">
        <el-icon><Plus /></el-icon> 新增路线
      </el-button>
    </div>

    <!-- 空状态 -->
    <div v-if="routes.length === 0" class="route-selector__empty">
      <el-empty description="该产品的暂无工艺路线" :image-size="80">
        <el-button type="primary" @click="emit('create-route')">
          <el-icon><Plus /></el-icon> 创建第一条路线
        </el-button>
      </el-empty>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { ElMessage } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import type { RouteHeaderDto } from '@/types/routing'
import RouteTypeTag from './RouteTypeTag.vue'
import { toggleRouteActive, deleteRouteHeader } from '@/api/routing'

defineOptions({ name: 'RouteSelector' })

const props = defineProps<{
  productId: number
  routes: RouteHeaderDto[]
  activeRouteId: number | null
}>()

const emit = defineEmits<{
  'route-selected': [headerId: number]
  'create-route': []
  'route-toggled': [route: RouteHeaderDto]
  'route-deleted': [headerId: number]
  'route-created': [headerId: number]
}>()

const activeRouteId = ref<number | null>(props.activeRouteId)

watch(() => props.activeRouteId, (val) => {
  if (val !== null) activeRouteId.value = val
})

watch(() => props.routes, (val) => {
  if (val.length > 0 && activeRouteId.value === null) {
    const activeRoute = val.find(r => r.isActive) || val[0]
    activeRouteId.value = activeRoute.id
    emit('route-selected', activeRoute.id)
  }
}, { immediate: true })

function handleTabChange(headerId: number | string) {
  const id = typeof headerId === 'string' ? Number(headerId) : headerId
  if (id !== activeRouteId.value) {
    activeRouteId.value = id
    emit('route-selected', id)
  }
}

async function handleToggle(route: RouteHeaderDto) {
  try {
    await toggleRouteActive(route.id)
    ElMessage.success(route.isActive ? '已禁用' : '已启用')
    emit('route-toggled', route)
  } catch { /* error handled by interceptor */ }
}

async function handleCloseTab(headerId: number) {
  try {
    await deleteRouteHeader(headerId)
    ElMessage.success('删除成功')
    emit('route-deleted', headerId)
  } catch { /* error handled by interceptor */ }
}
</script>

<style scoped>
.route-selector {
  margin-bottom: 16px;
}

.route-selector__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.route-tabs {
  flex: 1;
}

:deep(.el-tabs__header) {
  margin: 0;
}

:deep(.el-tabs__nav) {
  border: none;
}

:deep(.el-tabs__item) {
  height: 32px;
  line-height: 32px;
  font-size: 13px;
}

.tab-label {
  display: flex;
  align-items: center;
  gap: 6px;
}

.default-tag {
  margin-left: 2px;
}

.toggle-btn {
  margin-left: 2px;
  font-size: 12px;
  padding: 0 4px;
  height: 20px;
  line-height: 18px;
}

.route-selector__empty {
  padding: 20px 0;
}
</style>
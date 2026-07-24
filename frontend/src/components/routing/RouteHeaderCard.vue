<template>
  <div class="route-header-card">
    <div class="route-header-card__info">
      <div class="route-header-card__title-row">
        <RouteTypeTag :type="route.routeType" />
        <span class="route-header-card__name">{{ route.routeName }}</span>
        <el-tag v-if="route.isDefault" type="primary" size="small" effect="plain">默认</el-tag>
        <el-tag v-if="!route.isActive" type="info" size="small">已禁用</el-tag>
      </div>
      <div class="route-header-card__meta">
        <span class="route-header-card__code">{{ route.routeCode }}</span>
        <el-divider direction="vertical" />
        <el-tag type="info" size="small">{{ route.stepCount }} 个步骤</el-tag>
        <el-tag v-if="route.totalStandardTimeMinutes > 0" type="warning" size="small">
          总工时 {{ route.totalStandardTimeMinutes }} min
        </el-tag>
      </div>
      <p v-if="route.description" class="route-header-card__desc">{{ route.description }}</p>
    </div>
    <div class="route-header-card__actions">
      <el-button size="small" text @click="emit('edit')">
        <el-icon><Edit /></el-icon> 编辑
      </el-button>
      <el-button size="small" text @click="emit('toggle')" :type="route.isActive ? 'warning' : 'success'">
        <el-icon><component :is="route.isActive ? 'CircleClose' : 'CircleCheck'" /></el-icon>
        {{ route.isActive ? '禁用' : '启用' }}
      </el-button>
      <el-popconfirm
        title="确认删除此路线及其所有步骤？"
        confirm-button-text="删除"
        cancel-button-text="取消"
        @confirm="emit('delete')"
      >
        <template #reference>
          <el-button size="small" text type="danger">
            <el-icon><Delete /></el-icon> 删除
          </el-button>
        </template>
      </el-popconfirm>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Edit, Delete, CircleClose, CircleCheck } from '@element-plus/icons-vue'
import type { RouteHeaderDto } from '@/types/routing'
import RouteTypeTag from './RouteTypeTag.vue'

defineOptions({ name: 'RouteHeaderCard' })

const props = defineProps<{ route: RouteHeaderDto }>()
const emit = defineEmits<{
  edit: []
  toggle: []
  delete: []
}>()
</script>

<style scoped>
.route-header-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 8px;
  margin-bottom: 16px;
}

.route-header-card__info {
  flex: 1;
  min-width: 0;
}

.route-header-card__title-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 6px;
}

.route-header-card__name {
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.route-header-card__meta {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}

.route-header-card__code {
  font-family: monospace;
}

.route-header-card__desc {
  margin: 6px 0 0;
  font-size: 12px;
  color: var(--el-text-color-regular);
  line-height: 1.4;
}

.route-header-card__actions {
  display: flex;
  gap: 4px;
  flex-shrink: 0;
  margin-left: 16px;
}
</style>
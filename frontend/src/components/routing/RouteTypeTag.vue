<template>
  <el-tag :type="color" size="small" effect="plain" class="route-type-tag">
    {{ label }}
  </el-tag>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { RouteType } from '@/types/routing'
import { ROUTE_TYPE_LABELS, ROUTE_TYPE_COLORS } from '@/types/routing'

defineOptions({ name: 'RouteTypeTag' })

const props = defineProps<{ type: RouteType }>()

const label = computed(() => ROUTE_TYPE_LABELS[props.type] || props.type)
const color = computed(() => ROUTE_TYPE_COLORS[props.type] || 'info')
</script>

<style scoped>
.route-type-tag {
  font-size: 11px;
  padding: 0 6px;
  height: 20px;
  line-height: 18px;
}
</style>
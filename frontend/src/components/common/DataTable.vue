<script setup lang="ts">
import { computed } from 'vue'

export interface Column {
  prop?: string
  label: string
  width?: string | number
  minWidth?: string | number
  fixed?: boolean | 'left' | 'right'
  align?: 'left' | 'center' | 'right'
  sortable?: boolean
  showOverflowTooltip?: boolean
  slotName?: string
  visible?: boolean
}

export interface TableProps {
  data: any[]
  columns?: Column[]
  loading?: boolean
  border?: boolean
  stripe?: boolean
  height?: string
  maxHeight?: string
  rowKey?: string
  pagination?: boolean
  total?: number
  currentPage?: number
  pageSize?: number
  pageSizes?: number[]
  selectable?: boolean
  showIndex?: boolean
  indexLabel?: string
  emptyText?: string
  highlightCurrentRow?: boolean
  fit?: boolean
}

const props = withDefaults(defineProps<TableProps>(), {
  columns: () => [],
  loading: false,
  border: true,
  stripe: true,
  pagination: true,
  total: 0,
  currentPage: 1,
  pageSize: 20,
  pageSizes: () => [10, 20, 50, 100],
  selectable: false,
  showIndex: false,
  indexLabel: '#',
  emptyText: '',
  highlightCurrentRow: false,
  fit: true,
})

const emit = defineEmits<{
  'update:currentPage': [val: number]
  'update:pageSize': [val: number]
  pageChange: [page: number]
  sizeChange: [size: number]
  rowClick: [row: any, column: any, event: Event]
  rowDblClick: [row: any, column: any, event: Event]
  selectionChange: [selection: any[]]
  sortChange: [data: any]
}>()

const currentPage = computed({
  get: () => props.currentPage,
  set: (val: number) => emit('update:currentPage', val),
})

const pageSize = computed({
  get: () => props.pageSize,
  set: (val: number) => emit('update:pageSize', val),
})

function handlePageChange(page: number) {
  emit('pageChange', page)
}

function handleSizeChange(size: number) {
  emit('sizeChange', size)
}

function handleRowClick(row: any, column: any, event: Event) {
  emit('rowClick', row, column, event)
}

function handleRowDblClick(row: any, column: any, event: Event) {
  emit('rowDblClick', row, column, event)
}

function handleSortChange(data: any) {
  emit('sortChange', data)
}
</script>

<template>
  <div class="data-table-wrapper">
    <el-table
      :data="data"
      :loading="loading"
      :border="border"
      :stripe="stripe"
      :height="height"
      :max-height="maxHeight"
      :row-key="rowKey || 'id'"
      :empty-text="emptyText || '暂无数据'"
      :highlight-current-row="highlightCurrentRow"
      :fit="fit"
      v-bind="$attrs"
      @row-click="handleRowClick"
      @row-dblclick="handleRowDblClick"
      @selection-change="(selection: any[]) => emit('selectionChange', selection)"
      @sort-change="handleSortChange"
    >
      <el-table-column v-if="selectable" type="selection" width="44" fixed="left" align="center" />
      
      <el-table-column v-if="showIndex" :label="indexLabel" width="56" fixed="left" align="center">
        <template #default="{ $index }">
          <span class="index-cell">{{ (currentPage - 1) * pageSize + $index + 1 }}</span>
        </template>
      </el-table-column>
      
      <slot></slot>
      
      <el-table-column
        v-for="col in columns"
        :key="col.prop || col.label"
        :prop="col.prop"
        :label="col.label"
        :width="col.width"
        :min-width="col.minWidth || 100"
        :fixed="col.fixed"
        :align="col.align || 'center'"
        :sortable="col.sortable"
      >
        <template #default="{ row }">
          <slot :name="col.slotName || col.prop || ''" :row="row">
            <span :title="col.prop ? String(row[col.prop] ?? '') : ''">
              {{ col.prop ? row[col.prop] ?? '-' : '-' }}
            </span>
          </slot>
        </template>
      </el-table-column>
    </el-table>

    <div v-if="pagination && total > 0" class="pagination-wrapper">
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="pageSizes"
        layout="total, sizes, prev, pager, next, jumper"
        @current-change="handlePageChange"
        @size-change="handleSizeChange"
      />
    </div>
  </div>
</template>

<style scoped>
.data-table-wrapper {
  background: var(--el-bg-color);
  border-radius: var(--radius-lg);
  border: 1px solid var(--el-border-color-lighter);
  overflow: hidden;
}

.el-table {
  border-radius: 0;
  overflow: visible;
}

/* ─── Table Header ─── */
.el-table th.el-table__cell {
  background: var(--el-fill-color-light) !important;
  font-weight: var(--font-semibold);
  color: var(--el-text-color-regular);
  font-size: var(--font-sm);
  letter-spacing: 0.02em;
  height: 42px;
  padding: 0 12px;
  border-bottom: 1px solid var(--el-border-color-light);
  white-space: nowrap;
}

/* ─── Table Body ─── */
.el-table td.el-table__cell {
  padding: 10px 12px;
  height: 42px;
  line-height: 20px;
  border-bottom: 1px solid var(--el-border-color-lighter);
  text-align: center;
  transition: background-color var(--duration-fast) var(--ease-out);
}

/* Row hover */
.data-table-wrapper .el-table__row:hover > td {
  background-color: var(--primary-light, #e6f4ff) !important;
}

/* Striped rows */
.data-table-wrapper .el-table__row--striped > td {
  background-color: var(--el-fill-color-lighter, #fafafa) !important;
}
.data-table-wrapper .el-table__row--striped:hover > td {
  background-color: var(--primary-light, #e6f4ff) !important;
}

/* ─── Index Column ─── */
.index-cell {
  font-weight: var(--font-medium);
  color: var(--el-text-color-secondary);
  font-size: var(--font-sm);
}

/* ─── Pagination ─── */
.pagination-wrapper {
  display: flex;
  justify-content: flex-end;
  padding: var(--space-4) var(--space-4) 0;
  border-top: 1px solid var(--el-border-color-lighter);
}

/* ─── Empty State ─── */
:deep(.el-table__empty-block) {
  min-height: 100px;
}

/* ─── Dark mode overrides ─── */
html.dark .el-table th.el-table__cell {
  background: var(--el-fill-color-light) !important;
  color: var(--el-text-color-regular);
}

html.dark .el-table td.el-table__cell {
  border-bottom: 1px solid var(--el-border-color-lighter);
}

html.dark .data-table-wrapper .el-table__row:hover > td {
  background-color: var(--el-fill-color, #363637) !important;
}

html.dark .data-table-wrapper .el-table__row--striped > td {
  background-color: var(--el-fill-color-lighter, #2a2a2d) !important;
}
html.dark .data-table-wrapper .el-table__row--striped:hover > td {
  background-color: var(--el-fill-color, #363637) !important;
}
</style>

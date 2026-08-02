<script setup lang="ts">
interface Props {
  status?: string
  type?: 'success' | 'warning' | 'danger' | 'info'
  text?: string
  size?: 'small' | 'default' | 'large'
  showDot?: boolean
  round?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  type: 'info',
  size: 'small',
  showDot: true,
  round: true,
})

const statusMap: Record<string, { type: string; text: string }> = {
  // Generic
  success: { type: 'success', text: props.text || '成功' },
  warning: { type: 'warning', text: props.text || '警告' },
  danger: { type: 'danger', text: props.text || '失败' },
  error: { type: 'danger', text: props.text || '错误' },
  info: { type: 'info', text: props.text || '信息' },
  
  // Common states
  enabled: { type: 'success', text: '启用' },
  disabled: { type: 'info', text: '停用' },
  inactive: { type: 'info', text: '未激活' },
  
  // Workflow states
  pending: { type: 'warning', text: '待处理' },
  processing: { type: 'warning', text: '处理中' },
  in_progress: { type: 'warning', text: '进行中' },
  completed: { type: 'success', text: '已完成' },
  finished: { type: 'success', text: '已完成' },
  
  // Approval states
  rejected: { type: 'danger', text: '已驳回' },
  approved: { type: 'success', text: '已通过' },
  submitted: { type: 'warning', text: '已提交' },
  
  // Quality states
  passed: { type: 'success', text: '合格' },
  passed_label: { type: 'success', text: '合格' },
  fail: { type: 'danger', text: '不合格' },
  failed: { type: 'danger', text: '不合格' },
  anomaly: { type: 'warning', text: '异常' },
  abnormal: { type: 'danger', text: '异常' },
  normal: { type: 'success', text: '正常' },
  accepted: { type: 'success', text: '已接受' },
  
  // Open/Close states
  open: { type: 'warning', text: '开启' },
  closed: { type: 'info', text: '关闭' },
  active: { type: 'success', text: '活跃' },
  archived: { type: 'info', text: '已归档' },
}

function getStatusInfo() {
  const key = (props.status || '').toLowerCase().replace(/[\s_]+/g, '_')
  return statusMap[key] || { type: props.type, text: props.text || props.status }
}

const info = getStatusInfo()
</script>

<template>
  <span :class="['status-tag', `status-tag--${info.type}`, { 'status-tag--round': round, 'status-tag--dot': showDot }]">
    <span v-if="showDot" class="status-tag__dot"></span>
    <span class="status-tag__text">{{ info.text }}</span>
  </span>
</template>

<style scoped>
.status-tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 8px;
  font-size: 11px;
  font-weight: 500;
  line-height: 1.4;
  white-space: nowrap;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.status-tag--success {
  background: #f6ffed;
  color: #52c41a;
}

.status-tag--warning {
  background: #fffbe6;
  color: #faad14;
}

.status-tag--danger {
  background: #fff2f0;
  color: #ff4d4f;
}

.status-tag--info {
  background: #fafafa;
  color: #8c8c8c;
}

.status-tag--round {
  border-radius: 10px;
}

.status-tag__dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  flex-shrink: 0;
}

.status-tag--success .status-tag__dot {
  background: #52c41a;
  box-shadow: 0 0 3px rgba(82, 196, 26, 0.4);
}

.status-tag--warning .status-tag__dot {
  background: #faad14;
  box-shadow: 0 0 3px rgba(250, 173, 20, 0.4);
}

.status-tag--danger .status-tag__dot {
  background: #ff4d4f;
  box-shadow: 0 0 3px rgba(255, 77, 79, 0.4);
}

.status-tag--info .status-tag__dot {
  background: #8c8c8c;
}

html.dark .status-tag--success {
  background: rgba(82, 196, 26, 0.15);
}

html.dark .status-tag--warning {
  background: rgba(250, 173, 20, 0.15);
}

html.dark .status-tag--danger {
  background: rgba(255, 77, 79, 0.15);
}

html.dark .status-tag--info {
  background: #27272f;
}
</style>

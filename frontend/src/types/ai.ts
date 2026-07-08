// ─── AI 预警级别 ──────────────────────────────────────
export const ALERT_LEVEL_OPTIONS = [
  { value: 'critical', label: '致命', type: 'danger' },
  { value: 'high', label: '高', type: 'warning' },
  { value: 'medium', label: '中', type: 'primary' },
  { value: 'low', label: '低', type: 'info' },
]

export const ALERT_LEVEL_MAP: Record<string, string> = {
  critical: '致命', high: '高', medium: '中', low: '低',
}

// ─── AI 模型类型 ──────────────────────────────────────
export const MODEL_TYPE_OPTIONS = [
  { value: 'classification', label: '分类模型' },
  { value: 'regression', label: '回归模型' },
  { value: 'anomaly_detection', label: '异常检测' },
  { value: 'prediction', label: '预测模型' },
]

export const MODEL_TYPE_MAP: Record<string, string> = {
  classification: '分类模型', regression: '回归模型', anomaly_detection: '异常检测', prediction: '预测模型',
}

// ─── 模型状态 ─────────────────────────────────────────
export const MODEL_STATUS_OPTIONS = [
  { value: 'training', label: '训练中', type: 'warning' },
  { value: 'ready', label: '就绪', type: 'success' },
  { value: 'error', label: '错误', type: 'danger' },
]

export const MODEL_STATUS_MAP: Record<string, string> = {
  training: '训练中', ready: '就绪', error: '错误',
}

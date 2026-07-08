// ─── M11 设备关联 ──────────────────────────────────────

export interface EquipmentParamMapping {
  id: number
  equipmentId: number
  mqttTopic: string
  systemParamCode: string
  paramGroupId?: number
  dataType: string
  unit?: string
  createdAt: string
  updatedAt: string
}

export interface EquipmentStatusHistory {
  id: number
  equipmentId: number
  signal: string
  signalData?: string
  recordedAt: string
}

export interface EquipmentQualityCorrelation {
  id: number
  equipmentId: number
  analysisDate: string
  correlationData: string
  createdAt: string
}

// ─── DTOs ──────────────────────────────────────────────

export interface CreateParamMapping {
  equipmentId: number
  mqttTopic: string
  systemParamCode: string
  paramGroupId?: number
  dataType: string
  unit?: string
}

export interface CreateStatusRecord {
  equipmentId: number
  signal: string
  signalData?: string
}

// ─── Constants ──────────────────────────────────────────

export const DATA_TYPE_OPTIONS = [
  { value: 'numeric', label: '数字' },
  { value: 'count', label: '计数' },
  { value: 'status', label: '状态' },
]

export const SIGNAL_OPTIONS = [
  { value: 'running', label: '运行', type: 'success' },
  { value: 'idle', label: '待机', type: 'info' },
  { value: 'fault', label: '故障', type: 'danger' },
]

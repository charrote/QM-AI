import request from './request'
import type {
  TraceResult,
  NgDiffusionResult,
  RecallSimulationResult,
} from '@/types/trace'

const BASE = '/trace'

export const traceApi = {
  /** 按 SN 编码追溯 */
  traceBySn(serialNumber: string): Promise<TraceResult> {
    return request.get(`${BASE}/sn/${encodeURIComponent(serialNumber)}`).then(r => r.data)
  },

  /** 按批次号追溯 */
  traceByBatch(batchCode: string): Promise<TraceResult> {
    return request.get(`${BASE}/batch/${encodeURIComponent(batchCode)}`).then(r => r.data)
  },

  /** 按设备追溯 */
  traceByEquipment(equipmentId: number | string): Promise<TraceResult> {
    return request.get(`${BASE}/equipment/${equipmentId}`).then(r => r.data)
  },

  /** NG 扩散分析 */
  ngDiffusion(batchCode: string): Promise<NgDiffusionResult> {
    return request.get(`${BASE}/ng-diffusion/${encodeURIComponent(batchCode)}`).then(r => r.data)
  },

  /** 召回模拟 */
  recallSimulation(batchCode: string): Promise<RecallSimulationResult> {
    return request.get(`${BASE}/recall-simulation/${encodeURIComponent(batchCode)}`).then(r => r.data)
  },
}
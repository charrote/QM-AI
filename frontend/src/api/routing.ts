import request from './request'
import type {
  RouteListDto, RouteDetailDto, RouteHeaderDto,
  CreateRouteHeaderDto, UpdateRouteHeaderDto,
  CloneRouteHeaderDto, ProductRouteDto, ProductRouteStepDto,
} from '@/types/routing'

// ─── 路线头 API ──────────────────────────────────────

export function getRouteHeaders(productId: number) {
  return request.get<RouteListDto>('/routings/headers', { params: { productId } }).then(r => r.data)
}

export function getRouteDetail(headerId: number) {
  return request.get<RouteDetailDto>(`/routings/headers/${headerId}`).then(r => r.data)
}

export function createRouteHeader(data: CreateRouteHeaderDto) {
  return request.post<RouteHeaderDto>('/routings/headers', data).then(r => r.data)
}

export function updateRouteHeader(headerId: number, data: UpdateRouteHeaderDto) {
  return request.put(`/routings/headers/${headerId}`, data).then(r => r.data)
}

export function deleteRouteHeader(headerId: number) {
  return request.delete(`/routings/headers/${headerId}`).then(r => r.data)
}

export function toggleRouteActive(headerId: number) {
  return request.patch(`/routings/headers/${headerId}/toggle-active`).then(r => r.data)
}

export function setRouteDefault(headerId: number) {
  return request.patch(`/routings/headers/${headerId}/set-default`).then(r => r.data)
}

export function cloneRouteHeader(data: CloneRouteHeaderDto) {
  return request.post('/routings/headers/clone', data).then(r => r.data)
}

// ─── 路线步骤 API ──────────────────────────────────────

export function getRouteSteps(headerId: number) {
  return request.get<ProductRouteStepDto[]>(`/routings/headers/${headerId}/steps`).then(r => r.data)
}

export function addRouteStep(headerId: number, data: { processId: number; stepOrder?: number; standardTimeMinutes?: number; description?: string; preWaitTimeMinutes?: number; postWaitTimeMinutes?: number }) {
  return request.post(`/routings/headers/${headerId}/steps`, data).then(r => r.data)
}

export function updateRouteStep(headerId: number, stepId: number, data: { processId: number; standardTimeMinutes?: number; description?: string; preWaitTimeMinutes?: number; postWaitTimeMinutes?: number }) {
  return request.put(`/routings/headers/${headerId}/steps/${stepId}`, data).then(r => r.data)
}

export function deleteRouteStep(headerId: number, stepId: number) {
  return request.delete(`/routings/headers/${headerId}/steps/${stepId}`).then(r => r.data)
}

export function reorderRouteSteps(headerId: number, stepIds: number[]) {
  return request.patch(`/routings/headers/${headerId}/steps/reorder`, { stepIds }).then(r => r.data)
}

// ─── 兼容旧接口 ──────────────────────────────────────

export function getProductRoute(productId: number) {
  return request.get<ProductRouteDto>(`/routings/product/${productId}`).then(r => r.data)
}

export function reorderSteps(productId: number, stepIds: number[]) {
  return request.patch('/routings/reorder', { productId, stepIds }).then(r => r.data)
}

export function batchCreateSteps(steps: { productId: number; processId: number; standardTimeMinutes?: number; description?: string }[]) {
  return request.post('/routings/batch', steps).then(r => r.data)
}

export function cloneRoute(sourceProductId: number, targetProductId: number) {
  return request.post('/routings/clone', { sourceProductId, targetProductId }).then(r => r.data)
}

export function deleteStep(id: number) {
  return request.delete(`/routings/${id}`).then(r => r.data)
}

export function updateStep(id: number, data: { processId: number; standardTimeMinutes?: number; description?: string }) {
  return request.put(`/routings/${id}`, data).then(r => r.data)
}
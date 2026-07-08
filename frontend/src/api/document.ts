import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type { Document, DocumentVersion, CreateDocument, UploadResponse, ApproveDocument, RejectDocument } from '@/types/document'

const BASE = '/m12/documents'

export const documentApi = {
  /** 获取文档列表 */
  getList(params?: PagedRequest & { docType?: string; status?: string }): Promise<PagedResult<Document>> {
    return request.get(BASE, { params }).then(r => r.data)
  },

  /** 获取文档详情 */
  getById(id: number): Promise<Document> {
    return request.get(`${BASE}/${id}`).then(r => r.data)
  },

  /** 创庻文档 */
  create(data: CreateDocument): Promise<Document> {
    return request.post(BASE, data).then(r => r.data)
  },

  /** 更新文档 */
  update(id: number, data: Partial<CreateDocument>): Promise<Document> {
    return request.put(`${BASE}/${id}`, data).then(r => r.data)
  },

  /** 删除文档 */
  remove(id: number): Promise<void> {
    return request.delete(`${BASE}/${id}`)
  },

  /** 批准文档 */
  approve(id: number, data: ApproveDocument): Promise<void> {
    return request.put(`${BASE}/${id}/approve`, data)
  },

  /** 驳回文档 */
  reject(id: number, data: RejectDocument): Promise<void> {
    return request.put(`${BASE}/${id}/reject`, data)
  },

  /** 获取版本历史 */
  getVersions(documentId: number): Promise<DocumentVersion[]> {
    return request.get(`${BASE}/${documentId}/versions`).then(r => r.data)
  },

}

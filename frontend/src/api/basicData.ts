import request from './request'
import type {
  PagedResult, PagedRequest,
  Product, CreateProduct,
  Bom, CreateBom,
  Process, CreateProcess,
  Routing, CreateRouting,
  InspectionStandard, CreateInspectionStandard,
  DefectCode, CreateDefectCode,
  Equipment, CreateEquipment,
  Tool, CreateTool,
  Supplier, CreateSupplier,
  Customer, CreateCustomer,
} from '@/types/basicData'

function crudApi<T, C>(basePath: string) {
  return {
    list(params?: PagedRequest): Promise<PagedResult<T>> {
      return request.get(basePath, { params }).then(r => r.data)
    },
    get(id: number): Promise<T> {
      return request.get(`${basePath}/${id}`).then(r => r.data)
    },
    create(data: C): Promise<T> {
      return request.post(basePath, data).then(r => r.data)
    },
    update(id: number, data: C & { isActive?: boolean }): Promise<T> {
      return request.put(`${basePath}/${id}`, data).then(r => r.data)
    },
    delete(id: number): Promise<void> {
      return request.delete(`${basePath}/${id}`)
    },
  }
}

export const productApi = crudApi<Product, CreateProduct>('/products')
export const bomApi = crudApi<Bom, CreateBom>('/boms')
export const processApi = crudApi<Process, CreateProcess>('/processes')
export const routingApi = crudApi<Routing, CreateRouting>('/routings')
export const inspectionStandardApi = crudApi<InspectionStandard, CreateInspectionStandard>('/inspection-standards')
export const defectCodeApi = {
  list(params?: PagedRequest & { defectTypes?: string[]; severities?: string[] }): Promise<PagedResult<DefectCode>> {
    return request.get('/defect-codes', { params }).then(r => r.data)
  },
  get(id: number): Promise<DefectCode> {
    return request.get(`/defect-codes/${id}`).then(r => r.data)
  },
  create(data: CreateDefectCode): Promise<DefectCode> {
    return request.post('/defect-codes', data).then(r => r.data)
  },
  update(id: number, data: CreateDefectCode & { isActive?: boolean }): Promise<DefectCode> {
    return request.put(`/defect-codes/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`/defect-codes/${id}`)
  },
}
export const equipmentApi = crudApi<Equipment, CreateEquipment>('/equipment')
export const toolApi = crudApi<Tool, CreateTool>('/tools')
export const supplierApi = crudApi<Supplier, CreateSupplier>('/suppliers')
export const customerApi = crudApi<Customer, CreateCustomer>('/customers')

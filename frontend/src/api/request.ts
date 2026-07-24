import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/authStore'
import { ElMessage } from 'element-plus'

const request = axios.create({
  baseURL: '/api/v1',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (value: unknown) => void
  reject: (reason: unknown) => void
}> = []

function processQueue(error: unknown, token: string | null) {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

// ─── User-friendly error messages by HTTP status ─────────
const STATUS_MESSAGES: Record<number, string> = {
  400: '请求参数错误，请检查后重试',
  401: '登录已过期，请重新登录',
  403: '您没有权限执行此操作',
  404: '请求的资源不存在',
  408: '请求超时，请稍后重试',
  409: '资源冲突，请检查后重试',
  422: '数据验证失败，请检查输入',
  429: '请求过于频繁，请稍后再试',
  500: '服务器内部错误，请联系管理员',
  502: '网关错误，请稍后重试',
  503: '服务暂时不可用，请稍后重试',
  504: '网关超时，请稍后重试',
}

/**
 * Extract error message from various backend error response formats.
 * 
 * Supported formats:
 * - { message: string, detail?: string }
 * - { title?: string, detail?: string, status?: number }  (ProblemDetails)
 * - { errors: string[] }  (validation errors)
 * - string (plain text)
 */
function getErrorMessage(error: AxiosError): string {
  // 1. Try to extract server-side message from various formats
  const data = error.response?.data
  
  if (data) {
    // Format: { message: "xxx" }
    if (typeof data === 'string') return data
    
    if (typeof data === 'object') {
      const obj = data as Record<string, unknown>
      
      // Format: { message: "xxx" }
      if (typeof obj.message === 'string') {
        return obj.message
      }
      
      // Format: { title: "Bad Request", detail: "xxx" } (ProblemDetails)
      if (typeof obj.detail === 'string' && obj.detail) {
        return obj.detail
      }
      if (typeof obj.title === 'string' && obj.title) {
        const status = obj.status as number | undefined
        if (status && STATUS_MESSAGES[status]) {
          return `${STATUS_MESSAGES[status]}`
        }
        return obj.title
      }
      
      // Format: { errors: ["msg1", "msg2"] } (validation errors)
      if (Array.isArray(obj.errors) && obj.errors.length > 0) {
        return obj.errors[0]
      }
      
      // Format: { invalidParams: { paramName: "msg" } }
      const invalidParams = obj.invalidParams as Record<string, string> | undefined
      if (invalidParams) {
        const firstKey = Object.keys(invalidParams)[0]
        if (firstKey && invalidParams[firstKey]) {
          return `${firstKey}: ${invalidParams[firstKey]}`
        }
      }
    }
  }

  // 2. Fall back to HTTP status message
  const status = error.response?.status
  if (status && STATUS_MESSAGES[status]) {
    return STATUS_MESSAGES[status]
  }

  // 3. Network error or unknown
  if (!error.response) {
    return '网络连接异常，请检查网络设置'
  }
  
  return error.message || '请求失败，请稍后重试'
}

request.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const authStore = useAuthStore()
    if (authStore.token) {
      config.headers.Authorization = `Bearer ${authStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

request.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean
      _silent?: boolean
    }

    if (!originalRequest) {
      return Promise.reject(error)
    }

    // ─── Token refresh flow ──────────────────────────────
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((token) => {
          if (token) {
            originalRequest.headers.Authorization = `Bearer ${token}`
          }
          return request(originalRequest)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      const authStore = useAuthStore()
      try {
        const newToken = await authStore.refreshTokenAction()
        processQueue(null, newToken)
        originalRequest.headers.Authorization = `Bearer ${newToken}`
        return request(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        authStore.logout(() => {
          window.location.href = '/login'
        })
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    // ─── Show user-friendly error ────────────────────────
    const msg = getErrorMessage(error)
    if (!originalRequest._silent) {
      // Use ElMessage.error for 4xx/5xx, silent for 401 (already handled)
      if (error.response?.status && error.response.status >= 400) {
        ElMessage.error(msg)
      }
    }
    return Promise.reject(error)
  },
)

export default request

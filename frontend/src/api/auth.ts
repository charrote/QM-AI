import request from './request'
import type { LoginRequest, LoginResponse, UserInfo } from '@/types/user'

export const authApi = {
  login(data: LoginRequest): Promise<LoginResponse> {
    return request.post('/auth/login', data).then(res => res.data)
  },

  logout(): Promise<void> {
    return request.post('/auth/logout', undefined, { headers: { 'Content-Type': 'text/plain' } }).then(res => res.data)
  },

  refresh(refreshToken: string): Promise<LoginResponse> {
    return request.post('/auth/refresh', { refreshToken }).then(res => res.data)
  },

  getMe(): Promise<UserInfo> {
    return request.get('/auth/me').then(res => res.data)
  },
}

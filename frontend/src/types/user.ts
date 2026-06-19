export interface UserInfo {
  id: number
  username: string
  displayName: string
  avatar: string
  email: string
  role: string
}

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: UserInfo
}

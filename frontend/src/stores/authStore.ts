import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserInfo } from '@/types/user'
import { authApi } from '@/api/auth'

const TOKEN_KEY = 'qm-ai-token'
const REFRESH_KEY = 'qm-ai-refresh-token'
const USER_KEY = 'qm-ai-user'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>('')
  const refreshToken = ref<string>('')
  const user = ref<UserInfo | null>(null)
  const isAuthenticated = computed(() => !!token.value && !!user.value)

  // 外部注入的路由实例（由 App.vue 或 router 设置）
  let onSessionExpired: (() => void) | null = null

  function setOnSessionExpired(handler: () => void) {
    onSessionExpired = handler
  }

  function initFromStorage() {
    const savedToken = localStorage.getItem(TOKEN_KEY)
    const savedRefresh = localStorage.getItem(REFRESH_KEY)
    const savedUser = localStorage.getItem(USER_KEY)
    if (savedToken && savedUser) {
      token.value = savedToken
      refreshToken.value = savedRefresh || ''
      try {
        user.value = JSON.parse(savedUser)
      } catch {
        clearAuth()
      }
    }
  }

  function persistAuth() {
    if (token.value) {
      localStorage.setItem(TOKEN_KEY, token.value)
      localStorage.setItem(REFRESH_KEY, refreshToken.value)
      localStorage.setItem(USER_KEY, JSON.stringify(user.value))
    } else {
      clearAuth()
    }
  }

  function clearAuth() {
    token.value = ''
    refreshToken.value = ''
    user.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_KEY)
    localStorage.removeItem(USER_KEY)
  }

  async function login(username: string, password: string) {
    const res = await authApi.login({ username, password })
    token.value = res.token
    refreshToken.value = res.refreshToken
    user.value = res.user
    persistAuth()
  }

  async function logout(onRedirect?: () => void) {
    try {
      await authApi.logout()
    } catch {
      // ignore
    }
    clearAuth()
    if (onRedirect) {
      onRedirect()
    } else {
      window.location.href = '/login'
    }
  }

  async function refreshTokenAction() {
    try {
      const res = await authApi.refresh(refreshToken.value)
      token.value = res.token
      refreshToken.value = res.refreshToken
      persistAuth()
      return res.token
    } catch {
      clearAuth()
      if (onSessionExpired) {
        onSessionExpired()
      }
      throw new Error('Session expired')
    }
  }

  return {
    token,
    refreshToken,
    user,
    isAuthenticated,
    initFromStorage,
    login,
    logout,
    refreshTokenAction,
    setOnSessionExpired,
  }
})

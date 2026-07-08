import { defineStore } from 'pinia'
import { ref, watch } from 'vue'

const SIDEBAR_KEY = 'qm-ai-sidebar-collapsed'
const THEME_KEY = 'qm-ai-theme'

export const useAppStore = defineStore('app', () => {
  const sidebarCollapsed = ref(localStorage.getItem(SIDEBAR_KEY) === 'true')

  const theme = ref<'light' | 'dark'>((localStorage.getItem(THEME_KEY) as 'light' | 'dark') || 'light')

  const activeAlertCount = ref<number>(0)

  function updateAlertCount(count: number) {
    activeAlertCount.value = count
  }

  function toggleSidebar() {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  function toggleTheme() {
    theme.value = theme.value === 'light' ? 'dark' : 'light'
  }

  function applyTheme() {
    if (theme.value === 'dark') {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }

  watch(sidebarCollapsed, (val) => {
    localStorage.setItem(SIDEBAR_KEY, String(val))
  })

  watch(theme, (val) => {
    localStorage.setItem(THEME_KEY, val)
    applyTheme()
  })

  return {
    sidebarCollapsed,
    theme,
    activeAlertCount,
    toggleSidebar,
    toggleTheme,
    applyTheme,
    updateAlertCount,
  }
})

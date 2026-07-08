import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import type { TabItem } from '@/types/tab'
import { menuConfigs } from '@/config/menu.config'

const STORAGE_KEY = 'qm-ai-tabs'
const STORAGE_ACTIVE_KEY = 'qm-ai-active-tab'

let saveTimer: ReturnType<typeof setTimeout> | null = null

function findMenuByPath(path: string): TabItem | undefined {
  // 先查顶层菜单
  const top = menuConfigs.find(m => m.path === path)
  if (top) return top
  // 再查子菜单
  for (const menu of menuConfigs) {
    if (menu.children) {
      const child = menu.children.find(c => c.path === path)
      if (child) return child
    }
  }
  return undefined
}

export const useTabStore = defineStore('tab', () => {
  const tabs = ref<TabItem[]>([])
  const activeTabId = ref<string>('')
  const nextOrder = ref<number>(0)
  const refreshKey = ref<number>(0)

  const dashboardTab = computed(() => {
    const dt = menuConfigs.find(m => m.id === 'dashboard')
    if (dt) {
      return { ...dt, closable: false }
    }
    return null
  })

  const closableTabs = computed(() => tabs.value.filter(t => t.closable))

  const tabCount = computed(() => tabs.value.length)

  const needScrollArrows = computed(() => tabCount.value > 8)

  function initTabs() {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved) {
      try {
        const parsed: TabItem[] = JSON.parse(saved)
        tabs.value = parsed
        const maxOrder = parsed.reduce((max, t) => Math.max(max, t.order), 0)
        nextOrder.value = maxOrder + 1
      } catch {
        resetToDefault()
      }
    } else {
      resetToDefault()
    }

    const savedActive = localStorage.getItem(STORAGE_ACTIVE_KEY)
    if (savedActive && tabs.value.some(t => t.id === savedActive)) {
      activeTabId.value = savedActive
    } else if (tabs.value.length > 0) {
      activeTabId.value = tabs.value[0].id
    }
  }

  function resetToDefault() {
    const dashboard = menuConfigs.find(m => m.id === 'dashboard')
    if (dashboard) {
      tabs.value = [{ ...dashboard, closable: false }]
      activeTabId.value = 'dashboard'
      nextOrder.value = 1
    }
  }

  function addOrActivateTab(path: string) {
    const menu = findMenuByPath(path)
    if (!menu) return

    const exists = tabs.value.find(t => t.id === menu.id)
    if (exists) {
      activeTabId.value = menu.id
    } else {
      const newTab: TabItem = {
        ...menu,
        closable: menu.id !== 'dashboard',
        order: nextOrder.value++,
      }
      tabs.value.push(newTab)
      activeTabId.value = newTab.id
    }
  }

  function closeTab(tabId: string) {
    const tab = tabs.value.find(t => t.id === tabId)
    if (!tab || !tab.closable) return

    const idx = tabs.value.findIndex(t => t.id === tabId)
    tabs.value = tabs.value.filter(t => t.id !== tabId)

    if (activeTabId.value === tabId) {
      if (tabs.value.length === 0) {
        resetToDefault()
      } else if (idx < tabs.value.length) {
        activeTabId.value = tabs.value[idx].id
      } else {
        activeTabId.value = tabs.value[tabs.value.length - 1].id
      }
    }
  }

  function closeOtherTabs(tabId: string) {
    tabs.value = tabs.value.filter(t => t.id === tabId || !t.closable)
    activeTabId.value = tabId
  }

  function closeAllTabs() {
    tabs.value = tabs.value.filter(t => !t.closable)
    if (tabs.value.length > 0) {
      activeTabId.value = tabs.value[0].id
    }
  }

  function refreshTab() {
    refreshKey.value++
  }

  let saveTimer: ReturnType<typeof setTimeout> | null = null

  function saveTabs() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(tabs.value))
    localStorage.setItem(STORAGE_ACTIVE_KEY, activeTabId.value)
  }

  watch([tabs, activeTabId], () => {
    if (saveTimer) clearTimeout(saveTimer)
    saveTimer = setTimeout(saveTabs, 300)
  })

  watch([tabs, activeTabId], () => {
    if (saveTimer) clearTimeout(saveTimer)
    saveTimer = setTimeout(saveTabs, 300)
  })

  return {
    tabs,
    activeTabId,
    nextOrder,
    refreshKey,
    dashboardTab,
    closableTabs,
    tabCount,
    needScrollArrows,
    initTabs,
    addOrActivateTab,
    closeTab,
    closeOtherTabs,
    closeAllTabs,
    refreshTab,
    saveTabs,
  }
})

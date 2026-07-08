<script setup lang="ts">
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useTabStore } from '@/stores/tabStore'
import { useAuthStore } from '@/stores/authStore'
import { useAppStore } from '@/stores/appStore'
import { menuConfigs } from '@/config/menu.config'
import ContextMenu from '@/components/ContextMenu.vue'
import { useOrgStore } from '@/stores/orgStore'
import * as Icons from '@element-plus/icons-vue'
import type { TabItem } from '@/types/tab'

const router = useRouter()
const route = useRoute()
const tabStore = useTabStore()
const authStore = useAuthStore()
const appStore = useAppStore()
const orgStore = useOrgStore()

const tabContainerRef = ref<HTMLElement>()
const showScrollLeft = ref(false)
const showScrollRight = ref(false)

const contextMenuVisible = ref(false)
const contextMenuX = ref(0)
const contextMenuY = ref(0)
const contextMenuTabId = ref('')
const contextMenuClosable = ref(false)

// Refresh mechanism: toggle this to force KeepAlive to remount current component
const keepAliveEnabled = ref(true)

// 计算需要默认展开的父菜单
const defaultOpenedMenus = computed(() => {
  for (const menu of menuConfigs) {
    if (menu.children && menu.children.some(c => c.path === route.path)) {
      return [menu.path]
    }
  }
  return []
})

const componentNameMap: Record<string, string> = {
  dashboard: 'Dashboard',
  'basic-product': 'BasicData',
  'basic-process': 'BasicData',
  'basic-routing': 'BasicData',
  'basic-bom': 'BasicData',
  'basic-standard': 'BasicData',
  'basic-defect': 'BasicData',
  'basic-equipment': 'BasicData',
  'basic-tool': 'BasicData',
  'basic-supplier': 'BasicData',
  'basic-customer': 'BasicData',
  'basic-inspection-items': 'InspectionItemsPage',
  'basic-inspection-plans': 'InspectionPlansPage',
  'iqc-params': 'IqcParams',
  'iqc-receipts': 'IqcReceiptsPage',
  'iqc-inspections': 'IqcInspectionsPage',
  'iqc-anomalies': 'IqcAnomaliesPage',
  'iqc-suppliers': 'IqcSuppliersPage',
  'iqc-trace': 'IqcTracePage',
  'ipqc-first-pieces': 'IpqcFirstPiecesPage',
  'ipqc-patrols': 'IpqcPatrolsPage',
  'ipqc-plans': 'IpqcPlansPage',
  'ipqc-risk': 'IpqcRiskDashboard',
  'fqc-inspections': 'FqcInspectionsPage',
  'fqc-batches': 'FqcBatchesPage',
  'fqc-releases': 'FqcOqcReleasesPage',
  'fqc-packaging': 'FqcPackagingPage',
  spc: 'SPC',
  defects: 'Defects',
  'capa-page': 'CapaPage',
  'scrap-rework-page': 'ScrapReworkPage',
  'trace-page': 'TracePage',
  'ng-diffusion-page': 'NgDiffusionPage',
  'recall-simulation-page': 'RecallSimulationPage',
  complaints: 'Complaints',
  'complaint-list': 'ComplaintListPage',
  'd8-report': 'D8ReportPage',
  'complaint-timeline': 'ComplaintTimelinePage',
   ai: 'AI',
   'alert-center': 'AlertCenterPage',
   'root-cause': 'RootCauseAnalysisPage',
   'model-management': 'ModelManagementPage',
   'equipment-link': 'EquipmentLink',
   'param-mapping': 'ParamMappingPage',
   'status-history': 'StatusHistoryPage',
   'quality-correlation': 'QualityCorrelationPage',
  documents: 'Documents',
  'document-list': 'DocumentListPage',
  'version-history': 'VersionHistoryPage',
  audits: 'Audits',
  'audit-list': 'AuditListPage',
  'audit-detail': 'AuditDetailPage',
  finding: 'FindingPage',
  reports: 'Reports',
  'quality-dashboard': 'QualityDashboardPage',
  'report-builder': 'ReportBuilderPage',
  'export-center': 'ExportCenterPage',
  organizations: 'OrganizationPage',
  settings: 'Settings',
}

const keepAliveIncludes = computed(() => {
  return tabStore.tabs.map(t => componentNameMap[t.id]).filter(Boolean)
})

const sidebarWidth = computed(() => (appStore.sidebarCollapsed ? '64px' : '220px'))

const sortedTabs = computed(() => {
  return [...tabStore.tabs].sort((a, b) => a.order - b.order)
})

const currentRouteTabId = computed(() => {
  const path = route.path
  // 查顶层
  const top = menuConfigs.find(m => m.path === path)
  if (top) return top.id
  // 查子菜单
  for (const menu of menuConfigs) {
    if (menu.children) {
      const child = menu.children.find(c => c.path === path)
      if (child) return child.id
    }
  }
  return ''
})

onMounted(() => {
  tabStore.initTabs()
  if (route.path && route.path !== '/login') {
    tabStore.addOrActivateTab(route.path)
  }
  checkScrollArrows()
  // 加载组织树（供组织选择器使用），仅在已登录时
  if (authStore.isAuthenticated) {
    orgStore.loadOrgTree()
  }
})

watch(() => route.path, (newPath) => {
  if (newPath && newPath !== '/login') {
    tabStore.addOrActivateTab(newPath)
    nextTick(() => {
      scrollTabIntoView(newPath)
      checkScrollArrows()
    })
  }
})

function scrollTabIntoView(tabPath: string) {
  const container = tabContainerRef.value
  if (!container) return
  const activeEl = container.querySelector(`[data-tab-path="${tabPath}"]`) as HTMLElement
  if (activeEl) {
    activeEl.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' })
  }
}

function checkScrollArrows() {
  const container = tabContainerRef.value
  if (!container) return
  showScrollLeft.value = container.scrollLeft > 0
  showScrollRight.value = container.scrollLeft + container.clientWidth < container.scrollWidth
}

function scrollLeft() {
  tabContainerRef.value?.scrollBy({ left: -200, behavior: 'smooth' })
  setTimeout(checkScrollArrows, 150)
}

function scrollRight() {
  tabContainerRef.value?.scrollBy({ left: 200, behavior: 'smooth' })
  setTimeout(checkScrollArrows, 150)
}

function activateTab(tab: TabItem) {
  if (tab.path !== route.path) {
    router.push(tab.path)
  }
}

function closeTab(tabId: string) {
  tabStore.closeTab(tabId)
  const stillOpen = tabStore.tabs.find(t => t.id === tabId)
  if (!stillOpen && tabStore.tabs.length > 0) {
    const lastTab = tabStore.tabs[tabStore.tabs.length - 1]
    router.push(lastTab.path)
  }
}

function handleContextMenu(e: MouseEvent, tab: TabItem) {
  e.preventDefault()
  contextMenuX.value = e.clientX
  contextMenuY.value = e.clientY
  contextMenuTabId.value = tab.id
  contextMenuClosable.value = tab.closable
  contextMenuVisible.value = true
}

function closeContextMenu() {
  contextMenuVisible.value = false
}

function handleCloseOthers(tabId: string) {
  tabStore.closeOtherTabs(tabId)
  const tab = tabStore.tabs.find(t => t.id === tabId)
  if (tab && tab.path !== route.path) {
    router.push(tab.path)
  }
  closeContextMenu()
}

function handleCloseAll() {
  const dashboardTab = tabStore.dashboardTab
  tabStore.closeAllTabs()
  if (dashboardTab && dashboardTab.path !== route.path) {
    router.push(dashboardTab.path)
  }
  closeContextMenu()
}

function handleRefresh() {
  // Toggle KeepAlive off then on to force current component remount
  keepAliveEnabled.value = false
  nextTick(() => {
    keepAliveEnabled.value = true
  })
  closeContextMenu()
}

function handleMenuSelect(index: string) {
  if (index !== route.path) {
    router.push(index)
  }
}

function handleLogout() {
  authStore.logout(() => {
    router.push('/login')
  })
}

// ─── 组织选择器辅助 ──────────────────────────────────────
const LEVEL_PADDING: Record<string, number> = {
  group: 0, company: 12, workshop: 24, line: 36,
}

const LEVEL_COLORS: Record<string, string> = {
  group: '#8B5CF6', company: '#409EFF', workshop: '#E6A23C', line: '#67C23A',
}

function getLevelPadding(level: string): number {
  return LEVEL_PADDING[level] || 0
}

function getOrgLevelColor(level: string): string {
  return LEVEL_COLORS[level] || '#909399'
}

function getIconComponent(iconName?: string) {
  if (!iconName) return null
  return (Icons as Record<string, any>)[iconName]
}
</script>

<template>
  <el-container class="layout-container">
    <el-header class="layout-header">
      <div class="header-left">
        <div class="collapse-btn" @click="appStore.toggleSidebar()">
          <el-icon :size="20">
            <Fold v-if="!appStore.sidebarCollapsed" />
            <Expand v-else />
          </el-icon>
        </div>
        <div class="logo-area">
          <el-icon :size="24" color="#409eff"><Monitor /></el-icon>
          <span class="logo-text" v-show="!appStore.sidebarCollapsed">QM-AI</span>
        </div>
      </div>
      <div class="header-right">
        <!-- 组织选择器 -->
        <el-dropdown
          trigger="click"
          @visible-change="(v: boolean) => { if (v) orgStore.loadOrgTree() }"
          class="org-selector"
        >
          <span class="org-selector-trigger">
            <el-icon :size="16"><Connection /></el-icon>
            <span class="org-name">{{ orgStore.selectedOrgName || '全企业' }}</span>
            <el-icon :size="12"><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu class="org-dropdown-menu">
              <el-dropdown-item @click="orgStore.clearSelection()">
                <el-icon><RefreshLeft /></el-icon>
                全企业（不限）
              </el-dropdown-item>
              <el-dropdown-item divided disabled style="font-size: 12px; color: #909399; cursor: default;">
                选择组织
              </el-dropdown-item>
              <template v-for="org in orgStore.orgList" :key="org.id">
                <el-dropdown-item
                  @click="orgStore.selectOrg(org.id, org.name)"
                  :class="{ 'is-active': orgStore.selectedOrgId === org.id }"
                >
                  <span :style="{ paddingLeft: getLevelPadding(org.level) + 'px' }">
                    <el-tag
                      :color="getOrgLevelColor(org.level)"
                      size="small"
                      effect="dark"
                      style="margin-right: 4px;"
                    >
                      {{ org.levelLabel }}
                    </el-tag>
                    {{ org.name }}
                  </span>
                </el-dropdown-item>
              </template>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <div class="theme-toggle" @click="appStore.toggleTheme()">
          <el-icon :size="18">
            <Moon v-if="appStore.theme === 'light'" />
            <Sunny v-else />
          </el-icon>
        </div>
        <el-badge :value="appStore.activeAlertCount" :hidden="appStore.activeAlertCount === 0" class="alert-badge">
          <el-icon :size="20"><Bell /></el-icon>
        </el-badge>
        <el-dropdown trigger="click" @command="handleLogout">
          <span class="user-info">
            <el-avatar :size="28">
              <el-icon :size="16"><UserFilled /></el-icon>
            </el-avatar>
            <span class="user-name" v-show="!appStore.sidebarCollapsed">
              {{ authStore.user?.displayName || authStore.user?.username || '用户' }}
            </span>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="logout">
                <el-icon><SwitchButton /></el-icon>
                退出登录
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </el-header>

    <el-container class="layout-body">
      <el-aside :width="sidebarWidth" class="layout-sidebar">
        <el-menu
          :default-active="route.path"
          :default-openeds="defaultOpenedMenus"
          :collapse="appStore.sidebarCollapsed"
          :collapse-transition="false"
          :router="false"
          unique-opened
          class="sidebar-menu"
          @select="handleMenuSelect"
        >
          <template v-for="item in menuConfigs" :key="item.id">
            <!-- 有子菜单 -->
            <el-sub-menu v-if="item.children && item.children.length > 0" :index="item.path">
              <template #title>
                <el-icon>
                  <component :is="getIconComponent(item.icon)" />
                </el-icon>
                <span>{{ item.name }}</span>
              </template>
              <el-menu-item
                v-for="child in item.children"
                :key="child.id"
                :index="child.path"
              >
                <el-icon>
                  <component :is="getIconComponent(child.icon)" />
                </el-icon>
                <template #title>
                  <span>{{ child.name }}</span>
                </template>
              </el-menu-item>
            </el-sub-menu>
            <!-- 无子菜单 -->
            <el-menu-item v-else :index="item.path">
              <el-icon>
                <component :is="getIconComponent(item.icon)" />
              </el-icon>
              <template #title>
                <span>{{ item.name }}</span>
              </template>
            </el-menu-item>
          </template>
        </el-menu>
      </el-aside>

      <el-container class="layout-content-area">
        <div class="tab-bar" v-if="sortedTabs.length > 0">
          <div
            v-if="showScrollLeft"
            class="tab-scroll-btn left"
            @click="scrollLeft"
          >
            <el-icon><ArrowLeft /></el-icon>
          </div>
          <div
            ref="tabContainerRef"
            class="tab-container"
            @scroll.passive="checkScrollArrows"
          >
            <div
              v-for="tab in sortedTabs"
              :key="tab.id"
              :data-tab-path="tab.path"
              :class="[
                'tab-item',
                { active: tab.id === tabStore.activeTabId },
              ]"
              @click="activateTab(tab)"
              @contextmenu="handleContextMenu($event, tab)"
            >
              <el-icon class="tab-icon" :size="14">
                <component :is="getIconComponent(tab.icon)" />
              </el-icon>
              <span class="tab-name">{{ tab.name }}</span>
              <el-icon
                v-if="tab.closable"
                class="tab-close"
                :size="12"
                @click.stop="closeTab(tab.id)"
              >
                <Close />
              </el-icon>
            </div>
          </div>
          <div
            v-if="showScrollRight"
            class="tab-scroll-btn right"
            @click="scrollRight"
          >
            <el-icon><ArrowRight /></el-icon>
          </div>
        </div>

        <el-main class="content-area">
          <RouterView v-slot="{ Component }">
            <KeepAlive :include="keepAliveIncludes" v-if="keepAliveEnabled">
              <component :is="Component" />
            </KeepAlive>
          </RouterView>
        </el-main>
      </el-container>
    </el-container>

    <ContextMenu
      :visible="contextMenuVisible"
      :x="contextMenuX"
      :y="contextMenuY"
      :tab-id="contextMenuTabId"
      :closable="contextMenuClosable"
      @close="closeContextMenu"
      @close-others="handleCloseOthers"
      @close-all="handleCloseAll"
      @refresh="handleRefresh"
    />
  </el-container>
</template>

<style scoped>
.layout-container {
  height: 100vh;
  overflow: hidden;
}

.layout-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 48px;
  padding: 0 16px;
  background: var(--bg-header, #ffffff);
  border-bottom: 1px solid var(--border-color, #e4e7ed);
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
  z-index: 100;
  box-sizing: border-box;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.collapse-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  color: var(--text-regular, #606266);
  transition: background var(--transition-fast, 0.2s);
}

.collapse-btn:hover {
  background: var(--tab-item-hover-bg, #ecf5ff);
  color: var(--primary-color, #409eff);
}

.logo-area {
  display: flex;
  align-items: center;
  gap: 8px;
}

.logo-text {
  font-size: 18px;
  font-weight: 700;
  color: var(--text-primary, #303133);
  white-space: nowrap;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.theme-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 6px;
  cursor: pointer;
  color: var(--text-regular, #606266);
  transition: background var(--transition-fast, 0.2s);
}

.theme-toggle:hover {
  background: var(--tab-item-hover-bg, #ecf5ff);
  color: var(--primary-color, #409eff);
}

.alert-badge {
  display: flex;
  align-items: center;
  cursor: pointer;
  color: var(--text-regular, #606266);
}

.alert-badge:hover {
  color: var(--primary-color, #409eff);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 2px 8px;
  border-radius: 6px;
  transition: background var(--transition-fast, 0.2s);
}

.user-info:hover {
  background: var(--tab-item-hover-bg, #ecf5ff);
}

.user-name {
  font-size: 14px;
  color: var(--text-primary, #303133);
}

.layout-body {
  height: calc(100vh - 48px);
}

.layout-sidebar {
  background: var(--bg-sidebar, #304156);
  overflow-y: auto;
  overflow-x: hidden;
  transition: width var(--transition-normal, 0.3s);
  border-right: 1px solid var(--border-color, #e4e7ed);
}

.sidebar-menu {
  border-right: none;
  background: transparent;
}

.sidebar-menu:not(.el-menu--collapse) {
  width: 220px;
}

:deep(.sidebar-menu .el-menu-item),
:deep(.sidebar-menu .el-sub-menu__title) {
  color: var(--sidebar-text, #bfcbd9);
  background: transparent;
  height: 44px;
  line-height: 44px;
}

:deep(.sidebar-menu .el-menu-item:hover),
:deep(.sidebar-menu .el-sub-menu__title:hover) {
  background: rgba(255, 255, 255, 0.08);
}

:deep(.sidebar-menu .el-menu-item.is-active) {
  color: var(--sidebar-text-active, #ffffff);
  background: var(--sidebar-bg-active, #409eff);
}

/* 子菜单容器 — 与侧边栏同色系，略亮一点以体现层级 */
:deep(.sidebar-menu .el-sub-menu .el-menu) {
  background: rgba(0, 0, 0, 0.15);
}

/* 子菜单缩进 */
:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item) {
  padding-left: 48px !important;
}

/* 子菜单悬停 */
:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item:hover) {
  background: rgba(255, 255, 255, 0.1);
}

/* 子菜单激活 */
:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item.is-active) {
  color: var(--sidebar-text-active, #ffffff);
  background: var(--sidebar-bg-active, #409eff);
}

/* 展开箭头颜色 */
:deep(.sidebar-menu .el-sub-menu__title .el-sub-menu__icon-arrow) {
  color: var(--sidebar-text, #bfcbd9);
}

.layout-content-area {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.tab-bar {
  display: flex;
  align-items: center;
  height: 36px;
  background: var(--bg-tab-bar, #ffffff);
  border-bottom: 1px solid var(--tab-bar-border, #e4e7ed);
  box-shadow: var(--shadow-sm, 0 1px 2px rgba(0,0,0,0.06));
  position: relative;
  flex-shrink: 0;
}

.tab-scroll-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 100%;
  flex-shrink: 0;
  cursor: pointer;
  color: var(--text-secondary, #909399);
  z-index: 2;
  background: var(--bg-tab-bar, #ffffff);
  border: 1px solid var(--tab-bar-border, #e4e7ed);
}

.tab-scroll-btn:hover {
  color: var(--primary-color, #409eff);
  background: var(--tab-item-hover-bg, #ecf5ff);
}

.tab-container {
  display: flex;
  align-items: center;
  flex: 1;
  overflow-x: auto;
  overflow-y: hidden;
  scrollbar-width: none;
  gap: 2px;
  padding: 0 4px;
  height: 100%;
}

.tab-container::-webkit-scrollbar {
  display: none;
}

.tab-item {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 0 12px;
  height: 28px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 13px;
  color: var(--tab-item-color, #606266);
  background: var(--tab-item-bg, #ffffff);
  border: 1px solid transparent;
  white-space: nowrap;
  flex-shrink: 0;
  transition: all var(--transition-fast, 0.2s);
  user-select: none;
}

.tab-item:hover {
  background: var(--tab-item-hover-bg, #ecf5ff);
}

.tab-item.active {
  color: var(--tab-item-active-color, #409eff);
  background: var(--tab-item-active-bg, #e8f0fe);
  border-color: var(--primary-color, #409eff);
  border-bottom: 2px solid var(--primary-color, #409eff);
  font-weight: 600;
}

.tab-icon {
  display: flex;
  align-items: center;
}

.tab-name {
  font-size: 13px;
}

.tab-close {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  border-radius: 3px;
  margin-left: 2px;
  transition: all var(--transition-fast, 0.2s);
  color: var(--text-secondary, #909399);
}

.tab-close:hover {
  background: rgba(0, 0, 0, 0.1);
  color: var(--text-primary, #303133);
}

.content-area {
  background: var(--bg-content, #f0f2f5);
  overflow-y: auto;
  padding: 16px;
}

/* ── 组织选择器 ── */
.org-selector {
  cursor: pointer;
  margin: 0 8px;
}

.org-selector-trigger {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 6px;
  transition: background var(--transition-fast, 0.2s);
  color: var(--text-regular, #606266);
  font-size: 13px;
}

.org-selector-trigger:hover {
  background: var(--tab-item-hover-bg, #ecf5ff);
  color: var(--primary-color, #409eff);
}

.org-name {
  max-width: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.org-dropdown-menu .is-active {
  background: #ecf5ff;
  color: #409eff;
  font-weight: 600;
}
</style>

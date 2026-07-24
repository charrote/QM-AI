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
const showSearchPanel = ref(false)

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

const sidebarWidth = computed(() => (appStore.sidebarCollapsed ? '64px' : '224px'))

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
const orgSearch = ref('')
const filteredOrgs = computed(() => {
  if (!orgSearch.value) return orgStore.orgList
  const q = orgSearch.value.toLowerCase()
  return orgStore.orgList.filter(o =>
    o.name.toLowerCase().includes(q) ||
    (o.code && o.code.toLowerCase().includes(q)) ||
    (o.path && o.path.toLowerCase().includes(q))
  )
})

const LEVEL_PADDING: Record<string, number> = {
  group: 0, company: 12, workshop: 24, line: 36,
}

const LEVEL_COLORS: Record<string, string> = {
  group: '#8B5CF6', company: '#1677ff', workshop: '#faad14', line: '#52c41a',
}

function filterOrgList() {
  // reactive via computed
}

function selectOrgItem(org: { id: number; name: string }) {
  orgStore.selectOrg(org.id, org.name)
  orgSearch.value = ''
}

function getLevelPadding(level: string): number {
  return LEVEL_PADDING[level] || 0
}

function getOrgLevelColor(level: string): string {
  return LEVEL_COLORS[level] || '#909399'
}

function getOrgLevelIcon(level: string): any {
  const iconMap: Record<string, any> = {
    group: Icons.OfficeBuilding,
    company: Icons.School,
    workshop: Icons.Monitor,
    line: Icons.Tools,
  }
  return iconMap[level] || Icons.Connection
}

function getIconComponent(iconName?: string) {
  if (!iconName) return null
  return (Icons as Record<string, any>)[iconName]
}
</script>

<template>
  <el-container class="layout-container">
    <!-- ═══ Header ═══ -->
    <el-header class="layout-header">
      <div class="header-left">
        <div class="collapse-btn" @click="appStore.toggleSidebar()" title="折叠/展开菜单">
          <el-icon :size="18">
            <Fold v-if="!appStore.sidebarCollapsed" />
            <Expand v-else />
          </el-icon>
        </div>
        <div class="logo-area">
          <div class="logo-icon">
            <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M9 12L11 14L15 10M21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C16.9706 3 21 7.02944 21 12Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </div>
          <div class="logo-text" v-show="!appStore.sidebarCollapsed">
            <span class="logo-title">QM-AI</span>
            <span class="logo-subtitle">质量决策平台</span>
          </div>
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
            <el-icon :size="15"><Connection /></el-icon>
            <span class="org-name">{{ orgStore.selectedOrgName || '全企业' }}</span>
            <el-icon :size="10"><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu class="org-dropdown-menu">
              <el-dropdown-item @click="orgStore.clearSelection()" :class="{ 'is-active': !orgStore.selectedOrgId }">
                <el-icon><RefreshLeft /></el-icon>
                全企业
              </el-dropdown-item>

              <!-- 搜索框 -->
              <div class="org-search-box">
                <el-input
                  v-model="orgSearch"
                  placeholder="搜索组织名称..."
                  size="small"
                  clearable
                  prefix-icon="Search"
                  @input="filterOrgList"
                />
              </div>

              <el-dropdown-item divided disabled style="font-size: 11px; color: #909399; cursor: default; padding: 4px 16px;">
                {{ filteredOrgs.length }} 个组织
              </el-dropdown-item>

              <template v-for="org in filteredOrgs" :key="org.id">
                <el-dropdown-item
                  @click="selectOrgItem(org)"
                  :class="{ 'is-active': orgStore.selectedOrgId === org.id }"
                >
                  <span :style="{ paddingLeft: getLevelPadding(org.level) + 'px' }">
                    <el-icon :size="14" :class="['org-level-icon', org.level]">
                      <component :is="getOrgLevelIcon(org.level)" />
                    </el-icon>
                    {{ org.name }}
                  </span>
                  <span v-if="org.path" class="org-path">{{ org.path }}</span>
                </el-dropdown-item>
              </template>

              <el-dropdown-item v-if="filteredOrgs.length === 0" disabled style="padding: 16px; color: #909399; text-align: center;">
                无匹配结果
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <!-- 搜索按钮 -->
        <div class="header-icon-btn" @click="showSearchPanel = true" title="全局搜索">
          <el-icon :size="17"><Search /></el-icon>
        </div>

        <!-- 通知 -->
        <el-badge :value="appStore.activeAlertCount" :hidden="appStore.activeAlertCount === 0" :max="99">
          <div class="header-icon-btn alert-btn" title="预警通知">
            <el-icon :size="18"><Bell /></el-icon>
          </div>
        </el-badge>

        <!-- 主题切换 -->
        <div class="header-icon-btn" @click="appStore.toggleTheme()" :title="appStore.theme === 'light' ? '切换到暗色' : '切换到亮色'">
          <el-icon :size="17">
            <Moon v-if="appStore.theme === 'light'" />
            <Sunny v-else />
          </el-icon>
        </div>

        <!-- 全屏按钮 -->
        <div class="header-icon-btn" @click="toggleFullscreen" title="全屏">
          <el-icon :size="17">
            <FullScreen v-if="!isFullscreen" />
            <Bug v-else />
          </el-icon>
        </div>

        <el-divider direction="vertical" style="height: 20px; margin: 0 4px;" />

        <!-- 用户菜单 -->
        <el-dropdown trigger="click" @command="handleLogout">
          <span class="user-info">
            <el-avatar :size="30" class="user-avatar">
              <el-icon :size="16"><UserFilled /></el-icon>
            </el-avatar>
            <div class="user-detail" v-show="!appStore.sidebarCollapsed">
              <div class="user-name">{{ authStore.user?.displayName || authStore.user?.username || '用户' }}</div>
              <div class="user-role" v-if="authStore.user?.role">{{ authStore.user.role }}</div>
            </div>
            <el-icon :size="12"><ArrowDown /></el-icon>
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="profile">
                <el-icon><User /></el-icon>
                个人中心
              </el-dropdown-item>
              <el-dropdown-item command="settings">
                <el-icon><Setting /></el-icon>
                系统设置
              </el-dropdown-item>
              <el-dropdown-item divided command="logout">
                <el-icon><SwitchButton /></el-icon>
                退出登录
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </el-header>

    <!-- ═══ Main Body ═══ -->
    <el-container class="layout-body">
      <!-- Sidebar -->
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

      <!-- ═══ Content Area ═══ -->
      <el-container class="layout-content-area">
        <!-- TabBar -->
        <div class="tab-bar" v-if="sortedTabs.length > 0">
          <div
            v-if="showScrollLeft"
            class="tab-scroll-btn"
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
              :class="['tab-item', { active: tab.id === tabStore.activeTabId }]"
              @click="activateTab(tab)"
              @contextmenu="handleContextMenu($event, tab)"
            >
              <el-icon class="tab-icon" :size="13">
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
            class="tab-scroll-btn"
            @click="scrollRight"
          >
            <el-icon><ArrowRight /></el-icon>
          </div>
        </div>

        <!-- Main Content -->
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

<script lang="ts">
export default {
  computed: {
    isFullscreen(): boolean {
      return !!(document.fullscreenElement || (document as any).webkitFullscreenElement || (document as any).msFullscreenElement)
    }
  },
  methods: {
    toggleFullscreen() {
      if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen?.()
      } else {
        document.exitFullscreen?.()
      }
    }
  }
}
</script>

<style scoped>
/* ═══ Layout Container ═══ */
.layout-container {
  height: 100vh;
  overflow: hidden;
}

/* ═══ Header ═══ */
.layout-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: var(--layout-header-height);
  padding: 0 var(--space-4);
  background: var(--header-bg, #fff);
  border-bottom: 1px solid var(--header-border, #f0f0f0);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
  z-index: var(--z-fixed);
  box-sizing: border-box;
}

.header-left {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

.collapse-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--text-regular, #606266);
  transition: all var(--duration-fast) var(--ease-out);
  flex-shrink: 0;
}

.collapse-btn:hover {
  background: var(--tab-item-hover-bg, #f5f5f5);
  color: var(--primary-color, #4096ff);
}

/* Logo */
.logo-area {
  display: flex;
  align-items: center;
  gap: var(--space-3);
}

.logo-icon {
  width: 28px;
  height: 28px;
  color: var(--primary, #1677ff);
  flex-shrink: 0;
}

.logo-icon svg {
  width: 100%;
  height: 100%;
}

.logo-text {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}

.logo-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-primary, #1a1a1a);
  letter-spacing: 0.5px;
}

.logo-subtitle {
  font-size: 10px;
  color: var(--text-secondary, #8c8c8c);
  font-weight: 400;
}

/* Header Right */
.header-right {
  display: flex;
  align-items: center;
  gap: var(--space-2);
}

/* Icon Buttons */
.header-icon-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  border-radius: var(--radius-md);
  cursor: pointer;
  color: var(--text-regular, #606266);
  transition: all var(--duration-fast) var(--ease-out);
}

.header-icon-btn:hover {
  background: var(--tab-item-hover-bg, #f5f5f5);
  color: var(--primary, #1677ff);
}

.alert-btn:hover {
  color: var(--warning, #faad14);
}

/* Organization Selector */
.org-selector {
  cursor: pointer;
}

.org-selector-trigger {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 10px;
  border-radius: var(--radius-md);
  transition: all var(--duration-fast) var(--ease-out);
  color: var(--text-regular, #606266);
  font-size: var(--font-sm);
}

.org-selector-trigger:hover {
  background: var(--tab-item-hover-bg, #f5f5f5);
  color: var(--primary, #1677ff);
}

.org-name {
  max-width: 100px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* User Info */
.user-info {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  cursor: pointer;
  padding: 3px 8px 3px 3px;
  border-radius: var(--radius-lg);
  transition: all var(--duration-fast) var(--ease-out);
}

.user-info:hover {
  background: var(--tab-item-hover-bg, #f5f5f5);
}

.user-avatar {
  background: var(--primary-bg, #e6f4ff);
  color: var(--primary, #1677ff);
}

.user-detail {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}

.user-name {
  font-size: var(--font-sm);
  font-weight: var(--font-medium);
  color: var(--text-primary, #1a1a1a);
}

.user-role {
  font-size: 10px;
  color: var(--text-secondary, #8c8c8c);
}

/* ═══ Sidebar ═══ */
.layout-body {
  height: calc(100vh - var(--layout-header-height));
}

.layout-sidebar {
  background: var(--sidebar-bg, #001529);
  overflow-y: auto;
  overflow-x: hidden;
  transition: width var(--duration-normal) var(--ease-in-out);
  border-right: 1px solid rgba(255, 255, 255, 0.08);
  /* Custom scrollbar */
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 255, 255, 0.1) transparent;
}

.layout-sidebar::-webkit-scrollbar {
  width: 4px;
}

.layout-sidebar::-webkit-scrollbar-track {
  background: transparent;
}

.layout-sidebar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15);
  border-radius: 2px;
}

.sidebar-menu {
  border-right: none;
  background: transparent;
}

.sidebar-menu:not(.el-menu--collapse) {
  width: 224px;
}

/* Menu Items */
:deep(.sidebar-menu .el-menu-item),
:deep(.sidebar-menu .el-sub-menu > .el-sub-menu__title) {
  color: var(--sidebar-text, rgba(255, 255, 255, 0.65));
  background: transparent;
  height: 42px;
  line-height: 42px;
  border-radius: 0;
  margin: 0;
  padding: 0 0 0 20px !important;
  transition: all var(--duration-fast) var(--ease-out);
}

:deep(.sidebar-menu .el-menu-item .el-icon),
:deep(.sidebar-menu .el-sub-menu > .el-sub-menu__title .el-icon) {
  font-size: 17px;
  width: 20px;
  text-align: center;
  flex-shrink: 0;
  margin-right: 8px;
}

:deep(.sidebar-menu .el-menu-item .cell span),
:deep(.sidebar-menu .el-sub-menu > .el-sub-menu__title .cell span) {
  margin-left: 0;
}

:deep(.sidebar-menu .el-menu-item:hover),
:deep(.sidebar-menu .el-sub-menu > .el-sub-menu__title:hover) {
  background: var(--sidebar-hover, rgba(255, 255, 255, 0.06));
  color: rgba(255, 255, 255, 0.85);
}

:deep(.sidebar-menu .el-menu-item.is-active) {
  color: var(--sidebar-text-active, #fff);
  background: var(--sidebar-active-bg, #1677ff);
  border-radius: 0;
  border-right: 3px solid #fff;
}

/* Submenu */
:deep(.sidebar-menu .el-sub-menu .el-menu) {
  background: rgba(0, 0, 0, 0.2) !important;
  padding-left: 0;
}

:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item) {
  font-size: var(--font-sm);
  height: 38px;
  line-height: 38px;
  padding: 0 0 0 40px !important;
  margin: 0;
  border-radius: 0;
}

:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item .el-icon) {
  font-size: 16px;
  margin-right: 6px;
}

:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item:hover) {
  background: var(--sidebar-hover, rgba(255, 255, 255, 0.06));
}

:deep(.sidebar-menu .el-sub-menu .el-menu .el-menu-item.is-active) {
  color: var(--sidebar-text-active, #fff);
  background: var(--sidebar-active-bg, #1677ff);
  border-radius: 0;
  border-right: 3px solid #fff;
}

/* Arrow Icons */
:deep(.sidebar-menu .el-sub-menu__title .el-sub-menu__icon-arrow) {
  color: var(--sidebar-text, rgba(255, 255, 255, 0.65));
  font-size: 12px;
}

/* ═══ Content Area ═══ */
.layout-content-area {
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: var(--bg-base, #f5f7fa);
}

/* TabBar */
.tab-bar {
  display: flex;
  align-items: center;
  height: var(--layout-tabbar-height);
  background: var(--tabbar-bg, #fff);
  border-bottom: 1px solid var(--tabbar-border, #f0f0f0);
  position: relative;
  flex-shrink: 0;
  z-index: 1;
}

.tab-scroll-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 100%;
  flex-shrink: 0;
  cursor: pointer;
  color: var(--text-secondary, #8c8c8c);
  z-index: 2;
  background: var(--tabbar-bg, #fff);
  transition: all var(--duration-fast) var(--ease-out);
}

.tab-scroll-btn:hover {
  color: var(--primary, #1677ff);
  background: var(--tab-item-hover-bg, #f5f5f5);
}

.tab-container {
  display: flex;
  align-items: center;
  flex: 1;
  overflow-x: auto;
  overflow-y: hidden;
  gap: 2px;
  padding: 0 var(--space-3);
  height: 100%;
  scrollbar-width: none;
}

.tab-container::-webkit-scrollbar {
  display: none;
}

.tab-item {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 0 var(--space-3);
  height: 28px;
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--font-sm);
  color: var(--tab-item-text, #666);
  background: var(--tab-item-bg, transparent);
  border: 1px solid transparent;
  white-space: nowrap;
  flex-shrink: 0;
  transition: all var(--duration-fast) var(--ease-out);
  user-select: none;
}

.tab-item:hover {
  background: var(--tab-item-hover-bg, #f5f5f5);
  color: var(--text-primary, #1a1a1a);
}

.tab-item.active {
  color: var(--tab-item-active-text, #1677ff);
  background: var(--tab-item-active-bg, #fff);
  border-color: var(--tab-item-active-border, #1677ff);
  font-weight: var(--font-medium);
  box-shadow: 0 1px 3px rgba(22, 119, 255, 0.1);
}

.tab-icon {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.tab-name {
  font-size: var(--font-sm);
  line-height: 1;
}

.tab-close {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  border-radius: 3px;
  margin-left: -2px;
  transition: all var(--duration-fast) var(--ease-out);
  color: var(--text-secondary, #8c8c8c);
  opacity: 0;
}

.tab-item:hover .tab-close {
  opacity: 1;
}

.tab-close:hover {
  background: rgba(0, 0, 0, 0.08);
  color: var(--text-primary, #1a1a1a);
}

/* ═══ Content Area ═══ */
.content-area {
  background: var(--bg-base, #f5f7fa);
  overflow-y: auto;
  padding: var(--layout-content-padding);
  flex: 1;
  scrollbar-width: thin;
  scrollbar-color: rgba(0, 0, 0, 0.1) transparent;
}

.content-area::-webkit-scrollbar {
  width: 6px;
}

.content-area::-webkit-scrollbar-track {
  background: transparent;
}

.content-area::-webkit-scrollbar-thumb {
  background: rgba(0, 0, 0, 0.12);
  border-radius: 3px;
}

.content-area::-webkit-scrollbar-thumb:hover {
  background: rgba(0, 0, 0, 0.2);
}

/* Dark mode scrollbar */
html.dark .content-area::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15);
}

/* ═══ Dropdown Styles ═══ */
.org-dropdown-menu .is-active {
  background: var(--primary-bg, #f0f7ff);
  color: var(--primary, #1677ff);
  font-weight: var(--font-medium);
}

html.dark .org-dropdown-menu .is-active {
  background: rgba(22, 119, 255, 0.15);
}

/* Org search box */
.org-search-box {
  padding: var(--space-2) var(--space-3);
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.org-search-box .el-input__wrapper {
  box-shadow: 0 0 0 1px var(--el-border-color) inset;
  border-radius: var(--radius-md);
}

/* Org level icons */
.org-level-icon {
  flex-shrink: 0;
  margin-right: 6px;
}

.org-dropdown-menu .org-level-icon.group { color: #8B5CF6; }
.org-dropdown-menu .org-level-icon.company { color: #1677ff; }
.org-dropdown-menu .org-level-icon.workshop { color: #faad14; }
.org-dropdown-menu .org-level-icon.line { color: #52c41a; }

html.dark .org-dropdown-menu .org-level-icon.group { color: #a78bfa; }
html.dark .org-dropdown-menu .org-level-icon.company { color: #60a5fa; }
html.dark .org-dropdown-menu .org-level-icon.workshop { color: #fbbf24; }
html.dark .org-dropdown-menu .org-level-icon.line { color: #4ade80; }

/* Org path subtitle */
.org-path {
  font-size: var(--font-xs);
  color: var(--text-secondary, #8c8c8c);
  margin-left: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 120px;
  flex-shrink: 0;
}

html.dark .org-path {
  color: var(--text-secondary, #71717a);
}

/* ═══ Responsive ═══ */
@media (max-width: 1200px) {
  .user-detail {
    display: none;
  }
  
  .logo-subtitle {
    display: none;
  }
}
</style>

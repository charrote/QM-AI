import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useTabStore } from '@/stores/tabStore'

declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    keepAlive?: boolean
    requiresAuth?: boolean
  }
}

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { title: '登录', requiresAuth: false },
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    redirect: '/dashboard',
    meta: { requiresAuth: true },
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '首页', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data',
        redirect: '/basic-data/product',
        meta: { title: '基础数据', requiresAuth: true },
      },
      {
        path: 'basic-data/product',
        name: 'BasicDataProduct',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '产品管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/process',
        name: 'BasicDataProcess',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '工序管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/routing',
        name: 'BasicDataRouting',
        component: () => import('@/views/Routing.vue'),
        meta: { title: '产品工艺路线', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/bom',
        name: 'BasicDataBom',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: 'BOM清单', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/standard',
        name: 'BasicDataStandard',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '检验标准', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/defect',
        name: 'BasicDataDefect',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '不良代码', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/equipment',
        name: 'BasicDataEquipment',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '设备管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/tool',
        name: 'BasicDataTool',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '刀具管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/supplier',
        name: 'BasicDataSupplier',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '供应商管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/customer',
        name: 'BasicDataCustomer',
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '客户管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/inspection-items',
        name: 'InspectionItems',
        component: () => import('@/views/inspection/InspectionItemsPage.vue'),
        meta: { title: '检验项目管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'basic-data/inspection-plans',
        name: 'InspectionPlans',
        component: () => import('@/views/inspection/InspectionPlansPage.vue'),
        meta: { title: '检验计划管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'iqc',
        redirect: '/iqc/params',
        meta: { title: 'IQC来料检验', requiresAuth: true },
      },
      {
        path: 'iqc/params',
        name: 'IqcParams',
        component: () => import('@/views/iqc/IqcParams.vue'),
        meta: { title: '动态参数配置', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'iqc/receipts',
        name: 'IqcReceipts',
        component: () => import('@/views/iqc/IqcReceiptsPage.vue'),
        meta: { title: '来料登记', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'iqc/inspections',
        name: 'IqcInspections',
        component: () => import('@/views/iqc/IqcInspectionsPage.vue'),
        meta: { title: '检验单', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'iqc/anomalies',
        name: 'IqcAnomalies',
        component: () => import('@/views/iqc/IqcAnomaliesPage.vue'),
        meta: { title: '来料异常', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'iqc/suppliers',
        name: 'IqcSuppliers',
        component: () => import('@/views/iqc/IqcSuppliersPage.vue'),
        meta: { title: '供应商评分', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ipqc',
        redirect: '/ipqc/first-pieces',
        meta: { title: 'IPQC过程检验', requiresAuth: true },
      },
      {
        path: 'ipqc/first-pieces',
        name: 'IpqcFirstPieces',
        component: () => import('@/views/ipqc/IpqcFirstPiecesPage.vue'),
        meta: { title: '首件检验', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ipqc/patrols',
        name: 'IpqcPatrols',
        component: () => import('@/views/ipqc/IpqcPatrolsPage.vue'),
        meta: { title: '巡检记录', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ipqc/plans',
        name: 'IpqcPlans',
        component: () => import('@/views/ipqc/IpqcPlansPage.vue'),
        meta: { title: '巡检计划', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ipqc/risk',
        name: 'IpqcRisk',
        component: () => import('@/views/ipqc/IpqcRiskDashboard.vue'),
        meta: { title: 'AI风险仪表盘', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'fqc',
        redirect: '/fqc/inspections',
        meta: { title: 'FQC/OQC成品检验', requiresAuth: true },
      },
      {
        path: 'fqc/inspections',
        name: 'FqcInspections',
        component: () => import('@/views/fqc/FqcInspectionsPage.vue'),
        meta: { title: '成品检验', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'fqc/batches',
        name: 'FqcBatches',
        component: () => import('@/views/fqc/FqcBatchesPage.vue'),
        meta: { title: '批次管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'fqc/releases',
        name: 'FqcReleases',
        component: () => import('@/views/fqc/FqcOqcReleasesPage.vue'),
        meta: { title: '出货放行', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'fqc/packaging',
        name: 'FqcPackaging',
        component: () => import('@/views/fqc/FqcPackagingPage.vue'),
        meta: { title: '包装确认', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'spc',
        name: 'SPC',
        component: () => import('@/views/SPC.vue'),
        meta: { title: 'SPC统计分析', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'defects',
        name: 'Defects',
        component: () => import('@/views/defects/DefectsPage.vue'),
        meta: { title: '缺陷管理', requiresAuth: true },
      },
      {
        path: 'defects/capa',
        name: 'Capa',
        component: () => import('@/views/defects/CapaPage.vue'),
        meta: { title: 'CAPA 流程', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'defects/scrap-rework',
        name: 'ScrapRework',
        component: () => import('@/views/defects/ScrapReworkPage.vue'),
        meta: { title: '报废/返工', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'trace',
        name: 'Trace',
        component: () => import('@/views/trace/TracePage.vue'),
        meta: { title: '追溯查询', requiresAuth: true },
      },
      {
        path: 'trace/ng-diffusion',
        name: 'NgDiffusion',
        component: () => import('@/views/trace/NgDiffusionPage.vue'),
        meta: { title: 'NG 扩散分析', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'trace/recall-simulation',
        name: 'RecallSimulation',
        component: () => import('@/views/trace/RecallSimulationPage.vue'),
        meta: { title: '召回模拟', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'complaints',
        redirect: '/complaints/list',
        meta: { title: '客诉与8D', requiresAuth: true },
      },
      {
        path: 'complaints/list',
        name: 'ComplaintList',
        component: () => import('@/views/complaints/ComplaintListPage.vue'),
        meta: { title: '客诉列表', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'complaints/d8',
        name: 'D8Report',
        component: () => import('@/views/complaints/D8ReportPage.vue'),
        meta: { title: '8D 报告', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'complaints/timeline',
        name: 'ComplaintTimeline',
        component: () => import('@/views/complaints/ComplaintTimelinePage.vue'),
        meta: { title: '客诉时间线', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ai',
        redirect: '/ai/alerts',
        meta: { title: 'AI 质量分析', requiresAuth: true },
      },
      {
        path: 'ai/alerts',
        name: 'AlertCenter',
        component: () => import('@/views/ai/AlertCenterPage.vue'),
        meta: { title: '预警中心', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ai/root-cause',
        name: 'RootCauseAnalysis',
        component: () => import('@/views/ai/RootCauseAnalysisPage.vue'),
        meta: { title: '根因分析', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ai/models',
        name: 'ModelManagement',
        component: () => import('@/views/ai/ModelManagementPage.vue'),
        meta: { title: '模型管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'equipment-link',
        redirect: '/equipment-link/mapping',
        meta: { title: '设备质量联动', requiresAuth: true },
      },
      {
        path: 'equipment-link/mapping',
        name: 'ParamMapping',
        component: () => import('@/views/equipmentlink/ParamMappingPage.vue'),
        meta: { title: '参数映射', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'equipment-link/status',
        name: 'StatusHistory',
        component: () => import('@/views/equipmentlink/StatusHistoryPage.vue'),
        meta: { title: '状态历史', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'equipment-link/correlation',
        name: 'QualityCorrelation',
        component: () => import('@/views/equipmentlink/QualityCorrelationPage.vue'),
        meta: { title: '质量关联', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'documents',
        redirect: '/documents/list',
        meta: { title: '文件与体系', requiresAuth: true },
      },
      {
        path: 'documents/list',
        name: 'DocumentList',
        component: () => import('@/views/documents/DocumentListPage.vue'),
        meta: { title: '文档管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'documents/versions',
        name: 'VersionHistory',
        component: () => import('@/views/documents/VersionHistoryPage.vue'),
        meta: { title: '版本历史', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'audits',
        redirect: '/audits/list',
        meta: { title: '审核与稽核', requiresAuth: true },
      },
      {
        path: 'audits/list',
        name: 'AuditList',
        component: () => import('@/views/audits/AuditListPage.vue'),
        meta: { title: '审核列表', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'audits/detail',
        name: 'AuditDetail',
        component: () => import('@/views/audits/AuditDetailPage.vue'),
        meta: { title: '审核详情', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'audits/finding',
        name: 'Finding',
        component: () => import('@/views/audits/FindingPage.vue'),
        meta: { title: '不符合项管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'reports',
        redirect: '/reports/dashboard',
        meta: { title: '报表中心', requiresAuth: true },
      },
      {
        path: 'reports/dashboard',
        name: 'QualityDashboard',
        component: () => import('@/views/reports/QualityDashboardPage.vue'),
        meta: { title: '质量仪表盘', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'reports/builder',
        name: 'ReportBuilder',
        component: () => import('@/views/reports/ReportBuilderPage.vue'),
        meta: { title: '报表定制', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'reports/export',
        name: 'ExportCenter',
        component: () => import('@/views/reports/ExportCenterPage.vue'),
        meta: { title: '导出中心', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'organizations',
        name: 'Organizations',
        component: () => import('@/views/OrganizationPage.vue'),
        meta: { title: '企业层级', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'settings',
        name: 'Settings',
        component: () => import('@/views/Settings.vue'),
        meta: { title: '系统管理', keepAlive: true, requiresAuth: true },
      },
    ],
  },
  {
    path: '/pda/iqc',
    name: 'PdaIqc',
    component: () => import('@/views/iqc/PdaIqcScan.vue'),
    meta: { title: 'PDA扫码录入', requiresAuth: true },
  },
  {
    path: '/pda/ipqc',
    name: 'PdaIpqc',
    component: () => import('@/views/ipqc/PdaIpqcScan.vue'),
    meta: { title: 'PDA巡检执行', requiresAuth: true },
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/NotFound.vue'),
    meta: { title: '404', requiresAuth: false },
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

const BASE_TITLE = '工业AI质量决策平台'

router.afterEach((to) => {
  const pageTitle = to.meta.title as string | undefined
  document.title = pageTitle ? `${pageTitle} | ${BASE_TITLE}` : BASE_TITLE
})

router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth !== false && !authStore.isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } })
    return
  }

  if (to.name === 'Login' && authStore.isAuthenticated) {
    next({ name: 'Dashboard' })
    return
  }

  next()
})

export default router

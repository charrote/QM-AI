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
        component: () => import('@/views/BasicData.vue'),
        meta: { title: '工艺路线', keepAlive: true, requiresAuth: true },
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
        path: 'iqc/trace',
        name: 'IqcTrace',
        component: () => import('@/views/iqc/IqcTracePage.vue'),
        meta: { title: '批次追溯', keepAlive: true, requiresAuth: true },
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
        component: () => import('@/views/Defects.vue'),
        meta: { title: '不良与异常管理', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'trace',
        name: 'Trace',
        component: () => import('@/views/Trace.vue'),
        meta: { title: '质量追溯', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'complaints',
        name: 'Complaints',
        component: () => import('@/views/Complaints.vue'),
        meta: { title: '客诉与8D', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ai',
        name: 'AI',
        component: () => import('@/views/AI.vue'),
        meta: { title: 'AI质量分析', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'equipment-link',
        name: 'EquipmentLink',
        component: () => import('@/views/EquipmentLink.vue'),
        meta: { title: '设备质量联动', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'documents',
        name: 'Documents',
        component: () => import('@/views/Documents.vue'),
        meta: { title: '文件与体系', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'audits',
        name: 'Audits',
        component: () => import('@/views/Audits.vue'),
        meta: { title: '审核与稽核', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'reports',
        name: 'Reports',
        component: () => import('@/views/Reports.vue'),
        meta: { title: '报表中心', keepAlive: true, requiresAuth: true },
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
  // authStore.initFromStorage()  // called on app mount instead

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

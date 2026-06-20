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
        name: 'IQC',
        component: () => import('@/views/IQC.vue'),
        meta: { title: 'IQC来料检验', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'ipqc',
        name: 'IPQC',
        component: () => import('@/views/IPQC.vue'),
        meta: { title: 'IPQC过程检验', keepAlive: true, requiresAuth: true },
      },
      {
        path: 'fqc',
        name: 'FQC',
        component: () => import('@/views/FQC.vue'),
        meta: { title: 'FQC/OQC成品检验', keepAlive: true, requiresAuth: true },
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

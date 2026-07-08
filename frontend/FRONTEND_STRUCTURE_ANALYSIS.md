# QM-AI 前端代码结构分析报告

## 1. 技术栈概览

| 类别 | 技术 |
|------|------|
| 框架 | Vue 3.5.34 (Composition API, `<script setup>`) |
| 语言 | TypeScript 6.0.2 |
| UI 库 | Element Plus 2.14.2 + @element-plus/icons-vue |
| 路由 | Vue Router 4.6.4 (History 模式) |
| 状态管理 | Pinia 3.0.4 (Composition API 风格) |
| HTTP 客户端 | Axios 1.18.0 |
| 图表 | ECharts 6.1.0 + vue-echarts 8.0.1 |
| 构建工具 | Vite 8.0.12 + vue-tsc |
| 工具库 | @vueuse/core 14.3.0 |

## 2. 目录结构

```
frontend/src/
├── api/                    # API 调用层（每个模块一个文件）
│   ├── request.ts          #   axios 实例 + 拦截器（统一认证/刷新token）
│   ├── iqc.ts              #   IQC 模块 API
│   ├── ipqc.ts             #   IPQC 模块 API
│   ├── fqc.ts              #   FQC/OQC 模块 API
│   ├── spc.ts              #   SPC 模块 API
│   ├── defect.ts           #   不良与异常模块 API
│   ├── trace.ts            #   质量追溯模块 API
│   ├── complaint.ts        #   客诉与8D模块 API
│   ├── auth.ts             #   认证 API
│   ├── basicData.ts        #   基础数据 API
│   ├── inspectionItem.ts   #   检验项目 API
│   ├── inspectionPlan.ts   #   检验计划 API
│   ├── document.ts         #   文件体系 API
│   ├── equipmentLink.ts    #   设备联动 API
│   ├── organization.ts     #   组织层级 API
│   ├── dynamicParams.ts    #   动态参数 API
│   └── sysDict.ts          #   系统字典 API
│
├── types/                  # 类型定义（与 api/ 一一对应）
│   ├── basicData.ts        #   通用类型: PagedRequest, PagedResult + 各基础实体
│   ├── tab.ts              #   菜单/Tab 类型: TabItem, MenuConfig
│   ├── user.ts             #   用户类型
│   ├── iqc.ts              #   IQC 类型 + 枚举选项常量
│   ├── ipqc.ts             #   IPQC 类型 + 枚举选项常量
│   ├── fqc.ts              #   FQC 类型 + 枚举选项常量
│   ├── spc.ts              #   SPC 类型 + 枚举选项常量
│   ├── defect.ts           #   不良类型 + CAPA 类型 + 枚举选项常量
│   ├── trace.ts            #   追溯类型 + 枚举选项常量
│   ├── complaint.ts        #   客诉类型 + 8D类型 + 枚举选项常量
│   └── ...                 #   其余模块类型
│
├── stores/                 # Pinia 状态管理
│   ├── authStore.ts        #   认证状态 (token, user, login/logout/refresh)
│   ├── tabStore.ts         #   Tab 栏管理 (增删改查 + localStorage 持久化)
│   ├── appStore.ts         #   应用全局状态 (侧边栏折叠, 主题, 告警计数)
│   └── orgStore.ts         #   组织树状态 (组织选择器)
│
├── views/                  # 页面组件
│   ├── Dashboard.vue       #   首页
│   ├── Login.vue           #   登录页
│   ├── NotFound.vue        #   404 页
│   ├── BasicData.vue       #   基础数据（多 tab 复用同一组件）
│   ├── SPC.vue             #   SPC 统计分析（已完成，复杂组件 ~1300行）
│   │
│   ├── iqc/                #   IQC 子模块（已完成）
│   │   ├── IqcParams.vue
│   │   ├── IqcReceiptsPage.vue
│   │   ├── IqcInspectionsPage.vue
│   │   ├── IqcAnomaliesPage.vue
│   │   ├── IqcSuppliersPage.vue
│   │   ├── IqcTracePage.vue
│   │   └── PdaIqcScan.vue
│   │
│   ├── ipqc/               #   IPQC 子模块（已完成）
│   │   ├── IpqcFirstPiecesPage.vue
│   │   ├── IpqcPatrolsPage.vue
│   │   ├── IpqcPlansPage.vue
│   │   ├── IpqcRiskDashboard.vue
│   │   └── PdaIpqcScan.vue
│   │
│   ├── fqc/                #   FQC 子模块（已完成）
│   │   ├── FqcInspectionsPage.vue
│   │   ├── FqcBatchesPage.vue
│   │   ├── FqcOqcReleasesPage.vue
│   │   └── FqcPackagingPage.vue
│   │
│   ├── inspection/         #   检验管理子模块（已完成）
│   │   ├── InspectionItemsPage.vue
│   │   └── InspectionPlansPage.vue
│   │
│   └── 以下均为占位页面（需要开发）
│       ├── Defects.vue     #   不良与异常管理 ← S6
│       ├── Trace.vue       #   质量追溯 ← S7
│       ├── Complaints.vue  #   客诉与8D ← S8
│       ├── AI.vue          #   AI质量分析 ← S9
│       ├── EquipmentLink.vue # 设备质量联动 ← S10
│       ├── Documents.vue   #   文件与体系
│       ├── Audits.vue      #   审核与稽核
│       ├── Reports.vue     #   报表中心
│       └── Settings.vue    #   系统管理
│
├── layouts/
│   └── MainLayout.vue      # 主布局（侧边栏 + 顶栏 + Tab栏 + KeepAlive）
│
├── components/
│   └── ContextMenu.vue     # Tab 右键菜单
│
├── config/
│   └── menu.config.ts      # 菜单配置（驱动侧边栏 + Tab 栏）
│
├── router/
│   └── index.ts            # 路由配置
│
├── assets/styles/
│   └── variables.css       # CSS 变量（主题支持）
│
├── App.vue                 # 根组件
├── main.ts                 # 入口文件
└── style.css               # 全局样式
```

## 3. 已实现模块 vs 待开发模块

### 已完成（有完整实现）
| 模块 | 路由前缀 | 子页面数 | 备注 |
|------|----------|---------|------|
| 基础数据 | /basic-data/* | 12（共用 BasicData.vue） | 多 tab 复用 |
| IQC来料检验 | /iqc/* | 7 | 完整 CRUD + 抽样计算 + 供应商评分 + PDA |
| IPQC过程检验 | /ipqc/* | 5 | 完整 CRUD + 首件/巡检/计划/风险 + PDA |
| FQC成品检验 | /fqc/* | 4 | 完整 CRUD + 批次/出货/包装 |
| 检验项目 | /basic-data/inspection-items | 1 | 完整实现 |
| 检验计划 | /basic-data/inspection-plans | 1 | 完整实现 |
| SPC统计分析 | /spc | 1 | 完整实现（含ECharts图表、ANOVA、判异规则） |

### 待开发（占位页面，但 API 层和类型定义已完成）
| 模块 | 占位文件 | API 文件 | 类型文件 | 对应 Sprint |
|------|----------|----------|----------|------------|
| 不良与异常管理 | Defects.vue | defect.ts | defect.ts | **S6** |
| 质量追溯 | Trace.vue | trace.ts | trace.ts | **S7** |
| 客诉与8D | Complaints.vue | complaint.ts | complaint.ts | **S8** |
| AI质量分析 | AI.vue | - | - | **S9** |
| 设备质量联动 | EquipmentLink.vue | equipmentLink.ts | equipmentLink.ts | **S10** |

> 关键发现：S6/S7/S8 的 API 层和类型定义**已经预先写好**，只需开发 Vue 页面组件。
> S9/S10 需要同时确认后端 API 是否就绪。

## 4. 页面组件实现模式分析

### 4.1 标准列表页模式（IQC/IPQC/FQC 子页面）

```
┌─────────────────────────────────────────────────────┐
│  [搜索框]  [筛选下拉]  [刷新按钮]  [新建按钮]        │  ← toolbar-row
├─────────────────────────────────────────────────────┤
│  ┌──────┬──────┬──────┬──────┬──────┬──操作──┐     │
│  │ 列1  │ 列2  │ 列3  │ 列4  │ 列5  │ 详情/  │     │  ← el-table
│  │      │      │      │      │      │ 提交   │     │
│  └──────┴──────┴──────┴──────┴──────┴───────┘     │
├─────────────────────────────────────────────────────┤
│                        [1] [2] [3] ...              │  ← el-pagination
└─────────────────────────────────────────────────────┘
```

**核心模式：**
1. `<script setup lang="ts">` + `defineOptions({ name: 'XxxPage' })`
2. 状态：`ref` 用于列表数据、分页参数、搜索关键字；`reactive` 用于查询对象
3. 加载：`async function loadData()` — 调用 `xxxApi.list(params)`，赋值 `items.value = res.items`
4. 分页：`el-pagination` 双向绑定 `page`/`pageSize`，`@current-change` 触发 `loadData`
5. 操作列：`el-button link size="small" type="primary" @click="viewDetail(row)"`
6. 弹窗：`el-dialog` / `el-drawer` 用于新建/编辑/详情/提交
7. 生命周期：`onMounted(async () => { await loadData() })`
8. 样式：`<style scoped>` 包含 `.page-container`, `.toolbar-row`, `.pagination-row`

### 4.2 状态标签渲染模式

```typescript
// 在 types/ 中定义枚举选项
export const STATUS_OPTIONS = [
  { value: 'pending', label: '待处理', type: 'info' },
  { value: 'active', label: '进行中', type: 'primary' },
  { value: 'done', label: '已完成', type: 'success' },
]

// 在 Vue 组件中渲染
<el-tag :type="STATUS_OPTIONS.find(o => o.value === row.status)?.type || 'info'" size="small">
  {{ STATUS_OPTIONS.find(o => o.value === row.status)?.label || row.status }}
</el-tag>
```

### 4.3 检验提交流程模式

IQC/IPQC/FQC 三个模块的检验提交流程高度一致：
1. 点击"提交" → 加载检验单详情 → 自动从检验计划匹配检验项目
2. 弹出 Dialog，展示检验项目列表（可增删）
3. 每项：项目名称 + 规格范围(tooltip) + 实测值输入 + 合格/不合格下拉 + 备注
4. 提交 → 调用 `xxxApi.submit(id, { items, conclusion, ... })` → 关闭弹窗 → 刷新列表

### 4.4 复杂页面模式（SPC.vue）

SPC 页面是已实现的最复杂组件（~1300行），采用：
- 左侧列表 + 右侧详情的双栏布局
- `el-tabs` 多 tab 切换（控制图/判异规则/报警记录/方差分析/数据源）
- ECharts 动态渲染（X̄ 图和 R 图）
- 大量 CRUD 操作 + 数据分析引擎集成

## 5. API 层模式

### 5.1 request.ts — 统一 HTTP 客户端

```typescript
// baseURL: /api/v1 (通过 Vite proxy 转发到后端)
// 请求拦截：自动附加 Bearer token
// 响应拦截：401 自动 refresh token → 队列重试 → 失败则跳转登录
// 错误处理：ElMessage.error(msg)
```

### 5.2 模块 API 文件模式

```typescript
import request from './request'
import type { PagedRequest, PagedResult } from '@/types/basicData'
import type { Entity, EntityDetail, CreateEntity, UpdateEntity } from '@/types/xxx'

const BASE = '/module-name'

export const entityApi = {
  list(params: PagedRequest): Promise<PagedResult<Entity>> {
    return request.get(`${BASE}/entities`, { params }).then(r => r.data)
  },
  get(id: number): Promise<EntityDetail> {
    return request.get(`${BASE}/entities/${id}`).then(r => r.data)
  },
  create(data: CreateEntity): Promise<EntityDetail> {
    return request.post(`${BASE}/entities`, data).then(r => r.data)
  },
  update(id: number, data: UpdateEntity): Promise<EntityDetail> {
    return request.put(`${BASE}/entities/${id}`, data).then(r => r.data)
  },
  delete(id: number): Promise<void> {
    return request.delete(`${BASE}/entities/${id}`)
  },
}
```

### 5.3 特殊 API 模式

- **客诉**: 使用 `/m09/complaints` 路径（独立微服务？）
- **8D**: 使用 `/m09/d8reports` 路径
- **检验计划匹配**: `inspectionPlanApi.getByContext({ inspectionType, productId, ... })` — 跨模块调用
- **PDF 下载**: `window.open(url)` 直接打开

## 6. 类型定义模式

### 6.1 实体类型

```typescript
// 列表展示用（扁平）
export interface Entity {
  id: number
  code: string
  name: string
  // ... 展示所需字段
  status: string
  createdAt: string
}

// 详情用（嵌套）
export interface EntityDetail extends Entity {
  items?: EntityItem[]
  related?: RelatedEntity[]
}
```

### 6.2 CRUD 请求类型

```typescript
// 创建 — 不包含 id, createdAt 等服务端生成字段
export interface CreateEntity {
  code: string
  name: string
  // ...
}

// 更新 — 所有字段可选
export interface UpdateEntity {
  code?: string
  name?: string
  status?: string
}
```

### 6.3 通用分页类型

```typescript
// types/basicData.ts
export interface PagedRequest {
  page?: number; pageSize?: number; keyword?: string;
  status?: string; sortBy?: string; sortOrder?: string
}

export interface PagedResult<T> {
  items: T[]; total: number; page: number;
  pageSize: number; totalPages: number
}
```

### 6.4 枚举选项常量

每个 types 文件底部定义该模块的所有枚举选项，格式统一：
```typescript
export const XXX_OPTIONS = [
  { value: 'xxx', label: '中文标签', type: 'success' },
]
export const XXX_MAP: Record<string, string> = { xxx: '中文标签' }
```

## 7. 路由注册方式

**文件**: `src/router/index.ts`

### 7.1 路由结构

```
/ (MainLayout)
├── /dashboard         → Dashboard.vue
├── /basic-data/*      → BasicData.vue (同一组件，6个子路由)
├── /iqc/*             → iqc/IqcXxxPage.vue (6个子路由)
├── /ipqc/*            → ipqc/IpqcXxxPage.vue (4个子路由)
├── /fqc/*             → fqc/FqcXxxPage.vue (4个子路由)
├── /spc               → SPC.vue
├── /defects           → Defects.vue (占位)
├── /trace             → Trace.vue (占位)
├── /complaints        → Complaints.vue (占位)
├── /ai                → AI.vue (占位)
├── /equipment-link    → EquipmentLink.vue (占位)
├── /documents         → Documents.vue (占位)
├── /audits            → Audits.vue (占位)
├── /reports           → Reports.vue (占位)
├── /organizations     → OrganizationPage.vue
└── /settings          → Settings.vue

/pda/iqc               → iqc/PdaIqcScan.vue (独立路由，无 MainLayout)
/pda/ipqc              → ipqc/PdaIpqcScan.vue (独立路由，无 MainLayout)
/login                 → Login.vue (独立路由)
/:pathMatch(.*)*       → NotFound.vue
```

### 7.2 路由 Meta 规范

```typescript
interface RouteMeta {
  title?: string       // 页面标题（用于 document.title 和 Tab 栏）
  keepAlive?: boolean  // 是否参与 KeepAlive 缓存
  requiresAuth?: boolean // 是否需要登录（默认 true）
}
```

### 7.3 子模块路由模式

对于有子页面的模块（IQC/IPQC/FQC/基础数据），使用 redirect 模式：
```typescript
{
  path: 'iqc',
  redirect: '/iqc/params',    // 默认子页面
  meta: { title: 'IQC来料检验', requiresAuth: true },
},
{
  path: 'iqc/params',
  name: 'IqcParams',
  component: () => import('@/views/iqc/IqcParams.vue'),
  meta: { title: '动态参数配置', keepAlive: true, requiresAuth: true },
}
```

## 8. 菜单配置方式

**文件**: `src/config/menu.config.ts`

### 8.1 MenuConfig 结构

```typescript
interface MenuConfig {
  id: string           // 唯一标识，用于 Tab 匹配
  name: string         // 显示名称
  icon: string         // Element Plus 图标名称
  path: string         // 路由路径（父菜单用 _xxx 虚拟路径）
  closable: boolean    // 是否可关闭 Tab
  order: number        // 排序权重
  module: string       // 模块归属
  children?: MenuConfig[]
}
```

### 8.2 父菜单虚拟路径

有子菜单的父级使用 `_module` 格式（如 `_iqc`, `_ipqc`, `_fqc`, `_basic`），
这些路径不注册真实路由，仅用于侧边栏折叠/展开。

### 8.3 KeepAlive 组件名映射

`MainLayout.vue` 中的 `componentNameMap` 将 Tab ID 映射到 Vue 组件名：
```typescript
const componentNameMap: Record<string, string> = {
  'iqc-inspections': 'IqcInspectionsPage',
  'fqc-inspections': 'FqcInspectionsPage',
  defects: 'Defects',
  // ...
}
```

**新增页面时，必须同时更新此映射！**

## 9. 主布局架构

```
┌──────────────────────────────────────────────────────┐
│  [折叠] QM-AI    │  组织选择  主题  告警  用户     │  ← el-header (48px)
├──────────────────┼──────────────────────────────────┤
│                  │  [Tab1] [Tab2] [Tab3] [>]      │  ← tab-bar (36px)
│  ┌──────────────┐│                                  │
│  │ 首页         ││  ┌────────────────────────────┐ │
│  │ ▼基础数据    ││  │                            │ │
│  │  IQC来料检验 ││  │   RouterView + KeepAlive   │ │  ← el-main
│  │  IPQC过程检验││  │                            │ │
│  │  FQC成品检验 ││  └────────────────────────────┘ │
│  │  SPC统计     ││                                  │
│  │  ...         ││                                  │
│  └──────────────┘│                                  │
├──────────────────┴──────────────────────────────────┤
```

关键特性：
- 侧边栏宽度：折叠 64px / 展开 220px
- Tab 栏支持水平滚动 + 右键菜单（关闭/关闭其他/关闭全部/刷新）
- KeepAlive 基于 `componentNameMap` 动态计算 include 列表
- 路由切换自动添加/激活 Tab

## 10. 开发规范总结

### 10.1 文件命名
- 页面组件：`PascalCase.vue`，如 `IqcInspectionsPage.vue`
- API 文件：`camelCase.ts`，如 `iqc.ts`
- 类型文件：`camelCase.ts`，如 `iqc.ts`
- 子模块目录：`kebab-case` 或缩写，如 `iqc/`, `ipqc/`, `fqc/`

### 10.2 组件命名
- 必须使用 `defineOptions({ name: 'XxxPage' })` 声明组件名
- 组件名要与 `MainLayout.vue` 的 `componentNameMap` 保持一致

### 10.3 API 调用
- 统一通过 `request` 实例，不直接使用 axios
- 返回统一使用 `.then(r => r.data)` 解包
- 错误处理由拦截器统一处理，组件内 `try/catch` 仅做 ElMessage 提示

### 10.4 导入路径
- 使用 `@/` 别名指向 `src/`
- API: `import { xxxApi } from '@/api/xxx'`
- 类型: `import type { Xxx } from '@/types/xxx'`

### 10.5 路由注册
1. 在 `router/index.ts` 的 MainLayout children 中添加路由
2. 设置 `meta: { title: '中文', keepAlive: true, requiresAuth: true }`
3. 子模块使用 redirect 模式

### 10.6 菜单注册
1. 在 `config/menu.config.ts` 中添加 MenuConfig 项
2. 子模块添加到父菜单的 `children` 数组
3. `id` 必须唯一，用于 Tab 匹配

### 10.7 KeepAlive 映射
新增页面后，在 `MainLayout.vue` 的 `componentNameMap` 中添加映射：
```typescript
'xxx-menu-id': 'XxxPageComponentName',
```

## 11. S6-S10 前端开发指导

### S6: 不良与异常管理

**现状**: `Defects.vue` 是占位页面，`defect.ts` (API + 类型) 已完成。

**需要实现的功能**（基于类型定义推断）:
1. 不良记录列表 — CRUD + 筛选（来源、严重程度、状态）
2. CAPA 流程管理 — 6阶段流程（创建→临时措施→根本原因→纠正→预防→验证→关闭）
3. 报废/返工管理 — 审批流 + 返工检验
4. 统计分析 — 不良率、趋势

**建议页面结构**:
```
views/defects/
├── DefectsPage.vue          # 不良记录列表
├── DefectsDetail.vue        # 不良详情
├── CapaPage.vue             # CAPA 列表
├── CapaDetail.vue           # CAPA 详情（含阶段步骤条）
└── ScrapReworkPage.vue      # 报废/返工记录
```

**路由配置**:
```typescript
{ path: 'defects', redirect: '/defects/records', meta: {...} },
{ path: 'defects/records', name: 'Defects', component: () => import('@/views/defects/DefectsPage.vue'), meta: { title: '不良记录', keepAlive: true } },
{ path: 'defects/capa', name: 'Capa', component: () => import('@/views/defects/CapaPage.vue'), meta: { title: 'CAPA管理', keepAlive: true } },
```

**菜单配置**: 将 `defects` 改为有 children 的父菜单，path 改为 `_defects`。

**API 使用**: `defectApi` 已包含所有需要的接口。

---

### S7: 质量追溯

**现状**: `Trace.vue` 是占位页面，`trace.ts` (API + 类型) 已完成。

**需要实现的功能**:
1. 三种追溯方式：SN编码 / 批次号 / 设备
2. 追溯结果展示：产品 → 物料链 → 首件 → 巡检 → FQC → 出货
3. NG 扩散分析 — 关联批次影响分析
4. 召回模拟 — 影响客户 + 预估成本

**建议页面结构**:
```
views/trace/
└── TracePage.vue            # 追溯主页（搜索 + 结果时间线）
```

**页面模式**: 搜索区 + 时间线/关系图展示。
可参考 SPC.vue 的布局模式（左侧搜索/列表 + 右侧详情）。

**API 使用**: `traceApi` 包含 5 个方法：`traceBySn`, `traceByBatch`, `traceByEquipment`, `ngDiffusion`, `recallSimulation`。

---

### S8: 客诉与8D

**现状**: `Complaints.vue` 是占位页面，`complaint.ts` (API + 类型) 已完成。

**需要实现的功能**:
1. 客诉列表 — CRUD + 状态流转
2. 客诉详情 — 时间线 + 5W2H
3. 8D 报告 — 8步流程（D0-D8）
4. 8D 报告编辑 — 逐步填写
5. PDF 导出

**建议页面结构**:
```
views/complaints/
├── ComplaintsPage.vue       # 客诉列表
├── ComplaintDetail.vue      # 客诉详情（含时间线）
├── D8ReportsPage.vue        # 8D报告列表
└── D8ReportDetail.vue       # 8D报告详情（逐步编辑）
```

**路由配置**: 类似 defects，改为有子路由的模式。

**API 使用**: `complaintApi` 已包含所有接口，包括 `downloadPdf`。
注意：客诉 API 路径是 `/m09/complaints`（独立服务）。

---

### S9: AI质量分析

**现状**: `AI.vue` 是占位页面，**无专用 API 文件**。

**需要确认**:
1. 后端 AI 分析 API 是否已实现
2. AI 分析的具体功能范围

**可能需要的功能**:
- 基于 IQC/IPQC/FQC/SPC 数据的 AI 分析
- 质量趋势预测
- 异常模式识别
- 智能建议生成

**开发建议**:
1. 先与后端确认 API 接口，创建 `api/ai.ts` 和 `types/ai.ts`
2. 参考 `ipqc/riskScoreApi` 和 `iqc/aiRiskApi` 的模式（已有类似功能）
3. 可能需要聚合多个模块的数据

---

### S10: 设备质量联动

**现状**: `EquipmentLink.vue` 是占位页面，`equipmentLink.ts` (API + 类型) 已完成。

**建议**: 先读取 `api/equipmentLink.ts` 和 `types/equipmentLink.ts` 了解具体功能，然后按标准列表页模式开发。

---

## 12. 新增页面 Checklist

每新增一个页面模块，按以下顺序操作：

- [ ] 1. 创建 `views/xxx/` 目录和 `.vue` 组件文件
- [ ] 2. 组件内使用 `defineOptions({ name: 'XxxPage' })`
- [ ] 3. 在 `router/index.ts` 中注册路由（包括 redirect）
- [ ] 4. 在 `config/menu.config.ts` 中添加菜单配置项
- [ ] 5. 在 `MainLayout.vue` 的 `componentNameMap` 中添加映射
- [ ] 6. 如 API/类型文件不存在，创建 `api/xxx.ts` 和 `types/xxx.ts`
- [ ] 7. 遵循现有模式：`page-container` > `toolbar-row` > `el-table` > `pagination-row`
- [ ] 8. 运行 `npm run build` 验证编译通过

## 13. 关键注意事项

1. **KeepAlive 缓存**: 所有页面路由设置 `keepAlive: true`，组件名必须在 `componentNameMap` 中注册
2. **基础数据复用**: 产品/工序/BOM等12个子路由共用 `BasicData.vue`，通过路由 name/meta 区分 tab
3. **检验计划联动**: IQC/IPQC/FQC 检验提交时自动从 `inspectionPlanApi.getByContext()` 加载检验项目
4. **API 路径差异**: 客诉使用 `/m09/` 前缀，其他模块使用 `/模块名` 格式
5. **PDA 页面**: PDA 路由（`/pda/iqc`, `/pda/ipqc`）不在 MainLayout 下，独立渲染
6. **枚举常量复用**: types 中定义的 `XXX_OPTIONS` / `XXX_MAP` 供 Vue 组件直接使用

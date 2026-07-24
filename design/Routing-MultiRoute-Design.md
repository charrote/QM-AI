# 工艺路线多路线（Multi-Route）改造设计文档

> **文档版本**: v1.0
> **创建日期**: 2026-07-24
> **状态**: 待评审
> **影响范围**: 后端 M02/RoutingsController + Routing Model、前端 Routing.vue + routing 组件、database/init.sql

---

## 1. 需求背景与问题分析

### 1.1 业务需求

在质量管理场景中，同一产品可能存在多种工艺路线：

| 场景 | 路线类型 | 示例 |
|------|---------|------|
| 正常生产 | 标准路线 (STD) | 按工艺文件执行的标准加工流程 |
| 替代方案 | 替代路线 (ALT) | 某工序设备故障时使用替代工序 |
| 紧急状态 | 紧急路线 (EMG) | 产能紧缺时的快速工艺路线 |
| 客户特殊要求 | 客制路线 (CUS) | 特定客户的特殊加工流程 |

**核心需求**: 一个产品可以有多个工艺路线，用户需要能够切换、编辑、对比不同路线，而当前系统只能操作"一条路线"。

### 1.2 现有设计问题

#### 数据模型层面

当前 `routings` 表混合了两个职责：

| 职责 | 当前表现 | 问题 |
|------|---------|------|
| 路线头信息（编号、名称、描述） | 每个步骤行都重复 Code / RoutingName | 数据冗余，无法区分"路线"和"步骤" |
| 步骤信息（顺序、工序、工时） | 混在同一张表 | 创建步骤时需要同时填路线头信息，语义不清 |

```
当前 rountings 表结构：
┌───────┬──────────┬────────────┬──────────────┬──────────┬──────────┐
│ step1 │ STD-001  │ 标准路线   │ 1            │ P001     │ 10 min   │  ← Code 重复
│ step2 │ STD-001  │ 标准路线   │ 2            │ P002     │ 15 min   │  ← Code 重复
│ step3 │ STD-001  │ 标准路线   │ 3            │ P003     │ 8 min    │  ← Code 重复
│ step1 │ ALT-001  │ 替代路线   │ 1            │ P001     │ 10 min   │  ← 同一产品两条路线
│ step2 │ ALT-001  │ 替代路线   │ 2            │ P004     │ 20 min   │  ← 替换了 P002
└───────┴──────────┴────────────┴──────────────┴──────────┴──────────┘
```

- **无主键区分**：同一产品的不同路线之间没有清晰的"头-行"关系
- **路由 Code 复用**：同一 `routing_code` 在多个步骤中重复，但步骤之间没有 `routing_id` 关联
- **无法区分路线**：通过 `ProductId + routing_code` 组合判断路线，但 Code 本身不具备唯一约束
- **`routing_steps` 表废弃**：初始化 SQL 创建了 `routing_steps` 表但 EF Core 未使用，属于历史遗留

#### Controller 层面

| 接口 | 问题 |
|------|------|
| `GET product/{productId}` | 返回所有步骤扁平列表，没有按路线分组 |
| `GET product/{productId}` 返回的 `RouteCode` | 取自第一个步骤的 `process.Code`（工序编码），**语义错误** |
| `GET product/{productId}` 返回的 `RouteName` | 硬编码为 `"{productName}工艺路线"`，无实际路线名称 |
| `POST` / `PUT` 创建/更新 | 没有"路线头"概念，操作粒度是步骤 |
| `POST clone` 克隆 | 从源产品复制所有步骤到目标产品，但无法选择复制哪条路线 |

#### 前端层面

| 问题 | 说明 |
|------|------|
| 单路线视图 | 只能展示和操作一条路线，无路线切换器 |
| 按钮语义错误 | "新增路线"实际是添加步骤（`RouteStepDrawer`） |
| 克隆对话框 | 只按产品维度操作，无法选择源路线 |
| `routeCode` 显示工序编码 | 显示的是 `process.code` 而非 `routing.code` |
| 无路线选择组件 | 缺少路线 Tab / 下拉选择器 |

### 1.3 历史 SQL 与 EF Core 的不一致

`database/init.sql` 中定义了两个表：

- `routings`：包含 `routing_code`、`routing_name`、`description` 等字段，同时也有 `step_order`、`process_id`、`standard_time_minutes`
- `routing_steps`：独立表，包含 `routing_id`、`step_order`、`process_id`

但 EF Core 仅使用了 `routings` 表，`routing_steps` 从未被查询或维护。这说明早期设计者意图拆分头/行但未完成。

---

## 2. 设计目标

| 目标 | 说明 |
|------|------|
| **解耦头/行** | `routing_headers` 存路线元信息，`routing_steps` 存步骤数据，职责清晰 |
| **支持多路线** | 同一产品可创建 N 条工艺路线，每条路线有独立编号、名称、步骤集 |
| **保持向后兼容** | 通过迁移脚本将现有 `routings` 数据无缝转换，不丢数据 |
| **统一 API 语义** | 创建路线 → 路线头，添加步骤 → 路线步骤，操作粒度明确 |
| **前端路线切换** | 提供路线 Tab / 选择器，用户可切换查看不同路线 |
| **路线级克隆** | 克隆时可选择源路线、目标路线，支持全量/部分克隆 |
| **性能可控** | 索引设计合理，列表查询使用分页，详情查询使用 Include 预加载 |

---

## 3. 数据模型设计

### 3.1 新表结构

#### routing_headers — 工艺路线头

```sql
CREATE TABLE routing_headers (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    product_id      BIGINT NOT NULL COMMENT '所属产品ID',
    route_code      VARCHAR(50) NOT NULL COMMENT '工艺路线编号（如 STD-001）',
    route_name      VARCHAR(200) NOT NULL COMMENT '工艺路线名称（如 标准路线）',
    route_type      VARCHAR(20) NOT NULL DEFAULT 'STD' COMMENT '路线类型: STD/ALT/EMG/CUS',
    description     VARCHAR(500) COMMENT '工艺路线描述',
    is_default      TINYINT(1) DEFAULT 0 COMMENT '是否默认路线',
    is_active       TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    sort_order      INT DEFAULT 0 COMMENT '显示排序',
    created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    UNIQUE KEY uk_route_code_product (route_code, product_id),
    INDEX idx_routing_headers_product (product_id),
    INDEX idx_routing_headers_type (route_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

#### routing_steps — 工艺路线步骤

```sql
CREATE TABLE routing_steps (
    id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    routing_header_id   BIGINT NOT NULL COMMENT '所属路线头ID',
    step_order          INT NOT NULL COMMENT '工序顺序（从1开始）',
    process_id          BIGINT NOT NULL COMMENT '关联工序ID',
    standard_time_minutes DECIMAL(10,2) DEFAULT 0 COMMENT '标准工时（分钟）',
    description         VARCHAR(500) COMMENT '步骤备注',
    is_active           TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (routing_header_id) REFERENCES routing_headers(id) ON DELETE CASCADE,
    FOREIGN KEY (process_id) REFERENCES processes(id) ON DELETE RESTRICT,
    UNIQUE KEY uk_step_order_header (routing_header_id, step_order),
    INDEX idx_routing_steps_header (routing_header_id),
    INDEX idx_routing_steps_process (process_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.2 关系图

```
products
    │
    │ 1:N
    ▼
routing_headers ─── 1:N ───▶ routing_steps
    │                            │
    │                            │ N:1
    │                            ▼
    │                       processes
    │
    ▼
[旧表 routings — 迁移后废弃]
```

### 3.3 枚举值定义

**route_type 枚举**:

| 值 | 中文 | 说明 | 颜色标识 |
|----|------|------|---------|
| `STD` | 标准 | 正常生产路线 | 蓝色 |
| `ALT` | 替代 | 替代工序路线 | 橙色 |
| `EMG` | 紧急 | 紧急快速路线 | 红色 |
| `CUS` | 客制 | 客户特殊路线 | 绿色 |

---

## 4. 后端 API 设计

### 4.1 路由头 API

| 方法 | 路径 | 说明 |
|------|------|------|
| `GET` | `/api/v1/routings/headers?productId={id}` | 获取产品的所有路线头列表 |
| `GET` | `/api/v1/routings/headers/{headerId}` | 获取单个路线头详情（含步骤） |
| `POST` | `/api/v1/routings/headers` | 创建工艺路线头 |
| `PUT` | `/api/v1/routings/headers/{headerId}` | 更新工艺路线头 |
| `DELETE` | `/api/v1/routings/headers/{headerId}` | 删除工艺路线头（级联删除步骤） |
| `PATCH` | `/api/v1/routings/headers/{headerId}/toggle-active` | 启用/禁用路线 |
| `PATCH` | `/api/v1/routings/headers/{headerId}/set-default` | 设为默认路线 |
| `POST` | `/api/v1/routings/headers/clone` | 克隆路线（路线头级别） |

#### 4.1.1 GET `/api/v1/routings/headers?productId={id}`

**请求**：
```
GET /api/v1/routings/headers?productId=42
```

**响应**：
```json
{
  "productId": 42,
  "productName": "PCB主板-A100",
  "productCode": "PB-A100",
  "routes": [
    {
      "id": 1,
      "routeCode": "STD-001",
      "routeName": "标准路线",
      "routeType": "STD",
      "description": "正常生产流程",
      "isDefault": true,
      "isActive": true,
      "stepCount": 5,
      "totalStandardTimeMinutes": 63.0,
      "sortOrder": 0,
      "createdAt": "2026-01-10T08:00:00Z",
      "updatedAt": "2026-06-15T10:30:00Z"
    },
    {
      "id": 2,
      "routeCode": "ALT-001",
      "routeName": "替代路线",
      "routeType": "ALT",
      "description": "P002工序设备故障时启用",
      "isDefault": false,
      "isActive": true,
      "stepCount": 5,
      "totalStandardTimeMinutes": 70.5,
      "sortOrder": 1,
      "createdAt": "2026-03-20T14:00:00Z",
      "updatedAt": "2026-07-01T09:00:00Z"
    }
  ]
}
```

#### 4.1.2 GET `/api/v1/routings/headers/{headerId}`

**响应**（单条路线详情，含所有步骤）：
```json
{
  "id": 1,
  "productId": 42,
  "productName": "PCB主板-A100",
  "productCode": "PB-A100",
  "routeCode": "STD-001",
  "routeName": "标准路线",
  "routeType": "STD",
  "description": "正常生产流程",
  "isDefault": true,
  "isActive": true,
  "sortOrder": 0,
  "stepCount": 5,
  "totalStandardTimeMinutes": 63.0,
  "steps": [
    {
      "id": 101,
      "stepOrder": 1,
      "processId": 10,
      "processCode": "P-001",
      "processName": "贴片",
      "standardTimeMinutes": 10.0,
      "description": null,
      "isActive": true
    },
    {
      "id": 102,
      "stepOrder": 2,
      "processId": 12,
      "processCode": "P-003",
      "processName": "回流焊",
      "standardTimeMinutes": 8.0,
      "description": "温度曲线按SPEC-001",
      "isActive": true
    }
  ],
  "createdAt": "2026-01-10T08:00:00Z",
  "updatedAt": "2026-06-15T10:30:00Z"
}
```

#### 4.1.3 POST `/api/v1/routings/headers`

**请求体**：
```json
{
  "productId": 42,
  "routeCode": "STD-001",
  "routeName": "标准路线",
  "routeType": "STD",
  "description": "正常生产流程",
  "isDefault": true
}
```

**响应**：
```json
{
  "id": 1,
  "productId": 42,
  "routeCode": "STD-001",
  "routeName": "标准路线",
  "routeType": "STD",
  "description": "正常生产流程",
  "isDefault": true,
  "isActive": true,
  "sortOrder": 0,
  "stepCount": 0,
  "totalStandardTimeMinutes": 0.0,
  "steps": [],
  "createdAt": "2026-07-24T08:00:00Z",
  "updatedAt": "2026-07-24T08:00:00Z"
}
```

**校验规则**：
- `productId` 必须存在
- `routeCode` 在同一产品下唯一
- `routeName` 必填

#### 4.1.4 PUT `/api/v1/routings/headers/{headerId}`

**请求体**：
```json
{
  "routeCode": "STD-002",
  "routeName": "标准路线V2",
  "routeType": "STD",
  "description": "更新后的流程",
  "isDefault": true
}
```

#### 4.1.5 DELETE `/api/v1/routings/headers/{headerId}`

- 级联删除所有步骤
- 返回 204 NoContent

#### 4.1.6 POST `/api/v1/routings/headers/clone`

**请求体**：
```json
{
  "sourceHeaderId": 1,
  "targetProductId": 45,
  "targetRouteCode": "STD-001",
  "targetRouteName": "标准路线",
  "targetRouteType": "STD"
}
```

**响应**：
```json
{
  "id": 3,
  "routeCode": "STD-001",
  "routeName": "标准路线",
  "stepCount": 5,
  "message": "工艺路线克隆成功，已复制 5 个工序步骤"
}
```

### 4.2 路线步骤 API

| 方法 | 路径 | 说明 |
|------|------|------|
| `GET` | `/api/v1/routings/headers/{headerId}/steps` | 获取路线的所有步骤 |
| `POST` | `/api/v1/routings/headers/{headerId}/steps` | 添加步骤到路线 |
| `PUT` | `/api/v1/routings/headers/{headerId}/steps/{stepId}` | 更新步骤信息 |
| `DELETE` | `/api/v1/routings/headers/{headerId}/steps/{stepId}` | 删除步骤 |
| `PATCH` | `/api/v1/routings/headers/{headerId}/steps/reorder` | 批量调整步骤顺序 |
| `POST` | `/api/v1/routings/headers/{headerId}/steps/batch` | 批量添加步骤 |

#### 4.2.1 POST `/api/v1/routings/headers/{headerId}/steps`

**请求体**：
```json
{
  "processId": 12,
  "stepOrder": 3,
  "standardTimeMinutes": 8.0,
  "description": "温度曲线按SPEC-001"
}
```

#### 4.2.2 PATCH `/api/v1/routings/headers/{headerId}/steps/reorder`

**请求体**：
```json
{
  "stepIds": [103, 101, 104, 102, 105]
}
```

**响应**：
```json
{
  "message": "排序已更新",
  "count": 5
}
```

步骤按照 `stepIds` 数组顺序重新分配 `stepOrder`（1, 2, 3...）。

### 4.3 旧接口兼容

保留以下旧接口的兼容层（可择期下线）：

| 旧接口 | 兼容说明 |
|--------|---------|
| `GET product/{productId}` | 内部重定向到 `GET /headers?productId={id}`，取第一条活跃路线的详情返回 |
| `POST` 创建 Routing | 兼容层：如果 Code 存在且属于该产品 → 在该路线下添加步骤；否则 → 创建新路线头 + 步骤 |
| `DELETE {id}` | 兼容层：通过步骤 ID 定位到 headerId → 调用 header 级删除 |
| `POST batch` / `POST clone` | 保留不变，内部适配新表结构 |

---

## 5. 前端交互设计

### 5.1 页面布局

```
┌─────────────────────────────────────────────────────────────────┐
│  页面头部                                                        │
│  [文档图标] 产品工艺路线                                           │
│              管理产品的工序流程与步骤排序                           │
│                                                       [刷新] [克隆] [新增路线] │
├────────────┬────────────────────────────────────────────────────┤
│  左侧栏     │  主内容区                                           │
│            │                                                     │
│ ┌──────────┐│  ┌──────────────────────────────────────────────┐ │
│ │ 产品列表  ││  │  路线选择器（Tab 栏）                          │ │
│ │          ││  │  [标准路线] [替代路线] [紧急路线] [+ 新增]     │ │
│ │ ─搜索框 ─││  │                                              │ │
│ │ [P001    ]││  │  路线信息卡片                                  │ │
│ │ [P002    ]││  │  STD-001  |  标准路线  |  5步骤 | 63 min    │ │
│ │ [P003    ]││  │                                              │ │
│ │          ││  │  ┌───┐  ┌───┐  ┌───┐  ┌───┐  ┌───┐       │ │
│ │          ││  │  │步骤1│→│步骤2│→│步骤3│→│步骤4│→│步骤5│       │ │
│ │          ││  │  │贴片 │  │回流│  │检测│  │包装│  │出货│       │ │
│ │          ││  │  │P001│  │P003│  │P005│  │P007│  │P008│       │ │
│ │          ││  │  │10m │  │ 8m │  │ 5m │  │ 3m │ │ 7m │       │ │
│ │          ││  │  └───┘  └───┘  └───┘  └───┘  └───┘       │ │
│ │          ││  │        ↕ 拖拽排序                            │ │
│ │          ││  │            (+) 添加步骤                      │ │
│ │          ││  │                                              │ │
│ └──────────┘│  └──────────────────────────────────────────────┘ │
└────────────┴────────────────────────────────────────────────────┘
```

### 5.2 组件设计

#### 5.2.1 RouteSelector — 路线选择器组件（新增）

```vue
<!-- RouteSelector.vue -->
<template>
  <div class="route-selector">
    <!-- 路线 Tab 栏 -->
    <el-tabs v-model="activeRouteId" @tab-change="onRouteChange">
      <el-tab-pane
        v-for="route in routes"
        :key="route.id"
        :name="route.id"
        :label="route.routeName"
      >
        <template #label>
          <div class="tab-label">
            <RouteTypeTag :type="route.routeType" />
            <span>{{ route.routeName }}</span>
            <el-tag v-if="route.isDefault" type="primary" size="small" effect="plain">默认</el-tag>
            <el-tag v-if="!route.isActive" type="info" size="small">已禁用</el-tag>
            <el-button
              size="small"
              text
              @click.stop="toggleActive(route)"
            >
              {{ route.isActive ? '禁用' : '启用' }}
            </el-button>
          </div>
        </template>
      </el-tab-pane>
    </el-tabs>

    <!-- 添加路线按钮 -->
    <el-button text @click="openCreateRoute">
      <el-icon><Plus /></el-icon> 新增路线
    </el-button>
  </div>
</template>
```

**Props**:
```typescript
interface RouteSelectorProps {
  productId: number
  routes: RouteHeaderDto[]
  activeRouteId: number | null
}
```

**Events**:
```typescript
emit('route-selected', routeId: number)
emit('route-created', routeId: number)
emit('route-toggled', route: RouteHeaderDto)
```

#### 5.2.2 RouteTypeTag — 路线类型标签组件（新增）

| 类型 | 标签样式 |
|------|---------|
| STD (标准) | 蓝色 primary tag |
| ALT (替代) | 橙色 warning tag |
| EMG (紧急) | 红色 danger tag |
| CUS (客制) | 绿色 success tag |

#### 5.2.3 RouteHeaderCard — 路线信息卡片组件（新增）

展示路线概要信息：

```
┌─────────────────────────────────────────────┐
│  STD-001  │  标准路线  │  正常生产流程       │
│  ──────────────────────────────────────      │
│  默认: ●    启用: ●    步骤: 5    总工时: 63min │
└─────────────────────────────────────────────┘
```

### 5.3 状态管理

#### 5.3.1 Pinia Store（可选，简单场景用 ref 即可）

如果后续需要跨组件共享路线状态，可创建 `useRouteStore`：

```typescript
interface RouteStore {
  // 当前产品
  productId: number | null
  // 路线列表
  routes: RouteHeaderDto[]
  // 当前激活路线
  activeRoute: RouteHeaderDto | null
  // 当前路线的步骤
  steps: ProductRouteStepDto[]
  // 加载状态
  loading: boolean
  
  fetchRoutes(productId: number): Promise<void>
  fetchRouteDetail(headerId: number): Promise<void>
  createRoute(data: CreateRouteHeaderDto): Promise<RouteHeaderDto>
  updateRoute(headerId: number, data: UpdateRouteHeaderDto): Promise<void>
  deleteRoute(headerId: number): Promise<void>
  switchRoute(headerId: number): Promise<void>
  addStep(headerId: number, data: AddStepDto): Promise<void>
  updateStep(headerId: number, stepId: number, data: UpdateStepDto): Promise<void>
  deleteStep(headerId: number, stepId: number): Promise<void>
  reorderSteps(headerId: number, stepIds: number[]): Promise<void>
  cloneRoute(data: CloneRouteDto): Promise<void>
}
```

> **决策**：初期版本使用组件内 `ref`，不引入 Store。当路线操作频率增加或需要跨路由共享时再引入 Pinia。

### 5.4 路由类型选择器

新增路线时，弹出 `RouteHeaderDrawer`（复用 `RouteStepDrawer` 的结构）：

```vue
<!-- RouteHeaderDrawer.vue -->
<template>
  <el-drawer :title="drawerTitle" size="520px">
    <el-form>
      <el-form-item label="路线编号" prop="routeCode">
        <el-input v-model="form.routeCode" placeholder="如 STD-001" />
      </el-form-item>
      
      <el-form-item label="路线名称" prop="routeName">
        <el-input v-model="form.routeName" placeholder="如 标准路线" />
      </el-form-item>
      
      <el-form-item label="路线类型" prop="routeType">
        <el-radio-group v-model="form.routeType">
          <el-radio value="STD">标准</el-radio>
          <el-radio value="ALT">替代</el-radio>
          <el-radio value="EMG">紧急</el-radio>
          <el-radio value="CUS">客制</el-radio>
        </el-radio-group>
      </el-form-item>
      
      <el-form-item label="描述">
        <el-input type="textarea" v-model="form.description" />
      </el-form-item>
      
      <el-form-item label="设为默认">
        <el-switch v-model="form.isDefault" />
      </el-form-item>
    </el-form>
  </el-drawer>
</template>
```

### 5.5 克隆对话框改造

```
┌──────────────────────────────────────────────┐
│  克隆工艺路线                                  │
│                                              │
│  源产品              源路线                      │
│  [产品A ▼]           [标准路线 ▼]               │
│  (P001)              (STD-001: 5步骤)          │
│                                              │
│  ── 源路线步骤预览（5个步骤） ──               │
│  ① 贴片  P001  10min                         │
│  ② 回流焊 P003  8min                         │
│  ...                                        │
│                                              │
│  目标产品            目标路线                    │
│  [产品B ▼]           [新建路线 / 已有路线 ▼]    │
│  (P005)              (选择已有或新建)            │
│                                              │
│  ⚠️ 将把 5 个工序步骤复制到「产品B」的工艺路线中 │
│                                              │
│                    [取消]  [确认克隆]          │
└──────────────────────────────────────────────┘
```

---

## 6. 数据库迁移方案

### 6.1 迁移策略

采用 **分阶段迁移**：

1. **阶段一**：创建新表 + 数据迁移 + 索引
2. **阶段二**：更新 EF Core Model + DbContext
3. **阶段三**：更新 API + 前端（代码层面）
4. **阶段四**：废弃旧表 `routings`

### 6.2 迁移 SQL

```sql
-- ============================================================
-- QM-AI: 工艺路线多路线改造 — 数据库迁移脚本
-- 适用: MySQL 8.0+
-- ============================================================

USE qmai;

-- -----------------------------------------------
-- 阶段一：创建新表 + 数据迁移
-- -----------------------------------------------

-- 1. 创建 routing_headers 表
CREATE TABLE IF NOT EXISTS routing_headers (
    id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    product_id          BIGINT NOT NULL COMMENT '所属产品ID',
    route_code          VARCHAR(50) NOT NULL COMMENT '工艺路线编号',
    route_name          VARCHAR(200) NOT NULL COMMENT '工艺路线名称',
    route_type          VARCHAR(20) NOT NULL DEFAULT 'STD' COMMENT '路线类型: STD/ALT/EMG/CUS',
    description         VARCHAR(500) COMMENT '工艺路线描述',
    is_default          TINYINT(1) DEFAULT 0 COMMENT '是否默认路线',
    is_active           TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    sort_order          INT DEFAULT 0 COMMENT '显示排序',
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    UNIQUE KEY uk_route_code_product (route_code, product_id),
    INDEX idx_routing_headers_product (product_id),
    INDEX idx_routing_headers_type (route_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 2. 创建 routing_steps 表
CREATE TABLE IF NOT EXISTS routing_steps (
    id                  BIGINT AUTO_INCREMENT PRIMARY KEY,
    routing_header_id   BIGINT NOT NULL COMMENT '所属路线头ID',
    step_order          INT NOT NULL COMMENT '工序顺序',
    process_id          BIGINT NOT NULL COMMENT '关联工序ID',
    standard_time_minutes DECIMAL(10,2) DEFAULT 0 COMMENT '标准工时（分钟）',
    description         VARCHAR(500) COMMENT '步骤备注',
    is_active           TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    created_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (routing_header_id) REFERENCES routing_headers(id) ON DELETE CASCADE,
    FOREIGN KEY (process_id) REFERENCES processes(id) ON DELETE RESTRICT,
    UNIQUE KEY uk_step_order_header (routing_header_id, step_order),
    INDEX idx_routing_steps_header (routing_header_id),
    INDEX idx_routing_steps_process (process_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 3. 数据迁移：从旧 routings 表提取路线头信息
--    同一 (product_id, routing_code) 视为一条路线
INSERT INTO routing_headers (product_id, route_code, route_name, description, is_active, sort_order, created_at, updated_at)
SELECT DISTINCT
    product_id,
    routing_code,
    routing_name,
    description,
    is_active,
    ROW_NUMBER() OVER (PARTITION BY product_id ORDER BY MIN(step_order)) AS sort_order,
    MIN(created_at),
    MAX(updated_at)
FROM routings
GROUP BY product_id, routing_code, routing_name, description, is_active, created_at, updated_at
ON DUPLICATE KEY UPDATE route_name = VALUES(route_name);

-- 4. 数据迁移：从旧 routings 表提取步骤信息
INSERT INTO routing_steps (routing_header_id, step_order, process_id, standard_time_minutes, description, is_active, created_at, updated_at)
SELECT rh.id, r.step_order, r.process_id, r.standard_time_minutes, r.description, r.is_active, r.created_at, r.updated_at
FROM routings r
INNER JOIN routing_headers rh
    ON r.product_id = rh.product_id
    AND r.routing_code = rh.route_code
ORDER BY r.product_id, rh.sort_order, r.step_order;

-- 5. 验证迁移结果
SELECT
    rh.product_id,
    rh.route_code,
    rh.route_name,
    COUNT(rs.id) AS step_count
FROM routing_headers rh
LEFT JOIN routing_steps rs ON rh.id = rs.routing_header_id
GROUP BY rh.product_id, rh.route_code, rh.route_name;

-- 6. 删除旧表 routings（确认数据迁移无误后执行）
-- DROP TABLE IF EXISTS routings;
```

### 6.3 迁移验证

执行迁移后，用以下 SQL 校验：

```sql
-- 新旧数据行数对比
SELECT '旧表' AS source, COUNT(*) AS count FROM routings
UNION ALL
SELECT '新表headers', COUNT(*) FROM routing_headers
UNION ALL
SELECT '新表steps', COUNT(*) FROM routing_steps;

-- 各路线步骤数验证
SELECT rh.id, rh.route_code, rh.route_name,
       COUNT(rs.id) AS steps_migrated
FROM routing_headers rh
LEFT JOIN routing_steps rs ON rh.id = rs.routing_header_id
GROUP BY rh.id, rh.route_code, rh.route_name;
```

---

## 7. 前后端改动清单

### 7.1 后端改动

#### 7.1.1 新建文件

| 文件 | 说明 |
|------|------|
| `Models/RoutingHeader.cs` | 路线头 Model |
| `Models/RoutingStep.cs` | 路线步骤 Model |
| `DTOs/M02/RoutingDtos.cs` | 路线头/步骤 DTO（新增） |

#### 7.1.2 修改文件

| 文件 | 改动内容 |
|------|---------|
| `Data/AppDbContext.cs` | 添加 `DbSet<RoutingHeader>`、`DbSet<RoutingStep>`；在 `OnModelCreating` 中配置关系和约束 |
| `Controllers/M02/RoutingsController.cs` | 重构：添加 Header 相关 CRUD + Step 相关 CRUD；保留旧接口兼容层 |
| `DTOs/M02/BasicDataDtos.cs` | 更新 `ProductRouteDto` / `ProductRouteStepDto` 以匹配新结构；删除不再需要的旧 DTO |
| `Migrations/` | 运行 `dotnet ef migrations add MultiRouteDesign` 自动生成迁移 |

#### 7.1.3 RoutingHeader.cs 示例

```csharp
[Table("routing_headers")]
public class RoutingHeader
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("route_code")]
    public string RouteCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("route_name")]
    public string RouteName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("route_type")]
    public string RouteType { get; set; } = "STD";

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public ICollection<RoutingStep> Steps { get; set; } = new List<RoutingStep>();
}
```

#### 7.1.4 RoutingStep.cs 示例

```csharp
[Table("routing_steps")]
public class RoutingStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("routing_header_id")]
    public long RoutingHeaderId { get; set; }

    [Column("step_order")]
    public int StepOrder { get; set; }

    [Column("process_id")]
    public long ProcessId { get; set; }

    [Column("standard_time_minutes")]
    public double? StandardTimeMinutes { get; set; }

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(RoutingHeaderId))]
    public RoutingHeader? RoutingHeader { get; set; }

    [ForeignKey(nameof(ProcessId))]
    public Process? Process { get; set; }
}
```

#### 7.1.5 AppDbContext 变更

```csharp
// 添加
public DbSet<RoutingHeader> RoutingHeaders { get; set; } = null!;
public DbSet<RoutingStep> RoutingSteps { get; set; } = null!;

// OnModelCreating 中配置
modelBuilder.Entity<RoutingHeader>(entity =>
{
    entity.HasOne(h => h.Product)
          .WithMany()
          .HasForeignKey(h => h.ProductId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasIndex(h => new { h.RouteCode, h.ProductId }).IsUnique();
    entity.HasIndex(h => h.RouteType);

    entity.HasMany(h => h.Steps)
          .WithOne(s => s.RoutingHeader)
          .HasForeignKey(s => s.RoutingHeaderId)
          .OnDelete(DeleteBehavior.Cascade);
});

modelBuilder.Entity<RoutingStep>(entity =>
{
    entity.HasOne(s => s.Process)
          .WithMany()
          .HasForeignKey(s => s.ProcessId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasIndex(s => new { s.RoutingHeaderId, s.StepOrder }).IsUnique();
});
```

### 7.2 前端改动

#### 7.2.1 新建文件

| 文件 | 说明 |
|------|------|
| `src/components/routing/RouteSelector.vue` | 路线选择器（Tab 栏） |
| `src/components/routing/RouteHeaderDrawer.vue` | 路线头编辑抽屉 |
| `src/components/routing/RouteTypeTag.vue` | 路线类型标签 |
| `src/components/routing/RouteHeaderCard.vue` | 路线信息卡片 |

#### 7.2.2 修改文件

| 文件 | 改动内容 |
|------|---------|
| `src/types/routing.ts` | 新增 `RouteHeaderDto`、`CreateRouteHeaderDto`、`UpdateRouteHeaderDto` 等类型 |
| `src/api/routing.ts` | 新增路由头 API 调用函数 |
| `src/views/Routing.vue` | 主体改造：增加路线选择器、路由头信息卡片、适配新 API |
| `src/components/routing/RouteStepCard.vue` | 微调：适配 `routingHeaderId` 参数传递 |
| `src/components/routing/RouteStepDrawer.vue` | 改造：从"新增步骤"变为"在指定路线下新增步骤" |
| `src/components/routing/CloneDialog.vue` | 改造：增加路线选择器、支持按路线克隆 |

#### 7.2.3 types/routing.ts 变更

```typescript
// 新增
export type RouteType = 'STD' | 'ALT' | 'EMG' | 'CUS'

export interface RouteHeaderDto {
  id: number
  productId: number
  routeCode: string
  routeName: string
  routeType: RouteType
  description?: string
  isDefault: boolean
  isActive: boolean
  sortOrder: number
  stepCount: number
  totalStandardTimeMinutes: number
  createdAt: string
  updatedAt: string
}

export interface RouteListDto {
  productId: number
  productName: string
  productCode: string
  routes: RouteHeaderDto[]
}

export interface RouteDetailDto {
  id: number
  productId: number
  productName: string
  productCode: string
  routeCode: string
  routeName: string
  routeType: RouteType
  description?: string
  isDefault: boolean
  isActive: boolean
  sortOrder: number
  stepCount: number
  totalStandardTimeMinutes: number
  steps: ProductRouteStepDto[]
  createdAt: string
  updatedAt: string
}

export interface CreateRouteHeaderDto {
  productId: number
  routeCode: string
  routeName: string
  routeType: RouteType
  description?: string
  isDefault?: boolean
}

export interface UpdateRouteHeaderDto {
  routeCode?: string
  routeName?: string
  routeType?: RouteType
  description?: string
  isDefault?: boolean
}

export interface CloneRouteDto {
  sourceHeaderId: number
  targetProductId: number
  targetRouteCode: string
  targetRouteName: string
  targetRouteType: RouteType
}

// 更新
export interface ProductRouteStepDto {
  id: number
  routingHeaderId: number  // 新增
  stepOrder: number
  processId: number
  processCode: string
  processName: string
  standardTimeMinutes?: number
  description?: string
}

export interface ProductRouteDto {
  productId: number
  productCode: string
  productName: string
  routeCode: string          // 路线编号（来自 header）
  routeName: string          // 路线名称（来自 header）
  routeType: RouteType       // 路线类型（新增）
  totalSteps: number
  totalStandardTimeMinutes: number
  steps: ProductRouteStepDto[]
}
```

#### 7.2.4 api/routing.ts 变更

```typescript
// 新增
export function getRouteHeaders(productId: number) {
  return request.get<RouteListDto>(`/routings/headers`, { params: { productId } }).then(r => r.data)
}

export function getRouteDetail(headerId: number) {
  return request.get<RouteDetailDto>(`/routings/headers/${headerId}`).then(r => r.data)
}

export function createRouteHeader(data: CreateRouteHeaderDto) {
  return request.post<RouteHeaderDto>('/routings/headers', data).then(r => r.data)
}

export function updateRouteHeader(headerId: number, data: UpdateRouteHeaderDto) {
  return request.put(`/routings/headers/${headerId}`, data).then(r => r.data)
}

export function deleteRouteHeader(headerId: number) {
  return request.delete(`/routings/headers/${headerId}`).then(r => r.data)
}

export function toggleRouteActive(headerId: number) {
  return request.patch(`/routings/headers/${headerId}/toggle-active`).then(r => r.data)
}

export function addRouteStep(headerId: number, data: { processId: number; standardTimeMinutes?: number; description?: string }) {
  return request.post(`/routings/headers/${headerId}/steps`, data).then(r => r.data)
}

export function updateRouteStep(headerId: number, stepId: number, data: { processId: number; standardTimeMinutes?: number; description?: string }) {
  return request.put(`/routings/headers/${headerId}/steps/${stepId}`, data).then(r => r.data)
}

export function deleteRouteStep(headerId: number, stepId: number) {
  return request.delete(`/routings/headers/${headerId}/steps/${stepId}`).then(r => r.data)
}

export function reorderRouteSteps(headerId: number, stepIds: number[]) {
  return request.patch(`/routings/headers/${headerId}/steps/reorder`, { stepIds }).then(r => r.data)
}

export function cloneRouteHeader(data: CloneRouteDto) {
  return request.post('/routings/headers/clone', data).then(r => r.data)
}
```

---

## 8. 实施优先级

### Phase 1 — 数据模型 + 后端 API（建议 3-4 天）

| 序号 | 任务 | 预估 | 依赖 |
|------|------|------|------|
| 1 | 执行数据库迁移脚本（创建新表 + 数据迁移） | 0.5 天 | 无 |
| 2 | 创建 `RoutingHeader.cs`、`RoutingStep.cs` Model | 0.5 天 | #1 |
| 3 | 更新 `AppDbContext.cs` 配置 | 0.5 天 | #2 |
| 4 | 创建 DTOs（`RoutingDtos.cs`） | 0.5 天 | #2 |
| 5 | 实现路由头 CRUD API | 1 天 | #3 |
| 6 | 实现路线步骤 CRUD + reorder API | 0.5 天 | #3 |
| 7 | 保留旧接口兼容层（可选项） | 0.5 天 | #5 |
| 8 | 单元测试 + 集成测试 | 0.5 天 | #5, #6 |

**Phase 1 验收标准**:
- [ ] 新表 `routing_headers`、`routing_steps` 创建成功
- [ ] 旧数据完整迁移，无丢失
- [ ] `GET /routings/headers?productId=X` 返回多条路线
- [ ] `POST /routings/headers` 创建路线头
- [ ] `POST /routings/headers/{id}/steps` 添加步骤
- [ ] 拖拽排序 `PATCH /routings/headers/{id}/steps/reorder` 正常工作
- [ ] `GET product/{productId}` 兼容接口仍返回数据

### Phase 2 — 前端交互改造（建议 3-4 天）

| 序号 | 任务 | 预估 | 依赖 |
|------|------|------|------|
| 1 | 更新 TS 类型定义（`types/routing.ts`） | 0.5 天 | 无 |
| 2 | 新增 API 调用函数（`api/routing.ts`） | 0.5 天 | 无 |
| 3 | 开发 `RouteSelector` 组件 | 1 天 | 无 |
| 4 | 开发 `RouteHeaderDrawer` 组件 | 0.5 天 | #3 |
| 5 | 开发 `RouteHeaderCard` 组件 | 0.5 天 | #3 |
| 6 | 改造 `Routing.vue` 主页面 | 1 天 | #3, #4 |
| 7 | 改造 `RouteStepDrawer`、`RouteStepCard` 适配新结构 | 0.5 天 | #6 |
| 8 | 改造 `CloneDialog` 支持路线选择 | 0.5 天 | #3, #6 |

**Phase 2 验收标准**:
- [ ] 左侧产品列表中选中产品后，右侧显示路线 Tab 栏
- [ ] 可切换不同路线，步骤列表正确加载
- [ ] 点击"新增路线"弹出路线创建抽屉
- [ ] 路线信息卡片显示路线编号、名称、类型、步骤数、总工时
- [ ] 步骤拖拽排序功能正常
- [ ] 克隆对话框支持选择路线

### Phase 3 — 清理 + 优化（建议 1-2 天）

| 序号 | 任务 | 预估 | 依赖 |
|------|------|------|------|
| 1 | 删除旧 `routings` 表 + 旧 `Routing.cs` Model | 0.5 天 | Phase 1, 2 |
| 2 | 删除旧接口兼容层 | 0.5 天 | Phase 2 |
| 3 | 更新 `database/init.sql` | 0.5 天 | Phase 1, 2 |
| 4 | 性能优化（分页、懒加载、缓存） | 0.5 天 | 全部 |

---

## 9. 风险评估

### 9.1 数据迁移风险

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| 旧表数据迁移不完整 | 步骤丢失 | 迁移前备份；迁移后执行校验 SQL 对比新旧数据量 |
| 旧表 Code 重复/不规范 | 路线分组错误 | 迁移脚本按 `(product_id, routing_code)` 分组，同一分组合并为一条路线 |
| 迁移过程中服务中断 | 用户体验下降 | 建议在低峰期执行；使用事务确保原子性 |

### 9.2 兼容性问题

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| 其他模块依赖旧 `routings` 表 | 跨模块功能异常 | 全面扫描项目代码，确认无其他引用后再删除旧表 |
| EF Core Code First 与手动 SQL 冲突 | Migration 失败 | 先手动迁移数据，再删除旧表，最后生成 EF Migration |
| 前端组件引用了旧字段名 | 运行时错误 | 使用 TypeScript 类型检查 + ESLint 校验 |

### 9.3 前端交互风险

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| 路线切换时步骤数据不同步 | 用户看到错误步骤 | 切换路线时强制重新加载（`await fetchRouteDetail(headerId)`） |
| 拖拽排序后刷新页面丢失排序 | 用户体验差 | 排序后立即保存，`saveAfterReorder` 默认开启 |
| 大量步骤导致页面卡顿 | 性能问题 | 步骤卡片宽度限制 + 横向滚动，避免DOM爆炸 |

### 9.4 架构风险

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| `routing_headers` 的 `route_type` 枚举值扩展 | 未来无法新增路线类型 | 设计为 VARCHAR(20) + 注释说明，不使用 ENUM 类型 |
| 单产品路线数过多 | UI 拥挤 | 路线 Tab 支持横向滚动，超出显示 "+N" 更多按钮 |
| `is_default` 多默认 | 数据异常 | 后端校验：同一产品只能有一个 `is_default = true` |

### 9.5 性能考虑

| 场景 | 预期性能 | 优化方案 |
|------|---------|---------|
| 产品列表页加载路线 | < 200ms | 使用 `Include(h => h.Steps)` 预加载步骤 |
| 切换路线 Tab | < 100ms | 缓存已加载的路线数据，切换时直接读取缓存 |
| 批量添加 50 个步骤 | < 500ms | 使用 `DbSet.AddRange` + 单次 `SaveChangesAsync` |
| 产品有 10+ 条路线 | < 300ms | 列表接口只查 header 不加载 steps；详情接口才加载步骤 |

---

## 附录 A：完整 API 总览

```
GET    /api/v1/routings/headers              ?productId=XXX          获取产品路线列表
GET    /api/v1/routings/headers/{id}          获取路线详情（含步骤）
POST   /api/v1/routings/headers                          创建路线头
PUT    /api/v1/routings/headers/{id}                   更新路线头
DELETE /api/v1/routings/headers/{id}                   删除路线头（级联步骤）
PATCH  /api/v1/routings/headers/{id}/toggle-active     启用/禁用路线
PATCH  /api/v1/routings/headers/{id}/set-default       设为默认路线
POST   /api/v1/routings/headers/clone                  克隆路线

GET    /api/v1/routings/headers/{headerId}/steps       获取路线步骤列表
POST   /api/v1/routings/headers/{headerId}/steps       添加步骤
PUT    /api/v1/routings/headers/{headerId}/steps/{id}  更新步骤
DELETE /api/v1/routings/headers/{headerId}/steps/{id}  删除步骤
PATCH  /api/v1/routings/headers/{headerId}/steps/reorder 批量排序
POST   /api/v1/routings/headers/{headerId}/steps/batch   批量添加

-- 兼容层 --
GET    /api/v1/routings/product/{productId}            兼容：返回第一条路线
POST   /api/v1/routings/batch                          兼容：批量添加步骤
POST   /api/v1/routings/clone                          兼容：克隆路线
PATCH  /api/v1/routings/reorder                        兼容：排序
DELETE /api/v1/routings/{id}                           兼容：删除步骤
PUT    /api/v1/routings/{id}                           兼容：更新步骤
```

## 附录 B：路由类型枚举定义（前端）

```typescript
export const ROUTE_TYPE_OPTIONS = [
  { label: '标准路线', value: 'STD' },
  { label: '替代路线', value: 'ALT' },
  { label: '紧急路线', value: 'EMG' },
  { label: '客制路线', value: 'CUS' },
] as const

export const ROUTE_TYPE_COLORS: Record<RouteType, string> = {
  STD: 'primary',
  ALT: 'warning',
  EMG: 'danger',
  CUS: 'success',
}
```
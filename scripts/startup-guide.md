# QM-AI — 工业 AI 质量决策平台

> 来料检验 · 过程检验 · 成品检验 · SPC 统计过程控制 · 设备联动 · 质量追溯 · 客诉 8D

---

## 技术栈

| 层级 | 技术 | 版本 |
|------|------|------|
| 前端 | Vue 3 + TypeScript + Vite | ^3.5 / ^6.0 / ^8.0 |
| UI 框架 | Element Plus | ^2.14 |
| 状态管理 | Pinia | ^3.0 |
| 图表 | ECharts + vue-echarts | ^6.1 |
| 后端 | .NET 9 + ASP.NET Core | net9.0 |
| ORM | EF Core + Pomelo.MySql | 9.0 |
| AI 服务 | Python + FastAPI | 0.115 |
| 数据库 | MySQL | 8.0 |
| 缓存 | Redis | 7 |
| 对象存储 | MinIO | latest |
| 消息队列 | RabbitMQ | latest |

---

## 快速启动

### 方式一：本地直接启动（推荐开发使用）

**前置条件：** MySQL、Redis 已在本机运行。

```bash
# 检查环境
./start.sh --check

# 启动前后端
./start.sh

# 启动前后端 + AI 服务
./start.sh --ai

# 停止所有本地进程
./start.sh --stop
```

脚本会自动：恢复 NuGet 包、安装 npm 依赖、创建 Python venv。

### 方式二：Docker Compose 启动

```bash
# 构建 + 启动全部服务
./build.sh up

# 仅构建
./build.sh

# 仅启动（不重新构建）
docker compose up -d
```

---

## 服务端口

| 服务 | 端口 | 访问地址 |
|------|------|----------|
| 前端 | 5610 | http://localhost:5610 |
| 后端 API | 5611 | http://localhost:5611/swagger |
| AI 服务 | 8000 | http://localhost:8000/docs |
| MySQL | 3306 | — |
| Redis | 6379 | — |
| MinIO API | 19000 | http://localhost:19000 |
| MinIO Console | 19001 | http://localhost:19001 |
| RabbitMQ | 5672 / 15672 | http://localhost:15672 |

---

## 默认账号

| 角色 | 用户名 | 密码 | 权限 |
|------|--------|------|------|
| 系统管理员 | admin | admin123 | 全部 |
| 质检操作员 | operator | operator123 | 基础操作 |
| 质量检验员 | inspector | inspector123 | 检验操作 |

---

## 项目结构

```
QM-AI/
├── frontend/                       # Vue 3 + Vite
│   ├── src/
│   │   ├── api/                    # Axios 实例 + 各模块 API
│   │   ├── assets/styles/          # CSS 变量（亮色/暗色主题）
│   │   ├── components/             # 通用组件
│   │   ├── config/                 # 菜单配置（14 模块映射）
│   │   ├── layouts/                # MainLayout（Header + Sidebar + TabBar）
│   │   ├── router/                 # 路由 + 守卫
│   │   ├── stores/                 # Pinia Store
│   │   ├── types/                  # TypeScript 接口
│   │   └── views/                  # 14 个模块视图 + 登录 + 404
│   └── vite.config.ts              # 端口 5610，/api → :5611 代理
│
├── backend/                        # .NET 9 + ASP.NET Core
│   ├── Controllers/                # Auth + 各业务模块
│   ├── Models/                     # 实体模型
│   ├── DTOs/                       # 请求/响应 DTO
│   ├── Services/                   # 业务逻辑（AI 规则引擎、SPC 算法、8D 等）
│   ├── Middleware/                  # JWT + 全局异常处理
│   ├── Data/                       # EF Core DbContext + 种子数据
│   ├── Migrations/                 # 数据库迁移
│   └── Program.cs                  # 启动配置：JWT + CORS + Swagger + SignalR
│
├── ai-service/                     # Python FastAPI
│   ├── app/
│   │   ├── routers/                # 路由
│   │   ├── services/               # AI 推理 / 规则引擎
│   │   └── models/                 # 数据模型
│   └── requirements.txt
│
├── database/                       # SQL 脚本
│   └── init.sql                    # 45+ 表 Schema
│
├── docker/                         # Docker 配置
│   └── docker-compose.yml          # MySQL + Redis + MinIO + RabbitMQ
│
├── scripts/
│   ├── startup-guide.md            # 本文档
│   ├── start.sh                    # 本地启动脚本（非 Docker）
│   └── build.sh                    # Docker 构建 + 启动
│
└── .env                            # 环境变量（端口、密码、JWT Secret）
```

---

## 业务模块

| 编号 | 模块 | 说明 |
|------|------|------|
| M01 | 系统管理 | 用户、角色、权限、组织架构、字典 |
| M02 | 基础数据 | 产品、BOM、工序、工艺路线 |
| M02.1 | 检验主数据 | 检验项目、检验计划 |
| M03 | IQC 来料检验 | 来料接收、抽样检验、判定 |
| M04 | IPQC 过程检验 | 巡检、首件检验 |
| M05 | FQC 成品检验 | 成品抽样、判定 |
| M06 | SPC 统计过程控制 | X̄-R / I-MR / X̄-S 图等 |
| M07 | 不良与异常管理 | 缺陷代码、CAPA 闭环 |
| M08 | 质量追溯 | 全链路追溯 |
| M09 | 客诉 8D | 8D 报告流程 |
| M10 | AI 智能决策 | 规则引擎、异常检测 |
| M11 | 设备联动 | MQTT 数据采集、设备监控 |
| M12 | 文件管理 | 文档上传、版本控制 |
| M13 | 审核稽核 | 审核计划、问题跟踪 |
| M15 | 企业组织 | 多组织层级 |

---

## 环境配置

主要配置在 `.env` 文件中：

```env
# 端口
BACKEND_PORT=5611
FRONTEND_PORT=5610
AI_PORT=8000

# MySQL
MYSQL_ROOT_PASSWORD=qmai_root_2024
MYSQL_DATABASE=qmai

# Redis
REDIS_PASSWORD=qmai_redis

# JWT
JWT_SECRET=QM-AI-SuperSecretKey-2024-MustBeAtLeast32CharactersLong!
```

后端连接字符串在 `backend/appsettings.json` 的 `ConnectionStrings:DefaultConnection` 中配置。

---

## 后端 API

启动后访问 Swagger：http://localhost:5611/swagger

- `POST /api/auth/login` — 登录
- `POST /api/auth/register` — 注册
- `POST /api/auth/refresh` — 刷新 Token

---

*最后更新：2026-07-27*

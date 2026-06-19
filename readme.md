# 项目结构
```
QM-AI/
├── frontend/                          # Vue 3 + TypeScript + Element Plus
│   ├── src/
│   │   ├── api/                       # Axios 实例 + Auth API
│   │   ├── assets/styles/             # CSS 变量（亮色/暗色主题）
│   │   ├── components/                # ContextMenu 右键菜单
│   │   ├── config/                    # 菜单配置（14 模块映射）
│   │   ├── layouts/                   # MainLayout（Header + Sidebar + TabBar + Content）
│   │   ├── router/                    # 16 路由（14 模块 + 登录 + 404）+ 守卫
│   │   ├── stores/                    # Pinia Store（tabStore + authStore + appStore）
│   │   ├── types/                     # TypeScript 接口定义
│   │   └── views/                     # 14 个模块视图 + 登录 + 404
│   └── vite.config.ts
│
├── backend/                           # .NET 9 + ASP.NET Core
│   ├── Controllers/                   # AuthController + HealthController
│   ├── Models/                        # User, Role, Permission
│   ├── DTOs/Auth/                     # Login, Register, Token DTOs
│   ├── Services/                      # AuthService（JWT + 密码哈希）
│   ├── Middleware/                     # JWT 中间件
│   ├── Data/                          # EF Core DbContext + MySQL
│   └── Program.cs                     # JWT Auth + CORS + Swagger + SignalR
│
├── ai-service/                        # Python FastAPI 骨架
│   └── app/                           # routers, services, models, schemas
│
├── database/
│   └── init.sql                       # 45+ 表完整 Schema（MySQL 8.0）
│
└── docker/
    ├── docker-compose.yml             # MySQL + Redis + MinIO + RabbitMQ
    └── .env.example
```

# 已实现的核心功能
| 模块	| 功能 |
|----|----|
| 前端布局	| Header(48px) + Sidebar(220px/折叠64px) + TabBar(36px) + Content |
| Tab 管理 | 	Pinia Store + 打开/关闭/切换/右键菜单/持久化 |
| 路由系统 | 14 模块懒加载 + 路由守卫 + Tab 自动同步 |
| 登录页 | 居中卡片 + 表单验证 + 记住我功能 |
| JWT 认证 | 登录/登出/刷新 + Axios 拦截器 + 401 自动重试 |
| 暗色模式 |	CSS 变量系统 + 一键切换 |
| Docker 编排 |	MySQL 8.0 + Redis 7 + MinIO + RabbitMQ |
| 数据库 Schema | 45+ 表，含外键、索引、完整注释 |
| AI 服务 | 	FastAPI 骨架 + 规则引擎 + 健康检查 |

# 如何启动

** 1. 启动基础设施 ** (MySQL, Redis, MinIO, RabbitMQ)
cd docker && docker compose up -d

** 2. 初始化数据库 **
mysql -h 127.0.0.1 -u root -p qmai < ../database/init.sql

** 3. 启动后端 ** (http://localhost:5000)
cd ../backend && dotnet run

** 4. 启动前端 ** (http://localhost:5173)
cd ../frontend && npm run dev

** 5. 账号密码：**
== 默认登录账号 ==
|角色	|用户名	|密码	|说明|
|----|----|----|----|
|👑 系统管理员	|admin	|admin123	|全部权限|
|🔧 质检操作员	|operator	|operator123	|基础操作|
|🔍 质量检验员	|inspector	|inspector123	|检验操作| 

# 设施ID

``` PORT
BACKEND_PORT=5611
FRONTEND_PORT=5610
AI_PORT=8000
```

``` MySQL 
MYSQL_ROOT_PASSWORD=qmai_root_2024
MYSQL_DATABASE=qmai
MYSQL_USER=qmai
MYSQL_PASSWORD=qmai_123
MYSQL_PORT=3306
```

``` Redis 
REDIS_PASSWORD=qmai_redis
REDIS_PORT=6379
```

``` MinIO 
MINIO_ROOT_USER=qmai_admin
MINIO_ROOT_PASSWORD=qmai_minio_2024
MINIO_API_PORT=9000
MINIO_CONSOLE_PORT=9001
```

``` RabbitMQ 
RABBITMQ_USER=qmai
RABBITMQ_PASSWORD=qmai_rabbit
RABBITMQ_PORT=5672
RABBITMQ_MGMT_PORT=15672
```

``` JWT 
JWT_SECRET=QM-AI-SuperSecretKey-2024-MustBeAtLeast32CharactersLong!
```

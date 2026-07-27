using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QM_AI.API.Data;
using QM_AI.API.Helpers;
using QM_AI.API.Middleware;
using QM_AI.API.Services;

var builder = WebApplication.CreateBuilder(args);

// MySQL DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    try
    {
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        options.UseMySql(connectionString, serverVersion);
    }
    catch
    {
        options.UseMySql(connectionString, ServerVersion.Parse("8.0.0"));
    }
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secret = jwtSettings["Secret"]!;
var issuer = jwtSettings["Issuer"]!;
var audience = jwtSettings["Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = JwtHelper.GetTokenValidationParameters(secret, issuer, audience);
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "QM-AI API",
        Version = "v1",
        Description = "QM-AI Industrial AI Quality Decision Platform API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// SignalR
builder.Services.AddSignalR();

// Controllers
builder.Services.AddControllers();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<ClosureRuleEngine>();
builder.Services.AddSingleton<SamplingPlanCalculator>();
builder.Services.AddScoped<IqcService>();
builder.Services.AddScoped<IpqcService>();
builder.Services.AddScoped<FqcService>();
builder.Services.AddScoped<SpcService>();
builder.Services.AddSingleton<SpcAlgorithmService>();
builder.Services.AddSingleton<AnovaService>();
builder.Services.AddScoped<BusinessDataService>();

// M02.1 检验项目主数据 & 检验计划
builder.Services.AddScoped<InspectionItemService>();
builder.Services.AddScoped<InspectionPlanService>();

// M07 不良与异常管理
builder.Services.AddScoped<DefectService>();
builder.Services.AddScoped<CapaService>();

// M08 质量追溯
builder.Services.AddScoped<TraceService>();

// M09 客诉 8D
builder.Services.AddScoped<ComplaintService>();

// M11 设备联动
builder.Services.AddScoped<EquipmentLinkService>();

// M12 文件管理
builder.Services.AddScoped<DocumentService>();

// M13 审核稽核
builder.Services.AddScoped<AuditService>();

var app = builder.Build();

// ─── Global exception handler middleware ────────────────────────────
app.UseMiddleware<AppExceptionHandlerMiddleware>();

// 启动时自动初始化种子数据
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Console.WriteLine("[Program] Starting table creation...");

    // 创建所有表（按依赖顺序）
    var connection = context.Database.GetDbConnection();
    await connection.OpenAsync();
    using var cmd = connection.CreateCommand();
    Console.WriteLine("[Program] Connection opened.");
    
    // Auth
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `roles` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `name` VARCHAR(100) NOT NULL UNIQUE, `description` VARCHAR(500)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `permissions` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `name` VARCHAR(200) NOT NULL, `code` VARCHAR(200) NOT NULL UNIQUE, `module` VARCHAR(50)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `users` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `username` VARCHAR(100) NOT NULL UNIQUE, `password_hash` VARCHAR(500) NOT NULL, `display_name` VARCHAR(200), `avatar` VARCHAR(500), `email` VARCHAR(200), `is_active` TINYINT(1) DEFAULT 1, `role_id` BIGINT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`role_id`) REFERENCES `roles`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    // M02 基础数据
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `products` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `product_code` VARCHAR(100) NOT NULL UNIQUE, `product_name` VARCHAR(200) NOT NULL, `description` VARCHAR(500), `unit` VARCHAR(50), `product_type` VARCHAR(100), `default_inspection_level` VARCHAR(10), `default_aql` DECIMAL(5,2), `specification` LONGTEXT, `is_active` TINYINT(1) DEFAULT 1, `org_id` BIGINT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `boms` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `product_id` BIGINT NOT NULL, `material_code` VARCHAR(50), `material_name` VARCHAR(200), `quantity` DECIMAL(10,2), `unit` VARCHAR(20), `level` INT, `remark` VARCHAR(500), `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`product_id`) REFERENCES `products`(`id`) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `processes` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `process_code` VARCHAR(100) NOT NULL UNIQUE, `process_name` VARCHAR(200) NOT NULL, `description` VARCHAR(500), `process_type` VARCHAR(50), `department` VARCHAR(100), `org_id` BIGINT, `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `routings` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `product_id` BIGINT NOT NULL, `routing_code` VARCHAR(50) NOT NULL, `description` TEXT, `routing_name` VARCHAR(200), `step_order` INT, `process_id` BIGINT, `standard_time_minutes` DECIMAL(10,2), `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`product_id`) REFERENCES `products`(`id`) ON DELETE CASCADE, FOREIGN KEY (`process_id`) REFERENCES `processes`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();

    // M02.5 多工艺路线：路由头表
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `routing_headers` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `product_id` BIGINT NOT NULL COMMENT '所属产品ID', `route_code` VARCHAR(50) NOT NULL COMMENT '工艺路线编号', `route_name` VARCHAR(200) NOT NULL COMMENT '工艺路线名称', `route_type` VARCHAR(20) NOT NULL DEFAULT 'STD' COMMENT '路线类型: STD/ALT/EMG/CUS', `description` VARCHAR(500) COMMENT '工艺路线描述', `is_default` TINYINT(1) DEFAULT 0 COMMENT '是否默认路线', `is_active` TINYINT(1) DEFAULT 1 COMMENT '是否启用', `sort_order` INT DEFAULT 0 COMMENT '显示排序', `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`product_id`) REFERENCES `products`(`id`) ON DELETE CASCADE, UNIQUE KEY `uk_route_code_product` (`route_code`, `product_id`), INDEX `idx_routing_headers_product` (`product_id`), INDEX `idx_routing_headers_type` (`route_type`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();

    // M02.5 多工艺路线：路由步骤表（新结构，兼容旧 routing_steps 表迁移）
    // 策略：先删除旧 routing_steps 表（如有），再创建新表
    try
    {
        cmd.CommandText = @"DROP TABLE IF EXISTS `routing_steps`";
        await cmd.ExecuteNonQueryAsync();
        cmd.CommandText = @"CREATE TABLE `routing_steps` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `routing_header_id` BIGINT NOT NULL COMMENT '所属路线头ID', `step_order` INT NOT NULL COMMENT '工序顺序', `process_id` BIGINT COMMENT '关联工序ID', `standard_time_minutes` DECIMAL(10,2) DEFAULT 0 COMMENT '标准工时（分钟）', `description` VARCHAR(500) COMMENT '步骤备注', `is_active` TINYINT(1) DEFAULT 1 COMMENT '是否启用', `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`routing_header_id`) REFERENCES `routing_headers`(`id`) ON DELETE CASCADE, FOREIGN KEY (`process_id`) REFERENCES `processes`(`id`) ON DELETE RESTRICT, UNIQUE KEY `uk_step_order_header` (`routing_header_id`, `step_order`), INDEX `idx_routing_steps_header` (`routing_header_id`), INDEX `idx_routing_steps_process` (`process_id`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
        await cmd.ExecuteNonQueryAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Program] Warning: Failed to create routing_steps table: {ex.Message}");
    }

    // M02.5 多工艺路线：从旧 routings 表迁移数据到 routing_headers / routing_steps
    try
    {
        cmd.CommandText = @"SELECT COUNT(*) FROM `routings`";
        var oldCount = (long)(await cmd.ExecuteScalarAsync())!;
        if (oldCount > 0)
        {
            Console.WriteLine($"[Program] Migrating {oldCount} rows from old routings table...");

            // 3. 迁移路线头：同一 (product_id, routing_code) 视为一条路线
            cmd.CommandText = @"
INSERT INTO `routing_headers` (`product_id`, `route_code`, `route_name`, `description`, `is_active`, `sort_order`, `created_at`, `updated_at`)
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
ON DUPLICATE KEY UPDATE route_name = VALUES(route_name);";
            await cmd.ExecuteNonQueryAsync();

            // 4. 迁移步骤
            cmd.CommandText = @"
INSERT INTO `routing_steps` (`routing_header_id`, `step_order`, `process_id`, `standard_time_minutes`, `description`, `is_active`, `created_at`, `updated_at`)
SELECT rh.id, r.step_order, r.process_id, r.standard_time_minutes, r.description, r.is_active, r.created_at, r.updated_at
FROM routings r
INNER JOIN routing_headers rh
    ON r.product_id = rh.product_id
    AND r.routing_code = rh.route_code
ORDER BY r.product_id, rh.sort_order, r.step_order;";
            await cmd.ExecuteNonQueryAsync();

            cmd.CommandText = @"SELECT COUNT(*) FROM `routing_headers`";
            var headerCount = (long)(await cmd.ExecuteScalarAsync())!;
            cmd.CommandText = @"SELECT COUNT(*) FROM `routing_steps`";
            var stepCount = (long)(await cmd.ExecuteScalarAsync())!;
            Console.WriteLine($"[Program] Migration complete: {headerCount} headers, {stepCount} steps");
        }
        else
        {
            Console.WriteLine("[Program] Old routings table is empty, skipping migration.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Program] Warning: Data migration failed: {ex.Message}");
    }

    // M02.5 多工艺路线：插入测试数据（用于验证新功能）
    try
    {
        // 检查是否已有测试数据
        cmd.CommandText = @"SELECT COUNT(*) FROM `routing_headers`";
        var headerCount = (long)(await cmd.ExecuteScalarAsync())!;
        if (headerCount == 0)
        {
            Console.WriteLine("[Program] Creating test routing data...");

            // 1. 创建测试产品
            cmd.CommandText = @"INSERT IGNORE INTO `products` (`product_code`, `product_name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES ('TEST-001', '测试产品 A', '用于测试工艺路线功能', 1, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();

            // 2. 创建测试工序
            cmd.CommandText = @"INSERT IGNORE INTO `processes` (`process_code`, `process_name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES ('WX', '焊接', '焊接工序测试', 1, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = @"INSERT IGNORE INTO `processes` (`process_code`, `process_name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES ('ZZ', '组装', '组装工序测试', 1, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = @"INSERT IGNORE INTO `processes` (`process_code`, `process_name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES ('CS', '测试', '测试工序测试', 1, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = @"INSERT IGNORE INTO `processes` (`process_code`, `process_name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES ('BZ', '包装', '包装工序测试', 1, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = @"INSERT IGNORE INTO `processes` (`process_code`, `process_name`, `description`, `is_active`, `created_at`, `updated_at`) VALUES ('JY', '检验', '检验工序测试', 1, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();

            // 3. 创建测试路线头
            cmd.CommandText = @"INSERT IGNORE INTO `routing_headers` (`product_id`, `route_code`, `route_name`, `route_type`, `is_active`, `sort_order`, `created_at`, `updated_at`) VALUES (1, 'STD-001', '标准工艺路线', 'STD', 1, 0, NOW(), NOW())";
            await cmd.ExecuteNonQueryAsync();

            // 4. 获取路线头 ID 和工序 ID
            cmd.CommandText = @"SELECT id FROM `routing_headers` WHERE `route_code` = 'STD-001' LIMIT 1";
            var headerId = (long)(await cmd.ExecuteScalarAsync())!;

            var processCodes = new[] { "WX", "ZZ", "CS", "BZ", "JY" };
            foreach (var code in processCodes)
            {
                cmd.CommandText = $@"SELECT id FROM `processes` WHERE `process_code` = '{code}' LIMIT 1";
                var procId = (long)(await cmd.ExecuteScalarAsync())!;

                // 获取当前最大 step_order
                cmd.CommandText = @"SELECT COALESCE(MAX(step_order), 0) FROM `routing_steps` WHERE routing_header_id = @headerId";
                cmd.Parameters.Clear();
                // MySQL 参数需要使用 @ 前缀，但需要正确添加
                var param = cmd.CreateParameter();
                param.ParameterName = "@headerId";
                param.Value = headerId;
                cmd.Parameters.Add(param);
                var maxOrder = (long)(await cmd.ExecuteScalarAsync())!;

                // 插入步骤
                cmd.CommandText = @"INSERT INTO `routing_steps` (`routing_header_id`, `step_order`, `process_id`, `standard_time_minutes`, `is_active`, `created_at`, `updated_at`) VALUES (@headerId, @order, @processId, @time, 1, NOW(), NOW())";
                cmd.Parameters.Clear();
                var p1 = cmd.CreateParameter(); p1.ParameterName = "@headerId"; p1.Value = headerId; cmd.Parameters.Add(p1);
                var p2 = cmd.CreateParameter(); p2.ParameterName = "@order"; p2.Value = maxOrder + 1; cmd.Parameters.Add(p2);
                var p3 = cmd.CreateParameter(); p3.ParameterName = "@processId"; p3.Value = procId; cmd.Parameters.Add(p3);
                var p4 = cmd.CreateParameter(); p4.ParameterName = "@time"; p4.Value = 10.0 + (processCodes.ToList().IndexOf(code)) * 5; cmd.Parameters.Add(p4);
                await cmd.ExecuteNonQueryAsync();
            }

            cmd.CommandText = @"SELECT COUNT(*) FROM `routing_headers`";
            headerCount = (long)(await cmd.ExecuteScalarAsync())!;
            cmd.CommandText = @"SELECT COUNT(*) FROM `routing_steps`";
            var stepCount = (long)(await cmd.ExecuteScalarAsync())!;
            Console.WriteLine($"[Program] Test data created: {headerCount} headers, {stepCount} steps");
        }
        else
        {
            Console.WriteLine($"[Program] Test data already exists ({headerCount} headers).");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Program] Warning: Test data creation failed: {ex.Message}");
    }
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `inspection_standards` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `standard_code` VARCHAR(50) NOT NULL UNIQUE, `standard_name` VARCHAR(200) NOT NULL, `description` VARCHAR(500), `inspection_type` VARCHAR(10), `product_id` BIGINT, `process_id` BIGINT, `item_name` VARCHAR(200), `usl` DECIMAL(10,4), `lsl` DECIMAL(10,4), `target` DECIMAL(10,4), `unit` VARCHAR(20), `inspection_method` VARCHAR(200), `sampling_frequency` VARCHAR(200), `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`product_id`) REFERENCES `products`(`id`) ON DELETE SET NULL, FOREIGN KEY (`process_id`) REFERENCES `processes`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `defect_codes` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `defect_code` VARCHAR(50) NOT NULL UNIQUE, `defect_name` VARCHAR(200) NOT NULL, `description` VARCHAR(500), `defect_category` VARCHAR(50), `defect_severity` VARCHAR(10), `is_reworkable` TINYINT(1), `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `equipment` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `equipment_code` VARCHAR(50) NOT NULL UNIQUE, `equipment_name` VARCHAR(200) NOT NULL, `model` VARCHAR(100), `production_line` VARCHAR(100), `workshop` VARCHAR(100), `org_id` BIGINT, `workshop_id` BIGINT, `line_id` BIGINT, `status` VARCHAR(20) DEFAULT 'idle', `equipment_type` VARCHAR(50), `has_mqtt_connection` TINYINT(1) DEFAULT 0, `mqtt_topic_prefix` VARCHAR(500), `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `tools` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `tool_code` VARCHAR(50) NOT NULL UNIQUE, `tool_name` VARCHAR(200) NOT NULL, `model` VARCHAR(100), `tool_type` VARCHAR(50), `design_life` DECIMAL(10,2), `life_unit` VARCHAR(20), `life_current` DECIMAL(10,2), `supplier` VARCHAR(200), `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `suppliers` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `supplier_code` VARCHAR(50) NOT NULL UNIQUE, `supplier_name` VARCHAR(200) NOT NULL, `address` VARCHAR(500), `contact_person` VARCHAR(100), `phone` VARCHAR(50), `email` VARCHAR(200), `rating` VARCHAR(10), `supply_category` VARCHAR(100), `score` INT, `is_active` TINYINT(1) DEFAULT 1, `status` VARCHAR(20) DEFAULT 'active', `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `customers` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `customer_code` VARCHAR(50) NOT NULL UNIQUE, `customer_name` VARCHAR(200) NOT NULL, `address` VARCHAR(500), `contact_person` VARCHAR(100), `phone` VARCHAR(50), `email` VARCHAR(200), `is_active` TINYINT(1) DEFAULT 1, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    // M15 企业组织层级 & 字典
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `organizations` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `code` VARCHAR(50) NOT NULL UNIQUE, `name` VARCHAR(200) NOT NULL, `level` VARCHAR(20) NOT NULL, `parent_id` BIGINT, `sort_order` INT DEFAULT 0, `is_active` TINYINT(1) DEFAULT 1, `location` VARCHAR(500), `contact` JSON, `description` TEXT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `created_by` BIGINT, FOREIGN KEY (`parent_id`) REFERENCES `organizations`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();

    // 补建 organizations 表可能缺少的列（表在早期版本创建时可能不含这些字段）
    var orgAlterCols = new (string Col, string Def)[]
    {
        ("created_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP"),
        ("updated_at", "DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP"),
        ("contact", "JSON"),
        ("created_by", "BIGINT"),
    };
    foreach (var (col, def) in orgAlterCols)
    {
        try
        {
            cmd.CommandText = $"SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'organizations' AND COLUMN_NAME = '{col}'";
            var colExists = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            if (colExists == 0)
            {
                cmd.CommandText = $"ALTER TABLE `organizations` ADD COLUMN `{col}` {def}";
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[Program] Added missing column organizations.{col}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Program] Warning: Failed to check/add organizations.{col}: {ex.Message}");
        }
    }
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `sys_dict_types` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `type_code` VARCHAR(50) UNIQUE NOT NULL, `type_name` VARCHAR(200) NOT NULL, `is_system` TINYINT(1) DEFAULT 0, `status` TINYINT(1) DEFAULT 1, `remark` TEXT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `sys_dict_items` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `type_code` VARCHAR(50) NOT NULL, `item_label` VARCHAR(200) NOT NULL, `item_value` VARCHAR(100) NOT NULL, `sort_order` INT DEFAULT 0, `color` VARCHAR(20), `is_default` TINYINT(1) DEFAULT 0, `status` TINYINT(1) DEFAULT 1, `remark` TEXT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, INDEX `idx_dict_items_type` (`type_code`), INDEX `idx_dict_items_sort` (`type_code`, `sort_order`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    // M02.5 动态参数配置
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `param_groups` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `name` VARCHAR(200) NOT NULL, `code` VARCHAR(50) NOT NULL UNIQUE, `description` TEXT, `sort_order` INT DEFAULT 0, `is_active` TINYINT(1) DEFAULT 1, `created_by` BIGINT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `dynamic_params` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `group_id` BIGINT NOT NULL, `name` VARCHAR(200) NOT NULL, `code` VARCHAR(50) NOT NULL UNIQUE, `data_type` VARCHAR(20) NOT NULL, `unit` VARCHAR(20), `target_value` DECIMAL(15,6), `usl` DECIMAL(10,4), `lsl` DECIMAL(10,4), `precision` DECIMAL(10,4), `ai_strategy` TEXT, `sort_order` INT DEFAULT 0, `is_active` TINYINT(1) DEFAULT 1, `created_by` BIGINT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (`group_id`) REFERENCES `param_groups`(`id`) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `closure_rules` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `name` VARCHAR(200) NOT NULL, `code` VARCHAR(50) NOT NULL UNIQUE, `condition_json` TEXT, `logic` VARCHAR(10), `description` TEXT, `is_active` TINYINT(1) DEFAULT 1, `created_by` BIGINT, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `param_realtime_values` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `param_code` VARCHAR(50) NOT NULL, `equipment_id` BIGINT, `value` DECIMAL(10,4), `value_raw` VARCHAR(100), `quality_result` VARCHAR(10), `is_active` TINYINT(1) DEFAULT 1, `timestamp` DATETIME NOT NULL, `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, INDEX `idx_param_timestamp` (`param_code`, `timestamp`), INDEX `idx_equip_timestamp` (`equipment_id`, `timestamp`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    // M02.1 检验项目主数据
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `inspection_items` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `item_code` VARCHAR(50) NOT NULL UNIQUE, `item_name` VARCHAR(200) NOT NULL, `description` TEXT, `data_type` VARCHAR(20), `unit` VARCHAR(20), `usl` DECIMAL(10,4), `lsl` DECIMAL(10,4), `target_value` DECIMAL(10,4), `chart_type` VARCHAR(20), `subgroup_size` INT, `inspection_method` VARCHAR(200), `sample_size` INT, `is_active` TINYINT(1) DEFAULT 1, `created_by` BIGINT, INDEX `idx_item_code` (`item_code`), INDEX `idx_is_active` (`is_active`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `inspection_plans` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `plan_code` VARCHAR(50) NOT NULL UNIQUE, `plan_name` VARCHAR(200) NOT NULL, `inspection_type` VARCHAR(10), `description` TEXT, `product_id` BIGINT, `material_id` BIGINT, `supplier_id` BIGINT, `customer_id` BIGINT, `process_id` BIGINT, `equipment_id` BIGINT, `is_active` TINYINT(1) DEFAULT 1, `created_by` BIGINT, FOREIGN KEY (`product_id`) REFERENCES `products`(`id`) ON DELETE SET NULL, FOREIGN KEY (`supplier_id`) REFERENCES `suppliers`(`id`) ON DELETE SET NULL, FOREIGN KEY (`customer_id`) REFERENCES `customers`(`id`) ON DELETE SET NULL, FOREIGN KEY (`process_id`) REFERENCES `processes`(`id`) ON DELETE SET NULL, FOREIGN KEY (`equipment_id`) REFERENCES `equipment`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `inspection_plan_items` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `plan_id` BIGINT NOT NULL, `inspection_item_id` BIGINT NOT NULL, `sort_order` INT DEFAULT 0, `is_required` TINYINT(1) DEFAULT 1, FOREIGN KEY (`plan_id`) REFERENCES `inspection_plans`(`id`) ON DELETE CASCADE, FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items`(`id`) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    // M03 IQC 来料检验
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `iqc_receipts` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `receipt_no` VARCHAR(50) NOT NULL UNIQUE, `supplier_id` BIGINT, `product_id` BIGINT, `batch_no` VARCHAR(100), `quantity` DECIMAL(10,2), `unit` VARCHAR(20), `receipt_date` DATETIME, `inspector` VARCHAR(100), `status` VARCHAR(20) DEFAULT 'pending', FOREIGN KEY (`supplier_id`) REFERENCES `suppliers`(`id`) ON DELETE SET NULL, FOREIGN KEY (`product_id`) REFERENCES `products`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `iqc_inspections` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `inspection_no` VARCHAR(50) NOT NULL UNIQUE, `receipt_id` BIGINT NOT NULL, `sample_size` INT, `ac` INT, `re` INT, `defect_qty` INT, `sampling_level` VARCHAR(10), `aql_value` DECIMAL(5,2), `result` VARCHAR(10), `inspector` VARCHAR(100), `inspected_at` DATETIME, FOREIGN KEY (`receipt_id`) REFERENCES `iqc_receipts`(`id`) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `iqc_inspection_items` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `inspection_id` BIGINT NOT NULL, `inspection_item_id` BIGINT, `item_name` VARCHAR(200), `measured_value` DECIMAL(10,4), `usl` DECIMAL(10,4), `lsl` DECIMAL(10,4), `result` VARCHAR(10), `defect_code_id` BIGINT, FOREIGN KEY (`inspection_id`) REFERENCES `iqc_inspections`(`id`) ON DELETE CASCADE, FOREIGN KEY (`defect_code_id`) REFERENCES `defect_codes`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `iqc_anomalies` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `anomaly_no` VARCHAR(50) NOT NULL UNIQUE, `receipt_id` BIGINT, `inspection_id` BIGINT, `anomaly_type` VARCHAR(20), `severity` VARCHAR(10), `description` TEXT, `status` VARCHAR(20) DEFAULT 'open', FOREIGN KEY (`receipt_id`) REFERENCES `iqc_receipts`(`id`) ON DELETE SET NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();
    
    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS `supplier_scores` (`id` BIGINT AUTO_INCREMENT PRIMARY KEY, `supplier_id` BIGINT NOT NULL, `score` INT, `assessment_date` DATETIME, FOREIGN KEY (`supplier_id`) REFERENCES `suppliers`(`id`) ON DELETE CASCADE) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
    await cmd.ExecuteNonQueryAsync();

    await DbInitializer.Initialize(context);
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// NOTE: JwtMiddleware must run BEFORE UseAuthorization so that JWT validation
// (extracting user claims) happens before controller authorization checks.
app.UseAuthentication();
app.UseMiddleware<JwtMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

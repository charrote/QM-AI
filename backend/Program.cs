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

    // 自动创建新增的表（organizations, sys_dict_types, sys_dict_items）
    // 使用 EF Core 的默认 PascalCase 列命名
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS organizations (
            id              BIGINT AUTO_INCREMENT PRIMARY KEY,
            code            VARCHAR(50) NOT NULL,
            name            VARCHAR(200) NOT NULL,
            level           VARCHAR(20) NOT NULL,
            parent_id       BIGINT,
            sort_order      INT DEFAULT 0,
            is_active       TINYINT(1) DEFAULT 1,
            location        VARCHAR(500),
            contact         JSON,
            description     TEXT,
            created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            created_by      BIGINT,
            UNIQUE KEY uk_org_code (code),
            INDEX idx_org_parent (parent_id),
            INDEX idx_org_level (level)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS sys_dict_types (
            id              BIGINT AUTO_INCREMENT PRIMARY KEY,
            type_code       VARCHAR(50) UNIQUE NOT NULL,
            type_name       VARCHAR(200) NOT NULL,
            is_system       TINYINT(1) DEFAULT 0,
            status          TINYINT(1) DEFAULT 1,
            remark          TEXT,
            created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS sys_dict_items (
            id              BIGINT AUTO_INCREMENT PRIMARY KEY,
            type_code       VARCHAR(50) NOT NULL,
            item_label      VARCHAR(200) NOT NULL,
            item_value      VARCHAR(100) NOT NULL,
            sort_order      INT DEFAULT 0,
            color           VARCHAR(20),
            is_default      TINYINT(1) DEFAULT 0,
            status          TINYINT(1) DEFAULT 1,
            remark          TEXT,
            created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            INDEX idx_dict_items_type (type_code),
            INDEX idx_dict_items_sort (type_code, sort_order)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

    // 如果 organizations 表是新创建的，添加外键约束
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE organizations
            ADD CONSTRAINT fk_org_parent
            FOREIGN KEY (parent_id) REFERENCES organizations(id) ON DELETE SET NULL");
    }
    catch { /* 约束可能已存在 */ }

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

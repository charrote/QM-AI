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

// ─── 全局异常处理中间件 ─────────────────────────────────────────
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(contextFeature.Error, "Unhandled exception: {Message}", contextFeature.Error.Message);

            await context.Response.WriteAsJsonAsync(new
            {
                message = "服务器内部错误",
                detail = app.Environment.IsDevelopment() ? contextFeature.Error.Message : null,
            });
        }
    });
});

// 启动时自动初始化种子数据
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // 自动创建新增的表（organizations, sys_dict_types, sys_dict_items）
    // 使用 EF Core 的默认 PascalCase 列命名
    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS organizations (
            Id BIGINT AUTO_INCREMENT PRIMARY KEY,
            Code VARCHAR(50) NOT NULL,
            Name VARCHAR(200) NOT NULL,
            Level VARCHAR(20) NOT NULL,
            ParentId BIGINT,
            SortOrder INT DEFAULT 0,
            IsActive TINYINT(1) DEFAULT 1,
            Location VARCHAR(500),
            Contact JSON,
            Description TEXT,
            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            CreatedBy BIGINT,
            UNIQUE KEY uk_org_code (Code),
            INDEX idx_org_parent (ParentId),
            INDEX idx_org_level (Level)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS sys_dict_types (
            Id BIGINT AUTO_INCREMENT PRIMARY KEY,
            TypeCode VARCHAR(50) UNIQUE NOT NULL,
            TypeName VARCHAR(200) NOT NULL,
            IsSystem TINYINT(1) DEFAULT 0,
            Status TINYINT(1) DEFAULT 1,
            Remark TEXT,
            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

    await context.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS sys_dict_items (
            Id BIGINT AUTO_INCREMENT PRIMARY KEY,
            TypeCode VARCHAR(50) NOT NULL,
            ItemLabel VARCHAR(200) NOT NULL,
            ItemValue VARCHAR(100) NOT NULL,
            SortOrder INT DEFAULT 0,
            Color VARCHAR(20),
            IsDefault TINYINT(1) DEFAULT 0,
            Status TINYINT(1) DEFAULT 1,
            Remark TEXT,
            CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            INDEX idx_dict_items_type (TypeCode),
            INDEX idx_dict_items_sort (TypeCode, SortOrder)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci");

    // 如果 organizations 表是新创建的，添加外键约束
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            ALTER TABLE organizations
            ADD CONSTRAINT fk_org_parent
            FOREIGN KEY (ParentId) REFERENCES organizations(Id) ON DELETE SET NULL");
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

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();

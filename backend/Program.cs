using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QM_AI.API.Authorization;
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
        options.UseMySql(connectionString, serverVersion,
            mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null));
    }
    catch
    {
        options.UseMySql(connectionString, ServerVersion.Parse("8.0.0"),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null));
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
// 注册动态策略提供器和权限处理器
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PermissionHandler>();

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
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // 使用 camelCase 序列化，与前端 TypeScript 命名风格一致
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DataScopeService>();
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
builder.Services.AddScoped<D8ReportService>();

// M11 设备联动
builder.Services.AddScoped<EquipmentLinkService>();

// M12 文件管理
builder.Services.AddScoped<DocumentService>();

// M13 审核稽核
builder.Services.AddScoped<AuditService>();

// M14 报表 / 驾驶舱
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<AiAlertService>();

var app = builder.Build();

// ─── Global exception handler middleware ────────────────────────────
app.UseMiddleware<AppExceptionHandlerMiddleware>();

// 启动时自动初始化种子数据（容错：seed 失败不影响服务启动）
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // ─── 1. 业务种子（含角色/用户/组织/检验数据等） ────────────────
    try
    {
        // 自动创建缺失的表（EF Core 自动迁移）
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("[DB] Schema ensured (tables created if missing)");
        await DbInitializer.Initialize(context);
        Console.WriteLine("[Seed] 业务种子数据初始化完成");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed] 业务种子初始化失败（不影响服务运行）: {ex.Message}");
    }

    // ─── 2. RBAC 种子（独立调用，F1 修复） ─────────────────────────
    // 关键：独立于业务种子的 try 块，确保业务种子 FK 异常不会导致 RBAC 被跳过。
    // 不放在 DbInitializer.Initialize 内部，避免 hasRoles 交互问题。
    try
    {
        await RbacSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Seed] RBAC 种子初始化失败（不影响服务运行）: {ex.Message}");
    }
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

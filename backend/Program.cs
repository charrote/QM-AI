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

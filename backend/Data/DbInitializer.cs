using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;
using QM_AI.API.Services;

namespace QM_AI.API.Data;

/// <summary>
/// 数据库初始化器 — 启动时自动创建种子数据
/// </summary>
public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        // 确保数据库已创建
        await context.Database.EnsureCreatedAsync();

        // 已有数据则跳过
        if (await context.Users.AnyAsync())
            return;

        // ─── 角色 ────────────────────────────────────────────────
        var adminRole = new Role
        {
            Name = "Administrator",
            Description = "系统管理员，拥有全部权限"
        };
        var operatorRole = new Role
        {
            Name = "Operator",
            Description = "质检操作员"
        };
        var inspectorRole = new Role
        {
            Name = "Inspector",
            Description = "质量检验员"
        };
        var engineerRole = new Role
        {
            Name = "Engineer",
            Description = "质量工程师"
        };

        context.Roles.AddRange(adminRole, operatorRole, inspectorRole, engineerRole);
        await context.SaveChangesAsync();

        // ─── 权限 ────────────────────────────────────────────────
        var permissions = new List<Permission>
        {
            new() { Name = "全部权限", Code = "*:*", Module = "System" },
            new() { Name = "用户管理", Code = "system:user", Module = "M15" },
            new() { Name = "角色管理", Code = "system:role", Module = "M15" },
            new() { Name = "基础数据管理", Code = "basic:data", Module = "M02" },
            new() { Name = "IQC 检验", Code = "iqc:inspect", Module = "M03" },
            new() { Name = "IPQC 检验", Code = "ipqc:inspect", Module = "M04" },
            new() { Name = "FQC 检验", Code = "fqc:inspect", Module = "M05" },
            new() { Name = "SPC 查看", Code = "spc:view", Module = "M06" },
            new() { Name = "不良管理", Code = "defect:manage", Module = "M07" },
            new() { Name = "质量追溯", Code = "trace:view", Module = "M08" },
            new() { Name = "AI 分析", Code = "ai:analyze", Module = "M10" },
            new() { Name = "报表查看", Code = "report:view", Module = "M14" },
        };
        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // ─── 管理员用户 ───────────────────────────────────────────
        var authService = new AuthService(context, new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "QM-AI-SuperSecretKey-2024-MustBeAtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "QM-AI.API",
                ["Jwt:Audience"] = "QM-AI.Client",
                ["Jwt:ExpirationHours"] = "24"
            }!)
            .Build());

        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = authService.HashPassword("admin123"),
            DisplayName = "系统管理员",
            Avatar = "",
            Email = "admin@qm-ai.com",
            IsActive = true,
            RoleId = adminRole.Id
        };

        var operatorUser = new User
        {
            Username = "operator",
            PasswordHash = authService.HashPassword("operator123"),
            DisplayName = "质检操作员",
            Email = "operator@qm-ai.com",
            IsActive = true,
            RoleId = operatorRole.Id
        };

        var inspectorUser = new User
        {
            Username = "inspector",
            PasswordHash = authService.HashPassword("inspector123"),
            DisplayName = "质量检验员",
            Email = "inspector@qm-ai.com",
            IsActive = true,
            RoleId = inspectorRole.Id
        };

        context.Users.AddRange(adminUser, operatorUser, inspectorUser);
        await context.SaveChangesAsync();
    }
}

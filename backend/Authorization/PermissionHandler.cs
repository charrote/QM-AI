using Microsoft.AspNetCore.Authorization;

namespace QM_AI.API.Authorization;

/// <summary>
/// 权限处理器 — 从 HttpContext.Items["Permissions"] 读取每请求权限快照
/// 支持 "*:*" 通配（管理员）
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // 在 MVC 中, context.Resource 是 HttpContext
        var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
        var permissions = httpContext?.Items["Permissions"] as HashSet<string>
            ?? new HashSet<string>();

        if (permissions.Contains("*:*") || permissions.Contains(requirement.PermissionCode))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

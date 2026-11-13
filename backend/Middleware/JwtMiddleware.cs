using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models;

namespace QM_AI.API.Middleware;

/// <summary>
/// JWT 中间件 — 每请求加载用户权限快照到 HttpContext.Items["Permissions"]
/// 动态调整免重登：权限变更下次请求即生效
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, Services.AuthService authService, AppDbContext db)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var principal = authService.ValidateToken(token);
            if (principal != null)
            {
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                                 ?? principal.FindFirst("sub");

                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                {
                    // 直接从 DB 加载用户（含 Role），避免依赖 UserInfoDto
                    var user = await db.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user != null)
                    {
                        context.Items["User"] = user;
                        context.Items["UserId"] = userId;

                        // ─── 每请求加载权限快照（动态生效核心） ────────
                        var permissions = new HashSet<string>();
                        if (user.RoleId > 0)
                        {
                            var permCodes = await db.RolePermissions
                                .Where(rp => rp.RoleId == user.RoleId)
                                .Select(rp => rp.Permission.Code)
                                .ToListAsync();
                            permissions.UnionWith(permCodes);
                        }

                        context.Items["Permissions"] = permissions;
                    }
                }
            }
        }

        await _next(context);
    }
}

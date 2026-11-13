using Microsoft.AspNetCore.Authorization;

namespace QM_AI.API.Authorization;

/// <summary>
/// 动态策略提供器 — 为每个权限码自动生成同名 Policy。
/// 注：F4 修复 — 使用 non-nullable 标注消除 CS8613/CS8619 警告。
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public Task<AuthorizationPolicy?> GetDefaultPolicyAsync()
        => Task.FromResult<AuthorizationPolicy?>(null);

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => Task.FromResult<AuthorizationPolicy?>(null);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // 权限码目录内的 → 动态生成 PermissionRequirement policy
        if (PermissionCatalog.Codes.Contains(policyName))
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return Task.FromResult<AuthorizationPolicy?>(null);
    }
}

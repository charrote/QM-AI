using Microsoft.AspNetCore.Authorization;

namespace QM_AI.API.Authorization;

/// <summary>
/// 权限点校验要求 — Policy 名称 = 权限码
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}

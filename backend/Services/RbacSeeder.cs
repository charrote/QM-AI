using Microsoft.EntityFrameworkCore;
using QM_AI.API.Authorization;
using QM_AI.API.Data;
using QM_AI.API.Models;

namespace QM_AI.API.Services;

/// <summary>
/// RBAC 种子 — 独立于业务种子运行（F1 修复）。
/// 在 Program.cs 中 DbInitializer.Initialize 完成后独立调用，
/// 确保业务种子 FK 异常不会导致 RBAC 种子被跳过。
///
/// 幂等设计：
/// - 权限：按 Code 去重，仅新增缺失项
/// - 角色：按 Name 去重，已存在的更新 DataScope
/// - 角色-权限关联：仅补全缺失的组合
/// </summary>
public static class RbacSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        Console.WriteLine("[RbacSeeder] 开始 RBAC 种子...");

        // ─── 1. 确保 27 个权限点全部存在 ──────────────────────────
        var existingPermCodes = await context.Permissions.Select(p => p.Code).ToHashSetAsync();
        var newPermissions = new List<Permission>();

        foreach (var (code, name, module) in PermissionCatalog.All)
        {
            if (!existingPermCodes.Contains(code))
            {
                newPermissions.Add(new Permission { Code = code, Name = name, Module = module });
            }
        }

        if (newPermissions.Count > 0)
        {
            context.Permissions.AddRange(newPermissions);
            await context.SaveChangesAsync();
            Console.WriteLine($"[RbacSeeder] 新增 {newPermissions.Count} 个权限点");
        }

        // 重新加载（获取 ID）
        var allPerms = await context.Permissions.ToListAsync();
        var permByCode = allPerms.ToDictionary(p => p.Code);

        // ─── 2. 确保 5 个预置角色存在 ─────────────────────────────
        // (Name, Description, DataScope, 应关联的权限码)
        var roleDefs = new (string Name, string Desc, string Scope, string[] Perms)[]
        {
            ("Administrator", "系统管理员，拥有全部权限", "all", new[] { "*:*" }),
            ("Supervisor",    "质量主管/负责人，全部门数据+分析+导出", "all",
                new[] { "report:view","report:export","system:log","workflow:approve","workflow:manage",
                    "form:view","form:manage","spc:view","defect:manage","trace:view","complaint:manage",
                    "ai:analyze","document:manage","audit:manage","basic:data","iqc:inspect","ipqc:inspect","fqc:inspect" }),
            ("Engineer",      "质量工程师，本组数据+审批+导出", "group",
                new[] { "report:view","report:export","workflow:approve","form:view","form:fill",
                    "spc:view","defect:manage","trace:view","complaint:manage","iqc:inspect","ipqc:inspect","fqc:inspect" }),
            ("Inspector",     "质检员/检验员，仅本人数据", "self",
                new[] { "iqc:inspect","ipqc:inspect","fqc:inspect","form:view","form:fill","report:view" }),
            ("Operator",      "操作员，仅本人填报", "self",
                new[] { "ipqc:inspect","form:view","form:fill" }),
        };

        var existingRoles = await context.Roles.ToDictionaryAsync(r => r.Name);
        var newRoles = new List<Role>();

        foreach (var (name, desc, scope, _) in roleDefs)
        {
            if (existingRoles.TryGetValue(name, out var existing))
            {
                // 更新 DataScope（如果之前是 NULL）
                if (existing.DataScope == null)
                {
                    existing.DataScope = scope;
                    existing.Description = desc;
                }
            }
            else
            {
                var role = new Role { Name = name, Description = desc, DataScope = scope };
                newRoles.Add(role);
                existingRoles[name] = role;
            }
        }

        if (newRoles.Count > 0)
        {
            context.Roles.AddRange(newRoles);
            await context.SaveChangesAsync();
            Console.WriteLine($"[RbacSeeder] 新增 {newRoles.Count} 个预置角色");
        }
        else
        {
            await context.SaveChangesAsync(); // 保存 DataScope 更新
        }

        // ─── 3. 补全角色-权限关联 ─────────────────────────────────
        var allRolePerms = await context.RolePermissions.ToListAsync();
        var existingLinks = allRolePerms.Select(rp => (rp.RoleId, rp.PermissionId)).ToHashSet();

        var newLinks = new List<RolePermission>();

        foreach (var (name, _, _, permCodes) in roleDefs)
        {
            var role = existingRoles[name];
            foreach (var code in permCodes)
            {
                if (permByCode.TryGetValue(code, out var perm))
                {
                    var key = (role.Id, perm.Id);
                    if (!existingLinks.Contains(key))
                    {
                        newLinks.Add(new RolePermission { RoleId = role.Id, PermissionId = perm.Id });
                        existingLinks.Add(key);
                    }
                }
            }
        }

        if (newLinks.Count > 0)
        {
            context.RolePermissions.AddRange(newLinks);
            await context.SaveChangesAsync();
            Console.WriteLine($"[RbacSeeder] 新增 {newLinks.Count} 条角色-权限关联");
        }

        Console.WriteLine($"[RbacSeeder] RBAC 种子完成: {allPerms.Count} 权限, {existingRoles.Count} 角色, {existingLinks.Count} 关联");
    }
}

using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models;

namespace QM_AI.API.Services;

/// <summary>
/// 数据范围服务 — 根据角色的 DataScope 字段返回可见的用户 ID 集合
/// self: 仅本人
/// group: 所在组织及其全部下级组织的成员
/// all: 不限制
/// </summary>
public class DataScopeService
{
    private readonly AppDbContext _db;

    public DataScopeService(AppDbContext db) => _db = db;

    /// <summary>
    /// 返回当前用户可见的 UserId 集合（null = 不限制/全部）
    /// </summary>
    public async Task<HashSet<long>?> GetVisibleUserIdsAsync(User user)
    {
        if (user.Role == null)
            return new HashSet<long> { user.Id }; // 无角色 → 仅本人

        var scope = user.Role.DataScope ?? "self";

        return scope switch
        {
            "all" => null, // 不限制
            "group" => await GetGroupUserIdsAsync(user),
            _ => new HashSet<long> { user.Id }, // "self" 或默认
        };
    }

    /// <summary>
    /// 获取本组（所在组织 + 全部下级组织）的成员 ID 集合
    /// </summary>
    private async Task<HashSet<long>> GetGroupUserIdsAsync(User user)
    {
        var myOrgId = user.OrganizationId;
        if (myOrgId == null)
            return new HashSet<long> { user.Id };

        // 递归获取组织子树
        var orgIds = await GetOrgSubtreeIdsAsync(myOrgId.Value);
        orgIds.Add(myOrgId.Value);

        var userIds = await _db.Users
            .Where(u => u.OrganizationId != null && orgIds.Contains(u.OrganizationId.Value))
            .Select(u => u.Id)
            .ToHashSetAsync();

        // 确保包含本人
        userIds.Add(user.Id);
        return userIds;
    }

    /// <summary>
    /// 递归获取组织子树的所有 ID
    /// </summary>
    private async Task<HashSet<long>> GetOrgSubtreeIdsAsync(long orgId)
    {
        var result = new HashSet<long>();
        var queue = new Queue<long>();
        queue.Enqueue(orgId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = await _db.Organizations
                .Where(o => o.ParentId == current)
                .Select(o => o.Id)
                .ToListAsync();

            foreach (var childId in children)
            {
                if (result.Add(childId))
                    queue.Enqueue(childId);
            }
        }

        return result;
    }
}

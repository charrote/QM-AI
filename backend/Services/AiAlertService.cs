using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M10;

namespace QM_AI.API.Services;

/// <summary>
/// M10 AI 预警服务：分页查询 / 标记已解决 / 统计。
/// 数据来源：ai_alerts 表（由 migration_m10_ai_alerts.sql 建表 + 种子数据）。
/// </summary>
public class AiAlertService
{
    private readonly AppDbContext _db;

    public AiAlertService(AppDbContext db) => _db = db;

    public async Task<(List<AiAlert> Items, int Total)> GetAlertsAsync(
        string? level, bool? resolved, int page = 1, int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.AiAlerts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(level))
            query = query.Where(a => a.Level == level);
        if (resolved.HasValue)
            query = query.Where(a => a.Resolved == resolved.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> ResolveAlertAsync(long id, long userId)
    {
        var alert = await _db.AiAlerts.FindAsync(id);
        if (alert == null || alert.Resolved) return false;

        alert.Resolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedBy = userId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<object> GetAlertStatsAsync()
    {
        var all = await _db.AiAlerts.AsNoTracking().ToListAsync();
        var unresolved = all.Where(a => !a.Resolved).ToList();

        return new
        {
            totalAlerts = all.Count,
            unresolvedAlerts = unresolved.Count,
            byLevel = unresolved.GroupBy(a => a.Level)
                .ToDictionary(g => g.Key, g => g.Count()),
            bySource = unresolved.GroupBy(a => a.Source)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}

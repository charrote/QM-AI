using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs.M14;

namespace QM_AI.API.Services;

/// <summary>
/// M14 报表服务：质量统计 / 缺陷帕累托 / 质量趋势。
/// 全部基于真实表结构查库聚合（已 inspect 各表列与状态取值）：
///   - iqc_inspection_items.result ∈ pass/fail（83 行，自带 created_at）
///   - fqc_inspection_items.result ∈ pass/fail（42 行，无时间列 → 按父单 fqc_inspections.created_at 过滤）
///   - ipqc_first_piece_items / ipqc_patrol_items.result ∈ pass/fail/pending
///   - defects.discovered_at、scrap_rework_records.type ∈ scrap/rework
/// </summary>
public class ReportsService
{
    private readonly AppDbContext _db;

    public ReportsService(AppDbContext db)
    {
        _db = db;
    }

    // ─── 工具 ─────────────────────────────────────────────────

    private static (DateTime? start, DateTime? end) ParseRange(string? startDate, string? endDate)
    {
        DateTime? start = null;
        DateTime? end = null;
        if (!string.IsNullOrWhiteSpace(startDate) && DateTime.TryParse(startDate, out var s))
            start = s.Date;
        if (!string.IsNullOrWhiteSpace(endDate) && DateTime.TryParse(endDate, out var e))
            end = e.Date.AddDays(1).AddSeconds(-1);
        if (start != null && end != null && start > end)
            return (null, null);
        return (start, end);
    }

    private static decimal Pct(long part, long whole) =>
        whole <= 0 ? 0m : Math.Round(part * 100m / whole, 2, MidpointRounding.AwayFromZero);

    // ─── 质量统计 ─────────────────────────────────────────────

    public async Task<QualityStatsDto> GetQualityStatsAsync(string? startDate, string? endDate)
    {
        var (start, end) = ParseRange(startDate, endDate);

        // IQC 检验项（已判定：pass/fail；pending 未判定不计入分母）
        // 实体未映射 created_at 列，按父检验单 created_at 过滤（与 FQC 口径一致）
        var iqcItems = _db.IqcInspectionItems
            .Where(i => i.Result == "pass" || i.Result == "fail");
        if (start != null) iqcItems = iqcItems.Where(i => i.Inspection.CreatedAt >= start);
        if (end != null) iqcItems = iqcItems.Where(i => i.Inspection.CreatedAt <= end);
        var iqcResults = await iqcItems.Select(i => i.Result).ToListAsync();
        var iqcTotal = iqcResults.Count;
        var iqcPass = iqcResults.Count(r => r == "pass");

        // FQC 检验项（表无时间列，按父检验单 created_at 过滤）
        var fqcItems = _db.FqcInspectionItems
            .Where(i => i.Result == "pass" || i.Result == "fail");
        if (start != null) fqcItems = fqcItems.Where(i => i.Inspection.CreatedAt >= start);
        if (end != null) fqcItems = fqcItems.Where(i => i.Inspection.CreatedAt <= end);
        var fqcResults = await fqcItems.Select(i => i.Result).ToListAsync();
        var fqcTotal = fqcResults.Count;
        var fqcPass = fqcResults.Count(r => r == "pass");

        // IPQC 检验项（首件 + 巡检）
        var fpItems = _db.IpqcFirstPieceItems
            .Where(i => i.Result == "pass" || i.Result == "fail");
        if (start != null) fpItems = fpItems.Where(i => i.FirstPiece.CreatedAt >= start);
        if (end != null) fpItems = fpItems.Where(i => i.FirstPiece.CreatedAt <= end);
        var fpResults = await fpItems.Select(i => i.Result).ToListAsync();

        var patrolItems = _db.IpqcPatrolItems
            .Where(i => i.Result == "pass" || i.Result == "fail");
        if (start != null) patrolItems = patrolItems.Where(i => i.Patrol.CreatedAt >= start);
        if (end != null) patrolItems = patrolItems.Where(i => i.Patrol.CreatedAt <= end);
        var patrolResults = await patrolItems.Select(i => i.Result).ToListAsync();

        var ipqcTotal = fpResults.Count + patrolResults.Count;
        var ipqcPass = fpResults.Count(r => r == "pass") + patrolResults.Count(r => r == "pass");

        // 缺陷总数（按发现时间过滤）
        var defectQuery = _db.Defects.AsQueryable();
        if (start != null) defectQuery = defectQuery.Where(d => d.DiscoveredAt >= start);
        if (end != null) defectQuery = defectQuery.Where(d => d.DiscoveredAt <= end);
        var totalDefects = await defectQuery.CountAsync();

        // 报废 / 返工：scrap_rework_records 按 type 汇总数量，
        // 各占（报废+返工）处置总量的比例；无记录时返回 0
        var scrapQuery = _db.ScrapReworkRecords.AsQueryable();
        if (start != null) scrapQuery = scrapQuery.Where(r => r.CreatedAt >= start);
        if (end != null) scrapQuery = scrapQuery.Where(r => r.CreatedAt <= end);
        var byType = await scrapQuery
            .GroupBy(r => r.Type)
            .Select(g => new { g.Key, Qty = g.Sum(r => r.Quantity) })
            .ToListAsync();
        var scrapQty = byType.Where(x => x.Key == "scrap").Sum(x => x.Qty);
        var reworkQty = byType.Where(x => x.Key == "rework").Sum(x => x.Qty);
        var dispositionTotal = scrapQty + reworkQty;

        var totalInspections = iqcTotal + fqcTotal;
        var passItemCount = iqcPass + fqcPass;

        return new QualityStatsDto
        {
            TotalInspections = totalInspections,
            IqcItemCount = iqcTotal,
            FqcItemCount = fqcTotal,
            PassItemCount = passItemCount,
            Fpy = Pct(passItemCount, totalInspections),
            IqcPassRate = Pct(iqcPass, iqcTotal),
            IpqcPassRate = Pct(ipqcPass, ipqcTotal),
            FqcPassRate = Pct(fqcPass, fqcTotal),
            ScrapRate = Pct((long)scrapQty, (long)dispositionTotal),
            ReworkRate = Pct((long)reworkQty, (long)dispositionTotal),
            TotalDefects = totalDefects
        };
    }

    // ─── 缺陷帕累托 ───────────────────────────────────────────

    public async Task<List<DefectParetoItemDto>> GetDefectParetoAsync(string? startDate, string? endDate)
    {
        var (start, end) = ParseRange(startDate, endDate);

        var query = _db.Defects.AsQueryable();
        if (start != null) query = query.Where(d => d.DiscoveredAt >= start);
        if (end != null) query = query.Where(d => d.DiscoveredAt <= end);

        // 按缺陷代码聚合（defects.defect_no）。
        // 说明：当前 defects 表未建 defect_codes 外键（仅 iqc_inspection_items.defect_code_id
        // 关联且全为 NULL），故名称用 defect_codes 字典做 LEFT JOIN 语义匹配，
        // 匹配不到时回退显示代码本身；待数据模型补上缺陷代码外键后名称会自动生效。
        var groups = await query
            .GroupBy(d => d.DefectCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .ToListAsync();

        var total = groups.Sum(g => g.Count);
        var codeNameMap = await _db.DefectCodes
            .ToDictionaryAsync(c => c.Code, c => c.Name);

        return groups
            .Select(g => new DefectParetoItemDto
            {
                DefectCode = g.Code,
                DefectName = codeNameMap.TryGetValue(g.Code, out var name) ? name : g.Code,
                Count = g.Count,
                Percentage = Pct(g.Count, total)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.DefectCode)
            .ToList();
    }

    // ─── 质量趋势（近 N 天按日聚合） ───────────────────────────

    public async Task<List<QualityTrendPointDto>> GetQualityTrendAsync(int days = 7)
    {
        days = Math.Clamp(days, 1, 30);
        var today = DateTime.Today;
        var windowStart = today.AddDays(-(days - 1));
        var windowEnd = today.AddDays(1);

        // 一次性取回窗口内的已判定检验项，按日在内存聚合（数据量小，避免 N 次查询）
        // IQC 项按父检验单 created_at 归日（实体未映射项级 created_at）
        var iqc = await _db.IqcInspectionItems
            .Where(i => i.Result == "pass" || i.Result == "fail")
            .Where(i => i.Inspection.CreatedAt >= windowStart && i.Inspection.CreatedAt < windowEnd)
            .Select(i => new { Date = i.Inspection.CreatedAt, i.Result })
            .ToListAsync();

        var fqc = await _db.FqcInspectionItems
            .Where(i => i.Result == "pass" || i.Result == "fail")
            .Where(i => i.Inspection.CreatedAt >= windowStart && i.Inspection.CreatedAt < windowEnd)
            .Select(i => new { Date = i.Inspection.CreatedAt, i.Result })
            .ToListAsync();

        var map = new Dictionary<DateTime, (int total, int pass)>();
        void Add(DateTime date, string result)
        {
            var key = date.Date;
            map.TryGetValue(key, out var v);
            map[key] = (v.total + 1, v.pass + (result == "pass" ? 1 : 0));
        }

        foreach (var x in iqc) Add(x.Date, x.Result);
        foreach (var x in fqc) Add(x.Date, x.Result);

        var points = new List<QualityTrendPointDto>();
        for (var d = windowStart; d < windowEnd; d = d.AddDays(1))
        {
            map.TryGetValue(d, out var v);
            points.Add(new QualityTrendPointDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                Total = v.total,
                Pass = v.pass,
                // 当日无数据 → fpy = null（稀疏如实留空，前端不填充假数据）
                Fpy = v.total > 0 ? Math.Round(v.pass * 100m / v.total, 2, MidpointRounding.AwayFromZero) : null
            });
        }
        return points;
    }
}

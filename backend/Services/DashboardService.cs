using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs.M14;

namespace QM_AI.API.Services;

/// <summary>
/// M14 驾驶舱服务：「今日待处理」聚合。
/// 数据来源与真实状态取值（已 inspect 数据库）：
///   - iqc_inspections.result = pending/空        → 待判定
///   - fqc_inspections.conclusion = unqualified   → 紧急 / pending → 处理中
///   - defects.status ∈ open/investigating/in_progress（未关闭）
///   - capa.status ∉ closed/cancelled（未完成）
///   - complaints.status ∉ closed/completed（未关闭）
///   - ipqc_first_pieces：conclusion=qualified 且未允许量产 → 待放行
/// 注：M10 AI 预警（ai_alerts 表）当前尚未建表（M10 端点为桩），该来源暂缺。
/// </summary>
public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        return s!.Length <= max ? s! : s![..max] + "…";
    }

    /// <summary>归一化截止日期到 DateOnly（Capa.DueDate 为 DateTime?，Complaint.DueDate 为 DateOnly?）</summary>
    private static DateOnly? ToDateOnly(DateTime? dt) => dt == null ? null : DateOnly.FromDateTime(dt.Value.Date);

    public async Task<(List<PendingItemDto> Items, int Total)> GetPendingAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var list = new List<PendingItemDto>();

        // 1) IQC 来料检验：结果未判定
        var iqcPending = await _db.IqcInspections
            .Where(i => i.Result == null || i.Result == "" || i.Result == "pending")
            .AsNoTracking()
            .ToListAsync();
        foreach (var i in iqcPending)
        {
            list.Add(new PendingItemDto
            {
                Id = i.Id,
                Type = "iqc",
                Title = $"IQC 来料检验 {i.InspectionNo} 结果未判定",
                Detail = $"检验水平 {i.SamplingLevel ?? "-"} · 抽样 {i.SampleSize} pcs · 检验员 {i.Inspector ?? "未指派"}",
                Time = i.InspectedAt ?? i.CreatedAt,
                Priority = 2,
                Label = "待判定"
            });
        }

        // 2) FQC 成品检验：不合格 / 检验中
        var fqcPending = await _db.FqcInspections
            .Where(f => f.Conclusion == "unqualified" || f.Conclusion == "pending")
            .AsNoTracking()
            .ToListAsync();
        foreach (var f in fqcPending)
        {
            var unq = f.Conclusion == "unqualified";
            list.Add(new PendingItemDto
            {
                Id = f.Id,
                Type = "fqc",
                Title = unq
                    ? $"FQC 成品检验 {f.InspectionNo} 不合格"
                    : $"FQC 成品检验 {f.InspectionNo} 检验中",
                Detail = $"抽检 {f.TotalChecked} pcs · 合格 {f.TotalPass} · 不合格 {f.TotalFail} · 工单 {(f.WorkOrderId == null ? "-" : $"WO-{f.WorkOrderId}")}",
                Time = f.CheckedAt ?? f.CreatedAt,
                Priority = unq ? 3 : 1,
                Label = unq ? "紧急" : "处理中"
            });
        }

        // 3) 未关闭缺陷
        var defects = await _db.Defects
            .Where(d => d.Status == "open" || d.Status == "investigating" || d.Status == "in_progress")
            .AsNoTracking()
            .ToListAsync();
        foreach (var d in defects)
        {
            int priority;
            string label;
            if (d.Severity == "critical")
            {
                priority = 3;
                label = "紧急";
            }
            else if (d.Severity == "major" && d.Status == "open")
            {
                priority = 2;
                label = "待判定";
            }
            else
            {
                priority = 1;
                label = "处理中";
            }
            list.Add(new PendingItemDto
            {
                Id = d.Id,
                Type = "defect",
                Title = $"缺陷 {d.DefectCode} {Truncate(d.Description, 28)}",
                Detail = $"严重度 {d.Severity} · 来源 {d.SourceType} · 数量 {d.Quantity:0} 件 · 状态 {d.Status}",
                Time = d.DiscoveredAt,
                Priority = priority,
                Label = label
            });
        }

        // 4) 未完成 CAPA（逾期 → 逾期风险）
        var capas = await _db.Capas
            .Where(c => c.Status != "closed" && c.Status != "cancelled")
            .AsNoTracking()
            .ToListAsync();
        foreach (var c in capas)
        {
            var due = ToDateOnly(c.DueDate);
            int priority;
            string label;
            if (due != null && due < today)
            {
                priority = 3;
                label = "逾期风险";
            }
            else if (due != null && due <= today.AddDays(1))
            {
                priority = 2;
                label = "逾期风险";
            }
            else
            {
                priority = 1;
                label = "处理中";
            }
            list.Add(new PendingItemDto
            {
                Id = c.Id,
                Type = "capa",
                Title = $"CAPA {c.CapaCode} {Truncate(c.Title, 24)}",
                Detail = $"阶段 {c.CurrentPhase} · 严重度 {c.Severity} · 截止 {(due?.ToString("MM-dd") ?? "未设置")}",
                Time = c.CreatedAt,
                Priority = priority,
                Label = label
            });
        }

        // 5) 未关闭客诉
        var complaints = await _db.Complaints
            .Where(cp => cp.Status != "closed" && cp.Status != "completed")
            .AsNoTracking()
            .ToListAsync();
        foreach (var cp in complaints)
        {
            int priority;
            string label;
            if (cp.DueDate != null && cp.DueDate < today)
            {
                priority = 3;
                label = "逾期风险";
            }
            else if (cp.DueDate != null && cp.DueDate <= today.AddDays(1))
            {
                priority = 2;
                label = "逾期风险";
            }
            else if (cp.Status == "new")
            {
                priority = 2;
                label = "待判定";
            }
            else
            {
                priority = 1;
                label = "处理中";
            }
            list.Add(new PendingItemDto
            {
                Id = cp.Id,
                Type = "complaint",
                Title = $"客诉 {cp.ComplaintCode} {Truncate(cp.Subject, 24)}",
                Detail = $"严重度 {cp.Severity} · 状态 {cp.Status} · 截止 {(cp.DueDate?.ToString("MM-dd") ?? "未设置")}",
                Time = cp.CreatedAt,
                Priority = priority,
                Label = label
            });
        }

        // 6) 首件检验通过、待放行
        var firstPieces = await _db.IpqcFirstPieces
            .Where(f => f.Conclusion == "qualified" && !f.AllowedToProduce)
            .AsNoTracking()
            .ToListAsync();
        foreach (var f in firstPieces)
        {
            list.Add(new PendingItemDto
            {
                Id = f.Id,
                Type = "firstpiece",
                Title = $"首件 {f.FpNo} 检验通过，待放行",
                Detail = $"工单 WO-{f.WorkOrderId} · 首件原因 {f.Reason}",
                Time = f.CheckedAt ?? f.CreatedAt,
                Priority = 1,
                Label = "待放行"
            });
        }

        var total = list.Count;
        var items = list
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.Time)
            .Take(5)
            .ToList();
        return (items, total);
    }
}

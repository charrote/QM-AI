namespace QM_AI.API.DTOs.M14;

// ─── 质量统计（reports/quality） ───────────────────────────────
public class QualityStatsDto
{
    /// <summary>IQC + FQC 已判定检验项总数</summary>
    public int TotalInspections { get; set; }

    /// <summary>IQC 已判定检验项数</summary>
    public int IqcItemCount { get; set; }

    /// <summary>FQC 已判定检验项数</summary>
    public int FqcItemCount { get; set; }

    /// <summary>IQC+FQC 合格项数</summary>
    public int PassItemCount { get; set; }

    /// <summary>综合一次合格率 FPY = (IQC+FQC 合格项) / (IQC+FQC 判定项) * 100</summary>
    public decimal Fpy { get; set; }

    public decimal IqcPassRate { get; set; }
    public decimal IpqcPassRate { get; set; }
    public decimal FqcPassRate { get; set; }

    /// <summary>报废量占报废+返工处置总量的比例（%），无处置记录时为 0</summary>
    public decimal ScrapRate { get; set; }

    /// <summary>返工量占报废+返工处置总量的比例（%），无处置记录时为 0</summary>
    public decimal ReworkRate { get; set; }

    /// <summary>缺陷记录总数（defects 表，按 discovered_at 过滤）</summary>
    public int TotalDefects { get; set; }
}

// ─── 缺陷帕累托（reports/defect-pareto） ───────────────────────
public class DefectParetoItemDto
{
    public string DefectCode { get; set; } = string.Empty;
    public string DefectName { get; set; } = string.Empty;
    public int Count { get; set; }

    /// <summary>单项占比（%）= 本项 count / 总缺陷数</summary>
    public decimal Percentage { get; set; }

    /// <summary>累计占比（%），按 count 降序累加，用于帕累托图 80/20 分析</summary>
    public decimal CumulativePercentage { get; set; }
}

// ─── 质量趋势（reports/quality-trend） ─────────────────────────
public class QualityTrendPointDto
{
    /// <summary>yyyy-MM-dd</summary>
    public string Date { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pass { get; set; }

    /// <summary>当日 FPY（%），当日无判定项时为 null（稀疏数据如实留空）</summary>
    public decimal? Fpy { get; set; }
}

// ─── 今日待处理（dashboard/pending） ───────────────────────────
public class PendingItemDto
{
    /// <summary>来源实体主键（配合 Type 定位记录）</summary>
    public long Id { get; set; }

    /// <summary>来源类型：iqc / fqc / defect / capa / complaint / firstpiece</summary>
    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public DateTime Time { get; set; }

    /// <summary>3=最高 1=最低</summary>
    public int Priority { get; set; }

    /// <summary>优先级标签：紧急 / 待判定 / 逾期风险 / 待放行 / 处理中</summary>
    public string Label { get; set; } = string.Empty;
}

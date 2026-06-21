namespace QM_AI.API.DTOs.M02_Inspection;

// ─── InspectionItem (检验项目主数据) ────────────────────────────

public class InspectionItemListDto
{
    public long Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string DataType { get; set; } = "numeric";
    public string? Unit { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
    public string? ChartType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InspectionItemDetailDto : InspectionItemListDto
{
    public string? Description { get; set; }
    public decimal? Ucl { get; set; }
    public decimal? Lcl { get; set; }
    public string? DataCollectionParamCode { get; set; }
    public int? SubgroupSize { get; set; }
    public string? InspectionMethod { get; set; }
    public int? SampleSize { get; set; }
    public int CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateInspectionItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DataType { get; set; } = "numeric";
    public string? Unit { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Ucl { get; set; }
    public decimal? Lcl { get; set; }
    public string? DataCollectionParamCode { get; set; }
    public string? ChartType { get; set; }
    public int? SubgroupSize { get; set; }
    public string? InspectionMethod { get; set; }
    public int? SampleSize { get; set; }
}

public class UpdateInspectionItemDto : CreateInspectionItemDto
{
    public bool IsActive { get; set; } = true;
}

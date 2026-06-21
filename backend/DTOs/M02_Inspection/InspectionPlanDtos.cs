namespace QM_AI.API.DTOs.M02_Inspection;

// ─── InspectionPlan (检验计划) ─────────────────────────────────

public class InspectionPlanListDto
{
    public long Id { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string InspectionType { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public bool IsActive { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InspectionPlanDetailDto : InspectionPlanListDto
{
    public int? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public int? EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public string? Description { get; set; }
    public int CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<InspectionPlanItemDto> Items { get; set; } = new();
}

public class InspectionPlanItemDto
{
    public long Id { get; set; }
    public long InspectionItemId { get; set; }
    public string InspectionItemCode { get; set; } = string.Empty;
    public string InspectionItemName { get; set; } = string.Empty;
    public string DataType { get; set; } = "numeric";
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Ucl { get; set; }
    public decimal? Lcl { get; set; }
    public int? SampleSize { get; set; }
    public bool IsRequired { get; set; } = true;
}

public class CreateInspectionPlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string InspectionType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public int? MaterialId { get; set; }
    public int? SupplierId { get; set; }
    public int? CustomerId { get; set; }
    public int? ProcessId { get; set; }
    public int? EquipmentId { get; set; }
    public List<CreateInspectionPlanItemDto> Items { get; set; } = new();
}

public class CreateInspectionPlanItemDto
{
    public long InspectionItemId { get; set; }
    public int SortOrder { get; set; } = 0;
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Ucl { get; set; }
    public decimal? Lcl { get; set; }
    public int? SampleSize { get; set; }
    public bool IsRequired { get; set; } = true;
}

public class UpdateInspectionPlanDto
{
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ProductId { get; set; }
    public int? MaterialId { get; set; }
    public int? SupplierId { get; set; }
    public int? CustomerId { get; set; }
    public int? ProcessId { get; set; }
    public int? EquipmentId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CreateInspectionPlanItemDto> Items { get; set; } = new();
}

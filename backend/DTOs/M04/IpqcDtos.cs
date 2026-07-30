namespace QM_AI.API.DTOs.M04;

// ─── IpqcFirstPiece ───────────────────────────────────────────────

public class IpqcFirstPieceListDto
{
    public long Id { get; set; }
    public string FpNo { get; set; } = string.Empty;
    public long WorkOrderId { get; set; }
    public string? WorkOrderNo { get; set; }
    public long ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public long EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public string? Shift { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Conclusion { get; set; } = "pending";
    public bool AllowedToProduce { get; set; }
    public string? Inspector { get; set; }
    public DateTime? CheckedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IpqcFirstPieceDetailDto : IpqcFirstPieceListDto
{
    public long OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public List<IpqcFirstPieceItemDto>? Items { get; set; }
}

public class IpqcFirstPieceItemDto
{
    public long Id { get; set; }
    public long FirstPieceId { get; set; }
    public long? InspectionItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string DataType { get; set; } = "numeric";
    public decimal? ActualValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? ImageUrls { get; set; }
    public string? Remarks { get; set; }
}

public class CreateIpqcFirstPieceDto
{
    public string? FpNo { get; set; }
    public long WorkOrderId { get; set; }
    public long ProcessId { get; set; }
    public long EquipmentId { get; set; }
    public long OperatorId { get; set; }
    public string? Shift { get; set; }
    public string Reason { get; set; } = "班次切换";
    public string? Inspector { get; set; }
    public List<CreateIpqcFirstPieceItemDto> Items { get; set; } = new();
}

public class CreateIpqcFirstPieceItemDto
{
    public long? InspectionItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string DataType { get; set; } = "numeric";
    public decimal? ActualValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? ImageUrls { get; set; }
    public string? Remarks { get; set; }
}

public class SubmitIpqcFirstPieceDto
{
    public string Conclusion { get; set; } = "pending";
    public bool AllowedToProduce { get; set; }
    public string? Inspector { get; set; }
    public List<IpqcFirstPieceItemDto>? Items { get; set; }
}

// ─── IpqcPatrolPlan ───────────────────────────────────────────────

public class IpqcPatrolPlanListDto
{
    public long Id { get; set; }
    public string PlanNo { get; set; } = string.Empty;
    public long ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public long[] EquipmentIds { get; set; } = Array.Empty<long>();
    public string[] EquipmentNames { get; set; } = Array.Empty<string>();
    public int PatrolIntervalMin { get; set; }
    public bool AutoGenerate { get; set; }
    public string Status { get; set; } = "active";
    public string? Inspector { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateIpqcPatrolPlanDto
{
    public string? PlanNo { get; set; }
    public long ProcessId { get; set; }
    public long[] EquipmentIds { get; set; } = Array.Empty<long>();
    public int PatrolIntervalMin { get; set; }
    public bool AutoGenerate { get; set; } = true;
    public string? Inspector { get; set; }
}

public class UpdateIpqcPatrolPlanDto
{
    public long[]? EquipmentIds { get; set; }
    public int? PatrolIntervalMin { get; set; }
    public bool? AutoGenerate { get; set; }
    public string? Status { get; set; }
    public string? Inspector { get; set; }
}

// ─── IpqcPatrol ───────────────────────────────────────────────────

public class IpqcPatrolListDto
{
    public long Id { get; set; }
    public string PatrolNo { get; set; } = string.Empty;
    public long PatrolPlanId { get; set; }
    public string? PlanNo { get; set; }
    public long? WorkOrderId { get; set; }
    public long ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public long EquipmentId { get; set; }
    public string? EquipmentName { get; set; }
    public string? Inspector { get; set; }
    public DateTime ScheduledTime { get; set; }
    public DateTime? ActualTime { get; set; }
    public int TotalChecked { get; set; }
    public int TotalPass { get; set; }
    public int TotalFail { get; set; }
    public string Conclusion { get; set; } = "pending";
    public string Status { get; set; } = "scheduled";
    public DateTime CreatedAt { get; set; }
}

public class IpqcPatrolDetailDto : IpqcPatrolListDto
{
    public string? Remarks { get; set; }
    public List<IpqcPatrolItemDto>? Items { get; set; }
}

public class IpqcPatrolItemDto
{
    public long Id { get; set; }
    public long PatrolId { get; set; }
    public long? InspectionItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string DataType { get; set; } = "numeric";
    public decimal? ActualValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? ImageUrls { get; set; }
}

public class SubmitIpqcPatrolDto
{
    public string Conclusion { get; set; } = "pending";
    public string? Remarks { get; set; }
    public string? Inspector { get; set; }
    public List<IpqcPatrolItemSubmitDto>? Items { get; set; }
}

public class IpqcPatrolItemSubmitDto
{
    public long? Id { get; set; }
    public long? InspectionItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string DataType { get; set; } = "numeric";
    public decimal? ActualValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? ImageUrls { get; set; }
}

// ─── AiRiskScore ──────────────────────────────────────────────────

public class IpqcAiRiskScoreDto
{
    public int Score { get; set; }
    public string Level { get; set; } = "normal";
    public string? Trend { get; set; }
    public string? FactorsJson { get; set; }
    public string? Summary { get; set; }
    public List<RiskFactorDto> Factors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime? LastUpdated { get; set; }
}

public class RiskFactorDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Impact { get; set; }
    public double? CurrentValue { get; set; }
    public double? TargetValue { get; set; }
}

// ─── Closure ──────────────────────────────────────────────────────

public class IpqcClosureEvaluationDto
{
    public bool IsSatisfied { get; set; }
    public string[]? FailedConditions { get; set; }
    public string? Status { get; set; }
    public string? RuleName { get; set; }
}

// ─── Patrol Plan Auto-Generate Request ────────────────────────────

public class PatrolPlanGenerateRequestDto
{
    public long PlanId { get; set; }
    public long? WorkOrderId { get; set; }
    public DateTime StartTime { get; set; }
    public int Count { get; set; } = 4;
}

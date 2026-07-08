namespace QM_AI.API.DTOs.M06;

// ─── Control Chart DTOs ──────────────────────────────────────────

public class SpcControlChartListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long ProcessId { get; set; }
    public string? ProcessName { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public int SubgroupSize { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Cl { get; set; }
    public decimal? Ucl { get; set; }
    public decimal? Lcl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SpcControlChartDetailDto : SpcControlChartListDto
{
    public long CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int DataPointCount { get; set; }
}

public class CreateSpcControlChartDto
{
    public string Name { get; set; } = string.Empty;
    public long ProcessId { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string ChartType { get; set; } = "Xbar_R";
    public int SubgroupSize { get; set; } = 5;
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
}

public class UpdateSpcControlChartDto
{
    public string Name { get; set; } = string.Empty;
    public int SubgroupSize { get; set; } = 5;
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal? TargetValue { get; set; }
}

// ─── Data Point DTOs ────────────────────────────────────────────

public class SpcDataPointListDto
{
    public long Id { get; set; }
    public long ChartId { get; set; }
    public int SubgroupIndex { get; set; }
    public string IndividualValues { get; set; } = "[]";
    public decimal? SubgroupMean { get; set; }
    public decimal? SubgroupRange { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSpcDataPointDto
{
    public long ChartId { get; set; }
    public int SubgroupIndex { get; set; }
    public string IndividualValues { get; set; } = "[]";
    public DateTime MeasuredAt { get; set; }
}

public class BatchCreateDataPointsDto
{
    public long ChartId { get; set; }
    public List<CreateSpcDataPointDto> DataPoints { get; set; } = new();
}

// ─── Analysis Result DTOs ───────────────────────────────────────

public class SpcAnalysisResultDto
{
    public long Id { get; set; }
    public long ChartId { get; set; }
    public string AnalysisType { get; set; } = string.Empty;
    public decimal? Cp { get; set; }
    public decimal? Cpk { get; set; }
    public decimal? Pp { get; set; }
    public decimal? Ppk { get; set; }
    public decimal? SigmaWithin { get; set; }
    public decimal? SigmaOverall { get; set; }
    public decimal? EstimatedPpm { get; set; }
    public int? DataPointsUsed { get; set; }
    public string? Grade { get; set; }
    public DateTime? AnalysisPeriodStart { get; set; }
    public DateTime? AnalysisPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ─── Alert Rule DTOs ────────────────────────────────────────────

public class SpcAlertRuleListDto
{
    public long Id { get; set; }
    public long ChartId { get; set; }
    public int RuleNumber { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string? RuleDescription { get; set; }
    public bool Enabled { get; set; }
    public int TriggerThreshold { get; set; }
    public decimal SigmaThreshold { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSpcAlertRuleDto
{
    public long ChartId { get; set; }
    public int RuleNumber { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string? RuleDescription { get; set; }
    public bool Enabled { get; set; } = true;
    public int TriggerThreshold { get; set; } = 1;
    public decimal SigmaThreshold { get; set; } = 2.0m;
}

public class UpdateSpcAlertRuleDto
{
    public bool Enabled { get; set; }
    public int TriggerThreshold { get; set; }
    public decimal SigmaThreshold { get; set; }
}

// ─── Alert Trigger DTOs ─────────────────────────────────────────

public class SpcAlertTriggerListDto
{
    public long Id { get; set; }
    public long ChartId { get; set; }
    public long RuleId { get; set; }
    public int RuleNumber { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public int ViolatedPointIndex { get; set; }
    public string? Detail { get; set; }
    public bool Resolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

// ─── ANOVA DTOs ────────────────────────────────────────────────

public class SpcAnovaResultDto
{
    public long Id { get; set; }
    public long ChartId { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal SumOfSquares { get; set; }
    public int DegreesFreedom { get; set; }
    public decimal MeanSquare { get; set; }
    public decimal FRatio { get; set; }
    public decimal PValue { get; set; }
    public bool Significant { get; set; }
    public DateTime AnalysisDate { get; set; }
}

public class SpcAnovaRequestDto
{
    public long ChartId { get; set; }
    public List<SpcAnovaFactorDto> Factors { get; set; } = new();
}

public class SpcAnovaFactorDto
{
    public string Source { get; set; } = string.Empty; // operator, machine, material, method, environment
    public string Label { get; set; } = string.Empty;
    public List<double> Values { get; set; } = new();
}

// ─── Control Limits DTO ────────────────────────────────────────

public class SpcControlLimitsDto
{
    public double ClXbar { get; set; }
    public double UclXbar { get; set; }
    public double LclXbar { get; set; }
    public double ClR { get; set; }
    public double UclR { get; set; }
    public double LclR { get; set; }
}

// ─── Rule Violation DTO ────────────────────────────────────────

public class SpcRuleViolationDto
{
    public int RuleNumber { get; set; }
    public int Index { get; set; }
    public string Description { get; set; } = string.Empty;
}

// ─── Data Source DTOs ──────────────────────────────────────────

public class SpcDataSourceDto
{
    public long Id { get; set; }
    public long ChartId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public long? InspectionItemId { get; set; }
    public string? InspectionItemName { get; set; }
    public long? ProductId { get; set; }
    public long? ProcessId { get; set; }
    public long? SupplierId { get; set; }
    public long? CustomerId { get; set; }
    public long? EquipmentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSpcDataSourceDto
{
    public long ChartId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public long? InspectionItemId { get; set; }
    public long? ProductId { get; set; }
    public long? ProcessId { get; set; }
    public long? SupplierId { get; set; }
    public long? CustomerId { get; set; }
    public long? EquipmentId { get; set; }
}

// ─── 业务数据拉取 DTO（用于SPC从IQC/IPQC/FQC取数）─────────────

public class BusinessInspectionDataDto
{
    public long Id { get; set; }
    public string SourceType { get; set; } = string.Empty; // IQC/IPQC/FQC
    public string SourceNo { get; set; } = string.Empty; // 单据编号
    public long? InspectionItemId { get; set; }
    public string? InspectionItemName { get; set; }
    public decimal? MeasuredValue { get; set; }
    public string Result { get; set; } = string.Empty;
    public DateTime InspectedAt { get; set; }
    public long? ProductId { get; set; }
    public long? ProcessId { get; set; }
    public long? SupplierId { get; set; }
    public long? CustomerId { get; set; }
    public long? EquipmentId { get; set; }
}

public class BusinessDataQueryDto
{
    public string? SourceType { get; set; } // IQC/IPQC/FQC
    public long? InspectionItemId { get; set; }
    public long? ProductId { get; set; }
    public long? ProcessId { get; set; }
    public long? SupplierId { get; set; }
    public long? CustomerId { get; set; }
    public long? EquipmentId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Limit { get; set; }
}

// ─── SPC Analysis Request / Result ─────────────────────────────

public class SpcAnalyzeRequestDto
{
    public long ChartId { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
}

public class SpcAnalysisReportDto
{
    public SpcControlChartListDto? Chart { get; set; }
    public SpcControlLimitsDto? ControlLimits { get; set; }
    public SpcAnalysisResultDto? Capability { get; set; }
    public List<SpcRuleViolationDto> Violations { get; set; } = new();
    public List<SpcDataPointListDto> DataPoints { get; set; } = new();
}

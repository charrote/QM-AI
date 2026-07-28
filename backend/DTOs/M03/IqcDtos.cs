using QM_AI.API.Models.M03;

namespace QM_AI.API.DTOs.M03;

// ─── IqcReceipt ──────────────────────────────────────────────────
public class IqcReceiptListDto
{
    public long Id { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? Inspector { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

public class IqcReceiptDetailDto : IqcReceiptListDto
{
    public ICollection<IqcInspectionListDto>? Inspections { get; set; }
    public ICollection<IqcAnomalyListDto>? Anomalies { get; set; }
}

public class CreateIqcReceiptDto
{
    public string ReceiptNo { get; set; } = string.Empty;
    public long SupplierId { get; set; }
    public long ProductId { get; set; }
    public string? BatchNo { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? Inspector { get; set; }
}

public class UpdateIqcReceiptDto
{
    public string? BatchNo { get; set; }
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? Inspector { get; set; }
    public string? Status { get; set; }
}

// ─── IqcInspection ───────────────────────────────────────────────
public class IqcInspectionListDto
{
    public long Id { get; set; }
    public string InspectionNo { get; set; } = string.Empty;
    public long ReceiptId { get; set; }
    public string? ReceiptNo { get; set; }
    public long? StandardId { get; set; }
    public int SampleSize { get; set; }
    public int Ac { get; set; }
    public int Re { get; set; }
    public int DefectQty { get; set; }
    public string? SamplingLevel { get; set; }
    public double? AqlValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? Inspector { get; set; }
    public DateTime? InspectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class IqcInspectionDetailDto : IqcInspectionListDto
{
    public string SupplierName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public ICollection<IqcInspectionItemDto>? Items { get; set; }
}

public class CreateIqcInspectionDto
{
    public long ReceiptId { get; set; }
    public long? StandardId { get; set; }
    public int SampleSize { get; set; }
    public int Ac { get; set; }
    public int Re { get; set; }
    public string? SamplingLevel { get; set; } = "II";
    public double? AqlValue { get; set; }
    public string? Inspector { get; set; }
}

public class SubmitIqcInspectionDto
{
    public List<IqcInspectionItemSubmitDto> Items { get; set; } = new();
    public string? Inspector { get; set; }
}

public class IqcInspectionItemSubmitDto
{
    public long? Id { get; set; }
    public long? ParamId { get; set; }
    public long? InspectionItemId { get; set; }
    public string? ItemName { get; set; }
    public decimal? MeasuredValue { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string Result { get; set; } = "pending";
    public long? DefectCodeId { get; set; }
    public string? Remark { get; set; }
}

// ─── IqcInspectionItem ───────────────────────────────────────────
public class IqcInspectionItemDto
{
    public long Id { get; set; }
    public long InspectionId { get; set; }
    public long? ParamId { get; set; }
    public string? ItemName { get; set; }
    public decimal? MeasuredValue { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string Result { get; set; } = "pending";
    public long? DefectCodeId { get; set; }
    public string? DefectCodeName { get; set; }
    public string? Remark { get; set; }
}

// ─── IqcAnomaly (增强版) ──────────────────────────────────────────
public class IqcAnomalyListDto
{
    public long Id { get; set; }
    public string AnomalyNo { get; set; } = string.Empty;
    public long ReceiptId { get; set; }
    public string? ReceiptNo { get; set; }
    public long? InspectionId { get; set; }
    public string? FailedItemIds { get; set; }
    public int DefectQty { get; set; }
    public string AnomalyType { get; set; } = "quality";
    public string Severity { get; set; } = "major";
    public string? Description { get; set; }
    public decimal IsolatedInventory { get; set; } = 0;
    public string Disposition { get; set; } = "none";
    public string? DispositionBy { get; set; }
    public DateTime? DispositionDate { get; set; }
    public string? HandlerDept { get; set; }
    public string Status { get; set; } = "open";
    public string? Handler { get; set; }
    public bool MrbReviewed { get; set; }
    public string? MrbReviewer { get; set; }
    public DateTime? MrbReviewedAt { get; set; }
    public long? CapaId { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public bool SupplierNotified { get; set; }
    public DateTime? SupplierResponseAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateIqcAnomalyDto
{
    public string? AnomalyNo { get; set; }
    public long ReceiptId { get; set; }
    public long? InspectionId { get; set; }
    public string? FailedItemIds { get; set; }
    public int DefectQty { get; set; }
    public string AnomalyType { get; set; } = "quality";
    public string Severity { get; set; } = "major";
    public string? Description { get; set; }
    public decimal IsolatedInventory { get; set; }
    public string? HandlerDept { get; set; }
    public string? Handler { get; set; }
    public long? CreatedBy { get; set; }
}

public class UpdateIqcAnomalyDto
{
    public string? Status { get; set; }
    public string? Handler { get; set; }
    public string? HandlerDept { get; set; }
    public string? Description { get; set; }
    public decimal? IsolatedInventory { get; set; }
    public string? Disposition { get; set; }
    public string? DispositionBy { get; set; }
    public DateTime? DispositionDate { get; set; }
    public bool? MrbReviewed { get; set; }
    public string? MrbReviewer { get; set; }
    public DateTime? MrbReviewedAt { get; set; }
    public long? CapaId { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public bool? SupplierNotified { get; set; }
    public DateTime? SupplierResponseAt { get; set; }
    public long? UpdatedBy { get; set; }
}

public class ResolveIqcAnomalyDto
{
    public string Resolution { get; set; } = string.Empty;
    public string? Handler { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>MRB 评审动作</summary>
public class MrbReviewDto
{
    public bool Approved { get; set; }
    public string? Reviewer { get; set; }
    public string? ReviewComments { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>处置决定动作</summary>
public class DispositionDto
{
    public string Disposition { get; set; } = "none"; // return/concession/rework/special_purchase
    public string? DispositionBy { get; set; }
    public long? UpdatedBy { get; set; }
}

/// <summary>通知供应商动作</summary>
public class NotifySupplierDto
{
    public string? Message { get; set; }
    public long? UpdatedBy { get; set; }
}

// ─── SupplierScore ───────────────────────────────────────────────
public class SupplierScoreDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime? ScoreDate { get; set; }
    public decimal? Score { get; set; }
    public string? DimensionScores { get; set; }
    public string? Grade { get; set; }
    public string? Evaluation { get; set; }
}

public class UpdateSupplierScoreDto
{
    public DateTime? ScoreDate { get; set; }
    public decimal? Score { get; set; }
    public string? DimensionScores { get; set; }
    public string? Grade { get; set; }
    public string? Evaluation { get; set; }
}

// ─── Sampling Plan ───────────────────────────────────────────────
public class SamplingPlanDto
{
    public char SampleCode { get; set; }
    public int SampleSize { get; set; }
    public int Ac { get; set; }
    public int Re { get; set; }
    public string SamplingLevel { get; set; } = "II";
    public double AqlValue { get; set; }
    public int LotSize { get; set; }
    public bool IsReduced { get; set; }
    public bool IsNormal { get; set; }
    public bool IsStricter { get; set; }
}

public class SamplingPlanRequestDto
{
    public int LotSize { get; set; }
    public string SamplingLevel { get; set; } = "II";
    public double AqlValue { get; set; } = 1.0;
}

// ─── AI Risk Score ───────────────────────────────────────────────
public class AiRiskScoreDto
{
    public int Score { get; set; }
    public string Level { get; set; } = "low";
    public List<RiskFactorDto> Factors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class RiskFactorDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Impact { get; set; }
}

// ─── Batch Trace ─────────────────────────────────────────────────
public class BatchTraceDto
{
    public IqcReceiptListDto? Receipt { get; set; }
    public List<IqcInspectionListDto> Inspections { get; set; } = new();
    public List<IqcAnomalyListDto> Anomalies { get; set; } = new();
    public SupplierScoreDto? SupplierScore { get; set; }
}

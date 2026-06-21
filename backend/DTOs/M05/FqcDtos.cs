namespace QM_AI.API.DTOs.M05;

// ─── ProductBatch ─────────────────────────────────────────────────

public class ProductBatchListDto
{
    public long Id { get; set; }
    public string BatchCode { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public long? WorkOrderId { get; set; }
    public decimal Quantity { get; set; }
    public string Source { get; set; } = "manual";
    public string Status { get; set; } = "in_progress";
    public DateTime CreatedAt { get; set; }
}

public class ProductBatchDetailDto : ProductBatchListDto
{
    public List<FqcInspectionListDto>? Inspections { get; set; }
    public List<OqcReleaseListDto>? Releases { get; set; }
    public List<PackagingConfirmationListDto>? PackagingConfirmations { get; set; }
}

public class CreateProductBatchDto
{
    public string? BatchCode { get; set; }
    public int ProductId { get; set; }
    public long? WorkOrderId { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>批次来源：manual / ipqc-auto / work-order</summary>
    public string Source { get; set; } = "manual";
}

public class UpdateProductBatchDto
{
    public decimal? Quantity { get; set; }
    public string? Status { get; set; }
}

// ─── FqcInspection ────────────────────────────────────────────────

public class FqcInspectionListDto
{
    public long Id { get; set; }
    public string InspectionNo { get; set; } = string.Empty;
    public long BatchId { get; set; }
    public string? BatchCode { get; set; }
    public long? WorkOrderId { get; set; }
    public string InspectionType { get; set; } = "full";
    public decimal? AqlLevel { get; set; }
    public int SampleSize { get; set; }
    public int TotalChecked { get; set; }
    public int TotalPass { get; set; }
    public int TotalFail { get; set; }
    public int Ac { get; set; }
    public int Re { get; set; }
    public string Conclusion { get; set; } = "pending";
    public string? InspectorName { get; set; }
    public DateTime? CheckedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FqcInspectionDetailDto : FqcInspectionListDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal BatchQuantity { get; set; }
    public List<FqcInspectionItemDto>? Items { get; set; }
}

public class CreateFqcInspectionDto
{
    public long BatchId { get; set; }
    public long? WorkOrderId { get; set; }
    public string InspectionType { get; set; } = "full";
    public decimal? AqlLevel { get; set; }
    public int SampleSize { get; set; }
    public int Ac { get; set; }
    public int Re { get; set; }
    public int? InspectorId { get; set; }
}

public class SubmitFqcInspectionDto
{
    public int TotalChecked { get; set; }
    public int TotalPass { get; set; }
    public int TotalFail { get; set; }
    public int? InspectorId { get; set; }
    public List<FqcInspectionItemSubmitDto>? Items { get; set; }
}

public class FqcInspectionItemSubmitDto
{
    public long? Id { get; set; }
    public string? ItemName { get; set; }
    public string? ItemCode { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string DataType { get; set; } = "numeric";
    public decimal? ActualValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? ImageUrls { get; set; }
}

// ─── FqcInspectionItem ────────────────────────────────────────────

public class FqcInspectionItemDto
{
    public long Id { get; set; }
    public long InspectionId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public string DataType { get; set; } = "numeric";
    public decimal? ActualValue { get; set; }
    public string Result { get; set; } = "pending";
    public string? ImageUrls { get; set; }
}

// ─── OqcRelease ──────────────────────────────────────────────────

public class OqcReleaseListDto
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public string? BatchCode { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ReleaseNumber { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public decimal Quantity { get; set; }
    public string? AuthorizedName { get; set; }
    public string? ESignatureUrl { get; set; }
    public DateTime? SignatureTime { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
}

public class OqcReleaseDetailDto : OqcReleaseListDto
{
    public int? AuthorizedBy { get; set; }
}

public class CreateOqcReleaseDto
{
    public long BatchId { get; set; }
    public int CustomerId { get; set; }
    public string? ReleaseNumber { get; set; }
    public DateTime ReleaseDate { get; set; }
    public decimal Quantity { get; set; }
}

public class SignOqcReleaseDto
{
    public int AuthorizedBy { get; set; }
    public string ESignatureUrl { get; set; } = string.Empty;
}

// ─── PackagingConfirmation ────────────────────────────────────────

public class PackagingConfirmationListDto
{
    public long Id { get; set; }
    public long BatchId { get; set; }
    public string? BatchCode { get; set; }
    public string PackagingMethod { get; set; } = string.Empty;
    public int? QtyPerBox { get; set; }
    public int? TotalBoxes { get; set; }
    public bool LabelPrinted { get; set; }
    public string ConfirmedByName { get; set; } = string.Empty;
    public DateTime ConfirmedAt { get; set; }
}

public class CreatePackagingConfirmationDto
{
    public long BatchId { get; set; }
    public string PackagingMethod { get; set; } = string.Empty;
    public int? QtyPerBox { get; set; }
    public int? TotalBoxes { get; set; }
    public bool LabelPrinted { get; set; }
    public int ConfirmedBy { get; set; }
}

// ─── Batch Number Generation ─────────────────────────────────────

public class BatchNumberGenerateDto
{
    public string BatchCode { get; set; } = string.Empty;
}

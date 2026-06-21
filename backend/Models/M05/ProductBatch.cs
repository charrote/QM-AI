using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M05;

/// <summary>
/// 成品批次
/// </summary>
[Table("product_batches")]
public class ProductBatch
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>批次编号（唯一，LOT-YYYYMMDD-X 格式）</summary>
    [Required]
    [MaxLength(50)]
    public string BatchCode { get; set; } = string.Empty;

    /// <summary>批次来源：manual / ipqc-auto / work-order</summary>
    [Required]
    [MaxLength(20)]
    public string Source { get; set; } = "manual";

    /// <summary>关联产品</summary>
    public int ProductId { get; set; }

    /// <summary>关联工单</summary>
    public long? WorkOrderId { get; set; }

    /// <summary>数量</summary>
    public decimal Quantity { get; set; }

    /// <summary>状态：in_progress / inspected / released / quarantined</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "in_progress";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public ICollection<FqcInspection>? Inspections { get; set; }
    public ICollection<OqcRelease>? Releases { get; set; }
    public ICollection<PackagingConfirmation>? PackagingConfirmations { get; set; }
}

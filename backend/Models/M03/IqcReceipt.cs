using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M03;

/// <summary>
/// IQC 来料登记
/// </summary>
[Table("iqc_receipts")]
public class IqcReceipt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>收货单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>关联供应商</summary>
    public long SupplierId { get; set; }

    /// <summary>关联物料/产品</summary>
    public long ProductId { get; set; }

    /// <summary>批次号</summary>
    [MaxLength(100)]
    public string? BatchNo { get; set; }

    /// <summary>数量</summary>
    public int Quantity { get; set; }

    /// <summary>单位</summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>到货日期</summary>
    public DateTime? ReceiptDate { get; set; }

    /// <summary>检验员</summary>
    [MaxLength(100)]
    public string? Inspector { get; set; }

    /// <summary>状态：pending/inspecting/completed/anomaly</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public ICollection<IqcInspection>? Inspections { get; set; }
    public ICollection<IqcAnomaly>? Anomalies { get; set; }
}

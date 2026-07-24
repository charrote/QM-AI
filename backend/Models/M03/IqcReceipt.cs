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
    [Column("id")]
    public long Id { get; set; }

    /// <summary>收货单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("receipt_no")]
    public string ReceiptNo { get; set; } = string.Empty;

    /// <summary>关联供应商</summary>
    public long SupplierId { get; set; }

    /// <summary>关联物料/产品</summary>
    [Column("product_id")]
    public long ProductId { get; set; }

    /// <summary>批次号</summary>
    [MaxLength(100)]
    [Column("batch_no")]
    public string? BatchNo { get; set; }

    /// <summary>数量</summary>
    [Column("quantity")]
    public int Quantity { get; set; }

    /// <summary>单位</summary>
    [MaxLength(20)]
    [Column("unit")]
    public string? Unit { get; set; }

    /// <summary>到货日期</summary>
    public DateTime? ReceiptDate { get; set; }

    /// <summary>检验员</summary>
    [MaxLength(100)]
    [Column("inspector")]
    public string? Inspector { get; set; }

    /// <summary>状态：pending/inspecting/completed/anomaly</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public ICollection<IqcInspection>? Inspections { get; set; }
    public ICollection<IqcAnomaly>? Anomalies { get; set; }
}

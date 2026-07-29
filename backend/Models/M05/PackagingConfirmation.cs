using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M05;

/// <summary>
/// 包装确认
/// </summary>
[Table("packaging_confirmations")]
public class PackagingConfirmation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联批次</summary>
    [Column("batch_id")]
    public long BatchId { get; set; }

    /// <summary>包装方式</summary>
    [Required]
    [MaxLength(200)]
    [Column("packaging_method")]
    public string PackagingMethod { get; set; } = string.Empty;

    /// <summary>每箱数量</summary>
    [Column("qty_per_box")]
    public int? QtyPerBox { get; set; }

    /// <summary>总箱数</summary>
    [Column("total_boxes")]
    public int? TotalBoxes { get; set; }

    /// <summary>标签是否已打印</summary>
    [Column("label_printed")]
    public bool LabelPrinted { get; set; }

    /// <summary>确认人 ID</summary>
    [Column("confirmed_by")]
    public int ConfirmedBy { get; set; }

    /// <summary>确认时间</summary>
    [Column("confirmed_at")]
    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(BatchId))]
    public ProductBatch? Batch { get; set; }
}

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
    public string PackagingMethod { get; set; } = string.Empty;

    /// <summary>每箱数量</summary>
    public int? QtyPerBox { get; set; }

    /// <summary>总箱数</summary>
    public int? TotalBoxes { get; set; }

    /// <summary>标签是否已打印</summary>
    public bool LabelPrinted { get; set; }

    /// <summary>确认人 ID</summary>
    public int ConfirmedBy { get; set; }

    /// <summary>确认时间</summary>
    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(BatchId))]
    public ProductBatch? Batch { get; set; }
}

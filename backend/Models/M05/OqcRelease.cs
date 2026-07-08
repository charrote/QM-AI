using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M05;

/// <summary>
/// OQC 出货放行
/// </summary>
[Table("oqc_releases")]
public class OqcRelease
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联批次</summary>
    public long BatchId { get; set; }

    /// <summary>关联客户</summary>
    public long CustomerId { get; set; }

    /// <summary>放行单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string ReleaseNumber { get; set; } = string.Empty;

    /// <summary>放行日期</summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>放行数量</summary>
    public decimal Quantity { get; set; }

    /// <summary>授权人 ID</summary>
    public int? AuthorizedBy { get; set; }

    /// <summary>电子签名 URL（MinIO）</summary>
    [MaxLength(500)]
    public string? ESignatureUrl { get; set; }

    /// <summary>签名时间</summary>
    public DateTime? SignatureTime { get; set; }

    /// <summary>状态：pending / signed / released / cancelled</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(BatchId))]
    public ProductBatch? Batch { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }
}

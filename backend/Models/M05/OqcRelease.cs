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
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联批次</summary>
    [Column("batch_id")]
    public long BatchId { get; set; }

    /// <summary>关联客户</summary>
    [Column("customer_id")]
    public long CustomerId { get; set; }

    /// <summary>放行单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("release_number")]
    public string ReleaseNumber { get; set; } = string.Empty;

    /// <summary>放行日期</summary>
    [Column("release_date")]
    public DateTime ReleaseDate { get; set; }

    /// <summary>放行数量</summary>
    [Column("quantity")]
    public decimal Quantity { get; set; }

    /// <summary>授权人 ID</summary>
    [Column("authorized_by")]
    public int? AuthorizedBy { get; set; }

    /// <summary>电子签名 URL（MinIO）</summary>
    [MaxLength(500)]
    [Column("e_signature_url")]
    public string? ESignatureUrl { get; set; }

    /// <summary>签名时间</summary>
    [Column("signature_time")]
    public DateTime? SignatureTime { get; set; }

    /// <summary>状态：pending / signed / released / cancelled</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(BatchId))]
    public ProductBatch? Batch { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }
}

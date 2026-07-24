using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M03;

/// <summary>
/// IQC 来料异常单
/// </summary>
[Table("iqc_anomalies")]
public class IqcAnomaly
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>异常单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("anomaly_no")]
    public string AnomalyNo { get; set; } = string.Empty;

    /// <summary>关联来料登记</summary>
    public long ReceiptId { get; set; }

    /// <summary>关联检验单</summary>
    public long? InspectionId { get; set; }

    /// <summary>异常类型：quality/quantity/document/other</summary>
    [Required]
    [MaxLength(20)]
    public string AnomalyType { get; set; } = "quality";

    /// <summary>严重程度：critical/major/minor</summary>
    [Required]
    [MaxLength(10)]
    [Column("severity")]
    public string Severity { get; set; } = "major";

    /// <summary>异常描述</summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>状态：open/processing/resolved/closed</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "open";

    /// <summary>处理人</summary>
    [MaxLength(100)]
    public string? Handler { get; set; }

    /// <summary>解决时间</summary>
    public DateTime? ResolvedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ReceiptId))]
    public IqcReceipt? Receipt { get; set; }

    [ForeignKey(nameof(InspectionId))]
    public IqcInspection? Inspection { get; set; }
}

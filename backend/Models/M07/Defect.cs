using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// 缺陷记录
/// </summary>
[Table("defects")]
public class Defect
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>不良代码</summary>
    [Required]
    [MaxLength(50)]
    [Column("defect_no")]
    public string DefectCode { get; set; } = string.Empty;

    /// <summary>严重等级：critical / major / minor</summary>
    [Required]
    [MaxLength(10)]
    [Column("severity")]
    public string Severity { get; set; } = "major";

    /// <summary>来源类型：iqc / ipqc / fqc / oqc / customer</summary>
    [Required]
    [MaxLength(10)]
    [Column("source_type")]
    public string SourceType { get; set; } = string.Empty;

    /// <summary>来源ID（检验单/客诉等）</summary>
    [Column("source_ref_id")]
    public long? SourceId { get; set; }

    /// <summary>关联产品</summary>
    [Column("product_id")]
    public long? ProductId { get; set; }

    /// <summary>关联批次</summary>
    [Column("batch_id")]
    public long? BatchId { get; set; }

    /// <summary>关联设备</summary>
    [Column("equipment_id")]
    public long? EquipmentId { get; set; }

    /// <summary>不良数量</summary>
    [Required]
    [Column("quantity")]
    public decimal Quantity { get; set; }

    /// <summary>不良描述</summary>
    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>图片URLs</summary>
    [Column("image_urls")]
    public string? ImageUrls { get; set; }

    /// <summary>发现人</summary>
    [Column("discovered_by")]
    public long? DiscoveredBy { get; set; }

    /// <summary>发现时间</summary>
    [Required]
    [Column("discovered_at")]
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

    /// <summary>状态：open / investigating / resolved / closed</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "open";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
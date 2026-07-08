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
    public long Id { get; set; }

    /// <summary>不良代码</summary>
    [Required]
    [MaxLength(50)]
    public string DefectCode { get; set; } = string.Empty;

    /// <summary>严重等级：critical / major / minor</summary>
    [Required]
    [MaxLength(10)]
    public string Severity { get; set; } = "major";

    /// <summary>来源类型：iqc / ipqc / fqc / oqc / customer</summary>
    [Required]
    [MaxLength(10)]
    public string SourceType { get; set; } = string.Empty;

    /// <summary>来源ID（检验单/客诉等）</summary>
    public long? SourceId { get; set; }

    /// <summary>关联产品</summary>
    public long? ProductId { get; set; }

    /// <summary>关联批次</summary>
    public long? BatchId { get; set; }

    /// <summary>关联设备</summary>
    public long? EquipmentId { get; set; }

    /// <summary>不良数量</summary>
    [Required]
    [Column(TypeName = "decimal(15,2)")]
    public decimal Quantity { get; set; }

    /// <summary>不良描述</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>图片URLs</summary>
    public string? ImageUrls { get; set; }

    /// <summary>发现人</summary>
    public long? DiscoveredBy { get; set; }

    /// <summary>发现时间</summary>
    [Required]
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

    /// <summary>状态：open / investigating / resolved / closed</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 巡检结果明细
/// </summary>
[Table("ipqc_patrol_items")]
public class IpqcPatrolItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联巡检记录</summary>
    public long PatrolId { get; set; }

    /// <summary>检验项目名称</summary>
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>检验项目编码</summary>
    [MaxLength(50)]
    public string? ItemCode { get; set; }

    /// <summary>规格上限</summary>
    public decimal? Usl { get; set; }

    /// <summary>规格下限</summary>
    public decimal? Lsl { get; set; }

    /// <summary>数据类型</summary>
    [Required]
    [MaxLength(20)]
    public string DataType { get; set; } = "numeric";

    /// <summary>实测值</summary>
    public decimal? ActualValue { get; set; }

    /// <summary>结果：pass/fail/pending</summary>
    [Required]
    [MaxLength(10)]
    public string Result { get; set; } = "pending";

    /// <summary>图片 URLs (JSON array)</summary>
    public string? ImageUrls { get; set; }

    // Navigation
    [ForeignKey(nameof(PatrolId))]
    public IpqcPatrol? Patrol { get; set; }
}

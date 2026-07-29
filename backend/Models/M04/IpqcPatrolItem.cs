using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 巡检结果明细
/// </summary>
[Table("ipqc_patrol_items")]
public class IpqcPatrolItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联巡检记录</summary>
    [Column("patrol_id")]
    public long PatrolId { get; set; }

    /// <summary>关联检验项目主数据</summary>
    [Column("inspection_item_id")]
    public long? InspectionItemId { get; set; }

    /// <summary>检验项目名称</summary>
    [Required]
    [MaxLength(200)]
    [Column("item_name")]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>检验项目编码</summary>
    [MaxLength(50)]
    [Column("item_code")]
    public string? ItemCode { get; set; }

    /// <summary>规格上限</summary>
    [Column("usl")]
    public decimal? Usl { get; set; }

    /// <summary>规格下限</summary>
    [Column("lsl")]
    public decimal? Lsl { get; set; }

    /// <summary>数据类型</summary>
    [Required]
    [MaxLength(20)]
    [Column("data_type")]
    public string DataType { get; set; } = "numeric";

    /// <summary>实测值</summary>
    [Column("actual_value")]
    public decimal? ActualValue { get; set; }

    /// <summary>结果：pass/fail/pending</summary>
    [Required]
    [MaxLength(10)]
    [Column("result")]
    public string Result { get; set; } = "pending";

    /// <summary>图片 URLs (JSON array)</summary>
    [Column("image_urls")]
    public string? ImageUrls { get; set; }

    // Navigation
    [ForeignKey(nameof(PatrolId))]
    public IpqcPatrol? Patrol { get; set; }

    [ForeignKey(nameof(InspectionItemId))]
    public InspectionItem? InspectionItem { get; set; }
}

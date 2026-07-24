using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_Inspection;

/// <summary>
/// 检验项目主数据（品质部统一管理）
/// 这是S3/S4/S5/S6贯通的**核心主数据**
/// 维度：产品、材料、供应商、客户、工艺、设备
/// 每个检验项目可独立设置规格限、管理限、目标值、数采参数、控制图类型
/// </summary>
[Table("inspection_items")]
public class InspectionItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>检验项目编码（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("item_code")]
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>检验项目名称</summary>
    [Required]
    [MaxLength(200)]
    [Column("item_name")]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>描述</summary>
    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>数据类型：numeric / visual / attribute</summary>
    [Required]
    [MaxLength(20)]
    [Column("data_type")]
    public string DataType { get; set; } = "numeric";

    /// <summary>单位</summary>
    [MaxLength(50)]
    [Column("unit")]
    public string? Unit { get; set; }

    // ═══ 规格上下限（Spec Limits）═══
    /// <summary>规格上限 USL</summary>
    [Column("usl")]
    public decimal? Usl { get; set; }

    /// <summary>规格下限 LSL</summary>
    [Column("lsl")]
    public decimal? Lsl { get; set; }

    /// <summary>目标值 Target</summary>
    [Column("target_value", TypeName = "decimal(15,6)")]
    public decimal? TargetValue { get; set; }

    // ═══ 管理上下限（Control Limits - SPC用）═══
    /// <summary>管理上限 UCL</summary>
    [Column("ucl", TypeName = "decimal(15,6)")]
    public decimal? Ucl { get; set; }

    /// <summary>管理下限 LCL</summary>
    [Column("lcl", TypeName = "decimal(15,6)")]
    public decimal? Lcl { get; set; }

    // ═══ 数采参数（关联 DynamicParam）═══
    /// <summary>数采参数编码（关联 dynamic_params.code）</summary>
    [MaxLength(50)]
    [Column("data_collection_param_code")]
    public string? DataCollectionParamCode { get; set; }

    // ═══ SPC控制图类型 ═══
    /// <summary>控制图类型：none / Xbar_R / Xbar_S / I_MR / P / U / C</summary>
    [MaxLength(20)]
    [Column("chart_type")]
    public string? ChartType { get; set; }

    /// <summary>默认子组大小（用于SPC控制图）</summary>
    [Column("subgroup_size")]
    public int? SubgroupSize { get; set; }

    /// <summary>检验方法/工具</summary>
    [MaxLength(200)]
    [Column("inspection_method")]
    public string? InspectionMethod { get; set; }

    /// <summary>默认抽样数量</summary>
    [Column("sample_size")]
    public int? SampleSize { get; set; }

    /// <summary>是否启用</summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public long CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }
}

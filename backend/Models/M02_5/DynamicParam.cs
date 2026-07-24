using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_5;

/// <summary>
/// 自定义参数定义 — 各检验环节的动态参数配置
/// </summary>
[Table("dynamic_params")]
public class DynamicParam
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>所属参数组ID</summary>
    [Column("group_id")]
    public long GroupId { get; set; }

    /// <summary>参数名</summary>
    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>参数编码（全局唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>数据类型：numeric/categorical/boolean</summary>
    [Required]
    [MaxLength(20)]
    [Column("data_type")]
    public string DataType { get; set; } = "numeric";

    /// <summary>单位</summary>
    [MaxLength(20)]
    [Column("unit")]
    public string? Unit { get; set; }

    /// <summary>目标值</summary>
    [Column(TypeName = "decimal(15,6)")]
    public decimal? TargetValue { get; set; }

    /// <summary>上规格限</summary>
    [Column("usl")]
    public decimal? Usl { get; set; }

    /// <summary>下规格限</summary>
    [Column("lsl")]
    public decimal? Lsl { get; set; }

    /// <summary>精度/小数位数</summary>
    [Column("precision")]
    public decimal Precision { get; set; } = 1.0m;

    /// <summary>AI 策略预置配置（JSON）</summary>
    [Column("ai_strategy")]
    public string? AiStrategy { get; set; }

    /// <summary>排序号</summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>是否启用</summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人用户ID</summary>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(GroupId))]
    public ParamGroup Group { get; set; } = null!;
}

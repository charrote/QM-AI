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
    public long Id { get; set; }

    /// <summary>所属参数组ID</summary>
    public long GroupId { get; set; }

    /// <summary>参数名</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>参数编码（全局唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>数据类型：numeric/categorical/boolean</summary>
    [Required]
    [MaxLength(20)]
    public string DataType { get; set; } = "numeric";

    /// <summary>单位</summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>目标值</summary>
    [Column(TypeName = "decimal(15,6)")]
    public decimal? TargetValue { get; set; }

    /// <summary>上规格限</summary>
    [Column(TypeName = "decimal(15,6)")]
    public decimal? Usl { get; set; }

    /// <summary>下规格限</summary>
    [Column(TypeName = "decimal(15,6)")]
    public decimal? Lsl { get; set; }

    /// <summary>精度/小数位数</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Precision { get; set; } = 1.0m;

    /// <summary>AI 策略预置配置（JSON）</summary>
    [Column(TypeName = "json")]
    public string? AiStrategy { get; set; }

    /// <summary>排序号</summary>
    public int SortOrder { get; set; }

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人用户ID</summary>
    public long CreatedBy { get; set; }

    // Navigation
    [ForeignKey(nameof(GroupId))]
    public ParamGroup Group { get; set; } = null!;
}

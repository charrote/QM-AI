using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("tools")]
public class Tool
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("tool_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("tool_name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>刀具型号</summary>
    [MaxLength(200)]
    [Column("model")]
    public string? Model { get; set; }

    /// <summary>刀具类型：车刀/铣刀/钻头/磨具/其他</summary>
    [MaxLength(50)]
    [Column("tool_type")]
    public string? ToolType { get; set; }

    /// <summary>设计寿命（加工次数或小时）</summary>
    [Column("design_life")]
    public double? DesignLife { get; set; }

    /// <summary>寿命单位：cycles/hours</summary>
    [MaxLength(20)]
    [Column("life_unit")]
    public string? LifeUnit { get; set; } = "cycles";

    /// <summary>当前已用寿命</summary>
    [Column("life_current")]
    public double CurrentLife { get; set; } = 0;

    /// <summary>供应商</summary>
    [MaxLength(200)]
    [Column("supplier")]
    public string? Supplier { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

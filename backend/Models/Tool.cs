using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("Tools")]
public class Tool
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>刀具型号</summary>
    [MaxLength(200)]
    public string? Model { get; set; }

    /// <summary>刀具类型：车刀/铣刀/钻头/磨具/其他</summary>
    [MaxLength(50)]
    public string? ToolType { get; set; }

    /// <summary>设计寿命（加工次数或小时）</summary>
    public double? DesignLife { get; set; }

    /// <summary>寿命单位：cycles/hours</summary>
    [MaxLength(20)]
    public string? LifeUnit { get; set; } = "cycles";

    /// <summary>当前已用寿命</summary>
    public double CurrentLife { get; set; } = 0;

    /// <summary>供应商</summary>
    [MaxLength(200)]
    public string? Supplier { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

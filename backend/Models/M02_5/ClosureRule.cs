using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_5;

/// <summary>
/// 关单策略模板 — 定义检验工序的自动关单条件
/// </summary>
[Table("closure_rules")]
public class ClosureRule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>规则名称</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>规则编码（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>条件表达式 JSON</summary>
    [Required]
    [Column(TypeName = "json")]
    public string ConditionJson { get; set; } = "[]";

    /// <summary>逻辑运算符 AND/OR</summary>
    [Required]
    [MaxLength(5)]
    public string Logic { get; set; } = "AND";

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人用户ID</summary>
    public long CreatedBy { get; set; }
}

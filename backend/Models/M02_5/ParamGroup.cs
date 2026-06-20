using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_5;

/// <summary>
/// 参数组 — 用于对动态参数进行分组管理
/// </summary>
[Table("param_groups")]
public class ParamGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>组名</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>组编码（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>排序号</summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人用户ID</summary>
    public long CreatedBy { get; set; }

    // Navigation
    public ICollection<DynamicParam> DynamicParams { get; set; } = new List<DynamicParam>();
}

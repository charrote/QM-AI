using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

/// <summary>
/// 企业组织层级（自引用树形结构）
/// </summary>
[Table("organizations")]
public class Organization
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>组织编码</summary>
    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>组织名称</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>层级: group/company/workshop/line</summary>
    [Required]
    [MaxLength(20)]
    public string Level { get; set; } = string.Empty;

    /// <summary>父级组织ID</summary>
    public int? ParentId { get; set; }

    /// <summary>排序号</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>位置/地址</summary>
    [MaxLength(500)]
    public string? Location { get; set; }

    /// <summary>联系人信息 (JSON)</summary>
    public string? Contact { get; set; }

    /// <summary>描述</summary>
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人</summary>
    public int? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ParentId))]
    public Organization? Parent { get; set; }

    public ICollection<Organization> Children { get; set; } = new List<Organization>();
}

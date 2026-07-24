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
    [Column("id")]
    public long Id { get; set; }

    /// <summary>组织编码</summary>
    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>组织名称</summary>
    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>层级: group/company/workshop/line</summary>
    [Required]
    [MaxLength(20)]
    [Column("level")]
    public string Level { get; set; } = string.Empty;

    /// <summary>父级组织ID</summary>
    [Column("parent_id")]
    public long? ParentId { get; set; }

    /// <summary>排序号</summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    /// <summary>是否启用</summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>位置/地址</summary>
    [MaxLength(500)]
    [Column("location")]
    public string? Location { get; set; }

    /// <summary>联系人信息 (JSON)</summary>
    [Column("contact")]
    public string? Contact { get; set; }

    /// <summary>描述</summary>
    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>创建人</summary>
    [Column("created_by")]
    public long? CreatedBy { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ParentId))]
    public Organization? Parent { get; set; }

    public ICollection<Organization> Children { get; set; } = new List<Organization>();
}

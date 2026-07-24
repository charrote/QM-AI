using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

/// <summary>
/// 系统字典类型（下拉列表数据源）
/// </summary>
[Table("sys_dict_types")]
public class SysDictType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>字典类型编码</summary>
    [Required]
    [MaxLength(50)]
    [Column("type_code")]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>字典类型名称</summary>
    [Required]
    [MaxLength(200)]
    [Column("type_name")]
    public string TypeName { get; set; } = string.Empty;

    /// <summary>是否系统内置</summary>
    [Column("is_system")]
    public bool IsSystem { get; set; } = false;

    /// <summary>状态</summary>
    [Column("status")]
    public bool Status { get; set; } = true;

    /// <summary>备注</summary>
    [Column("remark")]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<SysDictItem> Items { get; set; } = new List<SysDictItem>();
}

/// <summary>
/// 系统字典项（下拉列表选项）
/// </summary>
[Table("sys_dict_items")]
public class SysDictItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>字典类型编码</summary>
    [Required]
    [MaxLength(50)]
    [Column("type_code")]
    public string TypeCode { get; set; } = string.Empty;

    /// <summary>显示标签</summary>
    [Required]
    [MaxLength(200)]
    [Column("item_label")]
    public string ItemLabel { get; set; } = string.Empty;

    /// <summary>选项值</summary>
    [Required]
    [MaxLength(100)]
    [Column("item_value")]
    public string ItemValue { get; set; } = string.Empty;

    /// <summary>排序号</summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    /// <summary>颜色标识</summary>
    [MaxLength(20)]
    public string? Color { get; set; }

    /// <summary>是否默认</summary>
    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    /// <summary>状态</summary>
    [Column("status")]
    public bool Status { get; set; } = true;

    /// <summary>备注</summary>
    [Column("remark")]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(TypeCode))]
    public SysDictType DictType { get; set; } = null!;
}

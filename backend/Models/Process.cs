using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("processes")]
public class Process
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("process_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("process_name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>工序类型：加工/装配/检验/包装</summary>
    [MaxLength(50)]
    [Column("process_type")]
    public string? ProcessType { get; set; }

    /// <summary>所属部门/车间（显示冗余）</summary>
    [MaxLength(100)]
    [Column("department")]
    public string? Department { get; set; }

    /// <summary>所属组织ID</summary>
    [Column("org_id")]
    public long? OrgId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

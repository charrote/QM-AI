using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("Processes")]
public class Process
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

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>工序类型：加工/装配/检验/包装</summary>
    [MaxLength(50)]
    public string? ProcessType { get; set; }

    /// <summary>所属部门/车间（显示冗余）</summary>
    [MaxLength(100)]
    public string? Department { get; set; }

    /// <summary>所属组织ID</summary>
    public int? OrgId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

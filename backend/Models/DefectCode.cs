using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("defect_codes")]
public class DefectCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("defect_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("defect_name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>不良类别：外观/尺寸/功能/材料/其他</summary>
    [MaxLength(50)]
    [Column("defect_category")]
    public string? DefectType { get; set; }

    /// <summary>严重等级：CR/MA/MI</summary>
    [Required]
    [MaxLength(10)]
    [Column("defect_severity")]
    public string Severity { get; set; } = "MI";

    /// <summary>是否可返工</summary>
    [Column("is_reworkable")]
    public bool IsReworkable { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

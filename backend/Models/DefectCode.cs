using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("DefectCodes")]
public class DefectCode
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>不良类别：外观/尺寸/功能/材料/其他</summary>
    [MaxLength(50)]
    public string? DefectType { get; set; }

    /// <summary>严重等级：CR/MA/MI</summary>
    [Required]
    [MaxLength(10)]
    public string Severity { get; set; } = "MI";

    /// <summary>是否可返工</summary>
    public bool IsReworkable { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

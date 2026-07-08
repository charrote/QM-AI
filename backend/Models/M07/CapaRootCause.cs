using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// CAPA 原因分析
/// </summary>
[Table("capa_root_causes")]
public class CapaRootCause
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long CapaId { get; set; }

    /// <summary>分析方法：five_whys / fishbone / other</summary>
    [Required]
    [MaxLength(20)]
    public string AnalysisMethod { get; set; } = "five_whys";

    /// <summary>分析内容（5Why问答/鱼骨图数据 JSON）</summary>
    [Required]
    public string Content { get; set; } = "[]";

    /// <summary>根本原因总结</summary>
    [Required]
    public string RootCauseSummary { get; set; } = string.Empty;

    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
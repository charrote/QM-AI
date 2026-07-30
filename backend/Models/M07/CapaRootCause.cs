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
    [Column("id")]
    public long Id { get; set; }

    [Column("capa_id")]
    public long CapaId { get; set; }

    /// <summary>分析方法：five_whys / fishbone / other</summary>
    [Required]
    [MaxLength(20)]
    [Column("analysis_method")]
    public string AnalysisMethod { get; set; } = "five_whys";

    /// <summary>分析内容（5Why问答/鱼骨图数据 JSON）</summary>
    [Required]
    [Column("content")]
    public string Content { get; set; } = "[]";

    /// <summary>根本原因总结</summary>
    [Required]
    [Column("root_cause_summary")]
    public string RootCauseSummary { get; set; } = string.Empty;

    [Column("created_by")]
    public long CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
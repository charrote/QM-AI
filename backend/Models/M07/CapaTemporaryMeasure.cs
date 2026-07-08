using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// CAPA 临时措施（围堵）
/// </summary>
[Table("capa_temporary_measures")]
public class CapaTemporaryMeasure
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long CapaId { get; set; }

    /// <summary>措施描述</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>执行人</summary>
    public long? ExecutedBy { get; set; }

    /// <summary>执行时间</summary>
    public DateTime? ExecutedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
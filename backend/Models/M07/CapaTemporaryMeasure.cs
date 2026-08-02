using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace QM_AI.API.Models.M07;

/// <summary>
/// CAPA 临时措施（围堵）
/// </summary>
[Table("capa_temporary_measures")]
public class CapaTemporaryMeasure
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("capa_id")]
    public long CapaId { get; set; }

    /// <summary>措施描述</summary>
    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>执行人</summary>
    [Column("executed_by")]
    public long? ExecutedBy { get; set; }

    /// <summary>执行时间</summary>
    [Column("executed_at")]
    public DateTime? ExecutedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    [JsonIgnore]
    public Capa? Capa { get; set; }
}
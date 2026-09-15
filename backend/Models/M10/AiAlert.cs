using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M10;

[Table("ai_alerts")]
public class AiAlert
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("alert_no")]
    public string AlertNo { get; set; } = "";

    /// <summary>预警级别：critical / warning / info</summary>
    [Column("level")]
    public string Level { get; set; } = "warning";

    [Column("title")]
    public string Title { get; set; } = "";

    [Column("description")]
    public string? Description { get; set; }

    /// <summary>来源模块：spc / iqc / fqc / ipqc / anomaly / capa / complaint</summary>
    [Column("source")]
    public string Source { get; set; } = "";

    [Column("source_ref_id")]
    public long? SourceRefId { get; set; }

    [Column("product_id")]
    public long? ProductId { get; set; }

    [Column("resolved")]
    public bool Resolved { get; set; }

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [Column("resolved_by")]
    public long? ResolvedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_alert_triggers")]
public class SpcAlertTrigger
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("chart_id")]
    public long ChartId { get; set; }

    [Column("rule_id")]
    public long RuleId { get; set; }

    [Column("rule_number")]
    public int RuleNumber { get; set; }

    [Column("triggered_at")]
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    [Column("violated_point_index")]
    public int ViolatedPointIndex { get; set; }

    [Column("detail", TypeName = "json")]
    public string? Detail { get; set; }

    [Column("resolved")]
    public bool Resolved { get; set; } = false;

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }

    [ForeignKey(nameof(RuleId))]
    public SpcAlertRule? Rule { get; set; }
}

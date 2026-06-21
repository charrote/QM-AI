using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_alert_triggers")]
public class SpcAlertTrigger
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ChartId { get; set; }

    public long RuleId { get; set; }

    public int RuleNumber { get; set; }

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    public int ViolatedPointIndex { get; set; }

    [Column(TypeName = "json")]
    public string? Detail { get; set; }

    public bool Resolved { get; set; } = false;

    public DateTime? ResolvedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }

    [ForeignKey(nameof(RuleId))]
    public SpcAlertRule? Rule { get; set; }
}

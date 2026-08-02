using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_alert_rules")]
public class SpcAlertRule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("chart_id")]
    public long ChartId { get; set; }

    [Column("rule_number")]
    public int RuleNumber { get; set; } // 1-8

    [Column("rule_name")]
    [Required]
    [MaxLength(200)]
    public string RuleName { get; set; } = string.Empty;

    [Column("rule_description")]
    public string? RuleDescription { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    [Column("trigger_threshold")]
    public int TriggerThreshold { get; set; } = 1; // e.g., consecutive N points

    [Column("sigma_threshold", TypeName = "decimal(5,2)")]
    public decimal SigmaThreshold { get; set; } = 2.0m;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }

    public ICollection<SpcAlertTrigger> Triggers { get; set; } = new List<SpcAlertTrigger>();
}

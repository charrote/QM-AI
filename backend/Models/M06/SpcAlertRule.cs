using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_alert_rules")]
public class SpcAlertRule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ChartId { get; set; }

    public int RuleNumber { get; set; } // 1-8

    [Required]
    [MaxLength(200)]
    public string RuleName { get; set; } = string.Empty;

    public string? RuleDescription { get; set; }

    public bool Enabled { get; set; } = true;

    public int TriggerThreshold { get; set; } = 1; // e.g., consecutive N points

    [Column(TypeName = "decimal(5,2)")]
    public decimal SigmaThreshold { get; set; } = 2.0m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }

    public ICollection<SpcAlertTrigger> Triggers { get; set; } = new List<SpcAlertTrigger>();
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_analysis_results")]
public class SpcAnalysisResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    public long ChartId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AnalysisType { get; set; } = "cpk"; // cpk, ppk, capability

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Cp { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Cpk { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Pp { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal? Ppk { get; set; }

    [Column(TypeName = "decimal(15,6)")]
    public decimal? SigmaWithin { get; set; }

    [Column(TypeName = "decimal(15,6)")]
    public decimal? SigmaOverall { get; set; }

    [Column(TypeName = "decimal(15,2)")]
    public decimal? EstimatedPpm { get; set; }

    public int? DataPointsUsed { get; set; }

    public DateTime? AnalysisPeriodStart { get; set; }

    public DateTime? AnalysisPeriodEnd { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }
}

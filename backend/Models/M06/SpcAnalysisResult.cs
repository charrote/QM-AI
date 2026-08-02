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

    [Column("chart_id")]
    public long ChartId { get; set; }

    [Column("analysis_type")]
    [Required]
    [MaxLength(20)]
    public string AnalysisType { get; set; } = "cpk"; // cpk, ppk, capability

    [Column("cp", TypeName = "decimal(10,4)")]
    public decimal? Cp { get; set; }

    [Column("cpk", TypeName = "decimal(10,4)")]
    public decimal? Cpk { get; set; }

    [Column("pp", TypeName = "decimal(10,4)")]
    public decimal? Pp { get; set; }

    [Column("ppk", TypeName = "decimal(10,4)")]
    public decimal? Ppk { get; set; }

    [Column("sigma_within", TypeName = "decimal(15,6)")]
    public decimal? SigmaWithin { get; set; }

    [Column("sigma_overall", TypeName = "decimal(15,6)")]
    public decimal? SigmaOverall { get; set; }

    [Column("estimated_ppm", TypeName = "decimal(15,2)")]
    public decimal? EstimatedPpm { get; set; }

    [Column("data_points_used")]
    public int? DataPointsUsed { get; set; }

    [Column("analysis_period_start")]
    public DateTime? AnalysisPeriodStart { get; set; }

    [Column("analysis_period_end")]
    public DateTime? AnalysisPeriodEnd { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }
}

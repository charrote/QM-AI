using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_control_charts")]
public class SpcControlChart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    public long ProcessId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("parameter_code")]
    public string ParameterCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [Column("chart_type")]
    public string ChartType { get; set; } = "Xbar_R"; // Xbar_R, Xbar_S, I_MR

    [Column("subgroup_size")]
    public int SubgroupSize { get; set; } = 5;

    [Column("usl")]
    public decimal? Usl { get; set; }

    [Column("lsl")]
    public decimal? Lsl { get; set; }

    [Column(TypeName = "decimal(15,6)")]
    public decimal? TargetValue { get; set; }

    [Column(TypeName = "decimal(15,6)")]
    public decimal? Cl { get; set; } // Center Line

    [Column(TypeName = "decimal(15,6)")]
    public decimal? Ucl { get; set; } // Upper Control Limit

    [Column(TypeName = "decimal(15,6)")]
    public decimal? Lcl { get; set; } // Lower Control Limit

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("created_by")]
    public long CreatedBy { get; set; }

    // Navigation
    public ICollection<SpcDataPoint> DataPoints { get; set; } = new List<SpcDataPoint>();
    public ICollection<SpcAnalysisResult> AnalysisResults { get; set; } = new List<SpcAnalysisResult>();
    public ICollection<SpcAlertRule> AlertRules { get; set; } = new List<SpcAlertRule>();
    public ICollection<SpcDataSource> DataSources { get; set; } = new List<SpcDataSource>();
}

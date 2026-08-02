using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_anova_results")]
public class SpcAnovaResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("chart_id")]
    public long ChartId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("source")]
    public string Source { get; set; } = string.Empty; // operator, machine, material, method, environment

    [Column("sum_of_squares", TypeName = "decimal(20,4)")]
    public decimal SumOfSquares { get; set; }

    [Column("degrees_freedom")]
    public int DegreesFreedom { get; set; }

    [Column("mean_square", TypeName = "decimal(20,4)")]
    public decimal MeanSquare { get; set; }

    [Column("f_ratio", TypeName = "decimal(10,4)")]
    public decimal FRatio { get; set; }

    [Column("p_value", TypeName = "decimal(10,6)")]
    public decimal PValue { get; set; }

    [Column("significant")]
    public bool Significant { get; set; } = false;

    [Column("analysis_date")]
    public DateTime AnalysisDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }
}

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

    public long ChartId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("source")]
    public string Source { get; set; } = string.Empty; // operator, machine, material, method, environment

    [Column(TypeName = "decimal(20,4)")]
    public decimal SumOfSquares { get; set; }

    public int DegreesFreedom { get; set; }

    [Column(TypeName = "decimal(20,4)")]
    public decimal MeanSquare { get; set; }

    [Column(TypeName = "decimal(10,4)")]
    public decimal FRatio { get; set; }

    [Column(TypeName = "decimal(10,6)")]
    public decimal PValue { get; set; }

    public bool Significant { get; set; } = false;

    [Column("analysis_date")]
    public DateTime AnalysisDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }
}

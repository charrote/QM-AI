using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_data_points")]
public class SpcDataPoint
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ChartId { get; set; }

    public int SubgroupIndex { get; set; }

    [Required]
    [Column(TypeName = "json")]
    public string IndividualValues { get; set; } = "[]"; // JSON array of raw values

    [Column(TypeName = "decimal(15,6)")]
    public decimal? SubgroupMean { get; set; } // X̄

    [Column(TypeName = "decimal(15,6)")]
    public decimal? SubgroupRange { get; set; } // R (or S for Xbar_S)

    public DateTime MeasuredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }
}

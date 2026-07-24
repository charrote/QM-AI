using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M06;

[Table("spc_data_points")]
public class SpcDataPoint
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    public long ChartId { get; set; }

    [Column("subgroup_index")]
    public int SubgroupIndex { get; set; }

    [Required]
    [Column("individual_values")]
    public string IndividualValues { get; set; } = "[]"; // JSON array of raw values

    [Column("subgroup_mean")]
    public decimal? SubgroupMean { get; set; } // X̄

    [Column("subgroup_range")]
    public decimal? SubgroupRange { get; set; } // R (or S for Xbar_S)

    public DateTime MeasuredAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M11;

/// <summary>
/// 设备-质量关联分析结果
/// </summary>
[Table("equipment_quality_correlation")]
public class EquipmentQualityCorrelation
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    [Required]
    [Column("analysis_date")]
    public DateOnly AnalysisDate { get; set; }

    [Column("correlation_data")]
    public string CorrelationData { get; set; } = "{}";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EquipmentId))]
    public virtual Equipment? Equipment { get; set; } = null!;
}

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
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联设备</summary>
    [Required]
    public long EquipmentId { get; set; }

    /// <summary>分析日期</summary>
    [Required]
    public DateOnly AnalysisDate { get; set; }

    /// <summary>关联分析结果（JSON）</summary>
    public string CorrelationData { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(EquipmentId))]
    public virtual Equipment? Equipment { get; set; } = null!;
}

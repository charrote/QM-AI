using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_5;

/// <summary>
/// 实时参数值 — 高频写入，按参数编码+时间索引
/// </summary>
[Table("param_realtime_values")]
public class ParamRealtimeValue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>参数编码</summary>
    [Required]
    [MaxLength(50)]
    public string ParamCode { get; set; } = string.Empty;

    /// <summary>设备ID</summary>
    [Column("equipment_id")]
    public long? EquipmentId { get; set; }

    /// <summary>数值（数值型参数）</summary>
    [Column("value")]
    public decimal? Value { get; set; }

    /// <summary>原始值（枚举型/布尔型）</summary>
    [MaxLength(100)]
    public string? ValueRaw { get; set; }

    /// <summary>采集时间</summary>
    [Required]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>质量结果</summary>
    [MaxLength(10)]
    [Column("quality_result")]
    public string? QualityResult { get; set; } = "UNKNOWN";
}

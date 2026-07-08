using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC AI 风险评分历史
/// </summary>
[Table("ipqc_ai_risk_scores")]
public class IpqcAiRiskScore
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联设备</summary>
    public long EquipmentId { get; set; }

    /// <summary>关联工序</summary>
    public long ProcessId { get; set; }

    /// <summary>关联工单（可选）</summary>
    public long? WorkOrderId { get; set; }

    /// <summary>风险评分 0-100</summary>
    public int RiskScore { get; set; }

    /// <summary>风险等级：normal/warning/critical</summary>
    [Required]
    [MaxLength(20)]
    public string RiskLevel { get; set; } = "normal";

    /// <summary>风险因素分解 (JSON)</summary>
    public string? FactorsJson { get; set; }

    /// <summary>趋势方向：stable/rising/falling</summary>
    [MaxLength(10)]
    public string? TrendDirection { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

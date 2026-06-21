using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 工单关单状态
/// </summary>
[Table("ipqc_closure_status")]
public class IpqcClosureStatus
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联工单 ID</summary>
    public long WorkOrderId { get; set; }

    /// <summary>状态：open/closed</summary>
    [Required]
    [MaxLength(10)]
    public string Status { get; set; } = "open";

    /// <summary>关单时间</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>关联关单规则</summary>
    public long? RuleId { get; set; }

    /// <summary>关单评估结果 (JSON)</summary>
    public string? EvaluationResult { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

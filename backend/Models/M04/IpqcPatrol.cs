using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 巡检记录
/// </summary>
[Table("ipqc_patrols")]
public class IpqcPatrol
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>巡检编号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string PatrolNo { get; set; } = string.Empty;

    /// <summary>关联巡检计划</summary>
    public long PatrolPlanId { get; set; }

    /// <summary>关联工单（可选）</summary>
    public long? WorkOrderId { get; set; }

    /// <summary>关联工序</summary>
    public long ProcessId { get; set; }

    /// <summary>关联设备</summary>
    public long EquipmentId { get; set; }

    /// <summary>检验员</summary>
    public long InspectorId { get; set; }

    /// <summary>计划时间</summary>
    public DateTime ScheduledTime { get; set; }

    /// <summary>实际执行时间</summary>
    public DateTime? ActualTime { get; set; }

    /// <summary>总检验数</summary>
    public int TotalChecked { get; set; }

    /// <summary>合格数</summary>
    public int TotalPass { get; set; }

    /// <summary>不合格数</summary>
    public int TotalFail { get; set; }

    /// <summary>结论：qualified/unqualified/pending</summary>
    [Required]
    [MaxLength(20)]
    public string Conclusion { get; set; } = "pending";

    /// <summary>状态：scheduled/in_progress/completed/missed/cancelled</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "scheduled";

    /// <summary>备注</summary>
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(PatrolPlanId))]
    public IpqcPatrolPlan? PatrolPlan { get; set; }

    public ICollection<IpqcPatrolItem>? Items { get; set; }
}

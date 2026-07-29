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
    [Column("id")]
    public long Id { get; set; }

    /// <summary>巡检编号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("patrol_no")]
    public string PatrolNo { get; set; } = string.Empty;

    /// <summary>关联巡检计划</summary>
    [Column("patrol_plan_id")]
    public long PatrolPlanId { get; set; }

    /// <summary>关联工单（可选）</summary>
    [Column("work_order_id")]
    public long? WorkOrderId { get; set; }

    /// <summary>关联工序</summary>
    [Column("process_id")]
    public long ProcessId { get; set; }

    /// <summary>关联设备</summary>
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>检验员</summary>
    [Column("inspector_id")]
    public long InspectorId { get; set; }

    /// <summary>计划时间</summary>
    [Column("scheduled_time")]
    public DateTime ScheduledTime { get; set; }

    /// <summary>实际执行时间</summary>
    [Column("actual_time")]
    public DateTime? ActualTime { get; set; }

    /// <summary>总检验数</summary>
    [Column("total_checked")]
    public int TotalChecked { get; set; }

    /// <summary>合格数</summary>
    [Column("total_pass")]
    public int TotalPass { get; set; }

    /// <summary>不合格数</summary>
    [Column("total_fail")]
    public int TotalFail { get; set; }

    /// <summary>结论：qualified/unqualified/pending</summary>
    [Required]
    [MaxLength(20)]
    [Column("conclusion")]
    public string Conclusion { get; set; } = "pending";

    /// <summary>状态：scheduled/in_progress/completed/missed/cancelled</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "scheduled";

    /// <summary>备注</summary>
    [Column("remarks")]
    public string? Remarks { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(PatrolPlanId))]
    public IpqcPatrolPlan? PatrolPlan { get; set; }

    public ICollection<IpqcPatrolItem>? Items { get; set; }
}

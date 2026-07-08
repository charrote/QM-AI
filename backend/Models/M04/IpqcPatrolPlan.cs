using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 巡检计划（按频次自动生成）
/// </summary>
[Table("ipqc_patrol_plans")]
public class IpqcPatrolPlan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>计划编号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string PlanNo { get; set; } = string.Empty;

    /// <summary>关联工序</summary>
    public long ProcessId { get; set; }

    /// <summary>关联设备</summary>
    public long EquipmentId { get; set; }

    /// <summary>巡检间隔（分钟）</summary>
    public int PatrolIntervalMin { get; set; }

    /// <summary>是否自动生成</summary>
    public bool AutoGenerate { get; set; } = true;

    /// <summary>状态：active/paused/completed</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "active";

    /// <summary>默认检验员</summary>
    [MaxLength(100)]
    public string? Inspector { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<IpqcPatrol>? Patrols { get; set; }
}

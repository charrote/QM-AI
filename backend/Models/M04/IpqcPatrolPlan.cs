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
    [Column("id")]
    public long Id { get; set; }

    /// <summary>计划编号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("plan_no")]
    public string PlanNo { get; set; } = string.Empty;

    /// <summary>关联工序</summary>
    [Column("process_id")]
    public long ProcessId { get; set; }

    /// <summary>关联设备</summary>
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>巡检间隔（分钟）</summary>
    [Column("patrol_interval_min")]
    public int PatrolIntervalMin { get; set; }

    /// <summary>是否自动生成</summary>
    [Column("auto_generate")]
    public bool AutoGenerate { get; set; } = true;

    /// <summary>状态：active/paused/completed</summary>
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active";

    /// <summary>默认检验员</summary>
    [MaxLength(100)]
    [Column("inspector")]
    public string? Inspector { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<IpqcPatrol>? Patrols { get; set; }
}

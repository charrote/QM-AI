using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// 巡检计划 - 设备关联表（多对多中间表）
/// </summary>
[Table("ipqc_patrol_plan_equipment")]
public class IpqcPatrolPlanEquipment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联巡检计划</summary>
    [Column("patrol_plan_id")]
    public long PatrolPlanId { get; set; }

    /// <summary>关联设备</summary>
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>排序</summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public IpqcPatrolPlan? PatrolPlan { get; set; }
    public Equipment? Equipment { get; set; }
}

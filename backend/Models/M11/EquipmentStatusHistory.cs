using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M11;

/// <summary>
/// 设备状态历史记录
/// </summary>
[Table("equipment_status_history")]
public class EquipmentStatusHistory
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("signal_type")]
    public string Signal { get; set; } = string.Empty;

    [Column("signal_detail")]
    public string? SignalData { get; set; }

    [Column("record_time")]
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(EquipmentId))]
    public virtual Equipment? Equipment { get; set; } = null!;
}

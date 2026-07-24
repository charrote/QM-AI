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
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联设备</summary>
    [Required]
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>信号类型：running / idle / fault</summary>
    [Required]
    [MaxLength(10)]
    [Column("signal")]
    public string Signal { get; set; } = string.Empty;

    /// <summary>附加数据（如故障代码 JSON）</summary>
    public string? SignalData { get; set; }

    /// <summary>记录时间</summary>
    [Column("recorded_at")]
    public DateTime RecordedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(EquipmentId))]
    public virtual Equipment? Equipment { get; set; } = null!;
}

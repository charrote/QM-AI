using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("Equipment")]
public class Equipment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>设备型号</summary>
    [MaxLength(200)]
    public string? Model { get; set; }

    /// <summary>所在产线（显示冗余）</summary>
    [MaxLength(100)]
    public string? ProductionLine { get; set; }

    /// <summary>所在车间（显示冗余）</summary>
    [MaxLength(100)]
    public string? Workshop { get; set; }

    /// <summary>所属组织ID</summary>
    public int? OrgId { get; set; }

    /// <summary>关联车间（组织ID，level=workshop）</summary>
    public int? WorkshopId { get; set; }

    /// <summary>关联产线（组织ID，level=line）</summary>
    public int? LineId { get; set; }

    /// <summary>设备状态：running/idle/fault/maintenance</summary>
    [MaxLength(20)]
    public string Status { get; set; } = "idle";

    /// <summary>设备类型：CNC/PLC/检测设备/其他</summary>
    [MaxLength(50)]
    public string? EquipmentType { get; set; }

    /// <summary>是否关联MQTT</summary>
    public bool HasMqttConnection { get; set; } = false;

    /// <summary>MQTT Topic 前缀</summary>
    [MaxLength(500)]
    public string? MqttTopicPrefix { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

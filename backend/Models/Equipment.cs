using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("equipments")]
public class Equipment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("equipment_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("equipment_name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>设备型号</summary>
    [MaxLength(200)]
    [Column("model")]
    public string? Model { get; set; }

    /// <summary>所在产线（显示冗余）</summary>
    [MaxLength(100)]
    [Column("production_line")]
    public string? ProductionLine { get; set; }

    /// <summary>所在车间（显示冗余）</summary>
    [MaxLength(100)]
    [Column("workshop")]
    public string? Workshop { get; set; }

    /// <summary>所属组织ID</summary>
    [Column("org_id")]
    public long? OrgId { get; set; }

    /// <summary>关联车间（组织ID，level=workshop）</summary>
    [Column("workshop_id")]
    public long? WorkshopId { get; set; }

    /// <summary>关联产线（组织ID，level=line）</summary>
    [Column("line_id")]
    public long? LineId { get; set; }

    /// <summary>设备状态：running/idle/fault/maintenance</summary>
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "idle";

    /// <summary>设备类型：CNC/PLC/检测设备/其他</summary>
    [MaxLength(50)]
    [Column("equipment_type")]
    public string? EquipmentType { get; set; }

    /// <summary>是否关联MQTT</summary>
    [Column("has_mqtt_connection")]
    public bool HasMqttConnection { get; set; } = false;

    /// <summary>MQTT Topic 前缀</summary>
    [MaxLength(500)]
    [Column("mqtt_topic_prefix")]
    public string? MqttTopicPrefix { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QM_AI.API.Models.M02_5;

namespace QM_AI.API.Models.M11;

/// <summary>
/// 设备-参数映射配置
/// </summary>
[Table("equipment_param_mappings")]
public class EquipmentParamMapping
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联设备</summary>
    [Required]
    public long EquipmentId { get; set; }

    /// <summary>MQTT Topic</summary>
    [Required]
    [MaxLength(500)]
    public string MqttTopic { get; set; } = string.Empty;

    /// <summary>系统参数编码</summary>
    [Required]
    [MaxLength(50)]
    public string SystemParamCode { get; set; } = string.Empty;

    /// <summary>参数分组</summary>
    public long? ParamGroupId { get; set; }

    /// <summary>数据类型：numeric / count / status</summary>
    [Required]
    [MaxLength(10)]
    public string DataType { get; set; } = "numeric";

    /// <summary>单位</summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(EquipmentId))]
    public virtual Equipment? Equipment { get; set; } = null!;

    [ForeignKey(nameof(ParamGroupId))]
    public virtual ParamGroup? ParamGroup { get; set; } = null!;
}

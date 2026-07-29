using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 首件检验
/// </summary>
[Table("ipqc_first_pieces")]
public class IpqcFirstPiece
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>首件检验单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("fp_no")]
    public string FpNo { get; set; } = string.Empty;

    /// <summary>关联工单</summary>
    [Column("work_order_id")]
    public long WorkOrderId { get; set; }

    /// <summary>关联工序</summary>
    [Column("process_id")]
    public long ProcessId { get; set; }

    /// <summary>关联设备</summary>
    [Column("equipment_id")]
    public long EquipmentId { get; set; }

    /// <summary>操作员</summary>
    [Column("operator_id")]
    public long OperatorId { get; set; }

    /// <summary>班次：早班/中班/晚班</summary>
    [MaxLength(20)]
    [Column("shift")]
    public string? Shift { get; set; }

    /// <summary>首件原因：班次切换/换线/换刀/设备维修/首次开机</summary>
    [Required]
    [MaxLength(20)]
    [Column("reason")]
    public string Reason { get; set; } = "班次切换";

    /// <summary>结论：qualified/unqualified/pending</summary>
    [Required]
    [MaxLength(20)]
    [Column("conclusion")]
    public string Conclusion { get; set; } = "pending";

    /// <summary>是否允许量产</summary>
    [Column("allowed_to_produce")]
    public bool AllowedToProduce { get; set; }

    /// <summary>检验员</summary>
    [Column("inspector_id")]
    public long? InspectorId { get; set; }

    /// <summary>检验时间</summary>
    [Column("checked_at")]
    public DateTime? CheckedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<IpqcFirstPieceItem>? Items { get; set; }
}

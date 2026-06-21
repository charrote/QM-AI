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
    public long Id { get; set; }

    /// <summary>首件检验单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string FpNo { get; set; } = string.Empty;

    /// <summary>关联工单</summary>
    public long WorkOrderId { get; set; }

    /// <summary>关联工序</summary>
    public int ProcessId { get; set; }

    /// <summary>关联设备</summary>
    public int EquipmentId { get; set; }

    /// <summary>操作员</summary>
    public int OperatorId { get; set; }

    /// <summary>班次：早班/中班/晚班</summary>
    [MaxLength(20)]
    public string? Shift { get; set; }

    /// <summary>首件原因：班次切换/换线/换刀/设备维修/首次开机</summary>
    [Required]
    [MaxLength(20)]
    public string Reason { get; set; } = "班次切换";

    /// <summary>结论：qualified/unqualified/pending</summary>
    [Required]
    [MaxLength(20)]
    public string Conclusion { get; set; } = "pending";

    /// <summary>是否允许量产</summary>
    public bool AllowedToProduce { get; set; }

    /// <summary>检验员</summary>
    public int? InspectorId { get; set; }

    /// <summary>检验时间</summary>
    public DateTime? CheckedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<IpqcFirstPieceItem>? Items { get; set; }
}

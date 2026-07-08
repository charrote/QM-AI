using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M09;

/// <summary>
/// 客诉时间线事件
/// </summary>
[Table("complaint_events")]
public class ComplaintEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联客诉</summary>
    [Required]
    public long ComplaintId { get; set; }

    /// <summary>事件类型：created / acknowledged / status_change / d8_update / verify / close</summary>
    [Required]
    [MaxLength(20)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>事件数据（JSON）</summary>
    public string? EventData { get; set; }

    /// <summary>操作用户</summary>
    public long CreatedBy { get; set; }

    /// <summary>发生时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(ComplaintId))]
    public virtual Complaint? Complaint { get; set; } = null!;
}

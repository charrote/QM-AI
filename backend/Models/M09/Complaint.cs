using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M09;

/// <summary>
/// 客诉记录
/// </summary>
[Table("complaints")]
public class Complaint
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>客诉编号 C-yyyyMMdd-NNN</summary>
    [Required]
    [MaxLength(50)]
    public string ComplaintCode { get; set; } = string.Empty;

    /// <summary>关联客户</summary>
    public long CustomerId { get; set; }

    /// <summary>严重程度：critical / major / minor</summary>
    [Required]
    [MaxLength(10)]
    public string Severity { get; set; } = "major";

    /// <summary>客诉主题</summary>
    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>详细描述</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>状态：new / acknowledged / in_progress / overdue / awaiting_verify / closed</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "new";

    /// <summary>5W2H 问题描述（JSON）</summary>
    public string? FiveW2HJson { get; set; }

    /// <summary>指派人</summary>
    public long? AssignedTo { get; set; }

    /// <summary>截止日期</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>确认时间</summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>关闭时间</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>创建人</summary>
    public long CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>更新时间</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    public virtual Customer? Customer { get; set; } = null!;
    public virtual ICollection<D8Report>? D8Reports { get; set; } = new List<D8Report>();
    public virtual ICollection<ComplaintEvent>? Events { get; set; } = new List<ComplaintEvent>();
}

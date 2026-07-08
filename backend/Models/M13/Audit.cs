using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M13;

/// <summary>
/// 审核计划
/// </summary>
[Table("audits")]
public class Audit
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>审核编号 AUD-NNN</summary>
    [Required]
    [MaxLength(50)]
    public string AuditCode { get; set; } = string.Empty;

    /// <summary>审核类型：internal / process / product</summary>
    [Required]
    [MaxLength(10)]
    public string AuditType { get; set; } = string.Empty;

    /// <summary>审核标题</summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>开始日期</summary>
    [Required]
    public DateOnly StartDate { get; set; }

    /// <summary>结束日期</summary>
    [Required]
    public DateOnly EndDate { get; set; }

    /// <summary>审核人ID</summary>
    [Required]
    public long AuditorId { get; set; }

    /// <summary>审核人IDs（JSON数组，兼容多审核人场景）</summary>
    public string? AuditorIdsJson { get; set; }

    /// <summary>统计: 总发现数</summary>
    public int TotalFindings { get; set; }

    /// <summary>统计: 符合项数</summary>
    public int Conformities { get; set; }

    /// <summary>统计: 不符合项数</summary>
    public int NonConformities { get; set; }

    /// <summary>统计: 改进机会数</summary>
    public int Opportunities { get; set; }

    /// <summary>审核范围（JSON：产线/工序/产品）</summary>
    public string? Scope { get; set; }

    /// <summary>状态：planned / in_progress / completed / archived</summary>
    [Required]
    [MaxLength(10)]
    public string Status { get; set; } = "planned";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    public virtual ICollection<AuditFinding>? Findings { get; set; } = new List<AuditFinding>();
}

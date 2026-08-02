using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M13;

/// <summary>
/// 审核发现项
/// </summary>
[Table("audit_findings")]
public class AuditFinding
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联审核</summary>
    [Column("audit_id")]
    [Required]
    public long AuditId { get; set; }

    /// <summary>发现类型：conformity / non_conformity / opportunity</summary>
    [Required]
    [MaxLength(20)]
    [Column("finding_type")]
    public string FindingType { get; set; } = string.Empty;

    /// <summary>分类（同 FindingType，供服务层兼容读取）</summary>
    [NotMapped]
    public string Classification => FindingType;

    /// <summary>严重等级：major / minor</summary>
    [MaxLength(20)]
    [Column("severity")]
    public string? Severity { get; set; }

    /// <summary>描述</summary>
    [Required]
    [Column("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>客观证据</summary>
    [Column("evidence")]
    public string? Evidence { get; set; }

    /// <summary>实际证据（同 Evidence，供服务层兼容写入）</summary>
    [NotMapped]
    public string? ActualEvidence { get; set; }

    /// <summary>引用标准条款</summary>
    [MaxLength(200)]
    [Column("requirement_ref")]
    public string? RequirementRef { get; set; }

    /// <summary>状态：open / rectifying / verified / rejected / closed</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "open";

    /// <summary>整改措施（JSON）</summary>
    [Column("rectification_plan")]
    public string? RectificationPlan { get; set; }

    /// <summary>整改责任人ID</summary>
    [Column("responsible_user_id")]
    public long? ResponsibleUserId { get; set; }

    /// <summary>整改责任人标识（字符串，供服务层兼容）</summary>
    [NotMapped]
    public string? ResponsibleUserIdStr { get; set; }

    /// <summary>整改截止日</summary>
    [Column("rectification_due_date")]
    public DateOnly? RectificationDueDate { get; set; }

    /// <summary>计划完成日期（同 RectificationDueDate，供服务层兼容）</summary>
    [NotMapped]
    public DateTime? PlannedCompletionDate { get; set; }

    /// <summary>验证人ID</summary>
    [Column("verified_by")]
    public long? VerifiedBy { get; set; }

    /// <summary>验证人标识（字符串，供服务层兼容）</summary>
    [NotMapped]
    public string? VerifiedByStr { get; set; }

    /// <summary>验证时间</summary>
    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(AuditId))]
    public virtual Audit? Audit { get; set; } = null!;
}

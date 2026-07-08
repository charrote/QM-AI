using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M09;

/// <summary>
/// 8D 报告（一对一关联客诉）
/// </summary>
[Table("d8_reports")]
public class D8Report
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联客诉（唯一）</summary>
    [Required]
    public long ComplaintId { get; set; }

    /// <summary>D0 问题概述</summary>
    public string? D0Description { get; set; }

    /// <summary>D1 改善小组成员（JSON数组）</summary>
    public string? D1Team { get; set; }

    /// <summary>D2 问题描述(5W2H)</summary>
    public string? D2Description { get; set; }

    /// <summary>D2 问题描述别名（供旧版服务兼容）</summary>
    public string? D2ProblemDesc { get; set; }

    /// <summary>D3 临时围堵措施（JSON数组）</summary>
    public string? D3Measures { get; set; }

    /// <summary>D4 分析方法：five_whys / fishbone / other</summary>
    [MaxLength(20)]
    public string? D4AnalysisMethod { get; set; }

    /// <summary>D4 分析数据（5Why/鱼骨图 JSON）</summary>
    public string? D4Content { get; set; }

    /// <summary>D4 根本原因总结</summary>
    public string? D4RootCause { get; set; }

    /// <summary>D5 永久纠正措施（JSON数组）</summary>
    public string? D5Actions { get; set; }

    /// <summary>D6 实施验证记录（JSON数组）</summary>
    public string? D6Verification { get; set; }

    /// <summary>D7 预防措施（JSON数组）</summary>
    public string? D7Preventive { get; set; }

    /// <summary>D8 小组祝贺与知识共享</summary>
    public string? D8Thanks { get; set; }

    /// <summary>当前步骤 D0~D8</summary>
    public int CurrentDiscipline { get; set; } = 0;

    /// <summary>状态：in_progress / completed / closed</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "in_progress";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(ComplaintId))]
    public virtual Complaint? Complaint { get; set; } = null!;
}

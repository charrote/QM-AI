using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// CAPA 单（纠正与预防措施）
/// </summary>
[Table("capa")]
public class Capa
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>CAPA 单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    public string CapaCode { get; set; } = string.Empty;

    /// <summary>关联缺陷记录</summary>
    public long? DefectId { get; set; }

    /// <summary>关联异常单（IQC/IPQC）</summary>
    public long? AnomalyId { get; set; }

    /// <summary>关联客诉</summary>
    public long? ComplaintId { get; set; }

    /// <summary>严重等级：critical / major / minor</summary>
    [Required]
    [MaxLength(10)]
    public string Severity { get; set; } = "major";

    /// <summary>标题</summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>详细描述</summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 当前阶段（数字 0-5）：
    /// 0=创建 / 1=临时措施 / 2=根本原因分析 / 3=纠正措施 / 4=预防措施 / 5=验证 / 6=关闭
    /// </summary>
    [Required]
    public int CurrentPhase { get; set; } = 0;

    /// <summary>状态：open / in_progress / completed / closed / cancelled</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "open";

    public long CreatedBy { get; set; }
    public long? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(DefectId))]
    public Defect? Defect { get; set; }

    public ICollection<CapaTemporaryMeasure> TemporaryMeasures { get; set; } = new List<CapaTemporaryMeasure>();
    public ICollection<CapaRootCause> RootCauses { get; set; } = new List<CapaRootCause>();
    public ICollection<CapaCorrectiveAction> CorrectiveActions { get; set; } = new List<CapaCorrectiveAction>();
    public ICollection<CapaPreventiveAction> PreventiveActions { get; set; } = new List<CapaPreventiveAction>();
    public ICollection<CapaVerification> Verifications { get; set; } = new List<CapaVerification>();
}
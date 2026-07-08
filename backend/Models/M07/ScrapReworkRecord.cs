using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// 报废/返工记录
/// </summary>
[Table("scrap_rework_records")]
public class ScrapReworkRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>类型：scrap / rework</summary>
    [Required]
    [MaxLength(10)]
    public string Type { get; set; } = string.Empty;

    /// <summary>关联缺陷记录</summary>
    public long? DefectId { get; set; }

    /// <summary>关联批次</summary>
    public long? BatchId { get; set; }

    /// <summary>数量</summary>
    [Required]
    [Column(TypeName = "decimal(15,2)")]
    public decimal Quantity { get; set; }

    /// <summary>原因</summary>
    [Required]
    public string Reason { get; set; } = string.Empty;

    /// <summary>返工步骤（JSON，仅返工有）</summary>
    public string? ReworkSteps { get; set; }

    /// <summary>是否需要返工后检验</summary>
    public bool ReworkInspectionRequired { get; set; }

    /// <summary>返工后检验结果：pass / fail / pending</summary>
    [MaxLength(10)]
    public string? ReworkInspectionResult { get; set; }

    /// <summary>授权人</summary>
    public long AuthorizedBy { get; set; }

    /// <summary>授权时间</summary>
    [Required]
    public DateTime AuthorizedAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DefectId))]
    public Defect? Defect { get; set; }
}
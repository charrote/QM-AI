using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M03;

/// <summary>
/// 供应商评分
/// </summary>
[Table("supplier_scores")]
public class SupplierScore
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联供应商</summary>
    public int SupplierId { get; set; }

    /// <summary>评分日期</summary>
    public DateTime? ScoreDate { get; set; }

    /// <summary>综合评分（0-100）</summary>
    public decimal? Score { get; set; }

    /// <summary>维度评分 JSON</summary>
    [Column(TypeName = "json")]
    public string? DimensionScores { get; set; }

    /// <summary>评级：A/B/C/D</summary>
    [MaxLength(10)]
    public string? Grade { get; set; }

    /// <summary>评估意见</summary>
    public string? Evaluation { get; set; }

    // Navigation
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }
}

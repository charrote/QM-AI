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
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联供应商</summary>
    [Column("supplier_id")]
    public long SupplierId { get; set; }

    /// <summary>评分日期</summary>
    [Column("assessment_date")]
    public DateTime? ScoreDate { get; set; }

    /// <summary>综合评分（0-100）</summary>
    [Column("score")]
    public decimal? Score { get; set; }

    /// <summary>评级：A/B/C/D</summary>
    [Column("grade")]
    public string? Grade { get; set; }

    /// <summary>评估意见</summary>
    [Column("evaluation")]
    public string? Evaluation { get; set; }

    /// <summary>维度评分 JSON</summary>
    [Column("dimension_scores")]
    public string? DimensionScores { get; set; }

    // Navigation
    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }
}

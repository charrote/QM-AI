using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M05;

/// <summary>
/// FQC 成品检验单
/// </summary>
[Table("fqc_inspections")]
public class FqcInspection
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>检验单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("inspection_no")]
    public string InspectionNo { get; set; } = string.Empty;

    /// <summary>关联批次</summary>
    [Column("batch_id")]
    public long BatchId { get; set; }

    /// <summary>关联工单</summary>
    [Column("work_order_id")]
    public long? WorkOrderId { get; set; }

    /// <summary>检验方式：full / sampling</summary>
    [Required]
    [MaxLength(10)]
    public string InspectionType { get; set; } = "full";

    /// <summary>AQL 值（抽检时）</summary>
    public decimal? AqlLevel { get; set; }

    /// <summary>样本量</summary>
    [Column("sample_size")]
    public int SampleSize { get; set; }

    /// <summary>已检数量</summary>
    [Column("total_checked")]
    public int TotalChecked { get; set; }

    /// <summary>合格数量</summary>
    [Column("total_pass")]
    public int TotalPass { get; set; }

    /// <summary>不合格数量</summary>
    [Column("total_fail")]
    public int TotalFail { get; set; }

    /// <summary>合格判定数 Ac</summary>
    [Column("ac")]
    public int Ac { get; set; }

    /// <summary>不合格判定数 Re</summary>
    [Column("re")]
    public int Re { get; set; }

    /// <summary>结论：qualified / unqualified / pending</summary>
    [Required]
    [MaxLength(20)]
    [Column("conclusion")]
    public string Conclusion { get; set; } = "pending";

    /// <summary>检验员 ID</summary>
    public long? InspectorId { get; set; }

    /// <summary>检验时间</summary>
    [Column("checked_at")]
    public DateTime? CheckedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(BatchId))]
    public ProductBatch? Batch { get; set; }

    public ICollection<FqcInspectionItem>? Items { get; set; }
}

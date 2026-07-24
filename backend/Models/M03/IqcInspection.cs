using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M03;

/// <summary>
/// IQC 检验单
/// </summary>
[Table("iqc_inspections")]
public class IqcInspection
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

    /// <summary>关联来料登记</summary>
    public long ReceiptId { get; set; }

    /// <summary>关联检验标准</summary>
    public long? StandardId { get; set; }

    /// <summary>抽样样本量</summary>
    [Column("sample_size")]
    public int SampleSize { get; set; }

    /// <summary>合格判定数 Ac</summary>
    [Column("ac")]
    public int Ac { get; set; }

    /// <summary>不合格判定数 Re</summary>
    [Column("re")]
    public int Re { get; set; }

    /// <summary>不合格数量</summary>
    [Column("defect_qty")]
    public int DefectQty { get; set; }

    /// <summary>AQL 检验水平：I/II/III</summary>
    [MaxLength(10)]
    [Column("sampling_level")]
    public string? SamplingLevel { get; set; } = "II";

    /// <summary>AQL 值</summary>
    [Column("aql_value")]
    public double? AqlValue { get; set; }

    /// <summary>结果：pending/pass/fail/scrap</summary>
    [Required]
    [MaxLength(10)]
    [Column("result")]
    public string Result { get; set; } = "pending";

    /// <summary>检验员</summary>
    [MaxLength(100)]
    [Column("inspector")]
    public string? Inspector { get; set; }

    /// <summary>检验时间</summary>
    [Column("inspected_at")]
    public DateTime? InspectedAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ReceiptId))]
    public IqcReceipt? Receipt { get; set; }

    [ForeignKey(nameof(StandardId))]
    public InspectionStandard? Standard { get; set; }

    public ICollection<IqcInspectionItem>? Items { get; set; }
}

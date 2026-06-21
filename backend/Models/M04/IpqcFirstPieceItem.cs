using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Models.M04;

/// <summary>
/// IPQC 首件检验结果明细
/// </summary>
[Table("ipqc_first_piece_items")]
public class IpqcFirstPieceItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联首件检验</summary>
    public long FirstPieceId { get; set; }

    /// <summary>关联检验项目主数据</summary>
    public long? InspectionItemId { get; set; }

    /// <summary>检验项目名称</summary>
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>检验项目编码</summary>
    [MaxLength(50)]
    public string? ItemCode { get; set; }

    /// <summary>规格上限</summary>
    public decimal? Usl { get; set; }

    /// <summary>规格下限</summary>
    public decimal? Lsl { get; set; }

    /// <summary>数据类型</summary>
    [Required]
    [MaxLength(20)]
    public string DataType { get; set; } = "numeric";

    /// <summary>实测值</summary>
    public decimal? ActualValue { get; set; }

    /// <summary>结果：pass/fail/pending</summary>
    [Required]
    [MaxLength(10)]
    public string Result { get; set; } = "pending";

    /// <summary>图片 URLs (JSON array)</summary>
    public string? ImageUrls { get; set; }

    /// <summary>备注</summary>
    public string? Remarks { get; set; }

    // Navigation
    [ForeignKey(nameof(FirstPieceId))]
    public IpqcFirstPiece? FirstPiece { get; set; }

    [ForeignKey(nameof(InspectionItemId))]
    public InspectionItem? InspectionItem { get; set; }
}

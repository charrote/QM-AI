using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Models.M03;

/// <summary>
/// IQC 检验明细项
/// </summary>
[Table("iqc_inspection_items")]
public class IqcInspectionItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联检验单</summary>
    public long InspectionId { get; set; }

    /// <summary>关联动态参数</summary>
    public long? ParamId { get; set; }

    /// <summary>关联检验项目主数据（贯通核心）</summary>
    public long? InspectionItemId { get; set; }

    /// <summary>检验项目名称（冗余）</summary>
    [MaxLength(200)]
    public string? ItemName { get; set; }

    /// <summary>实测值</summary>
    public decimal? MeasuredValue { get; set; }

    /// <summary>规格上限</summary>
    public decimal? Usl { get; set; }

    /// <summary>规格下限</summary>
    public decimal? Lsl { get; set; }

    /// <summary>结果：pass/fail</summary>
    [Required]
    [MaxLength(10)]
    public string Result { get; set; } = "pending";

    /// <summary>关联不良代码</summary>
    public long? DefectCodeId { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    // Navigation
    [ForeignKey(nameof(InspectionId))]
    public IqcInspection? Inspection { get; set; }

    [ForeignKey(nameof(DefectCodeId))]
    public DefectCode? DefectCode { get; set; }

    [ForeignKey(nameof(InspectionItemId))]
    public InspectionItem? InspectionItem { get; set; }
}

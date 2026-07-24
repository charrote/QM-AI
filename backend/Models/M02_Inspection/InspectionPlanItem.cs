using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_Inspection;

/// <summary>
/// 检验计划明细 —— 某计划下具体需要检验的项目
/// 支持覆盖主数据的规格限/管理限（按实际情况调整）
/// </summary>
[Table("inspection_plan_items")]
public class InspectionPlanItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联检验计划</summary>
    [Column("plan_id")]
    public long PlanId { get; set; }

    /// <summary>关联检验项目主数据</summary>
    [Column("inspection_item_id")]
    public long InspectionItemId { get; set; }

    /// <summary>排序号</summary>
    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    // ═══ 可覆盖主数据的规格（按计划单独调整）═══
    /// <summary>规格上限（覆盖主数据）</summary>
    [Column("usl")]
    public decimal? Usl { get; set; }

    /// <summary>规格下限（覆盖主数据）</summary>
    [Column("lsl")]
    public decimal? Lsl { get; set; }

    /// <summary>目标值（覆盖主数据）</summary>
    [Column("target_value", TypeName = "decimal(15,6)")]
    public decimal? TargetValue { get; set; }

    /// <summary>管理上限（覆盖主数据）</summary>
    [Column("ucl", TypeName = "decimal(15,6)")]
    public decimal? Ucl { get; set; }

    /// <summary>管理下限（覆盖主数据）</summary>
    [Column("lcl", TypeName = "decimal(15,6)")]
    public decimal? Lcl { get; set; }

    /// <summary>抽样数量（覆盖主数据）</summary>
    [Column("sample_size")]
    public int? SampleSize { get; set; }

    /// <summary>是否必须（必检/选检）</summary>
    [Column("is_required")]
    public bool IsRequired { get; set; } = true;

    // Navigation
    [ForeignKey(nameof(PlanId))]
    public InspectionPlan? Plan { get; set; }

    [ForeignKey(nameof(InspectionItemId))]
    public InspectionItem? InspectionItem { get; set; }
}

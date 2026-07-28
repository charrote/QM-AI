using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M03;

/// <summary>
/// IQC 来料异常单（增强版 - 对标标杆设计）
/// 支持完整流程：发现问题 → 隔离 → 异常报告 → MRB评审 → 处置决定 → 纠正措施 → 关闭
/// </summary>
[Table("iqc_anomalies")]
public class IqcAnomaly
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>异常单号（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("anomaly_no")]
    public string AnomalyNo { get; set; } = string.Empty;

    /// <summary>关联来料登记</summary>
    [Column("receipt_id")]
    public long ReceiptId { get; set; }

    /// <summary>关联检验单</summary>
    [Column("inspection_id")]
    public long? InspectionId { get; set; }

    /// <summary>关联检验项目明细（不合格的具体项目ID列表）</summary>
    [Column("failed_item_ids")]
    public string? FailedItemIds { get; set; }

    /// <summary>不合格品数量</summary>
    [Column("defect_qty")]
    public int DefectQty { get; set; }

    /// <summary>异常类型</summary>
    [Required]
    [MaxLength(20)]
    [Column("anomaly_type")]
    public string AnomalyType { get; set; } = "quality";

    /// <summary>严重程度：critical/major/minor</summary>
    [Required]
    [MaxLength(10)]
    [Column("severity")]
    public string Severity { get; set; } = "major";

    /// <summary>异常描述</summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>隔离库存量（不合格品隔离数量）</summary>
    [Column("isolated_inventory")]
    public decimal IsolatedInventory { get; set; } = 0;

    /// <summary>处置方式：return=退货, concession=让步接收, rework=返工挑选, special_purchase=特采, none=待处置</summary>
    [MaxLength(20)]
    [Column("disposition")]
    public string Disposition { get; set; } = "none";

    /// <summary>处置决定人</summary>
    [MaxLength(100)]
    [Column("disposition_by")]
    public string? DispositionBy { get; set; }

    /// <summary>处置决定时间</summary>
    [Column("disposition_date")]
    public DateTime? DispositionDate { get; set; }

    /// <summary>处理人部门（quality/purchasing/engineering/production/other）</summary>
    [MaxLength(20)]
    [Column("handler_dept")]
    public string? HandlerDept { get; set; }

    /// <summary>状态：open=待处理, quarantined=待隔离, investigating=调查分析中, mrb_reviewing=MRB评审中, mrb_approved=MRB通过, disposed=已处置, processing=处理中, resolved=已解决, closed=已关闭</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "open";

    /// <summary>处理人</summary>
    [MaxLength(100)]
    [Column("handler")]
    public string? Handler { get; set; }

    /// <summary>MRB评审是否完成</summary>
    [Column("mrb_reviewed")]
    public bool MrbReviewed { get; set; }

    /// <summary>MRB评审人（多部门会签）</summary>
    [MaxLength(500)]
    [Column("mrb_reviewer")]
    public string? MrbReviewer { get; set; }

    /// <summary>MRB评审完成时间</summary>
    [Column("mrb_reviewed_at")]
    public DateTime? MrbReviewedAt { get; set; }

    /// <summary>关联CAPA单ID</summary>
    [Column("capa_id")]
    public long? CapaId { get; set; }

    /// <summary>首次响应时间</summary>
    [Column("first_response_at")]
    public DateTime? FirstResponseAt { get; set; }

    /// <summary>是否已通知供应商</summary>
    [Column("supplier_notified")]
    public bool SupplierNotified { get; set; }

    /// <summary>供应商回复时间</summary>
    [Column("supplier_response_at")]
    public DateTime? SupplierResponseAt { get; set; }

    /// <summary>解决时间（兼容旧字段）</summary>
    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    /// <summary>创建人ID（操作审计）</summary>
    [Column("created_by")]
    public long? CreatedBy { get; set; }

    /// <summary>最后更新人ID</summary>
    [Column("updated_by")]
    public long? UpdatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ReceiptId))]
    public IqcReceipt? Receipt { get; set; }

    [ForeignKey(nameof(InspectionId))]
    public IqcInspection? Inspection { get; set; }
}

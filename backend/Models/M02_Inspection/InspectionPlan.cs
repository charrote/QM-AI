using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M02_Inspection;

/// <summary>
/// 检验计划 —— 桥接检验项目主数据与品质业务（S3/S4/S5）的关键实体
/// 
/// 定义了"在什么业务上下文下需要检验哪些项目"
/// 例如：IQC来料 → 料号X + 供应商Y → 需要检验项目A/B/C
///       IPQC首件 → 产品X + 工序Y → 需要检验项目B/D/E
///       FQC成品 → 产品X + 客户Z → 需要检验项目A/C/F
/// </summary>
[Table("inspection_plans")]
public class InspectionPlan
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>计划编码（唯一）</summary>
    [Required]
    [MaxLength(50)]
    [Column("plan_code")]
    public string PlanCode { get; set; } = string.Empty;

    /// <summary>计划名称</summary>
    [Required]
    [MaxLength(200)]
    public string PlanName { get; set; } = string.Empty;

    /// <summary>检验类型：IQC / IPQC / FQC / OQC</summary>
    [Required]
    [MaxLength(10)]
    public string InspectionType { get; set; } = string.Empty;

    /// <summary>描述</summary>
    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    // ═══ 业务维度（用于匹配业务上下文）═══
    /// <summary>产品维度（可为null表示通用计划）</summary>
    [Column("product_id")]
    public long? ProductId { get; set; }

    /// <summary>材料维度（关联products表，product_type='material'）</summary>
    [Column("material_id")]
    public long? MaterialId { get; set; }

    /// <summary>供应商维度</summary>
    public long? SupplierId { get; set; }

    /// <summary>客户维度</summary>
    public long? CustomerId { get; set; }

    /// <summary>工艺/工序维度</summary>
    public long? ProcessId { get; set; }

    /// <summary>设备维度</summary>
    [Column("equipment_id")]
    public long? EquipmentId { get; set; }

    /// <summary>是否启用</summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public long CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [ForeignKey(nameof(MaterialId))]
    public Product? Material { get; set; }

    [ForeignKey(nameof(SupplierId))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(ProcessId))]
    public Process? Process { get; set; }

    [ForeignKey(nameof(EquipmentId))]
    public Equipment? Equipment { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }

    /// <summary>计划明细项</summary>
    public ICollection<InspectionPlanItem> Items { get; set; } = new List<InspectionPlanItem>();
}

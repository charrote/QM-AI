using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("inspection_standards")]
public class InspectionStandard
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("standard_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("standard_name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>检验类型：IQC/IPQC/FQC/OQC</summary>
    [Required]
    [MaxLength(20)]
    [Column("inspection_type")]
    public string InspectionType { get; set; } = string.Empty;

    /// <summary>关联产品（可为空，表示通用标准）</summary>
    [Column("product_id")]
    public long? ProductId { get; set; }

    /// <summary>关联工序</summary>
    [Column("process_id")]
    public long? ProcessId { get; set; }

    /// <summary>检验项目名称</summary>
    [Required]
    [MaxLength(200)]
    [Column("item_name")]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>规格上限 USL</summary>
    [Column("usl")]
    public double? Usl { get; set; }

    /// <summary>规格下限 LSL</summary>
    [Column("lsl")]
    public double? Lsl { get; set; }

    /// <summary>目标值</summary>
    [Column("target")]
    public double? Target { get; set; }

    /// <summary>单位</summary>
    [MaxLength(50)]
    [Column("unit")]
    public string? Unit { get; set; }

    /// <summary>检验工具/方法</summary>
    [MaxLength(200)]
    [Column("inspection_method")]
    public string? InspectionMethod { get; set; }

    /// <summary>抽样频率</summary>
    [MaxLength(100)]
    [Column("sampling_frequency")]
    public string? SamplingFrequency { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [ForeignKey(nameof(ProcessId))]
    public Process? Process { get; set; }
}

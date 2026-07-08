using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("InspectionStandards")]
public class InspectionStandard
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>检验类型：IQC/IPQC/FQC/OQC</summary>
    [Required]
    [MaxLength(20)]
    public string InspectionType { get; set; } = string.Empty;

    /// <summary>关联产品（可为空，表示通用标准）</summary>
    public long? ProductId { get; set; }

    /// <summary>关联工序</summary>
    public long? ProcessId { get; set; }

    /// <summary>检验项目名称</summary>
    [Required]
    [MaxLength(200)]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>规格上限 USL</summary>
    public double? Usl { get; set; }

    /// <summary>规格下限 LSL</summary>
    public double? Lsl { get; set; }

    /// <summary>目标值</summary>
    public double? Target { get; set; }

    /// <summary>单位</summary>
    [MaxLength(50)]
    public string? Unit { get; set; }

    /// <summary>检验工具/方法</summary>
    [MaxLength(200)]
    public string? InspectionMethod { get; set; }

    /// <summary>抽样频率</summary>
    [MaxLength(100)]
    public string? SamplingFrequency { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    [ForeignKey(nameof(ProcessId))]
    public Process? Process { get; set; }
}

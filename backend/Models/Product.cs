using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("Products")]
public class Product
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

    [MaxLength(50)]
    public string? Unit { get; set; }

    /// <summary>产品类别</summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>默认检验水平（GB/T 2828.1）</summary>
    [MaxLength(10)]
    public string? DefaultInspectionLevel { get; set; } = "II";

    /// <summary>默认AQL值</summary>
    public double? DefaultAql { get; set; }

    /// <summary>所属组织ID</summary>
    public long? OrgId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

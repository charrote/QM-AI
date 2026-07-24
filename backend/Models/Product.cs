using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("products")]
public class Product
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("product_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("product_name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [MaxLength(50)]
    [Column("unit")]
    public string? Unit { get; set; }

/// <summary>产品类别</summary>
    [MaxLength(100)]
    [Column("product_type")]
    public string? Category { get; set; }

    /// <summary>默认检验水平（GB/T 2828.1）</summary>
    [MaxLength(10)]
    [Column("default_inspection_level")]
    public string? DefaultInspectionLevel { get; set; } = "II";

    /// <summary>默认 AQL 值</summary>
    [Column("default_aql")]
    public double? DefaultAql { get; set; }

    /// <summary>规格</summary>
    [Column("specification")]
    public string? Specification { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

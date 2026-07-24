using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("boms")]
public class Bom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    /// <summary>物料编码</summary>
    [Required]
    [MaxLength(100)]
    [Column("material_code")]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>物料名称</summary>
    [Required]
    [MaxLength(200)]
    [Column("material_name")]
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>用量</summary>
    [Column("quantity")]
    public double Quantity { get; set; } = 1;

    [MaxLength(50)]
    [Column("unit")]
    public string? Unit { get; set; }

    /// <summary>层级（0=成品，1=一级子件...）</summary>
    [Column("level")]
    public int Level { get; set; } = 0;

    [MaxLength(500)]
    [Column("remark")]
    public string? Remark { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}

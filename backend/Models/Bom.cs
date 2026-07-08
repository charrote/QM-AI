using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("Boms")]
public class Bom
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public long ProductId { get; set; }

    /// <summary>物料编码</summary>
    [Required]
    [MaxLength(100)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>物料名称</summary>
    [Required]
    [MaxLength(200)]
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>用量</summary>
    public double Quantity { get; set; } = 1;

    [MaxLength(50)]
    public string? Unit { get; set; }

    /// <summary>层级（0=成品，1=一级子件...）</summary>
    public int Level { get; set; } = 0;

    [MaxLength(500)]
    public string? Remark { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
}

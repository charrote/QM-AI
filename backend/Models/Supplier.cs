using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("Suppliers")]
public class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(100)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    /// <summary>供应商等级：A/B/C/D</summary>
    [MaxLength(10)]
    public string? Grade { get; set; } = "B";

    /// <summary>供应产品类别</summary>
    [MaxLength(500)]
    public string? SupplyCategory { get; set; }

    /// <summary>综合评分（0-100）</summary>
    public double? Score { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

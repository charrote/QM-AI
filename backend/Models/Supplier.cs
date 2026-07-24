using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("suppliers")]
public class Supplier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("supplier_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("supplier_name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Column("contact_person")]
    public string? ContactPerson { get; set; }

    [MaxLength(100)]
    [Column("phone")]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    [Column("email")]
    public string? Email { get; set; }

    /// <summary>供应商等级：A/B/C/D</summary>
    [MaxLength(10)]
    [Column("rating")]
    public string? Grade { get; set; } = "B";

    /// <summary>供应产品类别 - 仅内存中保存，不持久化</summary>
    [NotMapped]
    public string? SupplyCategory { get; set; }

    /// <summary>是否启用 - 仅内存中保存，不持久化</summary>
    [NotMapped]
    public bool IsActive { get; set; } = true;

    /// <summary>综合评分（0-100）- 仅内存中保存，不持久化</summary>
    [NotMapped]
    public double? Score { get; set; }

    /// <summary>更新时间 - 仅内存中保存，不持久化</summary>
    [NotMapped]
    public DateTime? UpdatedAt { get; set; }

    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

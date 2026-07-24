using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("customers")]
public class Customer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("customer_code")]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("customer_name")]
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

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

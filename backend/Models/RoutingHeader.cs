using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("routing_headers")]
public class RoutingHeader
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("route_code")]
    public string RouteCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("route_name")]
    public string RouteName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("route_type")]
    public string RouteType { get; set; } = "STD";

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; } = false;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }
    public ICollection<RoutingStep> Steps { get; set; } = new List<RoutingStep>();
}
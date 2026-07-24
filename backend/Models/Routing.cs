using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("routings")]
public class Routing
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    /// <summary>工艺路线编号</summary>
    [Required]
    [MaxLength(100)]
    [Column("routing_code")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>工艺路线名称</summary>
    [MaxLength(200)]
    [Column("routing_name")]
    public string? RoutingName { get; set; }

    /// <summary>工序顺序（步骤号）</summary>
    [Column("step_order")]
    public int StepOrder { get; set; }

    [Column("process_id")]
    public long ProcessId { get; set; }

    /// <summary>标准工时（分钟）</summary>
    [Column("standard_time_minutes")]
    public double? StandardTimeMinutes { get; set; }

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

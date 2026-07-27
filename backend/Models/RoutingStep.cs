using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("routing_steps")]
public class RoutingStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("routing_header_id")]
    public long RoutingHeaderId { get; set; }

    [Column("step_order")]
    public int StepOrder { get; set; }

    [Column("process_id")]
    public long ProcessId { get; set; }

    [Column("standard_time_minutes")]
    public double? StandardTimeMinutes { get; set; }

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>前置等待时间（分钟）</summary>
    [Column("pre_wait_time_minutes")]
    public double? PreWaitTimeMinutes { get; set; }

    /// <summary>后置等待时间（分钟）</summary>
    [Column("post_wait_time_minutes")]
    public double? PostWaitTimeMinutes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(RoutingHeaderId))]
    public RoutingHeader? RoutingHeader { get; set; }

    [ForeignKey(nameof(ProcessId))]
    public Process? Process { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models;

[Table("workflow_instances")]
public class WorkflowInstance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    [Column("definition_key")]
    public string? DefinitionKey { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    /// <summary>发起人 ID</summary>
    [Column("initiator_id")]
    public long InitiatorId { get; set; }

    [Column("initiator_name")]
    [MaxLength(100)]
    public string? InitiatorName { get; set; }

    /// <summary>当前处理人 ID</summary>
    [Column("current_assignee_id")]
    public long? CurrentAssigneeId { get; set; }

    [Column("nodes_json")]
    public string? NodesJson { get; set; }

    [Column("tasks_json")]
    public string? TasksJson { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}

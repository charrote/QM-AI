using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// CAPA 纠正措施
/// </summary>
[Table("capa_corrective_actions")]
public class CapaCorrectiveAction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("capa_id")]
    public long CapaId { get; set; }

    /// <summary>措施描述</summary>
    [Required]
    [Column("action_description")]
    public string ActionDescription { get; set; } = string.Empty;

    /// <summary>负责人</summary>
    [Column("responsible_person")]
    public long ResponsiblePerson { get; set; }

    /// <summary>完成期限</summary>
    [Required]
    [Column("due_date")]
    public DateTime DueDate { get; set; }

    /// <summary>状态：pending / in_progress / completed</summary>
    [Required]
    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
    public string? Remarks { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
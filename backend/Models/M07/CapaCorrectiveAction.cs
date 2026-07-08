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
    public long Id { get; set; }

    public long CapaId { get; set; }

    /// <summary>措施描述</summary>
    [Required]
    public string ActionDescription { get; set; } = string.Empty;

    /// <summary>负责人</summary>
    public long ResponsiblePerson { get; set; }

    /// <summary>完成期限</summary>
    [Required]
    public DateTime DueDate { get; set; }

    /// <summary>状态：pending / in_progress / completed</summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public DateTime? CompletedAt { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
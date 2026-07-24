using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M12;

/// <summary>
/// 文档版本历史
/// </summary>
[Table("document_versions")]
public class DocumentVersion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>关联文档</summary>
    [Required]
    public long DocumentId { get; set; }

    /// <summary>版本号</summary>
    [Required]
    [Column("version")]
    public int Version { get; set; }

    /// <summary>MinIO 对象键</summary>
    [Required]
    [MaxLength(500)]
    [Column("minio_key")]
    public string MinioKey { get; set; } = string.Empty;

    /// <summary>变更说明</summary>
    public string? ChangeDescription { get; set; }

    /// <summary>创建人</summary>
    [Column("created_by")]
    public long CreatedBy { get; set; }

    /// <summary>创建时间</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(DocumentId))]
    public virtual Document? Document { get; set; } = null!;
}

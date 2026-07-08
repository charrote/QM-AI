using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M12;

/// <summary>
/// 文件/文档
/// </summary>
[Table("documents")]
public class Document
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>文件标题</summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    /// <summary>文档类型：sop / work_instruction / inspection_standard / 8d_report / audit_report / other</summary>
    [Required]
    [MaxLength(20)]
    public string DocType { get; set; } = string.Empty;

    /// <summary>MinIO 对象键</summary>
    [Required]
    [MaxLength(500)]
    public string MinioKey { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>SHA-256 哈希</summary>
    [MaxLength(64)]
    public string? FileHash { get; set; }

    /// <summary>版本号</summary>
    public int Version { get; set; } = 1;

    /// <summary>状态：draft / reviewing / approved / archived</summary>
    [Required]
    [MaxLength(10)]
    public string Status { get; set; } = "draft";

    /// <summary>审批人ID</summary>
    public long? ApprovedBy { get; set; }

    /// <summary>审批人标识（字符串，供服务层兼容）</summary>
    public string? ApprovedByStr { get; set; }

    /// <summary>驳回理由</summary>
    public string? RejectionReason { get; set; }

    /// <summary>审批时间</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>有效期</summary>
    public DateOnly? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>创建人</summary>
    public long CreatedBy { get; set; }

    // Navigation
    public virtual ICollection<DocumentVersion>? Versions { get; set; } = new List<DocumentVersion>();
}

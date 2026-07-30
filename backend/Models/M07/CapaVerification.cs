using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QM_AI.API.Models.M07;

/// <summary>
/// CAPA 验证记录
/// </summary>
[Table("capa_verifications")]
public class CapaVerification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("capa_id")]
    public long CapaId { get; set; }

    /// <summary>验证人</summary>
    [Column("verifier_id")]
    public long VerifierId { get; set; }

    /// <summary>验证日期</summary>
    [Required]
    [Column("verification_date")]
    public DateTime VerificationDate { get; set; }

    /// <summary>结论：effective / not_effective / requires_revision</summary>
    [Required]
    [MaxLength(20)]
    [Column("conclusion")]
    public string Conclusion { get; set; } = string.Empty;

    /// <summary>验证证据</summary>
    [Column("evidence")]
    public string? Evidence { get; set; }

    /// <summary>图片URLs</summary>
    [Column("image_urls")]
    public string? ImageUrls { get; set; }

    /// <summary>备注</summary>
    public string? Remarks { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
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
    public long Id { get; set; }

    public long CapaId { get; set; }

    /// <summary>验证人</summary>
    public long VerifierId { get; set; }

    /// <summary>验证日期</summary>
    [Required]
    public DateTime VerificationDate { get; set; }

    /// <summary>结论：effective / not_effective / requires_revision</summary>
    [Required]
    [MaxLength(20)]
    public string Conclusion { get; set; } = string.Empty;

    /// <summary>验证证据</summary>
    public string? Evidence { get; set; }

    /// <summary>图片URLs</summary>
    public string? ImageUrls { get; set; }

    /// <summary>备注</summary>
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CapaId))]
    public Capa? Capa { get; set; }
}
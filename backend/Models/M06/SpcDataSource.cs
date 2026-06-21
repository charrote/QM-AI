using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Models.M06;

/// <summary>
/// SPC 数据源配置 —— 贯通S3/S4/S5业务数据到SPC的关键桥梁
/// 定义控制图从哪个业务模块、哪个检验项目拉取数据
/// </summary>
[Table("spc_data_sources")]
public class SpcDataSource
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>关联控制图</summary>
    public long ChartId { get; set; }

    /// <summary>数据源类型：IQC / IPQC / FQC</summary>
    [Required]
    [MaxLength(10)]
    public string SourceType { get; set; } = string.Empty;

    /// <summary>关联检验项目（null表示该数据源类型所有项目）</summary>
    public long? InspectionItemId { get; set; }

    /// <summary>过滤：产品</summary>
    public int? ProductId { get; set; }

    /// <summary>过滤：工序</summary>
    public int? ProcessId { get; set; }

    /// <summary>过滤：供应商</summary>
    public int? SupplierId { get; set; }

    /// <summary>过滤：客户</summary>
    public int? CustomerId { get; set; }

    /// <summary>过滤：设备</summary>
    public int? EquipmentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public SpcControlChart? Chart { get; set; }

    [ForeignKey(nameof(InspectionItemId))]
    public InspectionItem? InspectionItem { get; set; }
}

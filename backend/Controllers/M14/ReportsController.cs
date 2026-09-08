using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M14;

[ApiController]
[Route("api/v1/m14/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportsService _reports;

    public ReportsController(ReportsService reports)
    {
        _reports = reports;
    }

    // ─── 质量仪表盘 ───────────────────────────────────────

    [HttpGet("quality")]
    public async Task<IActionResult> GetQualityStats(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] string? module)
    {
        // 真实查库聚合（IQC/FQC 检验项合格率、缺陷总数、报废/返工处置比例）
        // 注：module 参数当前未用于过滤（前端未使用），保留以兼容既有契约
        return Ok(await _reports.GetQualityStatsAsync(startDate, endDate));
    }

    [HttpGet("defect-pareto")]
    public async Task<IActionResult> GetDefectPareto([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        // 按缺陷代码聚合 defects 表，LEFT JOIN defect_codes 取名称，count 降序
        return Ok(await _reports.GetDefectParetoAsync(startDate, endDate));
    }

    [HttpGet("quality-trend")]
    public async Task<IActionResult> GetQualityTrend([FromQuery] int days = 7)
    {
        // 近 N 天按日聚合 FPY（稀疏数据如实留 null，不填充）
        return Ok(await _reports.GetQualityTrendAsync(days));
    }

    [HttpGet("supplier-scores")]
    public IActionResult GetSupplierScores([FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        // TODO: Implement supplier score calculation
        return Ok(new object[0]);
    }

    // ─── 报表定制 ─────────────────────────────────────────

    [HttpPost("generate")]
    public IActionResult GenerateReport([FromBody] GenerateReportRequest req)
    {
        // TODO: Implement report generation service
        return Ok(new { jobId = Guid.NewGuid().ToString() });
    }

    public sealed class GenerateReportRequest
    {
        public string ReportType { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public string Module { get; set; } = "";
        public string Format { get; set; } = "csv";
    }

    // ─── 导出中心 ─────────────────────────────────────────

    [HttpGet("exports")]
    public IActionResult GetExports([FromQuery] string? status)
    {
        // TODO: Implement export list query
        return Ok(new object[0]);
    }

    [HttpGet("exports/{id}/download")]
    public IActionResult DownloadExport(long id)
    {
        // TODO: Implement export file download
        return Ok();
    }

    [HttpDelete("exports/{id}")]
    public IActionResult DeleteExport(long id)
    {
        // TODO: Implement export deletion
        return Ok();
    }
}

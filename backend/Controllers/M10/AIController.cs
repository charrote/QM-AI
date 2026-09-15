using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M10;

[ApiController]
[Route("api/v1/m10/ai")]
public class AIController : ControllerBase
{
    private readonly AiAlertService _alerts;

    public AIController(AiAlertService alerts)
    {
        _alerts = alerts;
    }

    // ─── 预警中心 ─────────────────────────────────────────

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] string? level,
        [FromQuery] bool? resolved,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (items, total) = await _alerts.GetAlertsAsync(level, resolved, page, pageSize);
        return Ok(new { items, total });
    }

    [HttpPut("alerts/{id}/resolve")]
    public async Task<IActionResult> ResolveAlert(long id)
    {
        // 登录用户 ID（从 JWT 取，无登录时按 0 处理）
        var userId = long.TryParse(User.FindFirst("uid")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "", out var u) ? u : 0;
        var ok = await _alerts.ResolveAlertAsync(id, userId);
        if (!ok) return NotFound(new { message = "预警不存在或已解决" });
        return Ok(new { id, resolved = true });
    }

    [HttpGet("alerts/stats")]
    public async Task<IActionResult> GetAlertStats()
    {
        return Ok(await _alerts.GetAlertStatsAsync());
    }

    // ─── 根因分析 ─────────────────────────────────────────

    [HttpPost("root-cause")]
    public IActionResult RunRootCauseAnalysis([FromBody] RootCauseRequest req)
    {
        // TODO: Implement root cause analysis
        return Ok(new {
            findings = new object[0],
            summary = "Analysis completed"
        });
    }

    public sealed class RootCauseRequest
    {
        public long? ProductId { get; set; }
        public string? DefectCode { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public long? ProcessId { get; set; }
    }

    // ─── 模型管理 ─────────────────────────────────────────

    [HttpGet("models")]
    public IActionResult GetModels(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Implement model service
        return Ok(new { items = new object[0], total = 0 });
    }

    [HttpPost("models/train")]
    public IActionResult TrainModel([FromBody] TrainModelRequest req)
    {
        // TODO: Implement model training
        return Ok(new { modelId = 1L, status = "training" });
    }

    [HttpGet("models/{id}")]
    public IActionResult GetModel(long id)
    {
        // TODO: Implement get model logic
        return Ok(new { id, name = "Example Model", type = "classification", status = "trained" });
    }

    [HttpDelete("models/{id}")]
    public IActionResult DeleteModel(long id)
    {
        // TODO: Implement delete model logic
        return Ok();
    }

    public sealed class TrainModelRequest
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public long? DatasetId { get; set; }
    }
}

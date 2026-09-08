using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M14;

[ApiController]
[Route("api/v1/m14/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    /// <summary>
    /// 今日待处理：未完成 IQC/FQC 检验、未关闭缺陷、未完成 CAPA、未关闭客诉、待放行首件。
    /// 按 priority 降序 + time 降序，最多 15 条；total 为过滤前全量计数。
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var (items, total) = await _service.GetPendingAsync();
        return Ok(new { items, total });
    }
}

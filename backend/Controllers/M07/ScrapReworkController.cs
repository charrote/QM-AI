using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Models.M07;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers;

/// <summary>
/// M07 报废/返工记录 API
/// </summary>
[ApiController]
[Route("api/v1/defects/scrap-rework")]
public class ScrapReworkController : ControllerBase
{
    private readonly CapaService _capaService;

    public ScrapReworkController(CapaService capaService)
    {
        _capaService = capaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ScrapReworkRecord>>> GetAll([FromQuery] long? defectId)
    {
        try
        {
            var records = await _capaService.GetScrapReworkRecordsAsync(defectId);
            return Ok(records);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询报废返工记录失败", detail = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ScrapReworkRecord>> Create([FromBody] ScrapReworkRecord record)
    {
        try
        {
            var created = await _capaService.CreateScrapReworkAsync(record);
            return CreatedAtAction(nameof(GetAll), new { defectId = created.DefectId }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建报废返工记录失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}/result")]
    public async Task<ActionResult<ScrapReworkRecord>> UpdateReworkResult(long id, [FromBody] UpdateReworkResultRequest request)
    {
        try
        {
            var updated = await _capaService.UpdateReworkInspectionResultAsync(id, request.Result);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新返工结果失败", detail = ex.Message });
        }
    }
}

public class UpdateReworkResultRequest
{
    public string Result { get; set; } = "pending";
}

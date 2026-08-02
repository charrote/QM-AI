using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Models.M09;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M09;

[ApiController]
[Route("api/v1/m09/d8reports")]
public class D8ReportController : ControllerBase
{
    private readonly D8ReportService _service;

    public D8ReportController(D8ReportService service) => _service = service;

    [HttpGet("complaint/{complaintId}")]
    public async Task<IActionResult> GetByComplaint(long complaintId)
    {
        try
        {
            var result = await _service.GetByComplaintIdAsync(complaintId);
            return Ok(result != null ? new List<D8Report> { result } : new List<D8Report>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询D8报告失败", detail = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            return Ok(await _service.GetListAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询D8报告失败", detail = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "8D报告不存在" });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询D8报告失败", detail = ex.Message });
        }
    }

    [HttpPost("complaint/{complaintId}")]
    public async Task<IActionResult> Create(long complaintId, [FromBody] D8Report report)
    {
        try
        {
            return CreatedAtAction(nameof(GetByComplaint), new { complaintId }, await _service.CreateOrUpdateAsync(complaintId, report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建D8报告失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] D8Report report)
    {
        try
        {
            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            return Ok(await _service.CreateOrUpdateAsync(existing.ComplaintId, report));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新D8报告失败", detail = ex.Message });
        }
    }

    [HttpPost("{id}/advance")]
    public async Task<IActionResult> AdvanceStep(long id, [FromBody] DisciplineAdvanceRequest req)
    {
        try
        {
            return Ok(await _service.AdvanceStepAsync(id, req.StepDelta));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "推进步骤失败", detail = ex.Message });
        }
    }

    public sealed class DisciplineAdvanceRequest { public int DisciplineIndex { get; set; } public int StepDelta { get; set; } = 1; public string? JsonPatch { get; set; } }
}

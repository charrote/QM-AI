using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.Models.M09;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M09;

[ApiController]
[Route("api/v1/m09/complaints")]
public class ComplaintController : ControllerBase
{
    private readonly ComplaintService _service;

    public ComplaintController(ComplaintService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? customerId, [FromQuery] string? severity, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var (items, total) = await _service.GetAllAsync(customerId, severity, status, page, pageSize);
        return Ok(new PagedResult<Complaint>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var complaint = await _service.GetByIdAsync(id);
        if (complaint == null) return NotFound();
        return Ok(complaint);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Complaint complaint)
    {
        try
        {
            var created = await _service.CreateAsync(complaint);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建客诉失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Complaint complaint)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, complaint);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新客诉失败", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok(new { success = true }) : NotFound(new { message = "客诉不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除客诉失败", detail = ex.Message });
        }
    }

    [HttpPost("{id}/transition")]
    public async Task<IActionResult> TransitionStatus(long id, [FromBody] StatusTransitionRequest req)
    {
        try
        {
            var result = await _service.TransitionStatusAsync(id, req.NewStatus);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "状态变更失败", detail = ex.Message });
        }
    }

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> Timeline(long id) => Ok(await _service.GetTimelineAsync(id));

    [HttpGet("{id}/export-pdf")]
    public async Task<IActionResult> ExportPdf(long id)
    {
        try
        {
            return Content(await _service.ExportPdfAsync(id), "application/pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "导出PDF失败", detail = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            return Ok(await _service.GetStatsAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取统计失败", detail = ex.Message });
        }
    }

    public sealed class StatusTransitionRequest { public string NewStatus { get; set; } = ""; }
}

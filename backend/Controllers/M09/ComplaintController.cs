using Microsoft.AspNetCore.Mvc;
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
        return Ok(new { items, total });
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
        var created = await _service.CreateAsync(complaint);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Complaint complaint)
    {
        var updated = await _service.UpdateAsync(id, complaint);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteAsync(id));

    [HttpPost("{id}/transition")]
    public async Task<IActionResult> TransitionStatus(long id, [FromBody] StatusTransitionRequest req) => Ok(await _service.TransitionStatusAsync(id, req.NewStatus));

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> Timeline(long id) => Ok(await _service.GetTimelineAsync(id));

    [HttpGet("{id}/export-pdf")]
    public async Task<IActionResult> ExportPdf(long id) => Content(await _service.ExportPdfAsync(id), "application/pdf");

    public sealed class StatusTransitionRequest { public string NewStatus { get; set; } = ""; }
}

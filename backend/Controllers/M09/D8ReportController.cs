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
    public async Task<IActionResult> GetByComplaint(long complaintId) => Ok(await _service.GetByComplaintIdAsync(complaintId));

    [HttpPost("complaint/{complaintId}")]
    public async Task<IActionResult> Create(long complaintId, [FromBody] D8Report report) => CreatedAtAction(nameof(GetByComplaint), new { complaintId }, await _service.CreateOrUpdateAsync(complaintId, report));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] D8Report report)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();
        return Ok(await _service.CreateOrUpdateAsync(existing.ComplaintId, report));
    }

    [HttpPost("{id}/advance")]
    public async Task<IActionResult> AdvanceStep(long id, [FromBody] DisciplineAdvanceRequest req) => Ok(await _service.AdvanceStepAsync(id, req.StepDelta));

    public sealed class DisciplineAdvanceRequest { public int NextDiscipline { get; set; } public int StepDelta { get; set; } = 1; public string? JsonPatch { get; set; } }
}

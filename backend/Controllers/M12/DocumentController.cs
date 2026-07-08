using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Models.M12;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M12;

[ApiController]
[Route("api/v1/m12/documents")]
public class DocumentController : ControllerBase
{
    private readonly DocumentService _service;

    public DocumentController(DocumentService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? docType, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => Ok(await _service.GetAllAsync(docType, status, page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id) { var d = await _service.GetByIdAsync(id); if (d == null) return NotFound(); return Ok(d); }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Document doc) => CreatedAtAction(nameof(GetById), new { id = doc.Id }, await _service.CreateAsync(doc));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Document doc) { var u = await _service.UpdateAsync(id, doc); if (u == null) return NotFound(); return Ok(u); }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteAsync(id));

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(long id, [FromBody] ApproveRequest req) { var r = await _service.ApproveAsync(id, req.ReviewerId); if (r == null) return NotFound(); return Ok(r); }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(long id, [FromBody] RejectRequest req) { var r = await _service.RejectAsync(id, req.ReviewerId, req.Reason); if (r == null) return NotFound(); return Ok(r); }

    [HttpPost("{id}/versions")]
    public async Task<IActionResult> CreateVersion(long id, [FromBody] DocumentVersion ver) => CreatedAtAction(nameof(GetById), new { id }, await _service.CreateVersionAsync(id, ver));

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> Versions(long id) => Ok(await _service.GetVersionsAsync(id));

    public sealed class ApproveRequest { public string ReviewerId { get; set; } = ""; }
    public sealed class RejectRequest { public string ReviewerId { get; set; } = ""; public string? Reason { get; set; } }
}

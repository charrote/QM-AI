using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Models.M13;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M13;

[ApiController]
[Route("api/v1/m13/audits")]
public class AuditController : ControllerBase
{
    private readonly AuditService _service;

    public AuditController(AuditService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? auditType, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) => Ok(await _service.GetAllAsync(auditType, status, page, pageSize));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id) { var a = await _service.GetByIdAsync(id); if (a == null) return NotFound(); return Ok(a); }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Audit audit) => CreatedAtAction(nameof(GetById), new { id = audit.Id }, await _service.CreateAsync(audit));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Audit audit) { var u = await _service.UpdateAsync(id, audit); if (u == null) return NotFound(); return Ok(u); }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id) => Ok(await _service.DeleteAsync(id));

    [HttpPost("{auditId}/findings")]
    public async Task<IActionResult> AddFinding(long auditId, [FromBody] AuditFinding finding) => CreatedAtAction(nameof(GetById), new { id = auditId }, await _service.AddFindingAsync(auditId, finding));

    [HttpGet("{auditId}/findings")]
    public async Task<IActionResult> Findings(long auditId) => Ok(await _service.GetFindingsAsync(auditId));

    [HttpPut("findings/{findingId}")]
    public async Task<IActionResult> UpdateFinding(long findingId, [FromBody] AuditFinding finding) { var u = await _service.UpdateFindingRectificationAsync(findingId, finding); if (u == null) return NotFound(); return Ok(u); }

    [HttpPost("findings/{findingId}/verify")]
    public async Task<IActionResult> VerifyFinding(long findingId, [FromBody] VerifyRequest req) { var v = await _service.VerifyFindingAsync(findingId, req.VerifierId, req.Passed); if (v == null) return NotFound(); return Ok(v); }

    public sealed class VerifyRequest { public string VerifierId { get; set; } = ""; public bool Passed { get; set; } }
}

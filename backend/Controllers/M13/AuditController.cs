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
    public async Task<IActionResult> Create([FromBody] Audit audit)
    {
        try
        {
            return CreatedAtAction(nameof(GetById), new { id = audit.Id }, await _service.CreateAsync(audit));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建审核失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Audit audit)
    {
        try
        {
            var u = await _service.UpdateAsync(id, audit);
            if (u == null) return NotFound();
            return Ok(u);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新审核失败", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok(new { success = true }) : NotFound(new { message = "审核不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除审核失败", detail = ex.Message });
        }
    }

    [HttpPost("{auditId}/findings")]
    public async Task<IActionResult> AddFinding(long auditId, [FromBody] AuditFinding finding)
    {
        try
        {
            return CreatedAtAction(nameof(GetById), new { id = auditId }, await _service.AddFindingAsync(auditId, finding));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "添加入账失败", detail = ex.Message });
        }
    }

    [HttpGet("{auditId}/findings")]
    public async Task<IActionResult> Findings(long auditId) => Ok(await _service.GetFindingsAsync(auditId));

    [HttpPut("findings/{findingId}")]
    public async Task<IActionResult> UpdateFinding(long findingId, [FromBody] AuditFinding finding)
    {
        try
        {
            var u = await _service.UpdateFindingRectificationAsync(findingId, finding);
            if (u == null) return NotFound();
            return Ok(u);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新发现项失败", detail = ex.Message });
        }
    }

    [HttpPost("findings/{findingId}/verify")]
    public async Task<IActionResult> VerifyFinding(long findingId, [FromBody] VerifyRequest req)
    {
        try
        {
            var v = await _service.VerifyFindingAsync(findingId, req.VerifierId, req.Passed);
            if (v == null) return NotFound();
            return Ok(v);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "验证发现项失败", detail = ex.Message });
        }
    }

    public sealed class VerifyRequest { public string VerifierId { get; set; } = ""; public bool Passed { get; set; } }
}

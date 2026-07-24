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
    public async Task<IActionResult> Create([FromBody] Document doc)
    {
        try
        {
            var created = await _service.CreateAsync(doc);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建文档失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] Document doc)
    {
        try
        {
            var u = await _service.UpdateAsync(id, doc);
            if (u == null) return NotFound();
            return Ok(u);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新文档失败", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok(new { success = true }) : NotFound(new { message = "文档不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除文档失败", detail = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(long id, [FromBody] ApproveRequest req)
    {
        try
        {
            var r = await _service.ApproveAsync(id, req.ReviewerId);
            if (r == null) return NotFound();
            return Ok(r);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "审批失败", detail = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(long id, [FromBody] RejectRequest req)
    {
        try
        {
            var r = await _service.RejectAsync(id, req.ReviewerId, req.Reason);
            if (r == null) return NotFound();
            return Ok(r);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "驳回失败", detail = ex.Message });
        }
    }

    [HttpPost("{id}/versions")]
    public async Task<IActionResult> CreateVersion(long id, [FromBody] DocumentVersion ver)
    {
        try
        {
            return CreatedAtAction(nameof(GetById), new { id }, await _service.CreateVersionAsync(id, ver));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建版本失败", detail = ex.Message });
        }
    }

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> Versions(long id) => Ok(await _service.GetVersionsAsync(id));

    public sealed class ApproveRequest { public string ReviewerId { get; set; } = ""; }
    public sealed class RejectRequest { public string ReviewerId { get; set; } = ""; public string? Reason { get; set; } }
}

using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Models.M07;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers;

/// <summary>
/// M07 缺陷记录 API
/// </summary>
[ApiController]
[Route("api/v1/defects")]
public class DefectsController : ControllerBase
{
    private readonly DefectService _defectService;

    public DefectsController(DefectService defectService)
    {
        _defectService = defectService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Defect>>> GetAll(
        [FromQuery] string? sourceType,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var defects = await _defectService.GetAllAsync(sourceType, severity, status, page, pageSize);
        return Ok(new { data = defects, total = await _defectService.GetCountAsync(sourceType) });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Defect>> GetById(long id)
    {
        var defect = await _defectService.GetByIdAsync(id);
        if (defect == null) return NotFound();
        return Ok(defect);
    }

    [HttpPost]
    public async Task<ActionResult<Defect>> Create([FromBody] Defect defect)
    {
        var created = await _defectService.CreateAsync(defect);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Defect>> Update(long id, [FromBody] Defect defect)
    {
        var updated = await _defectService.UpdateAsync(id, defect);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var result = await _defectService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("count")]
    public async Task<ActionResult> GetCount([FromQuery] string? sourceType)
    {
        var count = await _defectService.GetCountAsync(sourceType);
        return Ok(new { count });
    }
}
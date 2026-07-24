using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
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
    public async Task<ActionResult<PagedResult<Defect>>> GetAll(
        [FromQuery] string? sourceType,
        [FromQuery] string? severity,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var defects = await _defectService.GetAllAsync(sourceType, severity, status, page, pageSize);
            var total = await _defectService.GetCountAsync(sourceType);
            return Ok(new PagedResult<Defect>
            {
                Items = defects,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取缺陷列表失败", detail = ex.Message });
        }
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
        try
        {
            var created = await _defectService.CreateAsync(defect);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建缺陷记录失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Defect>> Update(long id, [FromBody] Defect defect)
    {
        try
        {
            var updated = await _defectService.UpdateAsync(id, defect);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新缺陷记录失败", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        try
        {
            var result = await _defectService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除缺陷记录失败", detail = ex.Message });
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount([FromQuery] string? sourceType)
    {
        var count = await _defectService.GetCountAsync(sourceType);
        return Ok(count);
    }
}

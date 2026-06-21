using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_Inspection;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M02_Inspection;

[ApiController]
[Route("api/v1/inspection-items")]
[Authorize]
public class InspectionItemController : ControllerBase
{
    private readonly InspectionItemService _service;

    public InspectionItemController(InspectionItemService service)
    {
        _service = service;
    }

    private long GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && long.TryParse(claim.Value, out var id) ? id : 0;
    }

    /// <summary>获取检验项目列表（分页）</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<InspectionItemListDto>>> List([FromQuery] PagedRequest req)
    {
        var result = await _service.List(req);
        return Ok(result);
    }

    /// <summary>获取检验项目下拉列表（全部启用项）</summary>
    [HttpGet("select-list")]
    public async Task<ActionResult<List<InspectionItemListDto>>> GetSelectList()
    {
        var result = await _service.GetSelectList();
        return Ok(result);
    }

    /// <summary>获取检验项目详情</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<InspectionItemDetailDto>> GetById(long id)
    {
        var result = await _service.GetById(id);
        if (result == null)
            return NotFound(new { message = $"检验项目 #{id} 不存在" });
        return Ok(result);
    }

    /// <summary>创建检验项目</summary>
    [HttpPost]
    public async Task<ActionResult<InspectionItemDetailDto>> Create([FromBody] CreateInspectionItemDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _service.Create(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>更新检验项目</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<InspectionItemDetailDto>> Update(long id, [FromBody] UpdateInspectionItemDto dto)
    {
        try
        {
            var result = await _service.Update(id, dto);
            if (result == null)
                return NotFound(new { message = $"检验项目 #{id} 不存在" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>删除检验项目</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _service.Delete(id);
        if (!result)
            return NotFound(new { message = $"检验项目 #{id} 不存在" });
        return NoContent();
    }
}

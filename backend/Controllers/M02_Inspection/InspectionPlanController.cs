using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_Inspection;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M02_Inspection;

[ApiController]
[Route("api/v1/inspection-plans")]
[Authorize]
public class InspectionPlanController : ControllerBase
{
    private readonly InspectionPlanService _service;

    public InspectionPlanController(InspectionPlanService service)
    {
        _service = service;
    }

    private long GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && long.TryParse(claim.Value, out var id) ? id : 0;
    }

    /// <summary>获取检验计划列表（分页）</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<InspectionPlanListDto>>> List([FromQuery] PagedRequest req)
    {
        var result = await _service.List(req);
        return Ok(result);
    }

    /// <summary>根据业务上下文获取检验计划（用于业务模块自动加载检验项目）</summary>
    [HttpGet("by-context")]
    public async Task<ActionResult<List<InspectionPlanDetailDto>>> GetPlansByContext(
        [FromQuery] string inspectionType,
        [FromQuery] int? productId,
        [FromQuery] int? supplierId,
        [FromQuery] int? customerId,
        [FromQuery] int? processId,
        [FromQuery] int? equipmentId)
    {
        var result = await _service.GetPlansByContext(
            inspectionType, productId, supplierId, customerId, processId, equipmentId);
        return Ok(result);
    }

    /// <summary>获取检验计划详情</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<InspectionPlanDetailDto>> GetById(long id)
    {
        var result = await _service.GetById(id);
        if (result == null)
            return NotFound(new { message = $"检验计划 #{id} 不存在" });
        return Ok(result);
    }

    /// <summary>创建检验计划</summary>
    [HttpPost]
    public async Task<ActionResult<InspectionPlanDetailDto>> Create([FromBody] CreateInspectionPlanDto dto)
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

    /// <summary>更新检验计划</summary>
    [HttpPut("{id:long}")]
    public async Task<ActionResult<InspectionPlanDetailDto>> Update(long id, [FromBody] UpdateInspectionPlanDto dto)
    {
        try
        {
            var result = await _service.Update(id, dto);
            if (result == null)
                return NotFound(new { message = $"检验计划 #{id} 不存在" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>删除检验计划</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _service.Delete(id);
        if (!result)
            return NotFound(new { message = $"检验计划 #{id} 不存在" });
        return NoContent();
    }
}

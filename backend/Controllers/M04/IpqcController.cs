using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M04;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M04;

[ApiController]
[Route("api/v1/ipqc")]
[Authorize]
public class IpqcController : ControllerBase
{
    private readonly IpqcService _ipqcService;

    public IpqcController(IpqcService ipqcService)
    {
        _ipqcService = ipqcService;
    }

    // ═══════════════════════════════════════════════════════════════
    // 首件检验
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("first-pieces")]
    public async Task<ActionResult<PagedResult<IpqcFirstPieceListDto>>> ListFirstPieces([FromQuery] PagedRequest req)
    {
        var result = await _ipqcService.ListFirstPieces(req);
        return Ok(result);
    }

    [HttpGet("first-pieces/{id:long}")]
    public async Task<ActionResult<IpqcFirstPieceDetailDto>> GetFirstPiece(long id)
    {
        var result = await _ipqcService.GetFirstPiece(id);
        if (result == null) return NotFound(new { message = "首件检验不存在" });
        return Ok(result);
    }

    [HttpPost("first-pieces")]
    public async Task<ActionResult<IpqcFirstPieceDetailDto>> CreateFirstPiece([FromBody] CreateIpqcFirstPieceDto dto)
    {
        try
        {
            var result = await _ipqcService.CreateFirstPiece(dto);
            return CreatedAtAction(nameof(GetFirstPiece), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("first-pieces/{id:long}/submit")]
    public async Task<ActionResult<IpqcFirstPieceDetailDto>> SubmitFirstPiece(long id, [FromBody] SubmitIpqcFirstPieceDto dto)
    {
        var result = await _ipqcService.SubmitFirstPiece(id, dto);
        if (result == null) return NotFound(new { message = "首件检验不存在" });
        return Ok(result);
    }

    [HttpDelete("first-pieces/{id:long}")]
    public async Task<IActionResult> DeleteFirstPiece(long id)
    {
        var result = await _ipqcService.DeleteFirstPiece(id);
        if (!result) return NotFound(new { message = "首件检验不存在" });
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // 巡检计划
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("patrol-plans")]
    public async Task<ActionResult<PagedResult<IpqcPatrolPlanListDto>>> ListPatrolPlans([FromQuery] PagedRequest req)
    {
        var result = await _ipqcService.ListPatrolPlans(req);
        return Ok(result);
    }

    [HttpGet("patrol-plans/{id:long}")]
    public async Task<ActionResult<IpqcPatrolPlanListDto>> GetPatrolPlan(long id)
    {
        var result = await _ipqcService.GetPatrolPlan(id);
        if (result == null) return NotFound(new { message = "巡检计划不存在" });
        return Ok(result);
    }

    [HttpPost("patrol-plans")]
    public async Task<ActionResult<IpqcPatrolPlanListDto>> CreatePatrolPlan([FromBody] CreateIpqcPatrolPlanDto dto)
    {
        try
        {
            var result = await _ipqcService.CreatePatrolPlan(dto);
            return CreatedAtAction(nameof(GetPatrolPlan), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("patrol-plans/{id:long}")]
    public async Task<ActionResult<IpqcPatrolPlanListDto>> UpdatePatrolPlan(long id, [FromBody] UpdateIpqcPatrolPlanDto dto)
    {
        var result = await _ipqcService.UpdatePatrolPlan(id, dto);
        if (result == null) return NotFound(new { message = "巡检计划不存在" });
        return Ok(result);
    }

    [HttpDelete("patrol-plans/{id:long}")]
    public async Task<IActionResult> DeletePatrolPlan(long id)
    {
        var result = await _ipqcService.DeletePatrolPlan(id);
        if (!result) return NotFound(new { message = "巡检计划不存在" });
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // S4-02: 巡检计划自动生成
    // ═══════════════════════════════════════════════════════════════

    [HttpPost("patrol-plans/{planId:long}/generate")]
    public async Task<ActionResult<List<IpqcPatrolListDto>>> GeneratePatrols(long planId, [FromBody] PatrolPlanGenerateRequestDto dto)
    {
        try
        {
            dto.PlanId = planId;
            var result = await _ipqcService.GeneratePatrols(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 巡检记录
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("patrols")]
    public async Task<ActionResult<PagedResult<IpqcPatrolListDto>>> ListPatrols([FromQuery] PagedRequest req)
    {
        var result = await _ipqcService.ListPatrols(req);
        return Ok(result);
    }

    [HttpGet("patrols/{id:long}")]
    public async Task<ActionResult<IpqcPatrolDetailDto>> GetPatrol(long id)
    {
        var result = await _ipqcService.GetPatrol(id);
        if (result == null) return NotFound(new { message = "巡检记录不存在" });
        return Ok(result);
    }

    [HttpPost("patrols/{id:long}/submit")]
    public async Task<ActionResult<IpqcPatrolDetailDto>> SubmitPatrol(long id, [FromBody] SubmitIpqcPatrolDto dto)
    {
        var result = await _ipqcService.SubmitPatrol(id, dto);
        if (result == null) return NotFound(new { message = "巡检记录不存在" });
        return Ok(result);
    }

    [HttpPost("patrols/{id:long}/miss")]
    public async Task<IActionResult> MissPatrol(long id)
    {
        var result = await _ipqcService.MissPatrol(id);
        if (!result) return NotFound(new { message = "巡检记录不存在" });
        return Ok(new { message = "已标记为错过" });
    }

    // ═══════════════════════════════════════════════════════════════
    // S4-03: AI 风险评分
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("risk-score")]
    public async Task<ActionResult<IpqcAiRiskScoreDto>> AnalyzeRisk(
        [FromQuery] int equipmentId,
        [FromQuery] int processId,
        [FromQuery] long? workOrderId)
    {
        var result = await _ipqcService.AnalyzeRisk(equipmentId, processId, workOrderId);
        return Ok(result);
    }

    [HttpGet("risk-score/latest")]
    public async Task<ActionResult<IpqcAiRiskScoreDto>> GetLatestRiskScore(
        [FromQuery] int equipmentId,
        [FromQuery] int processId)
    {
        var result = await _ipqcService.GetLatestRiskScore(equipmentId, processId);
        if (result == null) return NotFound(new { message = "暂无风险评分数据" });
        return Ok(result);
    }

    [HttpGet("risk-score/history")]
    public async Task<ActionResult<List<IpqcAiRiskScoreDto>>> GetRiskScoreHistory(
        [FromQuery] int equipmentId,
        [FromQuery] int processId,
        [FromQuery] int hours = 24)
    {
        var result = await _ipqcService.GetRiskScoreHistory(equipmentId, processId, hours);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // 关单评估
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("closure/evaluate")]
    public async Task<ActionResult<IpqcClosureEvaluationDto>> EvaluateClosure(
        [FromQuery] long workOrderId,
        [FromQuery] int processId,
        [FromQuery] int equipmentId)
    {
        var result = await _ipqcService.EvaluateClosure(workOrderId, processId, equipmentId);
        return Ok(result);
    }
}

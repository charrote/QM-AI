using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.Models.M07;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers;

/// <summary>
/// M07 CAPA 流程 API
/// </summary>
[ApiController]
[Route("api/v1/defects/capa")]
public class CapaController : ControllerBase
{
    private readonly CapaService _capaService;

    public CapaController(CapaService capaService)
    {
        _capaService = capaService;
    }

    // ─── CAPA 单 ─────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<PagedResult<Capa>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? phase,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var (capas, total) = await _capaService.GetAllAsync(status, phase, page, pageSize);
        return Ok(new PagedResult<Capa>
        {
            Items = capas,
            Total = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Capa>> GetById(long id)
    {
        var capa = await _capaService.GetByIdAsync(id);
        if (capa == null) return NotFound();
        return Ok(capa);
    }

    [HttpPost]
    public async Task<ActionResult<Capa>> Create([FromBody] Capa capa)
    {
        try
        {
            var created = await _capaService.CreateAsync(capa);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建 CAPA 失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Capa>> Update(long id, [FromBody] Capa capa)
    {
        try
        {
            if (id != capa.Id) return BadRequest("ID 不匹配");
            var updated = await _capaService.UpdateAsync(capa);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新 CAPA 失败", detail = ex.Message });
        }
    }

    [HttpPut("{id}/phase")]
    public async Task<ActionResult<Capa>> UpdatePhase(long id, [FromBody] UpdatePhaseRequest request)
    {
        try
        {
            var (updated, error) = await _capaService.UpdatePhaseAsync(id, request.Phase);
            if (updated == null) return BadRequest(error ?? "无效的阶段变更");
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "阶段变更失败", detail = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        try
        {
            var result = await _capaService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除 CAPA 失败", detail = ex.Message });
        }
    }

    // ─── 临时措施 ────────────────────────────────────────────

    [HttpPost("{id}/temporary-measures")]
    public async Task<ActionResult<CapaTemporaryMeasure>> AddTemporaryMeasure(long id, [FromBody] CapaTemporaryMeasure measure)
    {
        try
        {
            measure.CapaId = id;
            var created = await _capaService.AddTemporaryMeasureAsync(measure);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "添加临时措施失败", detail = ex.Message });
        }
    }

    // ─── 原因分析 ────────────────────────────────────────────

    [HttpPost("{id}/root-causes")]
    public async Task<ActionResult<CapaRootCause>> AddRootCause(long id, [FromBody] CapaRootCause cause)
    {
        try
        {
            cause.CapaId = id;
            var created = await _capaService.AddRootCauseAsync(cause);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "添加原因分析失败", detail = ex.Message });
        }
    }

    [HttpGet("{id}/root-causes")]
    public async Task<ActionResult<List<CapaRootCause>>> GetRootCauses(long id)
    {
        var causes = await _capaService.GetRootCausesAsync(id);
        return Ok(causes);
    }

    // ─── 纠正措施 ────────────────────────────────────────────

    [HttpPost("{id}/corrective-actions")]
    public async Task<ActionResult<CapaCorrectiveAction>> AddCorrectiveAction(long id, [FromBody] CapaCorrectiveAction action)
    {
        try
        {
            action.CapaId = id;
            var created = await _capaService.AddCorrectiveActionAsync(action);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "添加纠正措施失败", detail = ex.Message });
        }
    }

    [HttpPut("corrective-actions/{actionId}/status")]
    public async Task<ActionResult<CapaCorrectiveAction>> UpdateCorrectiveActionStatus(long actionId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var updated = await _capaService.UpdateCorrectiveActionStatusAsync(actionId, request.Status);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新纠正措施状态失败", detail = ex.Message });
        }
    }

    // ─── 预防措施 ────────────────────────────────────────────

    [HttpPost("{id}/preventive-actions")]
    public async Task<ActionResult<CapaPreventiveAction>> AddPreventiveAction(long id, [FromBody] CapaPreventiveAction action)
    {
        try
        {
            action.CapaId = id;
            var created = await _capaService.AddPreventiveActionAsync(action);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "添加预防措施失败", detail = ex.Message });
        }
    }

    [HttpPut("preventive-actions/{actionId}/status")]
    public async Task<ActionResult<CapaPreventiveAction>> UpdatePreventiveActionStatus(long actionId, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var updated = await _capaService.UpdatePreventiveActionStatusAsync(actionId, request.Status);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新预防措施状态失败", detail = ex.Message });
        }
    }

    // ─── 验证 ────────────────────────────────────────────────

    [HttpPost("{id}/verifications")]
    public async Task<ActionResult<CapaVerification>> AddVerification(long id, [FromBody] CapaVerification verification)
    {
        try
        {
            verification.CapaId = id;
            var created = await _capaService.AddVerificationAsync(verification);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "添加验证记录失败", detail = ex.Message });
        }
    }
}

public class UpdatePhaseRequest
{
    public int Phase { get; set; }
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

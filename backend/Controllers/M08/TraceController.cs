using Microsoft.AspNetCore.Mvc;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers;

/// <summary>
/// M08 质量追溯 API
/// </summary>
[ApiController]
[Route("api/v1/trace")]
public class TraceController : ControllerBase
{
    private readonly TraceService _traceService;

    public TraceController(TraceService traceService)
    {
        _traceService = traceService;
    }

    /// <summary>按 SN 追溯</summary>
    [HttpGet("sn/{serialNumber}")]
    public async Task<ActionResult<TraceResult>> TraceBySn(string serialNumber)
    {
        try
        {
            var result = await _traceService.TraceBySnAsync(serialNumber);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "SN 追溯失败", detail = ex.Message });
        }
    }

    /// <summary>按批次号追溯</summary>
    [HttpGet("batch/{batchCode}")]
    public async Task<ActionResult<TraceResult>> TraceByBatch(string batchCode)
    {
        try
        {
            var result = await _traceService.TraceByBatchAsync(batchCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批次追溯失败", detail = ex.Message });
        }
    }

    /// <summary>按设备追溯</summary>
    [HttpGet("equipment/{equipmentId}")]
    public async Task<ActionResult<TraceResult>> TraceByEquipment(long equipmentId)
    {
        try
        {
            var result = await _traceService.TraceByEquipmentAsync(equipmentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "设备追溯失败", detail = ex.Message });
        }
    }

    /// <summary>NG 扩散分析</summary>
    [HttpGet("ng-diffusion/{batchCode}")]
    public async Task<ActionResult<NgDiffusionResult>> NgDiffusion(string batchCode)
    {
        try
        {
            var result = await _traceService.AnalyzeNgDiffusionAsync(batchCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "NG 扩散分析失败", detail = ex.Message });
        }
    }

    /// <summary>召回模拟</summary>
    [HttpGet("recall-simulation/{batchCode}")]
    public async Task<ActionResult<RecallSimulationResult>> RecallSimulation(string batchCode)
    {
        try
        {
            var result = await _traceService.SimulateRecallAsync(batchCode);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "召回模拟失败", detail = ex.Message });
        }
    }
}

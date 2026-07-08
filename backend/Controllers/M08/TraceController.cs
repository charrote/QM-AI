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
        var result = await _traceService.TraceBySnAsync(serialNumber);
        return Ok(result);
    }

    /// <summary>按批次号追溯</summary>
    [HttpGet("batch/{batchCode}")]
    public async Task<ActionResult<TraceResult>> TraceByBatch(string batchCode)
    {
        var result = await _traceService.TraceByBatchAsync(batchCode);
        return Ok(result);
    }

    /// <summary>按设备追溯</summary>
    [HttpGet("equipment/{equipmentId}")]
    public async Task<ActionResult<TraceResult>> TraceByEquipment(long equipmentId)
    {
        var result = await _traceService.TraceByEquipmentAsync(equipmentId);
        return Ok(result);
    }

    /// <summary>NG 扩散分析</summary>
    [HttpGet("ng-diffusion/{batchCode}")]
    public async Task<ActionResult<NgDiffusionResult>> NgDiffusion(string batchCode)
    {
        var result = await _traceService.AnalyzeNgDiffusionAsync(batchCode);
        return Ok(result);
    }

    /// <summary>召回模拟</summary>
    [HttpGet("recall-simulation/{batchCode}")]
    public async Task<ActionResult<RecallSimulationResult>> RecallSimulation(string batchCode)
    {
        var result = await _traceService.SimulateRecallAsync(batchCode);
        return Ok(result);
    }
}
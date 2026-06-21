using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M06;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M06;

[ApiController]
[Route("api/v1/spc")]
[Authorize]
public class SpcController : ControllerBase
{
    private readonly SpcService _spcService;
    private readonly BusinessDataService _businessDataService;

    public SpcController(SpcService spcService, BusinessDataService businessDataService)
    {
        _spcService = spcService;
        _businessDataService = businessDataService;
    }

    private long GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && long.TryParse(claim.Value, out var id) ? id : 0;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Control Charts CRUD
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// List all SPC control charts with pagination.
    /// </summary>
    [HttpGet("control-charts")]
    public async Task<ActionResult<PagedResult<SpcControlChartListDto>>> ListCharts(
        [FromQuery] PagedRequest req)
    {
        var result = await _spcService.ListCharts(req);
        return Ok(result);
    }

    /// <summary>
    /// Get a single control chart by ID.
    /// </summary>
    [HttpGet("control-charts/{id:long}")]
    public async Task<ActionResult<SpcControlChartDetailDto>> GetChart(long id)
    {
        var result = await _spcService.GetChart(id);
        if (result == null)
            return NotFound(new { message = $"SPC控制图 #{id} 不存在" });
        return Ok(result);
    }

    /// <summary>
    /// Create a new SPC control chart.
    /// </summary>
    [HttpPost("control-charts")]
    public async Task<ActionResult<SpcControlChartDetailDto>> CreateChart(
        [FromBody] CreateSpcControlChartDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _spcService.CreateChart(dto, userId);
            return CreatedAtAction(nameof(GetChart), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing SPC control chart.
    /// </summary>
    [HttpPut("control-charts/{id:long}")]
    public async Task<ActionResult<SpcControlChartDetailDto>> UpdateChart(
        long id, [FromBody] UpdateSpcControlChartDto dto)
    {
        var result = await _spcService.UpdateChart(id, dto);
        if (result == null)
            return NotFound(new { message = $"SPC控制图 #{id} 不存在" });
        return Ok(result);
    }

    /// <summary>
    /// Delete an SPC control chart and all its associated data.
    /// </summary>
    [HttpDelete("control-charts/{id:long}")]
    public async Task<IActionResult> DeleteChart(long id)
    {
        var result = await _spcService.DeleteChart(id);
        if (!result)
            return NotFound(new { message = $"SPC控制图 #{id} 不存在" });
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    //  Data Points
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// List data points for a specific control chart.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/data-points")]
    public async Task<ActionResult<PagedResult<SpcDataPointListDto>>> ListDataPoints(
        long chartId, [FromQuery] PagedRequest req)
    {
        var result = await _spcService.ListDataPoints(chartId, req);
        return Ok(result);
    }

    /// <summary>
    /// Add a single data point to a control chart.
    /// </summary>
    [HttpPost("data-points")]
    public async Task<ActionResult<SpcDataPointListDto>> CreateDataPoint(
        [FromBody] CreateSpcDataPointDto dto)
    {
        try
        {
            var result = await _spcService.CreateDataPoint(dto);
            return CreatedAtAction(nameof(ListDataPoints), new { chartId = dto.ChartId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Batch add multiple data points to a control chart.
    /// </summary>
    [HttpPost("data-points/batch")]
    public async Task<ActionResult<List<SpcDataPointListDto>>> BatchCreateDataPoints(
        [FromBody] BatchCreateDataPointsDto dto)
    {
        try
        {
            var results = await _spcService.BatchCreateDataPoints(dto);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a data point.
    /// </summary>
    [HttpDelete("data-points/{id:long}")]
    public async Task<IActionResult> DeleteDataPoint(long id)
    {
        var result = await _spcService.DeleteDataPoint(id);
        if (!result)
            return NotFound(new { message = $"数据点 #{id} 不存在" });
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    //  SPC Analysis Engine
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Run full SPC analysis on a control chart (control limits, CPK, rule violations).
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<SpcAnalysisReportDto>> AnalyzeChart(
        [FromBody] SpcAnalyzeRequestDto dto)
    {
        try
        {
            var result = await _spcService.AnalyzeChart(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Analysis Results (History)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get analysis result history for a control chart.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/analysis-results")]
    public async Task<ActionResult<List<SpcAnalysisResultDto>>> ListAnalysisResults(long chartId)
    {
        var results = await _spcService.ListAnalysisResults(chartId);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Alert Rules
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get all alert rules for a control chart.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/alert-rules")]
    public async Task<ActionResult<List<SpcAlertRuleListDto>>> ListAlertRules(long chartId)
    {
        var rules = await _spcService.ListAlertRules(chartId);
        return Ok(rules);
    }

    /// <summary>
    /// Update an alert rule (enable/disable, thresholds).
    /// </summary>
    [HttpPut("alert-rules/{id:long}")]
    public async Task<ActionResult<SpcAlertRuleListDto>> UpdateAlertRule(
        long id, [FromBody] UpdateSpcAlertRuleDto dto)
    {
        var result = await _spcService.UpdateAlertRule(id, dto);
        if (result == null)
            return NotFound(new { message = $"判异规则 #{id} 不存在" });
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Alert Triggers
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get alert triggers for a control chart.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/triggers")]
    public async Task<ActionResult<PagedResult<SpcAlertTriggerListDto>>> ListTriggers(
        long chartId, [FromQuery] PagedRequest req)
    {
        var result = await _spcService.ListTriggers(chartId, req);
        return Ok(result);
    }

    /// <summary>
    /// Mark an alert trigger as resolved.
    /// </summary>
    [HttpPut("triggers/{id:long}/resolve")]
    public async Task<ActionResult> ResolveTrigger(long id)
    {
        var result = await _spcService.ResolveTrigger(id);
        if (!result)
            return NotFound(new { message = $"报警触发记录 #{id} 不存在" });
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    //  ANOVA Analysis
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Run ANOVA variance analysis.
    /// </summary>
    [HttpPost("anova")]
    public async Task<ActionResult<List<SpcAnovaResultDto>>> RunAnova(
        [FromBody] SpcAnovaRequestDto dto)
    {
        try
        {
            var results = await _spcService.RunAnova(dto);
            return Ok(results);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get ANOVA analysis results for a control chart.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/anova")]
    public async Task<ActionResult<List<SpcAnovaResultDto>>> ListAnovaResults(long chartId)
    {
        var results = await _spcService.ListAnovaResults(chartId);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Data Sources (贯通S3/S4/S5数据到SPC)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// List data sources for a control chart.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/data-sources")]
    public async Task<ActionResult<List<SpcDataSourceDto>>> ListDataSources(long chartId)
    {
        var sources = await _spcService.ListDataSources(chartId);
        return Ok(sources);
    }

    /// <summary>
    /// Add a data source to a control chart.
    /// </summary>
    [HttpPost("data-sources")]
    public async Task<ActionResult<SpcDataSourceDto>> CreateDataSource([FromBody] CreateSpcDataSourceDto dto)
    {
        var result = await _spcService.CreateDataSource(dto);
        return CreatedAtAction(nameof(ListDataSources), new { chartId = dto.ChartId }, result);
    }

    /// <summary>
    /// Delete a data source.
    /// </summary>
    [HttpDelete("data-sources/{id:long}")]
    public async Task<IActionResult> DeleteDataSource(long id)
    {
        var result = await _spcService.DeleteDataSource(id);
        if (!result)
            return NotFound(new { message = $"数据源 #{id} 不存在" });
        return NoContent();
    }

    /// <summary>
    /// Pull measurement data from business modules (IQC/IPQC/FQC) based on data source config.
    /// </summary>
    [HttpGet("control-charts/{chartId:long}/business-data")]
    public async Task<ActionResult<List<BusinessInspectionDataDto>>> GetBusinessData(
        long chartId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var data = await _businessDataService.GetDataForChart(chartId, startDate, endDate);
        return Ok(data);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M03;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M03;

[ApiController]
[Route("api/v1/iqc")]
[Authorize]
public class IqcController : ControllerBase
{
    private readonly IqcService _iqcService;
    private readonly SamplingPlanCalculator _samplingCalculator;

    public IqcController(IqcService iqcService, SamplingPlanCalculator samplingCalculator)
    {
        _iqcService = iqcService;
        _samplingCalculator = samplingCalculator;
    }

    // ═══════════════════════════════════════════════════════════════
    // 来料登记
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取来料登记列表
    /// </summary>
    [HttpGet("receipts")]
    public async Task<ActionResult<PagedResult<IqcReceiptListDto>>> ListReceipts([FromQuery] PagedRequest req)
    {
        var result = await _iqcService.ListReceipts(req);
        return Ok(result);
    }

    /// <summary>
    /// 获取来料登记详情
    /// </summary>
    [HttpGet("receipts/{id:long}")]
    public async Task<ActionResult<IqcReceiptDetailDto>> GetReceipt(long id)
    {
        var result = await _iqcService.GetReceipt(id);
        if (result == null) return NotFound(new { message = "来料登记不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 创建来料登记（自动生成检验单）
    /// </summary>
    [HttpPost("receipts")]
    public async Task<ActionResult<IqcReceiptDetailDto>> CreateReceipt([FromBody] CreateIqcReceiptDto dto)
    {
        try
        {
            var result = await _iqcService.CreateReceipt(dto);
            return CreatedAtAction(nameof(GetReceipt), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新来料登记
    /// </summary>
    [HttpPut("receipts/{id:long}")]
    public async Task<ActionResult<IqcReceiptDetailDto>> UpdateReceipt(long id, [FromBody] UpdateIqcReceiptDto dto)
    {
        var result = await _iqcService.UpdateReceipt(id, dto);
        if (result == null) return NotFound(new { message = "来料登记不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 删除来料登记
    /// </summary>
    [HttpDelete("receipts/{id:long}")]
    public async Task<IActionResult> DeleteReceipt(long id)
    {
        var result = await _iqcService.DeleteReceipt(id);
        if (!result) return NotFound(new { message = "来料登记不存在" });
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // 检验单
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取检验单列表
    /// </summary>
    [HttpGet("inspections")]
    public async Task<ActionResult<PagedResult<IqcInspectionListDto>>> ListInspections([FromQuery] PagedRequest req)
    {
        var result = await _iqcService.ListInspections(req);
        return Ok(result);
    }

    /// <summary>
    /// 获取检验单详情
    /// </summary>
    [HttpGet("inspections/{id:long}")]
    public async Task<ActionResult<IqcInspectionDetailDto>> GetInspection(long id)
    {
        var result = await _iqcService.GetInspection(id);
        if (result == null) return NotFound(new { message = "检验单不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 手动创建检验单
    /// </summary>
    [HttpPost("inspections")]
    public async Task<ActionResult<IqcInspectionDetailDto>> CreateInspection([FromBody] CreateIqcInspectionDto dto)
    {
        var result = await _iqcService.CreateInspection(dto);
        return CreatedAtAction(nameof(GetInspection), new { id = result.Id }, result);
    }

    /// <summary>
    /// 提交检验结果（自动判定合格/不合格）
    /// </summary>
    [HttpPost("inspections/{id:long}/submit")]
    public async Task<ActionResult<IqcInspectionDetailDto>> SubmitInspection(long id, [FromBody] SubmitIqcInspectionDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _iqcService.SubmitInspection(id, dto, userId);
            if (result == null) return NotFound(new { message = "检验单不存在" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 异常单
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取异常单列表
    /// </summary>
    [HttpGet("anomalies")]
    public async Task<ActionResult<PagedResult<IqcAnomalyListDto>>> ListAnomalies([FromQuery] PagedRequest req)
    {
        var result = await _iqcService.ListAnomalies(req);
        return Ok(result);
    }

    /// <summary>
    /// 创建异常单
    /// </summary>
    [HttpPost("anomalies")]
    public async Task<ActionResult<IqcAnomalyListDto>> CreateAnomaly([FromBody] CreateIqcAnomalyDto dto)
    {
        var result = await _iqcService.CreateAnomaly(dto);
        return CreatedAtAction(nameof(GetAnomaly), new { id = result.Id }, result);
    }

    /// <summary>
    /// 获取异常单详情
    /// </summary>
    [HttpGet("anomalies/{id:long}")]
    public async Task<ActionResult<IqcAnomalyListDto>> GetAnomaly(long id)
    {
        var anomalies = await _iqcService.ListAnomalies(new PagedRequest { Page = 1, PageSize = 1, Keyword = id.ToString() });
        if (anomalies.Items.Count == 0) return NotFound(new { message = "异常单不存在" });
        return Ok(anomalies.Items[0]);
    }

    /// <summary>
    /// 更新异常单
    /// </summary>
    [HttpPut("anomalies/{id:long}")]
    public async Task<ActionResult<IqcAnomalyListDto>> UpdateAnomaly(long id, [FromBody] UpdateIqcAnomalyDto dto)
    {
        var result = await _iqcService.UpdateAnomaly(id, dto);
        if (result == null) return NotFound(new { message = "异常单不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 解决异常单
    /// </summary>
    [HttpPost("anomalies/{id:long}/resolve")]
    public async Task<IActionResult> ResolveAnomaly(long id, [FromBody] ResolveIqcAnomalyDto dto)
    {
        var result = await _iqcService.ResolveAnomaly(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// 关闭异常单
    /// </summary>
    [HttpPost("anomalies/{id:long}/close")]
    public async Task<IActionResult> CloseAnomaly(long id)
    {
        var result = await _iqcService.CloseAnomaly(id);
        return Ok(result);
    }

    /// <summary>
    /// MRB 评审
    /// </summary>
    [HttpPost("anomalies/{id:long}/mrb-review")]
    public async Task<ActionResult<IqcAnomalyListDto>> MrbReview(long id, [FromBody] MrbReviewDto dto)
    {
        try
        {
            var result = await _iqcService.MrbReview(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 做出处置决定
    /// </summary>
    [HttpPost("anomalies/{id:long}/disposition")]
    public async Task<ActionResult<IqcAnomalyListDto>> MakeDisposition(long id, [FromBody] DispositionDto dto)
    {
        try
        {
            var result = await _iqcService.MakeDisposition(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 通知供应商
    /// </summary>
    [HttpPost("anomalies/{id:long}/notify-supplier")]
    public async Task<ActionResult<IqcAnomalyListDto>> NotifySupplier(long id, [FromBody] NotifySupplierDto dto)
    {
        try
        {
            var result = await _iqcService.NotifySupplier(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 供应商评分
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取供应商评分
    /// </summary>
    [HttpGet("suppliers/{supplierId:int}/score")]
    public async Task<ActionResult<SupplierScoreDto>> GetSupplierScore(int supplierId)
    {
        var result = await _iqcService.GetSupplierScore(supplierId);
        if (result == null) return NotFound(new { message = "供应商不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 手动更新供应商评分
    /// </summary>
    [HttpPut("suppliers/{supplierId:int}/score")]
    public async Task<ActionResult<SupplierScoreDto>> UpdateSupplierScore(int supplierId, [FromBody] UpdateSupplierScoreDto dto)
    {
        var result = await _iqcService.UpdateSupplierScore(supplierId, dto);
        if (result == null) return NotFound(new { message = "供应商不存在" });
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // 抽样方案计算
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// GB/T 2828.1 抽样方案计算
    /// </summary>
    [HttpPost("sampling-plan")]
    public ActionResult<SamplingPlanDto> CalculateSamplingPlan([FromBody] SamplingPlanRequestDto dto)
    {
        var plan = _samplingCalculator.GetSamplingPlan(dto.LotSize, dto.SamplingLevel, dto.AqlValue);

        return Ok(new SamplingPlanDto
        {
            SampleCode = plan.SampleCode,
            SampleSize = plan.SampleSize,
            Ac = plan.Ac,
            Re = plan.Re,
            SamplingLevel = plan.SamplingLevel,
            AqlValue = plan.AqlValue,
            LotSize = plan.LotSize,
            IsReduced = plan.Severity == "reduced",
            IsNormal = plan.Severity == "normal",
            IsStricter = plan.Severity == "tightened"
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // AI 风险分析
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// AI 来料风险分析（规则评分版）
    /// </summary>
    [HttpGet("receipts/{receiptId:long}/risk-analysis")]
    public async Task<ActionResult<AiRiskScoreDto>> AnalyzeRisk(long receiptId)
    {
        try
        {
            var result = await _iqcService.AnalyzeRisk(receiptId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 批次追溯
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 按批次号追溯
    /// </summary>
    [HttpGet("trace/{batchNo}")]
    public async Task<ActionResult<BatchTraceDto>> TraceByBatch(string batchNo)
    {
        var result = await _iqcService.TraceByBatch(batchNo);
        if (result == null) return NotFound(new { message = "批次未找到" });
        return Ok(result);
    }

    /// <summary>
    /// 按批次号追溯（支持 URL 编码）
    /// </summary>
    [HttpGet("trace")]
    public async Task<ActionResult<BatchTraceDto>> TraceByBatchQuery([FromQuery] string batchNo)
    {
        if (string.IsNullOrWhiteSpace(batchNo))
            return BadRequest(new { message = "批次号不能为空" });

        var result = await _iqcService.TraceByBatch(batchNo);
        if (result == null) return NotFound(new { message = "批次未找到" });
        return Ok(result);
    }
}

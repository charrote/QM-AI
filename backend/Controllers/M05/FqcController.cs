using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M05;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M05;

[ApiController]
[Route("api/v1/fqc")]
[Authorize]
public class FqcController : ControllerBase
{
    private readonly FqcService _fqcService;

    public FqcController(FqcService fqcService)
    {
        _fqcService = fqcService;
    }

    // ═══════════════════════════════════════════════════════════════
    // 批次管理
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取批次列表
    /// </summary>
    [HttpGet("batches")]
    public async Task<ActionResult<PagedResult<ProductBatchListDto>>> ListBatches([FromQuery] PagedRequest req)
    {
        var result = await _fqcService.ListBatches(req);
        return Ok(result);
    }

    /// <summary>
    /// 获取批次详情
    /// </summary>
    [HttpGet("batches/{id:long}")]
    public async Task<ActionResult<ProductBatchDetailDto>> GetBatch(long id)
    {
        var result = await _fqcService.GetBatch(id);
        if (result == null) return NotFound(new { message = "批次不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 创建批次（自动生成批次号）
    /// </summary>
    [HttpPost("batches")]
    public async Task<ActionResult<ProductBatchDetailDto>> CreateBatch([FromBody] CreateProductBatchDto dto)
    {
        try
        {
            var result = await _fqcService.CreateBatch(dto);
            return CreatedAtAction(nameof(GetBatch), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新批次
    /// </summary>
    [HttpPut("batches/{id:long}")]
    public async Task<ActionResult<ProductBatchDetailDto>> UpdateBatch(long id, [FromBody] UpdateProductBatchDto dto)
    {
        var result = await _fqcService.UpdateBatch(id, dto);
        if (result == null) return NotFound(new { message = "批次不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 生成批次号（LOT-YYYYMMDD-X）
    /// </summary>
    [HttpPost("batches/generate-number")]
    public async Task<ActionResult<BatchNumberGenerateDto>> GenerateBatchNumber()
    {
        var batchNo = await _fqcService.GenerateBatchNumber();
        return Ok(new BatchNumberGenerateDto { BatchCode = batchNo });
    }

    // ═══════════════════════════════════════════════════════════════
    // 成品检验
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取检验列表
    /// </summary>
    [HttpGet("inspections")]
    public async Task<ActionResult<PagedResult<FqcInspectionListDto>>> ListInspections([FromQuery] PagedRequest req)
    {
        var result = await _fqcService.ListInspections(req);
        return Ok(result);
    }

    /// <summary>
    /// 获取检验详情
    /// </summary>
    [HttpGet("inspections/{id:long}")]
    public async Task<ActionResult<FqcInspectionDetailDto>> GetInspection(long id)
    {
        var result = await _fqcService.GetInspection(id);
        if (result == null) return NotFound(new { message = "检验单不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 创建检验单
    /// </summary>
    [HttpPost("inspections")]
    public async Task<ActionResult<FqcInspectionDetailDto>> CreateInspection([FromBody] CreateFqcInspectionDto dto)
    {
        try
        {
            var result = await _fqcService.CreateInspection(dto);
            return CreatedAtAction(nameof(GetInspection), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 提交检验结果（自动判定合格/不合格）
    /// </summary>
    [HttpPost("inspections/{id:long}/submit")]
    public async Task<ActionResult<FqcInspectionDetailDto>> SubmitInspection(long id, [FromBody] SubmitFqcInspectionDto dto)
    {
        try
        {
            var result = await _fqcService.SubmitInspection(id, dto);
            if (result == null) return NotFound(new { message = "检验单不存在" });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 出货放行
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取放行单列表
    /// </summary>
    [HttpGet("releases")]
    public async Task<ActionResult<PagedResult<OqcReleaseListDto>>> ListReleases([FromQuery] PagedRequest req)
    {
        var result = await _fqcService.ListReleases(req);
        return Ok(result);
    }

    /// <summary>
    /// 获取放行单详情
    /// </summary>
    [HttpGet("releases/{id:long}")]
    public async Task<ActionResult<OqcReleaseListDto>> GetRelease(long id)
    {
        var result = await _fqcService.GetRelease(id);
        if (result == null) return NotFound(new { message = "放行单不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 创建放行单
    /// </summary>
    [HttpPost("releases")]
    public async Task<ActionResult<OqcReleaseListDto>> CreateRelease([FromBody] CreateOqcReleaseDto dto)
    {
        try
        {
            var result = await _fqcService.CreateRelease(dto);
            return CreatedAtAction(nameof(GetRelease), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 电子签名（签名图片保存到 MinIO）
    /// </summary>
    [HttpPost("releases/{id:long}/sign")]
    public async Task<ActionResult<OqcReleaseListDto>> SignRelease(long id, [FromBody] SignOqcReleaseDto dto)
    {
        var result = await _fqcService.SignRelease(id, dto);
        if (result == null) return NotFound(new { message = "放行单不存在" });
        return Ok(result);
    }

    /// <summary>
    /// 确认放行
    /// </summary>
    [HttpPost("releases/{id:long}/confirm")]
    public async Task<IActionResult> ConfirmRelease(long id)
    {
        var result = await _fqcService.ConfirmRelease(id);
        if (!result) return NotFound(new { message = "放行单不存在" });
        return Ok(new { message = "放行已完成" });
    }

    // ═══════════════════════════════════════════════════════════════
    // 包装确认
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 获取包装确认列表
    /// </summary>
    [HttpGet("packaging")]
    public async Task<ActionResult<PagedResult<PackagingConfirmationListDto>>> ListPackaging([FromQuery] PagedRequest req)
    {
        var result = await _fqcService.ListPackagingConfirmations(req);
        return Ok(result);
    }

    /// <summary>
    /// 创建包装确认
    /// </summary>
    [HttpPost("packaging")]
    public async Task<ActionResult<PackagingConfirmationListDto>> CreatePackaging([FromBody] CreatePackagingConfirmationDto dto)
    {
        var result = await _fqcService.CreatePackagingConfirmation(dto);
        return CreatedAtAction(null, result);
    }

    /// <summary>
    /// 更新标签打印状态
    /// </summary>
    [HttpPut("packaging/{id:long}/label-printed")]
    public async Task<IActionResult> UpdateLabelPrinted(long id, [FromBody] bool printed)
    {
        var result = await _fqcService.UpdateLabelPrinted(id, printed);
        if (!result) return NotFound(new { message = "包装确认不存在" });
        return Ok(new { message = "标签打印状态已更新" });
    }
}

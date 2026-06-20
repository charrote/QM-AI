using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_5;
using QM_AI.API.Models.M02_5;

namespace QM_AI.API.Controllers.M02_5;

[ApiController]
[Route("api/v1/dynamic-params")]
[Authorize]
public class DynamicParamsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DynamicParamsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<DynamicParamListDto>>> List([FromQuery] PagedRequest req, [FromQuery] long? groupId)
    {
        var query = _db.DynamicParams.Include(p => p.Group).AsQueryable();

        if (groupId.HasValue)
            query = query.Where(p => p.GroupId == groupId.Value);

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p => p.Name.Contains(req.Keyword) || p.Code.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Code)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new DynamicParamListDto
            {
                Id = p.Id,
                GroupId = p.GroupId,
                GroupName = p.Group.Name,
                Name = p.Name,
                Code = p.Code,
                DataType = p.DataType,
                Unit = p.Unit,
                TargetValue = p.TargetValue,
                Usl = p.Usl,
                Lsl = p.Lsl,
                Precision = p.Precision,
                AiStrategy = p.AiStrategy,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();

        return Ok(new PagedResult<DynamicParamListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        });
    }

    /// <summary>
    /// 按组获取参数列表（不分页，供前端树形结构使用）
    /// </summary>
    [HttpGet("by-group/{groupId:long}")]
    public async Task<ActionResult<List<DynamicParamListDto>>> GetByGroup(long groupId)
    {
        var items = await _db.DynamicParams
            .Include(p => p.Group)
            .Where(p => p.GroupId == groupId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new DynamicParamListDto
            {
                Id = p.Id,
                GroupId = p.GroupId,
                GroupName = p.Group.Name,
                Name = p.Name,
                Code = p.Code,
                DataType = p.DataType,
                Unit = p.Unit,
                TargetValue = p.TargetValue,
                Usl = p.Usl,
                Lsl = p.Lsl,
                Precision = p.Precision,
                AiStrategy = p.AiStrategy,
                SortOrder = p.SortOrder,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DynamicParamDetailDto>> Get(long id)
    {
        var entity = await _db.DynamicParams.Include(p => p.Group).FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return NotFound();
        return Ok(MapToDetail(entity));
    }

    [HttpPost]
    public async Task<ActionResult<DynamicParamDetailDto>> Create([FromBody] CreateDynamicParamDto dto)
    {
        if (await _db.DynamicParams.AnyAsync(p => p.Code == dto.Code))
            return Conflict(new { message = $"参数编码 '{dto.Code}' 已存在" });

        // 验证参数组存在
        if (!await _db.ParamGroups.AnyAsync(g => g.Id == dto.GroupId))
            return BadRequest(new { message = "指定的参数组不存在" });

        // 根据数据类型校验
        if (dto.DataType == "numeric" && dto.Usl.HasValue && dto.Lsl.HasValue && dto.Usl < dto.Lsl)
            return BadRequest(new { message = "上规格限(USL)必须大于下规格限(LSL)" });

        var entity = new DynamicParam
        {
            GroupId = dto.GroupId,
            Name = dto.Name,
            Code = dto.Code,
            DataType = dto.DataType,
            Unit = dto.Unit,
            TargetValue = dto.TargetValue,
            Usl = dto.Usl,
            Lsl = dto.Lsl,
            Precision = dto.Precision,
            AiStrategy = dto.AiStrategy,
            SortOrder = dto.SortOrder,
            CreatedBy = 1, // TODO: 从 JWT 获取
        };
        _db.DynamicParams.Add(entity);
        await _db.SaveChangesAsync();

        // 重新加载导航属性
        await _db.Entry(entity).Reference(p => p.Group).LoadAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, MapToDetail(entity));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<DynamicParamDetailDto>> Update(long id, [FromBody] UpdateDynamicParamDto dto)
    {
        var entity = await _db.DynamicParams.FindAsync(id);
        if (entity == null) return NotFound();

        if (dto.DataType == "numeric" && dto.Usl.HasValue && dto.Lsl.HasValue && dto.Usl < dto.Lsl)
            return BadRequest(new { message = "上规格限(USL)必须大于下规格限(LSL)" });

        entity.GroupId = dto.GroupId;
        entity.Name = dto.Name;
        entity.DataType = dto.DataType;
        entity.Unit = dto.Unit;
        entity.TargetValue = dto.TargetValue;
        entity.Usl = dto.Usl;
        entity.Lsl = dto.Lsl;
        entity.Precision = dto.Precision;
        entity.AiStrategy = dto.AiStrategy;
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(p => p.Group).LoadAsync();
        return Ok(MapToDetail(entity));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.DynamicParams.FindAsync(id);
        if (entity == null) return NotFound();
        _db.DynamicParams.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// AI 策略预置列表
    /// </summary>
    [HttpGet("ai-strategies")]
    public ActionResult<List<AiStrategyPresetDto>> GetAiStrategies()
    {
        var presets = new List<AiStrategyPresetDto>
        {
            new() { Id = "normal_distribution", Name = "正态分布检测", DataType = "numeric", Description = "检测参数是否符合正态分布，识别异常偏移" },
            new() { Id = "outlier_detection", Name = "异常值检测", DataType = "numeric", Description = "基于 IQR 或 Z-Score 检测异常值" },
            new() { Id = "cpk_monitoring", Name = "Cpk 监控", DataType = "numeric", Description = "实时计算过程能力指数 Cpk" },
            new() { Id = "pareto_analysis", Name = "帕累托分析", DataType = "categorical", Description = "对枚举型参数进行帕累托统计分析" },
            new() { Id = "trend_analysis", Name = "趋势分析", DataType = "numeric", Description = "检测参数的时间序列趋势变化" },
            new() { Id = "boolean_alert", Name = "布尔告警", DataType = "boolean", Description = "基于布尔值触发告警" },
        };
        return Ok(presets);
    }

    private static DynamicParamDetailDto MapToDetail(DynamicParam p) => new()
    {
        Id = p.Id,
        GroupId = p.GroupId,
        GroupName = p.Group?.Name ?? "",
        Name = p.Name,
        Code = p.Code,
        DataType = p.DataType,
        Unit = p.Unit,
        TargetValue = p.TargetValue,
        Usl = p.Usl,
        Lsl = p.Lsl,
        Precision = p.Precision,
        AiStrategy = p.AiStrategy,
        SortOrder = p.SortOrder,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };
}

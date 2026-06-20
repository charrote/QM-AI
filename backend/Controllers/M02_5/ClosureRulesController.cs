using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_5;
using QM_AI.API.Models.M02_5;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M02_5;

[ApiController]
[Route("api/v1/closure-rules")]
[Authorize]
public class ClosureRulesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ClosureRuleEngine _engine;
    public ClosureRulesController(AppDbContext db, ClosureRuleEngine engine)
    {
        _db = db;
        _engine = engine;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClosureRuleListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.ClosureRules.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(r => r.Name.Contains(req.Keyword) || r.Code.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new ClosureRuleListDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Logic = r.Logic,
                Description = r.Description,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();

        return Ok(new PagedResult<ClosureRuleListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        });
    }

    /// <summary>
    /// 获取全部启用的规则（不分页，供下拉选择等使用）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<ClosureRuleListDto>>> GetAll()
    {
        var items = await _db.ClosureRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ClosureRuleListDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                Logic = r.Logic,
                Description = r.Description,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ClosureRuleDetailDto>> Get(long id)
    {
        var entity = await _db.ClosureRules.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(MapToDetail(entity));
    }

    [HttpPost]
    public async Task<ActionResult<ClosureRuleDetailDto>> Create([FromBody] CreateClosureRuleDto dto)
    {
        if (await _db.ClosureRules.AnyAsync(r => r.Code == dto.Code))
            return Conflict(new { message = $"关单规则编码 '{dto.Code}' 已存在" });

        var entity = new ClosureRule
        {
            Name = dto.Name,
            Code = dto.Code,
            ConditionJson = dto.ConditionJson,
            Logic = dto.Logic,
            Description = dto.Description,
            CreatedBy = 1, // TODO: 从 JWT 获取
        };
        _db.ClosureRules.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, MapToDetail(entity));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ClosureRuleDetailDto>> Update(long id, [FromBody] UpdateClosureRuleDto dto)
    {
        var entity = await _db.ClosureRules.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Name = dto.Name;
        entity.ConditionJson = dto.ConditionJson;
        entity.Logic = dto.Logic;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(MapToDetail(entity));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.ClosureRules.FindAsync(id);
        if (entity == null) return NotFound();
        _db.ClosureRules.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// 评估关单条件
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<ClosureEvaluationResultDto>> Evaluate([FromBody] ClosureEvaluationRequest req)
    {
        var rule = await _db.ClosureRules.FindAsync(req.RuleId);
        if (rule == null) return NotFound(new { message = "关单规则不存在" });

        // 当前使用模拟上下文进行测试评估
        // 实际使用时，InspectionContext 将包含真实检验数据
        var context = new InspectionContext();
        var result = _engine.Evaluate(rule, context);

        return Ok(new ClosureEvaluationResultDto
        {
            IsSatisfied = result.IsSatisfied,
            FailedConditions = result.FailedConditions,
            RuleName = rule.Name,
            Logic = rule.Logic,
        });
    }

    private static ClosureRuleDetailDto MapToDetail(ClosureRule r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Code = r.Code,
        ConditionJson = r.ConditionJson,
        Logic = r.Logic,
        Description = r.Description,
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}

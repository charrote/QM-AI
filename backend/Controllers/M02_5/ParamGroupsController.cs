using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_5;
using QM_AI.API.Models.M02_5;

namespace QM_AI.API.Controllers.M02_5;

[ApiController]
[Route("api/v1/param-groups")]
[Authorize]
public class ParamGroupsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ParamGroupsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ParamGroupListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.ParamGroups.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(g => g.Name.Contains(req.Keyword) || g.Code.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Code)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(g => new ParamGroupListDto
            {
                Id = g.Id,
                Name = g.Name,
                Code = g.Code,
                Description = g.Description,
                SortOrder = g.SortOrder,
                ParamCount = g.DynamicParams.Count(p => p.IsActive),
                CreatedAt = g.CreatedAt,
            })
            .ToListAsync();

        return Ok(new PagedResult<ParamGroupListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        });
    }

    /// <summary>
    /// 获取全部参数组（树形结构用，不分页）
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<ParamGroupListDto>>> GetAll()
    {
        var items = await _db.ParamGroups
            .OrderBy(g => g.SortOrder)
            .Select(g => new ParamGroupListDto
            {
                Id = g.Id,
                Name = g.Name,
                Code = g.Code,
                Description = g.Description,
                SortOrder = g.SortOrder,
                ParamCount = g.DynamicParams.Count(p => p.IsActive),
                CreatedAt = g.CreatedAt,
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ParamGroupDetailDto>> Get(long id)
    {
        var entity = await _db.ParamGroups.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(MapToDetail(entity));
    }

    [HttpPost]
    public async Task<ActionResult<ParamGroupDetailDto>> Create([FromBody] CreateParamGroupDto dto)
    {
        if (await _db.ParamGroups.AnyAsync(g => g.Code == dto.Code))
            return Conflict(new { message = $"参数组编码 '{dto.Code}' 已存在" });

        var entity = new ParamGroup
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            CreatedBy = 1, // TODO: 从 JWT 中获取当前用户 ID
        };
        _db.ParamGroups.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, MapToDetail(entity));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ParamGroupDetailDto>> Update(long id, [FromBody] UpdateParamGroupDto dto)
    {
        var entity = await _db.ParamGroups.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(MapToDetail(entity));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.ParamGroups.FindAsync(id);
        if (entity == null) return NotFound();

        // 检查组下是否有参数
        if (await _db.DynamicParams.AnyAsync(p => p.GroupId == id))
            return Conflict(new { message = "该参数组下存在参数，无法删除。请先删除或移动组内参数。" });

        _db.ParamGroups.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ParamGroupDetailDto MapToDetail(ParamGroup g) => new()
    {
        Id = g.Id,
        Name = g.Name,
        Code = g.Code,
        Description = g.Description,
        SortOrder = g.SortOrder,
        CreatedAt = g.CreatedAt,
        UpdatedAt = g.UpdatedAt,
    };
}

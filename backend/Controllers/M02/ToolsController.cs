using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/tools")]
[Authorize]
public class ToolsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ToolsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ToolListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Tools.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(t => t.Code.Contains(req.Keyword) || t.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(t => t.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(t => new ToolListDto
            {
                Id = t.Id, Code = t.Code, Name = t.Name, Model = t.Model,
                ToolType = t.ToolType, DesignLife = t.DesignLife,
                LifeUnit = t.LifeUnit, CurrentLife = t.CurrentLife,
                Supplier = t.Supplier, IsActive = t.IsActive,
            })
            .ToListAsync();
        return Ok(new PagedResult<ToolListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tool>> Get(long id)
    {
        var entity = await _db.Tools.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Tool>> Create([FromBody] CreateToolDto dto)
    {
        if (await _db.Tools.AnyAsync(t => t.Code == dto.Code))
            return Conflict(new { message = $"刀具编码 '{dto.Code}' 已存在" });

        var entity = new Tool
        {
            Code = dto.Code, Name = dto.Name, Model = dto.Model,
            ToolType = dto.ToolType, DesignLife = dto.DesignLife,
            LifeUnit = dto.LifeUnit, CurrentLife = dto.CurrentLife, Supplier = dto.Supplier,
        };
        _db.Tools.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Tool>> Update(long id, [FromBody] UpdateToolDto dto)
    {
        var entity = await _db.Tools.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.Tools.AnyAsync(t => t.Code == dto.Code && t.Id != id))
            return Conflict(new { message = $"刀具编码 '{dto.Code}' 已被其他刀具使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Model = dto.Model;
        entity.ToolType = dto.ToolType; entity.DesignLife = dto.DesignLife;
        entity.LifeUnit = dto.LifeUnit; entity.CurrentLife = dto.CurrentLife;
        entity.Supplier = dto.Supplier; entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Tools.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Tools.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

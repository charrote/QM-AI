using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M15;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M15;

[ApiController]
[Route("api/v1/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PermissionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<PermissionListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Permissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p => p.Name.Contains(req.Keyword) || p.Code.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(p => new PermissionListDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Module = p.Module,
            })
            .ToListAsync();

        return Ok(new PagedResult<PermissionListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PermissionListDto>> Get(long id)
    {
        var entity = await _db.Permissions.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return NotFound();
        return Ok(new PermissionListDto { Id = entity.Id, Name = entity.Name, Code = entity.Code, Module = entity.Module });
    }

    [HttpPost]
    public async Task<ActionResult<PermissionListDto>> Create([FromBody] CreatePermissionDto dto)
    {
        if (await _db.Permissions.AnyAsync(p => p.Code == dto.Code))
            return Conflict(new { message = $"权限编码 '{dto.Code}' 已存在" });

        var entity = new Permission
        {
            Name = dto.Name,
            Code = dto.Code,
            Module = dto.Module,
        };
        _db.Permissions.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new PermissionListDto { Id = entity.Id, Name = entity.Name, Code = entity.Code, Module = entity.Module });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PermissionListDto>> Update(long id, [FromBody] UpdatePermissionDto dto)
    {
        var entity = await _db.Permissions.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.Permissions.AnyAsync(p => p.Code == dto.Code && p.Id != id))
            return Conflict(new { message = $"权限编码 '{dto.Code}' 已被其他权限使用" });

        entity.Name = dto.Name ?? entity.Name;
        entity.Code = dto.Code ?? entity.Code;
        entity.Module = dto.Module ?? entity.Module;
        await _db.SaveChangesAsync();
        return Ok(new PermissionListDto { Id = entity.Id, Name = entity.Name, Code = entity.Code, Module = entity.Module });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Permissions.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Permissions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

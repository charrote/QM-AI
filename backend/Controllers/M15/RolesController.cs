using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M15;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M15;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RolesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<RoleListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(r => r.Name.Contains(req.Keyword) || (r.Description != null && r.Description.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.Name)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(r => new RoleListDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                UserCount = r.Users != null ? r.Users.Count : 0,
                CreatedAt = DateTime.UtcNow,
            })
            .ToListAsync();

        return Ok(new PagedResult<RoleListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleListDto>> Get(long id)
    {
        var entity = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return NotFound();
        return Ok(new RoleListDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            UserCount = entity.Users?.Count ?? 0,
            CreatedAt = DateTime.UtcNow,
        });
    }

    [HttpPost]
    public async Task<ActionResult<RoleListDto>> Create([FromBody] CreateRoleDto dto)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == dto.Name))
            return Conflict(new { message = $"角色名称 '{dto.Name}' 已存在" });

        var entity = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
        };
        _db.Roles.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new RoleListDto
        {
            Id = entity.Id, Name = entity.Name, Description = entity.Description, UserCount = 0, CreatedAt = DateTime.UtcNow,
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoleListDto>> Update(long id, [FromBody] UpdateRoleDto dto)
    {
        var entity = await _db.Roles.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Name = dto.Name ?? entity.Name;
        entity.Description = dto.Description ?? entity.Description;
        await _db.SaveChangesAsync();
        return Ok(new RoleListDto { Id = entity.Id, Name = entity.Name, Description = entity.Description, UserCount = entity.Users?.Count ?? 0, CreatedAt = DateTime.UtcNow });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Roles.FindAsync(id);
        if (entity == null) return NotFound();
        // Check if role has users
        if (await _db.Users.AnyAsync(u => u.RoleId == id))
            return BadRequest(new { message = "该角色下仍有用户，无法删除" });
        _db.Roles.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

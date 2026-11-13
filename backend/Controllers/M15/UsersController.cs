using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M15;
using QM_AI.API.Models;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M15;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "system:user")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuthService _auth;

    public UsersController(AppDbContext db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Users
            .Include(u => u.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(u => u.Username.Contains(req.Keyword) || (u.DisplayName != null && u.DisplayName.Contains(req.Keyword)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                RoleName = u.Role != null ? u.Role.Name : null,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
            })
            .ToListAsync();

        return Ok(new PagedResult<UserListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserListDto>> Get(long id)
    {
        var entity = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        if (entity == null) return NotFound();
        return Ok(new UserListDto
        {
            Id = entity.Id,
            Username = entity.Username,
            DisplayName = entity.DisplayName,
            Email = entity.Email,
            RoleName = entity.Role?.Name,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        });
    }

    [HttpPost]
    public async Task<ActionResult<UserListDto>> Create([FromBody] CreateUserDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            return Conflict(new { message = $"用户名 '{dto.Username}' 已存在" });

        // F2 修复：统一使用 AuthService.HashPassword (PBKDF2)
        var hash = _auth.HashPassword(dto.Password);

        var entity = new User
        {
            Username = dto.Username,
            PasswordHash = hash,
            DisplayName = dto.DisplayName,
            Email = dto.Email,
            RoleId = dto.RoleId,
            IsActive = true,
        };
        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, await GetSingle(entity.Id));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserListDto>> Update(long id, [FromBody] UpdateUserDto dto)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return NotFound();

        entity.DisplayName = dto.DisplayName ?? entity.DisplayName;
        entity.Email = dto.Email ?? entity.Email;
        if (dto.RoleId.HasValue) entity.RoleId = dto.RoleId.Value;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        if (!string.IsNullOrEmpty(dto.Password))
            entity.PasswordHash = _auth.HashPassword(dto.Password);
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(await GetSingle(id));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Users.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Users.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<Role>>> GetRoles()
    {
        var roles = await _db.Roles.OrderBy(r => r.Name).ToListAsync();
        return Ok(roles);
    }

    private async Task<UserListDto?> GetSingle(long id)
    {
        var u = await _db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
        if (u == null) return null;
        return new UserListDto
        {
            Id = u.Id, Username = u.Username, DisplayName = u.DisplayName,
            Email = u.Email, RoleName = u.Role?.Name, IsActive = u.IsActive,
            CreatedAt = u.CreatedAt, UpdatedAt = u.UpdatedAt,
        };
    }


}

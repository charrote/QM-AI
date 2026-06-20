using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/processes")]
[Authorize]
public class ProcessesController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProcessesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProcessListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Processes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p => p.Code.Contains(req.Keyword) || p.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(p => new ProcessListDto
            {
                Id = p.Id, Code = p.Code, Name = p.Name,
                ProcessType = p.ProcessType, Department = p.Department, IsActive = p.IsActive,
            })
            .ToListAsync();
        return Ok(new PagedResult<ProcessListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Process>> Get(int id)
    {
        var entity = await _db.Processes.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Process>> Create([FromBody] CreateProcessDto dto)
    {
        if (await _db.Processes.AnyAsync(p => p.Code == dto.Code))
            return Conflict(new { message = $"工序编码 '{dto.Code}' 已存在" });

        var entity = new Process
        {
            Code = dto.Code, Name = dto.Name, Description = dto.Description,
            ProcessType = dto.ProcessType, Department = dto.Department,
        };
        _db.Processes.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Process>> Update(int id, [FromBody] UpdateProcessDto dto)
    {
        var entity = await _db.Processes.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.Processes.AnyAsync(p => p.Code == dto.Code && p.Id != id))
            return Conflict(new { message = $"工序编码 '{dto.Code}' 已被其他工序使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Description = dto.Description;
        entity.ProcessType = dto.ProcessType; entity.Department = dto.Department;
        entity.IsActive = dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Processes.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Processes.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

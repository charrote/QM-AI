using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/defect-codes")]
[Authorize]
public class DefectCodesController : ControllerBase
{
    private readonly AppDbContext _db;
    public DefectCodesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<DefectCodeListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.DefectCodes.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(d => d.Code.Contains(req.Keyword) || d.Name.Contains(req.Keyword));
        if (req.DefectTypes != null && req.DefectTypes.Length > 0)
            query = query.Where(d => req.DefectTypes.Contains(d.DefectType!));
        if (req.Severities != null && req.Severities.Length > 0)
            query = query.Where(d => req.Severities.Contains(d.Severity));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(d => d.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(d => new DefectCodeListDto
            {
                Id = d.Id, Code = d.Code, Name = d.Name,
                DefectType = d.DefectType, Severity = d.Severity,
                IsReworkable = d.IsReworkable, IsActive = d.IsActive,
            })
            .ToListAsync();
        return Ok(new PagedResult<DefectCodeListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DefectCode>> Get(long id)
    {
        var entity = await _db.DefectCodes.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<DefectCode>> Create([FromBody] CreateDefectCodeDto dto)
    {
        if (await _db.DefectCodes.AnyAsync(d => d.Code == dto.Code))
            return Conflict(new { message = $"不良代码 '{dto.Code}' 已存在" });

        var entity = new DefectCode
        {
            Code = dto.Code, Name = dto.Name, Description = dto.Description,
            DefectType = dto.DefectType, Severity = dto.Severity, IsReworkable = dto.IsReworkable,
        };
        _db.DefectCodes.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DefectCode>> Update(long id, [FromBody] UpdateDefectCodeDto dto)
    {
        var entity = await _db.DefectCodes.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.DefectCodes.AnyAsync(d => d.Code == dto.Code && d.Id != id))
            return Conflict(new { message = $"不良代码 '{dto.Code}' 已被使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Description = dto.Description;
        entity.DefectType = dto.DefectType; entity.Severity = dto.Severity;
        entity.IsReworkable = dto.IsReworkable; entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.DefectCodes.FindAsync(id);
        if (entity == null) return NotFound();
        _db.DefectCodes.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

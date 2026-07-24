using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/inspection-standards")]
[Authorize]
public class InspectionStandardsController : ControllerBase
{
    private readonly AppDbContext _db;
    public InspectionStandardsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<InspectionStandardListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.InspectionStandards
            .Include(s => s.Product).Include(s => s.Process).AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(s => s.Code.Contains(req.Keyword) || s.Name.Contains(req.Keyword) || s.ItemName.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(s => new InspectionStandardListDto
            {
                Id = s.Id, Code = s.Code, Name = s.Name,
                InspectionType = s.InspectionType,
                ProductName = s.Product != null ? s.Product.Name : null,
                ProcessName = s.Process != null ? s.Process.Name : null,
                ItemName = s.ItemName, Usl = s.Usl, Lsl = s.Lsl, Unit = s.Unit,
                IsActive = s.IsActive,
            })
            .ToListAsync();
        return Ok(new PagedResult<InspectionStandardListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InspectionStandard>> Get(long id)
    {
        var entity = await _db.InspectionStandards
            .Include(s => s.Product).Include(s => s.Process)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<InspectionStandard>> Create([FromBody] CreateInspectionStandardDto dto)
    {
        var entity = new InspectionStandard
        {
            Code = dto.Code, Name = dto.Name, Description = dto.Description,
            InspectionType = dto.InspectionType, ProductId = dto.ProductId,
            ProcessId = dto.ProcessId, ItemName = dto.ItemName,
            Usl = dto.Usl, Lsl = dto.Lsl, Target = dto.Target,
            Unit = dto.Unit, InspectionMethod = dto.InspectionMethod,
            SamplingFrequency = dto.SamplingFrequency,
        };
        _db.InspectionStandards.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InspectionStandard>> Update(long id, [FromBody] UpdateInspectionStandardDto dto)
    {
        var entity = await _db.InspectionStandards.FindAsync(id);
        if (entity == null) return NotFound();

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Description = dto.Description;
        entity.InspectionType = dto.InspectionType; entity.ProductId = dto.ProductId;
        entity.ProcessId = dto.ProcessId; entity.ItemName = dto.ItemName;
        entity.Usl = dto.Usl; entity.Lsl = dto.Lsl; entity.Target = dto.Target;
        entity.Unit = dto.Unit; entity.InspectionMethod = dto.InspectionMethod;
        entity.SamplingFrequency = dto.SamplingFrequency;
        entity.IsActive = dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.InspectionStandards.FindAsync(id);
        if (entity == null) return NotFound();
        _db.InspectionStandards.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

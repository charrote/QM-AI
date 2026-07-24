using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/routings")]
[Authorize]
public class RoutingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoutingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<RoutingListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Routings.Include(r => r.Product).Include(r => r.Process).AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(r => r.Code.Contains(req.Keyword) || r.Product!.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.ProductId).ThenBy(r => r.StepOrder)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(r => new RoutingListDto
            {
                Id = r.Id, ProductId = r.ProductId, ProductName = r.Product!.Name,
                Code = r.Code, StepOrder = r.StepOrder, ProcessId = r.ProcessId,
                ProcessName = r.Process!.Name, StandardTimeMinutes = r.StandardTimeMinutes,
            })
            .ToListAsync();
        return Ok(new PagedResult<RoutingListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Routing>> Get(long id)
    {
        var entity = await _db.Routings.Include(r => r.Product).Include(r => r.Process).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Routing>> Create([FromBody] CreateRoutingDto dto)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });
        if (!await _db.Processes.AnyAsync(p => p.Id == dto.ProcessId))
            return BadRequest(new { message = "工序不存在" });

        var entity = new Routing
        {
            ProductId = dto.ProductId, Code = dto.Code, Description = dto.Description,
            StepOrder = dto.StepOrder, ProcessId = dto.ProcessId, StandardTimeMinutes = dto.StandardTimeMinutes,
        };
        _db.Routings.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Routing>> Update(long id, [FromBody] UpdateRoutingDto dto)
    {
        var entity = await _db.Routings.FindAsync(id);
        if (entity == null) return NotFound();
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });
        if (!await _db.Processes.AnyAsync(p => p.Id == dto.ProcessId))
            return BadRequest(new { message = "工序不存在" });

        entity.ProductId = dto.ProductId; entity.Code = dto.Code; entity.Description = dto.Description;
        entity.StepOrder = dto.StepOrder; entity.ProcessId = dto.ProcessId;
        entity.StandardTimeMinutes = dto.StandardTimeMinutes; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Routings.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Routings.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

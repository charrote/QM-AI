using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/boms")]
[Authorize]
public class BomsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BomsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<BomListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Boms.Include(b => b.Product).AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(b => b.MaterialCode.Contains(req.Keyword) || b.MaterialName.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.ProductId).ThenBy(b => b.Level)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(b => new BomListDto
            {
                Id = b.Id, ProductId = b.ProductId, ProductName = b.Product!.Name,
                MaterialCode = b.MaterialCode, MaterialName = b.MaterialName,
                Quantity = b.Quantity, Unit = b.Unit, Level = b.Level,
            })
            .ToListAsync();
        return Ok(new PagedResult<BomListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Bom>> Get(long id)
    {
        var entity = await _db.Boms.Include(b => b.Product).FirstOrDefaultAsync(b => b.Id == id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Bom>> Create([FromBody] CreateBomDto dto)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });

        var entity = new Bom
        {
            ProductId = dto.ProductId, MaterialCode = dto.MaterialCode,
            MaterialName = dto.MaterialName, Quantity = dto.Quantity,
            Unit = dto.Unit, Level = dto.Level, Remark = dto.Remark,
        };
        _db.Boms.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Bom>> Update(long id, [FromBody] UpdateBomDto dto)
    {
        var entity = await _db.Boms.FindAsync(id);
        if (entity == null) return NotFound();
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });

        entity.ProductId = dto.ProductId; entity.MaterialCode = dto.MaterialCode;
        entity.MaterialName = dto.MaterialName; entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit; entity.Level = dto.Level; entity.Remark = dto.Remark;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Boms.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Boms.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

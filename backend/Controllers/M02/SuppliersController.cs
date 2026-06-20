using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly AppDbContext _db;
    public SuppliersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<SupplierListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(s => s.Code.Contains(req.Keyword) || s.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(s => s.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(s => new SupplierListDto
            {
                Id = s.Id, Code = s.Code, Name = s.Name,
                ContactPerson = s.ContactPerson, ContactPhone = s.ContactPhone,
                Grade = s.Grade, SupplyCategory = s.SupplyCategory,
                Score = s.Score, IsActive = s.IsActive,
            })
            .ToListAsync();
        return Ok(new PagedResult<SupplierListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Supplier>> Get(int id)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Supplier>> Create([FromBody] CreateSupplierDto dto)
    {
        if (await _db.Suppliers.AnyAsync(s => s.Code == dto.Code))
            return Conflict(new { message = $"供应商编码 '{dto.Code}' 已存在" });

        var entity = new Supplier
        {
            Code = dto.Code, Name = dto.Name, Address = dto.Address,
            ContactPerson = dto.ContactPerson, ContactPhone = dto.ContactPhone,
            Email = dto.Email, Grade = dto.Grade, SupplyCategory = dto.SupplyCategory,
        };
        _db.Suppliers.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Supplier>> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.Suppliers.AnyAsync(s => s.Code == dto.Code && s.Id != id))
            return Conflict(new { message = $"供应商编码 '{dto.Code}' 已被其他供应商使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Address = dto.Address;
        entity.ContactPerson = dto.ContactPerson; entity.ContactPhone = dto.ContactPhone;
        entity.Email = dto.Email; entity.Grade = dto.Grade;
        entity.SupplyCategory = dto.SupplyCategory; entity.Score = dto.Score;
        entity.IsActive = dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Suppliers.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Suppliers.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

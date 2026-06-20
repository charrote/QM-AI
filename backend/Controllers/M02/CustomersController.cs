using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<CustomerListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(c => c.Code.Contains(req.Keyword) || c.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(c => new CustomerListDto
            {
                Id = c.Id, Code = c.Code, Name = c.Name,
                ContactPerson = c.ContactPerson, ContactPhone = c.ContactPhone,
                IsActive = c.IsActive,
            })
            .ToListAsync();
        return Ok(new PagedResult<CustomerListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Customer>> Get(int id)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] CreateCustomerDto dto)
    {
        if (await _db.Customers.AnyAsync(c => c.Code == dto.Code))
            return Conflict(new { message = $"客户编码 '{dto.Code}' 已存在" });

        var entity = new Customer
        {
            Code = dto.Code, Name = dto.Name, Address = dto.Address,
            ContactPerson = dto.ContactPerson, ContactPhone = dto.ContactPhone, Email = dto.Email,
        };
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Customer>> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.Customers.AnyAsync(c => c.Code == dto.Code && c.Id != id))
            return Conflict(new { message = $"客户编码 '{dto.Code}' 已被其他客户使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Address = dto.Address;
        entity.ContactPerson = dto.ContactPerson; entity.ContactPhone = dto.ContactPhone;
        entity.Email = dto.Email; entity.IsActive = dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Customers.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

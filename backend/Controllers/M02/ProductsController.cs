using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p => p.Code.Contains(req.Keyword) || p.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Code)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                Unit = p.Unit,
                DefaultInspectionLevel = p.DefaultInspectionLevel,
                DefaultAql = p.DefaultAql,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();

        return Ok(new PagedResult<ProductListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> Get(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProductDetailDto
        {
            Id = p.Id, Code = p.Code, Name = p.Name, Description = p.Description,
            Category = p.Category, Unit = p.Unit, DefaultInspectionLevel = p.DefaultInspectionLevel,
            DefaultAql = p.DefaultAql, IsActive = p.IsActive, CreatedAt = p.CreatedAt,
        });
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] CreateProductDto dto)
    {
        if (await _db.Products.AnyAsync(p => p.Code == dto.Code))
            return Conflict(new { message = $"产品编码 '{dto.Code}' 已存在" });

        var entity = new Product
        {
            Code = dto.Code, Name = dto.Name, Description = dto.Description,
            Unit = dto.Unit, Category = dto.Category,
            DefaultInspectionLevel = dto.DefaultInspectionLevel, DefaultAql = dto.DefaultAql,
        };
        _db.Products.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, MapToDetail(entity));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity == null) return NotFound();

        if (await _db.Products.AnyAsync(p => p.Code == dto.Code && p.Id != id))
            return Conflict(new { message = $"产品编码 '{dto.Code}' 已被其他产品使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Description = dto.Description;
        entity.Unit = dto.Unit; entity.Category = dto.Category;
        entity.DefaultInspectionLevel = dto.DefaultInspectionLevel; entity.DefaultAql = dto.DefaultAql;
        entity.IsActive = dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(MapToDetail(entity));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ProductDetailDto MapToDetail(Product p) => new()
    {
        Id = p.Id, Code = p.Code, Name = p.Name, Description = p.Description,
        Category = p.Category, Unit = p.Unit, DefaultInspectionLevel = p.DefaultInspectionLevel,
        DefaultAql = p.DefaultAql, IsActive = p.IsActive, CreatedAt = p.CreatedAt,
    };
}

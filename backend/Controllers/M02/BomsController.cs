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
                Id = b.Id, ProductId = b.ProductId,
                ProductCode = b.Product!.Code ?? string.Empty,
                ProductName = b.Product!.Name,
                ParentId = b.ParentId,
                MaterialCode = b.MaterialCode, MaterialName = b.MaterialName,
                Quantity = b.Quantity, Unit = b.Unit, Level = b.Level,
                Remark = b.Remark,
            })
            .ToListAsync();
        return Ok(new PagedResult<BomListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    /// <summary>BOM树形结构（按产品分组，展开子件层级）</summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<BomTreeDto>>> Tree()
    {
        var boms = await _db.Boms
            .Include(b => b.Product)
            .ToListAsync();

        // Build tree: root = products, children = BOM items linked by ParentId
        var bomDict = boms.ToDictionary(b => b.Id);
        var productGroups = boms
            .GroupBy(b => b.ProductId)
            .OrderBy(g => g.First().Product!.Name)
            .Select(g => new
            {
                Product = g.First().Product!,
                Items = g.OrderBy(b => b.Level).ThenBy(b => b.Id).ToList(),
            })
            .ToList();

        var tree = productGroups.Select(pg => BuildProductTree(pg.Product!, pg.Items, bomDict)).ToList();
        return Ok(tree);
    }

    private BomTreeDto BuildProductTree(Product product, List<Bom> productBoms, Dictionary<long, Bom> bomDict)
    {
        // Root node: the product itself
        var root = new BomTreeDto
        {
            Id = product.Id,
            ProductId = product.Id,
            ProductCode = product.Code,
            ProductName = product.Name,
            MaterialCode = product.Code,
            MaterialName = product.Name,
            Quantity = 1,
            Unit = product.Unit,
            Level = 0,
            IsProduct = true,
            Children = new List<BomTreeDto>(),
        };

        // Find top-level children (ParentId points to product or is null)
        var topLevel = productBoms
            .Where(b => b.ParentId == product.Id || b.ParentId == null)
            .OrderBy(b => b.Level).ThenBy(b => b.Id)
            .ToList();

        foreach (var item in topLevel)
        {
            root.Children!.Add(BuildBomNode(item, bomDict));
        }

        return root;
    }

    private BomTreeDto BuildBomNode(Bom bom, Dictionary<long, Bom> bomDict)
    {
        var node = new BomTreeDto
        {
            Id = bom.Id,
            ProductId = bom.ProductId,
            ProductCode = bom.Product?.Code ?? string.Empty,
            ProductName = bom.Product?.Name ?? string.Empty,
            ParentId = bom.ParentId,
            MaterialCode = bom.MaterialCode,
            MaterialName = bom.MaterialName,
            Quantity = bom.Quantity,
            Unit = bom.Unit,
            Level = bom.Level,
            Remark = bom.Remark,
            Children = new List<BomTreeDto>(),
        };

        // Find direct children (ParentId == bom.Id) from the full bom list
        var allBoms = bomDict.Values.ToList();
        var children = allBoms
            .Where(b => b.ParentId == bom.Id)
            .OrderBy(b => b.Level).ThenBy(b => b.Id)
            .ToList();

        foreach (var child in children)
        {
            node.Children!.Add(BuildBomNode(child, bomDict));
        }

        return node;
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

        // Validate parent exists in the same product if specified
        if (dto.ParentId.HasValue && dto.ParentId.Value > 0)
        {
            var parent = await _db.Boms.FindAsync(dto.ParentId.Value);
            if (parent == null || parent.ProductId != dto.ProductId)
                return BadRequest(new { message = "父件不存在或不属于该产品" });
        }

        var product = await _db.Products.FirstAsync(p => p.Id == dto.ProductId);
        var entity = new Bom
        {
            ProductId = dto.ProductId,
            ProductCode = product.Code,
            ParentId = dto.ParentId,
            MaterialCode = dto.MaterialCode,
            MaterialName = dto.MaterialName,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            Level = dto.Level,
            Remark = dto.Remark,
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

        // Validate parent exists in the same product if specified
        if (dto.ParentId.HasValue && dto.ParentId.Value > 0)
        {
            if (dto.ParentId.Value == id)
                return BadRequest(new { message = "不能将自身设为主键" });
            var parent = await _db.Boms.FindAsync(dto.ParentId.Value);
            if (parent == null || parent.ProductId != dto.ProductId)
                return BadRequest(new { message = "父件不存在或不属于该产品" });
        }

        entity.ProductId = dto.ProductId;
        entity.ParentId = dto.ParentId;
        entity.MaterialCode = dto.MaterialCode;
        entity.MaterialName = dto.MaterialName;
        entity.Quantity = dto.Quantity;
        entity.Unit = dto.Unit;
        entity.Level = dto.Level;
        entity.Remark = dto.Remark;
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

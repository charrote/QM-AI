using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M15;

[ApiController]
[Route("api/v1/sys-dict")]
[Authorize]
public class SysDictController : ControllerBase
{
    private readonly AppDbContext _db;
    public SysDictController(AppDbContext db) => _db = db;

    /// <summary>获取所有字典类型</summary>
    [HttpGet("types")]
    public async Task<ActionResult<List<SysDictTypeDto>>> GetTypes()
    {
        var types = await _db.SysDictTypes
            .OrderBy(t => t.TypeCode)
            .Select(t => new SysDictTypeDto
            {
                Id = t.Id, TypeCode = t.TypeCode, TypeName = t.TypeName,
                IsSystem = t.IsSystem, Status = t.Status, Remark = t.Remark,
            })
            .ToListAsync();
        return Ok(types);
    }

    /// <summary>获取指定类型的字典项</summary>
    [HttpGet("items/{typeCode}")]
    public async Task<ActionResult<List<SysDictItemDto>>> GetItems(string typeCode)
    {
        var items = await _db.SysDictItems
            .Where(i => i.TypeCode == typeCode && i.Status)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.ItemLabel)
            .Select(i => new SysDictItemDto
            {
                Id = i.Id, TypeCode = i.TypeCode, ItemLabel = i.ItemLabel,
                ItemValue = i.ItemValue, SortOrder = i.SortOrder,
                Color = i.Color, IsDefault = i.IsDefault, Status = i.Status,
            })
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>获取所有字典数据（含选项）</summary>
    [HttpGet("full")]
    public async Task<ActionResult<List<SysDictFullDto>>> GetAllFull()
    {
        var types = await _db.SysDictTypes
            .Include(t => t.Items.Where(i => i.Status))
            .OrderBy(t => t.TypeCode)
            .ToListAsync();

        var result = types.Select(t => new SysDictFullDto
        {
            Type = new SysDictTypeDto
            {
                Id = t.Id, TypeCode = t.TypeCode, TypeName = t.TypeName,
                IsSystem = t.IsSystem, Status = t.Status, Remark = t.Remark,
            },
            Items = t.Items.OrderBy(i => i.SortOrder).Select(i => new SysDictItemDto
            {
                Id = i.Id, TypeCode = i.TypeCode, ItemLabel = i.ItemLabel,
                ItemValue = i.ItemValue, SortOrder = i.SortOrder,
                Color = i.Color, IsDefault = i.IsDefault, Status = i.Status,
            }).ToList(),
        }).ToList();

        return Ok(result);
    }

    /// <summary>批量获取多个字典类型的选项</summary>
    [HttpPost("batch")]
    public async Task<ActionResult<Dictionary<string, List<SysDictItemDto>>>> GetBatch([FromBody] List<string> typeCodes)
    {
        var items = await _db.SysDictItems
            .Where(i => typeCodes.Contains(i.TypeCode) && i.Status)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.ItemLabel)
            .Select(i => new SysDictItemDto
            {
                Id = i.Id, TypeCode = i.TypeCode, ItemLabel = i.ItemLabel,
                ItemValue = i.ItemValue, SortOrder = i.SortOrder,
                Color = i.Color, IsDefault = i.IsDefault, Status = i.Status,
            })
            .ToListAsync();

        var result = items.GroupBy(i => i.TypeCode)
            .ToDictionary(g => g.Key, g => g.ToList());
        return Ok(result);
    }

    /// <summary>创建字典类型</summary>
    [HttpPost("types")]
    public async Task<ActionResult<SysDictTypeDto>> CreateType([FromBody] CreateSysDictTypeDto dto)
    {
        if (await _db.SysDictTypes.AnyAsync(t => t.TypeCode == dto.TypeCode))
            return Conflict(new { message = $"字典类型编码 '{dto.TypeCode}' 已存在" });

        var entity = new SysDictType
        {
            TypeCode = dto.TypeCode,
            TypeName = dto.TypeName,
            Remark = dto.Remark,
            IsSystem = false,
            Status = true,
        };
        _db.SysDictTypes.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTypes), new { }, new SysDictTypeDto
        {
            Id = entity.Id, TypeCode = entity.TypeCode, TypeName = entity.TypeName,
            IsSystem = entity.IsSystem, Status = entity.Status, Remark = entity.Remark,
        });
    }

    /// <summary>更新字典类型</summary>
    [HttpPut("types/{id:long}")]
    public async Task<ActionResult<SysDictTypeDto>> UpdateType(long id, [FromBody] UpdateSysDictTypeDto dto)
    {
        var entity = await _db.SysDictTypes.FindAsync(id);
        if (entity == null) return NotFound();
        entity.TypeName = dto.TypeName ?? entity.TypeName;
        entity.Remark = dto.Remark ?? entity.Remark;
        entity.Status = dto.Status ?? entity.Status;
        await _db.SaveChangesAsync();
        return Ok(new SysDictTypeDto
        {
            Id = entity.Id, TypeCode = entity.TypeCode, TypeName = entity.TypeName,
            IsSystem = entity.IsSystem, Status = entity.Status, Remark = entity.Remark,
        });
    }

    /// <summary>删除字典类型</summary>
    [HttpDelete("types/{id:long}")]
    public async Task<IActionResult> DeleteType(long id)
    {
        var entity = await _db.SysDictTypes.FindAsync(id);
        if (entity == null) return NotFound();
        if (entity.IsSystem) return BadRequest(new { message = "系统内置字典类型不可删除" });
        _db.SysDictTypes.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>创建字典项</summary>
    [HttpPost("items")]
    public async Task<ActionResult<SysDictItemDto>> CreateItem([FromBody] CreateSysDictItemDto dto)
    {
        var entity = new SysDictItem
        {
            TypeCode = dto.TypeCode,
            ItemLabel = dto.ItemLabel,
            ItemValue = dto.ItemValue,
            SortOrder = dto.SortOrder,
            Color = dto.Color,
            Status = true,
        };
        _db.SysDictItems.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItems), new { typeCode = entity.TypeCode }, new SysDictItemDto
        {
            Id = entity.Id, TypeCode = entity.TypeCode, ItemLabel = entity.ItemLabel,
            ItemValue = entity.ItemValue, SortOrder = entity.SortOrder,
            Color = entity.Color, IsDefault = entity.IsDefault, Status = entity.Status,
        });
    }

    /// <summary>更新字典项</summary>
    [HttpPut("items/{id:long}")]
    public async Task<ActionResult<SysDictItemDto>> UpdateItem(long id, [FromBody] UpdateSysDictItemDto dto)
    {
        var entity = await _db.SysDictItems.FindAsync(id);
        if (entity == null) return NotFound();
        entity.ItemLabel = dto.ItemLabel ?? entity.ItemLabel;
        entity.ItemValue = dto.ItemValue ?? entity.ItemValue;
        entity.SortOrder = dto.SortOrder ?? entity.SortOrder;
        entity.Color = dto.Color ?? entity.Color;
        entity.Status = dto.Status ?? entity.Status;
        await _db.SaveChangesAsync();
        return Ok(new SysDictItemDto
        {
            Id = entity.Id, TypeCode = entity.TypeCode, ItemLabel = entity.ItemLabel,
            ItemValue = entity.ItemValue, SortOrder = entity.SortOrder,
            Color = entity.Color, IsDefault = entity.IsDefault, Status = entity.Status,
        });
    }

    /// <summary>删除字典项</summary>
    [HttpDelete("items/{id:long}")]
    public async Task<IActionResult> DeleteItem(long id)
    {
        var entity = await _db.SysDictItems.FindAsync(id);
        if (entity == null) return NotFound();
        _db.SysDictItems.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ─── DTOs ─────────────────────────────────────────
    public class CreateSysDictTypeDto
    {
        public string TypeCode { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }

    public class UpdateSysDictTypeDto
    {
        public string? TypeName { get; set; }
        public string? Remark { get; set; }
        public bool? Status { get; set; }
    }

    public class CreateSysDictItemDto
    {
        public string TypeCode { get; set; } = string.Empty;
        public string ItemLabel { get; set; } = string.Empty;
        public string ItemValue { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string? Color { get; set; }
    }

    public class UpdateSysDictItemDto
    {
        public string? ItemLabel { get; set; }
        public string? ItemValue { get; set; }
        public int? SortOrder { get; set; }
        public string? Color { get; set; }
        public bool? Status { get; set; }
    }
}

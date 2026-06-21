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
}

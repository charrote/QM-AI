using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/equipment")]
[Authorize]
public class EquipmentController : ControllerBase
{
    private readonly AppDbContext _db;
    public EquipmentController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<EquipmentListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Equipment.AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(e => e.Code.Contains(req.Keyword) || e.Name.Contains(req.Keyword));
        if (req.OrgId.HasValue)
            query = query.Where(e => e.OrgId == req.OrgId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Code)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(e => new EquipmentListDto
            {
                Id = e.Id, Code = e.Code, Name = e.Name, Model = e.Model,
                ProductionLine = e.ProductionLine, Workshop = e.Workshop,
                Status = e.Status, EquipmentType = e.EquipmentType,
                HasMqttConnection = e.HasMqttConnection, IsActive = e.IsActive,
                OrgId = e.OrgId, WorkshopId = e.WorkshopId, LineId = e.LineId,
            })
            .ToListAsync();

        // 丰富组织名称
        var orgIds = items.Where(i => i.WorkshopId.HasValue || i.LineId.HasValue)
            .SelectMany(i => new[] { i.WorkshopId, i.LineId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var orgMap = orgIds.Any()
            ? await _db.Organizations.Where(o => orgIds.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.Name)
            : new Dictionary<int, string>();

        foreach (var item in items)
        {
            if (item.WorkshopId.HasValue && orgMap.TryGetValue(item.WorkshopId.Value, out var wn))
                item.WorkshopName = wn;
            if (item.LineId.HasValue && orgMap.TryGetValue(item.LineId.Value, out var ln))
                item.LineName = ln;
        }

        return Ok(new PagedResult<EquipmentListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Equipment>> Get(int id)
    {
        var entity = await _db.Equipment.FindAsync(id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Equipment>> Create([FromBody] CreateEquipmentDto dto)
    {
        if (await _db.Equipment.AnyAsync(e => e.Code == dto.Code))
            return Conflict(new { message = $"设备编码 '{dto.Code}' 已存在" });

        var entity = new Equipment
        {
            Code = dto.Code, Name = dto.Name, Model = dto.Model,
            ProductionLine = dto.ProductionLine, Workshop = dto.Workshop,
            EquipmentType = dto.EquipmentType, HasMqttConnection = dto.HasMqttConnection,
            MqttTopicPrefix = dto.MqttTopicPrefix,
            OrgId = dto.OrgId, WorkshopId = dto.WorkshopId, LineId = dto.LineId,
        };
        _db.Equipment.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Equipment>> Update(int id, [FromBody] UpdateEquipmentDto dto)
    {
        var entity = await _db.Equipment.FindAsync(id);
        if (entity == null) return NotFound();
        if (await _db.Equipment.AnyAsync(e => e.Code == dto.Code && e.Id != id))
            return Conflict(new { message = $"设备编码 '{dto.Code}' 已被其他设备使用" });

        entity.Code = dto.Code; entity.Name = dto.Name; entity.Model = dto.Model;
        entity.ProductionLine = dto.ProductionLine; entity.Workshop = dto.Workshop;
        entity.Status = dto.Status; entity.EquipmentType = dto.EquipmentType;
        entity.HasMqttConnection = dto.HasMqttConnection; entity.MqttTopicPrefix = dto.MqttTopicPrefix;
        entity.IsActive = dto.IsActive; entity.UpdatedAt = DateTime.UtcNow;
        entity.OrgId = dto.OrgId; entity.WorkshopId = dto.WorkshopId; entity.LineId = dto.LineId;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Equipment.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Equipment.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

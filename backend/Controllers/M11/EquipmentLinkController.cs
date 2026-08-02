using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models.M11;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.M11;

[ApiController]
[Route("api/v1/m11/equipment-link")]
public class EquipmentLinkController : ControllerBase
{
    private readonly EquipmentLinkService _service;

    public EquipmentLinkController(EquipmentLinkService service) => _service = service;

    [HttpGet("equipment/{equipmentId}")]
    public async Task<IActionResult> GetMappings(long equipmentId)
    {
        try
        {
            return Ok(await _service.GetMappingsByEquipmentIdAsync(equipmentId));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询映射失败", detail = ex.Message });
        }
    }

    [HttpGet("map")]
    public async Task<IActionResult> ListMaps([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] long? equipmentId = null, [FromQuery] string? keyword = null)
    {
        try
        {
            var query = _service.GetAllMapsQueryable();

            if (equipmentId.HasValue)
                query = query.Where(m => m.EquipmentId == equipmentId.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(m =>
                    m.MqttTopic.Contains(keyword) ||
                    m.SystemParamCode.Contains(keyword));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { items, total, page, pageSize });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询映射列表失败", detail = ex.Message });
        }
    }

    [HttpGet("map/{id}")]
    public async Task<IActionResult> GetMap(long id)
    {
        try
        {
            var map = await _service.GetMapByIdAsync(id);
            if (map == null) return NotFound();
            return Ok(map);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询映射失败", detail = ex.Message });
        }
    }

    [HttpPost("map")]
    public async Task<IActionResult> CreateMap([FromBody] EquipmentParamMapping map)
    {
        try
        {
            return CreatedAtAction(nameof(GetMappings), new { equipmentId = map.EquipmentId }, await _service.CreateMapAsync(map));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建映射失败", detail = ex.Message });
        }
    }

    [HttpPut("map/{id}")]
    public async Task<IActionResult> UpdateMap(long id, [FromBody] EquipmentParamMapping map)
    {
        try
        {
            return Ok(await _service.UpdateMapAsync(id, map));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新映射失败", detail = ex.Message });
        }
    }

    [HttpDelete("map/{id}")]
    public async Task<IActionResult> DeleteMap(long id)
    {
        try
        {
            var result = await _service.DeleteMapAsync(id);
            return result ? Ok(new { success = true }) : NotFound(new { message = "映射不存在" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除映射失败", detail = ex.Message });
        }
    }

    [HttpPost("status")]
    public async Task<IActionResult> RecordStatus([FromBody] EquipmentStatusRecordInput input)
    {
        try
        {
            var mapping = await _service.GetMappingAsync(input.MappingId, "");
            var history = new EquipmentStatusHistory
            {
                EquipmentId = mapping?.EquipmentId ?? input.MappingId,
                Signal = "manual_record",
                SignalData = "{\"value\":" + input.Value + "}"
            };
            await _service.RecordStatusAsync(history);
            return Accepted();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "记录状态失败", detail = ex.Message });
        }
    }

    public sealed class EquipmentStatusRecordInput { public long MappingId { get; set; } public decimal Value { get; set; } }

    [HttpGet("recent-status/{equipmentId}")]
    public async Task<IActionResult> RecentStatus(long equipmentId, [FromQuery] int limit = 50)
    {
        try
        {
            return Ok(await _service.GetRecentStatusAsync(equipmentId, limit));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询历史状态失败", detail = ex.Message });
        }
    }

    [HttpGet("correlations")]
    public async Task<IActionResult> ListCorrelations([FromQuery] long? equipmentId = null,
        [FromQuery] string? dateFrom = null, [FromQuery] string? dateTo = null)
    {
        try
        {
            DateOnly? from = null;
            DateOnly? to = null;
            if (!string.IsNullOrWhiteSpace(dateFrom) && DateOnly.TryParse(dateFrom, out var f)) from = f;
            if (!string.IsNullOrWhiteSpace(dateTo) && DateOnly.TryParse(dateTo, out var t)) to = t;

            var items = await _service.GetCorrelationsAsync(equipmentId, from, to);
            return Ok(new { items, total = items.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "查询关联数据失败", detail = ex.Message });
        }
    }

    [HttpGet("drift")]
    public async Task<IActionResult> DetectDrift([FromQuery] long equipmentId,
        [FromQuery] string? sysParamCode = null, [FromQuery] decimal currentValue = 0)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sysParamCode))
                return BadRequest(new { message = "请提供系统参数代码" });

            return Ok(await _service.DetectDriftAsync(equipmentId, sysParamCode, currentValue));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "漂移检测失败", detail = ex.Message });
        }
    }
}

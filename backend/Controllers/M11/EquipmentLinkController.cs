using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> GetMappings(long equipmentId) => Ok(await _service.GetMappingsByEquipmentIdAsync(equipmentId));

    [HttpGet("map")]
    public async Task<IActionResult> ListMaps() => Ok(await _service.GetAllMapsAsync());

    [HttpGet("map/{id}")]
    public async Task<IActionResult> GetMap(long id)
    {
        var map = await _service.GetMapByIdAsync(id);
        if (map == null) return NotFound();
        return Ok(map);
    }

    [HttpPost("map")]
    public async Task<IActionResult> CreateMap([FromBody] EquipmentParamMapping map) => CreatedAtAction(nameof(GetMappings), new { equipmentId = map.EquipmentId }, await _service.CreateMapAsync(map));

    [HttpPut("map/{id}")]
    public async Task<IActionResult> UpdateMap(long id, [FromBody] EquipmentParamMapping map) => Ok(await _service.UpdateMapAsync(id, map));

    [HttpDelete("map/{id}")]
    public async Task<IActionResult> DeleteMap(long id) => Ok(await _service.DeleteMapAsync(id));

    [HttpPost("status")]
    public async Task<IActionResult> RecordStatus([FromBody] EquipmentStatusRecordInput input)
    {
        var mapping = await _service.GetMappingAsync(input.MappingId, "");
        var history = new EquipmentStatusHistory
        {
            EquipmentId = mapping?.EquipmentId ?? (int)input.MappingId,
            Signal = "manual_record",
            SignalData = $"{{\"value\":{input.Value}}}"
        };
        await _service.RecordStatusAsync(history);
        return Accepted();
    }

    public sealed class EquipmentStatusRecordInput { public long MappingId { get; set; } public decimal Value { get; set; } }

    [HttpGet("recent-status/{mappingId}")]
    public async Task<IActionResult> RecentStatus(long mappingId, [FromQuery] int limit = 50) => Ok(await _service.GetRecentStatusAsync(mappingId, limit));

    [HttpGet("drift")]
    public async Task<IActionResult> DetectDrift([FromQuery] long equipmentId, [FromQuery] string sysParamCode, [FromQuery] decimal currentValue) => Ok(await _service.DetectDriftAsync(equipmentId, sysParamCode, currentValue));
}

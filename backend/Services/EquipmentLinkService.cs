using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models;
using QM_AI.API.Models.M11;

namespace QM_AI.API.Services;

public class EquipmentLinkService
{
    private readonly AppDbContext _db;

    public EquipmentLinkService(AppDbContext db) => _db = db;

    public async Task<List<EquipmentParamMapping>> GetMappingsByEquipmentIdAsync(long equipmentId) =>
        await _db.EquipmentParamMappings.Where(m => m.EquipmentId == equipmentId).ToListAsync();

    public async Task<List<EquipmentParamMapping>> GetAllMapsAsync() =>
        await _db.EquipmentParamMappings.ToListAsync();

    public IQueryable<EquipmentParamMapping> GetAllMapsQueryable() =>
        _db.EquipmentParamMappings.AsQueryable();

    public async Task<EquipmentParamMapping?> GetMapByIdAsync(long id) =>
        await _db.EquipmentParamMappings.FindAsync(id);

    public async Task<EquipmentParamMapping?> GetMappingAsync(long equipmentId, string sysParamCode) =>
        await _db.EquipmentParamMappings.FirstOrDefaultAsync(m => m.EquipmentId == equipmentId && m.SystemParamCode == sysParamCode);

    public async Task<EquipmentParamMapping> CreateMapAsync(EquipmentParamMapping mapping)
    {
        mapping.CreatedAt = DateTime.Now;
        _db.EquipmentParamMappings.Add(mapping);
        await _db.SaveChangesAsync();
        return mapping;
    }

    public async Task<EquipmentParamMapping?> UpdateMapAsync(long id, EquipmentParamMapping updated)
    {
        var m = await _db.EquipmentParamMappings.FindAsync(id);
        if (m == null) return null;
        m.MqttTopic = updated.MqttTopic;
        m.SystemParamCode = updated.SystemParamCode;
        m.ParamGroupId = updated.ParamGroupId;
        m.DataType = updated.DataType;
        m.Unit = updated.Unit;
        m.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return m;
    }

    public async Task<bool> DeleteMapAsync(long id)
    {
        var m = await _db.EquipmentParamMappings.FindAsync(id);
        if (m == null) return false;
        _db.EquipmentParamMappings.Remove(m);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RecordStatusAsync(EquipmentStatusHistory history)
    {
        history.RecordedAt = DateTime.SpecifyKind(history.RecordedAt, DateTimeKind.Utc);
        _db.EquipmentStatusHistories.Add(history);
        await _db.SaveChangesAsync();
    }

    public async Task<List<EquipmentStatusHistory>> GetRecentStatusAsync(long equipmentId, int limit = 50) =>
        await _db.EquipmentStatusHistories.Where(h => h.EquipmentId == equipmentId)
            .OrderByDescending(h => h.RecordedAt).Take(limit).ToListAsync();

    public async Task<List<EquipmentQualityCorrelation>> GetCorrelationsAsync(long? equipmentId, DateOnly? dateFrom, DateOnly? dateTo)
    {
        var query = _db.EquipmentQualityCorrelations.AsQueryable();
        if (equipmentId.HasValue) query = query.Where(q => q.EquipmentId == equipmentId.Value);
        if (dateFrom.HasValue) query = query.Where(q => q.AnalysisDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(q => q.AnalysisDate <= dateTo.Value);
        return await query.OrderByDescending(q => q.AnalysisDate).ToListAsync();
    }

    public async Task<bool> DetectDriftAsync(long equipmentId, string sysParamCode, decimal currentValue)
    {
        var mapping = await GetMappingAsync(equipmentId, sysParamCode);
        if (mapping == null) return false;
        // TODO: implement drift detection with configurable thresholds stored externally or in param group specs
        return false; // placeholder
    }

    public async Task<EquipmentQualityCorrelation> CreateCorrelationAsync(EquipmentQualityCorrelation correlation)
    {
        correlation.AnalysisDate = DateOnly.FromDateTime(DateTime.Now); // sync today's date
        _db.EquipmentQualityCorrelations.Add(correlation);
        await _db.SaveChangesAsync();
        return correlation;
    }

    public async Task<List<Equipment>> GetLinkedEquipmentsAsync() => await _db.Equipment.ToListAsync();
}

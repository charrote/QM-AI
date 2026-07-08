using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M07;

namespace QM_AI.API.Services;

/// <summary>
/// 缺陷记录服务
/// </summary>
public class DefectService
{
    private readonly AppDbContext _db;

    public DefectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Defect>> GetAllAsync(string? sourceType, string? severity, string? status, int page = 1, int pageSize = 20)
    {
        var query = _db.Defects.AsQueryable();

        if (!string.IsNullOrEmpty(sourceType))
            query = query.Where(d => d.SourceType == sourceType);
        if (!string.IsNullOrEmpty(severity))
            query = query.Where(d => d.Severity == severity);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(d => d.Status == status);

        return await query
            .OrderByDescending(d => d.DiscoveredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Defect?> GetByIdAsync(long id)
    {
        return await _db.Defects
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Defect> CreateAsync(Defect defect)
    {
        defect.CreatedAt = DateTime.UtcNow;
        defect.UpdatedAt = DateTime.UtcNow;
        _db.Defects.Add(defect);
        await _db.SaveChangesAsync();
        return defect;
    }

    public async Task<Defect?> UpdateAsync(long id, Defect updated)
    {
        var defect = await _db.Defects.FindAsync(id);
        if (defect == null) return null;

        defect.DefectCode = updated.DefectCode;
        defect.Severity = updated.Severity;
        defect.SourceType = updated.SourceType;
        defect.ProductId = updated.ProductId;
        defect.BatchId = updated.BatchId;
        defect.EquipmentId = updated.EquipmentId;
        defect.Quantity = updated.Quantity;
        defect.Description = updated.Description;
        defect.ImageUrls = updated.ImageUrls;
        defect.Status = updated.Status;
        defect.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return defect;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var defect = await _db.Defects.FindAsync(id);
        if (defect == null) return false;
        _db.Defects.Remove(defect);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountAsync(string? sourceType = null)
    {
        var query = _db.Defects.AsQueryable();
        if (!string.IsNullOrEmpty(sourceType))
            query = query.Where(d => d.SourceType == sourceType);
        return await query.CountAsync();
    }
}
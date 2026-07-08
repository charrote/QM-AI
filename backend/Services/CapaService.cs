using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M07;

namespace QM_AI.API.Services;

/// <summary>
/// CAPA 服务（纠正与预防措施流程引擎）
/// </summary>
public class CapaService
{
    private readonly AppDbContext _db;

    public CapaService(AppDbContext db)
    {
        _db = db;
    }

    // ─── CAPA 单 CRUD ───────────────────────────────────────

    public async Task<(List<Capa> data, int total)> GetAllAsync(string? status, int? phase, int page = 1, int pageSize = 20)
    {
        var query = _db.Capas.AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);
        if (phase.HasValue)
            query = query.Where(c => c.CurrentPhase == phase.Value);

        int total = await query.CountAsync();

        var data = await query
            .Include(c => c.Defect)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, total);
    }

    public async Task<Capa?> GetByIdAsync(long id)
    {
        return await _db.Capas
            .Include(c => c.Defect)
            .Include(c => c.TemporaryMeasures)
            .Include(c => c.RootCauses)
            .Include(c => c.CorrectiveActions)
            .Include(c => c.PreventiveActions)
            .Include(c => c.Verifications)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Capa?> UpdateAsync(Capa capa)
    {
        var existing = await _db.Capas.FindAsync(capa.Id);
        if (existing == null) return null;

        existing.Title = capa.Title;
        existing.Description = capa.Description;
        existing.Severity = capa.Severity;
        existing.AssignedTo = capa.AssignedTo;
        existing.DueDate = capa.DueDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<Capa> CreateAsync(Capa capa)
    {
        // 自动生成 CAPA 单号
        if (string.IsNullOrEmpty(capa.CapaCode))
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _db.Capas.CountAsync(c => c.CapaCode!.Contains(datePart)) + 1;
            capa.CapaCode = $"CAPA-{datePart}-{count:D3}";
        }
        capa.CurrentPhase = 0;
        capa.Status = "open";
        capa.CreatedAt = DateTime.UtcNow;
        capa.UpdatedAt = DateTime.UtcNow;
        _db.Capas.Add(capa);
        await _db.SaveChangesAsync();
        return capa;
    }

    public async Task<(Capa? capa, string? error)> UpdatePhaseAsync(long id, int newPhase)
    {
        var capa = await _db.Capas.FindAsync(id);
        if (capa == null) return (null, "CAPA 单未找到");

        if (newPhase < 0 || newPhase > 6) return (null, "无效的阶段值（0-6）");

        // 阶段顺序校验：必须按顺序推进，不允许跳阶段
        // 0=创建 → 1=临时措施 → 2=根因分析 → 3=纠正措施 → 4=预防措施 → 5=验证 → 6=关闭
        // 特殊规则：验证失败时可退回纠正(3)或预防(4)阶段
        if (newPhase > capa.CurrentPhase + 1)
        {
            return (null, $"不允许跳阶段：当前阶段 {capa.CurrentPhase}，只能推进到 {capa.CurrentPhase + 1}");
        }

        // 允许退回的情况：从验证(5)退回纠正(3)或预防(4)
        if (capa.CurrentPhase == 5 && (newPhase == 3 || newPhase == 4))
        {
            // 允许验证失败退回
        }
        else if (newPhase < capa.CurrentPhase && !(capa.CurrentPhase == 5 && (newPhase == 3 || newPhase == 4)))
        {
            return (null, $"不允许回退阶段：从 {capa.CurrentPhase} 回退到 {newPhase}（仅验证阶段可退回纠正/预防）");
        }

        capa.CurrentPhase = newPhase;

        // 更新状态
        capa.Status = newPhase switch
        {
            0 => "open",
            1 => "in_progress",
            2 => "in_progress",
            3 => "in_progress",
            4 => "in_progress",
            5 => "in_progress",
            6 => "closed",
            _ => capa.Status
        };

        if (newPhase == 6)
        {
            capa.ClosedAt = DateTime.UtcNow;
        }

        capa.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (capa, null);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var capa = await _db.Capas.FindAsync(id);
        if (capa == null) return false;
        _db.Capas.Remove(capa);
        await _db.SaveChangesAsync();
        return true;
    }

    // ─── 临时措施 ────────────────────────────────────────────

    public async Task<CapaTemporaryMeasure> AddTemporaryMeasureAsync(CapaTemporaryMeasure measure)
    {
        measure.CreatedAt = DateTime.UtcNow;
        _db.CapaTemporaryMeasures.Add(measure);
        await _db.SaveChangesAsync();
        return measure;
    }

    // ─── 原因分析 ────────────────────────────────────────────

    public async Task<CapaRootCause> AddRootCauseAsync(CapaRootCause cause)
    {
        cause.CreatedAt = DateTime.UtcNow;
        _db.CapaRootCauses.Add(cause);
        await _db.SaveChangesAsync();
        return cause;
    }

    public async Task<List<CapaRootCause>> GetRootCausesAsync(long capaId)
    {
        return await _db.CapaRootCauses
            .Where(r => r.CapaId == capaId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // ─── 纠正措施 ────────────────────────────────────────────

    public async Task<CapaCorrectiveAction> AddCorrectiveActionAsync(CapaCorrectiveAction action)
    {
        action.CreatedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;
        _db.CapaCorrectiveActions.Add(action);
        await _db.SaveChangesAsync();
        return action;
    }

    public async Task<CapaCorrectiveAction?> UpdateCorrectiveActionStatusAsync(long id, string status)
    {
        var action = await _db.CapaCorrectiveActions.FindAsync(id);
        if (action == null) return null;
        action.Status = status;
        if (status == "completed") action.CompletedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return action;
    }

    // ─── 预防措施 ────────────────────────────────────────────

    public async Task<CapaPreventiveAction> AddPreventiveActionAsync(CapaPreventiveAction action)
    {
        action.CreatedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;
        _db.CapaPreventiveActions.Add(action);
        await _db.SaveChangesAsync();
        return action;
    }

    public async Task<CapaPreventiveAction?> UpdatePreventiveActionStatusAsync(long id, string status)
    {
        var action = await _db.CapaPreventiveActions.FindAsync(id);
        if (action == null) return null;
        action.Status = status;
        if (status == "completed") action.CompletedAt = DateTime.UtcNow;
        action.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return action;
    }

    // ─── 验证 ────────────────────────────────────────────────

    public async Task<CapaVerification> AddVerificationAsync(CapaVerification verification)
    {
        verification.CreatedAt = DateTime.UtcNow;
        _db.CapaVerifications.Add(verification);
        await _db.SaveChangesAsync();
        return verification;
    }

    // ─── 报废/返工 ────────────────────────────────────────────

    public async Task<ScrapReworkRecord> CreateScrapReworkAsync(ScrapReworkRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        _db.ScrapReworkRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task<List<ScrapReworkRecord>> GetScrapReworkRecordsAsync(long? defectId = null)
    {
        var query = _db.ScrapReworkRecords.AsQueryable();
        if (defectId.HasValue)
            query = query.Where(r => r.DefectId == defectId);
        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<ScrapReworkRecord?> UpdateReworkInspectionResultAsync(long id, string result)
    {
        var record = await _db.ScrapReworkRecords.FindAsync(id);
        if (record == null) return null;
        record.ReworkInspectionResult = result;
        await _db.SaveChangesAsync();
        return record;
    }
}
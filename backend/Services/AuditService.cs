using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M13;

namespace QM_AI.API.Services;

public class AuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db) => _db = db;

    public async Task<(List<Audit> Items, int Total)> GetAllAsync(string? auditType, string? status, int page = 1, int pageSize = 20)
    {
        var query = _db.Audits.AsQueryable();
        if (!string.IsNullOrEmpty(auditType)) query = query.Where(a => a.AuditType == auditType);
        if (!string.IsNullOrEmpty(status)) query = query.Where(a => a.Status == status);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.StartDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Audit?> GetByIdAsync(long id) =>
        await _db.Audits.Include(a => a.Findings).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<Audit> CreateAsync(Audit audit)
    {
        audit.AuditCode = $"AUD-{DateTime.Now:yyyyMMdd}-{_db.Audits.Count():D3}";
        audit.Status = "planned";
        audit.CreatedAt = DateTime.Now;
        _db.Audits.Add(audit);
        await _db.SaveChangesAsync();
        return audit;
    }

    public async Task<Audit?> UpdateAsync(long id, Audit updated)
    {
        var a = await _db.Audits.FindAsync(id);
        if (a == null) return null;
        a.Title = updated.Title;
        a.Description = updated.Description;
        a.AuditType = updated.AuditType;
        a.StartDate = updated.StartDate;
        a.EndDate = updated.EndDate;
        a.AuditorIdsJson = updated.AuditorIdsJson ?? a.AuditorIdsJson;
        a.Scope = updated.Scope ?? a.Scope;
        a.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return a;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var a = await _db.Audits.FindAsync(id);
        if (a == null) return false;
        _db.Audits.Remove(a);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<AuditFinding> AddFindingAsync(long auditId, AuditFinding finding) => await AddFindingInternal(auditId, finding, "non_verified");

    public async Task<AuditFinding> AddFindingAsync(long auditId, AuditFinding finding, string initialStatus) => await AddFindingInternal(auditId, finding, initialStatus);

    private async Task<AuditFinding> AddFindingInternal(long auditId, AuditFinding finding, string initialStatus)
    {
        var audit = await _db.Audits.FindAsync(auditId);
        if (audit == null) throw new InvalidOperationException($"Audit {auditId} not found");

        finding.AuditId = auditId;
        finding.Status = initialStatus;
        finding.CreatedAt = DateTime.Now;

        _db.AuditFindings.Add(finding);
        await _db.SaveChangesAsync();

        // Refresh audit stats
        var findings = await _db.AuditFindings.Where(f => f.AuditId == auditId).ToListAsync();
        audit.TotalFindings = findings.Count;
        audit.Conformities = findings.Count(f => f.Classification == "conformity");
        audit.NonConformities = findings.Count(f => f.Classification == "non_conformity");
        audit.Opportunities = findings.Count(f => f.Classification == "opportunity");
        await _db.SaveChangesAsync();

        return finding;
    }

    public async Task<AuditFinding?> UpdateFindingRectificationAsync(long findingId, AuditFinding updated)
    {
        var f = await _db.AuditFindings.FindAsync(findingId);
        if (f == null) return null;

        if (!string.IsNullOrWhiteSpace(updated.RectificationPlan))
            f.RectificationPlan = updated.RectificationPlan;

        if (!string.IsNullOrWhiteSpace(updated.ResponsibleUserIdStr))
            f.ResponsibleUserIdStr = updated.ResponsibleUserIdStr;
        if (long.TryParse(updated.ResponsibleUserIdStr ?? "", out var rid) && f.ResponsibleUserId != rid)
            f.ResponsibleUserId = rid;

        if (updated.PlannedCompletionDate.HasValue && f.PlannedCompletionDate != updated.PlannedCompletionDate)
            f.PlannedCompletionDate = updated.PlannedCompletionDate.Value;

        if (!string.IsNullOrWhiteSpace(updated.ActualEvidence) && string.IsNullOrWhiteSpace(f.ActualEvidence))
            f.ActualEvidence = updated.ActualEvidence;

        if (!string.IsNullOrWhiteSpace(updated.VerifiedByStr) && string.IsNullOrWhiteSpace(f.VerifiedByStr))
        {
            f.VerifiedByStr = updated.VerifiedByStr;
            if (long.TryParse(updated.VerifiedByStr ?? "", out var vid)) f.VerifiedBy = vid;
            if (updated.VerifiedAt.HasValue) f.VerifiedAt = updated.VerifiedAt.Value;
        }

        f.UpdatedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(updated.Status)) f.Status = updated.Status;

        await _db.SaveChangesAsync();
        return f;
    }

    public async Task<AuditFinding?> VerifyFindingAsync(long findingId, string verifierId, bool passed) => await VerifyFindingInternal(findingId, verifierId, passed ? "verified" : "rejected");

    private async Task<AuditFinding?> VerifyFindingInternal(long findingId, string verifierId, string newStatus)
    {
        var f = await _db.AuditFindings.FindAsync(findingId);
        if (f == null) return null;

        if (f.Status != "rectifying") throw new InvalidOperationException($"Cannot verify finding in status '{f.Status}'");

        f.Status = newStatus;
        f.VerifiedByStr = verifierId;
        if (long.TryParse(verifierId, out var vid)) f.VerifiedBy = vid;
        f.VerifiedAt = DateTime.Now;
        f.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        return f;
    }

    public async Task<List<AuditFinding>> GetFindingsAsync(long auditId) => await _db.AuditFindings.Where(f => f.AuditId == auditId).OrderByDescending(f => f.CreatedAt).ToListAsync();

    public async Task<int> GetCountAsync() => await _db.Audits.CountAsync();
}

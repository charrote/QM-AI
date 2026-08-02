using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M09;

namespace QM_AI.API.Services;

public class D8ReportService
{
    private readonly AppDbContext _db;

    public D8ReportService(AppDbContext db) => _db = db;

    public async Task<D8Report?> GetByComplaintIdAsync(long complaintId) =>
        await _db.D8Reports.FirstOrDefaultAsync(d => d.ComplaintId == complaintId);

    public async Task<List<D8Report>> GetListAsync() =>
        await _db.D8Reports.ToListAsync();

    public async Task<D8Report?> GetByIdAsync(long id) =>
        await _db.D8Reports.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<D8Report> CreateOrUpdateAsync(long complaintId, D8Report report)
    {
        var existing = await GetByComplaintIdAsync(complaintId);
        if (existing != null)
        {
            existing.CurrentDiscipline = report.CurrentDiscipline;
            existing.D1Team = report.D1Team;
            existing.D2ProblemDesc = report.D2ProblemDesc;
            existing.D3Measures = report.D3Measures;
            existing.D4RootCause = report.D4RootCause;
            existing.D4Content = report.D4Content;
            existing.D5Actions = report.D5Actions;
            existing.D6Verification = report.D6Verification;
            existing.D7Preventive = report.D7Preventive;
            existing.CompletedAt = report.CompletedAt;
            await _db.SaveChangesAsync();
            return existing;
        }
        report.ComplaintId = complaintId;
        report.CreatedAt = DateTime.Now;
        _db.D8Reports.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<D8Report?> AdvanceStepAsync(long reportId, int stepDelta)
    {
        var r = await GetByIdAsync(reportId);
        if (r == null) return null;
        r.CurrentDiscipline += stepDelta;
        if (r.CurrentDiscipline >= 8) r.CompletedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return r;
    }

    public static string[] DisciplineLabels => ["D0-Team Setup", "D1-Team", "D2-Problem Desc", "D3-Interim Containment", "D4-Root Cause", "D5-Corrective Actions", "D6-Verify Effectiveness", "D7-Prevent Recurrence", "D8-Recognize Team"];
}

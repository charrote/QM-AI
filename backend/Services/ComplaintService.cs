using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M09;

namespace QM_AI.API.Services;

public class ComplaintService
{
    private readonly AppDbContext _db;

    public ComplaintService(AppDbContext db) => _db = db;

    public async Task<(List<Complaint> Items, int Total)> GetAllAsync(
        string? customerId, string? severity, string? status, int page = 1, int pageSize = 20)
    {
        var query = _db.Complaints.Include(c => c.Customer).AsQueryable();
        if (!string.IsNullOrEmpty(customerId) && int.TryParse(customerId, out var cid)) query = query.Where(c => c.CustomerId == cid);
        if (!string.IsNullOrEmpty(severity)) query = query.Where(c => c.Severity == severity);
        if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Complaint?> GetByIdAsync(long id) =>
        await _db.Complaints.Include(c => c.Customer).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Complaint> CreateAsync(Complaint complaint)
    {
        complaint.ComplaintCode = $"C-{DateTime.Now:yyyyMMdd}-{_db.Complaints.Count():D3}";
        complaint.Status = "new";
        complaint.CreatedAt = DateTime.Now;
        complaint.UpdatedAt = DateTime.Now;
        _db.Complaints.Add(complaint);
        await LogEventAsync(complaint.Id, "created", null);
        await _db.SaveChangesAsync();
        return complaint;
    }

    public async Task<Complaint?> UpdateAsync(long id, Complaint updated)
    {
        var c = await _db.Complaints.FindAsync(id);
        if (c == null) return null;
        var oldStatus = c.Status;
        c.Subject = updated.Subject;
        c.Description = updated.Description;
        c.AssignedTo = updated.AssignedTo;
        c.DueDate = updated.DueDate;
        c.FiveW2HJson = updated.FiveW2HJson ?? c.FiveW2HJson;
        if (oldStatus != updated.Status)
            await LogEventAsync(id, "status_change", $@"{{""from"":""{oldStatus}"",""to"":""{updated.Status}""}}");
        c.Status = updated.Status;
        c.UpdatedAt = DateTime.Now;
        if (updated.ClosedAt.HasValue && !c.ClosedAt.HasValue) c.ClosedAt = updated.ClosedAt.Value;
        await _db.SaveChangesAsync();
        return c;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var c = await _db.Complaints.FindAsync(id);
        if (c == null) return false;
        _db.Complaints.Remove(c);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ComplaintEvent>> GetTimelineAsync(long complaintId) =>
        await _db.ComplaintEvents.Where(e => e.ComplaintId == complaintId)
            .OrderBy(e => e.CreatedAt).ToListAsync();

    public async Task<int> GetCountAsync() => await _db.Complaints.CountAsync();

    public async Task<Complaint?> TransitionStatusAsync(long id, string newStatus)
    {
        var c = await _db.Complaints.FindAsync(id);
        if (c == null) return null;
        var oldStatus = c.Status;
        c.Status = newStatus;
        c.UpdatedAt = DateTime.Now;
        if (newStatus == "closed" && !c.ClosedAt.HasValue) c.ClosedAt = DateTime.Now;
        await LogEventAsync(id, "status_change", $"from:{oldStatus}->to:{newStatus}");
        await _db.SaveChangesAsync();
        return c;
    }

    public async Task<string> ExportPdfAsync(long id)
    {
        var c = await _db.Complaints.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return "";
        // Placeholder: return minimal PDF-like text representation
        return $"Complaint Report: {c.ComplaintCode}\nSubject: {c.Subject}\nSeverity: {c.Severity}\nStatus: {c.Status}\nDescription: {c.Description}";
    }

    private async Task LogEventAsync(long complaintId, string eventType, string? eventData) =>
        _db.ComplaintEvents.Add(new ComplaintEvent { ComplaintId = complaintId, EventType = eventType, EventData = eventData });
}

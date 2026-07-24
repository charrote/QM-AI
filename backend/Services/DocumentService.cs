using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M12;

namespace QM_AI.API.Services;

public class DocumentService
{
    private readonly AppDbContext _db;

    public DocumentService(AppDbContext db) => _db = db;

    public async Task<(List<Document> Items, int Total)> GetAllAsync(string? docType, string? status, int page = 1, int pageSize = 20)
    {
        var query = _db.Documents.AsQueryable();
        if (!string.IsNullOrEmpty(docType)) query = query.Where(d => d.DocType == docType);
        if (!string.IsNullOrEmpty(status)) query = query.Where(d => d.Status == status);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public async Task<Document?> GetByIdAsync(long id) => await _db.Documents.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Document> CreateAsync(Document doc)
    {
        doc.Status = "draft";
        doc.Version = 1;
        doc.CreatedAt = DateTime.Now;
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    public async Task<Document?> UpdateAsync(long id, Document updated)
    {
        var d = await _db.Documents.FindAsync(id);
        if (d == null) return null;
        d.Title = updated.Title;
        d.DocType = updated.DocType;
        d.MinioKey = updated.MinioKey;
        d.FileSizeBytes = updated.FileSizeBytes;
        d.FileHash = updated.FileHash ?? d.FileHash;
        d.ExpiresAt = updated.ExpiresAt ?? d.ExpiresAt;
        d.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return d;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var d = await _db.Documents.FindAsync(id);
        if (d == null) return false;
        _db.Documents.Remove(d);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Document?> ApproveAsync(long id, string reviewerId) => await ApproveInternal(id, reviewerId, "approved");

    public async Task<Document?> RejectAsync(long id, string reviewerId, string? reason) => await RejectInternal(id, reviewerId, reason ?? "Rejected");

    private async Task<Document?> ApproveInternal(long id, string reviewerId, string newStatus)
    {
        var d = await _db.Documents.FindAsync(id);
        if (d == null) return null;
        d.ApprovedByStr = reviewerId;
        if (long.TryParse(reviewerId, out var aid)) d.ApprovedBy = aid;
        d.ApprovedAt = DateTime.Now;
        d.Status = newStatus;
        d.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return d;
    }

    private async Task<Document?> RejectInternal(long id, string reviewerId, string reason)
    {
        var d = await _db.Documents.FindAsync(id);
        if (d == null) return null;
        d.ApprovedByStr = reviewerId;
        if (long.TryParse(reviewerId, out var aid)) d.ApprovedBy = aid;
        d.ApprovedAt = DateTime.Now;
        d.Status = "draft";
        d.RejectionReason = reason;
        d.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return d;
    }

    public async Task<DocumentVersion> CreateVersionAsync(long documentId, DocumentVersion version) => await CreateVersionInternal(documentId, version);

    private async Task<DocumentVersion> CreateVersionInternal(long documentId, DocumentVersion version)
    {
        var doc = await _db.Documents.FindAsync(documentId);
        if (doc == null) throw new InvalidOperationException($"Document {documentId} not found");

        var maxVer = await _db.DocumentVersions.Where(v => v.DocumentId == documentId).MaxAsync(v => v.Version);
        version.DocumentId = documentId;
        version.Version = maxVer + 1;
        version.CreatedAt = DateTime.Now;

        _db.DocumentVersions.Add(version);
        await _db.SaveChangesAsync();

        // Sync doc latest pointer
        doc.MinioKey = version.MinioKey; // latest version wins
        doc.Version = maxVer + 1;
        doc.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        return version;
    }

    public async Task<List<DocumentVersion>> GetVersionsAsync(long documentId) => await _db.DocumentVersions.Where(v => v.DocumentId == documentId).OrderByDescending(v => v.Version).ToListAsync();

    public async Task<int> GetCountAsync() => await _db.Documents.CountAsync();
}

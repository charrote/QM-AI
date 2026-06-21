using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M05;
using QM_AI.API.Models.M05;

namespace QM_AI.API.Services;

/// <summary>
/// FQC/OQC 成品检验业务服务 — S5-01 + S5-02 + S5-03
/// </summary>
public class FqcService
{
    private readonly AppDbContext _db;

    public FqcService(AppDbContext db)
    {
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════════
    // 批次管理 (ProductBatch)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<ProductBatchListDto>> ListBatches(PagedRequest req)
    {
        var query = _db.ProductBatches
            .Include(b => b.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(b =>
                b.BatchCode.Contains(req.Keyword) ||
                b.Product!.Name.Contains(req.Keyword) ||
                b.Product!.Code.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(b => b.Status == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(b => new ProductBatchListDto
            {
                Id = b.Id,
                BatchCode = b.BatchCode,
                ProductId = b.ProductId,
                ProductName = b.Product!.Name,
                WorkOrderId = b.WorkOrderId,
                Quantity = b.Quantity,
                Source = b.Source,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ProductBatchListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<ProductBatchDetailDto?> GetBatch(long id)
    {
        return await _db.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Inspections!)
                .ThenInclude(i => i.Items)
            .Include(b => b.Releases!)
                .ThenInclude(r => r.Customer)
            .Include(b => b.PackagingConfirmations)
            .Where(b => b.Id == id)
            .Select(b => new ProductBatchDetailDto
            {
                Id = b.Id,
                BatchCode = b.BatchCode,
                ProductId = b.ProductId,
                ProductName = b.Product!.Name,
                WorkOrderId = b.WorkOrderId,
                Quantity = b.Quantity,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                Inspections = b.Inspections!.Select(i => new FqcInspectionListDto
                {
                    Id = i.Id,
                    InspectionNo = i.InspectionNo,
                    BatchId = i.BatchId,
                    WorkOrderId = i.WorkOrderId,
                    InspectionType = i.InspectionType,
                    AqlLevel = i.AqlLevel,
                    SampleSize = i.SampleSize,
                    TotalChecked = i.TotalChecked,
                    TotalPass = i.TotalPass,
                    TotalFail = i.TotalFail,
                    Ac = i.Ac,
                    Re = i.Re,
                    Conclusion = i.Conclusion,
                    CheckedAt = i.CheckedAt,
                    CreatedAt = i.CreatedAt
                }).ToList(),
                Releases = b.Releases!.Select(r => new OqcReleaseListDto
                {
                    Id = r.Id,
                    BatchId = r.BatchId,
                    BatchCode = b.BatchCode,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer != null ? r.Customer.Name : "",
                    ReleaseNumber = r.ReleaseNumber,
                    ReleaseDate = r.ReleaseDate,
                    Quantity = r.Quantity,
                    ESignatureUrl = r.ESignatureUrl,
                    SignatureTime = r.SignatureTime,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToList(),
                PackagingConfirmations = b.PackagingConfirmations!.Select(p => new PackagingConfirmationListDto
                {
                    Id = p.Id,
                    BatchId = p.BatchId,
                    BatchCode = b.BatchCode,
                    PackagingMethod = p.PackagingMethod,
                    QtyPerBox = p.QtyPerBox,
                    TotalBoxes = p.TotalBoxes,
                    LabelPrinted = p.LabelPrinted,
                    ConfirmedByName = "",
                    ConfirmedAt = p.ConfirmedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 创建批次（S5-02：自动生成批次号 LOT-YYYYMMDD-X）
    /// </summary>
    public async Task<ProductBatchDetailDto> CreateBatch(CreateProductBatchDto dto)
    {
        var batchCode = dto.BatchCode ?? await GenerateBatchNumber();

        // 检查编号唯一
        if (await _db.ProductBatches.AnyAsync(b => b.BatchCode == batchCode))
            throw new InvalidOperationException($"批次号 '{batchCode}' 已存在");

        var entity = new ProductBatch
        {
            BatchCode = batchCode,
            ProductId = dto.ProductId,
            WorkOrderId = dto.WorkOrderId,
            Quantity = dto.Quantity,
            Source = dto.Source,
            Status = "in_progress"
        };

        _db.ProductBatches.Add(entity);
        await _db.SaveChangesAsync();

        return (await GetBatch(entity.Id))!;
    }

    public async Task<ProductBatchDetailDto?> UpdateBatch(long id, UpdateProductBatchDto dto)
    {
        var entity = await _db.ProductBatches.FindAsync(id);
        if (entity == null) return null;

        if (dto.Quantity.HasValue) entity.Quantity = dto.Quantity.Value;
        if (dto.Status != null) entity.Status = dto.Status;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetBatch(id);
    }

    // ═══════════════════════════════════════════════════════════════
    // 成品检验 (FqcInspection)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<FqcInspectionListDto>> ListInspections(PagedRequest req)
    {
        var query = _db.FqcInspections
            .Include(i => i.Batch)
                .ThenInclude(b => b!.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(i =>
                i.InspectionNo.Contains(req.Keyword) ||
                i.Batch!.BatchCode.Contains(req.Keyword) ||
                i.Batch.Product!.Name.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(i => i.Conclusion == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => new FqcInspectionListDto
            {
                Id = i.Id,
                InspectionNo = i.InspectionNo,
                BatchId = i.BatchId,
                BatchCode = i.Batch!.BatchCode,
                WorkOrderId = i.WorkOrderId,
                InspectionType = i.InspectionType,
                AqlLevel = i.AqlLevel,
                SampleSize = i.SampleSize,
                TotalChecked = i.TotalChecked,
                TotalPass = i.TotalPass,
                TotalFail = i.TotalFail,
                Ac = i.Ac,
                Re = i.Re,
                Conclusion = i.Conclusion,
                CheckedAt = i.CheckedAt,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<FqcInspectionListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<FqcInspectionDetailDto?> GetInspection(long id)
    {
        return await _db.FqcInspections
            .Include(i => i.Batch)
                .ThenInclude(b => b!.Product)
            .Include(i => i.Items!)
            .Where(i => i.Id == id)
            .Select(i => new FqcInspectionDetailDto
            {
                Id = i.Id,
                InspectionNo = i.InspectionNo,
                BatchId = i.BatchId,
                BatchCode = i.Batch!.BatchCode,
                WorkOrderId = i.WorkOrderId,
                InspectionType = i.InspectionType,
                AqlLevel = i.AqlLevel,
                SampleSize = i.SampleSize,
                TotalChecked = i.TotalChecked,
                TotalPass = i.TotalPass,
                TotalFail = i.TotalFail,
                Ac = i.Ac,
                Re = i.Re,
                Conclusion = i.Conclusion,
                CheckedAt = i.CheckedAt,
                CreatedAt = i.CreatedAt,
                ProductName = i.Batch.Product!.Name,
                BatchQuantity = i.Batch.Quantity,
                Items = i.Items!.Select(it => new FqcInspectionItemDto
                {
                    Id = it.Id,
                    InspectionId = it.InspectionId,
                    ItemName = it.ItemName,
                    ItemCode = it.ItemCode,
                    Usl = it.Usl,
                    Lsl = it.Lsl,
                    DataType = it.DataType,
                    ActualValue = it.ActualValue,
                    Result = it.Result,
                    ImageUrls = it.ImageUrls
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<FqcInspectionDetailDto> CreateInspection(CreateFqcInspectionDto dto)
    {
        var inspectionNo = await GenerateInspectionNo();

        var entity = new FqcInspection
        {
            InspectionNo = inspectionNo,
            BatchId = dto.BatchId,
            WorkOrderId = dto.WorkOrderId,
            InspectionType = dto.InspectionType,
            AqlLevel = dto.AqlLevel,
            SampleSize = dto.SampleSize,
            Ac = dto.Ac,
            Re = dto.Re,
            InspectorId = dto.InspectorId,
            Conclusion = "pending"
        };

        _db.FqcInspections.Add(entity);
        await _db.SaveChangesAsync();

        // 更新批次状态
        var batch = await _db.ProductBatches.FindAsync(dto.BatchId);
        if (batch != null && batch.Status == "in_progress")
        {
            batch.Status = "inspected";
            batch.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return (await GetInspection(entity.Id))!;
    }

    /// <summary>
    /// 提交检验结果（自动判定合格/不合格）
    /// </summary>
    public async Task<FqcInspectionDetailDto?> SubmitInspection(long id, SubmitFqcInspectionDto dto)
    {
        var inspection = await _db.FqcInspections
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inspection == null) return null;
        if (inspection.Conclusion != "pending")
            throw new InvalidOperationException("检验单已提交，不可重复提交");

        // 更新检验明细
        if (dto.Items != null)
        {
            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Id.HasValue)
                {
                    var existing = inspection.Items?.FirstOrDefault(it => it.Id == itemDto.Id.Value);
                    if (existing != null)
                    {
                        existing.ActualValue = itemDto.ActualValue;
                        existing.Result = itemDto.Result;
                        existing.ImageUrls = itemDto.ImageUrls;
                    }
                }
                else
                {
                    inspection.Items?.Add(new FqcInspectionItem
                    {
                        InspectionId = id,
                        ItemName = itemDto.ItemName ?? "",
                        ItemCode = itemDto.ItemCode,
                        Usl = itemDto.Usl,
                        Lsl = itemDto.Lsl,
                        DataType = itemDto.DataType,
                        ActualValue = itemDto.ActualValue,
                        Result = itemDto.Result,
                        ImageUrls = itemDto.ImageUrls
                    });
                }
            }
        }

        inspection.TotalChecked = dto.TotalChecked;
        inspection.TotalPass = dto.TotalPass;
        inspection.TotalFail = dto.TotalFail;
        inspection.InspectorId = dto.InspectorId;
        inspection.CheckedAt = DateTime.UtcNow;

        // 自动判定
        if (dto.TotalFail <= inspection.Ac || inspection.InspectionType == "full")
        {
            // 全检：全部合格才判定合格
            if (inspection.InspectionType == "full")
            {
                inspection.Conclusion = dto.TotalFail == 0 ? "qualified" : "unqualified";
            }
            else
            {
                // 抽检：不合格数 ≤ Ac 判定合格
                inspection.Conclusion = "qualified";
            }
        }
        else
        {
            inspection.Conclusion = "unqualified";
        }

        inspection.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // 更新批次状态
        var batch = await _db.ProductBatches.FindAsync(inspection.BatchId);
        if (batch != null)
        {
            batch.Status = inspection.Conclusion == "qualified" ? "inspected" : "quarantined";
            batch.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return await GetInspection(id);
    }

    // ═══════════════════════════════════════════════════════════════
    // 出货放行 (OqcRelease)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<OqcReleaseListDto>> ListReleases(PagedRequest req)
    {
        var query = _db.OqcReleases
            .Include(r => r.Batch)
            .Include(r => r.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(r =>
                r.ReleaseNumber.Contains(req.Keyword) ||
                r.Batch!.BatchCode.Contains(req.Keyword) ||
                r.Customer!.Name.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(r => r.Status == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new OqcReleaseListDto
            {
                Id = r.Id,
                BatchId = r.BatchId,
                BatchCode = r.Batch!.BatchCode,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer!.Name,
                ReleaseNumber = r.ReleaseNumber,
                ReleaseDate = r.ReleaseDate,
                Quantity = r.Quantity,
                ESignatureUrl = r.ESignatureUrl,
                SignatureTime = r.SignatureTime,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<OqcReleaseListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<OqcReleaseListDto?> GetRelease(long id)
    {
        return await _db.OqcReleases
            .Include(r => r.Batch)
            .Include(r => r.Customer)
            .Where(r => r.Id == id)
            .Select(r => new OqcReleaseListDto
            {
                Id = r.Id,
                BatchId = r.BatchId,
                BatchCode = r.Batch!.BatchCode,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer!.Name,
                ReleaseNumber = r.ReleaseNumber,
                ReleaseDate = r.ReleaseDate,
                Quantity = r.Quantity,
                ESignatureUrl = r.ESignatureUrl,
                SignatureTime = r.SignatureTime,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OqcReleaseListDto> CreateRelease(CreateOqcReleaseDto dto)
    {
        var releaseNo = dto.ReleaseNumber ?? $"REL-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}";

        if (await _db.OqcReleases.AnyAsync(r => r.ReleaseNumber == releaseNo))
            throw new InvalidOperationException($"放行单号 '{releaseNo}' 已存在");

        var entity = new OqcRelease
        {
            BatchId = dto.BatchId,
            CustomerId = dto.CustomerId,
            ReleaseNumber = releaseNo,
            ReleaseDate = dto.ReleaseDate,
            Quantity = dto.Quantity,
            Status = "pending"
        };

        _db.OqcReleases.Add(entity);
        await _db.SaveChangesAsync();

        return (await GetRelease(entity.Id))!;
    }

    /// <summary>
    /// 电子签名（S5-03：签名图片保存到 MinIO）
    /// </summary>
    public async Task<OqcReleaseListDto?> SignRelease(long id, SignOqcReleaseDto dto)
    {
        var entity = await _db.OqcReleases.FindAsync(id);
        if (entity == null) return null;

        entity.AuthorizedBy = dto.AuthorizedBy;
        entity.ESignatureUrl = dto.ESignatureUrl;
        entity.SignatureTime = DateTime.UtcNow;
        entity.Status = "signed";
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // 更新批次状态
        var batch = await _db.ProductBatches.FindAsync(entity.BatchId);
        if (batch != null)
        {
            batch.Status = "released";
            batch.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return await GetRelease(id);
    }

    public async Task<bool> ConfirmRelease(long id)
    {
        var entity = await _db.OqcReleases.FindAsync(id);
        if (entity == null) return false;

        entity.Status = "released";
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // 包装确认 (PackagingConfirmation)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<PackagingConfirmationListDto>> ListPackagingConfirmations(PagedRequest req)
    {
        var query = _db.PackagingConfirmations
            .Include(p => p.Batch)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p =>
                p.Batch!.BatchCode.Contains(req.Keyword) ||
                p.PackagingMethod.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.ConfirmedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new PackagingConfirmationListDto
            {
                Id = p.Id,
                BatchId = p.BatchId,
                BatchCode = p.Batch!.BatchCode,
                PackagingMethod = p.PackagingMethod,
                QtyPerBox = p.QtyPerBox,
                TotalBoxes = p.TotalBoxes,
                LabelPrinted = p.LabelPrinted,
                ConfirmedByName = "",
                ConfirmedAt = p.ConfirmedAt
            })
            .ToListAsync();

        return new PagedResult<PackagingConfirmationListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<PackagingConfirmationListDto> CreatePackagingConfirmation(CreatePackagingConfirmationDto dto)
    {
        var entity = new PackagingConfirmation
        {
            BatchId = dto.BatchId,
            PackagingMethod = dto.PackagingMethod,
            QtyPerBox = dto.QtyPerBox,
            TotalBoxes = dto.TotalBoxes,
            LabelPrinted = dto.LabelPrinted,
            ConfirmedBy = dto.ConfirmedBy
        };

        _db.PackagingConfirmations.Add(entity);
        await _db.SaveChangesAsync();

        // 更新批次状态
        var batch = await _db.ProductBatches.FindAsync(dto.BatchId);
        if (batch != null)
        {
            batch.Status = "released";
            batch.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return new PackagingConfirmationListDto
        {
            Id = entity.Id,
            BatchId = entity.BatchId,
            PackagingMethod = entity.PackagingMethod,
            QtyPerBox = entity.QtyPerBox,
            TotalBoxes = entity.TotalBoxes,
            LabelPrinted = entity.LabelPrinted,
            ConfirmedAt = entity.ConfirmedAt
        };
    }

    /// <summary>
    /// 更新标签打印状态
    /// </summary>
    public async Task<bool> UpdateLabelPrinted(long id, bool printed)
    {
        var entity = await _db.PackagingConfirmations.FindAsync(id);
        if (entity == null) return false;

        entity.LabelPrinted = printed;
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // 批次号生成（S5-02）
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 自动生成批次号：LOT-YYYYMMDD-X
    /// </summary>
    public async Task<string> GenerateBatchNumber()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var prefix = $"LOT-{date}-";

        var todayCount = await _db.ProductBatches
            .CountAsync(b => b.BatchCode.StartsWith(prefix));

        return $"{prefix}{(char)('A' + todayCount)}";
    }

    /// <summary>
    /// 生成检验单号
    /// </summary>
    private async Task<string> GenerateInspectionNo()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _db.FqcInspections
            .CountAsync(i => i.InspectionNo.StartsWith($"FQC-{date}")) + 1;

        return $"FQC-{date}-{count:D4}";
    }
}

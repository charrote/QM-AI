using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M03;
using QM_AI.API.Models;
using QM_AI.API.Models.M03;

namespace QM_AI.API.Services;

/// <summary>
/// IQC 来料检验业务服务
/// </summary>
public class IqcService
{
    private readonly AppDbContext _db;
    private readonly SamplingPlanCalculator _samplingCalculator;

    public IqcService(AppDbContext db, SamplingPlanCalculator samplingCalculator)
    {
        _db = db;
        _samplingCalculator = samplingCalculator;
    }

    // ═══════════════════════════════════════════════════════════════
    // 来料登记 (Receipt)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<IqcReceiptListDto>> ListReceipts(PagedRequest req)
    {
        var query = _db.IqcReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(r =>
                r.ReceiptNo.Contains(req.Keyword) ||
                r.BatchNo!.Contains(req.Keyword) ||
                r.Supplier!.Name.Contains(req.Keyword) ||
                r.Product!.Name.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(r => r.Status == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(r => new IqcReceiptListDto
            {
                Id = r.Id,
                ReceiptNo = r.ReceiptNo,
                SupplierId = r.SupplierId,
                SupplierName = r.Supplier!.Name,
                ProductId = r.ProductId,
                ProductName = r.Product!.Name,
                BatchNo = r.BatchNo,
                Quantity = r.Quantity,
                Unit = r.Unit,
                ReceiptDate = r.ReceiptDate,
                Inspector = r.Inspector,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<IqcReceiptListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<IqcReceiptDetailDto?> GetReceipt(long id)
    {
        return await _db.IqcReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Product)
            .Include(r => r.Inspections!)
                .ThenInclude(i => i.Items)
            .Include(r => r.Anomalies)
            .Where(r => r.Id == id)
            .Select(r => new IqcReceiptDetailDto
            {
                Id = r.Id,
                ReceiptNo = r.ReceiptNo,
                SupplierId = r.SupplierId,
                SupplierName = r.Supplier!.Name,
                ProductId = r.ProductId,
                ProductName = r.Product!.Name,
                BatchNo = r.BatchNo,
                Quantity = r.Quantity,
                Unit = r.Unit,
                ReceiptDate = r.ReceiptDate,
                Inspector = r.Inspector,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                Inspections = r.Inspections!.Select(i => new IqcInspectionListDto
                {
                    Id = i.Id,
                    InspectionNo = i.InspectionNo,
                    ReceiptId = i.ReceiptId,
                    SampleSize = i.SampleSize,
                    Ac = i.Ac,
                    Re = i.Re,
                    DefectQty = i.DefectQty,
                    SamplingLevel = i.SamplingLevel,
                    AqlValue = i.AqlValue,
                    Result = i.Result,
                    Inspector = i.Inspector,
                    InspectedAt = i.InspectedAt,
                    CreatedAt = i.CreatedAt
                }).ToList(),
                Anomalies = r.Anomalies!.Select(a => new IqcAnomalyListDto
                {
                    Id = a.Id,
                    AnomalyNo = a.AnomalyNo,
                    ReceiptId = a.ReceiptId,
                    InspectionId = a.InspectionId,
                    AnomalyType = a.AnomalyType,
                    Severity = a.Severity,
                    Description = a.Description,
                    Status = a.Status,
                    Handler = a.Handler,
                    ResolvedAt = a.ResolvedAt,
                    CreatedAt = a.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IqcReceiptDetailDto> CreateReceipt(CreateIqcReceiptDto dto)
    {
        // 检查单号唯一
        if (await _db.IqcReceipts.AnyAsync(r => r.ReceiptNo == dto.ReceiptNo))
            throw new InvalidOperationException($"收货单号 '{dto.ReceiptNo}' 已存在");

        var entity = new IqcReceipt
        {
            ReceiptNo = dto.ReceiptNo,
            SupplierId = dto.SupplierId,
            ProductId = dto.ProductId,
            BatchNo = dto.BatchNo,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            ReceiptDate = dto.ReceiptDate ?? DateTime.UtcNow,
            Inspector = dto.Inspector,
            Status = "pending"
        };

        _db.IqcReceipts.Add(entity);
        await _db.SaveChangesAsync();

        // 自动创建检验单
        await AutoCreateInspection(entity);

        return (await GetReceipt(entity.Id))!;
    }

    public async Task<IqcReceiptDetailDto?> UpdateReceipt(long id, UpdateIqcReceiptDto dto)
    {
        var entity = await _db.IqcReceipts.FindAsync(id);
        if (entity == null) return null;

        if (dto.BatchNo != null) entity.BatchNo = dto.BatchNo;
        entity.Quantity = dto.Quantity;
        if (dto.Unit != null) entity.Unit = dto.Unit;
        if (dto.ReceiptDate.HasValue) entity.ReceiptDate = dto.ReceiptDate;
        if (dto.Inspector != null) entity.Inspector = dto.Inspector;
        if (dto.Status != null) entity.Status = dto.Status;

        await _db.SaveChangesAsync();
        return await GetReceipt(id);
    }

    public async Task<bool> DeleteReceipt(long id)
    {
        var entity = await _db.IqcReceipts.FindAsync(id);
        if (entity == null) return false;

        _db.IqcReceipts.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // 检验单 (Inspection)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<IqcInspectionListDto>> ListInspections(PagedRequest req)
    {
        var query = _db.IqcInspections
            .Include(i => i.Receipt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(i =>
                i.InspectionNo.Contains(req.Keyword) ||
                i.Receipt!.ReceiptNo.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(i => i.Result == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => new IqcInspectionListDto
            {
                Id = i.Id,
                InspectionNo = i.InspectionNo,
                ReceiptId = i.ReceiptId,
                ReceiptNo = i.Receipt!.ReceiptNo,
                SampleSize = i.SampleSize,
                Ac = i.Ac,
                Re = i.Re,
                DefectQty = i.DefectQty,
                SamplingLevel = i.SamplingLevel,
                AqlValue = i.AqlValue,
                Result = i.Result,
                Inspector = i.Inspector,
                InspectedAt = i.InspectedAt,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<IqcInspectionListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<IqcInspectionDetailDto?> GetInspection(long id)
    {
        return await _db.IqcInspections
            .Include(i => i.Receipt)
                .ThenInclude(r => r!.Supplier)
            .Include(i => i.Receipt)
                .ThenInclude(r => r!.Product)
            .Include(i => i.Items!)
                .ThenInclude(it => it.DefectCode)
            .Where(i => i.Id == id)
            .Select(i => new IqcInspectionDetailDto
            {
                Id = i.Id,
                InspectionNo = i.InspectionNo,
                ReceiptId = i.ReceiptId,
                ReceiptNo = i.Receipt!.ReceiptNo,
                SampleSize = i.SampleSize,
                Ac = i.Ac,
                Re = i.Re,
                DefectQty = i.DefectQty,
                SamplingLevel = i.SamplingLevel,
                AqlValue = i.AqlValue,
                Result = i.Result,
                Inspector = i.Inspector,
                InspectedAt = i.InspectedAt,
                CreatedAt = i.CreatedAt,
                SupplierName = i.Receipt.Supplier!.Name,
                ProductName = i.Receipt.Product!.Name,
                Items = i.Items!.Select(it => new IqcInspectionItemDto
                {
                    Id = it.Id,
                    InspectionId = it.InspectionId,
                    ParamId = it.ParamId,
                    ItemName = it.ItemName,
                    MeasuredValue = it.MeasuredValue,
                    Usl = it.Usl,
                    Lsl = it.Lsl,
                    Result = it.Result,
                    DefectCodeId = it.DefectCodeId,
                    DefectCodeName = it.DefectCode != null ? it.DefectCode.Name : null,
                    Remark = it.Remark
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IqcInspectionDetailDto> CreateInspection(CreateIqcInspectionDto dto)
    {
        // 生成检验单号
        var inspectionNo = await GenerateInspectionNo();

        var entity = new IqcInspection
        {
            InspectionNo = inspectionNo,
            ReceiptId = dto.ReceiptId,
            StandardId = dto.StandardId,
            SampleSize = dto.SampleSize,
            Ac = dto.Ac,
            Re = dto.Re,
            SamplingLevel = dto.SamplingLevel,
            AqlValue = dto.AqlValue,
            Inspector = dto.Inspector,
            Result = "pending"
        };

        _db.IqcInspections.Add(entity);

        // 更新来料登记状态
        var receipt = await _db.IqcReceipts.FindAsync(dto.ReceiptId);
        if (receipt != null && receipt.Status == "pending")
            receipt.Status = "inspecting";

        await _db.SaveChangesAsync();
        return (await GetInspection(entity.Id))!;
    }

    /// <summary>
    /// 提交检验结果（自动判定合格/不合格）
    /// </summary>
    public async Task<IqcInspectionDetailDto?> SubmitInspection(long id, SubmitIqcInspectionDto dto, int userId)
    {
        var inspection = await _db.IqcInspections
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inspection == null) return null;
        if (inspection.Result != "pending")
            throw new InvalidOperationException("检验单已提交，不可重复提交");

        // 更新或创建检验明细
        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Id.HasValue)
            {
                var existing = inspection.Items?.FirstOrDefault(it => it.Id == itemDto.Id.Value);
                if (existing != null)
                {
                    existing.MeasuredValue = itemDto.MeasuredValue;
                    existing.Result = itemDto.Result;
                    existing.DefectCodeId = itemDto.DefectCodeId;
                    existing.Remark = itemDto.Remark;
                }
            }
            else
            {
                inspection.Items?.Add(new IqcInspectionItem
                {
                    InspectionId = id,
                    ParamId = itemDto.ParamId,
                    InspectionItemId = itemDto.InspectionItemId,
                    ItemName = itemDto.ItemName,
                    MeasuredValue = itemDto.MeasuredValue,
                    Usl = itemDto.Usl,
                    Lsl = itemDto.Lsl,
                    Result = itemDto.Result,
                    DefectCodeId = itemDto.DefectCodeId,
                    Remark = itemDto.Remark
                });
            }
        }

        // 统计不合格数量
        var failCount = (inspection.Items ?? new List<IqcInspectionItem>())
            .Count(it => it.Result == "fail");

        inspection.DefectQty = failCount;
        inspection.Inspector = dto.Inspector;
        inspection.InspectedAt = DateTime.UtcNow;

        // 自动判定
        if (failCount <= inspection.Ac)
        {
            inspection.Result = "pass";

            // 更新来料登记状态
            var receipt = await _db.IqcReceipts.FindAsync(inspection.ReceiptId);
            if (receipt != null) receipt.Status = "qualified";
        }
        else
        {
            inspection.Result = "fail";

            // 更新来料登记状态
            var receipt = await _db.IqcReceipts.FindAsync(inspection.ReceiptId);
            if (receipt != null) receipt.Status = "unqualified";

            // 自动创建异常单
            await AutoCreateAnomaly(inspection);
        }

        await _db.SaveChangesAsync();

        // 更新供应商评分
        await UpdateSupplierScore(inspection.ReceiptId);

        return await GetInspection(id);
    }

    /// <summary>
    /// 自动创建检验单（来料登记时调用）
    /// </summary>
    private async Task AutoCreateInspection(IqcReceipt receipt)
    {
        // 获取产品默认检验设置
        var product = await _db.Products.FindAsync(receipt.ProductId);

        var level = product?.DefaultInspectionLevel ?? "II";
        var aql = product?.DefaultAql ?? 1.0;

        // 获取检验标准
        var standard = await _db.InspectionStandards
            .Where(s => s.InspectionType == "IQC" && s.IsActive)
            .FirstOrDefaultAsync();

        // 计算抽样方案
        var plan = _samplingCalculator.GetSamplingPlan(receipt.Quantity, level, aql);

        var inspectionNo = $"IQC-{DateTime.Now:yyyyMMdd}-{receipt.Id:D4}";

        var inspection = new IqcInspection
        {
            InspectionNo = inspectionNo,
            ReceiptId = receipt.Id,
            StandardId = standard?.Id,
            SampleSize = plan.SampleSize > 0 ? plan.SampleSize : Math.Min(receipt.Quantity, 50),
            Ac = plan.Ac,
            Re = plan.Re,
            SamplingLevel = level,
            AqlValue = aql,
            Result = "pending",
            Inspector = receipt.Inspector
        };

        _db.IqcInspections.Add(inspection);
        await _db.SaveChangesAsync();

        // 从检验标准生成检验项目
        if (standard != null)
        {
            var item = new IqcInspectionItem
            {
                InspectionId = inspection.Id,
                ItemName = standard.ItemName,
                Usl = standard.Usl.HasValue ? (decimal)standard.Usl.Value : null,
                Lsl = standard.Lsl.HasValue ? (decimal)standard.Lsl.Value : null,
                Result = "pending"
            };
            _db.IqcInspectionItems.Add(item);
        }
        else
        {
            // 无检验标准时，添加默认检验项目
            var item = new IqcInspectionItem
            {
                InspectionId = inspection.Id,
                ItemName = "外观检查",
                Result = "pending"
            };
            _db.IqcInspectionItems.Add(item);
        }

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 自动创建异常单（不合格品自动触发）
    /// 自动完成：异常创建 → 隔离 → 不合格数量统计 → 来料状态更新
    /// </summary>
    private async Task AutoCreateAnomaly(IqcInspection inspection)
    {
        var anomalyNo = $"ANM-{DateTime.Now:yyyyMMdd}-{inspection.Id:D4}";

        // 获取不合格检验项目ID列表
        var failedItems = (inspection.Items ?? new List<IqcInspectionItem>())
            .Where(it => it.Result == "fail")
            .Select(it => it.Id)
            .ToList();

        var anomaly = new IqcAnomaly
        {
            AnomalyNo = anomalyNo,
            ReceiptId = inspection.ReceiptId,
            InspectionId = inspection.Id,
            FailedItemIds = failedItems.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(failedItems) : null,
            DefectQty = inspection.DefectQty,
            AnomalyType = "quality",
            Severity = inspection.DefectQty > inspection.Re ? "critical" : "major",
            Description = $"来料检验不合格：检验单 {inspection.InspectionNo}，不合格数 {inspection.DefectQty}/{inspection.SampleSize}，Ac={inspection.Ac}，Re={inspection.Re}",
            IsolatedInventory = inspection.Receipt?.Quantity ?? 0,
            Status = "quarantined",
            Handler = inspection.Inspector,
            FirstResponseAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.IqcAnomalies.Add(anomaly);

        // 更新来料登记状态为隔离/异常
        var receipt = await _db.IqcReceipts.FindAsync(inspection.ReceiptId);
        if (receipt != null) receipt.Status = "anomaly";

        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// 更新供应商评分
    /// </summary>
    private async Task UpdateSupplierScore(long receiptId)
    {
        var receipt = await _db.IqcReceipts.FindAsync(receiptId);
        if (receipt == null) return;

        // 获取供应商最近若干批次的检验数据
        var recentInspections = await _db.IqcInspections
            .Where(i => i.Receipt!.SupplierId == receipt.SupplierId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(20)
            .ToListAsync();

        if (recentInspections.Count == 0) return;

        var passCount = recentInspections.Count(i => i.Result == "pass");
        var passRate = (double)passCount / recentInspections.Count;

        // 计算评分
        var score = passRate * 100;

        // 扣分项：最近异常次数
        var anomalyCount = await _db.IqcAnomalies
            .CountAsync(a => a.Receipt!.SupplierId == receipt.SupplierId &&
                             a.CreatedAt >= DateTime.UtcNow.AddMonths(-3));
        score -= anomalyCount * 2;

        score = Math.Max(0, Math.Min(100, score));

        // 评级
        var grade = score switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 60 => "C",
            _ => "D"
        };

        // 保存或更新评分
        var existingScore = await _db.SupplierScores
            .Where(s => s.SupplierId == receipt.SupplierId)
            .OrderByDescending(s => s.ScoreDate)
            .FirstOrDefaultAsync();

        if (existingScore != null && existingScore.ScoreDate?.Date == DateTime.UtcNow.Date)
        {
            existingScore.Score = (decimal)score;
            existingScore.Grade = grade;
        }
        else
        {
            _db.SupplierScores.Add(new SupplierScore
            {
                SupplierId = receipt.SupplierId,
                ScoreDate = DateTime.UtcNow,
                Score = (decimal)score,
                Grade = grade,
                Evaluation = $"基于最近 {recentInspections.Count} 批检验数据自动计算"
            });
        }

        // 同步更新供应商表
        var supplier = await _db.Suppliers.FindAsync(receipt.SupplierId);
        if (supplier != null)
        {
            supplier.Score = Math.Round(score, 2);
            supplier.Grade = grade;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // 异常单 (Anomaly) - 增强版：完整流程支持
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<IqcAnomalyListDto>> ListAnomalies(PagedRequest req)
    {
        var query = _db.IqcAnomalies
            .Include(a => a.Receipt)
            .Include(a => a.Inspection)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(a =>
                a.AnomalyNo.Contains(req.Keyword) ||
                a.Receipt!.ReceiptNo.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(a => a.Status == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(a => new IqcAnomalyListDto
            {
                Id = a.Id,
                AnomalyNo = a.AnomalyNo,
                ReceiptId = a.ReceiptId,
                ReceiptNo = a.Receipt!.ReceiptNo,
                InspectionId = a.InspectionId,
                FailedItemIds = a.FailedItemIds,
                DefectQty = a.DefectQty,
                AnomalyType = a.AnomalyType,
                Severity = a.Severity,
                Description = a.Description,
                IsolatedInventory = a.IsolatedInventory,
                Disposition = a.Disposition,
                DispositionBy = a.DispositionBy,
                DispositionDate = a.DispositionDate,
                HandlerDept = a.HandlerDept,
                Status = a.Status,
                Handler = a.Handler,
                MrbReviewed = a.MrbReviewed,
                MrbReviewer = a.MrbReviewer,
                MrbReviewedAt = a.MrbReviewedAt,
                CapaId = a.CapaId,
                FirstResponseAt = a.FirstResponseAt,
                SupplierNotified = a.SupplierNotified,
                SupplierResponseAt = a.SupplierResponseAt,
                ResolvedAt = a.ResolvedAt,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<IqcAnomalyListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    /// <summary>
    /// 创建异常单（增强版）
    /// 状态自动设为 quarantined（待隔离），标记首次响应时间
    /// </summary>
    public async Task<IqcAnomalyListDto> CreateAnomaly(CreateIqcAnomalyDto dto)
    {
        var anomalyNo = dto.AnomalyNo ?? $"ANM-{DateTime.Now:yyyyMMdd}-{GenerateAnomalySeq()}";

        var entity = new IqcAnomaly
        {
            AnomalyNo = anomalyNo,
            ReceiptId = dto.ReceiptId,
            InspectionId = dto.InspectionId,
            FailedItemIds = dto.FailedItemIds,
            DefectQty = dto.DefectQty,
            AnomalyType = dto.AnomalyType,
            Severity = dto.Severity,
            Description = dto.Description,
            IsolatedInventory = dto.IsolatedInventory,
            HandlerDept = dto.HandlerDept,
            Status = "quarantined", // 异常创建后自动进入隔离状态
            Handler = dto.Handler,
            FirstResponseAt = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.IqcAnomalies.Add(entity);

        // 更新来料登记状态为异常/隔离
        var receipt = await _db.IqcReceipts.FindAsync(dto.ReceiptId);
        if (receipt != null && receipt.Status == "pending")
            receipt.Status = "anomaly";

        await _db.SaveChangesAsync();

        return MapAnomalyToDto(entity);
    }

    /// <summary>
    /// 更新异常单（通用更新）
    /// </summary>
    public async Task<IqcAnomalyListDto?> UpdateAnomaly(long id, UpdateIqcAnomalyDto dto)
    {
        var entity = await _db.IqcAnomalies.FindAsync(id);
        if (entity == null) return null;

        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.Handler != null) entity.Handler = dto.Handler;
        if (dto.HandlerDept != null) entity.HandlerDept = dto.HandlerDept;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.IsolatedInventory.HasValue) entity.IsolatedInventory = dto.IsolatedInventory.Value;
        if (dto.FirstResponseAt.HasValue) entity.FirstResponseAt = dto.FirstResponseAt.Value;

        // MRB 评审相关
        if (dto.MrbReviewed.HasValue) entity.MrbReviewed = dto.MrbReviewed.Value;
        if (dto.MrbReviewer != null) entity.MrbReviewer = dto.MrbReviewer;
        if (dto.MrbReviewedAt.HasValue) entity.MrbReviewedAt = dto.MrbReviewedAt.Value;

        // 处置相关
        if (dto.Disposition != null) entity.Disposition = dto.Disposition;
        if (dto.DispositionBy != null) entity.DispositionBy = dto.DispositionBy;
        if (dto.DispositionDate.HasValue) entity.DispositionDate = dto.DispositionDate.Value;

        // CAPA 关联
        if (dto.CapaId.HasValue) entity.CapaId = dto.CapaId.Value;

        // 供应商通知
        if (dto.SupplierNotified.HasValue) entity.SupplierNotified = dto.SupplierNotified.Value;
        if (dto.SupplierResponseAt.HasValue) entity.SupplierResponseAt = dto.SupplierResponseAt.Value;

        // 自动状态流转
        if (dto.MrbReviewed == true && entity.Status == "mrb_reviewing")
            entity.Status = "mrb_approved";
        if (entity.Status == "open" && entity.FirstResponseAt.HasValue)
            entity.Status = "investigating";

        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await ListAnomalies(new PagedRequest { Page = 1, PageSize = 1, Keyword = entity.AnomalyNo })
            .ContinueWith(t => t.Result.Items.FirstOrDefault());
    }

    /// <summary>
    /// 完成 MRB 评审（多部门会签）
    /// </summary>
    public async Task<IqcAnomalyListDto> MrbReview(long id, MrbReviewDto dto)
    {
        var entity = await _db.IqcAnomalies.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException("异常单不存在");

        entity.MrbReviewed = true;
        entity.MrbReviewer = dto.Reviewer;
        entity.MrbReviewedAt = DateTime.UtcNow;
        entity.Status = dto.Approved ? "mrb_approved" : "investigating"; // 驳回则退回调查
        if (!string.IsNullOrWhiteSpace(dto.ReviewComments))
            entity.Description = $"{entity.Description}\n[MRB评审] {dto.ReviewComments}";
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapAnomalyToDto(entity);
    }

    /// <summary>
    /// 做出处置决定
    /// </summary>
    public async Task<IqcAnomalyListDto> MakeDisposition(long id, DispositionDto dto)
    {
        var entity = await _db.IqcAnomalies.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException("异常单不存在");

        entity.Disposition = dto.Disposition;
        entity.DispositionBy = dto.DispositionBy;
        entity.DispositionDate = DateTime.UtcNow;
        entity.Status = "disposed";
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapAnomalyToDto(entity);
    }

    /// <summary>
    /// 通知供应商
    /// </summary>
    public async Task<IqcAnomalyListDto> NotifySupplier(long id, NotifySupplierDto dto)
    {
        var entity = await _db.IqcAnomalies.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException("异常单不存在");

        entity.SupplierNotified = true;
        entity.SupplierResponseAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Message))
            entity.Description = $"{entity.Description}\n[通知供应商] {dto.Message}";
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapAnomalyToDto(entity);
    }

    /// <summary>
    /// 解决异常单（增强版）
    /// 支持多步骤状态流转：open → investigating → resolved
    /// </summary>
    public async Task<IqcAnomalyListDto> ResolveAnomaly(long id, ResolveIqcAnomalyDto dto)
    {
        var entity = await _db.IqcAnomalies.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException("异常单不存在");

        entity.Status = "resolved";
        entity.Handler = dto.Handler ?? entity.Handler;
        entity.ResolvedAt = DateTime.UtcNow;
        entity.Description = entity.Description != null
            ? $"{entity.Description}\n[解决] {dto.Resolution}"
            : $"[解决] {dto.Resolution}";
        entity.UpdatedBy = dto.UpdatedBy;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapAnomalyToDto(entity);
    }

    /// <summary>
    /// 关闭异常单
    /// </summary>
    public async Task<IqcAnomalyListDto> CloseAnomaly(long id)
    {
        var entity = await _db.IqcAnomalies.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException("异常单不存在");

        entity.Status = "closed";
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapAnomalyToDto(entity);
    }

    /// <summary>
    /// 映射 Entity → DTO（增强版）
    /// </summary>
    private IqcAnomalyListDto MapAnomalyToDto(IqcAnomaly entity)
    {
        return new IqcAnomalyListDto
        {
            Id = entity.Id,
            AnomalyNo = entity.AnomalyNo,
            ReceiptId = entity.ReceiptId,
            InspectionId = entity.InspectionId,
            FailedItemIds = entity.FailedItemIds,
            DefectQty = entity.DefectQty,
            AnomalyType = entity.AnomalyType,
            Severity = entity.Severity,
            Description = entity.Description,
            IsolatedInventory = entity.IsolatedInventory,
            Disposition = entity.Disposition,
            DispositionBy = entity.DispositionBy,
            DispositionDate = entity.DispositionDate,
            HandlerDept = entity.HandlerDept,
            Status = entity.Status,
            Handler = entity.Handler,
            MrbReviewed = entity.MrbReviewed,
            MrbReviewer = entity.MrbReviewer,
            MrbReviewedAt = entity.MrbReviewedAt,
            CapaId = entity.CapaId,
            FirstResponseAt = entity.FirstResponseAt,
            SupplierNotified = entity.SupplierNotified,
            SupplierResponseAt = entity.SupplierResponseAt,
            ResolvedAt = entity.ResolvedAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// 生成异常单号（序号递增格式：ANM-YYYYMMDD-NNNN）
    /// </summary>
    private static int _nextAnomalySeq = 1;
    private string GenerateAnomalySeq()
    {
        var count = Interlocked.Increment(ref _nextAnomalySeq);
        return count.ToString("D4");
    }

    // ═══════════════════════════════════════════════════════════════
    // 供应商评分
    // ═══════════════════════════════════════════════════════════════

    public async Task<SupplierScoreDto?> GetSupplierScore(long supplierId)
    {
        var supplier = await _db.Suppliers.FindAsync(supplierId);
        if (supplier == null) return null;

        var score = await _db.SupplierScores
            .Where(s => s.SupplierId == supplierId)
            .OrderByDescending(s => s.ScoreDate)
            .FirstOrDefaultAsync();

        return new SupplierScoreDto
        {
            Id = score?.Id ?? 0,
            SupplierId = supplierId,
            SupplierName = supplier.Name,
            ScoreDate = score?.ScoreDate,
            Score = score?.Score ?? (decimal?)supplier.Score,
            DimensionScores = score?.DimensionScores,
            Grade = score?.Grade ?? supplier.Grade,
            Evaluation = score?.Evaluation
        };
    }

    public async Task<SupplierScoreDto?> UpdateSupplierScore(int supplierId, UpdateSupplierScoreDto dto)
    {
        var supplier = await _db.Suppliers.FindAsync(supplierId);
        if (supplier == null) return null;

        // 更新主表
        if (dto.Score.HasValue) supplier.Score = (double)dto.Score.Value;
        if (dto.Grade != null) supplier.Grade = dto.Grade;

        // 创建评分记录
        var score = new SupplierScore
        {
            SupplierId = supplierId,
            ScoreDate = dto.ScoreDate ?? DateTime.UtcNow,
            Score = dto.Score,
            DimensionScores = dto.DimensionScores,
            Grade = dto.Grade,
            Evaluation = dto.Evaluation
        };

        _db.SupplierScores.Add(score);
        await _db.SaveChangesAsync();

        return await GetSupplierScore(supplierId);
    }

    // ═══════════════════════════════════════════════════════════════
    // 批次追溯
    // ═══════════════════════════════════════════════════════════════

    public async Task<BatchTraceDto?> TraceByBatch(string batchNo)
    {
        var receipt = await _db.IqcReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.BatchNo == batchNo);

        if (receipt == null) return null;

        var inspections = await _db.IqcInspections
            .Where(i => i.ReceiptId == receipt.Id)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new IqcInspectionListDto
            {
                Id = i.Id,
                InspectionNo = i.InspectionNo,
                ReceiptId = i.ReceiptId,
                SampleSize = i.SampleSize,
                Ac = i.Ac,
                Re = i.Re,
                DefectQty = i.DefectQty,
                SamplingLevel = i.SamplingLevel,
                AqlValue = i.AqlValue,
                Result = i.Result,
                Inspector = i.Inspector,
                InspectedAt = i.InspectedAt,
                CreatedAt = i.CreatedAt
            })
            .ToListAsync();

        var anomalies = await _db.IqcAnomalies
            .Where(a => a.ReceiptId == receipt.Id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new IqcAnomalyListDto
            {
                Id = a.Id,
                AnomalyNo = a.AnomalyNo,
                ReceiptId = a.ReceiptId,
                InspectionId = a.InspectionId,
                AnomalyType = a.AnomalyType,
                Severity = a.Severity,
                Description = a.Description,
                Status = a.Status,
                Handler = a.Handler,
                ResolvedAt = a.ResolvedAt,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        var supplierScore = await GetSupplierScore(receipt.SupplierId);

        return new BatchTraceDto
        {
            Receipt = new IqcReceiptListDto
            {
                Id = receipt.Id,
                ReceiptNo = receipt.ReceiptNo,
                SupplierId = receipt.SupplierId,
                SupplierName = receipt.Supplier?.Name ?? "",
                ProductId = receipt.ProductId,
                ProductName = receipt.Product?.Name ?? "",
                BatchNo = receipt.BatchNo,
                Quantity = receipt.Quantity,
                Unit = receipt.Unit,
                ReceiptDate = receipt.ReceiptDate,
                Inspector = receipt.Inspector,
                Status = receipt.Status,
                CreatedAt = receipt.CreatedAt
            },
            Inspections = inspections,
            Anomalies = anomalies,
            SupplierScore = supplierScore
        };
    }

    /// <summary>
    /// AI 来料风险分析（规则评分版 — S3-05）
    /// </summary>
    public async Task<AiRiskScoreDto> AnalyzeRisk(long receiptId)
    {
        var receipt = await _db.IqcReceipts
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.Id == receiptId);

        if (receipt == null)
            throw new KeyNotFoundException($"来料登记 {receiptId} 不存在");

        var features = new Dictionary<string, double>();

        // 1. 供应商合格率
        var recentInspections = await _db.IqcInspections
            .Where(i => i.Receipt!.SupplierId == receipt.SupplierId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .ToListAsync();

        var passRate = recentInspections.Count > 0
            ? (double)recentInspections.Count(i => i.Result == "pass") / recentInspections.Count
            : 0.95;
        features["supplier_qualified_rate"] = passRate;

        // 2. 供应商异常次数（3个月）
        var anomalyCount = await _db.IqcAnomalies
            .CountAsync(a => a.Receipt!.SupplierId == receipt.SupplierId &&
                             a.CreatedAt >= DateTime.UtcNow.AddMonths(-3));
        features["supplier_anomaly_count_3m"] = anomalyCount;

        // 3. 该物料历史异常次数
        var materialAnomalyCount = await _db.IqcAnomalies
            .CountAsync(a => a.Receipt!.ProductId == receipt.ProductId &&
                             a.CreatedAt >= DateTime.UtcNow.AddMonths(-6));
        features["material_anomaly_history"] = materialAnomalyCount;

        // 4. 批量偏离度
        var avgQty = recentInspections.Count > 0
            ? recentInspections.Average(i => i.SampleSize)
            : receipt.Quantity;
        features["batch_quantity_vs_avg"] = avgQty > 0 ? receipt.Quantity / avgQty : 1.0;

        // 5. 上次异常距今天数
        var lastAnomaly = await _db.IqcAnomalies
            .Where(a => a.Receipt!.SupplierId == receipt.SupplierId)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();
        var daysSinceLastAnomaly = lastAnomaly != null
            ? (DateTime.UtcNow - lastAnomaly.CreatedAt).TotalDays
            : 365;
        features["days_since_last_anomaly"] = daysSinceLastAnomaly;

        // ─── 冷启动规则评分 ───
        var score = 50; // 基准分

        var riskFactors = new List<RiskFactorDto>();

        if (passRate < 0.90)
        {
            score += 15;
            riskFactors.Add(new RiskFactorDto
            {
                Name = "供应商合格率偏低",
                Description = $"近期合格率 {passRate:P1}，低于 90%",
                Impact = 15
            });
        }
        if (passRate < 0.85)
        {
            score += 10;
            riskFactors.Add(new RiskFactorDto
            {
                Name = "供应商合格率严重偏低",
                Description = $"近期合格率 {passRate:P1}，低于 85%",
                Impact = 10
            });
        }
        if (anomalyCount > 3)
        {
            score += 15;
            riskFactors.Add(new RiskFactorDto
            {
                Name = "供应商近期异常偏多",
                Description = $"近 3 个月 {anomalyCount} 次异常",
                Impact = 15
            });
        }
        if (materialAnomalyCount > 2)
        {
            score += 10;
            riskFactors.Add(new RiskFactorDto
            {
                Name = "该物料历史异常较多",
                Description = $"近 6 个月 {materialAnomalyCount} 次异常",
                Impact = 10
            });
        }
        if (daysSinceLastAnomaly < 30)
        {
            score += 10;
            riskFactors.Add(new RiskFactorDto
            {
                Name = "近期刚出过异常",
                Description = $"距上次异常仅 {daysSinceLastAnomaly:F0} 天",
                Impact = 10
            });
        }

        score = Math.Min(score, 100);

        // 风险等级
        var level = score switch
        {
            < 40 => "low",
            < 70 => "warning",
            _ => "high"
        };

        // 建议
        var recommendations = new List<string>();
        if (level == "high")
            recommendations.Add("建议提高抽样比例至 III 级或加严检验");
        else if (level == "warning")
            recommendations.Add("建议保持正常抽检，重点关注高风险项");
        else
            recommendations.Add("建议正常抽检");

        if (passRate < 0.85)
            recommendations.Add("建议对该供应商启动供应商审核流程");

        if (riskFactors.Count == 0)
        {
            riskFactors.Add(new RiskFactorDto
            {
                Name = "供应商表现稳定",
                Description = "近期无风险因素",
                Impact = 0
            });
        }

        return new AiRiskScoreDto
        {
            Score = score,
            Level = level,
            Factors = riskFactors,
            Recommendations = recommendations
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // 辅助方法
    // ═══════════════════════════════════════════════════════════════

    private async Task<string> GenerateInspectionNo()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _db.IqcInspections
            .CountAsync(i => i.InspectionNo!.StartsWith($"INS-{date}")) + 1;

        return $"INS-{date}-{count:D4}";
    }
}

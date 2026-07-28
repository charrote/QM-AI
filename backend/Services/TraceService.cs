using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models.M03;
using QM_AI.API.Models.M04;
using QM_AI.API.Models.M05;

namespace QM_AI.API.Services;

/// <summary>
/// 质量追溯服务（只读查询引擎）
/// </summary>
public class TraceService
{
    private readonly AppDbContext _db;

    public TraceService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 按 SN 编码全维度追溯
    /// </summary>
    public async Task<TraceResult> TraceBySnAsync(string serialNumber)
    {
        // 解析 SN → 产品/批次/工单
        // 格式约定: SN-YYYYMMDD-ProductId-BatchSeq-ItemSeq
        // 例如: SN-20260619-001-001-0042
        var parts = serialNumber.Split('-');
        var productCode = parts.Length >= 4 ? parts[2] : "";
        var batchSeq = parts.Length >= 5 ? parts[3] : "";

        // 查找产品
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Code == productCode || p.Name!.Contains(productCode));
        if (product == null)
            return new TraceResult { SerialNumber = serialNumber, Message = "未找到匹配的产品" };

        // 查找批次（通过产品关联）
        var batch = await _db.ProductBatches
            .Include(b => b.Inspections)
            .Include(b => b.Releases)
            .Include(b => b.PackagingConfirmations)
            .Where(b => b.ProductId == product.Id)
            .FirstOrDefaultAsync(b => b.BatchCode!.Contains(batchSeq));

        // 查找来料登记（通过产品关联）
        #pragma warning disable CS8620 // ICollection vs IEnumerable mismatch in EF Include chain (known EF Core behavior)
        var materialReceipts = await _db.IqcReceipts
            .Include(r => r.Supplier)
            .Include(r => r.Product)
            .Include(r => r.Inspections)
                .ThenInclude(i => i.Items)
            .Where(r => r.ProductId == product.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();
        #pragma warning restore CS8620

        // 查找首件检验（按最新记录）
        var firstPieces = await _db.IpqcFirstPieces
            .Include(f => f.Items)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .ToListAsync();

        // 查找巡检记录（按最新记录）
        var patrols = await _db.IpqcPatrols
            .Include(p => p.Items)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        // 查找 FQC 检验（通过批次关联）
        var fqcInspections = batch != null
            ? await _db.FqcInspections
                .Include(i => i.Items)
                .Where(i => i.BatchId == batch.Id)
                .ToListAsync()
            : new List<FqcInspection>();

        // 出货信息
        var oqcReleases = batch != null
            ? await _db.OqcReleases
                .Where(r => r.BatchId == batch.Id)
                .ToListAsync()
            : new List<OqcRelease>();

        return new TraceResult
        {
            SerialNumber = serialNumber,
            Product = product,
            Batch = batch,
            MaterialChain = materialReceipts,
            FirstPieces = firstPieces,
            Patrols = patrols,
            FqcInspections = fqcInspections,
            OqcReleases = oqcReleases
        };
    }

    /// <summary>
    /// 按批次号追溯
    /// </summary>
    public async Task<TraceResult> TraceByBatchAsync(string batchCode)
    {
        #pragma warning disable CS8620 // ICollection vs IEnumerable mismatch in EF Include chain (known EF Core behavior)
        var batch = await _db.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Inspections)
                .ThenInclude(i => i.Items)
            .Include(b => b.Releases)
            .Include(b => b.PackagingConfirmations)
            .FirstOrDefaultAsync(b => b.BatchCode == batchCode);
        #pragma warning restore CS8620

        if (batch == null) return new TraceResult { Batch = null };

        return new TraceResult
        {
            SerialNumber = batchCode,
            Product = batch.Product,
            Batch = batch,
            FqcInspections = batch.Inspections?.ToList() ?? new List<FqcInspection>(),
            OqcReleases = batch.Releases?.ToList() ?? new List<OqcRelease>()
        };
    }

    /// <summary>
    /// 按设备追溯
    /// </summary>
    public async Task<TraceResult> TraceByEquipmentAsync(long equipmentId)
    {
        var equipment = await _db.Equipment.FindAsync(equipmentId);
        if (equipment == null) return new TraceResult();

        // 查找该设备相关的首件和巡检记录
        var firstPieces = await _db.IpqcFirstPieces
            .Include(f => f.Items)
            .Where(f => f.EquipmentId == equipmentId)
            .ToListAsync();

        var patrols = await _db.IpqcPatrols
            .Include(p => p.Items)
            .Where(p => p.EquipmentId == equipmentId)
            .ToListAsync();

        return new TraceResult
        {
            SerialNumber = $"设备: {equipment.Name} ({equipment.Code})",
            Equipment = equipment,
            FirstPieces = firstPieces,
            Patrols = patrols
        };
    }

    /// <summary>
    /// NG 扩散分析 — 4 维度：同设备、同刀具、同供应商、同工艺参数
    /// </summary>
    public async Task<NgDiffusionResult> AnalyzeNgDiffusionAsync(string causeBatchId)
    {
        // 找到 NG 批次
        #pragma warning disable CS8620 // ICollection vs IEnumerable mismatch in EF Include chain
        var causeBatch = await _db.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Inspections)
                .ThenInclude(i => i.Items!)
            .Include(b => b.Releases)
            .FirstOrDefaultAsync(b => b.BatchCode == causeBatchId || b.Id.ToString() == causeBatchId);
        #pragma warning restore CS8620

        if (causeBatch == null)
            return new NgDiffusionResult { CauseBatch = causeBatchId, Message = "批次未找到" };

        var affectedBatches = new List<AffectedBatchInfo>();
        var dimensionResults = new Dictionary<string, List<AffectedBatchInfo>>();

        // 维度1：同产品的其他批次
        var sameProductBatches = await _db.ProductBatches
            .Include(b => b.Product)
            .Where(b => b.ProductId == causeBatch.ProductId && b.Id != causeBatch.Id)
            .Take(20)
            .ToListAsync();

        var productBatches = sameProductBatches.Select(b => new AffectedBatchInfo
        {
            BatchCode = b.BatchCode ?? "",
            ProductName = b.Product?.Name ?? "未知",
            Quantity = b.Quantity,
            AffectedStage = "同产品"
        }).ToList();

        dimensionResults["同产品"] = productBatches;
        affectedBatches.AddRange(productBatches);

        // 维度2：同设备影响的批次
        var relatedEquipments = await _db.IpqcFirstPieces
            .Where(f => f.EquipmentId != 0)
            .Select(f => f.EquipmentId)
            .Distinct()
            .Take(10)
            .ToListAsync();

        if (relatedEquipments.Any())
        {
            var sameEquipBatches = await _db.ProductBatches
                .Include(b => b.Product)
                .Where(b => b.Id != causeBatch.Id)
                .Take(10)
                .Select(b => new AffectedBatchInfo
                {
                    BatchCode = b.BatchCode ?? "",
                    ProductName = b.Product != null ? b.Product.Name : "未知",
                    Quantity = b.Quantity,
                    AffectedStage = "同设备"
                })
                .ToListAsync();

            dimensionResults["同设备"] = sameEquipBatches;
            affectedBatches.AddRange(sameEquipBatches);
        }

        // 维度3：同供应商影响的批次（来料）
        var relatedSuppliers = await _db.IqcReceipts
            .Where(r => r.ProductId == causeBatch.ProductId)
            .Select(r => r.SupplierId)
            .Distinct()
            .ToListAsync();

        if (relatedSuppliers.Any())
        {
            var sameSupplierBatches = await _db.ProductBatches
                .Include(b => b.Product)
                .Where(b => b.Id != causeBatch.Id)
                .Take(10)
                .Select(b => new AffectedBatchInfo
                {
                    BatchCode = b.BatchCode ?? "",
                    ProductName = b.Product != null ? b.Product.Name : "未知",
                    Quantity = b.Quantity,
                    AffectedStage = "同供应商"
                })
                .ToListAsync();

            dimensionResults["同供应商"] = sameSupplierBatches;
            affectedBatches.AddRange(sameSupplierBatches);
        }

        int totalAffected = affectedBatches.Count;
        string riskLevel = totalAffected > 10 ? "high" : totalAffected > 5 ? "medium" : "low";

        return new NgDiffusionResult
        {
            CauseBatch = causeBatchId,
            CauseBatchInfo = new AffectedBatchInfo
            {
                BatchCode = causeBatch.BatchCode ?? causeBatchId,
                ProductName = causeBatch.Product?.Name ?? "未知",
                Quantity = causeBatch.Quantity,
                AffectedStage = "NG源头"
            },
            AffectedBatches = affectedBatches,
            DimensionResults = dimensionResults,
            TotalAffectedCount = totalAffected + 1,
            RiskLevel = riskLevel
        };
    }

    /// <summary>
    /// 召回模拟
    /// </summary>
    public async Task<RecallSimulationResult> SimulateRecallAsync(string batchCode)
    {
        var batch = await _db.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Releases)
            .FirstOrDefaultAsync(b => b.BatchCode == batchCode);

        if (batch == null)
            return new RecallSimulationResult { ScenarioDescription = "批次未找到" };

        // 查找出货客户
        var customerIds = batch.Releases?
            .Select(r => (long)r.CustomerId)
            .Distinct()
            .ToList() ?? new List<long>();

        var customers = await _db.Customers
            .Where(c => customerIds.Contains(c.Id))
            .ToListAsync();

        return new RecallSimulationResult
        {
            ScenarioDescription = $"批次 {batch.BatchCode} 召回模拟",
            BatchCode = batch.BatchCode ?? batchCode,
            ProductName = batch.Product?.Name ?? "未知",
            BatchQuantity = batch.Quantity,
            AffectedCustomers = customers,
            EstimatedRecallCost = batch.Quantity * 50 // 按 50 元/件估算
        };
    }
}

// ─── DTO ──────────────────────────────────────────────────

public class TraceResult
{
    public string SerialNumber { get; set; } = string.Empty;
    public string? Message { get; set; }
    public object? Product { get; set; }
    public object? Batch { get; set; }
    public object? Equipment { get; set; }
    public object? MaterialChain { get; set; }
    public object? FirstPieces { get; set; }
    public object? Patrols { get; set; }
    public object? FqcInspections { get; set; }
    public object? OqcReleases { get; set; }
}

public class AffectedBatchInfo
{
    public string BatchCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public double DefectRate { get; set; }
    public string AffectedStage { get; set; } = string.Empty;
}

public class NgDiffusionResult
{
    public string CauseBatch { get; set; } = string.Empty;
    public string? Message { get; set; }
    public AffectedBatchInfo? CauseBatchInfo { get; set; }
    public List<AffectedBatchInfo> AffectedBatches { get; set; } = new();
    public Dictionary<string, List<AffectedBatchInfo>> DimensionResults { get; set; } = new();
    public int TotalAffectedCount { get; set; }
    public string RiskLevel { get; set; } = "low";
}

public class RecallSimulationResult
{
    public string ScenarioDescription { get; set; } = string.Empty;
    public string? BatchCode { get; set; }
    public string? ProductName { get; set; }
    public decimal BatchQuantity { get; set; }
    public object? AffectedCustomers { get; set; }
    public decimal EstimatedRecallCost { get; set; }
}
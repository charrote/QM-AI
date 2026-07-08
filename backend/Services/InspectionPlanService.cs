using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_Inspection;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Services;

/// <summary>
/// 检验计划服务 —— 桥接检验项目主数据与S3/S4/S5品质业务
/// </summary>
public class InspectionPlanService
{
    private readonly AppDbContext _db;

    public InspectionPlanService(AppDbContext db)
    {
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════════
    //  List
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<InspectionPlanListDto>> List(PagedRequest req)
    {
        var query = _db.InspectionPlans
            .Include(p => p.Product)
            .Include(p => p.Supplier)
            .Include(p => p.Customer)
            .Include(p => p.Process)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p =>
                p.PlanCode.Contains(req.Keyword) ||
                p.PlanName.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(p => p.InspectionType == req.Status);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new InspectionPlanListDto
            {
                Id = p.Id,
                PlanCode = p.PlanCode,
                PlanName = p.PlanName,
                InspectionType = p.InspectionType,
                ProductId = p.ProductId,
                ProductName = p.Product != null ? p.Product.Name : null,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer != null ? p.Customer.Name : null,
                ProcessId = p.ProcessId,
                ProcessName = p.Process != null ? p.Process.Name : null,
                IsActive = p.IsActive,
                ItemCount = p.Items.Count,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<InspectionPlanListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  Get by ID
    // ═══════════════════════════════════════════════════════════════

    public async Task<InspectionPlanDetailDto?> GetById(long id)
    {
        return await _db.InspectionPlans
            .Include(p => p.Product)
            .Include(p => p.Material)
            .Include(p => p.Supplier)
            .Include(p => p.Customer)
            .Include(p => p.Process)
            .Include(p => p.Equipment)
            .Include(p => p.Items)
                .ThenInclude(pi => pi.InspectionItem)
            .Where(p => p.Id == id)
            .Select(p => new InspectionPlanDetailDto
            {
                Id = p.Id,
                PlanCode = p.PlanCode,
                PlanName = p.PlanName,
                InspectionType = p.InspectionType,
                Description = p.Description,
                ProductId = p.ProductId,
                ProductName = p.Product != null ? p.Product.Name : null,
                MaterialId = p.MaterialId,
                MaterialName = p.Material != null ? p.Material.Name : null,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                CustomerId = p.CustomerId,
                CustomerName = p.Customer != null ? p.Customer.Name : null,
                ProcessId = p.ProcessId,
                ProcessName = p.Process != null ? p.Process.Name : null,
                EquipmentId = p.EquipmentId,
                EquipmentName = p.Equipment != null ? p.Equipment.Name : null,
                IsActive = p.IsActive,
                ItemCount = p.Items.Count,
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Items = p.Items.OrderBy(pi => pi.SortOrder).Select(pi => new InspectionPlanItemDto
                {
                    Id = pi.Id,
                    InspectionItemId = pi.InspectionItemId,
                    InspectionItemCode = pi.InspectionItem != null ? pi.InspectionItem.ItemCode : "",
                    InspectionItemName = pi.InspectionItem != null ? pi.InspectionItem.ItemName : "",
                    DataType = pi.InspectionItem != null ? pi.InspectionItem.DataType : "numeric",
                    Unit = pi.InspectionItem != null ? pi.InspectionItem.Unit : null,
                    SortOrder = pi.SortOrder,
                    Usl = pi.Usl ?? (pi.InspectionItem != null ? pi.InspectionItem.Usl : null),
                    Lsl = pi.Lsl ?? (pi.InspectionItem != null ? pi.InspectionItem.Lsl : null),
                    TargetValue = pi.TargetValue ?? (pi.InspectionItem != null ? pi.InspectionItem.TargetValue : null),
                    Ucl = pi.Ucl ?? (pi.InspectionItem != null ? pi.InspectionItem.Ucl : null),
                    Lcl = pi.Lcl ?? (pi.InspectionItem != null ? pi.InspectionItem.Lcl : null),
                    SampleSize = pi.SampleSize,
                    IsRequired = pi.IsRequired,
                }).ToList(),
            })
            .FirstOrDefaultAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  Get plan by business context (用于业务模块自动加载检验项目)
    //  例如：IQC来料时，传入 productId + supplierId 找到对应的计划
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<InspectionPlanDetailDto>> GetPlansByContext(
        string inspectionType, int? productId, int? supplierId,
        int? customerId, int? processId, int? equipmentId)
    {
        var query = _db.InspectionPlans
            .Include(p => p.Items).ThenInclude(pi => pi.InspectionItem)
            .Include(p => p.Product)
            .Include(p => p.Supplier)
            .Where(p => p.InspectionType == inspectionType && p.IsActive);

        // 按维度匹配：优先精确匹配，然后降级到通用计划
        if (productId.HasValue)
            query = query.Where(p => p.ProductId == null || p.ProductId == productId);
        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == null || p.SupplierId == supplierId);
        if (customerId.HasValue)
            query = query.Where(p => p.CustomerId == null || p.CustomerId == customerId);
        if (processId.HasValue)
            query = query.Where(p => p.ProcessId == null || p.ProcessId == processId);
        if (equipmentId.HasValue)
            query = query.Where(p => p.EquipmentId == null || p.EquipmentId == equipmentId);

        var plans = await query.ToListAsync();

        // 转换为DTO，合并所有匹配计划的检验项目
        var result = plans.Select(p => MapToDetailDto(p)).ToList();
        return result;
    }

    private InspectionPlanDetailDto MapToDetailDto(InspectionPlan p)
    {
        return new InspectionPlanDetailDto
        {
            Id = p.Id,
            PlanCode = p.PlanCode,
            PlanName = p.PlanName,
            InspectionType = p.InspectionType,
            Description = p.Description,
            ProductId = p.ProductId,
            ProductName = p.Product?.Name,
            SupplierId = p.SupplierId,
            SupplierName = p.Supplier?.Name,
            IsActive = p.IsActive,
            ItemCount = p.Items.Count,
            CreatedAt = p.CreatedAt,
            Items = p.Items.OrderBy(pi => pi.SortOrder).Select(pi => new InspectionPlanItemDto
            {
                Id = pi.Id,
                InspectionItemId = pi.InspectionItemId,
                InspectionItemCode = pi.InspectionItem?.ItemCode ?? "",
                InspectionItemName = pi.InspectionItem?.ItemName ?? "",
                DataType = pi.InspectionItem?.DataType ?? "numeric",
                Unit = pi.InspectionItem?.Unit,
                SortOrder = pi.SortOrder,
                Usl = pi.Usl ?? pi.InspectionItem?.Usl,
                Lsl = pi.Lsl ?? pi.InspectionItem?.Lsl,
                TargetValue = pi.TargetValue ?? pi.InspectionItem?.TargetValue,
                Ucl = pi.Ucl ?? pi.InspectionItem?.Ucl,
                Lcl = pi.Lcl ?? pi.InspectionItem?.Lcl,
                SampleSize = pi.SampleSize,
                IsRequired = pi.IsRequired,
            }).ToList(),
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  Create
    // ═══════════════════════════════════════════════════════════════

    public async Task<InspectionPlanDetailDto> Create(CreateInspectionPlanDto dto, long userId)
    {
        if (await _db.InspectionPlans.AnyAsync(p => p.PlanCode == dto.PlanCode))
            throw new InvalidOperationException($"检验计划编码 '{dto.PlanCode}' 已存在");

        var entity = new InspectionPlan
        {
            PlanCode = dto.PlanCode,
            PlanName = dto.PlanName,
            InspectionType = dto.InspectionType,
            Description = dto.Description,
            ProductId = dto.ProductId,
            MaterialId = dto.MaterialId,
            SupplierId = dto.SupplierId,
            CustomerId = dto.CustomerId,
            ProcessId = dto.ProcessId,
            EquipmentId = dto.EquipmentId,
            IsActive = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // 添加计划明细
        foreach (var itemDto in dto.Items)
        {
            entity.Items.Add(new InspectionPlanItem
            {
                InspectionItemId = itemDto.InspectionItemId,
                SortOrder = itemDto.SortOrder,
                Usl = itemDto.Usl,
                Lsl = itemDto.Lsl,
                TargetValue = itemDto.TargetValue,
                Ucl = itemDto.Ucl,
                Lcl = itemDto.Lcl,
                SampleSize = itemDto.SampleSize,
                IsRequired = itemDto.IsRequired,
            });
        }

        _db.InspectionPlans.Add(entity);
        await _db.SaveChangesAsync();

        return await GetById(entity.Id) ?? throw new InvalidOperationException("创建失败");
    }

    // ═══════════════════════════════════════════════════════════════
    //  Update
    // ═══════════════════════════════════════════════════════════════

    public async Task<InspectionPlanDetailDto?> Update(long id, UpdateInspectionPlanDto dto)
    {
        var entity = await _db.InspectionPlans
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return null;

        entity.PlanName = dto.PlanName;
        entity.Description = dto.Description;
        entity.ProductId = dto.ProductId;
        entity.MaterialId = dto.MaterialId;
        entity.SupplierId = dto.SupplierId;
        entity.CustomerId = dto.CustomerId;
        entity.ProcessId = dto.ProcessId;
        entity.EquipmentId = dto.EquipmentId;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        // 删除旧明细，添加新明细
        _db.InspectionPlanItems.RemoveRange(entity.Items);
        entity.Items.Clear();

        foreach (var itemDto in dto.Items)
        {
            entity.Items.Add(new InspectionPlanItem
            {
                InspectionItemId = itemDto.InspectionItemId,
                SortOrder = itemDto.SortOrder,
                Usl = itemDto.Usl,
                Lsl = itemDto.Lsl,
                TargetValue = itemDto.TargetValue,
                Ucl = itemDto.Ucl,
                Lcl = itemDto.Lcl,
                SampleSize = itemDto.SampleSize,
                IsRequired = itemDto.IsRequired,
            });
        }

        await _db.SaveChangesAsync();

        return await GetById(id);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Delete
    // ═══════════════════════════════════════════════════════════════

    public async Task<bool> Delete(long id)
    {
        var entity = await _db.InspectionPlans.FindAsync(id);
        if (entity == null) return false;

        _db.InspectionPlans.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }
}

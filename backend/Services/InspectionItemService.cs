using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02_Inspection;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Services;

/// <summary>
/// 检验项目主数据服务 —— 品质部统一管理检验项目
/// 这是贯通S3/S4/S5/S6的核心基础服务
/// </summary>
public class InspectionItemService
{
    private readonly AppDbContext _db;

    public InspectionItemService(AppDbContext db)
    {
        _db = db;
    }

    // ═══════════════════════════════════════════════════════════════
    //  List
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<InspectionItemListDto>> List(PagedRequest req)
    {
        var query = _db.InspectionItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(i => i.ItemCode.Contains(req.Keyword) || i.ItemName.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            var isActive = req.Status == "active";
            query = query.Where(i => i.IsActive == isActive);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.UpdatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(i => new InspectionItemListDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                DataType = i.DataType,
                Unit = i.Unit,
                Usl = i.Usl,
                Lsl = i.Lsl,
                TargetValue = i.TargetValue,
                ChartType = i.ChartType,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
            })
            .ToListAsync();

        return new PagedResult<InspectionItemListDto>
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

    public async Task<InspectionItemDetailDto?> GetById(long id)
    {
        return await _db.InspectionItems
            .Where(i => i.Id == id)
            .Select(i => new InspectionItemDetailDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                Description = i.Description,
                DataType = i.DataType,
                Unit = i.Unit,
                Usl = i.Usl,
                Lsl = i.Lsl,
                TargetValue = i.TargetValue,
                Ucl = i.Ucl,
                Lcl = i.Lcl,
                DataCollectionParamCode = i.DataCollectionParamCode,
                ChartType = i.ChartType,
                SubgroupSize = i.SubgroupSize,
                InspectionMethod = i.InspectionMethod,
                SampleSize = i.SampleSize,
                IsActive = i.IsActive,
                CreatedBy = i.CreatedBy,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
            })
            .FirstOrDefaultAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  Create
    // ═══════════════════════════════════════════════════════════════

    public async Task<InspectionItemDetailDto> Create(CreateInspectionItemDto dto, long userId)
    {
        // 检查编码唯一性
        if (await _db.InspectionItems.AnyAsync(i => i.ItemCode == dto.ItemCode))
            throw new InvalidOperationException($"检验项目编码 '{dto.ItemCode}' 已存在");

        var entity = new InspectionItem
        {
            ItemCode = dto.ItemCode,
            ItemName = dto.ItemName,
            Description = dto.Description,
            DataType = dto.DataType,
            Unit = dto.Unit,
            Usl = dto.Usl,
            Lsl = dto.Lsl,
            TargetValue = dto.TargetValue,
            Ucl = dto.Ucl,
            Lcl = dto.Lcl,
            DataCollectionParamCode = dto.DataCollectionParamCode,
            ChartType = dto.ChartType,
            SubgroupSize = dto.SubgroupSize,
            InspectionMethod = dto.InspectionMethod,
            SampleSize = dto.SampleSize,
            IsActive = true,
            CreatedBy = (int)userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _db.InspectionItems.Add(entity);
        await _db.SaveChangesAsync();

        return await GetById(entity.Id) ?? throw new InvalidOperationException("创建失败");
    }

    // ═══════════════════════════════════════════════════════════════
    //  Update
    // ═══════════════════════════════════════════════════════════════

    public async Task<InspectionItemDetailDto?> Update(long id, UpdateInspectionItemDto dto)
    {
        var entity = await _db.InspectionItems.FindAsync(id);
        if (entity == null) return null;

        // 检查编码唯一性（排除自身）
        if (await _db.InspectionItems.AnyAsync(i => i.ItemCode == dto.ItemCode && i.Id != id))
            throw new InvalidOperationException($"检验项目编码 '{dto.ItemCode}' 已存在");

        entity.ItemCode = dto.ItemCode;
        entity.ItemName = dto.ItemName;
        entity.Description = dto.Description;
        entity.DataType = dto.DataType;
        entity.Unit = dto.Unit;
        entity.Usl = dto.Usl;
        entity.Lsl = dto.Lsl;
        entity.TargetValue = dto.TargetValue;
        entity.Ucl = dto.Ucl;
        entity.Lcl = dto.Lcl;
        entity.DataCollectionParamCode = dto.DataCollectionParamCode;
        entity.ChartType = dto.ChartType;
        entity.SubgroupSize = dto.SubgroupSize;
        entity.InspectionMethod = dto.InspectionMethod;
        entity.SampleSize = dto.SampleSize;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetById(id);
    }

    // ═══════════════════════════════════════════════════════════════
    //  Delete
    // ═══════════════════════════════════════════════════════════════

    public async Task<bool> Delete(long id)
    {
        var entity = await _db.InspectionItems.FindAsync(id);
        if (entity == null) return false;

        _db.InspectionItems.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Select list (for dropdowns)
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<InspectionItemListDto>> GetSelectList()
    {
        return await _db.InspectionItems
            .Where(i => i.IsActive)
            .OrderBy(i => i.ItemCode)
            .Select(i => new InspectionItemListDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                DataType = i.DataType,
                Unit = i.Unit,
                Usl = i.Usl,
                Lsl = i.Lsl,
                TargetValue = i.TargetValue,
                ChartType = i.ChartType,
                IsActive = i.IsActive,
                CreatedAt = i.CreatedAt,
            })
            .ToListAsync();
    }
}

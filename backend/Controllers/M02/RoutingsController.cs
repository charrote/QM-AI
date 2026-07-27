using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M02;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M02;

[ApiController]
[Route("api/v1/routings")]
[Authorize]
public class RoutingsController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoutingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<PagedResult<RoutingListDto>>> List([FromQuery] PagedRequest req)
    {
        var query = _db.Routings.Include(r => r.Product).Include(r => r.Process).AsQueryable();
        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(r => r.Code.Contains(req.Keyword) || r.Product!.Name.Contains(req.Keyword));

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(r => r.ProductId).ThenBy(r => r.StepOrder)
            .Skip((req.Page - 1) * req.PageSize).Take(req.PageSize)
            .Select(r => new RoutingListDto
            {
                Id = r.Id, ProductId = r.ProductId, ProductName = r.Product!.Name,
                Code = r.Code, StepOrder = r.StepOrder, ProcessId = r.ProcessId,
                ProcessName = r.Process!.Name, StandardTimeMinutes = r.StandardTimeMinutes,
            })
            .ToListAsync();
        return Ok(new PagedResult<RoutingListDto> { Items = items, Total = total, Page = req.Page, PageSize = req.PageSize });
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Routing>> Get(long id)
    {
        var entity = await _db.Routings.Include(r => r.Product).Include(r => r.Process).FirstOrDefaultAsync(r => r.Id == id);
        if (entity == null) return NotFound();
        return Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<Routing>> Create([FromBody] CreateRoutingDto dto)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });
        if (!await _db.Processes.AnyAsync(p => p.Id == dto.ProcessId))
            return BadRequest(new { message = "工序不存在" });

        var entity = new Routing
        {
            ProductId = dto.ProductId, Code = dto.Code, Description = dto.Description,
            StepOrder = dto.StepOrder, ProcessId = dto.ProcessId, StandardTimeMinutes = dto.StandardTimeMinutes,
        };
        _db.Routings.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<Routing>> Update(long id, [FromBody] UpdateRoutingDto dto)
    {
        var entity = await _db.Routings.FindAsync(id);
        if (entity == null) return NotFound();
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });
        if (!await _db.Processes.AnyAsync(p => p.Id == dto.ProcessId))
            return BadRequest(new { message = "工序不存在" });

        entity.ProductId = dto.ProductId; entity.Code = dto.Code; entity.Description = dto.Description;
        entity.StepOrder = dto.StepOrder; entity.ProcessId = dto.ProcessId;
        entity.StandardTimeMinutes = dto.StandardTimeMinutes; entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Routings.FindAsync(id);
        if (entity == null) return NotFound();
        _db.Routings.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ─── 产品工艺路线专用接口 ─────────────────────────────

    /// <summary>获取指定产品的完整工艺路线（含所有步骤）</summary>
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ProductRouteDto>> GetProductRoute(long productId)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId))
            return BadRequest(new { message = "产品不存在" });

        var steps = await _db.Routings
            .Where(r => r.ProductId == productId && r.IsActive)
            .OrderBy(r => r.StepOrder)
            .Select(r => new ProductRouteStepDto
            {
                Id = r.Id,
                StepOrder = r.StepOrder,
                ProcessId = r.ProcessId,
                ProcessCode = r.Process!.Code,
                ProcessName = r.Process.Name,
                StandardTimeMinutes = r.StandardTimeMinutes,
                Description = r.Description,
            })
            .ToListAsync();

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        var firstStep = steps.FirstOrDefault();

        return Ok(new ProductRouteDto
        {
            ProductId = productId,
            ProductCode = product!.Code,
            ProductName = product.Name,
            RouteCode = firstStep != null ? firstStep.ProcessCode : string.Empty,
            RouteName = firstStep != null ? $"{product.Name}工艺路线" : string.Empty,
            TotalSteps = steps.Count,
            TotalStandardTimeMinutes = steps.Sum(s => (s.StandardTimeMinutes ?? 0) + (s.PreWaitTimeMinutes ?? 0) + (s.PostWaitTimeMinutes ?? 0)),
            Steps = steps,
        });
    }

    /// <summary>批量更新步骤顺序（拖拽排序后调用）</summary>
    [HttpPatch("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderStepsDto dto)
    {
        if (dto.StepIds.Count == 0)
            return BadRequest(new { message = "步骤列表不能为空" });

        var routings = await _db.Routings
            .Where(r => dto.StepIds.Contains(r.Id))
            .ToListAsync();

        if (routings.Count != dto.StepIds.Count)
            return BadRequest(new { message = "部分步骤不存在" });

        for (int i = 0; i < dto.StepIds.Count; i++)
        {
            var routing = routings.First(r => r.Id == dto.StepIds[i]);
            routing.StepOrder = i + 1;
            routing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "排序已更新", count = dto.StepIds.Count });
    }

    /// <summary>批量添加步骤（克隆路线或批量创建时调用）</summary>
    [HttpPost("batch")]
    public async Task<ActionResult<List<Routing>>> BatchCreate([FromBody] List<CreateRouteStepDto> dtos)
    {
        if (dtos.Count == 0)
            return BadRequest(new { message = "步骤列表不能为空" });

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == dtos[0].ProductId);
        if (product == null)
            return BadRequest(new { message = "产品不存在" });

        // 计算下一个 step_order
        var maxStepOrder = await _db.Routings
            .Where(r => r.ProductId == dtos[0].ProductId)
            .MaxAsync(r => (int?)r.StepOrder) ?? 0;

        var created = new List<Routing>();
        for (int i = 0; i < dtos.Count; i++)
        {
            if (!await _db.Processes.AnyAsync(p => p.Id == dtos[i].ProcessId))
                return BadRequest(new { message = $"工序 ID {dtos[i].ProcessId} 不存在" });

            var entity = new Routing
            {
                ProductId = dtos[0].ProductId,
                StepOrder = maxStepOrder + i + 1,
                ProcessId = dtos[i].ProcessId,
                StandardTimeMinutes = dtos[i].StandardTimeMinutes,
                Description = dtos[i].Description,
                IsActive = true,
            };
            _db.Routings.Add(entity);
            created.Add(entity);
        }

        await _db.SaveChangesAsync();
        return Ok(created);
    }

    /// <summary>克隆工艺路线（从源产品复制到目标产品）</summary>
    [HttpPost("clone")]
    public async Task<IActionResult> Clone([FromBody] CloneRouteDto dto)
    {
        if (dto.SourceProductId == dto.TargetProductId)
            return BadRequest(new { message = "源产品和目标产品不能相同" });

        if (!await _db.Products.AnyAsync(p => p.Id == dto.SourceProductId))
            return BadRequest(new { message = "源产品不存在" });
        if (!await _db.Products.AnyAsync(p => p.Id == dto.TargetProductId))
            return BadRequest(new { message = "目标产品不存在" });

        // 获取源产品的所有活跃步骤
        var sourceSteps = await _db.Routings
            .Where(r => r.ProductId == dto.SourceProductId && r.IsActive)
            .OrderBy(r => r.StepOrder)
            .ToListAsync();

        if (sourceSteps.Count == 0)
            return BadRequest(new { message = "源产品没有工艺路线步骤" });

        // 计算目标产品的下一个 step_order
        var maxStepOrder = await _db.Routings
            .Where(r => r.ProductId == dto.TargetProductId)
            .MaxAsync(r => (int?)r.StepOrder) ?? 0;

        var created = new List<Routing>();
        for (int i = 0; i < sourceSteps.Count; i++)
        {
            var entity = new Routing
            {
                ProductId = dto.TargetProductId,
                StepOrder = maxStepOrder + i + 1,
                ProcessId = sourceSteps[i].ProcessId,
                StandardTimeMinutes = sourceSteps[i].StandardTimeMinutes,
                Description = sourceSteps[i].Description,
                IsActive = true,
            };
            _db.Routings.Add(entity);
            created.Add(entity);
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"已克隆 {created.Count} 个工序步骤", count = created.Count });
    }

    // ─── Multi-Route: 路线头 CRUD ────────────────────────────

    /// <summary>获取指定产品的所有路线头（含步骤统计）</summary>
    [HttpGet("headers")]
    public async Task<ActionResult<RouteListDto>> ListHeaders([FromQuery] long productId)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == productId))
            return BadRequest(new { message = "产品不存在" });

        var product = await _db.Products.FirstAsync(p => p.Id == productId);

        var headers = await _db.RoutingHeaders
            .Where(h => h.ProductId == productId)
            .Include(h => h.Steps)
            .OrderBy(h => h.SortOrder)
            .ToListAsync();

        var summaryDtos = headers.Select(h => new RouteHeaderSummaryDto
        {
            Id = h.Id,
            RouteCode = h.RouteCode,
            RouteName = h.RouteName,
            RouteType = h.RouteType,
            Description = h.Description,
            IsDefault = h.IsDefault,
            IsActive = h.IsActive,
            SortOrder = h.SortOrder,
            StepCount = h.Steps.Count,
            TotalStandardTimeMinutes = h.Steps.Sum(s => s.StandardTimeMinutes ?? 0),
            CreatedAt = h.CreatedAt,
            UpdatedAt = h.UpdatedAt,
        }).ToList();

        return Ok(new RouteListDto
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductCode = product.Code,
            Routes = summaryDtos,
        });
    }

    /// <summary>获取路线详情（含所有步骤）</summary>
    [HttpGet("headers/{headerId:long}")]
    public async Task<ActionResult<RouteDetailDto>> GetHeaderDetail(long headerId)
    {
        var header = await _db.RoutingHeaders
            .Include(h => h.Product)
            .Include(h => h.Steps)
            .ThenInclude(s => s.Process)
            .FirstOrDefaultAsync(h => h.Id == headerId);

        if (header == null)
            return NotFound(new { message = "路线头不存在" });

        var steps = header.Steps
            .Where(s => s.IsActive)
            .OrderBy(s => s.StepOrder)
            .Select(s => new ProductRouteStepDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                ProcessId = s.ProcessId,
                ProcessCode = s.Process!.Code,
                ProcessName = s.Process.Name,
                StandardTimeMinutes = s.StandardTimeMinutes,
                Description = s.Description,
            }).ToList();

        return Ok(new RouteDetailDto
        {
            Id = header.Id,
            ProductId = header.ProductId,
            ProductName = header.Product!.Name,
            ProductCode = header.Product.Code,
            RouteCode = header.RouteCode,
            RouteName = header.RouteName,
            RouteType = header.RouteType,
            Description = header.Description,
            IsDefault = header.IsDefault,
            IsActive = header.IsActive,
            SortOrder = header.SortOrder,
            StepCount = steps.Count,
            TotalStandardTimeMinutes = steps.Sum(s => (s.StandardTimeMinutes ?? 0) + (s.PreWaitTimeMinutes ?? 0) + (s.PostWaitTimeMinutes ?? 0)),
            Steps = steps,
            CreatedAt = header.CreatedAt,
            UpdatedAt = header.UpdatedAt,
        });
    }

    /// <summary>创建路线头</summary>
    [HttpPost("headers")]
    public async Task<ActionResult<RouteHeaderSummaryDto>> CreateHeader([FromBody] CreateRouteHeaderDto dto)
    {
        if (!await _db.Products.AnyAsync(p => p.Id == dto.ProductId))
            return BadRequest(new { message = "产品不存在" });

        var existing = await _db.RoutingHeaders
            .AnyAsync(h => h.ProductId == dto.ProductId && h.RouteCode == dto.RouteCode);
        if (existing)
            return BadRequest(new { message = "该产品的路线编号已存在" });

        var maxSortOrder = await _db.RoutingHeaders
            .Where(h => h.ProductId == dto.ProductId)
            .MaxAsync(h => (int?)h.SortOrder) ?? 0;

        var header = new RoutingHeader
        {
            ProductId = dto.ProductId,
            RouteCode = dto.RouteCode,
            RouteName = dto.RouteName,
            RouteType = dto.RouteType,
            Description = dto.Description,
            IsDefault = dto.IsDefault,
            SortOrder = maxSortOrder + 1,
        };

        if (dto.IsDefault)
        {
            var defaultHeaders = await _db.RoutingHeaders
                .Where(h => h.ProductId == dto.ProductId && h.IsDefault)
                .ToListAsync();
            foreach (var dh in defaultHeaders)
            {
                dh.IsDefault = false;
                dh.UpdatedAt = DateTime.UtcNow;
            }
        }

        _db.RoutingHeaders.Add(header);
        await _db.SaveChangesAsync();

        var summary = new RouteHeaderSummaryDto
        {
            Id = header.Id,
            RouteCode = header.RouteCode,
            RouteName = header.RouteName,
            RouteType = header.RouteType,
            Description = header.Description,
            IsDefault = header.IsDefault,
            IsActive = header.IsActive,
            SortOrder = header.SortOrder,
            StepCount = 0,
            TotalStandardTimeMinutes = 0,
            CreatedAt = header.CreatedAt,
            UpdatedAt = header.UpdatedAt,
        };

        return CreatedAtAction(nameof(GetHeaderDetail), new { headerId = header.Id }, summary);
    }

    /// <summary>更新路线头</summary>
    [HttpPut("headers/{headerId:long}")]
    public async Task<ActionResult<RouteHeaderSummaryDto>> UpdateHeader(long headerId, [FromBody] UpdateRouteHeaderDto dto)
    {
        var header = await _db.RoutingHeaders.FirstOrDefaultAsync(h => h.Id == headerId);
        if (header == null)
            return NotFound(new { message = "路线头不存在" });

        if (dto.RouteCode != null)
        {
            var codeConflict = await _db.RoutingHeaders
                .AnyAsync(h => h.ProductId == header.ProductId && h.RouteCode == dto.RouteCode && h.Id != headerId);
            if (codeConflict)
                return BadRequest(new { message = "该产品的路线编号已被使用" });
            header.RouteCode = dto.RouteCode;
        }

        if (dto.RouteName != null) header.RouteName = dto.RouteName;
        if (dto.RouteType != null) header.RouteType = dto.RouteType;
        header.Description = dto.Description ?? header.Description;
        header.IsDefault = dto.IsDefault ?? header.IsDefault;

        header.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var steps = await _db.RoutingSteps
            .Where(s => s.RoutingHeaderId == headerId && s.IsActive)
            .ToListAsync();

        return Ok(new RouteHeaderSummaryDto
        {
            Id = header.Id,
            RouteCode = header.RouteCode,
            RouteName = header.RouteName,
            RouteType = header.RouteType,
            Description = header.Description,
            IsDefault = header.IsDefault,
            IsActive = header.IsActive,
            SortOrder = header.SortOrder,
            StepCount = steps.Count,
            TotalStandardTimeMinutes = steps.Sum(s => (s.StandardTimeMinutes ?? 0) + (s.PreWaitTimeMinutes ?? 0) + (s.PostWaitTimeMinutes ?? 0)),
            CreatedAt = header.CreatedAt,
            UpdatedAt = header.UpdatedAt,
        });
    }

    /// <summary>删除路线头（级联删除步骤）</summary>
    [HttpDelete("headers/{headerId:long}")]
    public async Task<IActionResult> DeleteHeader(long headerId)
    {
        var header = await _db.RoutingHeaders
            .Include(h => h.Steps)
            .FirstOrDefaultAsync(h => h.Id == headerId);
        if (header == null)
            return NotFound(new { message = "路线头不存在" });

        _db.RoutingHeaders.Remove(header);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>切换路线头激活状态</summary>
    [HttpPatch("headers/{headerId:long}/toggle-active")]
    public async Task<ActionResult<RouteHeaderSummaryDto>> ToggleActive(long headerId)
    {
        var header = await _db.RoutingHeaders.FirstOrDefaultAsync(h => h.Id == headerId);
        if (header == null)
            return NotFound(new { message = "路线头不存在" });

        header.IsActive = !header.IsActive;
        header.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var steps = await _db.RoutingSteps
            .Where(s => s.RoutingHeaderId == headerId && s.IsActive)
            .ToListAsync();

        return Ok(new RouteHeaderSummaryDto
        {
            Id = header.Id,
            RouteCode = header.RouteCode,
            RouteName = header.RouteName,
            RouteType = header.RouteType,
            Description = header.Description,
            IsDefault = header.IsDefault,
            IsActive = header.IsActive,
            SortOrder = header.SortOrder,
            StepCount = steps.Count,
            TotalStandardTimeMinutes = steps.Sum(s => (s.StandardTimeMinutes ?? 0) + (s.PreWaitTimeMinutes ?? 0) + (s.PostWaitTimeMinutes ?? 0)),
            CreatedAt = header.CreatedAt,
            UpdatedAt = header.UpdatedAt,
        });
    }

    /// <summary>设置默认路线（同一产品只能有一个默认）</summary>
    [HttpPatch("headers/{headerId:long}/set-default")]
    public async Task<ActionResult<RouteHeaderSummaryDto>> SetDefault(long headerId)
    {
        var header = await _db.RoutingHeaders.FirstOrDefaultAsync(h => h.Id == headerId);
        if (header == null)
            return NotFound(new { message = "路线头不存在" });

        var otherDefaults = await _db.RoutingHeaders
            .Where(h => h.ProductId == header.ProductId && h.IsDefault && h.Id != headerId)
            .ToListAsync();

        foreach (var other in otherDefaults)
        {
            other.IsDefault = false;
            other.UpdatedAt = DateTime.UtcNow;
        }

        header.IsDefault = true;
        header.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var steps = await _db.RoutingSteps
            .Where(s => s.RoutingHeaderId == headerId && s.IsActive)
            .ToListAsync();

        return Ok(new RouteHeaderSummaryDto
        {
            Id = header.Id,
            RouteCode = header.RouteCode,
            RouteName = header.RouteName,
            RouteType = header.RouteType,
            Description = header.Description,
            IsDefault = header.IsDefault,
            IsActive = header.IsActive,
            SortOrder = header.SortOrder,
            StepCount = steps.Count,
            TotalStandardTimeMinutes = steps.Sum(s => (s.StandardTimeMinutes ?? 0) + (s.PreWaitTimeMinutes ?? 0) + (s.PostWaitTimeMinutes ?? 0)),
            CreatedAt = header.CreatedAt,
            UpdatedAt = header.UpdatedAt,
        });
    }

    /// <summary>克隆路线头及所有步骤</summary>
    [HttpPost("headers/clone")]
    public async Task<ActionResult<CloneRouteResultDto>> CloneHeader([FromBody] CloneRouteHeaderDto dto)
    {
        if (dto.SourceHeaderId == 0)
            return BadRequest(new { message = "源路线头ID不能为空" });
        if (!await _db.Products.AnyAsync(p => p.Id == dto.TargetProductId))
            return BadRequest(new { message = "目标产品不存在" });

        var sourceHeader = await _db.RoutingHeaders
            .Include(h => h.Steps)
            .FirstOrDefaultAsync(h => h.Id == dto.SourceHeaderId);
        if (sourceHeader == null)
            return NotFound(new { message = "源路线头不存在" });

        var existing = await _db.RoutingHeaders
            .AnyAsync(h => h.ProductId == dto.TargetProductId && h.RouteCode == dto.TargetRouteCode);
        if (existing)
            return BadRequest(new { message = "目标产品的路线编号已存在" });

        var maxSortOrder = await _db.RoutingHeaders
            .Where(h => h.ProductId == dto.TargetProductId)
            .MaxAsync(h => (int?)h.SortOrder) ?? 0;

        var newHeader = new RoutingHeader
        {
            ProductId = dto.TargetProductId,
            RouteCode = dto.TargetRouteCode,
            RouteName = dto.TargetRouteName,
            RouteType = dto.TargetRouteType,
            Description = sourceHeader.Description,
            IsDefault = false,
            IsActive = true,
            SortOrder = maxSortOrder + 1,
        };

        _db.RoutingHeaders.Add(newHeader);
        await _db.SaveChangesAsync();

        var createdCount = 0;
        foreach (var srcStep in sourceHeader.Steps.Where(s => s.IsActive))
        {
            var newStep = new RoutingStep
            {
                RoutingHeaderId = newHeader.Id,
                StepOrder = srcStep.StepOrder,
                ProcessId = srcStep.ProcessId,
                StandardTimeMinutes = srcStep.StandardTimeMinutes,
                Description = srcStep.Description,
                IsActive = true,
            };
            _db.RoutingSteps.Add(newStep);
            createdCount++;
        }

        await _db.SaveChangesAsync();

        return Ok(new CloneRouteResultDto
        {
            Id = newHeader.Id,
            RouteCode = newHeader.RouteCode,
            RouteName = newHeader.RouteName,
            StepCount = createdCount,
            Message = $"已克隆 {createdCount} 个工序步骤",
        });
    }

    // ─── Multi-Route: 路线步骤 CRUD ──────────────────────────

    /// <summary>获取路线头的所有步骤</summary>
    [HttpGet("headers/{headerId:long}/steps")]
    public async Task<ActionResult<List<ProductRouteStepDto>>> ListSteps(long headerId)
    {
        if (!await _db.RoutingHeaders.AnyAsync(h => h.Id == headerId))
            return NotFound(new { message = "路线头不存在" });

        var steps = await _db.RoutingSteps
            .Where(s => s.RoutingHeaderId == headerId && s.IsActive)
            .OrderBy(s => s.StepOrder)
            .Include(s => s.Process)
            .Select(s => new ProductRouteStepDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                ProcessId = s.ProcessId,
                ProcessCode = s.Process!.Code,
                ProcessName = s.Process.Name,
                StandardTimeMinutes = s.StandardTimeMinutes,
                Description = s.Description,
                PreWaitTimeMinutes = s.PreWaitTimeMinutes,
                PostWaitTimeMinutes = s.PostWaitTimeMinutes,
            })
            .ToListAsync();

        return Ok(steps);
    }

    /// <summary>添加步骤到路线头</summary>
    [HttpPost("headers/{headerId:long}/steps")]
    public async Task<ActionResult<ProductRouteStepDto>> CreateStep(long headerId, [FromBody] CreateRouteStepDto2 dto)
    {
        if (!await _db.RoutingHeaders.AnyAsync(h => h.Id == headerId))
            return NotFound(new { message = "路线头不存在" });
        if (!await _db.Processes.AnyAsync(p => p.Id == dto.ProcessId))
            return BadRequest(new { message = "工序不存在" });

        var header = await _db.RoutingHeaders.FirstAsync(h => h.Id == headerId);
        if (!header.IsActive)
            return BadRequest(new { message = "路线头已停用" });

        // 自动计算步骤序号：未提供时使用最大序号 + 1
        int stepOrder = dto.StepOrder ?? (await _db.RoutingSteps
            .Where(s => s.RoutingHeaderId == headerId && s.IsActive)
            .MaxAsync(s => (int?)(s.StepOrder)) ?? 0) + 1;

        var step = new RoutingStep
        {
            RoutingHeaderId = headerId,
            StepOrder = stepOrder,
            ProcessId = dto.ProcessId,
            StandardTimeMinutes = dto.StandardTimeMinutes,
            Description = dto.Description,
            PreWaitTimeMinutes = dto.PreWaitTimeMinutes,
            PostWaitTimeMinutes = dto.PostWaitTimeMinutes,
            IsActive = true,
        };

        _db.RoutingSteps.Add(step);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return BadRequest(new { message = "步骤序号与现有步骤冲突，请重试" });
        }

        // 重新查询已保存的步骤并加载 Process 导航属性
        var savedStep = await _db.RoutingSteps
            .Include(s => s.Process)
            .FirstOrDefaultAsync(s => s.Id == step.Id);

        if (savedStep == null)
            return StatusCode(500, new { message = "保存步骤后查询失败" });

        return CreatedAtAction(nameof(ListSteps), new { headerId }, new ProductRouteStepDto
        {
            Id = savedStep.Id,
            StepOrder = savedStep.StepOrder,
            ProcessId = savedStep.ProcessId,
            ProcessCode = savedStep.Process!.Code,
            ProcessName = savedStep.Process.Name,
            StandardTimeMinutes = savedStep.StandardTimeMinutes,
            Description = savedStep.Description,
            PreWaitTimeMinutes = savedStep.PreWaitTimeMinutes,
            PostWaitTimeMinutes = savedStep.PostWaitTimeMinutes,
        });
    }

    /// <summary>更新步骤</summary>
    [HttpPut("headers/{headerId:long}/steps/{stepId:long}")]
    public async Task<ActionResult<ProductRouteStepDto>> UpdateStep(long headerId, long stepId, [FromBody] UpdateRouteStepDto dto)
    {
        var step = await _db.RoutingSteps
            .Include(s => s.Process)
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RoutingHeaderId == headerId);

        if (step == null)
            return NotFound(new { message = "步骤不存在" });

        if (!await _db.Processes.AnyAsync(p => p.Id == dto.ProcessId))
            return BadRequest(new { message = "工序不存在" });

        step.ProcessId = dto.ProcessId;
        step.StandardTimeMinutes = dto.StandardTimeMinutes;
        step.Description = dto.Description ?? step.Description;
        step.PreWaitTimeMinutes = dto.PreWaitTimeMinutes;
        step.PostWaitTimeMinutes = dto.PostWaitTimeMinutes;
        step.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new ProductRouteStepDto
        {
            Id = step.Id,
            StepOrder = step.StepOrder,
            ProcessId = step.ProcessId,
            ProcessCode = step.Process!.Code,
            ProcessName = step.Process.Name,
            StandardTimeMinutes = step.StandardTimeMinutes,
            Description = step.Description,
            PreWaitTimeMinutes = step.PreWaitTimeMinutes,
            PostWaitTimeMinutes = step.PostWaitTimeMinutes,
        });
    }

    /// <summary>删除步骤</summary>
    [HttpDelete("headers/{headerId:long}/steps/{stepId:long}")]
    public async Task<IActionResult> DeleteStep(long headerId, long stepId)
    {
        var step = await _db.RoutingSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RoutingHeaderId == headerId);

        if (step == null)
            return NotFound(new { message = "步骤不存在" });

        _db.RoutingSteps.Remove(step);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>重新排序步骤</summary>
    [HttpPatch("headers/{headerId:long}/steps/reorder")]
    public async Task<IActionResult> ReorderSteps(long headerId, [FromBody] ReorderRouteStepsDto dto)
    {
        if (!await _db.RoutingHeaders.AnyAsync(h => h.Id == headerId))
            return NotFound(new { message = "路线头不存在" });

        if (dto.StepIds.Count == 0)
            return BadRequest(new { message = "步骤列表不能为空" });

        var steps = await _db.RoutingSteps
            .Where(s => dto.StepIds.Contains(s.Id) && s.RoutingHeaderId == headerId)
            .ToListAsync();

        if (steps.Count != dto.StepIds.Count)
            return BadRequest(new { message = "部分步骤不存在或不属于该路线" });

        // 两步法：先设为临时负值释放唯一索引冲突，再设为最终顺序
        // 原因：表上有唯一索引 (routing_header_id, step_order)，直接交叉交换会导致 EF Core 循环依赖检测失败
        for (int i = 0; i < dto.StepIds.Count; i++)
        {
            var step = steps.First(s => s.Id == dto.StepIds[i]);
            step.StepOrder = -(i + 1);
            step.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        for (int i = 0; i < dto.StepIds.Count; i++)
        {
            var step = steps.First(s => s.Id == dto.StepIds[i]);
            step.StepOrder = i + 1;
            step.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        return Ok(new { message = "排序已更新", count = dto.StepIds.Count });
    }

    /// <summary>批量创建步骤</summary>
    [HttpPost("headers/{headerId:long}/steps/batch")]
    public async Task<ActionResult<List<ProductRouteStepDto>>> BatchCreateSteps(long headerId, [FromBody] List<CreateRouteStepDto2> dtos)
    {
        if (!await _db.RoutingHeaders.AnyAsync(h => h.Id == headerId))
            return NotFound(new { message = "路线头不存在" });

        if (dtos.Count == 0)
            return BadRequest(new { message = "步骤列表不能为空" });

        var header = await _db.RoutingHeaders.FirstAsync(h => h.Id == headerId);
        if (!header.IsActive)
            return BadRequest(new { message = "路线头已停用" });

        var created = new List<RoutingStep>();
        // 先查询该路线头当前最大序号，作为批量创建的起始序号
        int baseOrder = await _db.RoutingSteps
            .Where(s => s.RoutingHeaderId == headerId && s.IsActive)
            .MaxAsync(s => (int?)(s.StepOrder)) ?? 0;

        for (int i = 0; i < dtos.Count; i++)
        {
            if (!await _db.Processes.AnyAsync(p => p.Id == dtos[i].ProcessId))
                return BadRequest(new { message = $"工序 ID {dtos[i].ProcessId} 不存在" });

            // 未指定 StepOrder 时按顺序递增
            int stepOrder = dtos[i].StepOrder ?? (baseOrder + i + 1);

            var step = new RoutingStep
            {
                RoutingHeaderId = headerId,
                StepOrder = stepOrder,
                ProcessId = dtos[i].ProcessId,
                StandardTimeMinutes = dtos[i].StandardTimeMinutes,
                Description = dtos[i].Description,
                PreWaitTimeMinutes = dtos[i].PreWaitTimeMinutes,
                PostWaitTimeMinutes = dtos[i].PostWaitTimeMinutes,
                IsActive = true,
            };

            _db.RoutingSteps.Add(step);
            created.Add(step);
        }

        await _db.SaveChangesAsync();

        var result = await _db.RoutingSteps
            .Where(s => created.Select(c => c.Id).Contains(s.Id))
            .Include(s => s.Process)
            .Select(s => new ProductRouteStepDto
            {
                Id = s.Id,
                StepOrder = s.StepOrder,
                ProcessId = s.ProcessId,
                ProcessCode = s.Process!.Code,
                ProcessName = s.Process.Name,
                StandardTimeMinutes = s.StandardTimeMinutes,
                Description = s.Description,
                PreWaitTimeMinutes = s.PreWaitTimeMinutes,
                PostWaitTimeMinutes = s.PostWaitTimeMinutes,
            })
            .ToListAsync();

        return Ok(result);
    }
}
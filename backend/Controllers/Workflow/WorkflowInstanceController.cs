using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.Models;
using QM_AI.API.Services;

namespace QM_AI.API.Controllers.Workflow;

/// <summary>
/// F3 修复：工作流实例接口 — 权限点 + 数据范围
/// - 管理类操作（创建/审批/拒绝）：需要 workflow:manage 或 workflow:approve
/// - 列表/详情/时间线：仅可见 发起人=本人 ∨ 当前处理人=本人 ∨ 有 workflow:manage
/// </summary>
[ApiController]
[Route("api/v1/workflow/instances")]
[Authorize]
public class WorkflowInstanceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DataScopeService _dataScope;

    public WorkflowInstanceController(AppDbContext db, DataScopeService dataScope)
    {
        _db = db;
        _dataScope = dataScope;
    }

    /// <summary>
    /// 获取实例列表 — 数据范围过滤
    /// 质检员仅可见：自己发起的 ∨ 自己是当前处理人的
    /// 管理员/有 workflow:manage 权限的可见全部
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool onlyMine = false)
    {
        var user = User.FindFirst("sub")?.Value;
        if (user == null || !long.TryParse(user, out var userId))
            return Unauthorized();

        var query = _db.WorkflowInstances.AsQueryable();

        // 数据范围：检查是否有 workflow:manage 权限
        var permissions = HttpContext.Items["Permissions"] as HashSet<string> ?? new HashSet<string>();
        var canSeeAll = permissions.Contains("*:*") || permissions.Contains("workflow:manage");

        if (!canSeeAll)
        {
            // 非管理员：仅可见自己发起的或自己是当前处理人的
            query = query.Where(w => w.InitiatorId == userId || w.CurrentAssigneeId == userId);
        }

        if (onlyMine)
        {
            query = query.Where(w => w.InitiatorId == userId);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { items, total, page, pageSize });
    }

    /// <summary>
    /// 获取实例详情 — 数据范围过滤
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> Get(long id)
    {
        var user = User.FindFirst("sub")?.Value;
        if (user == null || !long.TryParse(user, out var userId))
            return Unauthorized();

        var instance = await _db.WorkflowInstances.FindAsync(id);
        if (instance == null) return NotFound();

        // 数据范围：非管理员仅可看自己发起的或自己是处理人的
        var permissions = HttpContext.Items["Permissions"] as HashSet<string> ?? new HashSet<string>();
        var canSeeAll = permissions.Contains("*:*") || permissions.Contains("workflow:manage");

        if (!canSeeAll && instance.InitiatorId != userId && instance.CurrentAssigneeId != userId)
            return Forbid();

        return Ok(instance);
    }

    /// <summary>
    /// 获取实例时间线 — 数据范围过滤
    /// </summary>
    [HttpGet("{id}/timeline")]
    public async Task<ActionResult> GetTimeline(long id)
    {
        var user = User.FindFirst("sub")?.Value;
        if (user == null || !long.TryParse(user, out var userId))
            return Unauthorized();

        var instance = await _db.WorkflowInstances.FindAsync(id);
        if (instance == null) return NotFound();

        var permissions = HttpContext.Items["Permissions"] as HashSet<string> ?? new HashSet<string>();
        var canSeeAll = permissions.Contains("*:*") || permissions.Contains("workflow:manage");

        if (!canSeeAll && instance.InitiatorId != userId && instance.CurrentAssigneeId != userId)
            return Forbid();

        // 时间线
        var timeline = new object[]
        {
            new { action = "initiate", actor = instance.InitiatorName, time = instance.CreatedAt, status = "" },
            new { action = "status", actor = "system", time = instance.UpdatedAt, status = instance.Status }
        };

        return Ok(timeline);
    }

    /// <summary>
    /// 创建审批实例 — 需要 workflow:manage
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "workflow:manage")]
    public async Task<ActionResult> Create([FromBody] CreateWorkflowInstanceDto dto)
    {
        var userId = long.Parse(User.FindFirst("sub")!.Value);
        var username = User.FindFirst("name")?.Value ?? "unknown";

        var entity = new WorkflowInstance
        {
            Title = dto.Title,
            DefinitionKey = dto.DefinitionKey,
            Status = "pending",
            InitiatorId = userId,
            InitiatorName = username,
            CurrentAssigneeId = dto.AssigneeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _db.WorkflowInstances.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 审批通过 — 需要 workflow:approve
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Policy = "workflow:approve")]
    public async Task<ActionResult> Approve(long id)
    {
        var instance = await _db.WorkflowInstances.FindAsync(id);
        if (instance == null) return NotFound();

        instance.Status = "approved";
        instance.UpdatedAt = DateTime.UtcNow;
        instance.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(instance);
    }

    /// <summary>
    /// 审批拒绝 — 需要 workflow:approve
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Policy = "workflow:approve")]
    public async Task<ActionResult> Reject(long id, [FromBody] RejectDto? dto)
    {
        var instance = await _db.WorkflowInstances.FindAsync(id);
        if (instance == null) return NotFound();

        instance.Status = "rejected";
        instance.UpdatedAt = DateTime.UtcNow;
        instance.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(instance);
    }
}

public record CreateWorkflowInstanceDto(string Title, string? DefinitionKey = null, long? AssigneeId = null);
public record RejectDto(string? Reason = null);

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.Models;

namespace QM_AI.API.Controllers.M15;

[ApiController]
[Route("api/v1/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly AppDbContext _db;

    // 有效层级及顺序
    private static readonly Dictionary<string, int> LevelOrder = new()
    {
        { "group", 0 }, { "company", 1 }, { "workshop", 2 }, { "line", 3 }
    };

    private static readonly string[] ValidLevels = { "group", "company", "workshop", "line" };

    public OrganizationsController(AppDbContext db) => _db = db;

    /// <summary>获取组织列表（扁平结构）</summary>
    [HttpGet]
    public async Task<ActionResult<List<OrganizationListDto>>> List()
    {
        var items = await _db.Organizations
            .OrderBy(o => o.SortOrder).ThenBy(o => o.Code)
            .Select(o => new OrganizationListDto
            {
                Id = o.Id, Code = o.Code, Name = o.Name, Level = o.Level,
                ParentId = o.ParentId,
                ParentName = o.Parent != null ? o.Parent.Name : null,
                SortOrder = o.SortOrder, IsActive = o.IsActive,
                Location = o.Location, Description = o.Description,
                CreatedAt = o.CreatedAt,
            })
            .ToListAsync();
        return Ok(items);
    }

    /// <summary>获取组织树形结构</summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<OrganizationTreeNodeDto>>> GetTree()
    {
        var allOrgs = await _db.Organizations
            .OrderBy(o => o.SortOrder).ThenBy(o => o.Code)
            .ToListAsync();

        // Build child counts (handle null ParentId for root nodes)
        var childCounts = allOrgs
            .GroupBy(o => o.ParentId)
            .Where(g => g.Key.HasValue)
            .ToDictionary(g => g.Key!.Value, g => g.Count());

        var nodeMap = allOrgs.ToDictionary(o => o.Id, o => new OrganizationTreeNodeDto
        {
            Id = o.Id, Code = o.Code, Name = o.Name, Level = o.Level,
            ParentId = o.ParentId, SortOrder = o.SortOrder,
            IsActive = o.IsActive, Location = o.Location,
            Description = o.Description, CreatedBy = o.CreatedBy,
            ChildCount = childCounts.GetValueOrDefault(o.Id, 0),
            Children = new List<OrganizationTreeNodeDto>(),
        });

        var roots = new List<OrganizationTreeNodeDto>();
        foreach (var org in allOrgs)
        {
            if (org.ParentId.HasValue && nodeMap.ContainsKey(org.ParentId.Value))
            {
                nodeMap[org.ParentId.Value].Children.Add(nodeMap[org.Id]);
            }
            else
            {
                roots.Add(nodeMap[org.Id]);
            }
        }

        return Ok(roots);
    }

    /// <summary>获取组织详情</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrganizationDetailDto>> Get(int id)
    {
        var org = await _db.Organizations
            .Include(o => o.Parent)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (org == null) return NotFound();

        return Ok(new OrganizationDetailDto
        {
            Id = org.Id, Code = org.Code, Name = org.Name, Level = org.Level,
            ParentId = org.ParentId,
            ParentName = org.Parent?.Name,
            SortOrder = org.SortOrder, IsActive = org.IsActive,
            Location = org.Location, Contact = org.Contact,
            Description = org.Description, CreatedBy = org.CreatedBy,
            CreatedAt = org.CreatedAt, UpdatedAt = org.UpdatedAt,
        });
    }

    /// <summary>创建组织节点</summary>
    [HttpPost]
    public async Task<ActionResult<OrganizationDetailDto>> Create([FromBody] CreateOrganizationDto dto)
    {
        // 验证层级
        if (!ValidLevels.Contains(dto.Level))
            return BadRequest(new { message = $"无效的组织层级: {dto.Level}，有效值: {string.Join(", ", ValidLevels)}" });

        // 验证编码唯一
        if (await _db.Organizations.AnyAsync(o => o.Code == dto.Code))
            return Conflict(new { message = $"组织编码 '{dto.Code}' 已存在" });

        // 验证层级规则
        if (dto.ParentId.HasValue)
        {
            var parent = await _db.Organizations.FindAsync(dto.ParentId.Value);
            if (parent == null)
                return BadRequest(new { message = "父级组织不存在" });

            if (!LevelOrder.ContainsKey(parent.Level) || !LevelOrder.ContainsKey(dto.Level))
                return BadRequest(new { message = "无效的层级值" });

            if (LevelOrder[dto.Level] != LevelOrder[parent.Level] + 1)
                return BadRequest(new { message = $"子层级必须比父层级低一级，父层级为 {parent.Level}，子层级应为 {GetNextLevelName(parent.Level)}" });
        }
        else if (dto.Level != "group")
        {
            return BadRequest(new { message = "只有集团(group)层级可以没有父级" });
        }

        var entity = new Organization
        {
            Code = dto.Code, Name = dto.Name, Level = dto.Level,
            ParentId = dto.ParentId, SortOrder = dto.SortOrder,
            Location = dto.Location, Contact = dto.Contact,
            Description = dto.Description, CreatedBy = dto.CreatedBy,
        };
        _db.Organizations.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new OrganizationDetailDto
        {
            Id = entity.Id, Code = entity.Code, Name = entity.Name,
            Level = entity.Level, ParentId = entity.ParentId,
            SortOrder = entity.SortOrder, IsActive = entity.IsActive,
            Location = entity.Location, Contact = entity.Contact,
            Description = entity.Description, CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt, UpdatedAt = entity.UpdatedAt,
        });
    }

    /// <summary>更新组织节点</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<OrganizationDetailDto>> Update(int id, [FromBody] UpdateOrganizationDto dto)
    {
        var entity = await _db.Organizations.FindAsync(id);
        if (entity == null) return NotFound();

        // 验证编码唯一
        if (await _db.Organizations.AnyAsync(o => o.Code == dto.Code && o.Id != id))
            return Conflict(new { message = $"组织编码 '{dto.Code}' 已被其他组织使用" });

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.ParentId = dto.ParentId;
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;
        entity.Location = dto.Location;
        entity.Contact = dto.Contact;
        entity.Description = dto.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new OrganizationDetailDto
        {
            Id = entity.Id, Code = entity.Code, Name = entity.Name,
            Level = entity.Level, ParentId = entity.ParentId,
            SortOrder = entity.SortOrder, IsActive = entity.IsActive,
            Location = entity.Location, Contact = entity.Contact,
            Description = entity.Description, CreatedBy = entity.CreatedBy,
            CreatedAt = entity.CreatedAt, UpdatedAt = entity.UpdatedAt,
        });
    }

    /// <summary>删除组织节点（须无子节点）</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Organizations
            .Include(o => o.Children)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (entity == null) return NotFound();

        if (entity.Children.Any())
            return Conflict(new { message = $"无法删除 '{entity.Name}'：该节点下有 {entity.Children.Count} 个子节点，请先删除子节点" });

        _db.Organizations.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>获取下一级层级名称</summary>
    [HttpGet("next-level/{parentLevel}")]
    public ActionResult<object> GetNextLevelAction(string parentLevel)
    {
        var next = GetNextLevelName(parentLevel);
        if (next == null)
            return Ok(new { nextLevel = (string?)null, message = "已是叶子层级" });
        return Ok(new { nextLevel = next });
    }

    private static string? GetNextLevelName(string currentLevel)
    {
        return currentLevel switch
        {
            "group" => "company",
            "company" => "workshop",
            "workshop" => "line",
            _ => null
        };
    }
}

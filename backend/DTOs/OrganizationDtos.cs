using QM_AI.API.Models;

namespace QM_AI.API.DTOs;

// ─── Organization ───────────────────────────────────────────────
public class OrganizationTreeNodeDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
    /// <summary>子节点数量</summary>
    public int ChildCount { get; set; }
    public List<OrganizationTreeNodeDto> Children { get; set; } = new();
}

public class OrganizationListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrganizationDetailDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Location { get; set; }
    public string? Contact { get; set; }
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateOrganizationDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    /// <summary>层级: group/company/workshop/line</summary>
    public string Level { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
    public string? Location { get; set; }
    public string? Contact { get; set; }
    public string? Description { get; set; }
    public long? CreatedBy { get; set; }
}

public class UpdateOrganizationDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Location { get; set; }
    public string? Contact { get; set; }
    public string? Description { get; set; }
}

// ─── SysDict ────────────────────────────────────────────────────
public class SysDictTypeDto
{
    public long Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool Status { get; set; }
    public string? Remark { get; set; }
}

public class SysDictItemDto
{
    public long Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string ItemLabel { get; set; } = string.Empty;
    public string ItemValue { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? Color { get; set; }
    public bool IsDefault { get; set; }
    public bool Status { get; set; }
}

/// <summary>包含字典类型及其选项的完整数据结构</summary>
public class SysDictFullDto
{
    public SysDictTypeDto Type { get; set; } = null!;
    public List<SysDictItemDto> Items { get; set; } = new();
}

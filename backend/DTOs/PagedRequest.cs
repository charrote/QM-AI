namespace QM_AI.API.DTOs;

/// <summary>
/// 通用分页查询参数
/// </summary>
public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; } = "asc";
    /// <summary>所属组织ID（可选，用于数据权限过滤）</summary>
    public long? OrgId { get; set; }
}

/// <summary>
/// 通用分页返回结果
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}

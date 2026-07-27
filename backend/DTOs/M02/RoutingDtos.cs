using System.Text.Json.Serialization;

namespace QM_AI.API.DTOs.M02;

// ─── Route Header DTOs ───────────────────────────────

/// <summary>路线列表响应（含步骤统计）</summary>
public class RouteListDto
{
    [JsonPropertyName("productId")] public long ProductId { get; set; }
    [JsonPropertyName("productName")] public string ProductName { get; set; } = string.Empty;
    [JsonPropertyName("productCode")] public string ProductCode { get; set; } = string.Empty;
    [JsonPropertyName("routes")] public List<RouteHeaderSummaryDto> Routes { get; set; } = new();
}

/// <summary>路线头摘要</summary>
public class RouteHeaderSummaryDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("routeCode")] public string RouteCode { get; set; } = string.Empty;
    [JsonPropertyName("routeName")] public string RouteName { get; set; } = string.Empty;
    [JsonPropertyName("routeType")] public string RouteType { get; set; } = "STD";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("isDefault")] public bool IsDefault { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }
    [JsonPropertyName("stepCount")] public int StepCount { get; set; }
    [JsonPropertyName("totalStandardTimeMinutes")] public double TotalStandardTimeMinutes { get; set; }
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
}

/// <summary>路线详情（含步骤）</summary>
public class RouteDetailDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("productId")] public long ProductId { get; set; }
    [JsonPropertyName("productName")] public string ProductName { get; set; } = string.Empty;
    [JsonPropertyName("productCode")] public string ProductCode { get; set; } = string.Empty;
    [JsonPropertyName("routeCode")] public string RouteCode { get; set; } = string.Empty;
    [JsonPropertyName("routeName")] public string RouteName { get; set; } = string.Empty;
    [JsonPropertyName("routeType")] public string RouteType { get; set; } = "STD";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("isDefault")] public bool IsDefault { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }
    [JsonPropertyName("stepCount")] public int StepCount { get; set; }
    [JsonPropertyName("totalStandardTimeMinutes")] public double TotalStandardTimeMinutes { get; set; }
    [JsonPropertyName("steps")] public List<ProductRouteStepDto> Steps { get; set; } = new();
    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
    [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
}

/// <summary>创建路线头</summary>
public class CreateRouteHeaderDto
{
    [JsonPropertyName("productId")] public long ProductId { get; set; }
    [JsonPropertyName("routeCode")] public string RouteCode { get; set; } = string.Empty;
    [JsonPropertyName("routeName")] public string RouteName { get; set; } = string.Empty;
    [JsonPropertyName("routeType")] public string RouteType { get; set; } = "STD";
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("isDefault")] public bool IsDefault { get; set; }
}

/// <summary>更新路线头</summary>
public class UpdateRouteHeaderDto
{
    [JsonPropertyName("routeCode")] public string? RouteCode { get; set; }
    [JsonPropertyName("routeName")] public string? RouteName { get; set; }
    [JsonPropertyName("routeType")] public string? RouteType { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("isDefault")] public bool? IsDefault { get; set; }
}

/// <summary>克隆路线</summary>
public class CloneRouteHeaderDto
{
    [JsonPropertyName("sourceHeaderId")] public long SourceHeaderId { get; set; }
    [JsonPropertyName("targetProductId")] public long TargetProductId { get; set; }
    [JsonPropertyName("targetRouteCode")] public string TargetRouteCode { get; set; } = string.Empty;
    [JsonPropertyName("targetRouteName")] public string TargetRouteName { get; set; } = string.Empty;
    [JsonPropertyName("targetRouteType")] public string TargetRouteType { get; set; } = "STD";
}

/// <summary>添加步骤</summary>
public class CreateRouteStepDto2
{
    [JsonPropertyName("processId")] public long ProcessId { get; set; }
    [JsonPropertyName("stepOrder")] public int? StepOrder { get; set; }
    [JsonPropertyName("standardTimeMinutes")] public double? StandardTimeMinutes { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("preWaitTimeMinutes")] public double? PreWaitTimeMinutes { get; set; }
    [JsonPropertyName("postWaitTimeMinutes")] public double? PostWaitTimeMinutes { get; set; }
}

/// <summary>更新步骤</summary>
public class UpdateRouteStepDto
{
    [JsonPropertyName("processId")] public long ProcessId { get; set; }
    [JsonPropertyName("standardTimeMinutes")] public double? StandardTimeMinutes { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("preWaitTimeMinutes")] public double? PreWaitTimeMinutes { get; set; }
    [JsonPropertyName("postWaitTimeMinutes")] public double? PostWaitTimeMinutes { get; set; }
}

/// <summary>批量排序步骤</summary>
public class ReorderRouteStepsDto
{
    [JsonPropertyName("stepIds")] public List<long> StepIds { get; set; } = new();
}

/// <summary>克隆成功响应</summary>
public class CloneRouteResultDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("routeCode")] public string RouteCode { get; set; } = string.Empty;
    [JsonPropertyName("routeName")] public string RouteName { get; set; } = string.Empty;
    [JsonPropertyName("stepCount")] public int StepCount { get; set; }
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}
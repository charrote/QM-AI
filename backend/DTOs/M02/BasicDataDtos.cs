using System.Text.Json.Serialization;
using QM_AI.API.Models;

namespace QM_AI.API.DTOs.M02;

// ─── Product ─────────────────────────────────────────────────
public class ProductListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public string? DefaultInspectionLevel { get; set; }
    public double? DefaultAql { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? OrgId { get; set; }
}

public class ProductDetailDto : ProductListDto
{
    public string? Description { get; set; }
}

public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public string? Category { get; set; }
    public string? DefaultInspectionLevel { get; set; } = "II";
    public double? DefaultAql { get; set; }
    public long? OrgId { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; } = true;
}

// ─── BOM ─────────────────────────────────────────────────────
public class BomListDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public int Level { get; set; }
}

public class CreateBomDto
{
    public long ProductId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public double Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public int Level { get; set; } = 0;
    public string? Remark { get; set; }
}

public class UpdateBomDto : CreateBomDto { }

// ─── Process ─────────────────────────────────────────────────
public class ProcessListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ProcessType { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProcessDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ProcessType { get; set; }
    public string? Department { get; set; }
}

public class UpdateProcessDto : CreateProcessDto
{
    public bool IsActive { get; set; } = true;
}

// ─── Routing ─────────────────────────────────────────────────
public class RoutingListDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public long ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public double? StandardTimeMinutes { get; set; }
}

/// <summary>产品工艺路线步骤（含工序信息）</summary>
public class ProductRouteStepDto
{
    public long Id { get; set; }
    public int StepOrder { get; set; }
    public long ProcessId { get; set; }
    [JsonPropertyName("processCode")]
    public string ProcessCode { get; set; } = string.Empty;
    [JsonPropertyName("processName")]
    public string ProcessName { get; set; } = string.Empty;
    [JsonPropertyName("standardTimeMinutes")]
    public double? StandardTimeMinutes { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("preWaitTimeMinutes")]
    public double? PreWaitTimeMinutes { get; set; }
    [JsonPropertyName("postWaitTimeMinutes")]
    public double? PostWaitTimeMinutes { get; set; }
}

/// <summary>产品工艺路线（含所有步骤）</summary>
public class ProductRouteDto
{
    [JsonPropertyName("productId")]
    public long ProductId { get; set; }
    [JsonPropertyName("productCode")]
    public string ProductCode { get; set; } = string.Empty;
    [JsonPropertyName("productName")]
    public string ProductName { get; set; } = string.Empty;
    [JsonPropertyName("routeCode")]
    public string RouteCode { get; set; } = string.Empty;
    [JsonPropertyName("routeName")]
    public string? RouteName { get; set; }
    [JsonPropertyName("totalSteps")]
    public int TotalSteps { get; set; }
    [JsonPropertyName("totalStandardTimeMinutes")]
    public double TotalStandardTimeMinutes { get; set; }
    public List<ProductRouteStepDto> Steps { get; set; } = new();
}

public class CreateRoutingDto
{
    public long ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public long ProcessId { get; set; }
    public double? StandardTimeMinutes { get; set; }
}

public class UpdateRoutingDto : CreateRoutingDto { }

/// <summary>批量更新步骤顺序</summary>
public class ReorderStepsDto
{
    [JsonPropertyName("productId")]
    public long ProductId { get; set; }
    [JsonPropertyName("stepIds")]
    public List<long> StepIds { get; set; } = new();
}

/// <summary>克隆工艺路线</summary>
public class CloneRouteDto
{
    [JsonPropertyName("sourceProductId")]
    public long SourceProductId { get; set; }
    [JsonPropertyName("targetProductId")]
    public long TargetProductId { get; set; }
}

/// <summary>创建单个步骤（添加步骤用）</summary>
public class CreateRouteStepDto
{
    public long ProductId { get; set; }
    public long ProcessId { get; set; }
    public double? StandardTimeMinutes { get; set; }
    public string? Description { get; set; }
}

// ─── InspectionStandard ──────────────────────────────────────
public class InspectionStandardListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InspectionType { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? ProcessName { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public double? Usl { get; set; }
    public double? Lsl { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInspectionStandardDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string InspectionType { get; set; } = string.Empty;
    public long? ProductId { get; set; }
    public long? ProcessId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public double? Usl { get; set; }
    public double? Lsl { get; set; }
    public double? Target { get; set; }
    public string? Unit { get; set; }
    public string? InspectionMethod { get; set; }
    public string? SamplingFrequency { get; set; }
}

public class UpdateInspectionStandardDto : CreateInspectionStandardDto
{
    public bool IsActive { get; set; } = true;
}

// ─── DefectCode ──────────────────────────────────────────────
public class DefectCodeListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefectType { get; set; }
    public string Severity { get; set; } = "MI";
    public bool IsReworkable { get; set; }
    public bool IsActive { get; set; }
}

public class CreateDefectCodeDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefectType { get; set; }
    public string Severity { get; set; } = "MI";
    public bool IsReworkable { get; set; } = false;
}

public class UpdateDefectCodeDto : CreateDefectCodeDto
{
    public bool IsActive { get; set; } = true;
}

// ─── Equipment ───────────────────────────────────────────────
public class EquipmentListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? ProductionLine { get; set; }
    public string? Workshop { get; set; }
    public string Status { get; set; } = "idle";
    public string? EquipmentType { get; set; }
    public bool HasMqttConnection { get; set; }
    public bool IsActive { get; set; }
    // 组织层级
    public long? OrgId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
    public string? WorkshopName { get; set; }
    public string? LineName { get; set; }
}

public class CreateEquipmentDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? ProductionLine { get; set; }
    public string? Workshop { get; set; }
    public string? EquipmentType { get; set; }
    public bool HasMqttConnection { get; set; } = false;
    public string? MqttTopicPrefix { get; set; }
    // 组织层级
    public long? OrgId { get; set; }
    public long? WorkshopId { get; set; }
    public long? LineId { get; set; }
}

public class UpdateEquipmentDto : CreateEquipmentDto
{
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "idle";
}

// ─── Tool ────────────────────────────────────────────────────
public class ToolListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? ToolType { get; set; }
    public double? DesignLife { get; set; }
    public string? LifeUnit { get; set; }
    public double CurrentLife { get; set; }
    public string? Supplier { get; set; }
    public bool IsActive { get; set; }
}

public class CreateToolDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? ToolType { get; set; }
    public double? DesignLife { get; set; }
    public string? LifeUnit { get; set; } = "cycles";
    public double CurrentLife { get; set; } = 0;
    public string? Supplier { get; set; }
}

public class UpdateToolDto : CreateToolDto
{
    public bool IsActive { get; set; } = true;
}

// ─── Supplier ────────────────────────────────────────────────
public class SupplierListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? Grade { get; set; }
    public string? SupplyCategory { get; set; }
    public double? Score { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSupplierDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? Email { get; set; }
    public string? Grade { get; set; } = "B";
    public string? SupplyCategory { get; set; }
}

public class UpdateSupplierDto : CreateSupplierDto
{
    public bool IsActive { get; set; } = true;
    public double? Score { get; set; }
}

// ─── Customer ────────────────────────────────────────────────
public class CustomerListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCustomerDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? Email { get; set; }
}

public class UpdateCustomerDto : CreateCustomerDto
{
    public bool IsActive { get; set; } = true;
}

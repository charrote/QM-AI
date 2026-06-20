using QM_AI.API.Models;

namespace QM_AI.API.DTOs.M02;

// ─── Product ─────────────────────────────────────────────────
public class ProductListDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public string? DefaultInspectionLevel { get; set; }
    public double? DefaultAql { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
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
}

public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; } = true;
}

// ─── BOM ─────────────────────────────────────────────────────
public class BomListDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public int Level { get; set; }
}

public class CreateBomDto
{
    public int ProductId { get; set; }
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
    public int Id { get; set; }
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
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public double? StandardTimeMinutes { get; set; }
}

public class CreateRoutingDto
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int StepOrder { get; set; }
    public int ProcessId { get; set; }
    public double? StandardTimeMinutes { get; set; }
}

public class UpdateRoutingDto : CreateRoutingDto { }

// ─── InspectionStandard ──────────────────────────────────────
public class InspectionStandardListDto
{
    public int Id { get; set; }
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
    public int? ProductId { get; set; }
    public int? ProcessId { get; set; }
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
    public int Id { get; set; }
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
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? ProductionLine { get; set; }
    public string? Workshop { get; set; }
    public string Status { get; set; } = "idle";
    public string? EquipmentType { get; set; }
    public bool HasMqttConnection { get; set; }
    public bool IsActive { get; set; }
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
}

public class UpdateEquipmentDto : CreateEquipmentDto
{
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "idle";
}

// ─── Tool ────────────────────────────────────────────────────
public class ToolListDto
{
    public int Id { get; set; }
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
    public int Id { get; set; }
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
    public int Id { get; set; }
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

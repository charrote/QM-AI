using QM_AI.API.Models.M02_5;

namespace QM_AI.API.DTOs.M02_5;

// ─── ParamGroup ─────────────────────────────────────────────────
public class ParamGroupListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int ParamCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ParamGroupDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateParamGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateParamGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}

// ─── DynamicParam ──────────────────────────────────────────────
public class DynamicParamListDto
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal Precision { get; set; }
    public string? AiStrategy { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DynamicParamDetailDto
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal Precision { get; set; }
    public string? AiStrategy { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateDynamicParamDto
{
    public long GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DataType { get; set; } = "numeric";
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal Precision { get; set; } = 1.0m;
    public string? AiStrategy { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateDynamicParamDto
{
    public long GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "numeric";
    public string? Unit { get; set; }
    public decimal? TargetValue { get; set; }
    public decimal? Usl { get; set; }
    public decimal? Lsl { get; set; }
    public decimal Precision { get; set; } = 1.0m;
    public string? AiStrategy { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ─── ClosureRule ──────────────────────────────────────────────
public class ClosureRuleListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Logic { get; set; } = "AND";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ClosureRuleDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ConditionJson { get; set; } = "[]";
    public string Logic { get; set; } = "AND";
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateClosureRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string ConditionJson { get; set; } = "[]";
    public string Logic { get; set; } = "AND";
    public string? Description { get; set; }
}

public class UpdateClosureRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string ConditionJson { get; set; } = "[]";
    public string Logic { get; set; } = "AND";
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

// ─── Closure Evaluation ───────────────────────────────────────
/// <summary>
/// 关单评估请求
/// </summary>
public class ClosureEvaluationRequest
{
    public long RuleId { get; set; }
}

/// <summary>
/// 关单评估结果
/// </summary>
public class ClosureEvaluationResultDto
{
    public bool IsSatisfied { get; set; }
    public string[]? FailedConditions { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Logic { get; set; } = "AND";
}

// ─── AI Strategy Presets ──────────────────────────────────────
public class AiStrategyPresetDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// ─── Condition types for JSON serialization ──────────────────
public class ClosureCondition
{
    public string Type { get; set; } = string.Empty;
    public string ParamCode { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public string Operator { get; set; } = ">";
}

using System.Text.Json;
using QM_AI.API.DTOs.M02_5;
using QM_AI.API.Models.M02_5;

namespace QM_AI.API.Services;

/// <summary>
/// 关单规则引擎 — 评估检验工序是否满足关单条件
/// </summary>
public class ClosureRuleEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// 评估关单条件是否满足
    /// </summary>
    public ClosureEvaluation Evaluate(ClosureRule rule, InspectionContext context)
    {
        var conditions = ParseConditions(rule.ConditionJson);
        var failedConditions = new List<string>();

        bool result = rule.Logic == "AND";

        foreach (var condition in conditions)
        {
            bool match = EvaluateCondition(condition, context);
            if (rule.Logic == "AND")
            {
                if (!match)
                {
                    failedConditions.Add($"{GetConditionDescription(condition)}: 不满足");
                    result = false;
                    // AND 模式下任一失败即终止
                }
            }
            else // OR
            {
                if (match)
                {
                    result = true;
                    break; // OR 模式下任一满足即通过
                }
                else
                {
                    failedConditions.Add($"{GetConditionDescription(condition)}: 不满足");
                }
            }
        }

        return new ClosureEvaluation
        {
            IsSatisfied = result,
            FailedConditions = failedConditions.Count > 0 && !result ? failedConditions.ToArray() : null,
        };
    }

    private List<ClosureCondition> ParseConditions(string conditionJson)
    {
        if (string.IsNullOrWhiteSpace(conditionJson))
            return new List<ClosureCondition>();

        try
        {
            // 支持直接数组格式 [{"type":"...", ...}]
            var conditions = JsonSerializer.Deserialize<List<ClosureCondition>>(conditionJson, JsonOptions);
            return conditions ?? new List<ClosureCondition>();
        }
        catch (JsonException)
        {
            return new List<ClosureCondition>();
        }
    }

    private bool EvaluateCondition(ClosureCondition cond, InspectionContext ctx)
    {
        double? actualValue = cond.Type switch
        {
            "consecutive_ok" => ctx.GetConsecutiveOkCount(cond.ParamCode),
            "spk_cpk" => ctx.GetCpk(cond.ParamCode),
            "sampling_rate" => ctx.GetSamplingPassRate(cond.ParamCode),
            "ai_risk_score" => ctx.GetAiRiskScore(),
            _ => null,
        };

        if (actualValue == null)
            return false;

        return cond.Operator switch
        {
            ">" => actualValue > cond.Threshold,
            ">=" => actualValue >= cond.Threshold,
            "<" => actualValue < cond.Threshold,
            "<=" => actualValue <= cond.Threshold,
            "=" => Math.Abs(actualValue.Value - cond.Threshold) < 0.0001,
            _ => actualValue > cond.Threshold,
        };
    }

    private static string GetConditionDescription(ClosureCondition cond)
    {
        return cond.Type switch
        {
            "consecutive_ok" => $"连续 {cond.Threshold} 件合格",
            "spk_cpk" => $"Cpk > {cond.Threshold}",
            "sampling_rate" => $"抽检合格率 > {cond.Threshold}%",
            "ai_risk_score" => $"AI 风险评分 < {cond.Threshold}",
            _ => $"{cond.Type} {cond.Operator} {cond.Threshold}",
        };
    }
}

/// <summary>
/// 关单评估结果
/// </summary>
public class ClosureEvaluation
{
    public bool IsSatisfied { get; set; }
    public string[]? FailedConditions { get; set; }
}

/// <summary>
/// 检验上下文 — 提供关单条件评估所需的数据
/// 实际使用时需注入真实数据源（DB/Redis/实时计算）
/// </summary>
public class InspectionContext
{
    private readonly Dictionary<string, double> _mockData = new()
    {
        ["consecutive_ok"] = 12,     // 连续 12 件合格
        ["cpk"] = 1.45,              // Cpk=1.45
        ["sampling_rate"] = 99.2,    // 合格率 99.2%
        ["ai_risk_score"] = 35,      // AI 风险评分 35
    };

    /// <summary>获取连续合格件数</summary>
    public virtual double GetConsecutiveOkCount(string paramCode) => _mockData["consecutive_ok"];

    /// <summary>获取指定参数的 Cpk</summary>
    public virtual double GetCpk(string paramCode) => _mockData["cpk"];

    /// <summary>获取抽检合格率</summary>
    public virtual double GetSamplingPassRate(string paramCode) => _mockData["sampling_rate"];

    /// <summary>获取 AI 风险评分</summary>
    public virtual double GetAiRiskScore() => _mockData["ai_risk_score"];
}

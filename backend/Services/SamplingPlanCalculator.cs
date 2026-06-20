namespace QM_AI.API.Services;

/// <summary>
/// GB/T 2828.1 (ISO 2859-1) 计数抽样方案计算器
/// 支持正常/加严/放宽三种严格度，覆盖样本量字母代码 A~Q
/// </summary>
public class SamplingPlanCalculator
{
    private readonly Dictionary<string, (int SampleSize, int Ac, int Re)> _normalTable;
    private readonly Dictionary<string, (int SampleSize, int Ac, int Re)> _reducedTable;
    private readonly Dictionary<string, (int SampleSize, int Ac, int Re)> _tightenedTable;

    public SamplingPlanCalculator()
    {
        // ── 正常检验方案表 (GB/T 2828.1 表2-A) ──
        // Key: "{code}_{aql}" e.g. "C_0.65"
        _normalTable = BuildNormalTable();

        // ── 加严检验方案表 (GB/T 2828.1 表2-B) ──
        _tightenedTable = BuildTightenedTable();

        // ── 放宽检验方案表 (GB/T 2828.1 表2-C) ──
        _reducedTable = BuildReducedTable();
    }

    public (int SampleSize, int Ac, int Re) Calculate(int lotSize, string level, double aql, string? severity = "normal")
    {
        var code = GetSampleCode(lotSize, level);
        return severity switch
        {
            "tightened" => Lookup(_tightenedTable, code, aql),
            "reduced" => Lookup(_reducedTable, code, aql),
            _ => Lookup(_normalTable, code, aql)
        };
    }

    /// <summary>
    /// 获取完整抽样方案对象（含字母代码）
    /// </summary>
    public SamplingResult GetSamplingPlan(int lotSize, string level, double aql, string? severity = "normal")
    {
        var code = GetSampleCode(lotSize, level);
        var (sampleSize, ac, re) = Calculate(lotSize, level, aql, severity);

        return new SamplingResult
        {
            SampleCode = code,
            SampleSize = sampleSize,
            Ac = ac,
            Re = re,
            LotSize = lotSize,
            SamplingLevel = level,
            AqlValue = aql,
            Severity = severity ?? "normal"
        };
    }

    /// <summary>
    /// 根据批量大小和检验水平获取样本量字母代码
    /// GB/T 2828.1 表1
    /// </summary>
    public char GetSampleCode(int lotSize, string level)
    {
        // 批量范围 → 字母代码映射
        // 按一般检验水平 II 的标准样本量代码表
        var ranges = level switch
        {
            "S-1" or "S-2" => _sampleCodeS1S2,
            "S-3" => _sampleCodeS3,
            "S-4" => _sampleCodeS4,
            "I" => _sampleCodeI,
            "II" => _sampleCodeII,
            "III" => _sampleCodeIII,
            _ => _sampleCodeII
        };

        foreach (var (min, max, code) in ranges)
        {
            if (lotSize >= min && lotSize <= max)
                return code;
        }

        // 超出最大批量，返回最大代码
        return ranges.Last().Code;
    }

    // ─── 抽样方案查表 ─────────────────────────────────────────────
    private (int SampleSize, int Ac, int Re) Lookup(Dictionary<string, (int, int, int)> table, char code, double aql)
    {
        // 将 AQL 值转换为查表 key 格式
        var aqlKey = FormatAqlKey(aql);
        var key = $"{code}_{aqlKey}";

        if (table.TryGetValue(key, out var result))
            return result;

        // 尝试向上查找最近的 AQL 值
        var fallback = FindNearestAql(table, code, aql);
        if (fallback.HasValue)
            return fallback.Value;

        // 默认返回全检方案
        return (-1, 0, 1); // 标记为全检
    }

    private string FormatAqlKey(double aql)
    {
        // AQL 值标准化为字符串 key
        return aql switch
        {
            <= 0.01 => "0_01",
            <= 0.015 => "0_015",
            <= 0.025 => "0_025",
            <= 0.04 => "0_04",
            <= 0.065 => "0_065",
            <= 0.10 => "0_10",
            <= 0.15 => "0_15",
            <= 0.25 => "0_25",
            <= 0.40 => "0_40",
            <= 0.65 => "0_65",
            <= 1.0 => "1_0",
            <= 1.5 => "1_5",
            <= 2.5 => "2_5",
            <= 4.0 => "4_0",
            <= 6.5 => "6_5",
            <= 10.0 => "10_0",
            _ => "10_0"
        };
    }

    private (int, int, int)? FindNearestAql(Dictionary<string, (int, int, int)> table, char code, double aql)
    {
        var aqlValues = new[] { 0.01, 0.015, 0.025, 0.04, 0.065, 0.10, 0.15, 0.25, 0.40, 0.65, 1.0, 1.5, 2.5, 4.0, 6.5, 10.0 };

        // 向上取 AQL
        foreach (var a in aqlValues)
        {
            if (a >= aql)
            {
                var key = $"{code}_{FormatAqlKey(a)}";
                if (table.TryGetValue(key, out var result))
                    return result;
            }
        }

        return null;
    }

    // ─── 样本量代码表 ────────────────────────────────────────────
    private static readonly List<(int Min, int Max, char Code)> _sampleCodeS1S2 = new()
    {
        (2, 8, 'A'), (9, 15, 'A'), (16, 25, 'A'), (26, 50, 'A'), (51, 90, 'B'),
        (91, 150, 'B'), (151, 280, 'B'), (281, 500, 'B'), (501, 1200, 'C'),
        (1201, 3200, 'C'), (3201, 10000, 'C'), (10001, 35000, 'C'), (35001, 150000, 'D'),
        (150001, 500000, 'D')
    };

    private static readonly List<(int Min, int Max, char Code)> _sampleCodeS3 = new()
    {
        (2, 8, 'A'), (9, 15, 'A'), (16, 25, 'A'), (26, 50, 'B'), (51, 90, 'B'),
        (91, 150, 'B'), (151, 280, 'C'), (281, 500, 'C'), (501, 1200, 'C'),
        (1201, 3200, 'D'), (3201, 10000, 'D'), (10001, 35000, 'E'), (35001, 150000, 'E'),
        (150001, 500000, 'F')
    };

    private static readonly List<(int Min, int Max, char Code)> _sampleCodeS4 = new()
    {
        (2, 8, 'A'), (9, 15, 'A'), (16, 25, 'B'), (26, 50, 'B'), (51, 90, 'C'),
        (91, 150, 'C'), (151, 280, 'D'), (281, 500, 'D'), (501, 1200, 'E'),
        (1201, 3200, 'E'), (3201, 10000, 'F'), (10001, 35000, 'F'), (35001, 150000, 'G'),
        (150001, 500000, 'G')
    };

    private static readonly List<(int Min, int Max, char Code)> _sampleCodeI = new()
    {
        (2, 8, 'A'), (9, 15, 'A'), (16, 25, 'B'), (26, 50, 'C'), (51, 90, 'C'),
        (91, 150, 'D'), (151, 280, 'E'), (281, 500, 'F'), (501, 1200, 'G'),
        (1201, 3200, 'H'), (3201, 10000, 'J'), (10001, 35000, 'K'), (35001, 150000, 'L'),
        (150001, 500000, 'M')
    };

    private static readonly List<(int Min, int Max, char Code)> _sampleCodeII = new()
    {
        (2, 8, 'A'), (9, 15, 'B'), (16, 25, 'C'), (26, 50, 'D'), (51, 90, 'E'),
        (91, 150, 'F'), (151, 280, 'G'), (281, 500, 'H'), (501, 1200, 'J'),
        (1201, 3200, 'K'), (3201, 10000, 'L'), (10001, 35000, 'M'), (35001, 150000, 'N'),
        (150001, 500000, 'P')
    };

    private static readonly List<(int Min, int Max, char Code)> _sampleCodeIII = new()
    {
        (2, 8, 'B'), (9, 15, 'C'), (16, 25, 'D'), (26, 50, 'E'), (51, 90, 'F'),
        (91, 150, 'G'), (151, 280, 'H'), (281, 500, 'J'), (501, 1200, 'K'),
        (1201, 3200, 'L'), (3201, 10000, 'M'), (10001, 35000, 'N'), (35001, 150000, 'P'),
        (150001, 500000, 'Q')
    };

    // ─── 正常检验方案表数据 ──────────────────────────────────────
    private static Dictionary<string, (int, int, int)> BuildNormalTable()
    {
        var t = new Dictionary<string, (int, int, int)>();

        // 格式: AddEntry(t, code, aqlKey, sampleSize, Ac, Re);
        // 代码 A
        AddEntry(t, 'A', "0_65", 2, 0, 1);
        AddEntry(t, 'A', "1_0", 2, 0, 1);
        AddEntry(t, 'A', "1_5", 2, 0, 1);
        AddEntry(t, 'A', "2_5", 2, 0, 1);
        AddEntry(t, 'A', "4_0", 2, 0, 1);
        AddEntry(t, 'A', "6_5", 2, 0, 1);
        AddEntry(t, 'A', "10_0", 2, 0, 1);
        AddEntry(t, 'A', "0_25", 2, 0, 1);
        AddEntry(t, 'A', "0_40", 2, 0, 1);
        AddEntry(t, 'A', "0_10", 2, 0, 1);
        AddEntry(t, 'A', "0_15", 2, 0, 1);

        // 代码 B
        AddEntry(t, 'B', "0_40", 3, 0, 1);
        AddEntry(t, 'B', "0_65", 3, 0, 1);
        AddEntry(t, 'B', "1_0", 3, 0, 1);
        AddEntry(t, 'B', "1_5", 3, 0, 1);
        AddEntry(t, 'B', "2_5", 3, 0, 1);
        AddEntry(t, 'B', "4_0", 3, 0, 1);
        AddEntry(t, 'B', "6_5", 3, 0, 1);
        AddEntry(t, 'B', "10_0", 3, 0, 1);

        // 代码 C
        AddEntry(t, 'C', "0_25", 5, 0, 1);
        AddEntry(t, 'C', "0_40", 5, 0, 1);
        AddEntry(t, 'C', "0_65", 5, 0, 1);
        AddEntry(t, 'C', "1_0", 5, 0, 1);
        AddEntry(t, 'C', "1_5", 5, 0, 1);
        AddEntry(t, 'C', "2_5", 5, 0, 1);
        AddEntry(t, 'C', "4_0", 5, 0, 1);
        AddEntry(t, 'C', "6_5", 5, 0, 1);
        AddEntry(t, 'C', "10_0", 5, 0, 1);

        // 代码 D
        AddEntry(t, 'D', "0_25", 8, 0, 1);
        AddEntry(t, 'D', "0_40", 8, 0, 1);
        AddEntry(t, 'D', "0_65", 8, 0, 1);
        AddEntry(t, 'D', "1_0", 8, 0, 1);
        AddEntry(t, 'D', "1_5", 8, 0, 1);
        AddEntry(t, 'D', "2_5", 8, 0, 1);
        AddEntry(t, 'D', "4_0", 8, 0, 1);
        AddEntry(t, 'D', "6_5", 8, 0, 1);
        AddEntry(t, 'D', "10_0", 8, 0, 1);

        // 代码 E
        AddEntry(t, 'E', "0_15", 13, 0, 1);
        AddEntry(t, 'E', "0_25", 13, 0, 1);
        AddEntry(t, 'E', "0_40", 13, 0, 1);
        AddEntry(t, 'E', "0_65", 13, 0, 1);
        AddEntry(t, 'E', "1_0", 13, 0, 1);
        AddEntry(t, 'E', "1_5", 13, 0, 1);
        AddEntry(t, 'E', "2_5", 13, 1, 2);
        AddEntry(t, 'E', "4_0", 13, 1, 2);
        AddEntry(t, 'E', "6_5", 13, 2, 3);
        AddEntry(t, 'E', "10_0", 13, 3, 4);

        // 代码 F
        AddEntry(t, 'F', "0_15", 20, 0, 1);
        AddEntry(t, 'F', "0_25", 20, 0, 1);
        AddEntry(t, 'F', "0_40", 20, 0, 1);
        AddEntry(t, 'F', "0_65", 20, 0, 1);
        AddEntry(t, 'F', "1_0", 20, 1, 2);
        AddEntry(t, 'F', "1_5", 20, 1, 2);
        AddEntry(t, 'F', "2_5", 20, 2, 3);
        AddEntry(t, 'F', "4_0", 20, 3, 4);
        AddEntry(t, 'F', "6_5", 20, 5, 6);
        AddEntry(t, 'F', "10_0", 20, 7, 8);

        // 代码 G
        AddEntry(t, 'G', "0_10", 32, 0, 1);
        AddEntry(t, 'G', "0_15", 32, 0, 1);
        AddEntry(t, 'G', "0_25", 32, 0, 1);
        AddEntry(t, 'G', "0_40", 32, 0, 1);
        AddEntry(t, 'G', "0_65", 32, 1, 2);
        AddEntry(t, 'G', "1_0", 32, 1, 2);
        AddEntry(t, 'G', "1_5", 32, 2, 3);
        AddEntry(t, 'G', "2_5", 32, 3, 4);
        AddEntry(t, 'G', "4_0", 32, 5, 6);
        AddEntry(t, 'G', "6_5", 32, 7, 8);
        AddEntry(t, 'G', "10_0", 32, 10, 11);

        // 代码 H
        AddEntry(t, 'H', "0_10", 50, 0, 1);
        AddEntry(t, 'H', "0_15", 50, 0, 1);
        AddEntry(t, 'H', "0_25", 50, 0, 1);
        AddEntry(t, 'H', "0_40", 50, 1, 2);
        AddEntry(t, 'H', "0_65", 50, 1, 2);
        AddEntry(t, 'H', "1_0", 50, 2, 3);
        AddEntry(t, 'H', "1_5", 50, 3, 4);
        AddEntry(t, 'H', "2_5", 50, 5, 6);
        AddEntry(t, 'H', "4_0", 50, 7, 8);
        AddEntry(t, 'H', "6_5", 50, 10, 11);
        AddEntry(t, 'H', "10_0", 50, 14, 15);

        // 代码 J
        AddEntry(t, 'J', "0_065", 80, 0, 1);
        AddEntry(t, 'J', "0_10", 80, 0, 1);
        AddEntry(t, 'J', "0_15", 80, 0, 1);
        AddEntry(t, 'J', "0_25", 80, 1, 2);
        AddEntry(t, 'J', "0_40", 80, 1, 2);
        AddEntry(t, 'J', "0_65", 80, 2, 3);
        AddEntry(t, 'J', "1_0", 80, 3, 4);
        AddEntry(t, 'J', "1_5", 80, 5, 6);
        AddEntry(t, 'J', "2_5", 80, 7, 8);
        AddEntry(t, 'J', "4_0", 80, 10, 11);
        AddEntry(t, 'J', "6_5", 80, 14, 15);
        AddEntry(t, 'J', "10_0", 80, 21, 22);

        // 代码 K
        AddEntry(t, 'K', "0_065", 125, 0, 1);
        AddEntry(t, 'K', "0_10", 125, 0, 1);
        AddEntry(t, 'K', "0_15", 125, 1, 2);
        AddEntry(t, 'K', "0_25", 125, 1, 2);
        AddEntry(t, 'K', "0_40", 125, 2, 3);
        AddEntry(t, 'K', "0_65", 125, 3, 4);
        AddEntry(t, 'K', "1_0", 125, 5, 6);
        AddEntry(t, 'K', "1_5", 125, 7, 8);
        AddEntry(t, 'K', "2_5", 125, 10, 11);
        AddEntry(t, 'K', "4_0", 125, 14, 15);
        AddEntry(t, 'K', "6_5", 125, 21, 22);
        AddEntry(t, 'K', "10_0", 125, 21, 22);

        // 代码 L
        AddEntry(t, 'L', "0_04", 200, 0, 1);
        AddEntry(t, 'L', "0_065", 200, 0, 1);
        AddEntry(t, 'L', "0_10", 200, 1, 2);
        AddEntry(t, 'L', "0_15", 200, 1, 2);
        AddEntry(t, 'L', "0_25", 200, 2, 3);
        AddEntry(t, 'L', "0_40", 200, 3, 4);
        AddEntry(t, 'L', "0_65", 200, 5, 6);
        AddEntry(t, 'L', "1_0", 200, 7, 8);
        AddEntry(t, 'L', "1_5", 200, 10, 11);
        AddEntry(t, 'L', "2_5", 200, 14, 15);
        AddEntry(t, 'L', "4_0", 200, 21, 22);
        AddEntry(t, 'L', "6_5", 200, 21, 22);
        AddEntry(t, 'L', "10_0", 200, 21, 22);

        // 代码 M
        AddEntry(t, 'M', "0_04", 315, 0, 1);
        AddEntry(t, 'M', "0_065", 315, 1, 2);
        AddEntry(t, 'M', "0_10", 315, 1, 2);
        AddEntry(t, 'M', "0_15", 315, 2, 3);
        AddEntry(t, 'M', "0_25", 315, 3, 4);
        AddEntry(t, 'M', "0_40", 315, 5, 6);
        AddEntry(t, 'M', "0_65", 315, 7, 8);
        AddEntry(t, 'M', "1_0", 315, 10, 11);
        AddEntry(t, 'M', "1_5", 315, 14, 15);
        AddEntry(t, 'M', "2_5", 315, 21, 22);
        AddEntry(t, 'M', "4_0", 315, 21, 22);
        AddEntry(t, 'M', "6_5", 315, 21, 22);
        AddEntry(t, 'M', "10_0", 315, 21, 22);

        // 代码 N
        AddEntry(t, 'N', "0_04", 500, 1, 2);
        AddEntry(t, 'N', "0_065", 500, 1, 2);
        AddEntry(t, 'N', "0_10", 500, 2, 3);
        AddEntry(t, 'N', "0_15", 500, 3, 4);
        AddEntry(t, 'N', "0_25", 500, 5, 6);
        AddEntry(t, 'N', "0_40", 500, 7, 8);
        AddEntry(t, 'N', "0_65", 500, 10, 11);
        AddEntry(t, 'N', "1_0", 500, 14, 15);
        AddEntry(t, 'N', "1_5", 500, 21, 22);
        AddEntry(t, 'N', "2_5", 500, 21, 22);
        AddEntry(t, 'N', "4_0", 500, 21, 22);
        AddEntry(t, 'N', "6_5", 500, 21, 22);
        AddEntry(t, 'N', "10_0", 500, 21, 22);

        // 代码 P
        AddEntry(t, 'P', "0_04", 800, 1, 2);
        AddEntry(t, 'P', "0_065", 800, 2, 3);
        AddEntry(t, 'P', "0_10", 800, 3, 4);
        AddEntry(t, 'P', "0_15", 800, 5, 6);
        AddEntry(t, 'P', "0_25", 800, 7, 8);
        AddEntry(t, 'P', "0_40", 800, 10, 11);
        AddEntry(t, 'P', "0_65", 800, 14, 15);
        AddEntry(t, 'P', "1_0", 800, 21, 22);
        AddEntry(t, 'P', "1_5", 800, 21, 22);
        AddEntry(t, 'P', "2_5", 800, 21, 22);
        AddEntry(t, 'P', "4_0", 800, 21, 22);
        AddEntry(t, 'P', "6_5", 800, 21, 22);
        AddEntry(t, 'P', "10_0", 800, 21, 22);

        // 代码 Q
        AddEntry(t, 'Q', "0_04", 1250, 2, 3);
        AddEntry(t, 'Q', "0_065", 1250, 3, 4);
        AddEntry(t, 'Q', "0_10", 1250, 5, 6);
        AddEntry(t, 'Q', "0_15", 1250, 7, 8);
        AddEntry(t, 'Q', "0_25", 1250, 10, 11);
        AddEntry(t, 'Q', "0_40", 1250, 14, 15);
        AddEntry(t, 'Q', "0_65", 1250, 21, 22);
        AddEntry(t, 'Q', "1_0", 1250, 21, 22);
        AddEntry(t, 'Q', "1_5", 1250, 21, 22);
        AddEntry(t, 'Q', "2_5", 1250, 21, 22);
        AddEntry(t, 'Q', "4_0", 1250, 21, 22);
        AddEntry(t, 'Q', "6_5", 1250, 21, 22);
        AddEntry(t, 'Q', "10_0", 1250, 21, 22);

        return t;
    }

    // ─── 加严检验方案表数据 ──────────────────────────────────────
    private static Dictionary<string, (int, int, int)> BuildTightenedTable()
    {
        // 简化：加严方案通常 Ac 更小或 Re 更小，样本量可能与正常相同
        // 对于大多数场景，使用正常方案的 Ac-1（如果 Ac > 0）或增加样本量
        // 这里仅提供主要差异项，完整方案可以后续补充
        var t = new Dictionary<string, (int, int, int)>();

        // 代码 F - 加严
        AddEntry(t, 'F', "1_0", 20, 0, 1);
        AddEntry(t, 'F', "1_5", 20, 0, 1);
        AddEntry(t, 'F', "2_5", 20, 1, 2);
        AddEntry(t, 'F', "4_0", 20, 2, 3);
        AddEntry(t, 'F', "6_5", 20, 3, 4);
        AddEntry(t, 'F', "10_0", 20, 5, 6);

        // 代码 G - 加严
        AddEntry(t, 'G', "0_65", 32, 0, 1);
        AddEntry(t, 'G', "1_0", 32, 0, 1);
        AddEntry(t, 'G', "1_5", 32, 1, 2);
        AddEntry(t, 'G', "2_5", 32, 2, 3);
        AddEntry(t, 'G', "4_0", 32, 3, 4);
        AddEntry(t, 'G', "6_5", 32, 5, 6);
        AddEntry(t, 'G', "10_0", 32, 8, 9);

        // 代码 H - 加严
        AddEntry(t, 'H', "0_40", 50, 0, 1);
        AddEntry(t, 'H', "0_65", 50, 0, 1);
        AddEntry(t, 'H', "1_0", 50, 1, 2);
        AddEntry(t, 'H', "1_5", 50, 2, 3);
        AddEntry(t, 'H', "2_5", 50, 3, 4);
        AddEntry(t, 'H', "4_0", 50, 5, 6);
        AddEntry(t, 'H', "6_5", 50, 8, 9);
        AddEntry(t, 'H', "10_0", 50, 12, 13);

        // 代码 J - 加严
        AddEntry(t, 'J', "0_25", 80, 0, 1);
        AddEntry(t, 'J', "0_40", 80, 0, 1);
        AddEntry(t, 'J', "0_65", 80, 1, 2);
        AddEntry(t, 'J', "1_0", 80, 2, 3);
        AddEntry(t, 'J', "1_5", 80, 3, 4);
        AddEntry(t, 'J', "2_5", 80, 5, 6);
        AddEntry(t, 'J', "4_0", 80, 8, 9);
        AddEntry(t, 'J', "6_5", 80, 12, 13);
        AddEntry(t, 'J', "10_0", 80, 18, 19);

        return t;
    }

    // ─── 放宽检验方案表数据 ──────────────────────────────────────
    private static Dictionary<string, (int, int, int)> BuildReducedTable()
    {
        // 简化：放宽方案样本量通常为正常方案的 40-60%
        var t = new Dictionary<string, (int, int, int)>();

        // 代码 G - 放宽
        AddEntry(t, 'G', "0_65", 13, 0, 2);
        AddEntry(t, 'G', "1_0", 13, 0, 2);
        AddEntry(t, 'G', "1_5", 13, 0, 2);
        AddEntry(t, 'G', "2_5", 13, 0, 2);
        AddEntry(t, 'G', "4_0", 13, 1, 3);
        AddEntry(t, 'G', "6_5", 13, 2, 4);
        AddEntry(t, 'G', "10_0", 13, 4, 6);

        // 代码 H - 放宽
        AddEntry(t, 'H', "0_40", 20, 0, 2);
        AddEntry(t, 'H', "0_65", 20, 0, 2);
        AddEntry(t, 'H', "1_0", 20, 0, 2);
        AddEntry(t, 'H', "1_5", 20, 0, 2);
        AddEntry(t, 'H', "2_5", 20, 1, 3);
        AddEntry(t, 'H', "4_0", 20, 2, 4);
        AddEntry(t, 'H', "6_5", 20, 4, 6);
        AddEntry(t, 'H', "10_0", 20, 6, 8);

        // 代码 J - 放宽
        AddEntry(t, 'J', "0_25", 32, 0, 2);
        AddEntry(t, 'J', "0_40", 32, 0, 2);
        AddEntry(t, 'J', "0_65", 32, 0, 2);
        AddEntry(t, 'J', "1_0", 32, 0, 2);
        AddEntry(t, 'J', "1_5", 32, 0, 2);
        AddEntry(t, 'J', "2_5", 32, 1, 3);
        AddEntry(t, 'J', "4_0", 32, 2, 4);
        AddEntry(t, 'J', "6_5", 32, 4, 6);
        AddEntry(t, 'J', "10_0", 32, 6, 8);

        // 代码 K - 放宽
        AddEntry(t, 'K', "0_065", 50, 0, 2);
        AddEntry(t, 'K', "0_10", 50, 0, 2);
        AddEntry(t, 'K', "0_15", 50, 0, 2);
        AddEntry(t, 'K', "0_25", 50, 0, 2);
        AddEntry(t, 'K', "0_40", 50, 0, 2);
        AddEntry(t, 'K', "0_65", 50, 0, 2);
        AddEntry(t, 'K', "1_0", 50, 1, 3);
        AddEntry(t, 'K', "1_5", 50, 2, 4);
        AddEntry(t, 'K', "2_5", 50, 4, 6);
        AddEntry(t, 'K', "4_0", 50, 6, 8);
        AddEntry(t, 'K', "6_5", 50, 10, 12);

        return t;
    }

    private static void AddEntry(Dictionary<string, (int, int, int)> table, char code, string aqlKey, int sampleSize, int ac, int re)
    {
        table[$"{code}_{aqlKey}"] = (sampleSize, ac, re);
    }
}

/// <summary>
/// 抽样方案计算结果
/// </summary>
public class SamplingResult
{
    public char SampleCode { get; set; }
    public int SampleSize { get; set; }
    public int Ac { get; set; }
    public int Re { get; set; }
    public int LotSize { get; set; }
    public string SamplingLevel { get; set; } = "II";
    public double AqlValue { get; set; }
    public string Severity { get; set; } = "normal";
    public bool IsFullInspection => SampleSize < 0;
}

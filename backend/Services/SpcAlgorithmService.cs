namespace QM_AI.API.Services;

/// <summary>
/// SPC algorithm service providing:
/// - X̄-R control chart calculations
/// - CPK/PPK process capability analysis
/// - Western Electric 8 rule violation detection
/// - Statistical helper functions
/// </summary>
public class SpcAlgorithmService
{
    /// <summary>
    /// Calculate X̄-R control limits from subgroup data.
    /// </summary>
    public ControlLimitsResult CalculateXbarRControlLimits(IEnumerable<double[]> subgroups)
    {
        var subgroupsList = subgroups.ToList();
        if (!subgroupsList.Any() || subgroupsList.Any(s => !s.Any()))
            throw new ArgumentException("Subgroup data cannot be empty.");

        var xbarValues = new List<double>();
        var rValues = new List<double>();

        foreach (var subgroup in subgroupsList)
        {
            double xbar = subgroup.Average();
            double r = subgroup.Max() - subgroup.Min();
            xbarValues.Add(xbar);
            rValues.Add(r);
        }

        double xDouble = xbarValues.Average();  // Grand mean (X̿)
        double rDouble = rValues.Average();      // Average range (R̄)

        int n = subgroupsList.First().Length;
        var constants = SpcConstants.GetConstants(n);

        double uclXbar = xDouble + constants.A2 * rDouble;
        double lclXbar = xDouble - constants.A2 * rDouble;
        double uclR = constants.D4 * rDouble;
        double lclR = constants.D3 * rDouble;

        return new ControlLimitsResult
        {
            ClXbar = xDouble,
            UclXbar = uclXbar,
            LclXbar = lclXbar,
            ClR = rDouble,
            UclR = uclR,
            LclR = lclR,
            SigmaEstimate = rDouble / constants.d2
        };
    }

    /// <summary>
    /// Calculate X̄-S control limits from subgroup data.
    /// </summary>
    public ControlLimitsResult CalculateXbarSControlLimits(IEnumerable<double[]> subgroups)
    {
        var subgroupsList = subgroups.ToList();
        if (!subgroupsList.Any() || subgroupsList.Any(s => !s.Any()))
            throw new ArgumentException("Subgroup data cannot be empty.");

        var xbarValues = new List<double>();
        var sValues = new List<double>();

        foreach (var subgroup in subgroupsList)
        {
            double xbar = subgroup.Average();
            double stdDev = CalculateSampleStdDev(subgroup);
            xbarValues.Add(xbar);
            sValues.Add(stdDev);
        }

        double xDouble = xbarValues.Average(); // Grand mean
        double sDouble = sValues.Average();     // Average standard deviation

        int n = subgroupsList.First().Length;
        var constants = SpcConstants.GetXbarSConstants(n);

        double uclXbar = xDouble + constants.A3 * sDouble;
        double lclXbar = xDouble - constants.A3 * sDouble;
        double uclS = constants.B4 * sDouble;
        double lclS = constants.B3 * sDouble;

        return new ControlLimitsResult
        {
            ClXbar = xDouble,
            UclXbar = uclXbar,
            LclXbar = lclXbar,
            ClR = sDouble,
            UclR = uclS,
            LclR = lclS,
            SigmaEstimate = sDouble / constants.c4
        };
    }

    /// <summary>
    /// Calculate I-MR (Individual-Moving Range) control limits.
    /// </summary>
    public ControlLimitsResult CalculateImrControlLimits(IEnumerable<double> individuals)
    {
        var values = individuals.ToList();
        if (values.Count < 2)
            throw new ArgumentException("Need at least 2 data points for I-MR chart.");

        double cl = values.Average();

        // Moving ranges
        var mrValues = new List<double>();
        for (int i = 1; i < values.Count; i++)
        {
            mrValues.Add(Math.Abs(values[i] - values[i - 1]));
        }

        double mrBar = mrValues.Average();

        // Constants for n=2 (individual moving range uses subgroup size 2)
        double d2 = 1.128;
        double d3 = 0;
        double d4 = 3.267;

        double sigma = mrBar / d2;
        double ucl = cl + 3 * sigma;
        double lcl = cl - 3 * sigma;

        double uclMr = d4 * mrBar;
        double lclMr = d3 * mrBar;

        return new ControlLimitsResult
        {
            ClXbar = cl,
            UclXbar = ucl,
            LclXbar = lcl,
            ClR = mrBar,
            UclR = uclMr,
            LclR = lclMr,
            SigmaEstimate = sigma
        };
    }

    /// <summary>
    /// Calculate P (fraction nonconforming) control limits.
    /// individual_values encodes [sampleSize, defectCount] per point.
    /// Variable-n: per-point limits differ, but we return fixed limits based on
    /// the average sample size n̄ (exact when n is constant, as in demo data).
    /// </summary>
    public ControlLimitsResult CalculatePControlLimits(IEnumerable<AttributePoint> points)
    {
        var pts = points.ToList();
        if (!pts.Any())
            throw new ArgumentException("Attribute data cannot be empty.");

        double sumN = pts.Sum(p => p.SampleSize);
        double sumD = pts.Sum(p => p.Defects);
        double pBar = sumD / sumN;          // Overall fraction nonconforming
        double nBar = sumN / pts.Count;     // Average sample size

        double ucl = pBar + 3 * Math.Sqrt(pBar * (1 - pBar) / nBar);
        double lcl = pBar - 3 * Math.Sqrt(pBar * (1 - pBar) / nBar);
        if (lcl < 0) lcl = 0;
        if (ucl > 1) ucl = 1;

        return new ControlLimitsResult
        {
            ClXbar = pBar,
            UclXbar = ucl,
            LclXbar = lcl,
            SigmaEstimate = nBar > 0 ? Math.Sqrt(pBar * (1 - pBar) / nBar) : 0
        };
    }

    /// <summary>
    /// Calculate C (count of nonconformities) control limits.
    /// individual_values encodes [1, defectCount] or [sampleSize, defectCount]; only Defects used.
    /// c̄ ± 3√c̄
    /// </summary>
    public ControlLimitsResult CalculateCControlLimits(IEnumerable<AttributePoint> points)
    {
        var pts = points.ToList();
        if (!pts.Any())
            throw new ArgumentException("Attribute data cannot be empty.");

        double cBar = pts.Average(p => p.Defects);  // Average count
        double ucl = cBar + 3 * Math.Sqrt(cBar);
        double lcl = cBar - 3 * Math.Sqrt(cBar);
        if (lcl < 0) lcl = 0;

        return new ControlLimitsResult
        {
            ClXbar = cBar,
            UclXbar = ucl,
            LclXbar = lcl,
            SigmaEstimate = Math.Sqrt(cBar)
        };
    }

    /// <summary>
    /// Calculate U (nonconformities per unit) control limits.
    /// individual_values encodes [sampleSize, defectCount]. ū = Σc / Σn, UCL = ū + 3√(ū/n̄)
    /// </summary>
    public ControlLimitsResult CalculateUControlLimits(IEnumerable<AttributePoint> points)
    {
        var pts = points.ToList();
        if (!pts.Any())
            throw new ArgumentException("Attribute data cannot be empty.");

        double sumN = pts.Sum(p => p.SampleSize);
        double sumC = pts.Sum(p => p.Defects);
        double uBar = sumC / sumN;          // Average nonconformities per unit
        double nBar = sumN / pts.Count;     // Average sample size (units inspected)

        double ucl = uBar + 3 * Math.Sqrt(uBar / nBar);
        double lcl = uBar - 3 * Math.Sqrt(uBar / nBar);
        if (lcl < 0) lcl = 0;

        return new ControlLimitsResult
        {
            ClXbar = uBar,
            UclXbar = ucl,
            LclXbar = lcl,
            SigmaEstimate = nBar > 0 ? Math.Sqrt(uBar / nBar) : 0
        };
    }

    /// <summary>
    /// Calculate process capability indices (Cp, Cpk, Pp, Ppk).
    /// </summary>
    /// <summary>
    /// Calculate process capability with subgroup-based sigma estimation (R̄/d₂).
    /// For X̄-R charts, sigmaWithin must be estimated from within-subgroup variation,
    /// NOT from individual values. This matches Minitab behavior.
    /// </summary>
    public CpkResult CalculateCapabilityFromSubgroups(IEnumerable<double[]> subgroups, double usl, double lsl, bool useStdDev = false)
    {
        var subgroupsList = subgroups.ToList();
        if (!subgroupsList.Any() || subgroupsList.First().Length < 2)
            throw new ArgumentException("Need at least 2 subgroups with 2+ values each.");

        // Flatten all values for overall statistics
        var allValues = subgroupsList.SelectMany(s => s).ToList();
        double mean = allValues.Average();

        // Within-subgroup sigma: S̄/c₄ for Xbar-S, R̄/d₂ for Xbar-R (Minitab standard)
        int n = subgroupsList.First().Length;
        double sigmaWithin;
        if (useStdDev)
        {
            var sValues = subgroupsList.Select(s => CalculateSampleStdDev(s)).ToList();
            double sDouble = sValues.Average();
            double c4 = SpcConstants.GetXbarSConstants(n).c4;
            sigmaWithin = c4 > 0 ? sDouble / c4 : sDouble;
        }
        else
        {
            var rValues = subgroupsList.Select(s => s.Max() - s.Min()).ToList();
            double rDouble = rValues.Average();
            sigmaWithin = rDouble / SpcConstants.GetConstants(n).d2;
        }

        double sigmaOverall = CalculatePopulationStdDev(allValues);

        double cp = (usl - lsl) / (6 * sigmaWithin);
        double cpu = (usl - mean) / (3 * sigmaWithin);
        double cpl = (mean - lsl) / (3 * sigmaWithin);
        double cpk = Math.Min(cpu, cpl);

        double pp = (usl - lsl) / (6 * sigmaOverall);
        double ppu = (usl - mean) / (3 * sigmaOverall);
        double ppl = (mean - lsl) / (3 * sigmaOverall);
        double ppk = Math.Min(ppu, ppl);

        string grade = cpk >= 1.67 ? "优秀" :
                       cpk >= 1.33 ? "良好" :
                       cpk >= 1.0 ? "临界" : "不足";

        double ppm = (1 - NormalCdf((usl - mean) / sigmaWithin) +
                      NormalCdf((lsl - mean) / sigmaWithin)) * 1_000_000;

        return new CpkResult
        {
            Cp = Math.Round(cp, 4),
            Cpk = Math.Round(cpk, 4),
            Pp = Math.Round(pp, 4),
            Ppk = Math.Round(ppk, 4),
            SigmaWithin = Math.Round(sigmaWithin, 6),
            SigmaOverall = Math.Round(sigmaOverall, 6),
            Grade = grade,
            EstimatedPpm = Math.Round(ppm, 2),
            Mean = Math.Round(mean, 6)
        };
    }

    public CpkResult CalculateCapability(IEnumerable<double> data, double usl, double lsl)
    {
        var dataList = data.ToList();
        if (dataList.Count < 2)
            throw new ArgumentException("Need at least 2 data points for capability analysis.");

        double mean = dataList.Average();
        double sigmaWithin = CalculateSampleStdDev(dataList);
        double sigmaOverall = CalculatePopulationStdDev(dataList);

        double cp = (usl - lsl) / (6 * sigmaWithin);
        double cpu = (usl - mean) / (3 * sigmaWithin);
        double cpl = (mean - lsl) / (3 * sigmaWithin);
        double cpk = Math.Min(cpu, cpl);

        double pp = (usl - lsl) / (6 * sigmaOverall);
        double ppu = (usl - mean) / (3 * sigmaOverall);
        double ppl = (mean - lsl) / (3 * sigmaOverall);
        double ppk = Math.Min(ppu, ppl);

        string grade = cpk >= 1.67 ? "优秀" :
                       cpk >= 1.33 ? "良好" :
                       cpk >= 1.0 ? "临界" : "不足";

        // Estimate DPMO using standard normal distribution
        double ppm = (1 - NormalCdf((usl - mean) / sigmaWithin) +
                      NormalCdf((lsl - mean) / sigmaWithin)) * 1_000_000;

        return new CpkResult
        {
            Cp = Math.Round(cp, 4),
            Cpk = Math.Round(cpk, 4),
            Pp = Math.Round(pp, 4),
            Ppk = Math.Round(ppk, 4),
            SigmaWithin = Math.Round(sigmaWithin, 6),
            SigmaOverall = Math.Round(sigmaOverall, 6),
            Grade = grade,
            EstimatedPpm = Math.Round(ppm, 2),
            Mean = Math.Round(mean, 6)
        };
    }

    /// <summary>
    /// Detect Western Electric 8 rule violations.
    /// </summary>
    public List<RuleViolationResult> DetectRuleViolations(
        double[] means, double cl, double ucl, double lcl, double sigma)
    {
        var violations = new List<RuleViolationResult>();
        int n = means.Length;

        if (n == 0) return violations;

        double oneSigma = sigma;
        double twoSigma = 2 * sigma;

        // ── Rule 1: 1 point beyond 3σ (beyond UCL or LCL) — report EVERY offending point ──
        for (int i = 0; i < n; i++)
        {
            if (means[i] > ucl)
            {
                violations.Add(new RuleViolationResult(1, i,
                    $"1点超出3σ控制限UCL (点 #{i + 1}, 值={means[i]:F4})"));
            }
            else if (means[i] < lcl)
            {
                violations.Add(new RuleViolationResult(1, i,
                    $"1点超出3σ控制限LCL (点 #{i + 1}, 值={means[i]:F4})"));
            }
        }

        // ── Rule 2: 9+ consecutive points on same side of CL (per run) ──
        int runStart = 0;
        bool aboveCl = means[0] >= cl;
        for (int i = 1; i <= n; i++)
        {
            if (i < n)
            {
                bool currentAbove = means[i] >= cl;
                if (currentAbove == aboveCl) continue; // run continues
            }
            int runLen = i - runStart;
            if (runLen >= 9)
            {
                violations.Add(new RuleViolationResult(2, i - 1,
                    $"连续{runLen}点在CL同侧 (点 #{runStart + 1} ~ #{i})"));
            }
            if (i < n)
            {
                runStart = i;
                aboveCl = means[i] >= cl;
            }
        }

        // ── Rule 3: 6 consecutive points increasing or decreasing (per maximal run) ──
        int trendLen = 1;
        int dir = 0; // +1 increasing, -1 decreasing
        for (int i = 1; i <= n; i++)
        {
            if (i < n)
            {
                int d = means[i] > means[i - 1] ? 1 : (means[i] < means[i - 1] ? -1 : 0);
                if (d == 0) { trendLen = 1; dir = 0; continue; }
                if (dir == 0) dir = d;
                if (d == dir) { trendLen++; }
                else { // direction changed
                    if (trendLen >= 6)
                        violations.Add(new RuleViolationResult(3, i - 1,
                            $"连续{trendLen - 1}点{(dir == 1 ? "递增" : "递减")} (点 #{i - trendLen + 1} ~ #{i - 1})"));
                    trendLen = 1;
                    dir = d;
                }
            }
            else if (trendLen >= 6)
            {
                violations.Add(new RuleViolationResult(3, i - 1,
                    $"连续{trendLen}点{(dir == 1 ? "递增" : "递减")} (点 #{i - trendLen + 1} ~ #{i})"));
            }
        }

        // ── Rule 4: 14 consecutive points alternating up and down (per maximal run) ──
        int altStart = 0;
        int altLen = 1;
        int lastSign = 0;
        for (int i = 1; i <= n; i++)
        {
            if (i < n)
            {
                double diff = means[i] - means[i - 1];
                if (diff == 0) { // flat breaks alternation
                    if (altLen >= 14)
                        violations.Add(new RuleViolationResult(4, i - 1, $"连续{altLen - 1}点上下交替 (点 #{altStart + 1} ~ #{i - 1})"));
                    altStart = i; altLen = 1; lastSign = 0; continue;
                }
                int sgn = diff > 0 ? 1 : -1;
                if (lastSign == 0) { lastSign = sgn; altLen = 2; }
                else if (sgn != lastSign) { lastSign = sgn; altLen++; }
                else { // same sign — run broken
                    if (altLen >= 14)
                        violations.Add(new RuleViolationResult(4, i - 1, $"连续{altLen - 1}点上下交替 (点 #{altStart + 1} ~ #{i - 1})"));
                    altStart = i - 1; altLen = 2; lastSign = sgn;
                }
            }
            else if (altLen >= 14)
            {
                violations.Add(new RuleViolationResult(4, i - 1, $"连续{altLen}点上下交替 (点 #{altStart + 1} ~ #{i})"));
            }
        }

        // ── Rule 5: 2 out of 3 points > 2σ from CL (same side) ──
        for (int i = 2; i < n; i++)
        {
            int countAbove = 0, countBelow = 0;
            for (int j = i - 2; j <= i; j++)
            {
                if (means[j] > cl + twoSigma) countAbove++;
                if (means[j] < cl - twoSigma) countBelow++;
            }
            if (countAbove >= 2)
            {
                violations.Add(new RuleViolationResult(5, i,
                    $"连续3点中2点超过2σ (点 #{i - 1} ~ #{i + 1}, 上方)"));
            }
            else if (countBelow >= 2)
            {
                violations.Add(new RuleViolationResult(5, i,
                    $"连续3点中2点超过2σ (点 #{i - 1} ~ #{i + 1}, 下方)"));
            }
        }

        // ── Rule 6: 4 out of 5 points > 1σ from CL (same side) ──
        for (int i = 4; i < n; i++)
        {
            int countAbove = 0, countBelow = 0;
            for (int j = i - 4; j <= i; j++)
            {
                if (means[j] > cl + oneSigma) countAbove++;
                if (means[j] < cl - oneSigma) countBelow++;
            }
            if (countAbove >= 4)
            {
                violations.Add(new RuleViolationResult(6, i,
                    $"连续5点中4点超过1σ (点 #{i - 3} ~ #{i + 1}, 上方)"));
            }
            else if (countBelow >= 4)
            {
                violations.Add(new RuleViolationResult(6, i,
                    $"连续5点中4点超过1σ (点 #{i - 3} ~ #{i + 1}, 下方)"));
            }
        }

        // ── Rule 7: 15 consecutive points within 1σ of CL (per maximal run) ──
        int withinStart = 0;
        bool inWithin = Math.Abs(means[0] - cl) <= oneSigma;
        int withinLen = inWithin ? 1 : 0;
        for (int i = 1; i <= n; i++)
        {
            bool cur = i < n ? Math.Abs(means[i] - cl) <= oneSigma : false;
            if (cur == inWithin && cur) { if (i < n) withinLen++; continue; }
            if (inWithin && withinLen >= 15)
                violations.Add(new RuleViolationResult(7, i - 1, $"连续{withinLen}点在1σ内 (点 #{withinStart + 1} ~ #{i})"));
            if (i < n) { inWithin = cur; withinStart = cur ? i : 0; withinLen = cur ? 1 : 0; }
        }

        // ── Rule 8: 8 consecutive points > 1σ from CL (both sides) (per maximal run) ──
        int beyondStart = 0;
        bool inBeyond = Math.Abs(means[0] - cl) > oneSigma;
        int beyondLen = inBeyond ? 1 : 0;
        for (int i = 1; i <= n; i++)
        {
            bool cur = i < n ? Math.Abs(means[i] - cl) > oneSigma : false;
            if (cur == inBeyond && cur) { if (i < n) beyondLen++; continue; }
            if (inBeyond && beyondLen >= 8)
                violations.Add(new RuleViolationResult(8, i - 1, $"连续{beyondLen}点超出1σ (点 #{beyondStart + 1} ~ #{i})"));
            if (i < n) { inBeyond = cur; beyondStart = cur ? i : 0; beyondLen = cur ? 1 : 0; }
        }

        return violations;
    }

    /// <summary>
    /// Calculate sample standard deviation (n-1 denominator).
    /// </summary>
    public double CalculateSampleStdDev(IEnumerable<double> data)
    {
        var list = data.ToList();
        if (list.Count < 2) return 0;
        double mean = list.Average();
        double sumSq = list.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumSq / (list.Count - 1));
    }

    /// <summary>
    /// Calculate population standard deviation (n denominator).
    /// </summary>
    public double CalculatePopulationStdDev(IEnumerable<double> data)
    {
        var list = data.ToList();
        if (list.Count < 2) return 0;
        double mean = list.Average();
        double sumSq = list.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumSq / list.Count);
    }

    /// <summary>
    /// Standard normal CDF approximation (Abramowitz & Stegun).
    /// </summary>
    public double NormalCdf(double x)
    {
        return 0.5 * (1.0 + Erf(x / Math.Sqrt(2)));
    }

    /// <summary>
    /// Error function approximation.
    /// </summary>
    public double Erf(double x)
    {
        const double a1 = 0.254829592;
        const double a2 = -0.284496736;
        const double a3 = 1.421413741;
        const double a4 = -1.453152027;
        const double a5 = 1.061405429;
        const double p = 0.3275911;

        int sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);
        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);
        return sign * y;
    }

    /// <summary>
    /// Inverse normal CDF (for control limit calculations).
    /// </summary>
    public double NormalQuantile(double p)
    {
        // Rational approximation (Acklam)
        if (p <= 0 || p >= 1)
            throw new ArgumentException("p must be between 0 and 1 exclusive.");

        double[] a = { -3.969683028665376e+1, 2.209460984245205e+2, -2.759285104469687e+2,
                        1.383577518672690e+2, -3.066479806614716e+1, 2.506628277459239e+0 };
        double[] b = { -5.447609879822406e+1, 1.615858368580409e+2, -1.556989798598866e+2,
                        6.680131188771972e+1, -1.328068155288572e+1 };
        double[] c = { -7.784894002430293e-3, -3.223964580411365e-1, -2.400758277161838e+0,
                        -2.549732539343734e+0, 4.374664141464968e+0, 2.938163982698783e+0 };
        double[] d = { 7.784695709041462e-3, 3.224671290700398e-1, 2.445134137142996e+0,
                        3.754408661907416e+0 };

        double pLow = 0.02425;
        double pHigh = 1 - pLow;

        double x;
        if (p < pLow)
        {
            double q = Math.Sqrt(-2 * Math.Log(p));
            x = (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
        }
        else if (p <= pHigh)
        {
            double q = p - 0.5;
            double r = q * q;
            x = (((((a[0] * r + a[1]) * r + a[2]) * r + a[3]) * r + a[4]) * r + a[5]) * q /
                (((((b[0] * r + b[1]) * r + b[2]) * r + b[3]) * r + b[4]) * r + 1);
        }
        else
        {
            double q = Math.Sqrt(-2 * Math.Log(1 - p));
            x = -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
                ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
        }

        return x;
    }
}

// ─── Result records ──────────────────────────────────────────────

/// <summary>
/// One attribute (P/C/U) data point: sample size + defect/defect-count.
/// </summary>
public class AttributePoint
{
    public double SampleSize { get; set; }
    public double Defects { get; set; }
}

public class ControlLimitsResult
{
    public double ClXbar { get; set; }
    public double UclXbar { get; set; }
    public double LclXbar { get; set; }
    public double ClR { get; set; }
    public double UclR { get; set; }
    public double LclR { get; set; }
    public double SigmaEstimate { get; set; }
}

public class CpkResult
{
    public double Cp { get; set; }
    public double Cpk { get; set; }
    public double Pp { get; set; }
    public double Ppk { get; set; }
    public double SigmaWithin { get; set; }
    public double SigmaOverall { get; set; }
    public double Mean { get; set; }
    public string Grade { get; set; } = string.Empty;
    public double EstimatedPpm { get; set; }
}

public class RuleViolationResult
{
    public int RuleNumber { get; set; }
    public int Index { get; set; }
    public string Description { get; set; } = string.Empty;

    public RuleViolationResult(int ruleNumber, int index, string description)
    {
        RuleNumber = ruleNumber;
        Index = index;
        Description = description;
    }
}

namespace QM_AI.API.Services;

/// <summary>
/// ANOVA (Analysis of Variance) service for one-way ANOVA calculations.
/// Used to determine if different factors (operator, machine, material, method, environment)
/// have statistically significant effects on quality metrics.
/// </summary>
public class AnovaService
{
    /// <summary>
    /// Perform one-way ANOVA analysis.
    /// </summary>
    /// <param name="groups">Dictionary of group label to list of values</param>
    /// <returns>ANOVA result with SS, df, MS, F-ratio, and p-value</returns>
    public AnovaResult CalculateOneWayAnova(Dictionary<string, List<double>> groups)
    {
        if (groups == null || groups.Count < 2)
            throw new ArgumentException("At least 2 groups are required for ANOVA.");

        int k = groups.Count; // Number of groups
        int totalN = groups.Sum(g => g.Value.Count);

        if (totalN < k + 1)
            throw new ArgumentException("Total sample size must be greater than number of groups.");

        // Grand mean
        double grandMean = groups.SelectMany(g => g.Value).Average();

        // Calculate sums
        double ssBetween = 0;
        double ssWithin = 0;
        var groupResults = new List<AnovaGroupResult>();

        foreach (var group in groups)
        {
            var values = group.Value;
            int n = values.Count;
            double groupMean = values.Average();

            // SS between: n_j * (mean_j - grand_mean)^2
            ssBetween += n * Math.Pow(groupMean - grandMean, 2);

            // SS within: sum of (x_ij - mean_j)^2
            ssWithin += values.Sum(v => Math.Pow(v - groupMean, 2));

            groupResults.Add(new AnovaGroupResult
            {
                Label = group.Key,
                Count = n,
                Mean = Math.Round(groupMean, 6),
                Variance = n > 1 ? Math.Round(values.Select(v => Math.Pow(v - groupMean, 2)).Sum() / (n - 1), 6) : 0
            });
        }

        double ssTotal = ssBetween + ssWithin;

        int dfBetween = k - 1;
        int dfWithin = totalN - k;
        int dfTotal = totalN - 1;

        double msBetween = ssBetween / dfBetween;
        double msWithin = ssWithin / dfWithin;

        double fRatio = msBetween / msWithin;

        // Calculate p-value using F-distribution
        double pValue = CalculateFDistributionPValue(fRatio, dfBetween, dfWithin);

        bool significant = pValue < 0.05;

        // Calculate eta-squared (effect size)
        double etaSquared = ssBetween / ssTotal;

        return new AnovaResult
        {
            Source = "between_groups",
            SumOfSquares = Math.Round(ssBetween, 4),
            DegreesFreedom = dfBetween,
            MeanSquare = Math.Round(msBetween, 4),
            FRatio = Math.Round(fRatio, 4),
            PValue = Math.Round(pValue, 6),
            Significant = significant,
            TotalSumOfSquares = Math.Round(ssTotal, 4),
            TotalDegreesFreedom = dfTotal,
            WithinSumOfSquares = Math.Round(ssWithin, 4),
            WithinDegreesFreedom = dfWithin,
            WithinMeanSquare = Math.Round(msWithin, 4),
            EtaSquared = Math.Round(etaSquared, 4),
            GrandMean = Math.Round(grandMean, 6),
            GroupResults = groupResults
        };
    }

    /// <summary>
    /// Calculate p-value from F-distribution using approximation.
    /// Uses the incomplete beta function approximation.
    /// </summary>
    private double CalculateFDistributionPValue(double f, int df1, int df2)
    {
        if (f <= 0) return 1.0;
        if (double.IsInfinity(f)) return 0.0;

        // Use regularized incomplete beta function
        double x = (double)df1 * f / ((double)df1 * f + (double)df2);
        double a = df1 / 2.0;
        double b = df2 / 2.0;

        return 1.0 - RegularizedIncompleteBeta(x, a, b);
    }

    /// <summary>
    /// Regularized incomplete beta function I_x(a,b) using continued fraction approximation.
    /// </summary>
    private double RegularizedIncompleteBeta(double x, double a, double b)
    {
        if (x < 0 || x > 1) return 0;
        if (x == 0 || x == 1) return x;

        // Use symmetry: I_x(a,b) = 1 - I_{1-x}(b,a)
        if (x > (a + 1) / (a + b + 2))
            return 1.0 - RegularizedIncompleteBeta(1 - x, b, a);

        // Continued fraction approximation (Lentz's method)
        double lbeta = LogBeta(a, b);
        double front = Math.Exp(Math.Log(x) * a + Math.Log(1 - x) * b - lbeta) / a;

        double f = 1.0;
        double c = 1.0;
        double d = 1.0 - (a + b) * x / (a + 1);
        if (Math.Abs(d) < 1e-30) d = 1e-30;
        d = 1.0 / d;
        f = d;

        const int maxIter = 200;
        const double epsilon = 3e-12;

        for (int m = 1; m <= maxIter; m++)
        {
            // 2m step
            double numerator = m * (b - m) * x / ((a + 2 * m - 1) * (a + 2 * m));
            d = 1.0 + numerator * d;
            if (Math.Abs(d) < 1e-30) d = 1e-30;
            c = 1.0 + numerator / c;
            if (Math.Abs(c) < 1e-30) c = 1e-30;
            d = 1.0 / d;
            f *= d * c;

            // 2m+1 step
            numerator = -(a + m) * (a + b + m) * x / ((a + 2 * m) * (a + 2 * m + 1));
            d = 1.0 + numerator * d;
            if (Math.Abs(d) < 1e-30) d = 1e-30;
            c = 1.0 + numerator / c;
            if (Math.Abs(c) < 1e-30) c = 1e-30;
            d = 1.0 / d;
            double delta = d * c;
            f *= delta;

            if (Math.Abs(delta - 1.0) < epsilon)
                break;
        }

        return front * f;
    }

    /// <summary>
    /// Log of beta function.
    /// </summary>
    private double LogBeta(double a, double b)
    {
        return LogGamma(a) + LogGamma(b) - LogGamma(a + b);
    }

    /// <summary>
    /// Log of gamma function using Stirling's approximation (Lanczos).
    /// </summary>
    private double LogGamma(double x)
    {
        if (x <= 0)
            return double.NaN;

        // Coefficients for Lanczos approximation
        double[] coeff = {
            76.18009172947146,
            -86.50532032941677,
            24.01409824083091,
            -1.231739572450155,
            0.1208650973866179e-2,
            -0.5395239384953e-5
        };

        double y = x;
        double tmp = x + 5.5;
        tmp -= (x + 0.5) * Math.Log(tmp);

        double ser = 1.000000000190015;
        for (int j = 0; j < 6; j++)
        {
            ser += coeff[j] / ++y;
        }

        return -tmp + Math.Log(2.5066282746310005 * ser / x);
    }

    /// <summary>
    /// Calculate ANOVA for multiple factors (operator, machine, material, method, environment).
    /// </summary>
    public List<AnovaResult> CalculateMultiFactorAnova(Dictionary<string, Dictionary<string, List<double>>> factorGroups)
    {
        var results = new List<AnovaResult>();

        foreach (var factor in factorGroups)
        {
            string factorName = factor.Key;
            var groups = factor.Value;

            if (groups.Count >= 2)
            {
                var result = CalculateOneWayAnova(groups);
                result.Source = factorName;
                results.Add(result);
            }
        }

        return results;
    }
}

// ─── Result models ──────────────────────────────────────────────

public class AnovaResult
{
    public string Source { get; set; } = string.Empty;
    public double SumOfSquares { get; set; }
    public int DegreesFreedom { get; set; }
    public double MeanSquare { get; set; }
    public double FRatio { get; set; }
    public double PValue { get; set; }
    public bool Significant { get; set; }
    public double TotalSumOfSquares { get; set; }
    public int TotalDegreesFreedom { get; set; }
    public double WithinSumOfSquares { get; set; }
    public int WithinDegreesFreedom { get; set; }
    public double WithinMeanSquare { get; set; }
    public double EtaSquared { get; set; }
    public double GrandMean { get; set; }
    public List<AnovaGroupResult> GroupResults { get; set; } = new();
}

public class AnovaGroupResult
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Mean { get; set; }
    public double Variance { get; set; }
}

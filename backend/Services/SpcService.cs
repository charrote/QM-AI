using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M06;
using QM_AI.API.Models.M06;
using QM_AI.API.Models.M02_Inspection;

namespace QM_AI.API.Services;

public class SpcService
{
    private readonly AppDbContext _db;
    private readonly SpcAlgorithmService _algorithm;
    private readonly AnovaService _anovaService;

    public SpcService(AppDbContext db, SpcAlgorithmService algorithm, AnovaService anovaService)
    {
        _db = db;
        _algorithm = algorithm;
        _anovaService = anovaService;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Control Charts
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<SpcControlChartListDto>> ListCharts(PagedRequest req)
    {
        var query = _db.SpcControlCharts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(c => c.Name.Contains(req.Keyword) || c.ParameterCode.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(c => c.ChartType == req.Status);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(c => new SpcControlChartListDto
            {
                Id = c.Id,
                Name = c.Name,
                ProcessId = c.ProcessId,
                ParameterCode = c.ParameterCode,
                ChartType = c.ChartType,
                SubgroupSize = c.SubgroupSize,
                Usl = c.Usl,
                Lsl = c.Lsl,
                TargetValue = c.TargetValue,
                Cl = c.Cl,
                Ucl = c.Ucl,
                Lcl = c.Lcl,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<SpcControlChartListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        };
    }

    public async Task<SpcControlChartDetailDto?> GetChart(long id)
    {
        return await _db.SpcControlCharts
            .Where(c => c.Id == id)
            .Select(c => new SpcControlChartDetailDto
            {
                Id = c.Id,
                Name = c.Name,
                ProcessId = c.ProcessId,
                ParameterCode = c.ParameterCode,
                ChartType = c.ChartType,
                SubgroupSize = c.SubgroupSize,
                Usl = c.Usl,
                Lsl = c.Lsl,
                TargetValue = c.TargetValue,
                Cl = c.Cl,
                Ucl = c.Ucl,
                Lcl = c.Lcl,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                CreatedBy = c.CreatedBy,
                DataPointCount = c.DataPoints.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SpcControlChartDetailDto> CreateChart(CreateSpcControlChartDto dto, long userId)
    {
        var chart = new SpcControlChart
        {
            Name = dto.Name,
            ProcessId = dto.ProcessId,
            ParameterCode = dto.ParameterCode,
            ChartType = dto.ChartType,
            SubgroupSize = dto.SubgroupSize,
            Usl = dto.Usl,
            Lsl = dto.Lsl,
            TargetValue = dto.TargetValue,
            CreatedBy = userId
        };

        _db.SpcControlCharts.Add(chart);
        await _db.SaveChangesAsync();

        // Auto-create 8 Western Electric alert rules
        var defaultRules = GetDefaultAlertRules(chart.Id);
        _db.SpcAlertRules.AddRange(defaultRules);
        await _db.SaveChangesAsync();

        return (await GetChart(chart.Id))!;
    }

    public async Task<SpcControlChartDetailDto?> UpdateChart(long id, UpdateSpcControlChartDto dto)
    {
        var chart = await _db.SpcControlCharts.FindAsync(id);
        if (chart == null) return null;

        chart.Name = dto.Name;
        chart.SubgroupSize = dto.SubgroupSize;
        chart.Usl = dto.Usl;
        chart.Lsl = dto.Lsl;
        chart.TargetValue = dto.TargetValue;
        chart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetChart(id);
    }

    public async Task<bool> DeleteChart(long id)
    {
        var chart = await _db.SpcControlCharts.FindAsync(id);
        if (chart == null) return false;

        _db.SpcControlCharts.Remove(chart);
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Data Points
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<SpcDataPointListDto>> ListDataPoints(long chartId, PagedRequest req)
    {
        var query = _db.SpcDataPoints.Where(p => p.ChartId == chartId);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.SubgroupIndex)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new SpcDataPointListDto
            {
                Id = p.Id,
                ChartId = p.ChartId,
                SubgroupIndex = p.SubgroupIndex,
                IndividualValues = p.IndividualValues,
                SubgroupMean = p.SubgroupMean,
                SubgroupRange = p.SubgroupRange,
                MeasuredAt = p.MeasuredAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<SpcDataPointListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        };
    }

    public async Task<SpcDataPointListDto> CreateDataPoint(CreateSpcDataPointDto dto)
    {
        var values = System.Text.Json.JsonSerializer.Deserialize<List<double>>(dto.IndividualValues) ?? new List<double>();

        var dataPoint = new SpcDataPoint
        {
            ChartId = dto.ChartId,
            SubgroupIndex = dto.SubgroupIndex,
            IndividualValues = dto.IndividualValues,
            SubgroupMean = values.Count > 0 ? (decimal)values.Average() : null,
            SubgroupRange = values.Count > 0 ? (decimal)(values.Max() - values.Min()) : null,
            MeasuredAt = dto.MeasuredAt
        };

        _db.SpcDataPoints.Add(dataPoint);

        // Recalculate control limits after adding data point
        await RecalculateControlLimits(dto.ChartId);

        await _db.SaveChangesAsync();

        return new SpcDataPointListDto
        {
            Id = dataPoint.Id,
            ChartId = dataPoint.ChartId,
            SubgroupIndex = dataPoint.SubgroupIndex,
            IndividualValues = dataPoint.IndividualValues,
            SubgroupMean = dataPoint.SubgroupMean,
            SubgroupRange = dataPoint.SubgroupRange,
            MeasuredAt = dataPoint.MeasuredAt,
            CreatedAt = dataPoint.CreatedAt
        };
    }

    public async Task<List<SpcDataPointListDto>> BatchCreateDataPoints(BatchCreateDataPointsDto dto)
    {
        var results = new List<SpcDataPointListDto>();

        foreach (var dp in dto.DataPoints)
        {
            var result = await CreateDataPoint(dp);
            results.Add(result);
        }

        return results;
    }

    public async Task<bool> DeleteDataPoint(long id)
    {
        var dp = await _db.SpcDataPoints.FindAsync(id);
        if (dp == null) return false;

        long chartId = dp.ChartId;
        _db.SpcDataPoints.Remove(dp);

        await RecalculateControlLimits(chartId);
        await _db.SaveChangesAsync();

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  Analysis / SPC Engine
    // ═══════════════════════════════════════════════════════════════

    public async Task<SpcAnalysisReportDto> AnalyzeChart(SpcAnalyzeRequestDto dto)
    {
        var chart = await _db.SpcControlCharts.FindAsync(dto.ChartId);
        if (chart == null)
            throw new InvalidOperationException($"SPC chart #{dto.ChartId} not found.");

        // Get data points
        var query = _db.SpcDataPoints.Where(p => p.ChartId == dto.ChartId);

        if (dto.PeriodStart.HasValue)
            query = query.Where(p => p.MeasuredAt >= dto.PeriodStart.Value);
        if (dto.PeriodEnd.HasValue)
            query = query.Where(p => p.MeasuredAt <= dto.PeriodEnd.Value);

        var dataPoints = await query
            .OrderBy(p => p.SubgroupIndex)
            .ToListAsync();

        if (!dataPoints.Any())
            return new SpcAnalysisReportDto();

        // Parse individual values and build means array
        var means = new List<double>();
        var allValues = new List<double>();
        var subgroups = new List<double[]>();

        foreach (var dp in dataPoints)
        {
            var values = System.Text.Json.JsonSerializer.Deserialize<List<double>>(dp.IndividualValues) ?? new List<double>();
            if (values.Count > 0)
            {
                subgroups.Add(values.ToArray());
                means.Add((double)(dp.SubgroupMean ?? (decimal)values.Average()));
                allValues.AddRange(values);
            }
        }

        if (!means.Any())
            return new SpcAnalysisReportDto();

        // Calculate control limits
        ControlLimitsResult limits;
        try
        {
            limits = chart.ChartType switch
            {
                "Xbar_S" => _algorithm.CalculateXbarSControlLimits(subgroups),
                "I_MR" => _algorithm.CalculateImrControlLimits(allValues),
                _ => _algorithm.CalculateXbarRControlLimits(subgroups)
            };
        }
        catch (Exception)
        {
            return new SpcAnalysisReportDto();
        }

        // Detect rule violations
        var violations = _algorithm.DetectRuleViolations(
            means.ToArray(), limits.ClXbar, limits.UclXbar, limits.LclXbar, limits.SigmaEstimate);

        // Calculate capability if USL/LSL are set
        SpcAnalysisResultDto? capability = null;
        if (chart.Usl.HasValue && chart.Lsl.HasValue)
        {
            var cpkResult = _algorithm.CalculateCapability(
                allValues, (double)chart.Usl, (double)chart.Lsl);

            capability = new SpcAnalysisResultDto
            {
                ChartId = dto.ChartId,
                AnalysisType = "capability",
                Cp = (decimal)cpkResult.Cp,
                Cpk = (decimal)cpkResult.Cpk,
                Pp = (decimal)cpkResult.Pp,
                Ppk = (decimal)cpkResult.Ppk,
                SigmaWithin = (decimal)cpkResult.SigmaWithin,
                SigmaOverall = (decimal)cpkResult.SigmaOverall,
                EstimatedPpm = (decimal)cpkResult.EstimatedPpm,
                DataPointsUsed = allValues.Count,
                Grade = cpkResult.Grade
            };
        }

        // Save analysis result
        if (capability != null)
        {
            var analysisResult = new SpcAnalysisResult
            {
                ChartId = dto.ChartId,
                AnalysisType = "capability",
                Cp = capability.Cp,
                Cpk = capability.Cpk,
                Pp = capability.Pp,
                Ppk = capability.Ppk,
                SigmaWithin = capability.SigmaWithin,
                SigmaOverall = capability.SigmaOverall,
                EstimatedPpm = capability.EstimatedPpm,
                DataPointsUsed = capability.DataPointsUsed,
                AnalysisPeriodStart = dto.PeriodStart,
                AnalysisPeriodEnd = dto.PeriodEnd
            };
            _db.SpcAnalysisResults.Add(analysisResult);
            await _db.SaveChangesAsync();
        }

        // Save triggered alerts
        var enabledRules = await _db.SpcAlertRules
            .Where(r => r.ChartId == dto.ChartId && r.Enabled)
            .ToListAsync();

        foreach (var violation in violations)
        {
            var rule = enabledRules.FirstOrDefault(r => r.RuleNumber == violation.RuleNumber);
            if (rule != null)
            {
                var trigger = new SpcAlertTrigger
                {
                    ChartId = dto.ChartId,
                    RuleId = rule.Id,
                    RuleNumber = violation.RuleNumber,
                    ViolatedPointIndex = violation.Index,
                    Detail = System.Text.Json.JsonSerializer.Serialize(new { violation.Description, violation.Index }),
                    TriggeredAt = DateTime.UtcNow
                };
                _db.SpcAlertTriggers.Add(trigger);
            }
        }

        if (violations.Any())
            await _db.SaveChangesAsync();

        // Update control limits on chart
        chart.Cl = (decimal)limits.ClXbar;
        chart.Ucl = (decimal)limits.UclXbar;
        chart.Lcl = (decimal)limits.LclXbar;
        chart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new SpcAnalysisReportDto
        {
            Chart = new SpcControlChartListDto
            {
                Id = chart.Id,
                Name = chart.Name,
                ProcessId = chart.ProcessId,
                ParameterCode = chart.ParameterCode,
                ChartType = chart.ChartType,
                SubgroupSize = chart.SubgroupSize,
                Usl = chart.Usl,
                Lsl = chart.Lsl,
                TargetValue = chart.TargetValue,
                Cl = chart.Cl,
                Ucl = chart.Ucl,
                Lcl = chart.Lcl
            },
            ControlLimits = new SpcControlLimitsDto
            {
                ClXbar = limits.ClXbar,
                UclXbar = limits.UclXbar,
                LclXbar = limits.LclXbar,
                ClR = limits.ClR,
                UclR = limits.UclR,
                LclR = limits.LclR
            },
            Capability = capability,
            Violations = violations.Select(v => new SpcRuleViolationDto
            {
                RuleNumber = v.RuleNumber,
                Index = v.Index,
                Description = v.Description
            }).ToList(),
            DataPoints = dataPoints.Select(p => new SpcDataPointListDto
            {
                Id = p.Id,
                ChartId = p.ChartId,
                SubgroupIndex = p.SubgroupIndex,
                IndividualValues = p.IndividualValues,
                SubgroupMean = p.SubgroupMean,
                SubgroupRange = p.SubgroupRange,
                MeasuredAt = p.MeasuredAt,
                CreatedAt = p.CreatedAt
            }).ToList()
        };
    }

    // ═══════════════════════════════════════════════════════════════
    //  Alert Rules
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<SpcAlertRuleListDto>> ListAlertRules(long chartId)
    {
        return await _db.SpcAlertRules
            .Where(r => r.ChartId == chartId)
            .OrderBy(r => r.RuleNumber)
            .Select(r => new SpcAlertRuleListDto
            {
                Id = r.Id,
                ChartId = r.ChartId,
                RuleNumber = r.RuleNumber,
                RuleName = r.RuleName,
                RuleDescription = r.RuleDescription,
                Enabled = r.Enabled,
                TriggerThreshold = r.TriggerThreshold,
                SigmaThreshold = r.SigmaThreshold,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<SpcAlertRuleListDto?> UpdateAlertRule(long id, UpdateSpcAlertRuleDto dto)
    {
        var rule = await _db.SpcAlertRules.FindAsync(id);
        if (rule == null) return null;

        rule.Enabled = dto.Enabled;
        rule.TriggerThreshold = dto.TriggerThreshold;
        rule.SigmaThreshold = dto.SigmaThreshold;
        rule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await _db.SpcAlertRules
            .Where(r => r.Id == id)
            .Select(r => new SpcAlertRuleListDto
            {
                Id = r.Id,
                ChartId = r.ChartId,
                RuleNumber = r.RuleNumber,
                RuleName = r.RuleName,
                RuleDescription = r.RuleDescription,
                Enabled = r.Enabled,
                TriggerThreshold = r.TriggerThreshold,
                SigmaThreshold = r.SigmaThreshold,
                UpdatedAt = r.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  Alert Triggers
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<SpcAlertTriggerListDto>> ListTriggers(long chartId, PagedRequest req)
    {
        var query = _db.SpcAlertTriggers
            .Where(t => t.ChartId == chartId)
            .AsQueryable();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.TriggeredAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(t => new SpcAlertTriggerListDto
            {
                Id = t.Id,
                ChartId = t.ChartId,
                RuleId = t.RuleId,
                RuleNumber = t.RuleNumber,
                RuleName = t.Rule != null ? t.Rule.RuleName : "",
                TriggeredAt = t.TriggeredAt,
                ViolatedPointIndex = t.ViolatedPointIndex,
                Detail = t.Detail,
                Resolved = t.Resolved,
                ResolvedAt = t.ResolvedAt
            })
            .ToListAsync();

        return new PagedResult<SpcAlertTriggerListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize,
        };
    }

    public async Task<bool> ResolveTrigger(long id)
    {
        var trigger = await _db.SpcAlertTriggers.FindAsync(id);
        if (trigger == null) return false;

        trigger.Resolved = true;
        trigger.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ANOVA
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<SpcAnovaResultDto>> RunAnova(SpcAnovaRequestDto dto)
    {
        var chart = await _db.SpcControlCharts.FindAsync(dto.ChartId);
        if (chart == null)
            throw new InvalidOperationException($"SPC chart #{dto.ChartId} not found.");

        var factorData = new Dictionary<string, Dictionary<string, List<double>>>();

        foreach (var factor in dto.Factors)
        {
            var source = factor.Source;
            if (!factorData.ContainsKey(source))
                factorData[source] = new Dictionary<string, List<double>>();

            factorData[source][factor.Label] = factor.Values;
        }

        var anovaResults = _anovaService.CalculateMultiFactorAnova(factorData);

        // Save results
        var savedResults = new List<SpcAnovaResultDto>();
        foreach (var result in anovaResults)
        {
            var entity = new SpcAnovaResult
            {
                ChartId = dto.ChartId,
                Source = result.Source,
                SumOfSquares = (decimal)result.SumOfSquares,
                DegreesFreedom = result.DegreesFreedom,
                MeanSquare = (decimal)result.MeanSquare,
                FRatio = (decimal)result.FRatio,
                PValue = (decimal)result.PValue,
                Significant = result.Significant,
                AnalysisDate = DateTime.UtcNow
            };

            _db.SpcAnovaResults.Add(entity);
            await _db.SaveChangesAsync();

            savedResults.Add(new SpcAnovaResultDto
            {
                Id = entity.Id,
                ChartId = entity.ChartId,
                Source = entity.Source,
                SumOfSquares = entity.SumOfSquares,
                DegreesFreedom = entity.DegreesFreedom,
                MeanSquare = entity.MeanSquare,
                FRatio = entity.FRatio,
                PValue = entity.PValue,
                Significant = entity.Significant,
                AnalysisDate = entity.AnalysisDate
            });
        }

        return savedResults;
    }

    public async Task<List<SpcAnovaResultDto>> ListAnovaResults(long chartId)
    {
        return await _db.SpcAnovaResults
            .Where(r => r.ChartId == chartId)
            .OrderByDescending(r => r.AnalysisDate)
            .Select(r => new SpcAnovaResultDto
            {
                Id = r.Id,
                ChartId = r.ChartId,
                Source = r.Source,
                SumOfSquares = r.SumOfSquares,
                DegreesFreedom = r.DegreesFreedom,
                MeanSquare = r.MeanSquare,
                FRatio = r.FRatio,
                PValue = r.PValue,
                Significant = r.Significant,
                AnalysisDate = r.AnalysisDate
            })
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  History / Analysis Results
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<SpcAnalysisResultDto>> ListAnalysisResults(long chartId)
    {
        return await _db.SpcAnalysisResults
            .Where(r => r.ChartId == chartId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new SpcAnalysisResultDto
            {
                Id = r.Id,
                ChartId = r.ChartId,
                AnalysisType = r.AnalysisType,
                Cp = r.Cp,
                Cpk = r.Cpk,
                Pp = r.Pp,
                Ppk = r.Ppk,
                SigmaWithin = r.SigmaWithin,
                SigmaOverall = r.SigmaOverall,
                EstimatedPpm = r.EstimatedPpm,
                DataPointsUsed = r.DataPointsUsed,
                AnalysisPeriodStart = r.AnalysisPeriodStart,
                AnalysisPeriodEnd = r.AnalysisPeriodEnd,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    // ═══════════════════════════════════════════════════════════════
    //  Private Helpers
    // ═══════════════════════════════════════════════════════════════

    private async Task RecalculateControlLimits(long chartId)
    {
        var chart = await _db.SpcControlCharts.FindAsync(chartId);
        if (chart == null) return;

        var dataPoints = await _db.SpcDataPoints
            .Where(p => p.ChartId == chartId)
            .OrderBy(p => p.SubgroupIndex)
            .ToListAsync();

        if (dataPoints.Count < 2) return;

        var subgroups = new List<double[]>();
        foreach (var dp in dataPoints)
        {
            var values = System.Text.Json.JsonSerializer.Deserialize<List<double>>(dp.IndividualValues);
            if (values != null && values.Count > 0)
                subgroups.Add(values.ToArray());
        }

        if (subgroups.Count < 2) return;

        try
        {
            ControlLimitsResult limits = chart.ChartType switch
            {
                "Xbar_S" => _algorithm.CalculateXbarSControlLimits(subgroups),
                "I_MR" => _algorithm.CalculateImrControlLimits(subgroups.SelectMany(s => s).ToList()),
                _ => _algorithm.CalculateXbarRControlLimits(subgroups)
            };

            chart.Cl = (decimal)limits.ClXbar;
            chart.Ucl = (decimal)limits.UclXbar;
            chart.Lcl = (decimal)limits.LclXbar;
            chart.UpdatedAt = DateTime.UtcNow;
        }
        catch
        {
            // Silently fail - not enough data
        }
    }

    // ═══════════════════════════════════════════════════════════════
    //  Data Sources (从业务模块拉取数据到SPC)
    // ═══════════════════════════════════════════════════════════════

    public async Task<List<SpcDataSourceDto>> ListDataSources(long chartId)
    {
        return await _db.SpcDataSources
            .Where(s => s.ChartId == chartId)
            .Select(s => new SpcDataSourceDto
            {
                Id = s.Id,
                ChartId = s.ChartId,
                SourceType = s.SourceType,
                InspectionItemId = s.InspectionItemId,
                InspectionItemName = s.InspectionItem != null ? s.InspectionItem.ItemName : null,
                ProductId = s.ProductId,
                ProcessId = s.ProcessId,
                SupplierId = s.SupplierId,
                CustomerId = s.CustomerId,
                EquipmentId = s.EquipmentId,
                CreatedAt = s.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<SpcDataSourceDto> CreateDataSource(CreateSpcDataSourceDto dto)
    {
        var entity = new SpcDataSource
        {
            ChartId = dto.ChartId,
            SourceType = dto.SourceType,
            InspectionItemId = dto.InspectionItemId,
            ProductId = dto.ProductId,
            ProcessId = dto.ProcessId,
            SupplierId = dto.SupplierId,
            CustomerId = dto.CustomerId,
            EquipmentId = dto.EquipmentId,
        };

        _db.SpcDataSources.Add(entity);
        await _db.SaveChangesAsync();

        return (await ListDataSources(dto.ChartId)).First(s => s.Id == entity.Id);
    }

    public async Task<bool> DeleteDataSource(long id)
    {
        var entity = await _db.SpcDataSources.FindAsync(id);
        if (entity == null) return false;

        _db.SpcDataSources.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private List<SpcAlertRule> GetDefaultAlertRules(long chartId)
    {
        return new List<SpcAlertRule>
        {
            new() { ChartId = chartId, RuleNumber = 1, RuleName = "1点超出3σ控制限", RuleDescription = "任何数据点超出UCL或LCL", Enabled = true, TriggerThreshold = 1, SigmaThreshold = 3.0m },
            new() { ChartId = chartId, RuleNumber = 2, RuleName = "连续9点在CL同侧", RuleDescription = "连续9个点位于中心线同一侧", Enabled = true, TriggerThreshold = 9, SigmaThreshold = 0m },
            new() { ChartId = chartId, RuleNumber = 3, RuleName = "连续6点递增或递减", RuleDescription = "连续6个点单调上升或下降", Enabled = true, TriggerThreshold = 6, SigmaThreshold = 0m },
            new() { ChartId = chartId, RuleNumber = 4, RuleName = "连续14点上下交替", RuleDescription = "连续14个点呈现上下交替模式", Enabled = true, TriggerThreshold = 14, SigmaThreshold = 0m },
            new() { ChartId = chartId, RuleNumber = 5, RuleName = "连续3点中2点超出2σ", RuleDescription = "连续3点中有2点落在2σ和3σ之间（同一侧）", Enabled = true, TriggerThreshold = 2, SigmaThreshold = 2.0m },
            new() { ChartId = chartId, RuleNumber = 6, RuleName = "连续5点中4点超出1σ", RuleDescription = "连续5点中有4点落在1σ和2σ之间（同一侧）", Enabled = true, TriggerThreshold = 4, SigmaThreshold = 1.0m },
            new() { ChartId = chartId, RuleNumber = 7, RuleName = "连续15点在1σ内", RuleDescription = "连续15个点落在中心线1σ范围内（任一侧）", Enabled = true, TriggerThreshold = 15, SigmaThreshold = 1.0m },
            new() { ChartId = chartId, RuleNumber = 8, RuleName = "连续8点超出1σ", RuleDescription = "连续8个点落在1σ范围外（双侧）", Enabled = true, TriggerThreshold = 8, SigmaThreshold = 1.0m },
        };
    }
}

using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs;
using QM_AI.API.DTOs.M04;
using QM_AI.API.Models.M04;

namespace QM_AI.API.Services;

/// <summary>
/// IPQC 过程检验业务服务 — S4-01 + S4-02 + S4-03
/// </summary>
public class IpqcService
{
    private readonly AppDbContext _db;
    private readonly ClosureRuleEngine _ruleEngine;

    public IpqcService(AppDbContext db, ClosureRuleEngine ruleEngine)
    {
        _db = db;
        _ruleEngine = ruleEngine;
    }

    // ═══════════════════════════════════════════════════════════════
    // 首件检验 (First Piece Inspection)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<IpqcFirstPieceListDto>> ListFirstPieces(PagedRequest req)
    {
        var query = _db.IpqcFirstPieces.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(f =>
                f.FpNo.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(f => f.Conclusion == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(f => new IpqcFirstPieceListDto
            {
                Id = f.Id,
                FpNo = f.FpNo,
                WorkOrderId = f.WorkOrderId,
                ProcessId = f.ProcessId,
                EquipmentId = f.EquipmentId,
                Shift = f.Shift,
                Reason = f.Reason,
                Conclusion = f.Conclusion,
                AllowedToProduce = f.AllowedToProduce,
                CheckedAt = f.CheckedAt,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<IpqcFirstPieceListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<IpqcFirstPieceDetailDto?> GetFirstPiece(long id)
    {
        var entity = await _db.IpqcFirstPieces
            .FirstOrDefaultAsync(f => f.Id == id);
        if (entity == null) return null;

        var items = await _db.IpqcFirstPieceItems
            .Where(i => i.FirstPieceId == id)
            .Select(i => new IpqcFirstPieceItemDto
            {
                Id = i.Id,
                FirstPieceId = i.FirstPieceId,
                ItemName = i.ItemName,
                ItemCode = i.ItemCode,
                Usl = i.Usl,
                Lsl = i.Lsl,
                DataType = i.DataType,
                ActualValue = i.ActualValue,
                Result = i.Result,
                ImageUrls = i.ImageUrls,
                Remarks = i.Remarks
            })
            .ToListAsync();

        return new IpqcFirstPieceDetailDto
        {
            Id = entity.Id,
            FpNo = entity.FpNo,
            WorkOrderId = entity.WorkOrderId,
            ProcessId = entity.ProcessId,
            EquipmentId = entity.EquipmentId,
            OperatorId = entity.OperatorId,
            Shift = entity.Shift,
            Reason = entity.Reason,
            Conclusion = entity.Conclusion,
            AllowedToProduce = entity.AllowedToProduce,
            Inspector = null,
            CheckedAt = entity.CheckedAt,
            CreatedAt = entity.CreatedAt,
            Items = items
        };
    }

    public async Task<IpqcFirstPieceDetailDto> CreateFirstPiece(CreateIpqcFirstPieceDto dto)
    {
        // 生成首件检验单号
        var fpNo = dto.FpNo ?? await GenerateFirstPieceNo();

        var entity = new IpqcFirstPiece
        {
            FpNo = fpNo,
            WorkOrderId = dto.WorkOrderId,
            ProcessId = dto.ProcessId,
            EquipmentId = dto.EquipmentId,
            OperatorId = dto.OperatorId,
            Shift = dto.Shift,
            Reason = dto.Reason,
            Conclusion = "pending",
            AllowedToProduce = false
        };

        if (dto.Items.Count > 0)
        {
            entity.Items = dto.Items.Select(i => new IpqcFirstPieceItem
            {
                InspectionItemId = i.InspectionItemId,
                ItemName = i.ItemName,
                ItemCode = i.ItemCode,
                Usl = i.Usl,
                Lsl = i.Lsl,
                DataType = i.DataType,
                ActualValue = i.ActualValue,
                Result = i.Result,
                ImageUrls = i.ImageUrls,
                Remarks = i.Remarks
            }).ToList();
        }

        _db.IpqcFirstPieces.Add(entity);
        await _db.SaveChangesAsync();

        return (await GetFirstPiece(entity.Id))!;
    }

    /// <summary>
    /// 提交首件检验结果 — 判定合格/不合格
    /// </summary>
    public async Task<IpqcFirstPieceDetailDto?> SubmitFirstPiece(long id, SubmitIpqcFirstPieceDto dto)
    {
        var entity = await _db.IpqcFirstPieces
            .Include(f => f.Items)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (entity == null) return null;

        entity.Conclusion = dto.Conclusion;
        entity.AllowedToProduce = dto.AllowedToProduce;
        entity.CheckedAt = DateTime.UtcNow;

        if (dto.Items != null && dto.Items.Count > 0)
        {
            foreach (var itemDto in dto.Items)
            {
                var existing = entity.Items?.FirstOrDefault(i => i.Id == itemDto.Id);
                if (existing != null)
                {
                    existing.Result = itemDto.Result;
                    existing.ActualValue = itemDto.ActualValue;
                    existing.Remarks = itemDto.Remarks;
                    existing.ImageUrls = itemDto.ImageUrls;
                }
            }
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetFirstPiece(id);
    }

    public async Task<bool> DeleteFirstPiece(long id)
    {
        var entity = await _db.IpqcFirstPieces.FindAsync(id);
        if (entity == null) return false;

        _db.IpqcFirstPieces.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // 巡检计划 (Patrol Plan)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<IpqcPatrolPlanListDto>> ListPatrolPlans(PagedRequest req)
    {
        var query = _db.IpqcPatrolPlans.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p => p.PlanNo.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(p => p.Status == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new IpqcPatrolPlanListDto
            {
                Id = p.Id,
                PlanNo = p.PlanNo,
                ProcessId = p.ProcessId,
                EquipmentId = p.EquipmentId,
                PatrolIntervalMin = p.PatrolIntervalMin,
                AutoGenerate = p.AutoGenerate,
                Status = p.Status,
                Inspector = p.Inspector,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<IpqcPatrolPlanListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<IpqcPatrolPlanListDto?> GetPatrolPlan(long id)
    {
        return await _db.IpqcPatrolPlans
            .Where(p => p.Id == id)
            .Select(p => new IpqcPatrolPlanListDto
            {
                Id = p.Id,
                PlanNo = p.PlanNo,
                ProcessId = p.ProcessId,
                EquipmentId = p.EquipmentId,
                PatrolIntervalMin = p.PatrolIntervalMin,
                AutoGenerate = p.AutoGenerate,
                Status = p.Status,
                Inspector = p.Inspector,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IpqcPatrolPlanListDto> CreatePatrolPlan(CreateIpqcPatrolPlanDto dto)
    {
        var planNo = dto.PlanNo ?? await GeneratePatrolPlanNo();

        var entity = new IpqcPatrolPlan
        {
            PlanNo = planNo,
            ProcessId = dto.ProcessId,
            EquipmentId = dto.EquipmentId,
            PatrolIntervalMin = dto.PatrolIntervalMin,
            AutoGenerate = dto.AutoGenerate,
            Inspector = dto.Inspector,
            Status = "active"
        };

        _db.IpqcPatrolPlans.Add(entity);
        await _db.SaveChangesAsync();

        // 如果启用自动生成，立即生成一批巡检任务
        if (entity.AutoGenerate)
        {
            await AutoGeneratePatrols(entity.Id, null, DateTime.UtcNow, 4);
        }

        return (await GetPatrolPlan(entity.Id))!;
    }

    public async Task<IpqcPatrolPlanListDto?> UpdatePatrolPlan(long id, UpdateIpqcPatrolPlanDto dto)
    {
        var entity = await _db.IpqcPatrolPlans.FindAsync(id);
        if (entity == null) return null;

        if (dto.PatrolIntervalMin.HasValue) entity.PatrolIntervalMin = dto.PatrolIntervalMin.Value;
        if (dto.AutoGenerate.HasValue) entity.AutoGenerate = dto.AutoGenerate.Value;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.Inspector != null) entity.Inspector = dto.Inspector;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetPatrolPlan(id);
    }

    public async Task<bool> DeletePatrolPlan(long id)
    {
        var entity = await _db.IpqcPatrolPlans.FindAsync(id);
        if (entity == null) return false;

        _db.IpqcPatrolPlans.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // S4-02: 巡检计划自动生成 (按时间间隔)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 根据巡检计划自动生成一批巡检任务
    /// </summary>
    public async Task<List<IpqcPatrolListDto>> AutoGeneratePatrols(long planId, long? workOrderId, DateTime startTime, int count)
    {
        var plan = await _db.IpqcPatrolPlans.FindAsync(planId);
        if (plan == null || plan.Status != "active")
            throw new InvalidOperationException("巡检计划不存在或已暂停");

        var generated = new List<IpqcPatrol>();
        for (int i = 0; i < count; i++)
        {
            var patrolNo = await GeneratePatrolNo();
            var patrol = new IpqcPatrol
            {
                PatrolNo = patrolNo,
                PatrolPlanId = planId,
                WorkOrderId = workOrderId,
                ProcessId = plan.ProcessId,
                EquipmentId = plan.EquipmentId,
                InspectorId = 0, // will be assigned later
                ScheduledTime = startTime.AddMinutes(i * plan.PatrolIntervalMin),
                Status = "scheduled",
                Conclusion = "pending"
            };
            _db.IpqcPatrols.Add(patrol);
            generated.Add(patrol);
        }

        await _db.SaveChangesAsync();

        return generated.Select(p => new IpqcPatrolListDto
        {
            Id = p.Id,
            PatrolNo = p.PatrolNo,
            PatrolPlanId = p.PatrolPlanId,
            WorkOrderId = p.WorkOrderId,
            ProcessId = p.ProcessId,
            EquipmentId = p.EquipmentId,
            ScheduledTime = p.ScheduledTime,
            Status = p.Status,
            Conclusion = p.Conclusion,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// 手动触发巡检计划生成
    /// </summary>
    public async Task<List<IpqcPatrolListDto>> GeneratePatrols(PatrolPlanGenerateRequestDto dto)
    {
        return await AutoGeneratePatrols(dto.PlanId, dto.WorkOrderId, dto.StartTime, dto.Count);
    }

    // ═══════════════════════════════════════════════════════════════
    // 巡检记录 (Patrol)
    // ═══════════════════════════════════════════════════════════════

    public async Task<PagedResult<IpqcPatrolListDto>> ListPatrols(PagedRequest req)
    {
        var query = _db.IpqcPatrols.AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Keyword))
            query = query.Where(p =>
                p.PatrolNo.Contains(req.Keyword));

        if (!string.IsNullOrWhiteSpace(req.Status))
            query = query.Where(p => p.Status == req.Status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.ScheduledTime)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new IpqcPatrolListDto
            {
                Id = p.Id,
                PatrolNo = p.PatrolNo,
                PatrolPlanId = p.PatrolPlanId,
                WorkOrderId = p.WorkOrderId,
                ProcessId = p.ProcessId,
                EquipmentId = p.EquipmentId,
                ScheduledTime = p.ScheduledTime,
                ActualTime = p.ActualTime,
                TotalChecked = p.TotalChecked,
                TotalPass = p.TotalPass,
                TotalFail = p.TotalFail,
                Conclusion = p.Conclusion,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<IpqcPatrolListDto>
        {
            Items = items,
            Total = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<IpqcPatrolDetailDto?> GetPatrol(long id)
    {
        var entity = await _db.IpqcPatrols
            .FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return null;

        var items = await _db.IpqcPatrolItems
            .Where(i => i.PatrolId == id)
            .Select(i => new IpqcPatrolItemDto
            {
                Id = i.Id,
                PatrolId = i.PatrolId,
                ItemName = i.ItemName,
                ItemCode = i.ItemCode,
                Usl = i.Usl,
                Lsl = i.Lsl,
                DataType = i.DataType,
                ActualValue = i.ActualValue,
                Result = i.Result,
                ImageUrls = i.ImageUrls
            })
            .ToListAsync();

        return new IpqcPatrolDetailDto
        {
            Id = entity.Id,
            PatrolNo = entity.PatrolNo,
            PatrolPlanId = entity.PatrolPlanId,
            WorkOrderId = entity.WorkOrderId,
            ProcessId = entity.ProcessId,
            EquipmentId = entity.EquipmentId,
            ScheduledTime = entity.ScheduledTime,
            ActualTime = entity.ActualTime,
            TotalChecked = entity.TotalChecked,
            TotalPass = entity.TotalPass,
            TotalFail = entity.TotalFail,
            Conclusion = entity.Conclusion,
            Status = entity.Status,
            Remarks = entity.Remarks,
            CreatedAt = entity.CreatedAt,
            Items = items
        };
    }

    /// <summary>
    /// 提交巡检结果
    /// </summary>
    public async Task<IpqcPatrolDetailDto?> SubmitPatrol(long id, SubmitIpqcPatrolDto dto)
    {
        var entity = await _db.IpqcPatrols
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (entity == null) return null;

        entity.Conclusion = dto.Conclusion;
        entity.Remarks = dto.Remarks;
        entity.ActualTime = DateTime.UtcNow;
        entity.Status = "completed";

        // 更新检验明细
        if (dto.Items != null && dto.Items.Count > 0)
        {
            entity.Items ??= new List<IpqcPatrolItem>();

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Id.HasValue)
                {
                    // 更新已有项
                    var existing = entity.Items.FirstOrDefault(i => i.Id == itemDto.Id.Value);
                    if (existing != null)
                    {
                        existing.Result = itemDto.Result;
                        existing.ActualValue = itemDto.ActualValue;
                        existing.ImageUrls = itemDto.ImageUrls;
                    }
                }
                else
                {
                    // 新增项
                    entity.Items.Add(new IpqcPatrolItem
                    {
                        PatrolId = id,
                        InspectionItemId = itemDto.InspectionItemId,
                        ItemName = itemDto.ItemName,
                        ItemCode = itemDto.ItemCode,
                        Usl = itemDto.Usl,
                        Lsl = itemDto.Lsl,
                        DataType = itemDto.DataType,
                        ActualValue = itemDto.ActualValue,
                        Result = itemDto.Result,
                        ImageUrls = itemDto.ImageUrls
                    });
                }
            }

            // 统计
            entity.TotalChecked = entity.Items.Count;
            entity.TotalPass = entity.Items.Count(i => i.Result == "pass");
            entity.TotalFail = entity.Items.Count(i => i.Result == "fail");
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetPatrol(id);
    }

    /// <summary>
    /// 标记巡检为已错过
    /// </summary>
    public async Task<bool> MissPatrol(long id)
    {
        var entity = await _db.IpqcPatrols.FindAsync(id);
        if (entity == null) return false;

        entity.Status = "missed";
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    // S4-03: AI 实时风险评分
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// IPQC AI 风险评分（冷启动规则评分版）
    /// </summary>
    public async Task<IpqcAiRiskScoreDto> AnalyzeRisk(int equipmentId, int processId, long? workOrderId = null)
    {
        // 获取最近 1 小时数据窗口
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);

        // 1. 获取最近该设备的巡检合格率
        var recentPatrols = await _db.IpqcPatrols
            .Where(p => p.EquipmentId == equipmentId
                     && p.Status == "completed"
                     && p.ActualTime >= oneHourAgo)
            .ToListAsync();

        var recentPassRate = recentPatrols.Count > 0
            ? (double)recentPatrols.Sum(p => p.TotalPass) / Math.Max(recentPatrols.Sum(p => p.TotalChecked), 1)
            : 1.0;

        // 2. 最近该工序的不合格率
        var recentFails = await _db.IpqcPatrols
            .Where(p => p.ProcessId == processId
                     && p.Status == "completed"
                     && p.Conclusion == "unqualified"
                     && p.ActualTime >= oneHourAgo)
            .CountAsync();

        var recentTotal = await _db.IpqcPatrols
            .Where(p => p.ProcessId == processId
                     && p.Status == "completed"
                     && p.ActualTime >= oneHourAgo)
            .CountAsync();

        var defectRate = recentTotal > 0 ? (double)recentFails / recentTotal : 0;

        // 3. 最近该设备首件合格情况
        var recentFirstPieces = await _db.IpqcFirstPieces
            .Where(f => f.EquipmentId == equipmentId
                     && f.Conclusion != "pending"
                     && f.CheckedAt >= oneHourAgo)
            .ToListAsync();

        var firstPieceFailRate = recentFirstPieces.Count > 0
            ? (double)recentFirstPieces.Count(f => f.Conclusion == "unqualified") / recentFirstPieces.Count
            : 0;

        // ─── 冷启动规则评分 ───
        var score = 30; // 基准分（低风险）
        var factors = new List<RiskFactorDto>();
        var recommendations = new List<string>();

        // 核心主轴温度模拟（生产环境中从设备实时数据获取）
        // 此处简化处理：基于不合格率估算
        if (defectRate > 0.1)
        {
            score += 25;
            factors.Add(new RiskFactorDto
            {
                Name = "近期不合格率偏高",
                Description = $"最近 1 小时不合格率 {defectRate:P1}，超过 10%",
                Impact = 25,
                CurrentValue = defectRate * 100,
                TargetValue = 10
            });
        }
        else if (defectRate > 0.05)
        {
            score += 10;
            factors.Add(new RiskFactorDto
            {
                Name = "不合格率略高",
                Description = $"最近 1 小时不合格率 {defectRate:P1}",
                Impact = 10,
                CurrentValue = defectRate * 100,
                TargetValue = 5
            });
        }

        if (recentPassRate < 0.90)
        {
            score += 15;
            factors.Add(new RiskFactorDto
            {
                Name = "巡检合格率偏低",
                Description = $"巡检合格率 {recentPassRate:P1}，低于 90%",
                Impact = 15,
                CurrentValue = recentPassRate * 100,
                TargetValue = 90
            });
        }

        if (firstPieceFailRate > 0.2)
        {
            score += 20;
            factors.Add(new RiskFactorDto
            {
                Name = "首件不合格率高",
                Description = $"最近首件不合格率 {firstPieceFailRate:P1}",
                Impact = 20,
                CurrentValue = firstPieceFailRate * 100,
                TargetValue = 0
            });
        }

        // 趋势判定（简化版：比较最近 30 分钟和 30-60 分钟的数据）
        var recent30min = await _db.IpqcPatrols
            .Where(p => p.EquipmentId == equipmentId
                     && p.Status == "completed"
                     && p.ActualTime >= DateTime.UtcNow.AddMinutes(-30))
            .CountAsync();

        var earlier30min = await _db.IpqcPatrols
            .Where(p => p.EquipmentId == equipmentId
                     && p.Status == "completed"
                     && p.ActualTime >= DateTime.UtcNow.AddMinutes(-60)
                     && p.ActualTime < DateTime.UtcNow.AddMinutes(-30))
            .CountAsync();

        var trend = "stable";
        if (recent30min > earlier30min * 1.5 && earlier30min > 0)
            trend = "rising";
        else if (recent30min < earlier30min * 0.5)
            trend = "falling";

        score = Math.Min(score, 100);

        // 风险等级
        var level = score switch
        {
            < 40 => "normal",
            < 70 => "warning",
            _ => "critical"
        };

        // 建议
        if (level == "critical")
            recommendations.Add("建议立即停机检查，通知质量工程师介入");
        else if (level == "warning")
            recommendations.Add("建议加强巡检频次，重点关注高风险工序");
        else
            recommendations.Add("当前状态正常，保持常规巡检");

        if (defectRate > 0.1)
            recommendations.Add("建议检查设备参数和刀具状态");

        if (factors.Count == 0)
        {
            factors.Add(new RiskFactorDto
            {
                Name = "设备运行稳定",
                Description = "近期无风险因素",
                Impact = 0
            });
        }

        // 持久化风险评分
        var riskEntity = new IpqcAiRiskScore
        {
            EquipmentId = equipmentId,
            ProcessId = processId,
            WorkOrderId = workOrderId,
            RiskScore = score,
            RiskLevel = level,
            FactorsJson = System.Text.Json.JsonSerializer.Serialize(factors),
            TrendDirection = trend,
            CreatedAt = DateTime.UtcNow
        };
        _db.IpqcAiRiskScores.Add(riskEntity);
        await _db.SaveChangesAsync();

        return new IpqcAiRiskScoreDto
        {
            Score = score,
            Level = level,
            Trend = trend,
            Factors = factors,
            Recommendations = recommendations,
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 获取设备最新 AI 风险评分
    /// </summary>
    public async Task<IpqcAiRiskScoreDto?> GetLatestRiskScore(int equipmentId, int processId)
    {
        var latest = await _db.IpqcAiRiskScores
            .Where(r => r.EquipmentId == equipmentId && r.ProcessId == processId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        if (latest == null) return null;

        var factors = string.IsNullOrEmpty(latest.FactorsJson)
            ? new List<RiskFactorDto>()
            : System.Text.Json.JsonSerializer.Deserialize<List<RiskFactorDto>>(latest.FactorsJson) ?? new();

        return new IpqcAiRiskScoreDto
        {
            Score = latest.RiskScore,
            Level = latest.RiskLevel,
            Trend = latest.TrendDirection,
            FactorsJson = latest.FactorsJson,
            Factors = factors,
            LastUpdated = latest.CreatedAt
        };
    }

    /// <summary>
    /// 获取风险评分历史趋势数据
    /// </summary>
    public async Task<List<IpqcAiRiskScoreDto>> GetRiskScoreHistory(int equipmentId, int processId, int hours = 24)
    {
        var since = DateTime.UtcNow.AddHours(-hours);
        var scores = await _db.IpqcAiRiskScores
            .Where(r => r.EquipmentId == equipmentId && r.ProcessId == processId && r.CreatedAt >= since)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new IpqcAiRiskScoreDto
            {
                Score = r.RiskScore,
                Level = r.RiskLevel,
                Trend = r.TrendDirection,
                LastUpdated = r.CreatedAt
            })
            .ToListAsync();

        return scores;
    }

    // ═══════════════════════════════════════════════════════════════
    // 关单评估 (复用 ClosureRuleEngine)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 评估当前工单/工序是否满足关单条件
    /// </summary>
    public async Task<IpqcClosureEvaluationDto> EvaluateClosure(long workOrderId, int processId, int equipmentId)
    {
        // 获取该工序适用的关单规则
        var applicableRules = await _db.ClosureRules
            .Where(r => r.IsActive)
            .ToListAsync();

        if (applicableRules.Count == 0)
        {
            return new IpqcClosureEvaluationDto
            {
                IsSatisfied = false,
                FailedConditions = ["未配置关单规则"],
                Status = "open"
            };
        }

        // 收集实时数据构造 InspectionContext
        var context = new InspectionContext();

        // 通过 override 传递真实数据 — 使用匿名派生
        var realContext = new IpqcInspectionContext(_db, workOrderId, processId, equipmentId);

        foreach (var rule in applicableRules)
        {
            var evaluation = _ruleEngine.Evaluate(rule, realContext);
            if (evaluation.IsSatisfied)
            {
                // 任一规则满足即关单
                // 更新关单状态
                var closureStatus = await _db.IpqcClosureStatuses
                    .FirstOrDefaultAsync(c => c.WorkOrderId == workOrderId);

                if (closureStatus == null)
                {
                    closureStatus = new IpqcClosureStatus
                    {
                        WorkOrderId = workOrderId,
                        Status = "closed",
                        ClosedAt = DateTime.UtcNow,
                        RuleId = rule.Id,
                        EvaluationResult = System.Text.Json.JsonSerializer.Serialize(evaluation)
                    };
                    _db.IpqcClosureStatuses.Add(closureStatus);
                }
                else
                {
                    closureStatus.Status = "closed";
                    closureStatus.ClosedAt = DateTime.UtcNow;
                    closureStatus.RuleId = rule.Id;
                    closureStatus.EvaluationResult = System.Text.Json.JsonSerializer.Serialize(evaluation);
                    closureStatus.UpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();

                return new IpqcClosureEvaluationDto
                {
                    IsSatisfied = true,
                    Status = "closed",
                    RuleName = rule.Name
                };
            }
        }

        // 所有规则都不满足
        var lastRule = applicableRules.Last();
        var lastResult = _ruleEngine.Evaluate(lastRule, realContext);

        return new IpqcClosureEvaluationDto
        {
            IsSatisfied = false,
            FailedConditions = lastResult.FailedConditions,
            Status = "open",
            RuleName = lastRule.Name
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // 辅助方法
    // ═══════════════════════════════════════════════════════════════

    private async Task<string> GenerateFirstPieceNo()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _db.IpqcFirstPieces
            .CountAsync(f => f.FpNo.StartsWith($"FP-{date}")) + 1;
        return $"FP-{date}-{count:D4}";
    }

    private async Task<string> GeneratePatrolPlanNo()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _db.IpqcPatrolPlans
            .CountAsync(p => p.PlanNo.StartsWith($"PLAN-{date}")) + 1;
        return $"PLAN-{date}-{count:D4}";
    }

    private async Task<string> GeneratePatrolNo()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var count = await _db.IpqcPatrols
            .CountAsync(p => p.PatrolNo.StartsWith($"PTL-{date}")) + 1;
        return $"PTL-{date}-{count:D4}";
    }
}

/// <summary>
/// IPQC 专用的 InspectionContext 实现，从数据库获取实时数据
/// </summary>
public class IpqcInspectionContext : InspectionContext
{
    private readonly AppDbContext _db;
    private readonly long _workOrderId;
    private readonly int _processId;
    private readonly int _equipmentId;

    public IpqcInspectionContext(AppDbContext db, long workOrderId, int processId, int equipmentId)
    {
        _db = db;
        _workOrderId = workOrderId;
        _processId = processId;
        _equipmentId = equipmentId;
    }

    public override double GetConsecutiveOkCount(string paramCode)
    {
        // 查询该工序/设备最近连续合格件数
        var recentPatrols = _db.IpqcPatrols
            .Where(p => p.ProcessId == _processId
                     && p.EquipmentId == _equipmentId
                     && p.Status == "completed")
            .OrderByDescending(p => p.ActualTime)
            .Take(50)
            .ToList();

        int consecutive = 0;
        foreach (var patrol in recentPatrols)
        {
            if (patrol.Conclusion == "qualified")
                consecutive++;
            else if (patrol.Conclusion == "unqualified")
                break; // 遇到不合格中断计数
        }
        return consecutive;
    }

    public override double GetCpk(string paramCode)
    {
        // 简化实现：从最近的巡检数据估算 Cpk
        // 生产环境中从 SPC 模块获取精确 Cpk
        var recentItems = _db.IpqcPatrolItems
            .Where(i => i.Patrol!.ProcessId == _processId
                     && i.Patrol.EquipmentId == _equipmentId
                     && i.Result != "pending")
            .OrderByDescending(i => i.Patrol!.ActualTime)
            .Take(30)
            .ToList();

        if (recentItems.Count < 5 || recentItems.All(i => i.Usl == null || i.Lsl == null))
            return 1.33; // 默认值

        var numericValues = recentItems
            .Where(i => i.ActualValue.HasValue && i.Usl.HasValue && i.Lsl.HasValue)
            .Select(i => (double)i.ActualValue!.Value)
            .ToList();

        if (numericValues.Count < 5) return 1.33;

        var mean = numericValues.Average();
        var stdDev = Math.Sqrt(numericValues.Sum(v => Math.Pow(v - mean, 2)) / (numericValues.Count - 1));
        if (stdDev < 0.0001) return 2.0; // 无波动视为高 Cpk

        var usl = (double)recentItems.First(i => i.Usl.HasValue).Usl!.Value;
        var lsl = (double)recentItems.First(i => i.Lsl.HasValue).Lsl!.Value;

        var cpu = (usl - mean) / (3 * stdDev);
        var cpl = (mean - lsl) / (3 * stdDev);
        return Math.Min(cpu, cpl);
    }

    public override double GetSamplingPassRate(string paramCode)
    {
        var recentPatrols = _db.IpqcPatrols
            .Where(p => p.ProcessId == _processId
                     && p.Status == "completed")
            .OrderByDescending(p => p.ActualTime)
            .Take(30)
            .ToList();

        if (recentPatrols.Count == 0) return 100;

        var totalPass = recentPatrols.Sum(p => p.TotalPass);
        var totalChecked = recentPatrols.Sum(p => p.TotalChecked);
        return totalChecked > 0 ? (double)totalPass / totalChecked * 100 : 100;
    }

    public override double GetAiRiskScore()
    {
        var latest = _db.IpqcAiRiskScores
            .Where(r => r.EquipmentId == _equipmentId && r.ProcessId == _processId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault();

        return latest?.RiskScore ?? 30;
    }
}

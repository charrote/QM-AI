using Microsoft.EntityFrameworkCore;
using QM_AI.API.Data;
using QM_AI.API.DTOs.M06;

namespace QM_AI.API.Services;

/// <summary>
/// 业务数据拉取服务 —— 从IQC/IPQC/FQC统一拉取检验数据用于SPC分析
/// 这是贯通S3/S4/S5数据到S6的关键服务
/// </summary>
public class BusinessDataService
{
    private readonly AppDbContext _db;

    public BusinessDataService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 查询各业务模块的检验数据（统一返回格式）
    /// </summary>
    public async Task<List<BusinessInspectionDataDto>> QueryInspectionData(BusinessDataQueryDto query)
    {
        var results = new List<BusinessInspectionDataDto>();

        // IQC 数据
        if (string.IsNullOrEmpty(query.SourceType) || query.SourceType == "IQC")
        {
            var iqcQuery = from item in _db.IqcInspectionItems
                           join insp in _db.IqcInspections on item.InspectionId equals insp.Id
                           join receipt in _db.IqcReceipts on insp.ReceiptId equals receipt.Id
                           where (query.InspectionItemId == null || item.InspectionItemId == query.InspectionItemId)
                              && (query.ProductId == null || receipt.ProductId == query.ProductId)
                              && (query.SupplierId == null || receipt.SupplierId == query.SupplierId)
                           select new BusinessInspectionDataDto
                           {
                               Id = item.Id,
                               SourceType = "IQC",
                               SourceNo = insp.InspectionNo,
                               InspectionItemId = item.InspectionItemId,
                               InspectionItemName = item.ItemName,
                               MeasuredValue = item.MeasuredValue,
                               Result = item.Result,
                               InspectedAt = insp.InspectedAt ?? insp.CreatedAt,
                               ProductId = receipt.ProductId,
                               SupplierId = receipt.SupplierId,
                           };

            if (query.StartDate.HasValue)
                iqcQuery = iqcQuery.Where(d => d.InspectedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                iqcQuery = iqcQuery.Where(d => d.InspectedAt <= query.EndDate.Value);

            var iqcData = await iqcQuery
                .OrderByDescending(d => d.InspectedAt)
                .Take(query.Limit ?? 500)
                .ToListAsync();

            results.AddRange(iqcData);
        }

        // IPQC 巡检数据
        if (string.IsNullOrEmpty(query.SourceType) || query.SourceType == "IPQC-PATROL")
        {
            var patrolQuery = from item in _db.IpqcPatrolItems
                              join patrol in _db.IpqcPatrols on item.PatrolId equals patrol.Id
                              where (query.InspectionItemId == null || item.InspectionItemId == query.InspectionItemId)
                                 && (query.ProcessId == null || patrol.ProcessId == query.ProcessId)
                                 && (query.EquipmentId == null || patrol.EquipmentId == query.EquipmentId)
                              select new BusinessInspectionDataDto
                              {
                                  Id = item.Id,
                                  SourceType = "IPQC",
                                  SourceNo = patrol.PatrolNo,
                                  InspectionItemId = item.InspectionItemId,
                                  InspectionItemName = item.ItemName,
                                  MeasuredValue = item.ActualValue,
                                  Result = item.Result,
                                  InspectedAt = patrol.ActualTime ?? patrol.ScheduledTime,
                                  ProcessId = patrol.ProcessId,
                                  EquipmentId = patrol.EquipmentId,
                              };

            if (query.StartDate.HasValue)
                patrolQuery = patrolQuery.Where(d => d.InspectedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                patrolQuery = patrolQuery.Where(d => d.InspectedAt <= query.EndDate.Value);

            var patrolData = await patrolQuery
                .OrderByDescending(d => d.InspectedAt)
                .Take(query.Limit ?? 500)
                .ToListAsync();

            results.AddRange(patrolData);
        }

        // IPQC 首件数据
        if (string.IsNullOrEmpty(query.SourceType) || query.SourceType == "IPQC-FIRSTPIECE")
        {
            var fpQuery = from item in _db.IpqcFirstPieceItems
                          join fp in _db.IpqcFirstPieces on item.FirstPieceId equals fp.Id
                          where (query.InspectionItemId == null || item.InspectionItemId == query.InspectionItemId)
                             && (query.ProcessId == null || fp.ProcessId == query.ProcessId)
                             && (query.EquipmentId == null || fp.EquipmentId == query.EquipmentId)
                          select new BusinessInspectionDataDto
                          {
                              Id = item.Id,
                              SourceType = "IPQC",
                              SourceNo = fp.FpNo,
                              InspectionItemId = item.InspectionItemId,
                              InspectionItemName = item.ItemName,
                              MeasuredValue = item.ActualValue,
                              Result = item.Result,
                              InspectedAt = fp.CheckedAt ?? fp.CreatedAt,
                              ProcessId = fp.ProcessId,
                              EquipmentId = fp.EquipmentId,
                          };

            if (query.StartDate.HasValue)
                fpQuery = fpQuery.Where(d => d.InspectedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                fpQuery = fpQuery.Where(d => d.InspectedAt <= query.EndDate.Value);

            var fpData = await fpQuery
                .OrderByDescending(d => d.InspectedAt)
                .Take(query.Limit ?? 500)
                .ToListAsync();

            results.AddRange(fpData);
        }

        // FQC 数据
        if (string.IsNullOrEmpty(query.SourceType) || query.SourceType == "FQC")
        {
            var fqcQuery = from item in _db.FqcInspectionItems
                           join insp in _db.FqcInspections on item.InspectionId equals insp.Id
                           join batch in _db.ProductBatches on insp.BatchId equals batch.Id
                           where (query.InspectionItemId == null || item.InspectionItemId == query.InspectionItemId)
                              && (query.ProductId == null || batch.ProductId == query.ProductId)
                           select new BusinessInspectionDataDto
                           {
                               Id = item.Id,
                               SourceType = "FQC",
                               SourceNo = insp.InspectionNo,
                               InspectionItemId = item.InspectionItemId,
                               InspectionItemName = item.ItemName,
                               MeasuredValue = item.ActualValue,
                               Result = item.Result,
                               InspectedAt = insp.CreatedAt,
                               ProductId = batch.ProductId,
                           };

            if (query.StartDate.HasValue)
                fqcQuery = fqcQuery.Where(d => d.InspectedAt >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                fqcQuery = fqcQuery.Where(d => d.InspectedAt <= query.EndDate.Value);

            var fqcData = await fqcQuery
                .OrderByDescending(d => d.InspectedAt)
                .Take(query.Limit ?? 500)
                .ToListAsync();

            results.AddRange(fqcData);
        }

        return results.OrderByDescending(d => d.InspectedAt).ToList();
    }

    /// <summary>
    /// 根据SPC数据源配置，自动拉取业务数据
    /// </summary>
    public async Task<List<BusinessInspectionDataDto>> GetDataForChart(long chartId, DateTime? startDate, DateTime? endDate)
    {
        var dataSources = await _db.SpcDataSources
            .Where(s => s.ChartId == chartId)
            .ToListAsync();

        if (dataSources.Count == 0)
            return new List<BusinessInspectionDataDto>();

        var allData = new List<BusinessInspectionDataDto>();

        foreach (var ds in dataSources)
        {
            var query = new BusinessDataQueryDto
            {
                SourceType = ds.SourceType,
                InspectionItemId = ds.InspectionItemId,
                ProductId = ds.ProductId,
                ProcessId = ds.ProcessId,
                SupplierId = ds.SupplierId,
                CustomerId = ds.CustomerId,
                EquipmentId = ds.EquipmentId,
                StartDate = startDate,
                EndDate = endDate,
            };

            var data = await QueryInspectionData(query);
            allData.AddRange(data);
        }

        return allData.OrderBy(d => d.InspectedAt).ToList();
    }
}

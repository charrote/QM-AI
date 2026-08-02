using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;
using QM_AI.API.Models.M02_5;
using QM_AI.API.Models.M02_Inspection;
using QM_AI.API.Models.M03;
using QM_AI.API.Models.M04;
using QM_AI.API.Models.M05;
using QM_AI.API.Models.M06;
using QM_AI.API.Models.M07;
using QM_AI.API.Models.M09;
using QM_AI.API.Models.M11;
using QM_AI.API.Models.M12;
using QM_AI.API.Models.M13;
using System.Text.Json;

namespace QM_AI.API.Data;

// JSON 列值转换器辅助类（避免在 EF Core 表达式树中使用可选参数）
internal static class JsonColumnConverter
{
    private static readonly JsonSerializerOptions s_options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string? ToJsonString(object? value) =>
        value == null ? null : JsonSerializer.Serialize(value, s_options);

    public static string? FromJsonString(string? value) =>
        value == null ? null : JsonSerializer.Deserialize<string>(value, s_options);
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Auth
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;

    // M02 基础数据
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Bom> Boms { get; set; } = null!;
    public DbSet<Process> Processes { get; set; } = null!;
    public DbSet<Routing> Routings { get; set; } = null!;
    public DbSet<RoutingHeader> RoutingHeaders { get; set; } = null!;
    public DbSet<RoutingStep> RoutingSteps { get; set; } = null!;
    public DbSet<InspectionStandard> InspectionStandards { get; set; } = null!;
    public DbSet<DefectCode> DefectCodes { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<Tool> Tools { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

    // M02.1 检验项目主数据（贯通S3/S4/S5/S6的核心）
    public DbSet<InspectionItem> InspectionItems { get; set; } = null!;
    public DbSet<InspectionPlan> InspectionPlans { get; set; } = null!;
    public DbSet<InspectionPlanItem> InspectionPlanItems { get; set; } = null!;

    // M02.5 动态参数配置
    public DbSet<ParamGroup> ParamGroups { get; set; } = null!;
    public DbSet<DynamicParam> DynamicParams { get; set; } = null!;
    public DbSet<ClosureRule> ClosureRules { get; set; } = null!;
    public DbSet<ParamRealtimeValue> ParamRealtimeValues { get; set; } = null!;

    // M03 IQC 来料检验
    public DbSet<IqcReceipt> IqcReceipts { get; set; } = null!;
    public DbSet<IqcInspection> IqcInspections { get; set; } = null!;
    public DbSet<IqcInspectionItem> IqcInspectionItems { get; set; } = null!;
    public DbSet<IqcAnomaly> IqcAnomalies { get; set; } = null!;
    public DbSet<SupplierScore> SupplierScores { get; set; } = null!;

    // M04 IPQC 过程检验
    public DbSet<IpqcFirstPiece> IpqcFirstPieces { get; set; } = null!;
    public DbSet<IpqcFirstPieceItem> IpqcFirstPieceItems { get; set; } = null!;
    public DbSet<IpqcPatrolPlan> IpqcPatrolPlans { get; set; } = null!;
    public DbSet<IpqcPatrolPlanEquipment> IpqcPatrolPlanEquipment { get; set; } = null!;
    public DbSet<IpqcPatrol> IpqcPatrols { get; set; } = null!;
    public DbSet<IpqcPatrolItem> IpqcPatrolItems { get; set; } = null!;
    public DbSet<IpqcAiRiskScore> IpqcAiRiskScores { get; set; } = null!;
    public DbSet<IpqcClosureStatus> IpqcClosureStatuses { get; set; } = null!;

    // M05 FQC/OQC 成品检验
    public DbSet<ProductBatch> ProductBatches { get; set; } = null!;
    public DbSet<FqcInspection> FqcInspections { get; set; } = null!;
    public DbSet<FqcInspectionItem> FqcInspectionItems { get; set; } = null!;
    public DbSet<OqcRelease> OqcReleases { get; set; } = null!;
    public DbSet<PackagingConfirmation> PackagingConfirmations { get; set; } = null!;

    // M15 系统管理 - 企业层级 & 字典
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<SysDictType> SysDictTypes { get; set; } = null!;
    public DbSet<SysDictItem> SysDictItems { get; set; } = null!;

    // M06 SPC 统计分析
    public DbSet<SpcControlChart> SpcControlCharts { get; set; } = null!;
    public DbSet<SpcDataPoint> SpcDataPoints { get; set; } = null!;
    public DbSet<SpcAnalysisResult> SpcAnalysisResults { get; set; } = null!;
    public DbSet<SpcAlertRule> SpcAlertRules { get; set; } = null!;
    public DbSet<SpcAlertTrigger> SpcAlertTriggers { get; set; } = null!;
    public DbSet<SpcAnovaResult> SpcAnovaResults { get; set; } = null!;
    public DbSet<SpcDataSource> SpcDataSources { get; set; } = null!;

    // M07 不良与异常管理
    public DbSet<Defect> Defects { get; set; } = null!;
    public DbSet<Capa> Capas { get; set; } = null!;
    public DbSet<CapaTemporaryMeasure> CapaTemporaryMeasures { get; set; } = null!;
    public DbSet<CapaRootCause> CapaRootCauses { get; set; } = null!;
    public DbSet<CapaCorrectiveAction> CapaCorrectiveActions { get; set; } = null!;
    public DbSet<CapaPreventiveAction> CapaPreventiveActions { get; set; } = null!;
    public DbSet<CapaVerification> CapaVerifications { get; set; } = null!;
    public DbSet<ScrapReworkRecord> ScrapReworkRecords { get; set; } = null!;

    // M09 客诉 8D
    public DbSet<Complaint> Complaints { get; set; } = null!;
    public DbSet<ComplaintEvent> ComplaintEvents { get; set; } = null!;
    public DbSet<D8Report> D8Reports { get; set; } = null!;

    // M11 设备联动
    public DbSet<EquipmentParamMapping> EquipmentParamMappings { get; set; } = null!;
    public DbSet<EquipmentStatusHistory> EquipmentStatusHistories { get; set; } = null!;
    public DbSet<EquipmentQualityCorrelation> EquipmentQualityCorrelations { get; set; } = null!;

    // M12 文件管理
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;

    // M13 审核稽核
    public DbSet<Audit> Audits { get; set; } = null!;
    public DbSet<AuditFinding> AuditFindings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Auth ──
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
        });

        // ── M02 基础数据 ──
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
        });

        modelBuilder.Entity<Bom>(entity =>
        {
            entity.HasOne(b => b.Product)
                  .WithMany()
                  .HasForeignKey(b => b.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Process>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
        });

        modelBuilder.Entity<Routing>(entity =>
        {
            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Process)
                  .WithMany()
                  .HasForeignKey(r => r.ProcessId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoutingHeader>(entity =>
        {
            entity.HasOne(h => h.Product)
                  .WithMany()
                  .HasForeignKey(h => h.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(h => new { h.RouteCode, h.ProductId }).IsUnique();
            entity.HasIndex(h => h.RouteType);

            entity.HasMany(h => h.Steps)
                  .WithOne(s => s.RoutingHeader)
                  .HasForeignKey(s => s.RoutingHeaderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoutingStep>(entity =>
        {
            entity.HasOne(s => s.Process)
                  .WithMany()
                  .HasForeignKey(s => s.ProcessId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => new { s.RoutingHeaderId, s.StepOrder }).IsUnique();
        });

        modelBuilder.Entity<InspectionStandard>(entity =>
        {
            entity.HasOne(s => s.Product)
                  .WithMany()
                  .HasForeignKey(s => s.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(s => s.Process)
                  .WithMany()
                  .HasForeignKey(s => s.ProcessId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DefectCode>(entity =>
        {
            entity.HasIndex(d => d.Code).IsUnique();
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Tool>(entity =>
        {
            entity.HasIndex(t => t.Code).IsUnique();
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(s => s.Code).IsUnique();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.Code).IsUnique();
        });

        // ── M02.1 检验项目主数据 ──
        modelBuilder.Entity<InspectionItem>(entity =>
        {
            entity.HasIndex(e => e.ItemCode).IsUnique();
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.DataType).HasMaxLength(20);
            entity.Property(e => e.ChartType).HasMaxLength(20);
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InspectionPlan>(entity =>
        {
            entity.HasIndex(e => e.PlanCode).IsUnique();
            entity.Property(e => e.InspectionType).HasMaxLength(10);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Material)
                  .WithMany()
                  .HasForeignKey(e => e.MaterialId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Process)
                  .WithMany()
                  .HasForeignKey(e => e.ProcessId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Equipment)
                  .WithMany()
                  .HasForeignKey(e => e.EquipmentId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatedBy)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Items)
                  .WithOne(i => i.Plan)
                  .HasForeignKey(i => i.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InspectionPlanItem>(entity =>
        {
            entity.HasOne(i => i.InspectionItem)
                  .WithMany()
                  .HasForeignKey(i => i.InspectionItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── M02.5 动态参数配置 ──
        modelBuilder.Entity<ParamGroup>(entity =>
        {
            entity.HasIndex(g => g.Code).IsUnique();
            entity.HasMany(g => g.DynamicParams)
                  .WithOne(p => p.Group)
                  .HasForeignKey(p => p.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DynamicParam>(entity =>
        {
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(p => p.DataType)
                  .HasConversion<string>()
                  .HasMaxLength(20);
        });

        modelBuilder.Entity<ClosureRule>(entity =>
        {
            entity.HasIndex(r => r.Code).IsUnique();
        });

        modelBuilder.Entity<ParamRealtimeValue>(entity =>
        {
            entity.HasIndex(v => new { v.ParamCode, v.Timestamp });
            entity.HasIndex(v => new { v.EquipmentId, v.Timestamp });
            entity.Property(v => v.QualityResult)
                  .HasMaxLength(10);
        });

        // ── M03 IQC 来料检验 ──
        modelBuilder.Entity<IqcReceipt>(entity =>
        {
            entity.HasIndex(r => r.ReceiptNo).IsUnique();
            entity.Property(r => r.Status).HasMaxLength(20);
            entity.HasOne(r => r.Supplier)
                  .WithMany()
                  .HasForeignKey(r => r.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(r => r.Product)
                  .WithMany()
                  .HasForeignKey(r => r.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IqcInspection>(entity =>
        {
            entity.HasIndex(i => i.InspectionNo).IsUnique();
            entity.Property(i => i.Result).HasMaxLength(10);
            entity.Property(i => i.SamplingLevel).HasMaxLength(10);
            entity.HasOne(i => i.Receipt)
                  .WithMany(r => r.Inspections)
                  .HasForeignKey(i => i.ReceiptId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(i => i.Standard)
                  .WithMany()
                  .HasForeignKey(i => i.StandardId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IqcInspectionItem>(entity =>
        {
            entity.Property(i => i.Result).HasMaxLength(10);
            entity.HasOne(i => i.Inspection)
                  .WithMany(ins => ins.Items)
                  .HasForeignKey(i => i.InspectionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(i => i.DefectCode)
                  .WithMany()
                  .HasForeignKey(i => i.DefectCodeId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<IqcAnomaly>(entity =>
        {
            entity.HasIndex(a => a.AnomalyNo).IsUnique();
            entity.Property(a => a.AnomalyType).HasMaxLength(20);
            entity.Property(a => a.Severity).HasMaxLength(10);
            entity.Property(a => a.Status).HasMaxLength(20);
            entity.HasOne(a => a.Receipt)
                  .WithMany(r => r.Anomalies)
                  .HasForeignKey(a => a.ReceiptId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(a => a.Inspection)
                  .WithMany()
                  .HasForeignKey(a => a.InspectionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SupplierScore>(entity =>
        {
            entity.HasOne(s => s.Supplier)
                  .WithMany()
                  .HasForeignKey(s => s.SupplierId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── M04 IPQC 过程检验 ──
        modelBuilder.Entity<IpqcFirstPiece>(entity =>
        {
            entity.HasIndex(f => f.FpNo).IsUnique();
            entity.Property(f => f.Conclusion).HasMaxLength(20);
            entity.Property(f => f.Reason).HasMaxLength(20);
            entity.Property(f => f.Shift).HasMaxLength(20);
            entity.HasMany(f => f.Items)
                  .WithOne(i => i.FirstPiece)
                  .HasForeignKey(i => i.FirstPieceId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IpqcFirstPieceItem>(entity =>
        {
            entity.Property(i => i.Result).HasMaxLength(10);
            entity.Property(i => i.DataType).HasMaxLength(20);
        });

        modelBuilder.Entity<IpqcPatrolPlan>(entity =>
        {
            entity.HasIndex(p => p.PlanNo).IsUnique();
            entity.Property(p => p.Status).HasMaxLength(20);
            entity.HasMany(p => p.Patrols)
                  .WithOne(pa => pa.PatrolPlan)
                  .HasForeignKey(pa => pa.PatrolPlanId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.EquipmentLinks)
                  .WithOne(el => el.PatrolPlan)
                  .HasForeignKey(el => el.PatrolPlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IpqcPatrolPlanEquipment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PatrolPlanId, e.EquipmentId }).IsUnique();
            entity.HasOne(e => e.Equipment)
                  .WithMany()
                  .HasForeignKey(e => e.EquipmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IpqcPatrol>(entity =>
        {
            entity.HasIndex(p => p.PatrolNo).IsUnique();
            entity.Property(p => p.Conclusion).HasMaxLength(20);
            entity.Property(p => p.Status).HasMaxLength(20);
            entity.HasMany(p => p.Items)
                  .WithOne(i => i.Patrol)
                  .HasForeignKey(i => i.PatrolId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IpqcPatrolItem>(entity =>
        {
            entity.Property(i => i.Result).HasMaxLength(10);
            entity.Property(i => i.DataType).HasMaxLength(20);
        });

        modelBuilder.Entity<IpqcAiRiskScore>(entity =>
        {
            entity.HasIndex(r => new { r.EquipmentId, r.CreatedAt });
            entity.Property(r => r.RiskLevel).HasMaxLength(20);
            entity.Property(r => r.TrendDirection).HasMaxLength(10);
        });

        modelBuilder.Entity<IpqcClosureStatus>(entity =>
        {
            entity.Property(c => c.Status).HasMaxLength(10);
        });

        // ── M05 FQC/OQC 成品检验 ──
        modelBuilder.Entity<ProductBatch>(entity =>
        {
            entity.ToTable("product_batches");
            entity.HasIndex(b => b.BatchCode).IsUnique();
            entity.Property(b => b.Id).HasColumnName("id");
            entity.Property(b => b.BatchCode).HasColumnName("batch_code");
            entity.Property(b => b.Source).HasColumnName("source").HasMaxLength(20);
            entity.Property(b => b.ProductId).HasColumnName("product_id");
            entity.Property(b => b.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(b => b.Quantity).HasColumnName("quantity");
            entity.Property(b => b.Status).HasColumnName("status");
            entity.Property(b => b.CreatedAt).HasColumnName("created_at");
            entity.Property(b => b.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(b => b.Product)
                  .WithMany()
                  .HasForeignKey(b => b.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(b => b.Inspections)
                  .WithOne(i => i.Batch)
                  .HasForeignKey(i => i.BatchId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(b => b.Releases)
                  .WithOne(r => r.Batch)
                  .HasForeignKey(r => r.BatchId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(b => b.PackagingConfirmations)
                  .WithOne(p => p.Batch)
                  .HasForeignKey(p => p.BatchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FqcInspection 列名显式映射（MySQL snake_case）
        modelBuilder.Entity<FqcInspection>(entity =>
        {
            entity.ToTable("fqc_inspections");
            entity.HasIndex(i => i.InspectionNo).IsUnique();
            entity.Property(i => i.Id).HasColumnName("id");
            entity.Property(i => i.InspectionNo).HasColumnName("inspection_no");
            entity.Property(i => i.BatchId).HasColumnName("batch_id");
            entity.Property(i => i.WorkOrderId).HasColumnName("work_order_id");
            entity.Property(i => i.InspectionType).HasColumnName("inspection_type").HasMaxLength(10);
            entity.Property(i => i.AqlLevel).HasColumnName("aql_level");
            entity.Property(i => i.SampleSize).HasColumnName("sample_size");
            entity.Property(i => i.TotalChecked).HasColumnName("total_checked");
            entity.Property(i => i.TotalPass).HasColumnName("total_pass");
            entity.Property(i => i.TotalFail).HasColumnName("total_fail");
            entity.Property(i => i.Ac).HasColumnName("ac");
            entity.Property(i => i.Re).HasColumnName("re");
            entity.Property(i => i.Conclusion).HasColumnName("conclusion").HasMaxLength(20);
            entity.Property(i => i.InspectorId).HasColumnName("inspector_id");
            entity.Property(i => i.CheckedAt).HasColumnName("checked_at");
            entity.Property(i => i.CreatedAt).HasColumnName("created_at");
            entity.Property(i => i.UpdatedAt).HasColumnName("updated_at");
            entity.HasMany(i => i.Items)
                  .WithOne(it => it.Inspection)
                  .HasForeignKey(it => it.InspectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FqcInspectionItem 列名显式映射
        modelBuilder.Entity<FqcInspectionItem>(entity =>
        {
            entity.ToTable("fqc_inspection_items");
            entity.Property(i => i.Id).HasColumnName("id");
            entity.Property(i => i.InspectionId).HasColumnName("inspection_id");
            entity.Property(i => i.InspectionItemId).HasColumnName("inspection_item_id");
            entity.Property(i => i.ItemName).HasColumnName("item_name");
            entity.Property(i => i.ItemCode).HasColumnName("item_code");
            entity.Property(i => i.Usl).HasColumnName("usl");
            entity.Property(i => i.Lsl).HasColumnName("lsl");
            entity.Property(i => i.DataType).HasColumnName("data_type").HasMaxLength(20);
            entity.Property(i => i.ActualValue).HasColumnName("actual_value");
            entity.Property(i => i.Result).HasColumnName("result").HasMaxLength(10);
            entity.Property(i => i.ImageUrls).HasColumnName("image_urls");
        });

        // OqcRelease 列名显式映射
        modelBuilder.Entity<OqcRelease>(entity =>
        {
            entity.ToTable("oqc_releases");
            entity.HasIndex(r => r.ReleaseNumber).IsUnique();
            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.BatchId).HasColumnName("batch_id");
            entity.Property(r => r.CustomerId).HasColumnName("customer_id");
            entity.Property(r => r.ReleaseNumber).HasColumnName("release_number");
            entity.Property(r => r.ReleaseDate).HasColumnName("release_date");
            entity.Property(r => r.Quantity).HasColumnName("quantity");
            entity.Property(r => r.AuthorizedBy).HasColumnName("authorized_by");
            entity.Property(r => r.ESignatureUrl).HasColumnName("e_signature_url");
            entity.Property(r => r.SignatureTime).HasColumnName("signature_time");
            entity.Property(r => r.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(r => r.CreatedAt).HasColumnName("created_at");
            entity.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne(r => r.Customer)
                  .WithMany()
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // PackagingConfirmation 列名显式映射
        modelBuilder.Entity<PackagingConfirmation>(entity =>
        {
            entity.ToTable("packaging_confirmations");
            entity.Property(p => p.Id).HasColumnName("id");
            entity.Property(p => p.BatchId).HasColumnName("batch_id");
            entity.Property(p => p.PackagingMethod).HasColumnName("packaging_method");
            entity.Property(p => p.QtyPerBox).HasColumnName("qty_per_box");
            entity.Property(p => p.TotalBoxes).HasColumnName("total_boxes");
            entity.Property(p => p.LabelPrinted).HasColumnName("label_printed");
            entity.Property(p => p.ConfirmedBy).HasColumnName("confirmed_by");
            entity.Property(p => p.ConfirmedAt).HasColumnName("confirmed_at");
            entity.HasOne(p => p.Batch)
                  .WithMany(b => b.PackagingConfirmations)
                  .HasForeignKey(p => p.BatchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OqcRelease>(entity =>
        {
            entity.HasIndex(r => r.ReleaseNumber).IsUnique();
            entity.Property(r => r.Status).HasMaxLength(20);
            entity.HasOne(r => r.Customer)
                  .WithMany()
                  .HasForeignKey(r => r.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PackagingConfirmation>(entity =>
        {
            entity.HasOne(p => p.Batch)
                  .WithMany(b => b.PackagingConfirmations)
                  .HasForeignKey(p => p.BatchId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── M15 企业组织层级 ──
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasIndex(o => o.Code).IsUnique();
            entity.HasIndex(o => o.ParentId);
            entity.HasIndex(o => o.Level);
            entity.HasOne(o => o.Parent)
                  .WithMany(o => o.Children)
                  .HasForeignKey(o => o.ParentId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(o => o.Level).HasMaxLength(20);
        });

        // ── M15 系统字典 ──
        modelBuilder.Entity<SysDictType>(entity =>
        {
            entity.HasIndex(t => t.TypeCode).IsUnique();
            entity.HasMany(t => t.Items)
                  .WithOne(i => i.DictType)
                  .HasForeignKey(i => i.TypeCode)
                  .HasPrincipalKey(t => t.TypeCode)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SysDictItem>(entity =>
        {
            entity.HasIndex(i => new { i.TypeCode, i.SortOrder });
            entity.Property(i => i.TypeCode).HasMaxLength(50);
        });

        // ── M07 不良与异常管理 ──
        modelBuilder.Entity<Defect>(entity =>
        {
            entity.HasIndex(d => d.DefectCode).IsUnique();
            entity.HasIndex(d => d.SourceType);
            entity.HasIndex(d => d.Status);
            entity.Property(d => d.Severity).HasMaxLength(10);
            entity.Property(d => d.SourceType).HasMaxLength(10);
            entity.Property(d => d.Status).HasMaxLength(20);
            entity.Property(d => d.ImageUrls).HasColumnType("json");
        });

        modelBuilder.Entity<Capa>(entity =>
        {
            entity.HasIndex(c => c.CapaCode).IsUnique();
            entity.HasIndex(c => c.Status);
            entity.HasIndex(c => c.CurrentPhase);
            entity.Property(c => c.Severity).HasMaxLength(10);
            entity.Property(c => c.Status).HasMaxLength(20);
            entity.Property(c => c.Title).HasMaxLength(500);
            entity.HasOne(c => c.Defect)
                  .WithMany()
                  .HasForeignKey(c => c.DefectId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CapaTemporaryMeasure>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasOne(e => e.Capa)
                  .WithMany(c => c.TemporaryMeasures)
                  .HasForeignKey(e => e.CapaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CapaRootCause>(entity =>
        {
            entity.Property(e => e.AnalysisMethod).HasMaxLength(20);
            entity.Property(e => e.Content).HasColumnType("json");
            entity.Property(e => e.RootCauseSummary).HasMaxLength(2000);
            entity.HasOne(e => e.Capa)
                  .WithMany(c => c.RootCauses)
                  .HasForeignKey(e => e.CapaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CapaCorrectiveAction>(entity =>
        {
            entity.Property(e => e.ActionDescription).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.HasOne(e => e.Capa)
                  .WithMany(c => c.CorrectiveActions)
                  .HasForeignKey(e => e.CapaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CapaPreventiveAction>(entity =>
        {
            entity.Property(e => e.ActionDescription).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.HasOne(e => e.Capa)
                  .WithMany(c => c.PreventiveActions)
                  .HasForeignKey(e => e.CapaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CapaVerification>(entity =>
        {
            entity.Property(e => e.Conclusion).HasMaxLength(20);
            entity.Property(e => e.Evidence).HasMaxLength(2000);
            entity.Property(e => e.ImageUrls).HasColumnType("json");
            entity.Property(e => e.Remarks).HasMaxLength(1000);
            entity.HasOne(e => e.Capa)
                  .WithMany(c => c.Verifications)
                  .HasForeignKey(e => e.CapaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScrapReworkRecord>(entity =>
        {
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.Type).HasMaxLength(10);
            entity.Property(e => e.Reason).HasMaxLength(2000);
            entity.Property(e => e.ReworkSteps).HasColumnType("json");
            entity.Property(e => e.ReworkInspectionResult).HasMaxLength(10);
            entity.HasOne(e => e.Defect)
                  .WithMany()
                  .HasForeignKey(e => e.DefectId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ── M09 客诉 8D ──
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasIndex(e => e.ComplaintCode).IsUnique();
            entity.Property(e => e.Severity).HasMaxLength(10);
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasMany(e => e.Events)
                  .WithOne(ev => ev.Complaint)
                  .HasForeignKey(ev => ev.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.D8Reports)
                  .WithOne(d => d.Complaint)
                  .HasForeignKey(d => d.ComplaintId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComplaintEvent>(entity =>
        {
            entity.HasIndex(e => new { e.ComplaintId, e.CreatedAt });
            entity.Property(e => e.EventType).HasMaxLength(20);
        });

        modelBuilder.Entity<D8Report>(entity =>
        {
            entity.HasIndex(e => e.ComplaintId).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        // ── M11 设备联动 ──
        modelBuilder.Entity<EquipmentParamMapping>(entity =>
        {
            entity.Property(e => e.MqttTopic).HasMaxLength(500);
            entity.Property(e => e.SystemParamCode).HasMaxLength(50);
            entity.Property(e => e.DataType).HasMaxLength(10);
            entity.Property(e => e.Unit).HasMaxLength(20);
        });

        modelBuilder.Entity<EquipmentStatusHistory>(entity =>
        {
            entity.HasIndex(e => new { e.EquipmentId, e.RecordedAt });
            entity.Property(e => e.Signal).HasMaxLength(10);
        });

        modelBuilder.Entity<EquipmentQualityCorrelation>(entity =>
        {
            entity.HasIndex(e => new { e.EquipmentId, e.AnalysisDate });
        });

        // ── M12 文件管理 ──
        modelBuilder.Entity<Document>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.DocType).HasMaxLength(20);
            entity.Property(e => e.MinioKey).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.HasMany(e => e.Versions)
                  .WithOne(v => v.Document)
                  .HasForeignKey(v => v.DocumentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.Property(e => e.MinioKey).HasMaxLength(500);
        });

        // ── M13 审核稽核 ──
        modelBuilder.Entity<Audit>(entity =>
        {
            entity.HasIndex(e => e.AuditCode).IsUnique();
            entity.Property(e => e.AuditType).HasMaxLength(10);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(10);
            // JSON 列值转换器：MySQL JSON ↔ C# string
            entity.Property(e => e.Scope)
                .HasConversion(
                    v => v == null ? null : JsonColumnConverter.ToJsonString(v),
                    v => v == null ? null : JsonColumnConverter.FromJsonString(v));
            entity.Property(e => e.AuditorIdsJson)
                .HasConversion(
                    v => v == null ? null : JsonColumnConverter.ToJsonString(v),
                    v => v == null ? null : JsonColumnConverter.FromJsonString(v));
            entity.HasMany(e => e.Findings)
                  .WithOne(f => f.Audit)
                  .HasForeignKey(f => f.AuditId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditFinding>(entity =>
        {
            entity.HasIndex(e => e.AuditId);
            entity.Property(e => e.FindingType).HasMaxLength(20);
            entity.Property(e => e.Severity).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(10);
            entity.Property(e => e.RequirementRef).HasMaxLength(200);
        });

        // ── M06 SPC 统计分析 ──
        modelBuilder.Entity<SpcControlChart>(entity =>
        {
            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.ParameterCode);
            entity.Property(c => c.ChartType).HasMaxLength(10);
            entity.HasMany(c => c.DataPoints)
                  .WithOne(p => p.Chart)
                  .HasForeignKey(p => p.ChartId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.AnalysisResults)
                  .WithOne(r => r.Chart)
                  .HasForeignKey(r => r.ChartId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.AlertRules)
                  .WithOne(r => r.Chart)
                  .HasForeignKey(r => r.ChartId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SpcDataPoint>(entity =>
        {
            entity.HasIndex(p => new { p.ChartId, p.SubgroupIndex });
            entity.Property(p => p.IndividualValues).HasColumnType("json");
        });

        modelBuilder.Entity<SpcAnalysisResult>(entity =>
        {
            entity.Property(r => r.AnalysisType).HasMaxLength(20);
        });

        modelBuilder.Entity<SpcAlertRule>(entity =>
        {
            entity.Property(r => r.RuleName).HasMaxLength(200);
            entity.HasMany(r => r.Triggers)
                  .WithOne(t => t.Rule)
                  .HasForeignKey(t => t.RuleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SpcAlertTrigger>(entity =>
        {
            entity.HasIndex(t => new { t.ChartId, t.TriggeredAt });
            entity.Property(t => t.Detail).HasColumnType("json");
        });

        modelBuilder.Entity<SpcAnovaResult>(entity =>
        {
            entity.Property(r => r.Source).HasMaxLength(20);
        });

        modelBuilder.Entity<SpcDataSource>(entity =>
        {
            entity.Property(s => s.SourceType).HasMaxLength(10);
            entity.HasOne(s => s.Chart)
                  .WithMany(c => c.DataSources)
                  .HasForeignKey(s => s.ChartId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.InspectionItem)
                  .WithMany()
                  .HasForeignKey(s => s.InspectionItemId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

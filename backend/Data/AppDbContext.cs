using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;
using QM_AI.API.Models.M02_5;
using QM_AI.API.Models.M03;

namespace QM_AI.API.Data;

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
    public DbSet<InspectionStandard> InspectionStandards { get; set; } = null!;
    public DbSet<DefectCode> DefectCodes { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<Tool> Tools { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;

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
    }
}

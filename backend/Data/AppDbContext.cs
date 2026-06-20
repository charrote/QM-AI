using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;

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
    }
}

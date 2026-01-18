using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Run> Runs { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<Layout> Layouts { get; set; }
    public DbSet<EnvironmentConfig> EnvironmentConfigs { get; set; }
    public DbSet<SimulationConfig> SimulationConfigs { get; set; }
    public DbSet<CellsLayout> CellsLayouts { get; set; }
    public DbSet<CellDef> CellDefs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Run>().HasIndex(run => run.Status);
        modelBuilder.Entity<Run>().HasIndex(run => run.LayoutId);

        modelBuilder.Entity<Job>()
            .HasOne(job => job.Run)
            .WithMany(run => run.Jobs)
            .HasForeignKey(job => job.RunId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Job>()
            .HasIndex(job => new { job.RunId, job.CellCode });

        modelBuilder.Entity<Material>()
            .HasIndex(m => m.Name);

        modelBuilder.Entity<Material>()
            .HasIndex(m => m.LotId);

        modelBuilder.Entity<Layout>()
            .HasIndex(layout => layout.CreatedAt);

        modelBuilder.Entity<EnvironmentConfig>()
            .HasKey(e => e.ConfigKey);

        // SimulationConfig 설정
        modelBuilder.Entity<SimulationConfig>()
            .HasIndex(c => c.IsDefault)
            .HasFilter("\"IsDefault\" = true")
            .IsUnique();

        // CellsLayout 설정
        modelBuilder.Entity<CellsLayout>()
            .HasIndex(l => l.IsDefault)
            .HasFilter("\"IsDefault\" = true")
            .IsUnique();

        // CellDef 설정
        modelBuilder.Entity<CellDef>()
            .HasOne(c => c.Layout)
            .WithMany(l => l.Cells)
            .HasForeignKey(c => c.LayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // 같은 레이아웃 내 코드 중복 방지
        modelBuilder.Entity<CellDef>()
            .HasIndex(c => new { c.LayoutId, c.Code })
            .IsUnique();
    }
}
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Run> Runs { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Layout> Layouts { get; set; }

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

        modelBuilder.Entity<Book>()
            .HasIndex(book => book.Title);

        modelBuilder.Entity<Layout>()
            .HasIndex(layout => layout.CreatedAt);
    }
}
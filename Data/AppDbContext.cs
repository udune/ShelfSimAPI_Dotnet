using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Data;

// DbContext 클래스 정의
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Run> Runs { get; set; } // Run 엔티티에 대한 DbSet
    public DbSet<Job> Jobs { get; set; } // Job 엔티티에 대한 DbSet
    public DbSet<Book> Books { get; set; } // Book 엔티티에 대한 DbSet

    protected override void OnModelCreating(ModelBuilder modelBuilder) // 모델 생성 시 추가 설정
    {
        base.OnModelCreating(modelBuilder); // 기본 설정 호출
        
        modelBuilder.Entity<Run>().HasIndex(run => run.Status); // Run의 Status에 인덱스 추가
        
        modelBuilder.Entity<Job>() // Job과 Run 간의 1:N 관계 설정
            .HasOne(job => job.Run) // Job은 하나의 Run을 가짐
            .WithMany(run => run.Jobs) // Run은 여러 Job을 가짐
            .HasForeignKey(job => job.RunId) // 외래 키는 Job의 RunId
            .OnDelete(DeleteBehavior.Cascade); // Run이 삭제되면 관련된 Job도 삭제
        
        modelBuilder.Entity<Job>() // Job의 RunId와 CellCode에 복합 인덱스 추가
            .HasIndex(job => new { job.RunId, job.CellCode }); // 자주 조회되는 조합에 인덱스 추가
        
        modelBuilder.Entity<Book>() // Book의 Title에 인덱스 추가
            .HasIndex(book => book.Title); // 책 제목에 인덱스 추가
        
    }
}
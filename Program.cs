using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => // DbContext 설정
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")); // PostgreSQL 사용
});

builder.Services.AddCors(options => // CORS 정책 설정
{
    options.AddPolicy("AllowUnity", policy => // "AllowUnity" 정책 정의
    {
        policy.WithOrigins(
                "https://zingy-cascaron-9d9795.netlify.app",
                "http://localhost:5000",
                "https://localhost:5001",
                "http://localhost"
            ) // 특정 출처 허용
            .AllowAnyMethod() // 모든 HTTP 메서드 허용
            .AllowAnyHeader(); // 모든 헤더 허용
    });
});

// Add services to the container.

builder.Services.AddControllers(); // 컨트롤러 서비스 추가
builder.Services.AddEndpointsApiExplorer(); // 엔드포인트 API 탐색기 추가
builder.Services.AddSwaggerGen(); // Swagger 생성기 추가

var app = builder.Build(); // 애플리케이션 빌드

using (var scope = app.Services.CreateScope()) // 서비스 범위 생성
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // DbContext 인스턴스 가져오기

    try
    {
        db.Database.Migrate(); // 데이터베이스 마이그레이션 적용
        Console.WriteLine("Database migrated successfully.");
    }
    catch (Exception e)
    {
        Console.WriteLine("Database migration failed: " + e.Message);
    }
}

// Swagger UI를 항상 활성화
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); // HTTPS 리디렉션 사용
app.UseCors("AllowUnity"); // "AllowUnity" CORS 정책 사용
app.UseAuthorization(); // 권한 부여 미들웨어 사용
app.MapControllers(); // 컨트롤러 매핑

Console.WriteLine("Starting the application...");

app.Run(); // 애플리케이션 실행
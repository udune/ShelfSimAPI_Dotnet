using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => // DbContext 설정
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); // SQL Server 사용
});

builder.Services.AddCors(options => // CORS 정책 설정
{
    options.AddPolicy("AllowUnity", policy => // "AllowUnity" 정책 정의
    {
        policy.AllowAnyOrigin() // 모든 출처 허용
            .AllowAnyMethod() // 모든 HTTP 메서드 허용
            .AllowAnyHeader(); // 모든 헤더 허용
    });
});

// Add services to the container.

builder.Services.AddControllers(); // 컨트롤러 서비스 추가
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(); // OpenAPI 서비스 추가
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) // 개발 환경인지 확인
{
    app.MapOpenApi(); // 개발 환경에서 OpenAPI 매핑
    app.UseSwagger(); // 개발 환경에서 Swagger 사용
    app.UseSwaggerUI(); // 개발 환경에서 Swagger UI 사용
}

app.UseHttpsRedirection(); // HTTPS 리디렉션 사용
app.UseCors("AllowUnity"); // "AllowUnity" CORS 정책 사용
app.UseAuthorization(); // 권한 부여 미들웨어 사용
app.MapControllers(); // 컨트롤러 매핑

Console.WriteLine("Starting the application...");

app.Run(); // 애플리케이션 실행
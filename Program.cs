using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients", policy =>
    {
        policy.WithOrigins(
                "https://zingy-cascaron-9d9795.netlify.app",
                "http://localhost:5000",
                "https://localhost:5001",
                "http://localhost",
                "http://localhost:5173"  // WPF 개발용 추가
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        db.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");

        // Seed Data: 기본 SimulationConfig
        if (!db.SimulationConfigs.Any())
        {
            var defaultConfig = new SimulationConfig
            {
                Id = "default-config",
                Name = "기본 설정",
                HandleTime = 2.0f,
                RobotSpeed = 3.0f,
                MoveTimeoutSec = 30.0f,
                TopN = 3,
                RandomSeed = 42,
                WarehousePosX = 0,
                WarehousePosY = 0,
                IsDefault = true
            };
            db.SimulationConfigs.Add(defaultConfig);
            Console.WriteLine("Default SimulationConfig created.");
        }

        // Seed Data: 기본 CellsLayout
        if (!db.CellsLayouts.Any())
        {
            var defaultLayout = new CellsLayout
            {
                Id = "default-layout",
                Name = "기본 레이아웃",
                WarehouseX = 0,
                WarehouseY = 0,
                IsDefault = true
            };
            defaultLayout.CalculateLayoutHash();
            db.CellsLayouts.Add(defaultLayout);
            Console.WriteLine("Default CellsLayout created.");
        }

        await db.SaveChangesAsync();
    }
    catch (Exception e)
    {
        Console.WriteLine("Database migration failed: " + e.Message);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowClients");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Starting the application...");

app.Run();
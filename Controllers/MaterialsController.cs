using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController(AppDbContext context, ILogger<MaterialsController> logger) : ControllerBase
{
    private const float TempTarget = 23.0f;
    private const float TempTolerance = 2.0f;
    private const float HumidTarget = 45.0f;
    private const float HumidTolerance = 5.0f;

    [HttpGet]
    public async Task<ActionResult<List<MaterialResponseDto>>> GetMaterials(
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = context.Materials.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Name.Contains(search) ||
                                     (m.Vendor != null && m.Vendor.Contains(search)) ||
                                     (m.LotId != null && m.LotId.Contains(search)));
        }

        if (!string.IsNullOrEmpty(type))
        {
            query = query.Where(m => m.Type == type);
        }

        var materials = await query
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => ToDto(m))
            .ToListAsync();

        return Ok(materials);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MaterialResponseDto>> GetMaterial(int id)
    {
        var material = await context.Materials.FindAsync(id);
        if (material == null)
            return NotFound(new { error = "MATERIAL_NOT_FOUND" });

        return Ok(ToDto(material));
    }

    [HttpPost]
    public async Task<ActionResult<MaterialResponseDto>> CreateMaterial([FromBody] CreateMaterialDto dto)
    {
        var material = new Material
        {
            Id = dto.Id,
            Name = dto.Name,
            Vendor = dto.Vendor,
            LotId = dto.LotId,
            Type = dto.Type,
            StockQty = dto.StockQty,
            ExpiryDate = dto.ExpiryDate
        };

        context.Materials.Add(material);
        await context.SaveChangesAsync();

        logger.LogInformation("Material created: {Id} - {Name}", material.Id, material.Name);
        return CreatedAtAction(nameof(GetMaterial), new { id = material.Id }, ToDto(material));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(int id, [FromBody] CreateMaterialDto dto)
    {
        var material = await context.Materials.FindAsync(id);
        if (material == null)
            return NotFound(new { error = "MATERIAL_NOT_FOUND" });

        material.Name = dto.Name;
        material.Vendor = dto.Vendor;
        material.LotId = dto.LotId;
        material.Type = dto.Type;
        material.StockQty = dto.StockQty;
        material.ExpiryDate = dto.ExpiryDate;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMaterial(int id)
    {
        var material = await context.Materials.FindAsync(id);
        if (material == null)
            return NotFound(new { error = "MATERIAL_NOT_FOUND" });

        context.Materials.Remove(material);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("inbound")]
    public async Task<ActionResult> Inbound([FromBody] InboundRequestDto dto)
    {
        var material = await context.Materials.FindAsync(dto.MaterialId);
        if (material == null)
            return NotFound(new { error = "MATERIAL_NOT_FOUND" });

        var run = await GetOrCreateActiveRun();

        var job = new Job
        {
            RunId = run.Id,
            Action = "IN",
            CellCode = dto.CellCode,
            MaterialName = material.Name,
            Quantity = dto.Qty,
            WorkerId = dto.WorkerId,
            StartTs = DateTime.UtcNow
        };

        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return Ok(new { jobId = job.Id, message = "입고 작업이 등록되었습니다." });
    }

    [HttpPost("outbound")]
    public async Task<ActionResult> Outbound([FromBody] OutboundRequestDto dto)
    {
        var envCheck = ValidateEnvironment(dto.EnvData);
        if (!envCheck.IsValid)
        {
            var run = await GetOrCreateActiveRun();
            var rejectJob = new Job
            {
                RunId = run.Id,
                Action = "REJECT",
                CellCode = dto.CellCode,
                Quantity = dto.Qty,
                WorkerId = dto.WorkerId,
                Result = "Failed",
                FailReason = envCheck.Reason,
                ErrorCode = "ENV_INTERLOCK",
                SnapshotTemp = dto.EnvData.CurrentTemp,
                SnapshotHumid = dto.EnvData.CurrentHumid,
                SnapshotLightLeak = dto.EnvData.IsLightLeak,
                StartTs = DateTime.UtcNow,
                EndTs = DateTime.UtcNow
            };
            context.Jobs.Add(rejectJob);
            await context.SaveChangesAsync();

            return StatusCode(406, new
            {
                error = "ENV_INTERLOCK",
                message = envCheck.Reason,
                jobId = rejectJob.Id
            });
        }

        var material = await context.Materials.FindAsync(dto.MaterialId);
        if (material == null)
            return NotFound(new { error = "MATERIAL_NOT_FOUND" });

        if (material.ExpiryDate.HasValue && material.ExpiryDate.Value < DateTime.UtcNow)
        {
            return StatusCode(406, new
            {
                error = "MATERIAL_EXPIRED",
                message = $"자재 유효기간 만료 (만료일: {material.ExpiryDate:yyyy-MM-dd})"
            });
        }

        var activeRun = await GetOrCreateActiveRun();
        var job = new Job
        {
            RunId = activeRun.Id,
            Action = "OUT",
            CellCode = dto.CellCode,
            MaterialName = material.Name,
            Quantity = dto.Qty,
            WorkerId = dto.WorkerId,
            SnapshotTemp = dto.EnvData.CurrentTemp,
            SnapshotHumid = dto.EnvData.CurrentHumid,
            SnapshotLightLeak = dto.EnvData.IsLightLeak,
            StartTs = DateTime.UtcNow
        };

        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        return Ok(new { jobId = job.Id, message = "출고 작업이 등록되었습니다." });
    }

    private (bool IsValid, string? Reason) ValidateEnvironment(EnvironmentDataDto env)
    {
        float tempMin = TempTarget - TempTolerance;
        float tempMax = TempTarget + TempTolerance;
        if (env.CurrentTemp < tempMin || env.CurrentTemp > tempMax)
            return (false, $"온도 조건 불충족: {env.CurrentTemp}℃ (허용: {tempMin}~{tempMax}℃)");

        float humidMin = HumidTarget - HumidTolerance;
        float humidMax = HumidTarget + HumidTolerance;
        if (env.CurrentHumid < humidMin || env.CurrentHumid > humidMax)
            return (false, $"습도 조건 불충족: {env.CurrentHumid}% (허용: {humidMin}~{humidMax}%)");

        if (env.IsLightLeak)
            return (false, "빛 누출 감지: 출고 불가");

        return (true, null);
    }

    private async Task<Run> GetOrCreateActiveRun()
    {
        var today = DateTime.UtcNow.Date;
        var run = await context.Runs
            .Where(r => r.Status == "RUNNING" && r.CreatedAt.Date == today)
            .FirstOrDefaultAsync();

        if (run == null)
        {
            run = new Run
            {
                RandomSeed = (int)DateTime.UtcNow.Ticks,
                Status = "RUNNING",
                HandleTimeSec = 2.0f,
                RobotSpeedCellsPerSec = 3.0f,
                TopN = 3
            };
            context.Runs.Add(run);
            await context.SaveChangesAsync();
        }
        return run;
    }

    private static MaterialResponseDto ToDto(Material m) => new()
    {
        Id = m.Id.ToString(),
        Name = m.Name,
        Vendor = m.Vendor,
        LotId = m.LotId,
        Type = m.Type,
        StockQty = m.StockQty,
        ExpiryDate = m.ExpiryDate,
        CreatedAt = m.CreatedAt
    };
}

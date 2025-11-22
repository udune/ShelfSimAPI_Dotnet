using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RunsController(AppDbContext context, ILogger<RunsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Run>> CreateRun([FromBody] CreateRunDto dto)
    {
        logger.LogInformation("Creating Run");

        var run = new Run
        {
            RandomSeed = dto.RandomSeed,
            HandleTimeSec = dto.HandleTimeSec,
            RobotSpeedCellsPerSec = dto.RobotSpeedCellsPerSec,
            TopN = dto.TopN,
            Status = "PENDING"
        };

        context.Runs.Add(run);
        await context.SaveChangesAsync();

        logger.LogInformation("Run created: {RunId}", run.Id);

        return CreatedAtAction(nameof(GetRun), new { id = run.Id }, run);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Run>> GetRun(int id)
    {
        var run = await context.Runs
            .Include(r => r.Jobs)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
        {
            logger.LogWarning("Run not found: {RunId}", id);
            return NotFound(new {error = "Run not found"});
        }

        return run;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetRuns(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var totalCount = await context.Runs.CountAsync();
        var runs = await context.Runs
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data = runs,
            meta = new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var run = await context.Runs.FindAsync(id);
        if (run == null)
        {
            logger.LogWarning("Run not found: {RunId}", id);
            return NotFound(new {error = "Run not found"});
        }

        run.Status = dto.Status;
        await context.SaveChangesAsync();

        logger.LogInformation("Run status updated: {RunId} to {Status}", id, dto.Status);

        return Ok(run);
    }

    [HttpGet("{id}/results.csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCsv(int id)
    {
        var run = await context.Runs.FindAsync(id);
        if (run == null)
        {
            return NotFound(new {error = "Run not found"});
        }

        var jobs = await context.Jobs
            .Where(job => job.RunId == id)
            .OrderBy(job => job.StartTs)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.Append("\uFEFF");
        csv.AppendLine("JobId,Action,CellCode,BookTitle,Quantity,StartTs,EndTs,TravelTimeSec,HandleTimeSec,TotalTimeSec,PathLengthCells,Result,FailReason,RobotName");
        foreach (var job in jobs)
        {
            csv.AppendLine(
                $"{job.Id}," +
                $"{job.Action}," +
                $"{job.CellCode}," +
                $"{EscapeCsv(job.BookTitle ?? "")}," +
                $"{job.Quantity}," +
                $"{FormatTimestamp(job.StartTs)}," +
                $"{FormatTimestamp(job.EndTs)}," +
                $"{FormatFloat(job.TravelTimeSec)}," +
                $"{FormatFloat(job.HandleTimeSec)}," +
                $"{FormatFloat(job.TotalTimeSec)}," +
                $"{job.PathLengthCells ?? 0}," +
                $"{job.Result ?? ""}," +
                $"{EscapeCsv(job.FailReason ?? "")}," +
                $"{EscapeCsv(job.RobotName ?? "")}"
                );
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        var timeStamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        return File(bytes, "text/csv", $"results_{timeStamp}.csv");
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private string FormatTimestamp(DateTime? dateTime)
    {
        if (!dateTime.HasValue)
        {
            return "";
        }

        try
        {
            var timeZoneId = "Asia/Seoul";
            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                timeZoneId = "Korea Standard Time";
            }

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var convertedTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.Value, timeZone);
            return convertedTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch (Exception)
        {
            return dateTime.Value.ToString("yyyy-MM-dd HH:mm:ss (UTC)");
        }
    }

    private string FormatFloat(float? value)
    {
        if (!value.HasValue)
        {
            return "";
        }

        return value.Value.ToString("F2");
    }
}
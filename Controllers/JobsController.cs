using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController(AppDbContext context, ILogger<JobsController> logger): ControllerBase
{
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> CreateBatch([FromBody] CreateJobsBatchDto dto)
    {
        logger.LogInformation("Creating batch of Jobs for RunId: {RunId}", dto.RunId);

        var runExist = await context.Runs.AnyAsync(run => run.Id == dto.RunId);
        if (!runExist)
        {
            logger.LogWarning("Run not found: {RunId}", dto.RunId);
            return NotFound(new {error = "Run not found"});
        }

        var jobs = dto.Jobs.Select(job => new Job
        {
            RunId = dto.RunId,
            Action = job.Action.ToUpper(),
            CellCode = job.CellCode,
            BookTitle = job.BookTitle,
            Quantity = job.Quantity
        }).ToList();

        context.Jobs.AddRange(jobs);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {JobCount} jobs for RunId: {RunId}", jobs.Count, dto.RunId);

        return Ok(new
        {
            accepted = jobs.Count,
            runId = dto.RunId,
            jobIds = jobs.Select(job => job.RunId)
        });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Job>>> GetJobsByRun([FromQuery] int runId)
    {
        var jobs = await context.Jobs
            .Where(job => job.RunId == runId)
            .OrderBy(job => job.StartTs ?? DateTime.MaxValue)
            .ToListAsync();

        return Ok(jobs);
    }

    [HttpPatch("{id}/result")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateResult(int id, [FromBody] UpdateJobStatusDto dto)
    {
        var job = await context.Jobs.FindAsync(id);
        if (job == null)
        {
            logger.LogWarning("Job not found: {JobId}", id);
            return NotFound(new {error = "Job not found"});
        }

        if (dto.StartTs.HasValue) job.StartTs = dto.StartTs;
        if (dto.EndTs.HasValue) job.EndTs = dto.EndTs;
        if (dto.TravelTimeSec.HasValue) job.TravelTimeSec = dto.TravelTimeSec;
        if (dto.HandleTimeSec.HasValue) job.HandleTimeSec = dto.HandleTimeSec;
        if (dto.TotalTimeSec.HasValue) job.TotalTimeSec = dto.TotalTimeSec;
        if (dto.PathLengthCells.HasValue) job.PathLengthCells = dto.PathLengthCells;
        if (!string.IsNullOrEmpty(dto.Result)) job.Result = dto.Result;
        if (!string.IsNullOrEmpty(dto.FailReason)) job.FailReason = dto.FailReason;
        if (!string.IsNullOrEmpty(dto.RobotName)) job.RobotName = dto.RobotName;

        await context.SaveChangesAsync();

        logger.LogInformation("Updated Job: {JobId}", id);

        return NoContent();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Job>> GetJobById(int id)
    {
        var job = await context.Jobs
            .Include(job => job.Run)
            .FirstOrDefaultAsync(job => job.Id == id);
        if (job == null)
        {
            logger.LogWarning("Job not found: {JobId}", id);
            return NotFound(new {error = "Job not found"});
        }

        return job;
    }
}
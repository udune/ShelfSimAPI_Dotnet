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

        if (!string.IsNullOrEmpty(dto.LayoutId))
        {
            var layout = await context.Layouts.FindAsync(dto.LayoutId);
            if (layout == null)
            {
                logger.LogWarning("Layout not found: {LayoutId}", dto.LayoutId);
                return BadRequest(new
                {
                    error = "LAYOUT_NOT_FOUND",
                    message = $"Layout with ID '{dto.LayoutId}' not found. Please register the layout first."
                });
            }

            var layoutCellCodes = layout.Cells.Select(c => c.Code).ToHashSet();
            var invalidCodes = dto.Jobs
                .Select(j => j.CellCode)
                .Where(code => !layoutCellCodes.Contains(code))
                .Distinct()
                .ToList();

            if (invalidCodes.Any())
            {
                logger.LogWarning("Invalid cell codes found: {InvalidCodes}", string.Join(", ", invalidCodes));
                return BadRequest(new
                {
                    error = "INVALID_CELL_CODE",
                    message = $"Cell codes do not exist in layout '{dto.LayoutId}': {string.Join(", ", invalidCodes)}",
                    invalidCodes
                });
            }
        }

        var jobs = dto.Jobs.Select(job => new Job
        {
            RunId = dto.RunId,
            Action = job.Action.ToUpper(),
            CellCode = job.CellCode,
            MaterialName = job.MaterialName,
            Quantity = job.Quantity
        }).ToList();

        context.Jobs.AddRange(jobs);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {JobCount} jobs for RunId: {RunId}", jobs.Count, dto.RunId);

        return Ok(new
        {
            accepted = jobs.Count,
            runId = dto.RunId,
            jobIds = jobs.Select(job => job.Id).ToArray()
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

        // 이전 Result 상태 저장
        var previousResult = job.Result;

        if (dto.StartTs.HasValue) job.StartTs = dto.StartTs;
        if (dto.EndTs.HasValue) job.EndTs = dto.EndTs;
        if (dto.TravelTimeSec.HasValue) job.TravelTimeSec = dto.TravelTimeSec;
        if (dto.HandleTimeSec.HasValue) job.HandleTimeSec = dto.HandleTimeSec;
        if (dto.TotalTimeSec.HasValue) job.TotalTimeSec = dto.TotalTimeSec;
        if (dto.PathLengthCells.HasValue) job.PathLengthCells = dto.PathLengthCells;
        if (!string.IsNullOrEmpty(dto.Result)) job.Result = dto.Result;
        if (!string.IsNullOrEmpty(dto.FailReason)) job.FailReason = dto.FailReason;
        if (!string.IsNullOrEmpty(dto.ErrorCode)) job.ErrorCode = dto.ErrorCode;
        if (!string.IsNullOrEmpty(dto.RobotName)) job.RobotName = dto.RobotName;

        // Material 재고 자동 업데이트 로직
        // Result가 Success로 변경되고, MaterialName이 있는 경우에만 재고 업데이트
        if (job.Result?.Equals("Success", StringComparison.OrdinalIgnoreCase) == true &&
            previousResult?.Equals("Success", StringComparison.OrdinalIgnoreCase) != true &&
            !string.IsNullOrEmpty(job.MaterialName))
        {
            var material = await context.Materials.FirstOrDefaultAsync(m => m.Name == job.MaterialName);
            if (material != null)
            {
                var action = job.Action.ToUpper();
                if (action == "PUT" || action == "IN")
                {
                    // PUT/IN: 창고에 넣음 → 사용 가능한 재고 감소
                    material.StockQty -= job.Quantity;
                    logger.LogInformation(
                        "Material inventory updated ({Action}): {MaterialName}, Quantity: -{Quantity}, New Stock: {Stock}",
                        action, material.Name, job.Quantity, material.StockQty);
                }
                else if (action == "PICK" || action == "OUT")
                {
                    // PICK/OUT: 창고에서 꺼냄 → 사용 가능한 재고 증가
                    material.StockQty += job.Quantity;
                    logger.LogInformation(
                        "Material inventory updated ({Action}): {MaterialName}, Quantity: +{Quantity}, New Stock: {Stock}",
                        action, material.Name, job.Quantity, material.StockQty);
                }

                // 재고가 음수가 되는 경우 경고
                if (material.StockQty < 0)
                {
                    logger.LogWarning(
                        "Material inventory is negative: {MaterialName}, Stock: {Stock}",
                        material.Name, material.StockQty);
                }
            }
            else
            {
                logger.LogWarning("Material not found for inventory update: {MaterialName}", job.MaterialName);
            }
        }

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
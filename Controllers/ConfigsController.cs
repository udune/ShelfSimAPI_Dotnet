using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigsController(AppDbContext context, ILogger<ConfigsController> logger) : ControllerBase
{
    /// <summary>
    /// 모든 설정 목록 조회
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfigListResponse>> GetConfigs()
    {
        var configs = await context.SimulationConfigs
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var response = new ConfigListResponse
        {
            Data = configs.Select(MapToDto).ToList(),
            TotalCount = configs.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// 특정 설정 조회
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigDto>> GetConfig(string id)
    {
        var config = await context.SimulationConfigs.FindAsync(id);

        if (config == null)
        {
            logger.LogWarning("Config not found: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"설정을 찾을 수 없습니다: {id}"
                }
            });
        }

        return Ok(MapToDto(config));
    }

    /// <summary>
    /// 기본 설정 조회
    /// </summary>
    [HttpGet("default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigDto>> GetDefaultConfig()
    {
        var config = await context.SimulationConfigs
            .FirstOrDefaultAsync(c => c.IsDefault);

        if (config == null)
        {
            logger.LogWarning("Default config not found");
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = "기본 설정이 없습니다."
                }
            });
        }

        return Ok(MapToDto(config));
    }

    /// <summary>
    /// 새 설정 생성
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConfigDto>> CreateConfig([FromBody] CreateConfigRequest request)
    {
        logger.LogInformation("Creating config: {Name}", request.Name);

        var config = new SimulationConfig
        {
            Name = request.Name,
            HandleTime = request.HandleTime,
            RobotSpeed = request.RobotSpeed,
            MoveTimeoutSec = request.MoveTimeoutSec,
            TopN = request.TopN,
            RandomSeed = request.RandomSeed,
            WarehousePosX = request.WarehousePosX,
            WarehousePosY = request.WarehousePosY
        };

        context.SimulationConfigs.Add(config);
        await context.SaveChangesAsync();

        logger.LogInformation("Config created: {Id}", config.Id);

        return CreatedAtAction(nameof(GetConfig), new { id = config.Id }, MapToDto(config));
    }

    /// <summary>
    /// 설정 전체 수정
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigDto>> UpdateConfig(string id, [FromBody] UpdateConfigRequest request)
    {
        var config = await context.SimulationConfigs.FindAsync(id);

        if (config == null)
        {
            logger.LogWarning("Config not found for update: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"설정을 찾을 수 없습니다: {id}"
                }
            });
        }

        config.Name = request.Name;
        config.HandleTime = request.HandleTime;
        config.RobotSpeed = request.RobotSpeed;
        config.MoveTimeoutSec = request.MoveTimeoutSec;
        config.TopN = request.TopN;
        config.RandomSeed = request.RandomSeed;
        config.WarehousePosX = request.WarehousePosX;
        config.WarehousePosY = request.WarehousePosY;
        config.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        logger.LogInformation("Config updated: {Id}", id);

        return Ok(MapToDto(config));
    }

    /// <summary>
    /// 설정 삭제
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConfig(string id)
    {
        var config = await context.SimulationConfigs.FindAsync(id);

        if (config == null)
        {
            logger.LogWarning("Config not found for delete: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"설정을 찾을 수 없습니다: {id}"
                }
            });
        }

        if (config.IsDefault)
        {
            logger.LogWarning("Cannot delete default config: {Id}", id);
            return BadRequest(new
            {
                error = new
                {
                    code = "CANNOT_DELETE_DEFAULT",
                    message = "기본 설정은 삭제할 수 없습니다."
                }
            });
        }

        context.SimulationConfigs.Remove(config);
        await context.SaveChangesAsync();

        logger.LogInformation("Config deleted: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// 기본 설정으로 지정
    /// </summary>
    [HttpPost("{id}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConfigDto>> SetDefault(string id)
    {
        var config = await context.SimulationConfigs.FindAsync(id);

        if (config == null)
        {
            logger.LogWarning("Config not found for set-default: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"설정을 찾을 수 없습니다: {id}"
                }
            });
        }

        // 트랜잭션으로 처리
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 기존 기본 설정 해제
            var currentDefault = await context.SimulationConfigs
                .FirstOrDefaultAsync(c => c.IsDefault);

            if (currentDefault != null && currentDefault.Id != id)
            {
                currentDefault.IsDefault = false;
                currentDefault.UpdatedAt = DateTime.UtcNow;
            }

            // 새 기본 설정 지정
            config.IsDefault = true;
            config.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("Config set as default: {Id}", id);

            return Ok(MapToDto(config));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to set default config: {Id}", id);
            throw;
        }
    }

    private static ConfigDto MapToDto(SimulationConfig config)
    {
        return new ConfigDto
        {
            Id = config.Id,
            Name = config.Name,
            HandleTime = config.HandleTime,
            RobotSpeed = config.RobotSpeed,
            MoveTimeoutSec = config.MoveTimeoutSec,
            TopN = config.TopN,
            RandomSeed = config.RandomSeed,
            WarehousePosX = config.WarehousePosX,
            WarehousePosY = config.WarehousePosY,
            IsDefault = config.IsDefault,
            CreatedAt = config.CreatedAt.ToString("o"),
            UpdatedAt = config.UpdatedAt.ToString("o")
        };
    }
}
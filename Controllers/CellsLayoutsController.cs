using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CellsLayoutsController(AppDbContext context, ILogger<CellsLayoutsController> logger) : ControllerBase
{
    /// <summary>
    /// 모든 레이아웃 목록 조회 (셀 제외)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CellsLayoutListResponse>> GetLayouts()
    {
        var layouts = await context.CellsLayouts
            .Include(l => l.Cells)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var response = new CellsLayoutListResponse
        {
            Data = layouts.Select(MapToSummaryDto).ToList(),
            TotalCount = layouts.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// 특정 레이아웃 조회 (셀 포함)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CellsLayoutDetailDto>> GetLayout(string id)
    {
        var layout = await context.CellsLayouts
            .Include(l => l.Cells)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (layout == null)
        {
            logger.LogWarning("CellsLayout not found: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"레이아웃을 찾을 수 없습니다: {id}"
                }
            });
        }

        return Ok(MapToDetailDto(layout));
    }

    /// <summary>
    /// 기본 레이아웃 조회 (셀 포함)
    /// </summary>
    [HttpGet("default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CellsLayoutDetailDto>> GetDefaultLayout()
    {
        var layout = await context.CellsLayouts
            .Include(l => l.Cells)
            .FirstOrDefaultAsync(l => l.IsDefault);

        if (layout == null)
        {
            logger.LogWarning("Default layout not found");
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = "기본 레이아웃이 없습니다."
                }
            });
        }

        return Ok(MapToDetailDto(layout));
    }

    /// <summary>
    /// 새 레이아웃 생성
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CellsLayoutDetailDto>> CreateLayout([FromBody] CreateCellsLayoutRequest request)
    {
        logger.LogInformation("Creating cells layout: {Name}", request.Name);

        // 중복 셀 코드 검사
        if (request.Cells != null && request.Cells.Count > 0)
        {
            var duplicateCodes = request.Cells
                .GroupBy(c => c.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateCodes.Count > 0)
            {
                return Conflict(new
                {
                    error = new
                    {
                        code = "DUPLICATE_CODE",
                        message = $"중복된 셀 코드가 있습니다: {string.Join(", ", duplicateCodes)}"
                    }
                });
            }
        }

        var layout = new CellsLayout
        {
            Name = request.Name,
            WarehouseX = request.WarehouseX,
            WarehouseY = request.WarehouseY
        };

        if (request.Cells != null)
        {
            layout.Cells = request.Cells.Select(c => new CellDef
            {
                LayoutId = layout.Id,
                Code = c.Code,
                Width = c.Width,
                Height = c.Height,
                Orientation = c.Orientation
            }).ToList();
        }

        layout.CalculateLayoutHash();

        context.CellsLayouts.Add(layout);
        await context.SaveChangesAsync();

        logger.LogInformation("CellsLayout created: {Id} with {CellCount} cells", layout.Id, layout.Cells.Count);

        return CreatedAtAction(nameof(GetLayout), new { id = layout.Id }, MapToDetailDto(layout));
    }

    /// <summary>
    /// 레이아웃 전체 수정 (셀 포함)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CellsLayoutDetailDto>> UpdateLayout(string id, [FromBody] UpdateCellsLayoutRequest request)
    {
        var layout = await context.CellsLayouts
            .Include(l => l.Cells)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (layout == null)
        {
            logger.LogWarning("CellsLayout not found for update: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"레이아웃을 찾을 수 없습니다: {id}"
                }
            });
        }

        // 중복 셀 코드 검사
        if (request.Cells != null && request.Cells.Count > 0)
        {
            var duplicateCodes = request.Cells
                .GroupBy(c => c.Code)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateCodes.Count > 0)
            {
                return Conflict(new
                {
                    error = new
                    {
                        code = "DUPLICATE_CODE",
                        message = $"중복된 셀 코드가 있습니다: {string.Join(", ", duplicateCodes)}"
                    }
                });
            }
        }

        layout.Name = request.Name;
        layout.WarehouseX = request.WarehouseX;
        layout.WarehouseY = request.WarehouseY;
        layout.UpdatedAt = DateTime.UtcNow;

        // 셀 전체 교체
        if (request.Cells != null)
        {
            // 기존 셀 삭제
            context.CellDefs.RemoveRange(layout.Cells);

            // 새 셀 추가
            layout.Cells = request.Cells.Select(c => new CellDef
            {
                Id = c.Id ?? Guid.NewGuid().ToString(),
                LayoutId = layout.Id,
                Code = c.Code,
                Width = c.Width,
                Height = c.Height,
                Orientation = c.Orientation
            }).ToList();
        }

        layout.CalculateLayoutHash();

        await context.SaveChangesAsync();

        logger.LogInformation("CellsLayout updated: {Id}", id);

        return Ok(MapToDetailDto(layout));
    }

    /// <summary>
    /// 레이아웃 삭제 (셀도 함께 삭제)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLayout(string id)
    {
        var layout = await context.CellsLayouts.FindAsync(id);

        if (layout == null)
        {
            logger.LogWarning("CellsLayout not found for delete: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"레이아웃을 찾을 수 없습니다: {id}"
                }
            });
        }

        if (layout.IsDefault)
        {
            logger.LogWarning("Cannot delete default layout: {Id}", id);
            return BadRequest(new
            {
                error = new
                {
                    code = "CANNOT_DELETE_DEFAULT",
                    message = "기본 레이아웃은 삭제할 수 없습니다."
                }
            });
        }

        context.CellsLayouts.Remove(layout);
        await context.SaveChangesAsync();

        logger.LogInformation("CellsLayout deleted: {Id}", id);

        return NoContent();
    }

    /// <summary>
    /// 기본 레이아웃으로 지정
    /// </summary>
    [HttpPost("{id}/set-default")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CellsLayoutDetailDto>> SetDefault(string id)
    {
        var layout = await context.CellsLayouts
            .Include(l => l.Cells)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (layout == null)
        {
            logger.LogWarning("CellsLayout not found for set-default: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"레이아웃을 찾을 수 없습니다: {id}"
                }
            });
        }

        // 트랜잭션으로 처리
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 기존 기본 레이아웃 해제
            var currentDefault = await context.CellsLayouts
                .FirstOrDefaultAsync(l => l.IsDefault);

            if (currentDefault != null && currentDefault.Id != id)
            {
                currentDefault.IsDefault = false;
                currentDefault.UpdatedAt = DateTime.UtcNow;
            }

            // 새 기본 레이아웃 지정
            layout.IsDefault = true;
            layout.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("CellsLayout set as default: {Id}", id);

            return Ok(MapToDetailDto(layout));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to set default layout: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// 셀 일괄 추가/수정
    /// </summary>
    [HttpPost("{id}/cells/batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CellsLayoutDetailDto>> BatchCells(string id, [FromBody] BatchCellsRequest request)
    {
        var layout = await context.CellsLayouts
            .Include(l => l.Cells)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (layout == null)
        {
            logger.LogWarning("CellsLayout not found for batch cells: {Id}", id);
            return NotFound(new
            {
                error = new
                {
                    code = "NOT_FOUND",
                    message = $"레이아웃을 찾을 수 없습니다: {id}"
                }
            });
        }

        // 새로 추가할 셀들의 중복 코드 검사
        var newCodes = request.Cells.Select(c => c.Code).ToList();
        var duplicateNewCodes = newCodes
            .GroupBy(c => c)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNewCodes.Count > 0)
        {
            return Conflict(new
            {
                error = new
                {
                    code = "DUPLICATE_CODE",
                    message = $"요청에 중복된 셀 코드가 있습니다: {string.Join(", ", duplicateNewCodes)}"
                }
            });
        }

        // 기존 셀과의 중복 검사
        var existingCodes = layout.Cells.Select(c => c.Code).ToHashSet();
        var conflictCodes = newCodes.Where(c => existingCodes.Contains(c)).ToList();

        if (conflictCodes.Count > 0)
        {
            return Conflict(new
            {
                error = new
                {
                    code = "DUPLICATE_CODE",
                    message = $"이미 존재하는 셀 코드가 있습니다: {string.Join(", ", conflictCodes)}"
                }
            });
        }

        // 새 셀 추가
        var newCells = request.Cells.Select(c => new CellDef
        {
            LayoutId = layout.Id,
            Code = c.Code,
            Width = c.Width,
            Height = c.Height,
            Orientation = c.Orientation
        }).ToList();

        layout.Cells.AddRange(newCells);
        layout.UpdatedAt = DateTime.UtcNow;
        layout.CalculateLayoutHash();

        await context.SaveChangesAsync();

        logger.LogInformation("Batch cells added to layout {Id}: {Count} cells", id, newCells.Count);

        return Ok(MapToDetailDto(layout));
    }

    private static CellsLayoutSummaryDto MapToSummaryDto(CellsLayout layout)
    {
        return new CellsLayoutSummaryDto
        {
            Id = layout.Id,
            Name = layout.Name,
            WarehouseX = layout.WarehouseX,
            WarehouseY = layout.WarehouseY,
            CellCount = layout.Cells.Count,
            IsDefault = layout.IsDefault,
            CreatedAt = layout.CreatedAt.ToString("o"),
            UpdatedAt = layout.UpdatedAt.ToString("o")
        };
    }

    private static CellsLayoutDetailDto MapToDetailDto(CellsLayout layout)
    {
        return new CellsLayoutDetailDto
        {
            Id = layout.Id,
            Name = layout.Name,
            WarehouseX = layout.WarehouseX,
            WarehouseY = layout.WarehouseY,
            LayoutHash = layout.LayoutHash,
            IsDefault = layout.IsDefault,
            CreatedAt = layout.CreatedAt.ToString("o"),
            UpdatedAt = layout.UpdatedAt.ToString("o"),
            Cells = layout.Cells.Select(c => new CellDefDto
            {
                Id = c.Id,
                Code = c.Code,
                Width = c.Width,
                Height = c.Height,
                Orientation = c.Orientation
            }).ToList()
        };
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LayoutsController(AppDbContext context, ILogger<LayoutsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LayoutResponse>> CreateLayout([FromBody] CreateLayoutDto dto)
    {
        logger.LogInformation("Creating Layout: {LayoutId}", dto.LayoutId);

        var existingLayout = await context.Layouts.FindAsync(dto.LayoutId);
        if (existingLayout != null)
        {
            logger.LogWarning("Layout already exists: {LayoutId}", dto.LayoutId);
            return Conflict(new { error = "LAYOUT_ALREADY_EXISTS", message = $"Layout with ID '{dto.LayoutId}' already exists." });
        }

        var layout = new Layout
        {
            LayoutId = dto.LayoutId,
            SchemaVersion = dto.SchemaVersion,
            Type = dto.Type,
            GridSizeX = dto.GridSize.X,
            GridSizeY = dto.GridSize.Y,
            WarehouseX = dto.Warehouse.X,
            WarehouseY = dto.Warehouse.Y,
            Cells = dto.Cells,
            CellCount = dto.Cells.Count
        };

        context.Layouts.Add(layout);
        await context.SaveChangesAsync();

        logger.LogInformation("Layout created: {LayoutId} with {CellCount} cells", layout.LayoutId, layout.CellCount);

        var response = new LayoutResponse
        {
            LayoutId = layout.LayoutId,
            CreatedAt = layout.CreatedAt,
            CellCount = layout.CellCount
        };

        return CreatedAtAction(nameof(GetLayout), new { layoutId = layout.LayoutId }, response);
    }

    [HttpGet("{layoutId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LayoutDetailResponse>> GetLayout(string layoutId)
    {
        var layout = await context.Layouts.FindAsync(layoutId);

        if (layout == null)
        {
            logger.LogWarning("Layout not found: {LayoutId}", layoutId);
            return NotFound(new { error = "LAYOUT_NOT_FOUND", message = $"Layout with ID '{layoutId}' not found." });
        }

        var response = new LayoutDetailResponse
        {
            LayoutId = layout.LayoutId,
            SchemaVersion = layout.SchemaVersion,
            Type = layout.Type,
            GridSize = new GridSize { X = layout.GridSizeX, Y = layout.GridSizeY },
            Warehouse = new WarehousePosition { X = layout.WarehouseX, Y = layout.WarehouseY },
            Cells = layout.Cells,
            CreatedAt = layout.CreatedAt,
            CellCount = layout.CellCount
        };

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetLayouts()
    {
        var layouts = await context.Layouts
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var response = new
        {
            layouts = layouts.Select(l => new LayoutResponse
            {
                LayoutId = l.LayoutId,
                CreatedAt = l.CreatedAt,
                CellCount = l.CellCount
            }).ToList()
        };

        return Ok(response);
    }

    [HttpPut("{layoutId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LayoutResponse>> UpdateLayout(string layoutId, [FromBody] CreateLayoutDto dto)
    {
        if (layoutId != dto.LayoutId)
        {
            return BadRequest(new { error = "LAYOUT_ID_MISMATCH", message = "Layout ID in URL does not match Layout ID in body." });
        }

        var layout = await context.Layouts.FindAsync(layoutId);
        if (layout == null)
        {
            logger.LogWarning("Layout not found for update: {LayoutId}", layoutId);
            return NotFound(new { error = "LAYOUT_NOT_FOUND", message = $"Layout with ID '{layoutId}' not found." });
        }

        layout.SchemaVersion = dto.SchemaVersion;
        layout.Type = dto.Type;
        layout.GridSizeX = dto.GridSize.X;
        layout.GridSizeY = dto.GridSize.Y;
        layout.WarehouseX = dto.Warehouse.X;
        layout.WarehouseY = dto.Warehouse.Y;
        layout.Cells = dto.Cells;
        layout.CellCount = dto.Cells.Count;

        await context.SaveChangesAsync();

        logger.LogInformation("Layout updated: {LayoutId}", layoutId);

        var response = new LayoutResponse
        {
            LayoutId = layout.LayoutId,
            CreatedAt = layout.CreatedAt,
            CellCount = layout.CellCount
        };

        return Ok(response);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.DTOs;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController(AppDbContext context, ILogger<InventoryController> logger) : ControllerBase
{
    /// <summary>
    /// 전체 셀의 재고 상태 조회
    /// </summary>
    /// <param name="runId">특정 Run의 결과만 조회 (선택사항)</param>
    /// <param name="onlyOccupied">true일 경우 자재가 있는 셀만 반환</param>
    /// <returns>셀별 재고 상태 목록</returns>
    [HttpGet("cells")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CellInventoryDto>>> GetAllCellInventory(
        [FromQuery] int? runId = null,
        [FromQuery] bool onlyOccupied = true)
    {
        logger.LogInformation("Getting all cell inventory. RunId: {RunId}, OnlyOccupied: {OnlyOccupied}", runId, onlyOccupied);

        // Result가 "Success"이고 EndTs가 있는 Job만 조회
        var query = context.Jobs
            .Where(j => j.Result != null && j.Result.ToLower() == "success" && j.EndTs != null);

        // 특정 Run의 결과만 조회하는 경우
        if (runId.HasValue)
        {
            query = query.Where(j => j.RunId == runId.Value);
        }

        // EndTs 기준으로 정렬하여 모든 Job 가져오기
        var jobs = await query
            .OrderBy(j => j.EndTs)
            .ToListAsync();

        // CellCode별로 그룹화하여 마지막 작업 찾기
        var cellInventory = jobs
            .GroupBy(j => j.CellCode)
            .Select(g =>
            {
                var lastJob = g.Last(); // EndTs로 정렬했으므로 마지막이 최신
                var action = lastJob.Action.ToUpper();
                var isStored = action == "PUT" || action == "IN";
                return new CellInventoryDto
                {
                    CellCode = g.Key,
                    MaterialName = isStored ? lastJob.MaterialName : null,
                    Quantity = isStored ? lastJob.Quantity : 0,
                    LastAction = lastJob.Action,
                    LastUpdatedAt = lastJob.EndTs,
                    RunId = lastJob.RunId
                };
            })
            .ToList();

        // 자재가 있는 셀만 반환하는 경우
        if (onlyOccupied)
        {
            cellInventory = cellInventory.Where(c => !string.IsNullOrEmpty(c.MaterialName)).ToList();
        }

        logger.LogInformation("Found {Count} cells", cellInventory.Count);
        return Ok(cellInventory);
    }

    /// <summary>
    /// 특정 셀의 재고 상태 조회
    /// </summary>
    /// <param name="cellCode">셀 코드 (예: A01, B12)</param>
    /// <param name="runId">특정 Run의 결과만 조회 (선택사항)</param>
    /// <returns>해당 셀의 재고 상태</returns>
    [HttpGet("cells/{cellCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CellInventoryDto>> GetCellInventory(
        string cellCode,
        [FromQuery] int? runId = null)
    {
        logger.LogInformation("Getting inventory for cell: {CellCode}, RunId: {RunId}", cellCode, runId);

        // Result가 "Success"이고 EndTs가 있는 해당 셀의 Job만 조회
        var query = context.Jobs
            .Where(j => j.CellCode == cellCode && j.Result != null && j.Result.ToLower() == "success" && j.EndTs != null);

        // 특정 Run의 결과만 조회하는 경우
        if (runId.HasValue)
        {
            query = query.Where(j => j.RunId == runId.Value);
        }

        // EndTs 기준으로 내림차순 정렬하여 최신 작업 가져오기
        var lastJob = await query
            .OrderByDescending(j => j.EndTs)
            .FirstOrDefaultAsync();

        if (lastJob == null)
        {
            logger.LogWarning("No job history found for cell: {CellCode}", cellCode);
            return NotFound(new { error = "해당 셀의 작업 이력을 찾을 수 없습니다." });
        }

        var action = lastJob.Action.ToUpper();
        var isStored = action == "PUT" || action == "IN";
        var inventory = new CellInventoryDto
        {
            CellCode = cellCode,
            MaterialName = isStored ? lastJob.MaterialName : null,
            Quantity = isStored ? lastJob.Quantity : 0,
            LastAction = lastJob.Action,
            LastUpdatedAt = lastJob.EndTs,
            RunId = lastJob.RunId
        };

        return Ok(inventory);
    }

    /// <summary>
    /// 자재별 총 재고 현황 조회
    /// </summary>
    /// <param name="runId">특정 Run의 결과만 조회 (선택사항)</param>
    /// <returns>자재별 재고 현황 목록</returns>
    [HttpGet("materials")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaterialInventoryDto>>> GetMaterialInventory([FromQuery] int? runId = null)
    {
        logger.LogInformation("Getting material inventory. RunId: {RunId}", runId);

        // 전체 셀 재고 조회
        var cellInventoryResponse = await GetAllCellInventory(runId, onlyOccupied: true);

        if (cellInventoryResponse.Result is OkObjectResult okResult &&
            okResult.Value is List<CellInventoryDto> cellInventory)
        {
            // 자재명별로 그룹화하여 총 수량 계산
            var materialInventory = cellInventory
                .Where(c => !string.IsNullOrEmpty(c.MaterialName))
                .GroupBy(c => c.MaterialName!)
                .Select(g => new MaterialInventoryDto
                {
                    MaterialName = g.Key,
                    TotalQuantity = g.Sum(c => c.Quantity),
                    CellCodes = g.Select(c => c.CellCode).OrderBy(code => code).ToList()
                })
                .OrderBy(m => m.MaterialName)
                .ToList();

            logger.LogInformation("Found {Count} materials in inventory", materialInventory.Count);
            return Ok(materialInventory);
        }

        return Ok(new List<MaterialInventoryDto>());
    }

    /// <summary>
    /// 특정 자재의 재고 현황 조회
    /// </summary>
    /// <param name="materialName">자재명</param>
    /// <param name="runId">특정 Run의 결과만 조회 (선택사항)</param>
    /// <returns>해당 자재의 재고 현황</returns>
    [HttpGet("materials/{materialName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaterialInventoryDto>> GetMaterialInventoryByName(
        string materialName,
        [FromQuery] int? runId = null)
    {
        logger.LogInformation("Getting inventory for material: {MaterialName}, RunId: {RunId}", materialName, runId);

        // 전체 셀 재고 조회
        var cellInventoryResponse = await GetAllCellInventory(runId, onlyOccupied: true);

        if (cellInventoryResponse.Result is OkObjectResult okResult &&
            okResult.Value is List<CellInventoryDto> cellInventory)
        {
            // 해당 자재가 있는 셀들 필터링
            var materialCells = cellInventory
                .Where(c => c.MaterialName == materialName)
                .ToList();

            if (!materialCells.Any())
            {
                logger.LogWarning("Material not found in inventory: {MaterialName}", materialName);
                return NotFound(new { error = "해당 자재의 재고를 찾을 수 없습니다." });
            }

            var inventory = new MaterialInventoryDto
            {
                MaterialName = materialName,
                TotalQuantity = materialCells.Sum(c => c.Quantity),
                CellCodes = materialCells.Select(c => c.CellCode).OrderBy(code => code).ToList()
            };

            return Ok(inventory);
        }

        return NotFound(new { error = "재고 정보를 조회할 수 없습니다." });
    }
}
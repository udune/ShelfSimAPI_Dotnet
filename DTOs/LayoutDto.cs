using System.ComponentModel.DataAnnotations;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.DTOs;

public class GridSize
{
    [Required]
    [Range(1, 1000)]
    public int X { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Y { get; set; }
}

public class WarehousePosition
{
    [Required]
    [Range(0, 1000)]
    public int X { get; set; }

    [Required]
    [Range(0, 1000)]
    public int Y { get; set; }
}

public class CreateLayoutDto
{
    [Required]
    [MaxLength(100)]
    public string LayoutId { get; set; } = string.Empty;

    [MaxLength(20)]
    public string SchemaVersion { get; set; } = "1.0";

    [MaxLength(50)]
    public string Type { get; set; } = "cells_layout";

    [Required]
    public GridSize GridSize { get; set; } = new GridSize();

    [Required]
    public WarehousePosition Warehouse { get; set; } = new WarehousePosition();

    [Required]
    [MinLength(1, ErrorMessage = "Cells 배열은 최소 1개 이상의 셀을 포함해야 합니다.")]
    public List<CellData> Cells { get; set; } = new List<CellData>();
}

public class LayoutResponse
{
    public string LayoutId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CellCount { get; set; }
}

public class LayoutDetailResponse
{
    public string LayoutId { get; set; } = string.Empty;
    public string SchemaVersion { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public GridSize GridSize { get; set; } = new GridSize();
    public WarehousePosition Warehouse { get; set; } = new WarehousePosition();
    public List<CellData> Cells { get; set; } = new List<CellData>();
    public DateTime CreatedAt { get; set; }
    public int CellCount { get; set; }
}

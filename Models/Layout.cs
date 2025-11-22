using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ShelfSimAPI.Models;

public class Layout
{
    [Key]
    [MaxLength(100)]
    public string LayoutId { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string SchemaVersion { get; set; } = "1.0";

    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = "cells_layout";

    [Required]
    public int GridSizeX { get; set; }

    [Required]
    public int GridSizeY { get; set; }

    [Required]
    public int WarehouseX { get; set; }

    [Required]
    public int WarehouseY { get; set; }

    [Required]
    [Column(TypeName = "jsonb")]
    public string CellsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CellCount { get; set; } = 0;

    [NotMapped]
    public List<CellData> Cells
    {
        get => string.IsNullOrEmpty(CellsJson)
            ? new List<CellData>()
            : JsonSerializer.Deserialize<List<CellData>>(CellsJson) ?? new List<CellData>();
        set => CellsJson = JsonSerializer.Serialize(value);
    }
}

public class CellData
{
    public string Code { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int TileW { get; set; }
    public int TileH { get; set; }
    public string Orientation { get; set; } = "N";
    public List<string> ApproachPriority { get; set; } = new List<string>();
    public bool Blocked { get; set; }
}

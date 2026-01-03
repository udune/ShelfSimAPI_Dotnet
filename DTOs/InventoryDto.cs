namespace ShelfSimAPI.DTOs;

/// <summary>
/// 셀별 재고 정보 DTO
/// </summary>
public class CellInventoryDto
{
    public string CellCode { get; set; } = string.Empty;
    public string? MaterialName { get; set; }
    public int Quantity { get; set; }
    public string? LastAction { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    public int? RunId { get; set; }
}

/// <summary>
/// 자재별 총 재고 정보 DTO
/// </summary>
public class MaterialInventoryDto
{
    public string MaterialName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public List<string> CellCodes { get; set; } = new();
}
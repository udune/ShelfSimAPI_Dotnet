namespace ShelfSimAPI.DTOs;

/// <summary>
/// 셀별 재고 정보 DTO
/// </summary>
public class CellInventoryDto
{
    /// <summary>
    /// 셀 코드 (예: A01, B12)
    /// </summary>
    public string CellCode { get; set; } = string.Empty;

    /// <summary>
    /// 해당 셀에 있는 책 제목
    /// </summary>
    public string? BookTitle { get; set; }

    /// <summary>
    /// 수량
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 마지막 작업 유형 (PUT/PICK)
    /// </summary>
    public string? LastAction { get; set; }

    /// <summary>
    /// 마지막 작업 시간
    /// </summary>
    public DateTime? LastUpdatedAt { get; set; }

    /// <summary>
    /// 해당 작업이 속한 Run ID
    /// </summary>
    public int? RunId { get; set; }
}

/// <summary>
/// 책별 총 재고 정보 DTO
/// </summary>
public class BookInventoryDto
{
    /// <summary>
    /// 책 제목
    /// </summary>
    public string BookTitle { get; set; } = string.Empty;

    /// <summary>
    /// 총 재고 수량
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 해당 책이 있는 셀 목록
    /// </summary>
    public List<string> CellCodes { get; set; } = new();
}
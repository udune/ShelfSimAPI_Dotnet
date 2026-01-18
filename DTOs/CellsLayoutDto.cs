using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

// ===== Response DTOs =====

/// <summary>
/// GET /api/CellsLayouts 응답 (목록)
/// </summary>
public class CellsLayoutListResponse
{
    public List<CellsLayoutSummaryDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// 레이아웃 요약 정보 (목록용)
/// </summary>
public class CellsLayoutSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int WarehouseX { get; set; }
    public int WarehouseY { get; set; }
    public int CellCount { get; set; }
    public bool IsDefault { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
}

/// <summary>
/// GET /api/CellsLayouts/{id} 응답 (상세)
/// </summary>
public class CellsLayoutDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int WarehouseX { get; set; }
    public int WarehouseY { get; set; }
    public string? LayoutHash { get; set; }
    public bool IsDefault { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public List<CellDefDto> Cells { get; set; } = new();
}

/// <summary>
/// 셀 정보 DTO
/// </summary>
public class CellDefDto
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Orientation { get; set; } = string.Empty;
}

// ===== Request DTOs =====

/// <summary>
/// POST /api/CellsLayouts 요청
/// </summary>
public class CreateCellsLayoutRequest
{
    [Required(ErrorMessage = "Name은 필수입니다.")]
    [MaxLength(100, ErrorMessage = "Name은 최대 100자까지 가능합니다.")]
    public string Name { get; set; } = string.Empty;

    public int WarehouseX { get; set; } = 0;

    public int WarehouseY { get; set; } = 0;

    public List<CreateCellDefRequest>? Cells { get; set; }
}

/// <summary>
/// 셀 생성 요청
/// </summary>
public class CreateCellDefRequest
{
    [Required(ErrorMessage = "Code는 필수입니다.")]
    [RegularExpression(@"^[A-Z]+[0-9]+$", ErrorMessage = "Code는 'A01', 'B02', 'AA10' 형식이어야 합니다.")]
    public string Code { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Width는 0보다 커야 합니다.")]
    public int Width { get; set; } = 90;

    [Range(1, int.MaxValue, ErrorMessage = "Height는 0보다 커야 합니다.")]
    public int Height { get; set; } = 200;

    [RegularExpression(@"^[NSEW]$", ErrorMessage = "Orientation은 'N', 'S', 'E', 'W' 중 하나여야 합니다.")]
    public string Orientation { get; set; } = "N";
}

/// <summary>
/// PUT /api/CellsLayouts/{id} 요청
/// </summary>
public class UpdateCellsLayoutRequest
{
    [Required(ErrorMessage = "Name은 필수입니다.")]
    [MaxLength(100, ErrorMessage = "Name은 최대 100자까지 가능합니다.")]
    public string Name { get; set; } = string.Empty;

    public int WarehouseX { get; set; }

    public int WarehouseY { get; set; }

    public List<UpdateCellDefRequest>? Cells { get; set; }
}

/// <summary>
/// 셀 수정 요청
/// </summary>
public class UpdateCellDefRequest
{
    /// <summary>
    /// null이면 새로 생성
    /// </summary>
    public string? Id { get; set; }

    [Required(ErrorMessage = "Code는 필수입니다.")]
    [RegularExpression(@"^[A-Z]+[0-9]+$", ErrorMessage = "Code는 'A01', 'B02', 'AA10' 형식이어야 합니다.")]
    public string Code { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Width는 0보다 커야 합니다.")]
    public int Width { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Height는 0보다 커야 합니다.")]
    public int Height { get; set; }

    [RegularExpression(@"^[NSEW]$", ErrorMessage = "Orientation은 'N', 'S', 'E', 'W' 중 하나여야 합니다.")]
    public string Orientation { get; set; } = string.Empty;
}

/// <summary>
/// POST /api/CellsLayouts/{id}/cells/batch 요청
/// </summary>
public class BatchCellsRequest
{
    [Required(ErrorMessage = "Cells는 필수입니다.")]
    public List<CreateCellDefRequest> Cells { get; set; } = new();
}
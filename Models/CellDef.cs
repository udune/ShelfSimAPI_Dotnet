using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.Models;

/// <summary>
/// 개별 셀 정의
/// </summary>
public class CellDef
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// FK to CellsLayout
    /// </summary>
    [Required]
    [MaxLength(36)]
    public string LayoutId { get; set; } = string.Empty;

    /// <summary>
    /// 셀 코드 (예: "A01", "B02", "AA10")
    /// 형식: [알파벳][숫자]
    /// </summary>
    [Required]
    [MaxLength(10)]
    [RegularExpression(@"^[A-Z]+[0-9]+$", ErrorMessage = "Code는 'A01', 'B02', 'AA10' 형식이어야 합니다.")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 셀 너비 (mm), default: 90
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Width는 0보다 커야 합니다.")]
    public int Width { get; set; } = 90;

    /// <summary>
    /// 셀 높이 (mm), default: 200
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Height는 0보다 커야 합니다.")]
    public int Height { get; set; } = 200;

    /// <summary>
    /// 접근 방향 ("N", "S", "E", "W"), default: "N"
    /// </summary>
    [Required]
    [MaxLength(1)]
    [RegularExpression(@"^[NSEW]$", ErrorMessage = "Orientation은 'N', 'S', 'E', 'W' 중 하나여야 합니다.")]
    public string Orientation { get; set; } = "N";

    /// <summary>
    /// Navigation property
    /// </summary>
    public CellsLayout? Layout { get; set; }
}
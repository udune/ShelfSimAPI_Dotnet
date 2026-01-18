using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace ShelfSimAPI.Models;

/// <summary>
/// 그리드 레이아웃 정의. 셀 목록과 창고 위치를 포함
/// </summary>
public class CellsLayout
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 레이아웃 이름 (예: "50x50 기본", "테스트 레이아웃")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 창고 위치 X, default: 0
    /// </summary>
    public int WarehouseX { get; set; } = 0;

    /// <summary>
    /// 창고 위치 Y, default: 0
    /// </summary>
    public int WarehouseY { get; set; } = 0;

    /// <summary>
    /// 캐시 무효화용 해시 (자동 생성)
    /// </summary>
    [MaxLength(64)]
    public string? LayoutHash { get; set; }

    /// <summary>
    /// 기본 레이아웃 여부
    /// </summary>
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property - 셀 목록
    /// </summary>
    public List<CellDef> Cells { get; set; } = new List<CellDef>();

    /// <summary>
    /// 레이아웃 해시 계산
    /// </summary>
    public void CalculateLayoutHash()
    {
        var data = string.Join("|",
            Cells
                .OrderBy(c => c.Code)
                .Select(c => $"{c.Code}:{c.Width}:{c.Height}:{c.Orientation}")
        );
        data += $"|{WarehouseX}:{WarehouseY}";

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        LayoutHash = Convert.ToHexString(hash)[..16];
    }
}
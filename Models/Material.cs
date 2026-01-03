using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShelfSimAPI.Models;

/// <summary>
/// 반도체 PR 자재 (감광액, 현상액 등)
/// </summary>
public class Material
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    /// <summary>
    /// 자재명 (예: KrF Photoresist A-Type)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 공급업체 (예: Dongjin Semichem, Tokyo Ohka)
    /// </summary>
    [MaxLength(200)]
    public string? Vendor { get; set; }

    /// <summary>
    /// Lot 번호 (Traceability 핵심)
    /// </summary>
    [MaxLength(50)]
    public string? LotId { get; set; }

    /// <summary>
    /// 자재 유형 (PR, Thinner, Developer 등)
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }

    /// <summary>
    /// 현재 재고 수량
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    public int StockQty { get; set; } = 0;

    /// <summary>
    /// 유효기간 (PR은 기간 지나면 폐기)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
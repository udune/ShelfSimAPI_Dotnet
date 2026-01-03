using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

public class MaterialResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? LotId { get; set; }
    public string? Type { get; set; }
    public int StockQty { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMaterialDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Vendor { get; set; }

    [MaxLength(50)]
    public string? LotId { get; set; }

    [MaxLength(50)]
    public string? Type { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQty { get; set; } = 0;

    public DateTime? ExpiryDate { get; set; }
}

public class InboundRequestDto
{
    [Required]
    public int MaterialId { get; set; }

    [Required]
    [RegularExpression("^[A-Z][0-9]{2}$")]
    public string CellCode { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Qty { get; set; }

    [MaxLength(50)]
    public string? WorkerId { get; set; }
}

public class OutboundRequestDto
{
    [Required]
    public int MaterialId { get; set; }

    [Required]
    [RegularExpression("^[A-Z][0-9]{2}$")]
    public string CellCode { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int Qty { get; set; }

    [MaxLength(50)]
    public string? WorkerId { get; set; }

    [Required]
    public EnvironmentDataDto EnvData { get; set; } = new();
}

public class EnvironmentDataDto
{
    [Required]
    public float CurrentTemp { get; set; }

    [Required]
    public float CurrentHumid { get; set; }

    [Required]
    public bool IsLightLeak { get; set; }
}
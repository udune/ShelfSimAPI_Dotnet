using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

// ===== Response DTOs =====

/// <summary>
/// GET /api/Configs 응답
/// </summary>
public class ConfigListResponse
{
    public List<ConfigDto> Data { get; set; } = new();
    public int TotalCount { get; set; }
}

/// <summary>
/// GET /api/Configs/{id} 응답
/// </summary>
public class ConfigDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public float HandleTime { get; set; }
    public float RobotSpeed { get; set; }
    public float MoveTimeoutSec { get; set; }
    public int TopN { get; set; }
    public int RandomSeed { get; set; }
    public int WarehousePosX { get; set; }
    public int WarehousePosY { get; set; }
    public bool IsDefault { get; set; }
    public string CreatedAt { get; set; } = string.Empty;  // ISO 8601 형식
    public string UpdatedAt { get; set; } = string.Empty;  // ISO 8601 형식
}

// ===== Request DTOs =====

/// <summary>
/// POST /api/Configs 요청
/// </summary>
public class CreateConfigRequest
{
    [Required(ErrorMessage = "Name은 필수입니다.")]
    [MaxLength(100, ErrorMessage = "Name은 최대 100자까지 가능합니다.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.1, float.MaxValue, ErrorMessage = "HandleTime은 0.1 이상이어야 합니다.")]
    public float HandleTime { get; set; } = 2.0f;

    [Range(0.1, float.MaxValue, ErrorMessage = "RobotSpeed는 0.1 이상이어야 합니다.")]
    public float RobotSpeed { get; set; } = 3.0f;

    [Range(1.0, float.MaxValue, ErrorMessage = "MoveTimeoutSec는 1.0 이상이어야 합니다.")]
    public float MoveTimeoutSec { get; set; } = 30.0f;

    [Range(1, 10, ErrorMessage = "TopN은 1에서 10 사이여야 합니다.")]
    public int TopN { get; set; } = 3;

    public int RandomSeed { get; set; } = 42;

    public int WarehousePosX { get; set; } = 0;

    public int WarehousePosY { get; set; } = 0;
}

/// <summary>
/// PUT /api/Configs/{id} 요청
/// </summary>
public class UpdateConfigRequest
{
    [Required(ErrorMessage = "Name은 필수입니다.")]
    [MaxLength(100, ErrorMessage = "Name은 최대 100자까지 가능합니다.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.1, float.MaxValue, ErrorMessage = "HandleTime은 0.1 이상이어야 합니다.")]
    public float HandleTime { get; set; }

    [Range(0.1, float.MaxValue, ErrorMessage = "RobotSpeed는 0.1 이상이어야 합니다.")]
    public float RobotSpeed { get; set; }

    [Range(1.0, float.MaxValue, ErrorMessage = "MoveTimeoutSec는 1.0 이상이어야 합니다.")]
    public float MoveTimeoutSec { get; set; }

    [Range(1, 10, ErrorMessage = "TopN은 1에서 10 사이여야 합니다.")]
    public int TopN { get; set; }

    public int RandomSeed { get; set; }

    public int WarehousePosX { get; set; }

    public int WarehousePosY { get; set; }
}
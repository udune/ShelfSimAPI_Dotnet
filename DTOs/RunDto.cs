using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

public class CreateRunDto
{
    [Required(ErrorMessage = "RandomSeed는 필수입니다.")]
    public int RandomSeed { get; set; }

    [Range(0.1, 100, ErrorMessage = "HandleTimeSec는 0.1에서 100 사이의 값이어야 합니다.")]
    public float HandleTimeSec { get; set; } = 2.0f;

    [Range(0.1, 100, ErrorMessage = "RobotSpeedCellsPerSec는 0.1에서 100 사이의 값이어야 합니다.")]
    public float RobotSpeedCellsPerSec { get; set; } = 3.0f;

    [Range(1, 10, ErrorMessage = "TopN은 1에서 10 사이의 값이어야 합니다.")]
    public int TopN { get; set; } = 3;

    [MaxLength(100, ErrorMessage = "LayoutId는 최대 100자까지 가능합니다.")]
    public string? LayoutId { get; set; }

    [Range(1, 3600, ErrorMessage = "MoveTimeoutSec는 1에서 3600 사이의 값이어야 합니다.")]
    public float MoveTimeoutSec { get; set; } = 30.0f;
}

public class UpdateStatusDto
{
    [Required]
    [RegularExpression("^(PENDING|RUNNING|COMPLETED|FAILED)$", ErrorMessage = "Status는 'PENDING', 'RUNNING', 'COMPLETED', 'FAILED' 중 하나여야 합니다.")]
    public string Status { get; set; } = string.Empty;
    
    public string? Summary { get; set; } = null;
}
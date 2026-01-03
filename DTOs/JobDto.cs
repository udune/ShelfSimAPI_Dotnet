using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

public class JobDto
{
    [Required]
    [RegularExpression("^(IN|OUT|PUT|PICK)$")]
    public string Action { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[A-Z][0-9]{2}$")]
    public string CellCode { get; set; } = string.Empty;

    [Required]
    public string MaterialName { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CreateJobsBatchDto
{
    [Required]
    public int RunId { get; set; }

    [MaxLength(100)]
    public string? LayoutId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Jobs 리스트는 최소 1개 이상의 작업을 포함해야 합니다.")]
    public List<JobDto> Jobs { get; set; } = new List<JobDto>();
}

public class UpdateJobStatusDto
{
    public DateTime? StartTs { get; set; }
    public DateTime? EndTs { get; set; }

    [Range(0, float.MaxValue)]
    public float? TravelTimeSec { get; set; }

    [Range(0, float.MaxValue)]
    public float? HandleTimeSec { get; set; }

    [Range(0, float.MaxValue)]
    public float? TotalTimeSec { get; set; }

    [Range(0, int.MaxValue)]
    public int? PathLengthCells { get; set; }

    [RegularExpression("^(Success|Failed)$", ErrorMessage = "Result는 'Success' 또는 'Failed'이어야 합니다.")]
    public string? Result { get; set; }

    [MaxLength(500)]
    public string? FailReason { get; set; }

    [MaxLength(50)]
    public string? ErrorCode { get; set; }

    [MaxLength(50)]
    public string? RobotName { get; set; }
}
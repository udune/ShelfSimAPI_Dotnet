using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

public class JobDto
{
    [Required]
    [RegularExpression("^(PUT|PICK)$", ErrorMessage = "Action은 'PUT' 또는 'PICK'이어야 합니다.")]
    public string Action { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[A-Z][0-9]{2}$", ErrorMessage = "CellCode는 대문자 하나와 숫자 두 개로 구성되어야 합니다. 예: A01, B12")]
    public string CellCode { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "BookTitle은 최소 1자 이상이어야 합니다.")]
    public string BookTitle { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity는 1 이상의 값이어야 합니다.")]
    public int Quantity { get; set; }
}

public class CreateJobsBatchDto
{
    [Required]
    public int RunId { get; set; }

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
    public string? RobotName { get; set; }
}
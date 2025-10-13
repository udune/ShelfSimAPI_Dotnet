using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.DTOs;

// Job 생성 및 배치 작업 DTO
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

// 여러 Job을 한 번에 생성하기 위한 DTO
public class CreateJobsBatchDto
{
    [Required]
    public Guid RunId { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "Jobs 리스트는 최소 1개 이상의 작업을 포함해야 합니다.")]
    public List<JobDto> Jobs { get; set; } = new List<JobDto>();
}

// Job 상태 업데이트 DTO
public class UpdateJobStatusDto
{
    public DateTime? StartTs { get; set; } // 시작 시간
    public DateTime? EndTs { get; set; } // 종료 시간
    
    [Range(0, float.MaxValue)]
    public float? TravelTimeSec { get; set; } // 이동 시간 (초)
    
    [Range(0, float.MaxValue)]
    public float? HandleTimeSec { get; set; } // 처리 시간 (초)
    
    [Range(0, float.MaxValue)]
    public float? TotalTimeSec { get; set; } // 총 소요 시간 (초)
    
    [Range(0, int.MaxValue)]
    public int? PathLengthCells { get; set; } // 경로 길이 (셀 단위)
    
    [RegularExpression("^(Success|Failed)$", ErrorMessage = "Result는 'Success' 또는 'Failed'이어야 합니다.")]
    public string? Result { get; set; } // 작업 결과 (e.g., "Success", "Failed")
    
    [MaxLength(500)]
    public string? FailReason { get; set; } // 실패 사유
    
    [MaxLength(50)]
    public string? RobotName { get; set; } // 작업을 수행한 로봇 이름
}
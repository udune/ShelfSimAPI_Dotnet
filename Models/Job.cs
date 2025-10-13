using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShelfSimAPI.Models;

public class Job
{
    [Key] // 기본키 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } // 자동 증가 정수 ID
    
    [Required] // Not Null
    public int RunId { get; set; } // Run에 대한 외래 키
    
    [Required] // Not Null
    [MaxLength(10)]
    public string Action { get; set; } = string.Empty; // 작업 타입 (PUT, PICK)
    
    [Required] // Not Null
    [MaxLength(10)]
    public string CellCode { get; set; } = string.Empty; // 셀 코드
    
    [MaxLength(200)]
    public string? BookTitle { get; set; } // 책 제목 (선택 사항)
    
    [Required] // Not Null
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } // 수량
    
    public DateTime? StartTs { get; set; } // 시작 시간
    public DateTime? EndTs { get; set; } // 종료 시간
    
    public float? TravelTimeSec { get; set; } // 이동 시간 (초)
    public float? HandleTimeSec { get; set; } // 처리 시간 (초)
    public float? TotalTimeSec { get; set; } // 총 소요 시간 (초)
    
    public int? PathLengthCells { get; set; } // 경로 길이 (셀 단위)
    
    [MaxLength(20)]
    public string? Result { get; set; } // 작업 결과 (e.g., "Success", "Failed")
    
    [MaxLength(500)]
    public string? FailReason { get; set; } // 실패 사유
    
    [MaxLength(50)]
    public string? RobotName { get; set; } // 작업을 수행한 로봇 이름

    [ForeignKey(nameof(RunId))] // 외래 키 관계
    public Run Run { get; set; } = null!; // Run과의 탐색 속성
}
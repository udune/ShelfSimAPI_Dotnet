using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShelfSimAPI.Models;

public class Run
{
    [Key] // 기본 키
    public Guid Id { get; set; } = Guid.NewGuid(); // 새 GUID 자동 생성
    
    public Guid? LayoutId { get; set; } // Layout에 대한 외래 키
    
    [Required] // Not Null
    public int RandomSeed { get; set; } // 랜덤 시드
    
    [Required]
    public float HandleTimeSec { get; set; } = 2.0f; // 각 아이템 처리 시간
    
    [Required]
    public float RobotSpeedCellsPerSec { get; set; } = 3.0f; // 로봇 속도
    
    [Required]
    [Range(1, 10)] // 1 ~ 10 사이의 값
    public int TopN { get; set; } = 3; // 고려할 상위 N개
    
    [Required]
    [MaxLength(20)] public string Status { get; set; } = "Pending"; // 실행 상태
    
    [Column(TypeName = "nvarchar(max)")] // Json 형식
    public string? Summary { get; set; } // 실행 결과 요약
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 생성 시간

    public ICollection<Job> Jobs { get; set; } = new List<Job>(); // Job과의 탐색 속성
}
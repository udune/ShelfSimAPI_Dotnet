using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.Models;

/// <summary>
/// 시뮬레이션 실행 파라미터를 정의하는 설정
/// </summary>
public class SimulationConfig
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 설정 이름 (예: "기본 설정", "고속 모드")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 작업 처리 시간 (초), min: 0.1, default: 2.0
    /// </summary>
    [Range(0.1, float.MaxValue, ErrorMessage = "HandleTime은 0.1 이상이어야 합니다.")]
    public float HandleTime { get; set; } = 2.0f;

    /// <summary>
    /// 로봇 이동 속도 (cells/sec), min: 0.1, default: 3.0
    /// </summary>
    [Range(0.1, float.MaxValue, ErrorMessage = "RobotSpeed는 0.1 이상이어야 합니다.")]
    public float RobotSpeed { get; set; } = 3.0f;

    /// <summary>
    /// 이동 타임아웃 (초), min: 1.0, default: 30.0
    /// </summary>
    [Range(1.0, float.MaxValue, ErrorMessage = "MoveTimeoutSec는 1.0 이상이어야 합니다.")]
    public float MoveTimeoutSec { get; set; } = 30.0f;

    /// <summary>
    /// 경로 탐색 후보 수, range: 1-10, default: 3
    /// </summary>
    [Range(1, 10, ErrorMessage = "TopN은 1에서 10 사이여야 합니다.")]
    public int TopN { get; set; } = 3;

    /// <summary>
    /// 결정성 시드, default: 42
    /// </summary>
    public int RandomSeed { get; set; } = 42;

    /// <summary>
    /// 창고 위치 X, default: 0
    /// </summary>
    public int WarehousePosX { get; set; } = 0;

    /// <summary>
    /// 창고 위치 Y, default: 0
    /// </summary>
    public int WarehousePosY { get; set; } = 0;

    /// <summary>
    /// 기본 설정 여부
    /// </summary>
    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
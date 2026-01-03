using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ShelfSimAPI.Data;

namespace ShelfSimAPI.Models;

public class Job
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int RunId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string CellCode { get; set; } = string.Empty;

    /// <summary>
    /// 자재명 (기존 BookTitle에서 변경)
    /// </summary>
    [MaxLength(200)]
    public string? MaterialName { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public DateTime? StartTs { get; set; }
    public DateTime? EndTs { get; set; }

    public float? TravelTimeSec { get; set; }
    public float? HandleTimeSec { get; set; }
    public float? TotalTimeSec { get; set; }

    public int? PathLengthCells { get; set; }

    [MaxLength(20)]
    public string? Result { get; set; }

    [MaxLength(500)]
    public string? FailReason { get; set; }

    [MaxLength(50)]
    public string? ErrorCode { get; set; }

    [MaxLength(50)]
    public string? RobotName { get; set; }

    // === 환경 스냅샷 (신규) ===

    /// <summary>
    /// 작업 당시 온도 (℃)
    /// </summary>
    public float? SnapshotTemp { get; set; }

    /// <summary>
    /// 작업 당시 습도 (%)
    /// </summary>
    public float? SnapshotHumid { get; set; }

    /// <summary>
    /// 작업 당시 빛 누출 여부
    /// </summary>
    public bool? SnapshotLightLeak { get; set; }

    /// <summary>
    /// 작업자 ID (RobotName과 별도로 사람 작업자 기록)
    /// </summary>
    [MaxLength(50)]
    public string? WorkerId { get; set; }

    [ForeignKey(nameof(RunId))]
    [JsonIgnore]
    public Run Run { get; set; } = null!;
}
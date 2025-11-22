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

    [MaxLength(200)]
    public string? BookTitle { get; set; }

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

    [ForeignKey(nameof(RunId))]
    [JsonIgnore]
    public Run Run { get; set; } = null!;
}
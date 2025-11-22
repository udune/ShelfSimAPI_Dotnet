using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShelfSimAPI.Models;

public class Run
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int? LayoutId { get; set; }

    [Required]
    public int RandomSeed { get; set; }

    [Required]
    public float HandleTimeSec { get; set; } = 2.0f;

    [Required]
    public float RobotSpeedCellsPerSec { get; set; } = 3.0f;

    [Required]
    [Range(1, 10)]
    public int TopN { get; set; } = 3;

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public string? Summary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
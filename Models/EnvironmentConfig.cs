using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.Models;

public class EnvironmentConfig
{
    [Key]
    [MaxLength(50)]
    public string ConfigKey { get; set; } = string.Empty;

    [Required]
    public float TargetValue { get; set; }

    [Required]
    public float Tolerance { get; set; }

    public bool? BoolValue { get; set; }  // LightControl용

    [MaxLength(200)]
    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShelfSimAPI.Models;

public class Book
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Author { get; set; }

    [Required]
    [Range(1, 1000)]
    public int ThicknessMn { get; set; }

    [Required]
    [Range(1, 1000)]
    public int HeightMm { get; set; }

    [MaxLength(50)]
    public string? Sku { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
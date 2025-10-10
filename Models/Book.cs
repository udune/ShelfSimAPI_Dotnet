using System.ComponentModel.DataAnnotations;

namespace ShelfSimAPI.Models;

public class Book
{
    [Key] // 기본 키
    public Guid Id { get; set; } = Guid.NewGuid(); // 새 GUID 자동 생성
    
    [Required] // Not Null
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty; // 책 제목
    
    [MaxLength(200)] // 최대 길이 200
    public string? Author { get; set; } // 저자 (선택 사항)
    
    [Required] // Not Null
    [Range(1, 1000)] // 1 ~ 1000 사이의 값
    public int ThicknessMn { get; set; } // 책 두께 (밀리미터 단위)
    
    [Required] // Not Null
    [Range(1, 1000)] // 1 ~ 1000 사이의 값
    public int HeightMm { get; set; } // 책 높이 (밀리미터 단위)
    
    [MaxLength(50)] // 최대 길이 50
    public string? Sku { get; set; } // 재고 관리 코드 (선택 사항)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 생성 시간
}
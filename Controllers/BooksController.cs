using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController] // API 컨트롤러 지정
[Route("api/[controller]")] // 기본 라우트 설정
public class BooksController(AppDbContext context, ILogger<BooksController> logger) : ControllerBase // ControllerBase 상속
{
    // GetBooks: 책 목록을 조회하는 엔드포인트
    [HttpGet] // GET /api/books
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    public async Task<ActionResult<List<Book>>> GetBooks(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = context.Books.AsQueryable(); // IQueryable<Book> 생성

        if (!string.IsNullOrEmpty(search)) // 검색어가 있으면
        {
            query = query.Where(book => book.Title.Contains(search) || 
                                        (book.Author != null && book.Author.Contains(search)) || 
                                        (book.Sku != null && book.Sku.Contains(search))); // 제목, 저자, SKU로 필터링
        }

        var books = await query
            .OrderBy(book => book.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(); // 페이징 적용하여 책 목록 조회
        
        return Ok(books); // 200 응답 반환
    }
    
    [HttpGet("{id}")] // GET /api/books/{id}
    [ProducesResponseType(StatusCodes.Status200OK)] // 200 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<ActionResult<Book>> GetBook(int id) // GetBook 메서드 정의
    {
        var book = await context.Books.FindAsync(id); // ID로 책 조회
        if (book == null) // 책이 없으면
        {
            return NotFound(); // 404 응답 반환
        }

        return book; // 책 반환
    }
    
    // CreateBook: 새로운 책을 생성하는 엔드포인트
    [HttpPost] // POST /api/books
    [ProducesResponseType(StatusCodes.Status201Created)] // 201 응답 타입
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400 응답 타입
    public async Task<ActionResult<Book>> CreateBook([FromBody] Book book) // CreateBook 메서드 정의
    {
        context.Books.Add(book); // DbContext에 추가
        await context.SaveChangesAsync(); // 비동기 저장

        logger.LogInformation("Created book {book}", book);
        
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book); // 201 응답 반환
    }
    
    // UpdateBook: 특정 책의 정보를 업데이트하는 엔드포인트
    [HttpPut("{id}")] // PUT /api/books/{id}
    [ProducesResponseType(StatusCodes.Status204NoContent)] // 204 응답 타입
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // 400 응답 타입
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] Book updatedBook) // UpdateBook 메서드 정의
    {
        var book = await context.Books.FindAsync(id); // ID로 책 조회
        if (book == null) // 책이 없으면
        {
            return NotFound(); // 404 응답 반환
        }

        // 책 정보 업데이트
        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;
        book.ThicknessMn = updatedBook.ThicknessMn;
        book.HeightMm = updatedBook.HeightMm;
        book.Sku = updatedBook.Sku;

        context.Books.Update(book); // DbContext에서 업데이트
        await context.SaveChangesAsync(); // 비동기 저장

        logger.LogInformation("Updated book {book}", book);
        
        return NoContent(); // 204 응답 반환
    }
    
    // DeleteBook: 특정 책을 삭제하는 엔드포인트
    [HttpDelete("{id}")] // DELETE /api/books/{id}
    [ProducesResponseType(StatusCodes.Status204NoContent)] // 204 응답 타입
    [ProducesResponseType(StatusCodes.Status404NotFound)] // 404 응답 타입
    public async Task<IActionResult> DeleteBook(Guid id) // DeleteBook 메서드 정의
    {
        var book = await context.Books.FindAsync(id); // ID로 책 조회
        if (book == null) // 책이 없으면
        {
            return NotFound(); // 404 응답 반환
        }

        context.Books.Remove(book); // DbContext에서 제거
        await context.SaveChangesAsync(); // 비동기 저장

        logger.LogInformation("Deleted book {book}", book);
        
        return NoContent(); // 204 응답 반환
    }
}
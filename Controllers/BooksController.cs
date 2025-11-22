using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShelfSimAPI.Data;
using ShelfSimAPI.Models;

namespace ShelfSimAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController(AppDbContext context, ILogger<BooksController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Book>>> GetBooks(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = context.Books.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(book => book.Title.Contains(search) ||
                                        (book.Author != null && book.Author.Contains(search)) ||
                                        (book.Sku != null && book.Sku.Contains(search)));
        }

        var books = await query
            .OrderBy(book => book.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        return book;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
    {
        context.Books.Add(book);
        await context.SaveChangesAsync();

        logger.LogInformation("Created book {book}", book);

        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] Book updatedBook)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;
        book.ThicknessMn = updatedBook.ThicknessMn;
        book.HeightMm = updatedBook.HeightMm;
        book.Sku = updatedBook.Sku;

        context.Books.Update(book);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated book {book}", book);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook(Guid id)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted book {book}", book);

        return NoContent();
    }
}
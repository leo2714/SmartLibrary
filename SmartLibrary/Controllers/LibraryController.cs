using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.DTOs;
using SmartLibrary.Models;
using SmartLibrary.Services;

namespace SmartLibrary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BookDto>>>> GetAllBooks([FromQuery] string? category, [FromQuery] string? search)
    {
        var books = await _bookService.GetAllBooksAsync(category, search);
        return Ok(ApiResponse<List<BookDto>>.Success(books));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BookDto>>> GetBookById(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound(ApiResponse<BookDto>.Error(404, "图书不存在"));
        }
        var result = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Category = book.Category,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            Location = book.Location,
            PublishedDate = book.PublishedDate,
            Price = book.Price
        };
        return Ok(ApiResponse<BookDto>.Success(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookDto>>> CreateBook([FromBody] BookDto dto)
    {
        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Category = dto.Category,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.AvailableCopies,
            Location = dto.Location,
            PublishedDate = dto.PublishedDate,
            Price = dto.Price
        };

        var created = await _bookService.CreateBookAsync(book);
        var result = new BookDto
        {
            Id = created.Id,
            Title = created.Title,
            Author = created.Author,
            ISBN = created.ISBN,
            Category = created.Category,
            TotalCopies = created.TotalCopies,
            AvailableCopies = created.AvailableCopies,
            Location = created.Location,
            PublishedDate = created.PublishedDate,
            Price = created.Price
        };
        return CreatedAtAction(nameof(GetBookById), new { id = created.Id }, ApiResponse<BookDto>.Success(result, "创建成功"));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BookDto>>> UpdateBook(int id, [FromBody] BookDto dto)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound(ApiResponse<BookDto>.Error(404, "图书不存在"));
        }

        book.Title = dto.Title;
        book.Author = dto.Author;
        book.ISBN = dto.ISBN;
        book.Category = dto.Category;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = dto.AvailableCopies;
        book.Location = dto.Location;
        book.PublishedDate = dto.PublishedDate;
        book.Price = dto.Price;

        await _bookService.UpdateBookAsync(book);
        var result = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Category = book.Category,
            TotalCopies = book.TotalCopies,
            AvailableCopies = book.AvailableCopies,
            Location = book.Location,
            PublishedDate = book.PublishedDate,
            Price = book.Price
        };
        return Ok(ApiResponse<BookDto>.Success(result, "更新成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBook(int id)
    {
        await _bookService.DeleteBookAsync(id);
        return Ok(ApiResponse<object>.Success(null, "删除成功"));
    }
}

[ApiController]
[Route("api/[controller]")]
public class BorrowRecordsController : ControllerBase
{
    private readonly IBorrowService _borrowService;

    public BorrowRecordsController(IBorrowService borrowService)
    {
        _borrowService = borrowService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BorrowRecordDto>>>> GetAllBorrowRecords([FromQuery] int? userId, [FromQuery] string? status)
    {
        var records = await _borrowService.GetAllBorrowRecordsAsync(userId, status);
        return Ok(ApiResponse<List<BorrowRecordDto>>.Success(records));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BorrowRecordDto>>> GetBorrowRecordById(int id)
    {
        var record = await _borrowService.GetBorrowRecordByIdAsync(id);
        if (record == null)
        {
            return NotFound(ApiResponse<BorrowRecordDto>.Error(404, "借阅记录不存在"));
        }
        var result = new BorrowRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            UserName = record.UserName,
            BookId = record.BookId,
            BookTitle = record.BookTitle,
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            ReturnDate = record.ReturnDate,
            Status = record.Status
        };
        return Ok(ApiResponse<BorrowRecordDto>.Success(result));
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<ApiResponse<BorrowRecordDto>>> BorrowBook([FromBody] BorrowRequest request)
    {
        var record = await _borrowService.BorrowBookAsync(request.UserId, request.BookId, request.Days);
        var result = new BorrowRecordDto
        {
            Id = record.Id,
            UserId = record.UserId,
            BookId = record.BookId,
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            Status = record.Status
        };
        return Ok(ApiResponse<BorrowRecordDto>.Success(result, "借阅成功"));
    }

    [HttpPost("{id}/return")]
    public async Task<ActionResult<ApiResponse<object>>> ReturnBook(int id)
    {
        await _borrowService.ReturnBookAsync(id);
        return Ok(ApiResponse<object>.Success(null, "归还成功"));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBorrowRecord(int id)
    {
        await _borrowService.DeleteBorrowRecordAsync(id);
        return Ok(ApiResponse<object>.Success(null, "删除成功"));
    }
}

public class BorrowRequest
{
    public int UserId { get; set; }
    public int BookId { get; set; }
    public int Days { get; set; } = 30;
}

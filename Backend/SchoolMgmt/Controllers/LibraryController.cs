using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Library;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    // POST /api/library/books
    [HttpPost("books")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> AddBook([FromBody] CreateBookDto dto)
    {
        try
        {
            var result = await _libraryService.AddBookAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/library/books?search=math&category=Science
    [HttpGet("books")]
    public async Task<IActionResult> GetAllBooks(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null)
    {
        var books = await _libraryService.GetAllBooksAsync(search, category);
        return Ok(books);
    }

    // GET /api/library/books/{bookId}
    [HttpGet("books/{bookId}")]
    public async Task<IActionResult> GetBook(Guid bookId)
    {
        try
        {
            var book = await _libraryService.GetBookByIdAsync(bookId);
            return Ok(book);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE /api/library/books/{bookId}
    [HttpDelete("books/{bookId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteBook(Guid bookId)
    {
        try
        {
            await _libraryService.DeleteBookAsync(bookId);
            return Ok(new { message = "Book deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // POST /api/library/issue
    [HttpPost("issue")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> IssueBook([FromBody] IssueBookDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var result = await _libraryService.IssueBookAsync(dto, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT /api/library/return/{issueId}
    [HttpPut("return/{issueId}")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> ReturnBook(
        Guid issueId, [FromBody] ReturnBookDto dto)
    {
        try
        {
            var result = await _libraryService.ReturnBookAsync(issueId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/library/issues/active
    [HttpGet("issues/active")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetActiveIssues()
    {
        var issues = await _libraryService.GetActiveIssuesAsync();
        return Ok(issues);
    }

    // GET /api/library/issues/overdue
    [HttpGet("issues/overdue")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetOverdueIssues()
    {
        var issues = await _libraryService.GetOverdueIssuesAsync();
        return Ok(issues);
    }

    // GET /api/library/issues/student/{studentId}
    [HttpGet("issues/student/{studentId}")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetStudentIssueHistory(Guid studentId)
    {
        var issues = await _libraryService.GetStudentIssueHistoryAsync(studentId);
        return Ok(issues);
    }

    // GET /api/library/my-issues — Student sees their own history
    [HttpGet("my-issues")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyIssueHistory()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        try
        {
            var issues = await _libraryService.GetMyIssueHistoryAsync(userId);
            return Ok(issues);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
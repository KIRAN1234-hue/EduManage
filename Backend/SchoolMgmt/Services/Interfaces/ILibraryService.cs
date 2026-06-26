using SchoolMgmt.DTOs.Library;

namespace SchoolMgmt.Services.Interfaces;

public interface ILibraryService
{
    Task<BookResponseDto> AddBookAsync(CreateBookDto dto);
    Task<IEnumerable<BookResponseDto>> GetAllBooksAsync(string? search = null, string? category = null);
    Task<BookResponseDto> GetBookByIdAsync(Guid bookId);
    Task<BookIssueResponseDto> IssueBookAsync(IssueBookDto dto, Guid issuedByUserId);
    Task<BookIssueResponseDto> ReturnBookAsync(Guid issueId, ReturnBookDto dto);
    Task<IEnumerable<BookIssueResponseDto>> GetActiveIssuesAsync();
    Task<IEnumerable<BookIssueResponseDto>> GetOverdueIssuesAsync();
    Task<IEnumerable<BookIssueResponseDto>> GetStudentIssueHistoryAsync(Guid studentId);
    Task<IEnumerable<BookIssueResponseDto>> GetMyIssueHistoryAsync(Guid userId);
    Task DeleteBookAsync(Guid bookId);
}
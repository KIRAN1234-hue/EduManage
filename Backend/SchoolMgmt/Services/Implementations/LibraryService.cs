using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Library;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class LibraryService : ILibraryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private const decimal FinePerDay = 2m;

    public LibraryService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<BookResponseDto> AddBookAsync(CreateBookDto dto)
    {
        var exists = await _unitOfWork.LibraryBooks
            .AnyAsync(b => b.ISBN == dto.ISBN);

        if (exists)
            throw new InvalidOperationException(
                $"Book with ISBN {dto.ISBN} already exists.");

        var book = new LibraryBook
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Category = dto.Category,
            Publisher = dto.Publisher,
            PublishedYear = dto.PublishedYear,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,
            Description = dto.Description,
            ShelfLocation = dto.ShelfLocation
        };

        await _unitOfWork.LibraryBooks.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();

        return MapBook(book);
    }

    public async Task<IEnumerable<BookResponseDto>> GetAllBooksAsync(
        string? search = null, string? category = null)
    {
        var query = _context.LibraryBooks.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(b =>
                b.Title.Contains(search) ||
                b.Author.Contains(search) ||
                b.ISBN.Contains(search));

        if (!string.IsNullOrEmpty(category))
            query = query.Where(b => b.Category == category);

        var books = await query.OrderBy(b => b.Title).ToListAsync();
        return books.Select(MapBook);
    }

    public async Task<BookResponseDto> GetBookByIdAsync(Guid bookId)
    {
        var book = await _unitOfWork.LibraryBooks.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException("Book not found.");

        return MapBook(book);
    }

    public async Task<BookIssueResponseDto> IssueBookAsync(
        IssueBookDto dto, Guid issuedByUserId)
    {
        var book = await _unitOfWork.LibraryBooks.GetByIdAsync(dto.BookId)
            ?? throw new KeyNotFoundException("Book not found.");

        if (book.AvailableCopies <= 0)
            throw new InvalidOperationException(
                $"No copies available for '{book.Title}'.");

        var alreadyIssued = await _unitOfWork.BookIssues.AnyAsync(i =>
            i.BookId == dto.BookId &&
            i.StudentId == dto.StudentId &&
            i.Status == IssueStatus.Issued);

        if (alreadyIssued)
            throw new InvalidOperationException(
                "Student already has this book issued.");

        book.AvailableCopies--;
        _unitOfWork.LibraryBooks.Update(book);

        var issue = new BookIssue
        {
            Id = Guid.NewGuid(),
            BookId = dto.BookId,
            StudentId = dto.StudentId,
            IssuedAt = DateTime.UtcNow,
            DueDate = dto.DueDate,        // DateTime — matches entity
            IsReturned = false,
            Status = IssueStatus.Issued,
            FineAmount = 0,
            Remarks = dto.Remarks,
            IssuedByUserId = issuedByUserId
        };

        await _unitOfWork.BookIssues.AddAsync(issue);
        await _unitOfWork.SaveChangesAsync();

        return await BuildIssueResponse(issue.Id);
    }

    public async Task<BookIssueResponseDto> ReturnBookAsync(
        Guid issueId, ReturnBookDto dto)
    {
        var issue = await _unitOfWork.BookIssues.GetByIdAsync(issueId)
            ?? throw new KeyNotFoundException("Issue record not found.");

        if (issue.IsReturned)
            throw new InvalidOperationException("Book already returned.");

        // Fine calculation — DueDate is DateTime
        decimal fine = 0;
        if (DateTime.UtcNow > issue.DueDate)
        {
            var daysLate = (int)(DateTime.UtcNow - issue.DueDate).TotalDays;
            fine = daysLate * FinePerDay;
        }

        issue.ReturnedAt = DateTime.UtcNow;
        issue.IsReturned = true;
        issue.Status = IssueStatus.Returned;
        issue.FineAmount = fine;
        issue.Remarks = dto.Remarks ?? issue.Remarks;

        _unitOfWork.BookIssues.Update(issue);

        var book = await _unitOfWork.LibraryBooks.GetByIdAsync(issue.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
            _unitOfWork.LibraryBooks.Update(book);
        }

        await _unitOfWork.SaveChangesAsync();
        return await BuildIssueResponse(issueId);
    }

    public async Task<IEnumerable<BookIssueResponseDto>> GetActiveIssuesAsync()
    {
        var issues = await _context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student).ThenInclude(s => s.User)
            .Where(i => i.Status == IssueStatus.Issued)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        return issues.Select(MapIssue);
    }

    public async Task<IEnumerable<BookIssueResponseDto>> GetOverdueIssuesAsync()
    {
        var issues = await _context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student).ThenInclude(s => s.User)
            .Where(i => i.Status != IssueStatus.Returned &&
                        DateTime.UtcNow > i.DueDate)
            .OrderBy(i => i.DueDate)
            .ToListAsync();

        foreach (var issue in issues.Where(i => i.Status != IssueStatus.Overdue))
        {
            issue.Status = IssueStatus.Overdue;
            _unitOfWork.BookIssues.Update(issue);
        }
        await _unitOfWork.SaveChangesAsync();

        return issues.Select(MapIssue);
    }

    public async Task<IEnumerable<BookIssueResponseDto>> GetStudentIssueHistoryAsync(
        Guid studentId)
    {
        var issues = await _context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student).ThenInclude(s => s.User)
            .Where(i => i.StudentId == studentId)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync();

        return issues.Select(MapIssue);
    }

    public async Task<IEnumerable<BookIssueResponseDto>> GetMyIssueHistoryAsync(
        Guid userId)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId)
            ?? throw new KeyNotFoundException("Student profile not found.");

        return await GetStudentIssueHistoryAsync(student.Id);
    }

    public async Task DeleteBookAsync(Guid bookId)
    {
        var book = await _unitOfWork.LibraryBooks.GetByIdAsync(bookId)
            ?? throw new KeyNotFoundException("Book not found.");

        var hasActive = await _unitOfWork.BookIssues
            .AnyAsync(i => i.BookId == bookId &&
                          i.Status == IssueStatus.Issued);

        if (hasActive)
            throw new InvalidOperationException(
                "Cannot delete — book has active issues. Return all copies first.");

        _unitOfWork.LibraryBooks.Remove(book);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<BookIssueResponseDto> BuildIssueResponse(Guid issueId)
    {
        var i = await _context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(i => i.Id == issueId)
            ?? throw new KeyNotFoundException("Issue not found.");

        return MapIssue(i);
    }

    private static BookResponseDto MapBook(LibraryBook b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        ISBN = b.ISBN,
        Category = b.Category,
        Publisher = b.Publisher,
        PublishedYear = b.PublishedYear,
        TotalCopies = b.TotalCopies,
        AvailableCopies = b.AvailableCopies,
        ShelfLocation = b.ShelfLocation,
        Description = b.Description
    };

    private static BookIssueResponseDto MapIssue(BookIssue i) => new()
    {
        Id = i.Id,
        BookTitle = i.Book?.Title ?? string.Empty,
        BookAuthor = i.Book?.Author ?? string.Empty,
        ISBN = i.Book?.ISBN ?? string.Empty,
        StudentName = i.Student?.User?.FullName ?? string.Empty,
        RollNumber = i.Student?.RollNumber ?? string.Empty,
        IssuedAt = i.IssuedAt,
        DueDate = i.DueDate,
        ReturnedAt = i.ReturnedAt,
        FineAmount = i.FineAmount,
        IsReturned = i.IsReturned,
        Status = i.Status.ToString(),
        Remarks = i.Remarks
    };
}
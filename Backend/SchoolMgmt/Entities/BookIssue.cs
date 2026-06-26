using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class BookIssue
{
    public Guid Id { get; set; }

    public Guid BookId { get; set; }
    public LibraryBook Book { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid IssuedByUserId { get; set; }
    public ApplicationUser IssuedBy { get; set; } = null!;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public decimal FineAmount { get; set; } = 0;
    public bool IsReturned { get; set; } = false;
    public IssueStatus Status { get; set; } = IssueStatus.Issued;
    public string? Remarks { get; set; }
}
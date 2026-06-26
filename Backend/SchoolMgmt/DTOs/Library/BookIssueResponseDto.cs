namespace SchoolMgmt.DTOs.Library;

public class BookIssueResponseDto
{
    public Guid Id { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string BookAuthor { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime DueDate { get; set; }   // DateTime — matches entity
    public DateTime? ReturnedAt { get; set; }
    public decimal FineAmount { get; set; }   // FineAmount — matches entity
    public bool IsReturned { get; set; }
    public string Status { get; set; } = string.Empty;  // IssueStatus.ToString()
    public string? Remarks { get; set; }
    public bool IsOverdue => !IsReturned && DateTime.UtcNow > DueDate;
}
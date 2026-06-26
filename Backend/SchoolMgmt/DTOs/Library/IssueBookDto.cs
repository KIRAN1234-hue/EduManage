namespace SchoolMgmt.DTOs.Library;

public class IssueBookDto
{
    public Guid BookId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime DueDate { get; set; }   // DateTime — matches entity
    public string? Remarks { get; set; }
}
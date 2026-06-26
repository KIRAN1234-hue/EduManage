namespace SchoolMgmt.DTOs.Assignment;

public class CreateAssignmentDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }      // DateTime — matches entity
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public int TotalMarks { get; set; } = 10;
    public string? FilePath { get; set; }       // Azure Blob URL if any
}
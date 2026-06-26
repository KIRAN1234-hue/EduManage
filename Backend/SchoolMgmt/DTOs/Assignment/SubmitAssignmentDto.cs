namespace SchoolMgmt.DTOs.Assignment;

public class SubmitAssignmentDto
{
    public string Content { get; set; } = string.Empty;
    public string? FilePath { get; set; }  // Azure Blob URL after upload
}
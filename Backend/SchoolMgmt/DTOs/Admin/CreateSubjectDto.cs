namespace SchoolMgmt.DTOs.Admin;

public class CreateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int MaxMarks { get; set; } = 100;
    public bool IsElective { get; set; } = false;
    public Guid ClassId { get; set; }
    public Guid? TeacherId { get; set; }
}
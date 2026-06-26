namespace SchoolMgmt.DTOs.Admin;

public class ClassResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int MaxStrength { get; set; }
    public int StudentCount { get; set; }
    public string ClassTeacher { get; set; } = string.Empty;
}
namespace SchoolMgmt.DTOs.Admin;

public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int MaxStrength { get; set; } = 40;
}
namespace SchoolMgmt.DTOs.Admin;

public class TeacherForDoubtDto
{
    public Guid UserId { get; set; }   // AspNetUsers.Id — for messaging
    public string FullName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
}
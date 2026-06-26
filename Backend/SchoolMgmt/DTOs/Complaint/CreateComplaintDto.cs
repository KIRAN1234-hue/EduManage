using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.Complaint;

public class CreateComplaintDto
{
    public ComplaintCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
}
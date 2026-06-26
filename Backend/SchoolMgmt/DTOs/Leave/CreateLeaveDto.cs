namespace SchoolMgmt.DTOs.Leave;

public class CreateLeaveDto
{
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
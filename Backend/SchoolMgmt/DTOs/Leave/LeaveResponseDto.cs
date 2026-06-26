namespace SchoolMgmt.DTOs.Leave;

public class LeaveResponseDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovalRemark { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime AppliedOn { get; set; }
}
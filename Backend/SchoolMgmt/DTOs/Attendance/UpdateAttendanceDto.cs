namespace SchoolMgmt.DTOs.Attendance
{
    public class UpdateAttendanceDto
    {
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}

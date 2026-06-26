namespace SchoolMgmt.DTOs.Attendance
{
    public class MarkAttendanceDto
    {
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }
        public DateOnly Date { get; set; }
        public string Status { get; set; } = string.Empty; // "Present","Absent","Late","OnLeave"
        public string? Remarks { get; set; }
    }
}

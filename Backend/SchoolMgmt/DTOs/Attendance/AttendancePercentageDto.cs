namespace SchoolMgmt.DTOs.Attendance
{
    public class AttendancePercentageDto
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int TotalClasses { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public decimal Percentage { get; set; }

        // Green = safe, Amber = warning, Red = danger
        public string RiskLevel => Percentage >= 75 ? "Green"
                                 : Percentage >= 65 ? "Amber"
                                 : "Red";
    }
}

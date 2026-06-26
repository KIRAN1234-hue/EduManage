namespace SchoolMgmt.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int TotalSubjects { get; set; }
    public int PendingLeaveApplications { get; set; }
    public int TotalAssignments { get; set; }
    public int TotalNotices { get; set; }
    public double AttendanceTodayPercent { get; set; }
    public IEnumerable<RecentActivityDto> RecentActivities { get; set; }
        = new List<RecentActivityDto>();
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
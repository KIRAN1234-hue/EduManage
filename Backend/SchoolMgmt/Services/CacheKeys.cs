namespace SchoolMgmt.Services;

public static class CacheKeys
{
    // Timetable — 1 hour TTL
    public static string ClassTimetable(Guid classId, string year)
        => $"timetable:class:{classId}:{year}";

    public static string TeacherTimetable(Guid teacherId, string year)
        => $"timetable:teacher:{teacherId}:{year}";

    // Reference data — 30 min TTL
    public static string AllClasses() => "admin:classes:all";
    public static string AllSubjects() => "admin:subjects:all";
    public static string ClassSubjects(Guid classId) => $"admin:subjects:class:{classId}";

    // Analytics — 15 min TTL
    public static string AttendanceTrend(Guid studentId) => $"analytics:attendance:{studentId}";
    public static string ClassPerformance(Guid classId) => $"analytics:marks:{classId}";
    public static string FeeAnalytics(string year) => $"analytics:fees:{year}";
    public static string DashboardStats(string role) => $"analytics:dashboard:{role}";

    // Prefixes for bulk invalidation
    public const string TimetablePrefix = "timetable:";
    public const string AdminPrefix = "admin:";
    public const string AnalyticsPrefix = "analytics:";
}
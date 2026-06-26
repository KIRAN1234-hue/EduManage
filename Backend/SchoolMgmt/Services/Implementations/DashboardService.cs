using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Dashboard;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly ICacheService _cacheService;

    public DashboardService(IUnitOfWork unitOfWork, AppDbContext context, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<DashboardStatsDto> GetPrincipalStatsAsync()
    {
        var cacheKey = CacheKeys.DashboardStats("principal");

        // Check cache first
        var cached = await _cacheService.GetAsync<DashboardStatsDto>(cacheKey);
        if (cached != null)
            return cached;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var totalStudents = await _unitOfWork.Students.CountAsync();
        var totalTeachers = await _unitOfWork.Teachers.CountAsync();
        var totalClasses = await _unitOfWork.Classes.CountAsync();
        var totalSubjects = await _unitOfWork.Subjects.CountAsync();

        var pendingLeaves = await _unitOfWork.LeaveApplications
            .CountAsync(l => l.Status == LeaveStatus.Pending);

        var totalAssignments = await _unitOfWork.Assignments.CountAsync();
        var totalNotices = await _unitOfWork.Notices.CountAsync();

        // Attendance today
        var todayAttendance = await _context.Attendances
            .Where(a => a.Date == today)
            .ToListAsync();

        double attendancePercent = 0;

        if (todayAttendance.Any())
        {
            var presentCount = todayAttendance.Count(a =>
                a.Status == AttendanceStatus.Present ||
                a.Status == AttendanceStatus.Late);

            attendancePercent = Math.Round(
                (double)presentCount / todayAttendance.Count * 100,
                1);
        }

        // Recent Activities
        var recentActivities = new List<RecentActivityDto>();

        var recentLeaves = await _context.LeaveApplications
            .Include(l => l.Student)
                .ThenInclude(s => s.User)
            .OrderByDescending(l => l.AppliedOn)
            .Take(3)
            .ToListAsync();

        recentActivities.AddRange(
            recentLeaves.Select(l => new RecentActivityDto
            {
                Type = "Leave",
                Message = $"{l.Student?.User?.FullName} applied for {l.LeaveType}",
                OccurredAt = l.AppliedOn
            }));

        var recentNotices = await _context.Notices
            .Include(n => n.CreatedBy)
            .OrderByDescending(n => n.PublishedAt)
            .Take(3)
            .ToListAsync();

        recentActivities.AddRange(
            recentNotices.Select(n => new RecentActivityDto
            {
                Type = "Notice",
                Message = $"Notice posted: {n.Title}",
                OccurredAt = n.PublishedAt
            }));

        var result = new DashboardStatsDto
        {
            TotalStudents = totalStudents,
            TotalTeachers = totalTeachers,
            TotalClasses = totalClasses,
            TotalSubjects = totalSubjects,
            PendingLeaveApplications = pendingLeaves,
            TotalAssignments = totalAssignments,
            TotalNotices = totalNotices,
            AttendanceTodayPercent = attendancePercent,
            RecentActivities = recentActivities
                .OrderByDescending(r => r.OccurredAt)
                .Take(5)
                .ToList()
        };

        // Store in cache for 15 minutes
        await _cacheService.SetAsync(
            cacheKey,
            result,
            TimeSpan.FromMinutes(15));

        return result;
    }
    public async Task<DashboardStatsDto> GetTeacherStatsAsync(Guid teacherId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var myAssignments = await _unitOfWork.Assignments
            .CountAsync(a => a.TeacherId == teacherId && a.IsActive);
        var pendingLeaves = await _unitOfWork.LeaveApplications
            .CountAsync(l => l.Status == LeaveStatus.Pending);

        var myStudentCount = await _context.Teachers
            .Include(t => t.Class).ThenInclude(c => c.Students)
            .Where(t => t.Id == teacherId)
            .Select(t => t.Class != null ? t.Class.Students.Count : 0)
            .FirstOrDefaultAsync();

        var todayAttendance = await _context.Attendances
            .Where(a => a.Date == today && a.MarkedByTeacherId == teacherId)
            .ToListAsync();

        double attendancePercent = 0;
        if (todayAttendance.Any())
        {
            var presentCount = todayAttendance
                .Count(a => a.Status == AttendanceStatus.Present
                         || a.Status == AttendanceStatus.Late);
            attendancePercent = Math.Round(
                (double)presentCount / todayAttendance.Count * 100, 1);
        }

        return new DashboardStatsDto
        {
            TotalStudents = myStudentCount,
            TotalAssignments = myAssignments,
            PendingLeaveApplications = pendingLeaves,
            AttendanceTodayPercent = attendancePercent
        };
    }

    public async Task<DashboardStatsDto> GetStudentStatsAsync(Guid studentId)
    {
        var pendingLeaves = await _unitOfWork.LeaveApplications
            .CountAsync(l => l.StudentId == studentId &&
                             l.Status == LeaveStatus.Pending);

        var totalAttendance = await _unitOfWork.Attendances
            .CountAsync(a => a.StudentId == studentId);

        var presentCount = await _unitOfWork.Attendances
            .CountAsync(a => a.StudentId == studentId &&
                            (a.Status == AttendanceStatus.Present ||
                             a.Status == AttendanceStatus.Late));

        double attendancePercent = totalAttendance > 0
            ? Math.Round((double)presentCount / totalAttendance * 100, 1)
            : 0;

        return new DashboardStatsDto
        {
            AttendanceTodayPercent = attendancePercent,
            PendingLeaveApplications = pendingLeaves
        };
    }
}
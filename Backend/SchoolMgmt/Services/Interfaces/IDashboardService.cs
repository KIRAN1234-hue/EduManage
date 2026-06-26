using SchoolMgmt.DTOs.Dashboard;

namespace SchoolMgmt.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetPrincipalStatsAsync();
    Task<DashboardStatsDto> GetTeacherStatsAsync(Guid teacherId);
    Task<DashboardStatsDto> GetStudentStatsAsync(Guid studentId);
}
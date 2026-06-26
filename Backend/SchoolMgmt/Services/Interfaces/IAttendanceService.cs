using SchoolMgmt.DTOs.Attendance;

namespace SchoolMgmt.Services.Interfaces;

public interface IAttendanceService
{
    // Teacher marks attendance for multiple students at once
    Task<string> MarkBulkAttendanceAsync(
    List<MarkAttendanceDto> attendanceDtos, Guid? teacherId);

    // Student or parent views full attendance record
    Task<IEnumerable<AttendanceResponseDto>> GetStudentAttendanceAsync(Guid studentId);

    // Dashboard percentage per subject
    Task<IEnumerable<AttendancePercentageDto>> GetAttendancePercentageAsync(Guid studentId);

    // Teacher views class attendance for a date
    Task<IEnumerable<AttendanceResponseDto>> GetClassAttendanceForDateAsync(
        Guid classId, DateOnly date);

    // Teacher corrects a past attendance record
    Task<AttendanceResponseDto> UpdateAttendanceAsync(
        Guid attendanceId, UpdateAttendanceDto dto, Guid? teacherId);
}
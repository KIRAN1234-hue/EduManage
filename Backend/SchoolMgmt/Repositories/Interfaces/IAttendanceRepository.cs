using SchoolMgmt.Entities;

namespace SchoolMgmt.Repositories.Interfaces;

// Extends IGenericRepository — gets all CRUD methods free
// Adds complex queries that generic repo cannot handle
public interface IAttendanceRepository : IGenericRepository<Attendance>
{
    // Get full attendance record for a student with related data included
    Task<IEnumerable<Attendance>> GetStudentAttendanceAsync(
        Guid studentId, string? academicYear = null);

    // Get all student attendances for a class on a specific date
    Task<IEnumerable<Attendance>> GetClassAttendanceForDateAsync(
        Guid classId, DateOnly date);

    // Check if attendance already marked for student+subject+date
    Task<bool> IsAlreadyMarkedAsync(
        Guid studentId, Guid subjectId, DateOnly date);

    // Get all attendance records for a student in a specific subject
    Task<IEnumerable<Attendance>> GetStudentSubjectAttendanceAsync(
        Guid studentId, Guid subjectId);
}
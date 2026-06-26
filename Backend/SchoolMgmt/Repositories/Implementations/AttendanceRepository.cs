using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.Entities;
using SchoolMgmt.Repositories.Interfaces;

namespace SchoolMgmt.Repositories.Implementations;

public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Attendance>> GetStudentAttendanceAsync(
        Guid studentId, string? academicYear = null)
    {
        return await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .Include(a => a.MarkedByTeacher)
                .ThenInclude(t => t.User)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Attendance>> GetClassAttendanceForDateAsync(
        Guid classId, DateOnly date)
    {
        return await _context.Attendances
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Subject)
            .Where(a => a.Student.ClassId == classId && a.Date == date)
            .OrderBy(a => a.Student.RollNumber)
            .ToListAsync();
    }

    public async Task<bool> IsAlreadyMarkedAsync(
        Guid studentId, Guid subjectId, DateOnly date)
    {
        return await _context.Attendances
            .AnyAsync(a =>
                a.StudentId == studentId &&
                a.SubjectId == subjectId &&
                a.Date == date);
    }

    public async Task<IEnumerable<Attendance>> GetStudentSubjectAttendanceAsync(
        Guid studentId, Guid subjectId)
    {
        return await _context.Attendances
            .Where(a => a.StudentId == studentId && a.SubjectId == subjectId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
}
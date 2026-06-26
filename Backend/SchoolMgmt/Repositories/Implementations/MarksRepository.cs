using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;

namespace SchoolMgmt.Repositories.Implementations;

public class MarksRepository : GenericRepository<Mark>, IMarksRepository
{
    public MarksRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Mark>> GetStudentMarksAsync(Guid studentId)
    {
        return await _context.Marks
            .Include(m => m.Subject)
            .Include(m => m.Student)
                .ThenInclude(s => s.User)
            .Include(m => m.Student)
                .ThenInclude(s => s.Class)
            .Include(m => m.EnteredByTeacher)
                .ThenInclude(t => t.User)
            .Where(m => m.StudentId == studentId)
            .OrderBy(m => m.Subject.Name)
            .ThenBy(m => m.ExamType)
            .ToListAsync();
    }

    public async Task<IEnumerable<Mark>> GetClassMarksAsync(
        Guid classId, string examType)
    {
        if (!Enum.TryParse<ExamType>(examType, ignoreCase: true, out var examTypeEnum))
            return Enumerable.Empty<Mark>();

        return await _context.Marks
            .Include(m => m.Student)
                .ThenInclude(s => s.User)
            .Include(m => m.Subject)
            .Where(m =>
                m.Student.ClassId == classId &&
                m.ExamType == examTypeEnum)
            .OrderBy(m => m.Student.RollNumber)
            .ToListAsync();
    }

    public async Task<bool> IsAlreadyEnteredAsync(
        Guid studentId, Guid subjectId, string examType)
    {
        if (!Enum.TryParse<ExamType>(examType, ignoreCase: true, out var examTypeEnum))
            return false;

        return await _context.Marks.AnyAsync(m =>
            m.StudentId == studentId &&
            m.SubjectId == subjectId &&
            m.ExamType == examTypeEnum);
    }

    public async Task<IEnumerable<Mark>> GetStudentMarksWithDetailsAsync(
        Guid studentId)
    {
        return await _context.Marks
            .Include(m => m.Subject)
            .Where(m => m.StudentId == studentId)
            .OrderBy(m => m.Subject.Name)
            .ToListAsync();
    }
}
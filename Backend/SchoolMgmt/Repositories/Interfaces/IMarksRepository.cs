using SchoolMgmt.Entities;

namespace SchoolMgmt.Repositories.Interfaces;

public interface IMarksRepository : IGenericRepository<Mark>
{
    // All marks for a student — used for report card
    Task<IEnumerable<Mark>> GetStudentMarksAsync(Guid studentId);

    // All marks for a class for a specific exam type — used by teacher
    Task<IEnumerable<Mark>> GetClassMarksAsync(Guid classId, string examType);

    // Check if mark already entered for this student+subject+examtype
    Task<bool> IsAlreadyEnteredAsync(
        Guid studentId, Guid subjectId, string examType);

    // Marks for chart data — student performance across subjects
    Task<IEnumerable<Mark>> GetStudentMarksWithDetailsAsync(Guid studentId);
}
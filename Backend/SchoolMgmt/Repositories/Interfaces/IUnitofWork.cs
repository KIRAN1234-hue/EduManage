using SchoolMgmt.Entities;

namespace SchoolMgmt.Repositories.Interfaces;

// Unit of Work wraps all repositories under one transaction
// When you call SaveChangesAsync, ALL changes across ALL repos
// are saved in one single database transaction
public interface IUnitOfWork : IDisposable
{
    // One property per entity
    IGenericRepository<Student> Students { get; }
    IGenericRepository<Teacher> Teachers { get; }
    IGenericRepository<Parent> Parents { get; }
    IGenericRepository<Class> Classes { get; }
    IGenericRepository<Subject> Subjects { get; }
    IAttendanceRepository Attendances { get; }
    IMarksRepository Marks { get; }
    IGenericRepository<Assignment> Assignments { get; }
    IGenericRepository<Submission> Submissions { get; }
    IGenericRepository<Message> Messages { get; }
    IGenericRepository<FeeStructure> FeeStructures { get; }
    IGenericRepository<FeePayment> FeePayments { get; }
    IGenericRepository<LeaveApplication> LeaveApplications { get; }
    IGenericRepository<Complaint> Complaints { get; }
    IGenericRepository<Notice> Notices { get; }
    IGenericRepository<NoticeAcknowledgement> NoticeAcknowledgements { get; }
    IGenericRepository<AuditLog> AuditLogs { get; }
    IGenericRepository<LibraryBook> LibraryBooks { get; }
    IGenericRepository<BookIssue> BookIssues { get; }
    IGenericRepository<ExamSchedule> ExamSchedules { get; }
    IGenericRepository<Timetable> Timetables { get; }

    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IGenericRepository<Notification> Notifications { get; } 

    // Saves all pending changes to database in one transaction
    Task<int> SaveChangesAsync();
}
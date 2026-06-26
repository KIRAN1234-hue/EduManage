using SchoolMgmt.Data;
using SchoolMgmt.Entities;
using SchoolMgmt.Repositories.Interfaces;
using System;

namespace SchoolMgmt.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    // Lazy initialization — repository only created when first accessed
    private IGenericRepository<Student>? _students;
    private IGenericRepository<Teacher>? _teachers;
    private IGenericRepository<Parent>? _parents;
    private IGenericRepository<Class>? _classes;
    private IGenericRepository<Subject>? _subjects;
    private IAttendanceRepository? _attendances;
    private IMarksRepository? _marks;
    private IGenericRepository<Assignment>? _assignments;
    private IGenericRepository<Submission>? _submissions;
    private IGenericRepository<Message>? _messages;
    private IGenericRepository<FeeStructure>? _feeStructures;
    private IGenericRepository<FeePayment>? _feePayments;
    private IGenericRepository<LeaveApplication>? _leaveApplications;
    private IGenericRepository<Complaint>? _complaints;
    private IGenericRepository<Notice>? _notices;
    private IGenericRepository<NoticeAcknowledgement>? _noticeAcknowledgements;
    private IGenericRepository<AuditLog>? _auditLogs;
    private IGenericRepository<LibraryBook>? _libraryBooks;
    private IGenericRepository<BookIssue>? _bookIssues;
    private IGenericRepository<ExamSchedule>? _examSchedules;
    private IGenericRepository<Timetable>? _timetables;
    private IGenericRepository<RefreshToken>? _refreshTokens;
    public IGenericRepository<Notification>? _notifications;


    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    // Each property creates the repository only once (lazy)
    public IGenericRepository<Student>
        Students => _students ??= new GenericRepository<Student>(_context);

    public IGenericRepository<Teacher>
        Teachers => _teachers ??= new GenericRepository<Teacher>(_context);

    public IGenericRepository<Parent>
        Parents => _parents ??= new GenericRepository<Parent>(_context);

    public IGenericRepository<Class>
        Classes => _classes ??= new GenericRepository<Class>(_context);

    public IGenericRepository<Subject>
        Subjects => _subjects ??= new GenericRepository<Subject>(_context);

    public IAttendanceRepository
    Attendances => _attendances ??= new AttendanceRepository(_context);

    public IMarksRepository
    Marks => _marks ??= new MarksRepository(_context);

    public IGenericRepository<Assignment>
        Assignments => _assignments ??= new GenericRepository<Assignment>(_context);

    public IGenericRepository<Submission>
        Submissions => _submissions ??= new GenericRepository<Submission>(_context);

    public IGenericRepository<Notification>
        Notifications => _notifications ??= new GenericRepository<Notification>(_context);

    public IGenericRepository<Message>
        Messages => _messages ??= new GenericRepository<Message>(_context);

    public IGenericRepository<FeeStructure>
        FeeStructures => _feeStructures ??= new GenericRepository<FeeStructure>(_context);

    public IGenericRepository<FeePayment>
        FeePayments => _feePayments ??= new GenericRepository<FeePayment>(_context);

    public IGenericRepository<LeaveApplication>
        LeaveApplications => _leaveApplications ??= new GenericRepository<LeaveApplication>(_context);

    public IGenericRepository<Complaint>
        Complaints => _complaints ??= new GenericRepository<Complaint>(_context);

    public IGenericRepository<Notice>
        Notices => _notices ??= new GenericRepository<Notice>(_context);

    public IGenericRepository<NoticeAcknowledgement>
        NoticeAcknowledgements => _noticeAcknowledgements ??= new GenericRepository<NoticeAcknowledgement>(_context);

    public IGenericRepository<AuditLog>
        AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_context);

    public IGenericRepository<LibraryBook>
        LibraryBooks => _libraryBooks ??= new GenericRepository<LibraryBook>(_context);

    public IGenericRepository<BookIssue>
        BookIssues => _bookIssues ??= new GenericRepository<BookIssue>(_context);

    public IGenericRepository<ExamSchedule>
        ExamSchedules => _examSchedules ??= new GenericRepository<ExamSchedule>(_context);

    public IGenericRepository<Timetable>
        Timetables => _timetables ??= new GenericRepository<Timetable>(_context);

    public IGenericRepository<RefreshToken>
        RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}
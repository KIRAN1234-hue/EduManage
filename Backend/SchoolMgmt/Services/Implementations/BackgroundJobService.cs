using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolMgmt.Data;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(
        AppDbContext context,
        IEmailService emailService,
        ILogger<BackgroundJobService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    // ── Job 1: Daily attendance check — runs at 6PM every day
    public async Task CheckAttendanceAndAlertParentsAsync()
    {
        _logger.LogInformation("Running daily attendance check job...");

        var students = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent).ThenInclude(p => p!.User)
            .ToListAsync();

        foreach (var student in students)
        {
            try
            {
                // Calculate overall attendance
                var totalClasses = await _context.Attendances
                    .CountAsync(a => a.StudentId == student.Id);

                if (totalClasses == 0) continue;

                var present = await _context.Attendances
                    .CountAsync(a => a.StudentId == student.Id &&
                        (a.Status == Enums.AttendanceStatus.Present ||
                         a.Status == Enums.AttendanceStatus.Late));

                var percentage = (double)present / totalClasses * 100;

                if (percentage < 75 && student.Parent?.User != null)
                {
                    var belowSubjects = await _context.Subjects
                        .Where(sub => _context.Attendances
                            .Where(a => a.StudentId == student.Id &&
                                        a.SubjectId == sub.Id)
                            .Any()
                            &&
                            (double)_context.Attendances
                                .Count(a => a.StudentId == student.Id &&
                                            a.SubjectId == sub.Id &&
                                            (a.Status == Enums.AttendanceStatus.Present ||
                                             a.Status == Enums.AttendanceStatus.Late))
                            / _context.Attendances
                                .Count(a => a.StudentId == student.Id &&
                                            a.SubjectId == sub.Id)
                            * 100 < 75
                        ).CountAsync();

                    await _emailService.SendAsync(
                        toEmail: student.Parent.User.Email!,
                        toName: student.Parent.User.FullName,
                        subject: $"Attendance Alert — {student.User?.FullName}",
                        htmlBody: EmailTemplates.AttendanceAlert(
                            student.User?.FullName ?? string.Empty,
                            student.Parent.User.FullName,
                            percentage,
                            belowSubjects)
                    );

                    _logger.LogInformation(
                        "Attendance alert sent for student: {Name}",
                        student.User?.FullName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Attendance alert failed for student {Id}", student.Id);
            }
        }
    }

    // ── Job 2: Exam reminder — 2 days before exam
    public async Task SendExamRemindersAsync()
    {
        _logger.LogInformation("Running exam reminder job...");

        var targetDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

        var exams = await _context.ExamSchedules
            .Include(e => e.Subject)
            .Include(e => e.Class)
                .ThenInclude(c => c.Students)
                    .ThenInclude(s => s.User)
            .Where(e => e.ExamDate == targetDate)
            .ToListAsync();

        foreach (var exam in exams)
        {
            foreach (var student in exam.Class.Students)
            {
                if (student.User == null) continue;

                try
                {
                    await _emailService.SendAsync(
                        toEmail: student.User.Email!,
                        toName: student.User.FullName,
                        subject: $"Exam Reminder — {exam.Subject?.Name}",
                        htmlBody: EmailTemplates.ExamReminder(
                            student.User.FullName,
                            exam.Subject?.Name ?? string.Empty,
                            exam.ExamDate.ToString("MMMM dd, yyyy"),
                            exam.RoomNumber)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Exam reminder failed for {Student}", student.Id);
                }
            }
        }
    }

    // ── Job 3: Library return reminder — 2 days before due
    public async Task SendLibraryReturnRemindersAsync()
    {
        _logger.LogInformation("Running library return reminder job...");

        var targetDate = DateTime.Today.AddDays(2);

        var issues = await _context.BookIssues
            .Include(i => i.Book)
            .Include(i => i.Student).ThenInclude(s => s.User)
            .Where(i => !i.IsReturned &&
                        i.DueDate.Date == targetDate.Date)
            .ToListAsync();

        foreach (var issue in issues)
        {
            if (issue.Student?.User == null) continue;

            try
            {
                await _emailService.SendAsync(
                    toEmail: issue.Student.User.Email!,
                    toName: issue.Student.User.FullName,
                    subject: $"Library Return Reminder — {issue.Book?.Title}",
                    htmlBody: EmailTemplates.LibraryReturnReminder(
                        issue.Student.User.FullName,
                        issue.Book?.Title ?? string.Empty,
                        issue.DueDate.ToString("MMMM dd, yyyy"))
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Library reminder failed for {Issue}", issue.Id);
            }
        }
    }

    // ── Job 4: Overdue fee reminder — runs monthly
    public async Task SendOverdueFeeRemindersAsync()
    {
        _logger.LogInformation("Running overdue fee reminder job...");

        var today = DateOnly.FromDateTime(DateTime.Today);

        var overdueStructures = await _context.FeeStructures
            .Include(f => f.Class)
                .ThenInclude(c => c.Students)
                    .ThenInclude(s => s.User)
            .Where(f => f.DueDate < today)
            .ToListAsync();

        foreach (var structure in overdueStructures)
        {
            foreach (var student in structure.Class.Students)
            {
                if (student.User == null) continue;

                // Check if fully paid
                var paid = await _context.FeePayments
                    .Where(p => p.StudentId == student.Id &&
                                p.FeeStructureId == structure.Id)
                    .SumAsync(p => p.Amount - p.DiscountAmount);

                if (paid >= structure.Amount) continue;

                // Find parent
                var parent = await _context.Parents.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == student.ParentId);

                if (parent?.User == null) continue;

                try
                {
                    await _emailService.SendAsync(
                        toEmail: parent.User.Email!,
                        toName: parent.User.FullName,
                        subject: $"Fee Due — {structure.TermName} — {student.User.FullName}",
                        htmlBody: EmailTemplates.FeeReminder(
                            student.User.FullName,
                            parent.User.FullName,
                            structure.TermName,
                            structure.Amount - paid,
                            structure.DueDate.ToString("MMMM dd, yyyy"))
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Fee reminder failed for student {Id}", student.Id);
                }
            }
        }
    }
}
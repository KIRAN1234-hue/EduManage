using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Assignment;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public AssignmentService(IUnitOfWork unitOfWork, AppDbContext context, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<AssignmentResponseDto> CreateAssignmentAsync(
        CreateAssignmentDto dto, Guid teacherId)
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate,           // DateTime
            SubjectId = dto.SubjectId,
            TeacherId = teacherId,
            ClassId = dto.ClassId,
            TotalMarks = dto.TotalMarks,
            FilePath = dto.FilePath,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Assignments.AddAsync(assignment);
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch ( Exception ex)
        {
            
        }

        await NotifyClassAssignmentAsync(
    assignment.ClassId,
    assignment.Title,
    assignment.DueDate.ToString("MMM dd, yyyy")
);

        return await GetAssignmentByIdAsync(assignment.Id);
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetClassAssignmentsAsync(
        Guid classId)
    {
        return await _context.Assignments
            .Include(a => a.Subject)
            .Include(a => a.Teacher).ThenInclude(t => t.User)
            .Include(a => a.Class)
            .Include(a => a.Submissions)
            .Where(a => a.ClassId == classId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                TotalMarks = a.TotalMarks,
                FilePath = a.FilePath,
                IsActive = a.IsActive,
                SubjectName = a.Subject != null ? a.Subject.Name : string.Empty,
                ClassName = a.Class != null
                    ? $"{a.Class.Name} {a.Class.Section}" : string.Empty,
                TeacherName = a.Teacher != null
                    ? a.Teacher.User.FullName : string.Empty,
                CreatedAt = a.CreatedAt,
                SubmissionCount = a.Submissions.Count
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<AssignmentResponseDto>> GetTeacherAssignmentsAsync(
        Guid teacherId)
    {
        return await _context.Assignments
            .Include(a => a.Subject)
            .Include(a => a.Class)
            .Include(a => a.Submissions)
            .Where(a => a.TeacherId == teacherId && a.IsActive)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssignmentResponseDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                TotalMarks = a.TotalMarks,
                FilePath = a.FilePath,
                IsActive = a.IsActive,
                SubjectName = a.Subject != null ? a.Subject.Name : string.Empty,
                ClassName = a.Class != null
                    ? $"{a.Class.Name} {a.Class.Section}" : string.Empty,
                CreatedAt = a.CreatedAt,
                SubmissionCount = a.Submissions.Count
            })
            .ToListAsync();
    }

    public async Task<AssignmentResponseDto> GetAssignmentByIdAsync(Guid assignmentId)
    {
        var a = await _context.Assignments
            .Include(a => a.Subject)
            .Include(a => a.Teacher).ThenInclude(t => t.User)
            .Include(a => a.Class)
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new KeyNotFoundException($"Assignment not found.");

        return new AssignmentResponseDto
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            DueDate = a.DueDate,
            TotalMarks = a.TotalMarks,
            FilePath = a.FilePath,
            IsActive = a.IsActive,
            SubjectName = a.Subject?.Name ?? string.Empty,
            ClassName = a.Class != null
                ? $"{a.Class.Name} {a.Class.Section}" : string.Empty,
            TeacherName = a.Teacher?.User?.FullName ?? string.Empty,
            CreatedAt = a.CreatedAt,
            SubmissionCount = a.Submissions.Count
        };
    }

    public async Task<SubmissionResponseDto> SubmitAssignmentAsync(
        Guid assignmentId, Guid studentId, SubmitAssignmentDto dto)
    {
        var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId)
            ?? throw new KeyNotFoundException("Assignment not found.");

        var alreadySubmitted = await _unitOfWork.Submissions.AnyAsync(s =>
            s.AssignmentId == assignmentId && s.StudentId == studentId);

        if (alreadySubmitted)
            throw new InvalidOperationException(
                "You have already submitted this assignment.");

        // Check if submission is late
        var isLate = DateTime.UtcNow > assignment.DueDate;

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            Content = dto.Content,
            FilePath = dto.FilePath ?? string.Empty,
            SubmittedAt = DateTime.UtcNow,
            IsLate = isLate,
            Status = isLate
                ? SubmissionStatus.Late
                : SubmissionStatus.Submitted
        };

        await _unitOfWork.Submissions.AddAsync(submission);
        await _unitOfWork.SaveChangesAsync();


        return await BuildSubmissionResponse(submission.Id);
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync(
        Guid assignmentId)
    {
        var subs = await _context.Submissions
            .Include(s => s.Student).ThenInclude(st => st.User)
            .Include(s => s.Assignment)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderBy(s => s.Student.RollNumber)
            .ToListAsync();

        return subs.Select(s => new SubmissionResponseDto
        {
            Id = s.Id,
            StudentName = s.Student?.User?.FullName ?? string.Empty,
            RollNumber = s.Student?.RollNumber ?? string.Empty,
            Content = s.Content,
            FilePath = s.FilePath,
            SubmittedAt = s.SubmittedAt,
            IsLate = s.IsLate,
            MarksAwarded = s.MarksAwarded,
            TeacherRemark = s.TeacherRemark,
            Feedback = s.Feedback,
            Status = s.Status.ToString(),
            AssignmentTitle = s.Assignment?.Title ?? string.Empty
        });
    }

    public async Task<SubmissionResponseDto> GradeSubmissionAsync(
        Guid submissionId, GradeSubmissionDto dto)
    {
        var submission = await _unitOfWork.Submissions.GetByIdAsync(submissionId)
            ?? throw new KeyNotFoundException("Submission not found.");

        submission.MarksAwarded = dto.MarksAwarded;
        submission.TeacherRemark = dto.TeacherRemark;
        submission.Feedback = dto.Feedback;
        submission.Status = SubmissionStatus.Graded;

        _unitOfWork.Submissions.Update(submission);
        await _unitOfWork.SaveChangesAsync();

        return await BuildSubmissionResponse(submissionId);
    }

    public async Task<SubmissionResponseDto?> GetMySubmissionAsync(
        Guid assignmentId, Guid studentId)
    {
        var s = await _context.Submissions
            .Include(s => s.Student).ThenInclude(st => st.User)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s =>
                s.AssignmentId == assignmentId &&
                s.StudentId == studentId);

        if (s == null) return null;

        return new SubmissionResponseDto
        {
            Id = s.Id,
            StudentName = s.Student?.User?.FullName ?? string.Empty,
            RollNumber = s.Student?.RollNumber ?? string.Empty,
            Content = s.Content,
            FilePath = s.FilePath,
            SubmittedAt = s.SubmittedAt,
            IsLate = s.IsLate,
            MarksAwarded = s.MarksAwarded,
            TeacherRemark = s.TeacherRemark,
            Feedback = s.Feedback,
            Status = s.Status.ToString(),
            AssignmentTitle = s.Assignment?.Title ?? string.Empty
        };
    }

    private async Task<SubmissionResponseDto> BuildSubmissionResponse(Guid submissionId)
    {
        var s = await _context.Submissions
            .Include(s => s.Student).ThenInclude(st => st.User)
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId)
            ?? throw new KeyNotFoundException("Submission not found.");

        return new SubmissionResponseDto
        {
            Id = s.Id,
            StudentName = s.Student?.User?.FullName ?? string.Empty,
            RollNumber = s.Student?.RollNumber ?? string.Empty,
            Content = s.Content,
            FilePath = s.FilePath,
            SubmittedAt = s.SubmittedAt,
            IsLate = s.IsLate,
            MarksAwarded = s.MarksAwarded,
            TeacherRemark = s.TeacherRemark,
            Feedback = s.Feedback,
            Status = s.Status.ToString(),
            AssignmentTitle = s.Assignment?.Title ?? string.Empty
        };
    }

    private async Task NotifyClassAssignmentAsync(Guid classId, string assignmentTitle, string dueDate)
    {
        try
        {
            var classStudents = await _context.Students
                .Include(s => s.User)
                .Where(s => s.ClassId == classId)
                .ToListAsync();

            var userIds = classStudents
                .Where(s => s.User != null)
                .Select(s => s.UserId)
                .ToList();

            await _notificationService.PushToManyAsync(
                userIds: userIds,
                title: $"New Assignment: {assignmentTitle}",
                body: $"Due: {dueDate}. Check your assignments.",
                type: "Assignment",
                actionUrl: "/student/assignments"
            );
        }
        catch { }
    }
}
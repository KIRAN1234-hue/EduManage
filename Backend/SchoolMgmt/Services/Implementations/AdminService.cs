using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Admin;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Implementations;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public AdminService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _context = context;
    }

    // ════════════════════════════════════════════════════════════════════════
    // CLASSES
    // ════════════════════════════════════════════════════════════════════════
    public async Task<ClassResponseDto> CreateClassAsync(CreateClassDto dto)
    {
        // Check duplicate
        var exists = await _unitOfWork.Classes.AnyAsync(c =>
            c.Name == dto.Name &&
            c.Section == dto.Section &&
            c.AcademicYear == dto.AcademicYear);

        if (exists)
            throw new InvalidOperationException(
                $"Class {dto.Name} {dto.Section} already exists for {dto.AcademicYear}.");

        var cls = new Class
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Section = dto.Section,
            AcademicYear = dto.AcademicYear,
            MaxStrength = dto.MaxStrength
        };

        await _unitOfWork.Classes.AddAsync(cls);
        await _unitOfWork.SaveChangesAsync();

        return new ClassResponseDto
        {
            Id = cls.Id,
            Name = cls.Name,
            Section = cls.Section,
            AcademicYear = cls.AcademicYear,
            MaxStrength = cls.MaxStrength,
            StudentCount = 0,
            ClassTeacher = string.Empty
        };
    }

    public async Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync()
    {
        return await _context.Classes
            .Include(c => c.ClassTeacher)
                .ThenInclude(t => t.User)
            .Include(c => c.Students)
            .Select(c => new ClassResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Section = c.Section,
                AcademicYear = c.AcademicYear,
                MaxStrength = c.MaxStrength,
                StudentCount = c.Students.Count,
                ClassTeacher = c.ClassTeacher != null
                    ? c.ClassTeacher.User.FullName
                    : string.Empty
            })
            .ToListAsync();
    }
    public async Task<ClassResponseDto> GetClassByIdAsync(Guid classId)
    {
        var cls = await _context.Classes
            .Include(c => c.ClassTeacher).ThenInclude(t => t.User)
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.Id == classId)
            ?? throw new KeyNotFoundException($"Class {classId} not found.");

        return new ClassResponseDto
        {
            Id = cls.Id,
            Name = cls.Name,
            Section = cls.Section,
            AcademicYear = cls.AcademicYear,
            MaxStrength = cls.MaxStrength,
            StudentCount = cls.Students?.Count ?? 0,
            ClassTeacher = cls.ClassTeacher?.User?.FullName ?? string.Empty
        };
    }
    // ════════════════════════════════════════════════════════════════════════
    // SUBJECTS
    // ════════════════════════════════════════════════════════════════════════
    public async Task<SubjectResponseDto> CreateSubjectAsync(CreateSubjectDto dto)
    {
        // Validate class exists
        var classExists = await _unitOfWork.Classes.AnyAsync(c => c.Id == dto.ClassId);
        if (!classExists)
            throw new KeyNotFoundException($"Class {dto.ClassId} not found.");

        // Check duplicate code in same class
        var codeExists = await _unitOfWork.Subjects.AnyAsync(s =>
            s.Code == dto.Code &&
            s.ClassId == dto.ClassId);

        if (codeExists)
            throw new InvalidOperationException(
                $"Subject with code {dto.Code} already exists in this class.");

        
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            MaxMarks = dto.MaxMarks,
            IsElective = dto.IsElective,
            ClassId = dto.ClassId,
            TeacherId = dto.TeacherId
        };

        await _unitOfWork.Subjects.AddAsync(subject);
        await _unitOfWork.SaveChangesAsync();

        // Get class name for response
        var cls = await _unitOfWork.Classes.GetByIdAsync(dto.ClassId);
        string teacherName = string.Empty;

        if (dto.TeacherId.HasValue)
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(dto.TeacherId.Value);
            teacherName = teacher?.User?.FullName ?? string.Empty;
        }

        return new SubjectResponseDto
        {
            Id = subject.Id,
            Name = subject.Name,
            Code = subject.Code,
            MaxMarks = subject.MaxMarks,
            IsElective = subject.IsElective,
            ClassName = cls != null ? $"{cls.Name} {cls.Section}" : string.Empty,
            TeacherName = teacherName
        };
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetAllSubjectsAsync()
    {
        return await _context.Subjects
            .Include(s => s.Class)
            .Include(s => s.Teacher).ThenInclude(t => t.User)
            .Select(s => new SubjectResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                MaxMarks = s.MaxMarks,
                IsElective = s.IsElective,
                ClassName = s.Class != null ? $"{s.Class.Name} {s.Class.Section}" : string.Empty,
                TeacherName = s.Teacher != null ? s.Teacher.User.FullName : string.Empty
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<SubjectResponseDto>> GetSubjectsByClassAsync(Guid classId)
    {
        return await _context.Subjects
            .Include(s => s.Class)
            .Include(s => s.Teacher).ThenInclude(t => t.User)
            .Where(s => s.ClassId == classId)
            .Select(s => new SubjectResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                MaxMarks = s.MaxMarks,
                IsElective = s.IsElective,
                ClassName = s.Class != null ? $"{s.Class.Name} {s.Class.Section}" : string.Empty,
                TeacherName = s.Teacher != null ? s.Teacher.User.FullName : string.Empty
            })
            .ToListAsync();
    }
    // ════════════════════════════════════════════════════════════════════════
    // TEACHERS
    // ════════════════════════════════════════════════════════════════════════
    public async Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherDto dto)
    {
        // Check email not already taken
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"Email {dto.Email} is already registered.");

        // Check employee code not duplicate
        var codeExists = await _unitOfWork.Teachers.AnyAsync(t =>
            t.EmployeeCode == dto.EmployeeCode);
        if (codeExists)
            throw new InvalidOperationException(
                $"Employee code {dto.EmployeeCode} already exists.");

        // Create ApplicationUser (no password yet — set via invite token)
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),
            NormalizedUserName = dto.Email.ToUpper(),
            EmailConfirmed = false,   // must register to confirm
            RoleType = RoleType.Teacher,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create user with random temp password (they will change via invite token)
        var tempPassword = $"Temp@{Guid.NewGuid().ToString("N")[..8]}";
        var createResult = await _userManager.CreateAsync(user, tempPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        // Assign Teacher role
        await _userManager.AddToRoleAsync(user, "Teacher");

        // Create Teacher entity
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Qualification = dto.Qualification,
            EmployeeCode = dto.EmployeeCode,
            JoiningDate = dto.JoiningDate,
            IsClassTeacher = dto.IsClassTeacher,
            ClassId = dto.ClassId
        };

        await _unitOfWork.Teachers.AddAsync(teacher);
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {

        }

        // Generate invite token — teacher uses this to set their password
        var inviteToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        return new TeacherResponseDto
        {
            Id = teacher.Id,
            FullName = user.FullName,
            Email = user.Email!,
            EmployeeCode = teacher.EmployeeCode,
            Qualification = teacher.Qualification,
            IsActive = user.IsActive,
            InviteToken = inviteToken    // Principal shares this with teacher
        };
    }

    public async Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync()
    {
        return await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.Class)
            .Select(t => new TeacherResponseDto
            {
                Id = t.Id,
                FullName = t.User.FullName,
                Email = t.User.Email ?? string.Empty,
                EmployeeCode = t.EmployeeCode,
                Qualification = t.Qualification,
                ClassName = t.Class != null ? $"{t.Class.Name} {t.Class.Section}" : null,
                IsActive = t.User.IsActive
            })
            .ToListAsync();
    }
    // ════════════════════════════════════════════════════════════════════════
    // STUDENTS
    // ════════════════════════════════════════════════════════════════════════
    public async Task<StudentResponseDto> CreateStudentAsync(CreateStudentDto dto)
    {
        // Check email not taken
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException($"Email {dto.Email} is already registered.");

        // Check roll number unique in class
        var rollExists = await _unitOfWork.Students.AnyAsync(s =>
            s.RollNumber == dto.RollNumber &&
            s.ClassId == dto.ClassId);

        if (rollExists)
            throw new InvalidOperationException(
                $"Roll number {dto.RollNumber} already exists in this class.");

        // Validate class exists
        var classExists = await _unitOfWork.Classes.AnyAsync(c => c.Id == dto.ClassId);
        if (!classExists)
            throw new KeyNotFoundException($"Class {dto.ClassId} not found.");

        // Create ApplicationUser
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),
            NormalizedUserName = dto.Email.ToUpper(),
            EmailConfirmed = false,
            RoleType = RoleType.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var tempPassword = $"Temp@{Guid.NewGuid().ToString("N")[..8]}";
        var createResult = await _userManager.CreateAsync(user, tempPassword);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Student");

        // Create Student entity
        var student = new Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ClassId = dto.ClassId,
            RollNumber = dto.RollNumber,
            DateOfBirth = dto.DateOfBirth,
            AdmissionDate = dto.AdmissionDate
        };

        await _unitOfWork.Students.AddAsync(student);
        await _unitOfWork.SaveChangesAsync();

        // Generate invite token
        var inviteToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Get class name
        var cls = await _unitOfWork.Classes.GetByIdAsync(dto.ClassId);

        return new StudentResponseDto
        {
            Id = student.Id,
            FullName = user.FullName,
            Email = user.Email!,
            RollNumber = student.RollNumber,
            ClassName = cls != null ? $"{cls.Name} {cls.Section}" : string.Empty,
            DateOfBirth = student.DateOfBirth,
            AdmissionDate = student.AdmissionDate,
            IsActive = user.IsActive,
            InviteToken = inviteToken
        };
    }

    public async Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync()
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.User.FullName,
                Email = s.User.Email ?? string.Empty,
                RollNumber = s.RollNumber,
                ClassName = s.Class != null ? $"{s.Class.Name} {s.Class.Section}" : string.Empty,
                DateOfBirth = s.DateOfBirth,
                AdmissionDate = s.AdmissionDate,
                IsActive = s.User.IsActive
            })
            .ToListAsync();
    }
    public async Task<IEnumerable<StudentResponseDto>> GetStudentsByClassAsync(Guid classId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .Where(s => s.ClassId == classId)
            .Select(s => new StudentResponseDto
            {
                Id = s.Id,
                FullName = s.User.FullName,
                Email = s.User.Email ?? string.Empty,
                RollNumber = s.RollNumber,
                ClassName = s.Class != null ? $"{s.Class.Name} {s.Class.Section}" : string.Empty,
                DateOfBirth = s.DateOfBirth,
                AdmissionDate = s.AdmissionDate,
                IsActive = s.User.IsActive
            })
            .ToListAsync();
    }
    // ════════════════════════════════════════════════════════════════════════
    // DEACTIVATE USER
    // ════════════════════════════════════════════════════════════════════════
    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
    }
    // ── Delete Class ──────────────────────────────────────────────────────────
    public async Task DeleteClassAsync(Guid classId)
    {
        var cls = await _unitOfWork.Classes.GetByIdAsync(classId)
            ?? throw new KeyNotFoundException($"Class {classId} not found.");

        // Check no students linked
        var hasStudents = await _unitOfWork.Students.AnyAsync(s => s.ClassId == classId);
        if (hasStudents)
            throw new InvalidOperationException(
                "Cannot delete class. Move or remove all students first.");

        _unitOfWork.Classes.Remove(cls);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── Delete Subject ────────────────────────────────────────────────────────
    public async Task DeleteSubjectAsync(Guid subjectId)
    {
        var subject = await _unitOfWork.Subjects.GetByIdAsync(subjectId)
            ?? throw new KeyNotFoundException($"Subject {subjectId} not found.");

        // Check no attendance records linked
        var hasAttendance = await _unitOfWork.Attendances
            .AnyAsync(a => a.SubjectId == subjectId);
        if (hasAttendance)
            throw new InvalidOperationException(
                "Cannot delete subject. It has attendance records linked.");

        // Check no marks linked
        var hasMarks = await _unitOfWork.Marks.AnyAsync(m => m.SubjectId == subjectId);
        if (hasMarks)
            throw new InvalidOperationException(
                "Cannot delete subject. It has marks records linked.");

        _unitOfWork.Subjects.Remove(subject);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── Delete Teacher ────────────────────────────────────────────────────────
    public async Task DeleteTeacherAsync(Guid teacherId)
    {
        var teacher = await _unitOfWork.Teachers.GetByIdAsync(teacherId)
            ?? throw new KeyNotFoundException($"Teacher {teacherId} not found.");

        // Unlink from class if class teacher
        if (teacher.ClassId.HasValue)
        {
            var cls = await _unitOfWork.Classes.GetByIdAsync(teacher.ClassId.Value);
            if (cls != null && cls.ClassTeacherId == teacherId)
            {
                cls.ClassTeacherId = null;
                _unitOfWork.Classes.Update(cls);
            }
        }

        // Get linked user and deactivate
        var user = await _userManager.FindByIdAsync(teacher.UserId.ToString());
        if (user != null)
        {
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
        }

        _unitOfWork.Teachers.Remove(teacher);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── Delete Student ────────────────────────────────────────────────────────
    public async Task DeleteStudentAsync(Guid studentId)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException($"Student {studentId} not found.");

        // Check no attendance records
        var hasAttendance = await _unitOfWork.Attendances
            .AnyAsync(a => a.StudentId == studentId);
        if (hasAttendance)
            throw new InvalidOperationException(
                "Cannot delete student. They have attendance records.");

        // Check no marks records
        var hasMarks = await _unitOfWork.Marks.AnyAsync(m => m.StudentId == studentId);
        if (hasMarks)
            throw new InvalidOperationException(
                "Cannot delete student. They have marks records.");

        // Deactivate user
        var user = await _userManager.FindByIdAsync(student.UserId.ToString());
        if (user != null)
        {
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
        }

        _unitOfWork.Students.Remove(student);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<TeacherResponseDto>> GetTeachersForDoubtsAsync()
    {
        return await (
            from t in _context.Teachers
            join s in _context.Subjects
            on t.Id equals s.TeacherId
            select new TeacherResponseDto
            {
                UserId = t.UserId,
                FullName = t.User.FullName,
                SubjectName = s.Name
            }).ToListAsync();
    }
}
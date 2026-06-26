using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SchoolMgmt.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Mark> Marks { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<FeeStructure> FeeStructures { get; set; }
    public DbSet<FeePayment> FeePayments { get; set; }
    public DbSet<LeaveApplication> LeaveApplications { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<NoticeAcknowledgement> NoticeAcknowledgements { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<LibraryBook> LibraryBooks { get; set; }
    public DbSet<BookIssue> BookIssues { get; set; }
    public DbSet<ExamSchedule> ExamSchedules { get; set; }
    public DbSet<Timetable> Timetables { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ───────────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.FullName)
             .IsRequired()
             .HasMaxLength(150);

            e.Property(u => u.ProfilePhotoUrl)
             .HasMaxLength(500);
        });

        // ── Student ───────────────────────────────────────────────────────────
        builder.Entity<Student>(e =>
        {
            e.HasKey(s => s.Id);

            e.Property(s => s.RollNumber)
             .IsRequired()
             .HasMaxLength(20);

            e.HasOne(s => s.User)
             .WithOne(u => u.Student)
             .HasForeignKey<Student>(s => s.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Class)
             .WithMany(c => c.Students)
             .HasForeignKey(s => s.ClassId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Parent)
             .WithMany(p => p.Students)
             .HasForeignKey(s => s.ParentId)
             .OnDelete(DeleteBehavior.SetNull);

            // RollNumber must be unique within a class
            e.HasIndex(s => new { s.ClassId, s.RollNumber })
             .IsUnique();
        });

        // ── Teacher ───────────────────────────────────────────────────────────
        builder.Entity<Teacher>(e =>
        {
            e.HasKey(t => t.Id);

            e.Property(t => t.EmployeeCode)
             .IsRequired()
             .HasMaxLength(20);

            e.HasIndex(t => t.EmployeeCode)
             .IsUnique();

            e.HasOne(t => t.User)
             .WithOne(u => u.Teacher)
             .HasForeignKey<Teacher>(t => t.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Class)
             .WithMany()
             .HasForeignKey(t => t.ClassId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Parent ────────────────────────────────────────────────────────────
        builder.Entity<Parent>(e =>
        {
            e.HasKey(p => p.Id);

            e.Property(p => p.AnnualIncome)
             .HasPrecision(12, 2);

            e.HasOne(p => p.User)
             .WithOne(u => u.Parent)
             .HasForeignKey<Parent>(p => p.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Class ─────────────────────────────────────────────────────────────
        builder.Entity<Class>(e =>
        {
            e.HasKey(c => c.Id);

            e.Property(c => c.Name)
             .IsRequired()
             .HasMaxLength(50);

            e.Property(c => c.Section)
             .IsRequired()
             .HasMaxLength(5);

            e.Property(c => c.AcademicYear)
             .IsRequired()
             .HasMaxLength(10);

            // 10th A cannot exist twice in the same academic year
            e.HasIndex(c => new { c.Name, c.Section, c.AcademicYear })
             .IsUnique();
        });

        // ── Subject ───────────────────────────────────────────────────────────
        builder.Entity<Subject>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name)
             .IsRequired()
             .HasMaxLength(100);
            e.Property(s => s.Code)
             .IsRequired()
             .HasMaxLength(20);
            e.HasOne(s => s.Class)
             .WithMany(c => c.Subjects)
             .HasForeignKey(s => s.ClassId)
             .OnDelete(DeleteBehavior.Restrict);   // ← unchanged
            e.HasOne(s => s.Teacher)
             .WithMany(t => t.Subjects)
             .HasForeignKey(s => s.TeacherId)
             .OnDelete(DeleteBehavior.SetNull)     // ← LINE 1 CHANGED: was Restrict
             .IsRequired(false);                   // ← LINE 2 ADDED: this is new
            e.HasIndex(s => new { s.ClassId, s.Code })
             .IsUnique();
        });

        // ── Attendance ────────────────────────────────────────────────────────
        builder.Entity<Attendance>(e =>
        {
            e.HasKey(a => a.Id);

            e.Property(a => a.Remarks)
             .HasMaxLength(300);

            e.HasOne(a => a.Student)
             .WithMany(s => s.Attendances)
             .HasForeignKey(a => a.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Subject)
             .WithMany(s => s.Attendances)
             .HasForeignKey(a => a.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.MarkedByTeacher)
             .WithMany(t => t.MarkedAttendances)
             .HasForeignKey(a => a.MarkedByTeacherId)
             .OnDelete(DeleteBehavior.Restrict)
             .IsRequired(false);

            // One attendance record per student per subject per day
            e.HasIndex(a => new { a.StudentId, a.SubjectId, a.Date })
             .IsUnique();
        });

        // ── Mark ──────────────────────────────────────────────────────────────
        builder.Entity<Mark>(e =>
        {
            e.HasKey(m => m.Id);

            e.Property(m => m.MarksObtained)
             .HasPrecision(5, 2)
             .IsRequired();

            e.Property(m => m.Grade)
             .IsRequired()
             .HasMaxLength(5);

            e.HasOne(m => m.Student)
             .WithMany(s => s.Marks)
             .HasForeignKey(m => m.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Subject)
             .WithMany(s => s.Marks)
             .HasForeignKey(m => m.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.EnteredByTeacher)
             .WithMany(t => t.EnteredMarks)
             .HasForeignKey(m => m.EnteredByTeacherId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Assignment ────────────────────────────────────────────────────────
        builder.Entity<Assignment>(e =>
        {
            e.HasKey(a => a.Id);

            e.Property(a => a.Title)
             .IsRequired()
             .HasMaxLength(200);

            e.Property(a => a.FilePath)
             .HasMaxLength(500);

            e.HasOne(a => a.Subject)
             .WithMany(s => s.Assignments)
             .HasForeignKey(a => a.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Class)
             .WithMany(c => c.Assignments)
             .HasForeignKey(a => a.ClassId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Teacher)
             .WithMany(t => t.Assignments)
             .HasForeignKey(a => a.TeacherId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Submission ────────────────────────────────────────────────────────
        builder.Entity<Submission>(e =>
        {
            e.HasKey(s => s.Id);

            e.Property(s => s.FilePath)
             .IsRequired()
             .HasMaxLength(500);

            e.HasOne(s => s.Assignment)
             .WithMany(a => a.Submissions)
             .HasForeignKey(s => s.AssignmentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Student)
             .WithMany(st => st.Submissions)
             .HasForeignKey(s => s.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            // One student can only submit once per assignment
            e.HasIndex(s => new { s.AssignmentId, s.StudentId })
             .IsUnique();
        });

        // ── Message ───────────────────────────────────────────────────────────
        builder.Entity<Message>(e =>
{
    e.HasKey(m => m.Id);
    e.HasOne(m => m.Sender)
     .WithMany()
     .HasForeignKey(m => m.SenderId)
     .OnDelete(DeleteBehavior.Restrict);
    e.HasOne(m => m.Receiver)
     .WithMany()
     .HasForeignKey(m => m.ReceiverId)
     .OnDelete(DeleteBehavior.Restrict);
    e.HasOne(m => m.ParentMessage)
     .WithMany(m => m.Replies)
     .HasForeignKey(m => m.ParentMessageId)
     .OnDelete(DeleteBehavior.Restrict);
});

        // ── FeeStructure ──────────────────────────────────────────────────────
        builder.Entity<FeeStructure>(e =>
        {
            e.HasKey(f => f.Id);

            e.Property(f => f.Amount)
             .HasPrecision(10, 2)
             .IsRequired();

            e.Property(f => f.TermName)
             .IsRequired()
             .HasMaxLength(20);

            e.Property(f => f.AcademicYear)
             .IsRequired()
             .HasMaxLength(10);

            e.HasOne(f => f.Class)
             .WithMany(c => c.FeeStructures)
             .HasForeignKey(f => f.ClassId)
             .OnDelete(DeleteBehavior.Restrict);

            // One fee structure per class per term per year
            e.HasIndex(f => new { f.ClassId, f.TermName, f.AcademicYear })
             .IsUnique();
        });

        // ── FeePayment ────────────────────────────────────────────────────────
        builder.Entity<FeePayment>(e =>
        {
            e.HasKey(f => f.Id);

            e.Property(f => f.Amount)
             .HasPrecision(10, 2)
             .IsRequired();

            e.Property(f => f.DiscountAmount)
             .HasPrecision(10, 2);

            e.Property(f => f.ReceiptUrl)
             .HasMaxLength(500);

            e.HasOne(f => f.Student)
             .WithMany(s => s.FeePayments)
             .HasForeignKey(f => f.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(f => f.FeeStructure)
             .WithMany(fs => fs.FeePayments)
             .HasForeignKey(f => f.FeeStructureId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(f => f.RecordedBy)
 .WithMany()
 .HasForeignKey(f => f.RecordedByUserId)
 .OnDelete(DeleteBehavior.NoAction);
        });

        //Fee Structure
        builder.Entity<FeeStructure>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Amount).HasPrecision(10, 2);
            e.HasOne(f => f.Class)
             .WithMany()
             .HasForeignKey(f => f.ClassId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LeaveApplication ──────────────────────────────────────────────────
        builder.Entity<LeaveApplication>(e =>
        {
            e.HasKey(l => l.Id);

            e.Property(l => l.Reason)
             .IsRequired();

            e.HasOne(l => l.Student)
             .WithMany(s => s.LeaveApplications)
             .HasForeignKey(l => l.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.ApprovedByTeacher)
             .WithMany(t => t.ApprovedLeaves)
             .HasForeignKey(l => l.ApprovedByTeacherId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(l => l.User)
     .WithMany()
     .HasForeignKey(l => l.UserId)
     .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Complaint ─────────────────────────────────────────────────────────
        builder.Entity<Complaint>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.SubmittedBy)
             .WithMany()
             .HasForeignKey(c => c.SubmittedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.AssignedTo)
             .WithMany()
             .HasForeignKey(c => c.AssignedToUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── Notice ────────────────────────────────────────────────────────────
        builder.Entity<Notice>(e =>
        {
            e.HasKey(n => n.Id);

            e.Property(n => n.Title)
             .IsRequired()
             .HasMaxLength(200);

            e.Property(n => n.AttachmentUrl)
             .HasMaxLength(500);

            e.HasOne(n => n.CreatedBy)
             .WithMany()
             .HasForeignKey(n => n.PostedByUserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── NoticeAcknowledgement ─────────────────────────────────────────────
        builder.Entity<NoticeAcknowledgement>(e =>
        {
            e.HasKey(na => na.Id);

            e.HasOne(na => na.Notice)
             .WithMany(n => n.Acknowledgements)
             .HasForeignKey(na => na.NoticeId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(na => na.User)
             .WithMany()
             .HasForeignKey(na => na.UserId)
             .OnDelete(DeleteBehavior.NoAction);

            // One user can only acknowledge a notice once
            e.HasIndex(na => new { na.NoticeId, na.UserId })
             .IsUnique();
        });

        // ── AuditLog ──────────────────────────────────────────────────────────
        builder.Entity<AuditLog>(e =>
        {
            e.HasKey(a => a.Id);

            e.Property(a => a.Action)
             .IsRequired()
             .HasMaxLength(50);

            e.Property(a => a.EntityName)
             .IsRequired()
             .HasMaxLength(100);

            e.Property(a => a.EntityId)
             .IsRequired()
             .HasMaxLength(100);

            e.Property(a => a.OldValues)
             .HasColumnType("nvarchar(max)");

            e.Property(a => a.NewValues)
             .HasColumnType("nvarchar(max)");

            e.HasOne(a => a.User)
             .WithMany()
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── LibraryBook ───────────────────────────────────────────────────────
        builder.Entity<LibraryBook>(e =>
        {
            e.HasKey(b => b.Id);

            e.Property(b => b.Title)
             .IsRequired()
             .HasMaxLength(200);

            e.Property(b => b.Author)
             .IsRequired()
             .HasMaxLength(150);

            e.Property(b => b.ISBN)
             .IsRequired()
             .HasMaxLength(20);

            e.HasIndex(b => b.ISBN)
             .IsUnique();
        });

        // ── BookIssue ─────────────────────────────────────────────────────────
        builder.Entity<BookIssue>(e =>
        {
            e.HasKey(bi => bi.Id);

            e.Property(bi => bi.FineAmount)
             .HasPrecision(8, 2);

            e.HasOne(bi => bi.Book)
             .WithMany(b => b.BookIssues)
             .HasForeignKey(bi => bi.BookId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(bi => bi.Student)
             .WithMany(s => s.BookIssues)
             .HasForeignKey(bi => bi.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(bi => bi.IssuedBy)
             .WithMany()
             .HasForeignKey(bi => bi.IssuedByUserId)
             .OnDelete(DeleteBehavior.NoAction);
        });

        // ── ExamSchedule ──────────────────────────────────────────────────────
        builder.Entity<ExamSchedule>(e =>
        {
            e.HasKey(es => es.Id);

            e.Property(es => es.RoomNumber)
             .HasMaxLength(20);

            e.HasOne(es => es.Subject)
             .WithMany(s => s.ExamSchedules)
             .HasForeignKey(es => es.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(es => es.Class)
             .WithMany(c => c.ExamSchedules)
             .HasForeignKey(es => es.ClassId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(es => es.InvigilatorTeacher)
             .WithMany(t => t.InvigilatedExams)
             .HasForeignKey(es => es.InvigilatorTeacherId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Timetable ─────────────────────────────────────────────────────────
        builder.Entity<Timetable>(e =>
        {
            e.HasKey(t => t.Id);

            e.Property(t => t.AcademicYear)
             .IsRequired()
             .HasMaxLength(10);

            e.HasOne(t => t.Class)
             .WithMany(c => c.Timetables)
             .HasForeignKey(t => t.ClassId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Subject)
             .WithMany(s => s.Timetables)
             .HasForeignKey(t => t.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Teacher)
             .WithMany(tc => tc.TimetableSlots)
             .HasForeignKey(t => t.TeacherId)
             .OnDelete(DeleteBehavior.Restrict);

            // A class cannot have two subjects in the same period on the same day
            e.HasIndex(t => new { t.ClassId, t.DayOfWeek, t.PeriodNumber, t.AcademicYear })
             .IsUnique();
        });
        // ── RefreshToken ──────────────────────────────────────────────────────────
        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);

            e.Property(rt => rt.Token)
             .IsRequired()
             .HasMaxLength(500);

            // Index on Token for fast lookup during validation
            e.HasIndex(rt => rt.Token)
             .IsUnique();

            e.HasOne(rt => rt.User)
             .WithMany()
             .HasForeignKey(rt => rt.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        // ── Notification ──────────────────────────────────────────────────────────
        builder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).IsRequired().HasMaxLength(200);
            e.Property(n => n.Body).IsRequired().HasMaxLength(1000);
            e.HasOne(n => n.User)
             .WithMany()
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
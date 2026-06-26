namespace SchoolMgmt.Services;

public static class EmailTemplates
{
    public static string AttendanceAlert(
        string studentName, string parentName,
        double percentage, int belowSubjects) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
          <div style="background:#1E3A8A;padding:20px;border-radius:8px 8px 0 0">
            <h2 style="color:white;margin:0">EduManage — Attendance Alert</h2>
          </div>
          <div style="padding:24px;background:#F8FAFC">
            <p>Dear <strong>{parentName}</strong>,</p>
            <p>This is to inform you that your child <strong>{studentName}</strong>
               has an overall attendance of <strong
               style="color:#DC2626">{percentage:F1}%</strong>,
               which is below the required 75% threshold.</p>
            <p><strong>{belowSubjects}</strong> subject(s) are below the minimum requirement.</p>
            <p>Please ensure regular attendance to avoid academic consequences.</p>
            <hr style="border:1px solid #E2E8F0"/>
            <p style="color:#64748B;font-size:12px">EduManage School Management System</p>
          </div>
        </div>
        """;

    public static string ExamReminder(
        string studentName, string subjectName,
        string examDate, string roomNumber) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
          <div style="background:#7C3AED;padding:20px;border-radius:8px 8px 0 0">
            <h2 style="color:white;margin:0">EduManage — Exam Reminder</h2>
          </div>
          <div style="padding:24px;background:#F8FAFC">
            <p>Dear <strong>{studentName}</strong>,</p>
            <p>This is a reminder that your <strong>{subjectName}</strong> exam
               is scheduled for <strong>{examDate}</strong>.</p>
            <p>Room: <strong>{roomNumber}</strong></p>
            <p>Best of luck! Prepare well.</p>
            <hr style="border:1px solid #E2E8F0"/>
            <p style="color:#64748B;font-size:12px">EduManage School Management System</p>
          </div>
        </div>
        """;

    public static string LibraryReturnReminder(
        string studentName, string bookTitle,
        string dueDate) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
          <div style="background:#16A34A;padding:20px;border-radius:8px 8px 0 0">
            <h2 style="color:white;margin:0">EduManage — Library Return Reminder</h2>
          </div>
          <div style="padding:24px;background:#F8FAFC">
            <p>Dear <strong>{studentName}</strong>,</p>
            <p>Please return the book <strong>"{bookTitle}"</strong>
               by <strong>{dueDate}</strong> to avoid a fine.</p>
            <p>Fine rate: ₹2 per day after due date.</p>
            <hr style="border:1px solid #E2E8F0"/>
            <p style="color:#64748B;font-size:12px">EduManage School Management System</p>
          </div>
        </div>
        """;

    public static string LeaveStatusEmail(
        string studentName, string parentName,
        string status, string? remark) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
          <div style="background:{(status == "Approved" ? "#16A34A" : "#DC2626")};
               padding:20px;border-radius:8px 8px 0 0">
            <h2 style="color:white;margin:0">Leave Application {status}</h2>
          </div>
          <div style="padding:24px;background:#F8FAFC">
            <p>Dear <strong>{parentName}</strong>,</p>
            <p>The leave application for <strong>{studentName}</strong>
               has been <strong>{status.ToLower()}</strong>.</p>
            {(remark != null ? $"<p>Remark: <em>{remark}</em></p>" : "")}
            <hr style="border:1px solid #E2E8F0"/>
            <p style="color:#64748B;font-size:12px">EduManage School Management System</p>
          </div>
        </div>
        """;

    public static string FeeReminder(
        string studentName, string parentName,
        string termName, decimal amount, string dueDate) => $"""
        <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
          <div style="background:#D97706;padding:20px;border-radius:8px 8px 0 0">
            <h2 style="color:white;margin:0">EduManage — Fee Reminder</h2>
          </div>
          <div style="padding:24px;background:#F8FAFC">
            <p>Dear <strong>{parentName}</strong>,</p>
            <p>This is a reminder that the <strong>{termName}</strong> fee of
               <strong>₹{amount:N0}</strong> for <strong>{studentName}</strong>
               is due on <strong>{dueDate}</strong>.</p>
            <p>Please make the payment before the due date to avoid late fees.</p>
            <hr style="border:1px solid #E2E8F0"/>
            <p style="color:#64748B;font-size:12px">EduManage School Management System</p>
          </div>
        </div>
        """;
}
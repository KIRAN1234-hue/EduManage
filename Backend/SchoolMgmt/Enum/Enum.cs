namespace SchoolMgmt.Enums;

public enum RoleType
{
    Principal = 1,
    Teacher = 2,
    Student = 3,
    Parent = 4
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    OnLeave = 4
}

public enum ExamType
{
    UnitTest = 1,
    MidTerm = 2,
    Final = 3,
    Practical = 4
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum ComplaintStatus
{
    Open = 1,
    InReview = 2,
    Resolved = 3
}

public enum ComplaintCategory
{
    Academic = 1,
    Infrastructure = 2,
    TeacherConduct = 3,
    Administration = 4,
    Other = 5
}

public enum MessageType
{
    Doubt = 1,
    Chat = 2,
    Broadcast = 3
}

public enum FeePaymentStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}

public enum SubjectType
{
    Theory = 1,
    Practical = 2
}

public enum SchoolDay
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}

public enum NoticeTargetRole
{
    All = 1,
    Students = 2,
    Parents = 3,
    Teachers = 4
}

public enum SubmissionStatus
{
    Submitted = 1,
    Reviewed = 2,
    Late = 2,
    Graded = 3,
    Returned = 4
}

public enum PaymentStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Partial = 4
}

public enum IssueStatus
{
    Issued = 1,
    Returned = 2,
    Overdue = 3,
    Lost = 4
}
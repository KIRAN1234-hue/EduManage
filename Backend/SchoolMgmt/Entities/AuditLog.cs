namespace SchoolMgmt.Entities;

public class AuditLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Action { get; set; } = string.Empty;      // Create, Update, Delete
    public string EntityName { get; set; } = string.Empty;  // e.g. Mark, Attendance
    public string EntityId { get; set; } = string.Empty;

    public string? OldValues { get; set; }                  // JSON string
    public string NewValues { get; set; } = string.Empty;   // JSON string

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }
}
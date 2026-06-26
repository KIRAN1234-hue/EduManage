namespace SchoolMgmt.Entities;

public class NoticeAcknowledgement
{
    public Guid Id { get; set; }

    public Guid NoticeId { get; set; }
    public Notice Notice { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public DateTime AcknowledgedAt { get; set; } = DateTime.UtcNow;
}
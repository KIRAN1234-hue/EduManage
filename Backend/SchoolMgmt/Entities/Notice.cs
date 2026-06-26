using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Notice
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoticeTargetRole TargetRole { get; set; }
    public bool IsArchived { get; set; } = false;
    public string? AttachmentUrl { get; set; }        // Azure Blob URL

    public Guid PostedByUserId { get; set; }
    public ApplicationUser CreatedBy { get; set; } = null!;

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public ICollection<NoticeAcknowledgement> Acknowledgements { get; set; }
        = new List<NoticeAcknowledgement>();
}
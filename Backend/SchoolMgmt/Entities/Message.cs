using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Message
{
    public Guid Id { get; set; }

    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;

    public Guid ReceiverId { get; set; }
    public ApplicationUser Receiver { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public MessageType MessageType { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Groups messages into a conversation thread
    public Guid? ThreadId { get; set; }
    public Message? Thread { get; set; }

    //newly added
    public ICollection<Message> Replies { get; set; } = new List<Message>();
    public Guid? ParentMessageId { get; set; }
    public Message? ParentMessage { get; set; }
}   
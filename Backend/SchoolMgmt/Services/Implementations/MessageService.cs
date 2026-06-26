using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Message;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public MessageService(
        IUnitOfWork unitOfWork,
        AppDbContext context,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<MessageResponseDto> SendMessageAsync(
        SendMessageDto dto, Guid senderUserId)
    {
        // Use _context.Users — NOT _unitOfWork.Users
        var sender = await _context.Users.FindAsync(senderUserId)
            ?? throw new KeyNotFoundException("Sender not found.");

        var receiver = await _context.Users.FindAsync(dto.ReceiverId)
        ?? throw new KeyNotFoundException("Receiver not found.");

        // Parse MessageType enum from string
        var messageType = Enum.TryParse<MessageType>(dto.MessageType, out var mt)
            ? mt : MessageType.Doubt;

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SenderId = senderUserId,
            ReceiverId = dto.ReceiverId,
            Content = dto.Content,
            MessageType = messageType,           // enum — no Subject
            SentAt = DateTime.UtcNow,
            IsRead = false,
            ParentMessageId = dto.ParentMessageId
        };

        await _unitOfWork.Messages.AddAsync(message);
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch(Exception ex)
        {

        }

        // Push notification to receiver
        await _notificationService.PushAsync(
            userId: dto.ReceiverId,
            title: $"New message from {sender.FullName}",
            body: dto.Content.Length > 100
                             ? dto.Content[..100] + "..."
                             : dto.Content,
            type: "Message",
            relatedEntityId: message.Id.ToString()
        );

        return await BuildMessageResponse(message.Id);
    }

    public async Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(Guid userId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId)
                     && m.ParentMessageId == null)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        var conversations = messages
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(m => m.SentAt).First();
                var otherUser = latest.SenderId == userId ? latest.Receiver : latest.Sender;
                return new ConversationDto
                {
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.FullName,
                    OtherUserRole = string.Empty,
                    LastMessage = latest.Content.Length > 80
                        ? latest.Content[..80] + "..." : latest.Content,
                    LastMessageAt = latest.SentAt,
                    UnreadCount = g.Count(m => m.ReceiverId == userId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

        return conversations;
    }

    public async Task<IEnumerable<MessageResponseDto>> GetConversationAsync(
        Guid userId, Guid otherUserId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m =>
                ((m.SenderId == userId && m.ReceiverId == otherUserId) ||
                 (m.SenderId == otherUserId && m.ReceiverId == userId))
                && m.ParentMessageId == null)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        var unread = messages
            .Where(m => m.ReceiverId == userId && !m.IsRead).ToList();

        foreach (var m in unread)
        {
            m.IsRead = true;
            _unitOfWork.Messages.Update(m);
        }
        if (unread.Any()) await _unitOfWork.SaveChangesAsync();

        return messages.Select(MapMessage);
    }

    public async Task<IEnumerable<MessageResponseDto>> GetInboxAsync(Guid userId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ReceiverId == userId && m.ParentMessageId == null)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .ToListAsync();

        return messages.Select(MapMessage);
    }

    public async Task<IEnumerable<MessageResponseDto>> GetSentAsync(Guid userId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.SenderId == userId && m.ParentMessageId == null)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .ToListAsync();

        return messages.Select(MapMessage);
    }

    public async Task<IEnumerable<MessageResponseDto>> GetThreadAsync(
        Guid parentMessageId)
    {
        var replies = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => m.ParentMessageId == parentMessageId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return replies.Select(MapMessage);
    }

    public async Task MarkAsReadAsync(Guid messageId, Guid userId)
    {
        var message = await _unitOfWork.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId);

        if (message == null) return;

        message.IsRead = true;
        _unitOfWork.Messages.Update(message);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }

    public async Task DeleteMessageAsync(Guid messageId, Guid userId)
    {
        var message = await _unitOfWork.Messages.FirstOrDefaultAsync(
            m => m.Id == messageId &&
                (m.SenderId == userId || m.ReceiverId == userId));

        if (message == null)
            throw new KeyNotFoundException("Message not found.");

        _unitOfWork.Messages.Remove(message);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<MessageResponseDto> BuildMessageResponse(Guid messageId)
    {
        var m = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Replies)
            .FirstOrDefaultAsync(m => m.Id == messageId)
            ?? throw new KeyNotFoundException("Message not found.");

        return MapMessage(m);
    }

    private static MessageResponseDto MapMessage(Message m) => new()
    {
        Id = m.Id,
        SenderName = m.Sender?.FullName ?? string.Empty,
        ReceiverName = m.Receiver?.FullName ?? string.Empty,
        SenderId = m.SenderId,
        ReceiverId = m.ReceiverId,
        Content = m.Content,
        MessageType = m.MessageType.ToString(),   // enum → string ✅
        SentAt = m.SentAt,
        IsRead = m.IsRead,
        ParentMessageId = m.ParentMessageId,
        ReplyCount = m.Replies?.Count ?? 0
    };
}
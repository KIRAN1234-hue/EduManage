using SchoolMgmt.DTOs.Message;

namespace SchoolMgmt.Services.Interfaces;

public interface IMessageService
{
    Task<MessageResponseDto> SendMessageAsync(SendMessageDto dto, Guid senderUserId);
    Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(Guid userId);
    Task<IEnumerable<MessageResponseDto>> GetConversationAsync(Guid userId, Guid otherUserId);
    Task<IEnumerable<MessageResponseDto>> GetInboxAsync(Guid userId);
    Task<IEnumerable<MessageResponseDto>> GetSentAsync(Guid userId);
    Task<IEnumerable<MessageResponseDto>> GetThreadAsync(Guid parentMessageId);
    Task MarkAsReadAsync(Guid messageId, Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task DeleteMessageAsync(Guid messageId, Guid userId);
}
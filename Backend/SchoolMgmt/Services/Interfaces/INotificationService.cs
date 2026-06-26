using SchoolMgmt.DTOs.Notification;

namespace SchoolMgmt.Services.Interfaces;

public interface INotificationService
{
    // Called by other services to push notifications
    Task PushAsync(
        Guid userId,
        string title,
        string body,
        string type,
        string? actionUrl = null,
        string? relatedEntityId = null);

    Task PushToManyAsync(
        IEnumerable<Guid> userIds,
        string title,
        string body,
        string type,
        string? actionUrl = null,
        string? relatedEntityId = null);

    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteNotificationAsync(Guid notificationId, Guid userId);
}
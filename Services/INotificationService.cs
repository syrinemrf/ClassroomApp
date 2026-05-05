using ClassroomApp.Models.Enums;

namespace ClassroomApp.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid userId, string title, string content, NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null);
        Task NotifyClassroomAsync(Guid classroomId, string title, string content, NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null);
        Task<List<Models.Entities.Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<int> GetUserNotificationCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}

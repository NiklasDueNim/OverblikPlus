using OverblikPlus.Models.Dashboard;

namespace OverblikPlus.Services.Interfaces;

public interface INotificationService
{
    Task<List<Notification>> GetNotificationsAsync(string userId);
    Task<Notification?> GetNotificationAsync(string notificationId);
    Task<bool> CreateNotificationAsync(Notification notification);
    Task<bool> MarkAsReadAsync(string notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(string notificationId);
    Task<bool> DeleteAllNotificationsAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<NotificationSettings> GetSettingsAsync(string userId);
    Task<bool> UpdateSettingsAsync(NotificationSettings settings);
    Task<bool> SendNotificationAsync(string userId, string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal, string? actionUrl = null, string? actionText = null);
}


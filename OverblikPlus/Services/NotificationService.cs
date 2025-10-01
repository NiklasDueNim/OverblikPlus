using OverblikPlus.Models.Dashboard;
using OverblikPlus.Services.Interfaces;
using System.Text.Json;

namespace OverblikPlus.Services;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public NotificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["NotificationService__BaseUrl"] ?? "http://localhost:5002";
    }

    public async Task<List<Notification>> GetNotificationsAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/notifications/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<List<Notification>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Data ?? new List<Notification>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting notifications: {ex.Message}");
        }
        return new List<Notification>();
    }

    public async Task<Notification?> GetNotificationAsync(string notificationId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/notifications/single/{notificationId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<Notification>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting notification: {ex.Message}");
        }
        return null;
    }

    public async Task<bool> CreateNotificationAsync(Notification notification)
    {
        try
        {
            var json = JsonSerializer.Serialize(notification);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/notifications", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating notification: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> MarkAsReadAsync(string notificationId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/notifications/{notificationId}/read", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking notification as read: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/notifications/{userId}/read-all", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking all notifications as read: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteNotificationAsync(string notificationId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/notifications/{notificationId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting notification: {ex.Message}");
        }
        return false;
    }

    public async Task<bool> DeleteAllNotificationsAsync(string userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/notifications/{userId}/all");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting all notifications: {ex.Message}");
            return false;
        }
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/notifications/{userId}/unread-count");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<int>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Data ?? 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting unread count: {ex.Message}");
        }
        return 0;
    }

    public async Task<NotificationSettings> GetSettingsAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/notifications/{userId}/settings");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResult<NotificationSettings>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.Data ?? GetDefaultSettings(userId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting notification settings: {ex.Message}");
        }
        return GetDefaultSettings(userId);
    }

    public async Task<bool> UpdateSettingsAsync(NotificationSettings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{_baseUrl}/api/notifications/settings", content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating notification settings: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendNotificationAsync(string userId, string title, string message, NotificationType type = NotificationType.Info, NotificationPriority priority = NotificationPriority.Normal, string? actionUrl = null, string? actionText = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Priority = priority,
            ActionUrl = actionUrl,
            ActionText = actionText,
            ExpiresAt = DateTime.Now.AddDays(7) // Default 7 days expiration
        };

        return await CreateNotificationAsync(notification);
    }

    private NotificationSettings GetDefaultSettings(string userId)
    {
        return new NotificationSettings
        {
            UserId = userId,
            EnablePushNotifications = true,
            EnableEmailNotifications = false,
            EnableSound = true,
            EnabledTypes = new List<NotificationType>
            {
                NotificationType.Task,
                NotificationType.Shift,
                NotificationType.Activity,
                NotificationType.Message,
                NotificationType.System
            },
            MaxNotifications = 50,
            AutoHideDelay = 5000
        };
    }
}


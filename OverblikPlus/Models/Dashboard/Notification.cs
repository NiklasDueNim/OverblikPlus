namespace OverblikPlus.Models.Dashboard;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ExpiresAt { get; set; }
    public bool IsRead { get; set; } = false;
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Task,
    Shift,
    Activity,
    Message,
    System
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public class NotificationSettings
{
    public string UserId { get; set; } = string.Empty;
    public bool EnablePushNotifications { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = false;
    public bool EnableSound { get; set; } = true;
    public List<NotificationType> EnabledTypes { get; set; } = new();
    public int MaxNotifications { get; set; } = 50;
    public int AutoHideDelay { get; set; } = 5000; // milliseconds
}


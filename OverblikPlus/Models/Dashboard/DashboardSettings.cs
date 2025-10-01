namespace OverblikPlus.Models.Dashboard;

public class DashboardSettings
{
    public string UserId { get; set; } = string.Empty;
    public List<WidgetInfo> Widgets { get; set; } = new();
    public string Theme { get; set; } = "light";
    public bool ShowWelcomeMessage { get; set; } = true;
    public int WelcomeMessageDuration { get; set; } = 5000; // milliseconds
    public bool EnableNotifications { get; set; } = true;
    public List<string> NotificationTypes { get; set; } = new() { "tasks", "shifts", "activities", "messages" };
    public string Layout { get; set; } = "grid"; // grid, list, compact
}

public enum DashboardTheme
{
    Light,
    Dark,
    Blue,
    Green
}

public enum DashboardLayout
{
    Grid,
    List,
    Compact
}


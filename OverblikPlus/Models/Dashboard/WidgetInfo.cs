namespace OverblikPlus.Models.Dashboard;

public class WidgetInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Position { get; set; }
    public int Size { get; set; } = 1; // 1 = small, 2 = medium, 3 = large
    public bool IsVisible { get; set; } = true;
    public Dictionary<string, object> Settings { get; set; } = new();
    public string Role { get; set; } = "All"; // All, Beboer, Medarbejder, Admin
}

public enum WidgetType
{
    Tasks,
    Shifts,
    Activities,
    Messages,
    Mood,
    Budget,
    Announcements,
    QuickActions,
    Weather,
    Calendar
}

public enum WidgetSize
{
    Small = 1,
    Medium = 2,
    Large = 3
}


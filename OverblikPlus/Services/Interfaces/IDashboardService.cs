using OverblikPlus.Models.Dashboard;

namespace OverblikPlus.Services.Interfaces;

public interface IDashboardService
{
    /// <summary>Loads the saved widget layout for a user, or null if none is stored.</summary>
    Task<List<WidgetInfo>?> LoadLayoutAsync(string userId);

    /// <summary>Persists the user's current widget layout (order, visibility, size).</summary>
    Task SaveLayoutAsync(string userId, List<WidgetInfo> widgets);

    /// <summary>Removes the saved layout so defaults are used again.</summary>
    Task ClearLayoutAsync(string userId);
}

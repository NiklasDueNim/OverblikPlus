using Blazored.LocalStorage;
using OverblikPlus.Models.Dashboard;
using OverblikPlus.Services.Interfaces;

namespace OverblikPlus.Services;

// Persists each user's dashboard layout in browser localStorage. This keeps the
// "choose your own widgets" customisation across reloads without requiring a backend.
public class DashboardService : IDashboardService
{
    private const string KeyPrefix = "op_dashboard_";
    private readonly ILocalStorageService _localStorage;

    public DashboardService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    private static string Key(string userId) => $"{KeyPrefix}{userId}";

    public async Task<List<WidgetInfo>?> LoadLayoutAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return null;

        try
        {
            return await _localStorage.GetItemAsync<List<WidgetInfo>>(Key(userId));
        }
        catch
        {
            // Corrupt/unavailable storage: fall back to defaults rather than crash.
            return null;
        }
    }

    public async Task SaveLayoutAsync(string userId, List<WidgetInfo> widgets)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        await _localStorage.SetItemAsync(Key(userId), widgets);
    }

    public async Task ClearLayoutAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return;

        await _localStorage.RemoveItemAsync(Key(userId));
    }
}

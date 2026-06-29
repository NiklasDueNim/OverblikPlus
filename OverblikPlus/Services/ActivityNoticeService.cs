using Microsoft.AspNetCore.SignalR.Client;

namespace OverblikPlus.Services;

// Shared, app-wide SignalR connection for quick activity notices that staff broadcast
// to residents in real time (e.g. "Kortspil i fællestuen om 5 min").
public class ActivityNoticeService : IAsyncDisposable
{
    private readonly IConfiguration _config;
    private HubConnection? _hub;

    public event Action<string, string>? NoticeReceived; // (sender, message)

    public ActivityNoticeService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnsureConnectedAsync()
    {
        if (_hub != null) return;

        var userApiBaseUrl = _config["USER_API_BASE_URL"];
        if (string.IsNullOrEmpty(userApiBaseUrl)) return;

        _hub = new HubConnectionBuilder()
            .WithUrl($"{userApiBaseUrl}/chatHub")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<string, string>("ReceiveActivityNotice", (sender, message) =>
            NoticeReceived?.Invoke(sender, message));

        try
        {
            await _hub.StartAsync();
        }
        catch
        {
            // Hub unavailable (e.g. backend down): notices just won't arrive in real time.
        }
    }

    public async Task<bool> SendAsync(string sender, string message)
    {
        await EnsureConnectedAsync();
        if (_hub is { State: HubConnectionState.Connected })
        {
            await _hub.SendAsync("SendActivityNotice", sender, message);
            return true;
        }
        return false;
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub != null)
        {
            await _hub.DisposeAsync();
        }
    }
}

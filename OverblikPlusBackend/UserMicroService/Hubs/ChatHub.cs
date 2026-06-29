using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace UserMicroService.Hubs;

public class ChatHub : Hub
{
    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // Staff broadcasts a quick activity notice to everyone else currently connected
    // (e.g. "Kortspil i fællestuen om 5 min"). The sender doesn't receive it back.
    public Task SendActivityNotice(string sender, string message)
    {
        return Clients.Others.SendAsync("ReceiveActivityNotice", sender, message);
    }
}
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace H4G_Project
{
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}

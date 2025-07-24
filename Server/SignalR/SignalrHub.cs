using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.SignalR
{
    public class SignalrHub :Hub
    {
        public async Task ReloadMenuAsync()
        {
            await Clients.All.SendAsync("ReloadMenu");
        }
    }
}

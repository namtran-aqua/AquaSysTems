using Microsoft.AspNetCore.SignalR;

namespace AquaSolution.Server.SignalR
{
    public class SignalrHub :Hub
    {
        #region Menu và Role Admin
        public async Task ReloadMenuAsync()
        {
            await Clients.All.SendAsync("ReloadMenu");
        }
        #endregion
        #region RequestClinic
        public async Task ChangeStatusRequestClinic()
        {
            await Clients.All.SendAsync("ChangeStatusRequestClinic");
        }
        #endregion

    }
}

using AquaSolution.Server.Services.Common.UserConnectionManager;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AquaSolution.Server.SignalR
{
    public class SignalrHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        private readonly IUserConnectionManager _connectionManager;
        public SignalrHub(IUserConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        #region Menu và Role Admin
        public async Task ReloadMenuAsync()
        {
            await Clients.All.SendAsync("ReloadMenu");
        }
        public async Task ReloadIsLockSystem(string pageId)
        {
            await Clients.Group(pageId)
                .SendAsync("IsLockSystemChanged", pageId);
        }
        #endregion
        #region RequestClinic
        public async Task LoadRequestClinic()
        {
            await Clients.All.SendAsync("LoadRequestClinic");
        }
        #endregion
        #region RequestSuport
        public async Task ChangeStatusRequestSuport()
        {
            await Clients.All.SendAsync("LoadRequestSuport");
        }
        #endregion
        #region KPI
        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
            if (!string.IsNullOrEmpty(userId))
                _connections[userId] = Context.ConnectionId;

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(user.Key))
                _connections.TryRemove(user.Key, out _);

            return base.OnDisconnectedAsync(exception);
        }
        public async Task ReloadKPIForUserApproval()
        {
            await Clients.All.SendAsync("ReloadKPIForUserApproval");
        }
        #endregion
        #region ScanReport
        public async Task LoadScanReport()
        {
            await Clients.All.SendAsync("LoadScanReport");
        }
        #endregion
        #region RollReport
        public async Task LoadRollReport()
        {
            await Clients.All.SendAsync("LoadRollReport");
        }
        #endregion
    }
}

using AquaSolution.Shared.Menus;
using AquaSolution.Shared.UserManagements;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;

namespace AquaSolution.Client.Shared
{
    public partial class MainLayout
    {
        private ClaimsPrincipal user;
        private UserDto? userDto { get; set; }
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; }
        private HubConnection? _hubConnection;
        private CurrentUserInfo? CurrentUser;
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        List<MenuDto>? menu = new();
        private System.Timers.Timer? _timer;
        protected override async Task OnInitializedAsync()
        {
            _timer = new System.Timers.Timer(3600000);
            _timer.Elapsed += async (_, _) => await CheckTokenExpired();
            _timer.Start();

            _hubConnection = new HubConnectionBuilder()
                    .WithUrl(NavigationManager.ToAbsoluteUri(NavigationManager.BaseUri + "signalrhub"))
                    .Build();
            _hubConnection.On("ReloadMenu", async () =>
            {
                await LoadAuthenticationState();
                StateHasChanged();
            });
            await _hubConnection.StartAsync();
            await LoadAuthenticationState();
            Auth.AuthenticationStateChanged += OnAuthenticationStateChanged;
        }
        private async Task CheckTokenExpired()
        {
            var token = await localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token)) return;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var exp = jwt.ValidTo;

            if (DateTime.UtcNow >= exp)
            {
                await localStorage.RemoveItemAsync("authToken");
                await Http.PostAsync("api/auth/logout", null);
                NavigationManager.NavigateTo("/login", true);
            }
        }
        private void Home()
        {
            NavigationManager.NavigateTo("/");
        }
        private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            await LoadAuthenticationState();
            StateHasChanged();
        }
        private async Task LoadAuthenticationState()
        {
            var authState = await authenticationStateTask;
            user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Guid.TryParse(userIdStr, out var userId);

                CurrentUser = new CurrentUserInfo
                {
                    UserId = userId,
                    FullName = user.FindFirst(ClaimTypes.Name)?.Value ?? "",
                    Email = user.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    Roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList(),
                    Permissions = user.FindAll("permission").Select(p => p.Value).ToList()
                };
                userDto = await Http.GetFromJsonAsync<UserDto>($"api/user/get-curernUser/{CurrentUser.UserId}");
                if (CurrentUser.Roles.Contains("Admin"))
                {
                    menu = await httpClient.GetFromJsonAsync<List<MenuDto>>($"api/Menu/GetAllMenu");
                }
                else
                {
                    menu = await httpClient.GetFromJsonAsync<List<MenuDto>>($"api/Menu/GetMenu/{userId}");
                }
            }
            else
            {
                CurrentUser = null;
            }
        }
        public void Dispose()
        {
            Auth.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            _timer?.Stop();
            _timer?.Dispose();
        }
        private string GetLink(string url)
        {
            var basePath = NavigationManager.BaseUri.TrimEnd('/'); 
            var urlReturn = $"{basePath}{url}";
            return urlReturn;
        }
    }
}

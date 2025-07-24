using AquaService.Shared.AuthModels;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace AquaSolution.Client.Pages.Logins;

public partial class Login 
{
    [Inject] private HttpClient Http { get; set; }
    [Inject] private CustomAuthenticationStateProvider AuthProvider { get; set; }
    [Inject] private NavigationManager Nav { get; set; }
    [Inject] private ILocalStorageService localStorage { get; set; }
    private string username { get; set; }
    private string password { get; set; }

    private async Task DoLogin()
    {
        var response = await Http.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            UserName = username,
            Password = password
        });

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(content["token"]);
            var claims = token.Claims.ToList();
            await localStorage.SetItemAsync("authToken", content["token"]);
            AuthProvider.MarkUserAsAuthenticated(username, claims);

            Nav.NavigateTo("/");
        }
    }
    private async Task HandleKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await DoLogin();
        }
    }
}

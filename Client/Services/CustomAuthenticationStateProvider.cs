using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private ClaimsPrincipal _user = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token) || token.Count(c => c == '.') != 2)
        {
            // Token không hợp lệ
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            // Nếu token lỗi format vẫn trả về Anonymous
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }


    public void MarkUserAsAuthenticated(string username, List<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, "apiauth");
        var user = new ClaimsPrincipal(identity);

        var authState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public void MarkUserAsLoggedOut()
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_user)));
    }
}

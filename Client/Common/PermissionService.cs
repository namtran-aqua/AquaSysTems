using AquaSolution.Shared.Enum;
using Microsoft.AspNetCore.Components.Authorization;

namespace AquaSolution.Client.Common
{
    public class PermissionService
    {
        private readonly AuthenticationStateProvider _authStateProvider;


        public PermissionService(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> HasPermissionAsync(Guid pageId, PermissionActionType action)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            return user.Claims.Any(c =>
                c.Type == "permission" &&
                c.Value == $"{pageId}:{action}");
        }
    }

}

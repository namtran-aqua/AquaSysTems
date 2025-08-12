using Microsoft.AspNetCore.Components;

namespace AquaSolution.Client.Common
{
    public static class GetURL
    {
        static NavigationManager Navigation;
        public static string GetAvatarUrl(string? avatarPath)
        {
            if (string.IsNullOrWhiteSpace(avatarPath))
                return "/images/default-avatar.png";
            if (!avatarPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return $"{Navigation.BaseUri}AquaSolution.Server/{avatarPath.TrimStart('/')}";
            }

            return avatarPath;
        }
    }
}

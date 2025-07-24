using AquaSolution.Shared.UserManagements;

namespace AquaSolution.Client.Common
{
    public class HasPermission
    {
        public async Task<bool> CheckPermissions(Guid pageId, string action, UserDto CurrentUser)
        {
            if (CurrentUser == null || CurrentUser.Roles == null) return false;

            foreach (var role in CurrentUser.Roles)
            {
                foreach (var permissionByMenu in role.Permissions.Values)
                {
                    foreach (var permissionByPage in permissionByMenu)
                    {
                        var parts = permissionByPage.Key.Split(';');
                        if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid pageIdValue))
                        {
                            if(pageIdValue == pageId)
                            {
                                var perrmisson = permissionByPage.Value.ToList();
                                if (perrmisson.Contains(action, StringComparer.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
    
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }
    }
}

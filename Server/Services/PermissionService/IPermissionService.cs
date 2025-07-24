using AquaSolution.Data.Data.Entities;
using AquaSolution.Shared.Menus;
using AquaSolution.Shared.Permissions;

namespace AquaSolution.Server.Services.PermissionService
{
    public interface IPermissionService
    {
        Task<List<GroupedPermissionDto>> GetGroupedPermissionsByMenuViewAsync();
        Task<List<GroupedPermissionDto>> GetPermissionTreeByRoleId(Guid roleId);
        Task<List<MenuDto>> GetPermissionAll();
        Task<bool> CreatedPermission(HandlePermissionDto HandlePermissionDto);
        Task<bool> DeletePermission(Guid permissionId);
    }
}
